using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.Customs.SG.V4.Business.PermitPrinting
{
	public class PrintPermitProcessor
	{
		public void PrintLastPermit(JobDeclaration declaration, DocumentPack docPack)
		{
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				var receivedMessages = GetReceivedMessages(entry);
				receivedMessages.Sort(EDIMessageSchema.Constants.EM_SystemLastEditTimeUtc, ListSortDirection.Descending);

				foreach (EDIMessage message in receivedMessages)
				{
					if (message.EM_MessageType == Cuspmt09bMessageProcessor.MessageType && !IsGeneralDocument(message))
					{
						MakeSinglePermit(docPack, message, new BusinessObjectFactory());
						return;
					}
					else if (message.EM_MessageType == Cusres09bMessageProcessor.MessageType)
					{
						return;
					}
				}
			}
		}

		#region IsGeneralDocument

		public static bool IsGeneralDocument(EDIMessage message)
		{
			return IsSGCustomsTradenetXMLMessage(message)
				? IsGeneralDocumentCore(message as SGXmlEDIMessage)
				: IsGeneralDocumentCore(message);
		}

		static bool IsGeneralDocumentCore(EDIMessage message)
		{
			var result = false;

			if (IsSGCustomsTradenet4Message(message))
			{
				var cuspmt = new CUSPMT();
				cuspmt.Parse(new UNOASGCharacterSet(), message.EM_MessageText);

				if (cuspmt.BGM[0].DocumentMessageName.DocumentNameCode == DocumentNameCodeList.GeneralResponseCustoms)
				{
					result = true;
				}
			}

			return result;
		}

		static bool IsGeneralDocumentCore(SGXmlEDIMessage message)
		{
			var response = message?.TradenetResponse;

			var updateIndicatorCode = response
				?.OutboundMessage
				?.InPaymentUpdatePermit
				?.Update
				?.UpdateIndicatorCode?.Trim()?.ToUpperInvariant() ?? string.Empty;

			return !string.IsNullOrWhiteSpace(updateIndicatorCode) && (updateIndicatorCode == SGConstants.UpdateIndicators.AMR || updateIndicatorCode == SGConstants.UpdateIndicators.FRF || updateIndicatorCode == SGConstants.UpdateIndicators.PRG || updateIndicatorCode == SGConstants.UpdateIndicators.PRS);
		}

		#endregion

		#region IsTN4_1Permit

		public static bool IsTN4_1Permit(EDIMessage message)
		{
			var result = false;

			if (IsSGCustomsTradenet4Message(message))
			{
				var cuspmt = new CUSPMT();
				cuspmt.Parse(new UNOASGCharacterSet(), message.EM_MessageText);

				if (cuspmt.UNH[0].MessageIdentifier.AssociationAssignedCode == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne)
				{
					result = true;
				}
			}
			else
			{
				result = IsSGCustomsTradenetXMLMessage(message);
			}

			return result;
		}

		#endregion

		#region GetRefund

		public static RefundInfo GetRefund(EDIMessage message, BusinessObjectFactory factory)
		{
			return IsSGCustomsTradenetXMLMessage(message)
					? GetRefundCore(message as SGXmlEDIMessage, factory)
					: GetRefundCore(message, factory);
		}

		static RefundInfo GetRefundCore(SGXmlEDIMessage message, BusinessObjectFactory factory)
		{
			var permit = message?.TradenetResponse?.OutboundMessage?.InPaymentUpdatePermit;
			return permit != null ? new RefundInfo(new BaseTradeNetRefund(permit), factory) : null;
		}

		static RefundInfo GetRefundCore(EDIMessage message, BusinessObjectFactory factory)
		{
			var iptupt = new IPTUPT();
			iptupt.Parse(new UNOASGCharacterSet(), message.EM_MessageText);
			return new RefundInfo(iptupt, factory);
		}

		#endregion

		#region GetPermit

		public static PrintPermit GetPermit(EDIMessage message, BusinessObjectFactory factory)
		{
			return IsSGCustomsTradenetXMLMessage(message)
					? GetPrintPermitCore(factory, message as SGXmlEDIMessage)
					: GetPrintPermitCore(factory, message);
		}

		static PrintPermit GetPrintPermitCore(BusinessObjectFactory factory, EDIMessage message)
		{
			PrintPermit result;

			if (IsTN4_1Permit(message))
			{
				var cuspmt09b = new Cuspmt09b();
				cuspmt09b.Parse(new UNOASGCharacterSet(), message.EM_MessageText);
				result = new PrintPermit(cuspmt09b, factory, message.EM_LinkUniqueID);
			}
			else
			{
				var cuspmt = new CUSPMT();
				cuspmt.Parse(new UNOASGCharacterSet(), message.EM_MessageText);
				result = new PrintPermit(cuspmt, factory, message.EM_LinkUniqueID);
			}

			return result;
		}

		static PrintPermit GetPrintPermitCore(BusinessObjectFactory factory, SGXmlEDIMessage message)
		{
			IEnumerable<ITradeNetOutPermitSection> GetTradeNetOutPermitSections(TradenetResponse response)
			{
				var outboundMessage = response.OutboundMessage;

				if (outboundMessage != null)
				{
					yield return outboundMessage.InNonPaymentPermit;
					yield return outboundMessage.InNonPaymentUpdatePermit;

					yield return outboundMessage.InPaymentPermit;
					yield return outboundMessage.InPaymentUpdatePermit;

					yield return outboundMessage.OutwardPermit;
					yield return outboundMessage.OutwardUpdatePermit;

					yield return outboundMessage.TranshipmentMovementPermit;
					yield return outboundMessage.TranshipmentMovementUpdatePermit;
				}
			}

			var tradenetResponse = message?.TradenetResponse;
			var section = tradenetResponse != null ? GetTradeNetOutPermitSections(tradenetResponse).FirstOrDefault(c => c != null) : null;
			return section != null ? new PrintPermit(new BaseTradeNetPermit(factory, section), factory, message.EM_LinkUniqueID) : null;
		}

		#endregion

		#region Implement

		EDIMessageCollectionView GetReceivedMessages(CusEntryHeader entry)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Received);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);

			return new EDIMessageCollectionView(entry.Messages, query);
		}

		Report GetReport(DocumentPack docPack, ExcelTemplate template, PrintPermit permit)
		{
			Report result = null;
			var wrapper = DocSGPrintPermit.New(permit, permit.Factory);
			if (wrapper != null)
			{
				result = new Report(docPack, template, wrapper, "Cargo Clearance Permits", null, DocumentDirection.ANY, false);
			}

			return result;
		}

		ExcelTemplate GetTemplate(BusinessObjectFactory factory, bool tN4_1)
		{
			var templateName = tN4_1 ? SGConstants.TN4_1Permit : SGConstants.Permit;
			return ExcelTemplateRetriever.GetTemplate(templateName, Core.Constants.DataContext.SGPrintPermit, factory)
				?? throw new ZException("Template(s) used for this document has been deleted from the system. Documents not printed.");
		}

		void MakeSinglePermit(DocumentPack docPack, EDIMessage message, BusinessObjectFactory factory)
		{
			var isTN41 = IsTN4_1Permit(message);

			var template = GetTemplate(factory, isTN41);

			if (template != null)
			{
				docPack.Add(GetReport(docPack, template, GetPermit(message, factory)));
			}
		}

		public void DoPrintTask(DocumentPack docPack, bool isModulePrinting)
		{
			var printTask = new PrintTask();
			if (isModulePrinting)
			{
				docPack.DeliveryInstructions.PrintMultiDocPack = true;
				docPack.EmailSubjectForConsolidateReports = docPack.Count.ToString() + " Cargo Clearance Permit(s) from - " + GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName;
			}
			printTask.Add(docPack);
			RunPrintTask(printTask);
		}

		public void DoSimplePrint(EDIMessage message)
		{
			var docPack = new DocumentPack();
			MakeSinglePermit(docPack, message, new BusinessObjectFactory());
			DoPrintTask(docPack, false);
		}

		protected virtual void RunPrintTask(PrintTask printTask)
		{
			printTask.Run(Env.Security.None);
		}

		public AttachmentDef GenerateDocumentToPDFAttachment(EDIMessage message)
		{
			AttachmentDef pdfAttachment = null;
			var unSavedFactory = new BusinessObjectFactory();
			using (DocumentPack docPack = new DocumentPack())
			{
				MakeSinglePermit(docPack, message, unSavedFactory);
				if (docPack.Count > 0)
				{
					var pdfExport = new FlexCelPdfExportSafe();

					var signature = SingatureRepository.Instance.GetGlobalPdfSignature(message.Factory);
					if (signature != null)
					{
						pdfExport.Sign(signature);
					}

					var pdfStream = new MemoryStream();
					pdfExport.BeginExport(pdfStream);

					try
					{
						using (MemoryStream stream = new MemoryStream())
						{
							(docPack[0] as Report).Save(stream);
							stream.Position = 0;
							var flexFile = new XlsFile();
							flexFile.Open(stream);

							flexFile.ActiveSheet = 1;
							pdfExport.Workbook = flexFile;
							if (flexFile.SheetVisible == TXlsSheetVisible.Visible)
							{
								pdfExport.ExportSheet();
							}
						}
					}
					finally
					{
						pdfExport.EndExport();
					}

					pdfAttachment = new AttachmentDef("Permit.pdf", pdfStream.ToArray());
				}
			}
			return pdfAttachment;
		}

		static bool IsSGCustomsTradenet4Message(EDIMessage message) => message.EM_ApplicationCode == ApplicationCodeList.Codes.SGCustomsTradenet4;

		static bool IsSGCustomsTradenetXMLMessage(EDIMessage message) => message.EM_ApplicationCode == ApplicationCodeList.Codes.SGCustomsTradenetXML;

		#endregion
	}
}
