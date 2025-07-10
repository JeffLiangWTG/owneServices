using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Email.Html;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrderChangesDocumentManager
	{
		protected ImportedOrderChangesDocumentManager(List<ImportedOrder> importedOrders, ZString attachedOrderDescription)
		{
			importedOrderChanges = new ImportedOrderChanges(importedOrders, attachedOrderDescription);
		}
		internal ImportedOrderChanges importedOrderChanges;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Template Name")]
		const string TemplateName = "Order Import Report";

		#region DeliverAllImportedOrderChanges

		public static void DeliverAllImportedOrderChanges(List<ImportedOrder> importedOrders)
		{
			if (importedOrders == null || importedOrders.Count == 0)
			{
				return;
			}

			new ImportedOrderChangesDocumentManager(importedOrders, ZString.Empty).Deliver();
		}

		#endregion

		#region AttachDocumentInEmailWithAllImportedOrderChanges

		public static void AttachDocumentInEmailWithAllImportedOrderChanges(List<ImportedOrder> importedOrders, ZString attachedOrderDescription, string extraInformation, HtmlEmailDef htmlEmailDef)
		{
			if (importedOrders == null || importedOrders.Count == 0 || htmlEmailDef == null)
			{
				return;
			}

			var manager = new ImportedOrderChangesDocumentManager(importedOrders, attachedOrderDescription);
			manager.AttachDocumentInEmailIfNeed(extraInformation, htmlEmailDef);
		}

		#endregion

		#region Deliver

		void Deliver()
		{
			if (importedOrderChanges.HasChangesToReport)
			{
				foreach (KeyValuePair<string, string> item in GetEmailAddressesAndSubjects())
				{
					Deliver(item.Key, item.Value);
				}
			}
		}

		void Deliver(ZString emailAddress, ZString emailSubject)
		{
			using (PrintTask printTask = new PrintTask())
			{
				DocumentPack documentPack = CreateDocumentPack(emailSubject);
				DeliveryInstructions deliveryInstructions = CreateDeliveryInstructions(documentPack, emailAddress);

				printTask.Add(documentPack);
				printTask.Run(deliveryInstructions);
			}
		}

		KeyValuePair<string, string>[] GetEmailAddressesAndSubjects()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			foreach (ImportedOrder importedOrder in importedOrderChanges.AllOrders)
			{
				EDICommunicationsMode[] modes = GetCommunicationModes(importedOrder);
				if (modes.Length == 0 && NotificationGroup != null)
				{
					foreach (GlbStaff staff in NotificationGroup.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty)
						{
							result[staff.GS_EmailAddress] = Res.GetString("009245a5-edef-43bd-93dd-20b278f51c78", "Order Import Changes");
						}
					}
				}
				else
				{
					foreach (EDICommunicationsMode mode in modes)
					{
						result[mode.EK_Destination] = mode.EK_ServerAddressSubject;
					}
				}
			}
			return new List<KeyValuePair<string, string>>(result).ToArray();
		}

		EDICommunicationsMode[] GetCommunicationModes(ImportedOrder importedOrder)
		{
			List<EDICommunicationsMode> modes = new List<EDICommunicationsMode>();
			modes.AddRange(GetCommunicationModes((ZGuid)importedOrder.Buyer.Value));
			modes.AddRange(GetCommunicationModes((ZGuid)importedOrder.Supplier.Value));
			return modes.ToArray();
		}

		EDICommunicationsMode[] GetCommunicationModes(ZGuid organisationPK)
		{
			OrgHeader organisation = Factory.Load<OrgHeader>(organisationPK);
			List<EDICommunicationsMode> modes = new List<EDICommunicationsMode>();

			if (organisation != null)
			{
				foreach (EDICommunicationsMode mode in organisation.EDICommunicationsModes)
				{
					if (mode.EK_Module == WorkflowDescriptors.OrderWorkflowDescriptorCode &&
						mode.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.OrderImportReport)
					{
						modes.Add(mode);
					}
				}
			}

			return modes.ToArray();
		}

		#endregion

		#region AttachDocumentInEmailIfNeed

		void AttachDocumentInEmailIfNeed(string extraInformation, HtmlEmailDef htmlEmailDef)
		{
			if (!importedOrderChanges.HasChangesToReport)
			{
				return;
			}

			if (!string.IsNullOrWhiteSpace(extraInformation) && !htmlEmailDef.Body.Contains(extraInformation))
			{
				htmlEmailDef.Body += EmailHtmlTags.BR + EmailHtmlTags.BR;
				htmlEmailDef.Body += extraInformation;
				htmlEmailDef.Body += EmailHtmlTags.BR + EmailHtmlTags.BR;
			}

			using (var documentPack = CreateDocumentPack(ZString.Empty))
			{
				var report = documentPack.GetFirstReport();
				if (report != null)
				{
					using (var stream = new MemoryStream())
					{
						report.Save(stream);

						var bytes = DocumentConverter.ConvertFromExcel(stream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);

						htmlEmailDef.Attachments.Add(new AttachmentDef(TemplateName + ".PDF", bytes)); // File Name
					}
				}
			}
		}

		#endregion

		#region Implementation

		DocumentPack CreateDocumentPack(ZString emailSubject)
		{
			var template = new ExcelTemplateReadFromStmTemplateTable(DocumentTemplate);
			var documentPack = new DocumentPack();
			Report document = null;

			try
			{
				document = new Report(documentPack, template, BODocDataProvider.Get(importedOrderChanges), DocumentTemplate.SO_Name, null, DocumentDirection.ANY, false);
				document.EmailSubject = emailSubject;
				documentPack.Add(document);
			}
			catch
			{
				documentPack.Dispose();
				document?.Dispose();
				throw;
			}

			return documentPack;
		}

		DeliveryInstructions CreateDeliveryInstructions(DocumentPack documentPack, ZString emailAddress)
		{
			DeliveryInstructions deliveryInstructions = new DeliveryInstructions(documentPack);
			deliveryInstructions.Destination = DeliveryInstructionDestination.Auto;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.DeliveryAddress = emailAddress;
			contact.AttachmentType = OrdersDataRegistry.Instance.OrderImportReportAttachmentType.Value;
			deliveryInstructions.Recipients.Add(contact);
			return deliveryInstructions;
		}

		GlbGroup NotificationGroup
		{
			get
			{
				if (!notificationGroupRetrieved)
				{
					ZGuid notificationGroupPK = (ZGuid)NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.Value;
					notificationGroup = Factory.Load<GlbGroup>(notificationGroupPK);
					notificationGroupRetrieved = true;
				}
				return notificationGroup;
			}
		}
		bool notificationGroupRetrieved;
		GlbGroup notificationGroup;

		StmTemplate DocumentTemplate
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(StmTemplateSchema.SO_Name, TemplateName);
				query.AddToFilter(StmTemplateSchema.SO_DataContext, nameof(Core.Constants.DataContext.Order));
				return Factory.LoadTop1<StmTemplate>(query);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
