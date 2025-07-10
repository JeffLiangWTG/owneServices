using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification)]
	public class NotificationOfStatusProcessor : ACEABIProcessor
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var messageBlock = message.MessageBlock;
			var entryFilerCode = ((AABIOutputB)messageBlock.B).FilerCode;
			var jobNumber = ZString.Empty;
			(var linkHeaders, var keys) = GetLinkedHeaders(message.Factory, message.Branch.GB_GC, messageBlock.MessageBlocks, entryFilerCode, out var referenceNumberInMessage, out var identifierCode, true);
			switch (linkHeaders.Count)
			{
				case 1:
					var linkHeader = linkHeaders.First();
					jobNumber = linkHeader.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : linkHeader.TopLevelBizObjReferenceNumber;
					break;
				case 0:
					jobNumber = keys.FirstOrDefault();
					break;
				default:
					var jobNumbers = linkHeaders.Select(linkHeader => linkHeader.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : linkHeader.TopLevelBizObjReferenceNumber).ToHashSet();
					jobNumber = jobNumbers.FirstOrDefault();
					break;
			}
			return new LinkedBusinessObjectMetaData(ZString.Empty, ZGuid.Empty, ZGuid.Empty, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			HashSet<string> result = null;
			var messageBlock = message.MessageBlock;
			var entryFilerCode = ((AABIOutputB)messageBlock.B).FilerCode;
			(var linkHeaders, var keys) = GetLinkedHeaders(message.Factory, message.Branch.GB_GC, messageBlock.MessageBlocks, entryFilerCode, out var referenceNumberInMessage, out var identifierCode, true);
			switch (linkHeaders.Count)
			{
				case 1:
					var linkHeader = linkHeaders.First();
					result = new HashSet<string>(keys);
					break;
				case 0:
					if (keys.Count > 1)
					{
						result = new HashSet<string>(keys.Skip(1));
					}
					break;
				default:
					var jobNumbers = linkHeaders.Select(linkHeader => linkHeader.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : linkHeader.TopLevelBizObjReferenceNumber).ToHashSet();
					result = new HashSet<string>(keys);
					if (jobNumbers.Count > 1)
					{
						result.UnionWith(jobNumbers.Skip(1));
					}
					break;
			}
			return result;
		}

		#endregion

		public override void Process()
		{
			Message.EM_MessageSubType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			var linkHeaders = GetLinkedHeaders(Factory, GlbCompany.CurrentCompany.PK, messageBlocks, B.FilerCode, out var referenceNumberInMessage, out var identifierCode, false).linkHeaders;
			var message = Message;
			if (linkHeaders.Count < 1)
			{
				ProcessBlocksAndGenerateEmailBody(message, referenceNumberInMessage, identifierCode);
			}
			else
			{
				foreach (var header in linkHeaders)
				{
					if (linkedHeader != null)
					{
						message = (MQEDIMessage)Message.Clone();
						message.EM_LinkedObject = null;
						message.EM_Status = EDIMessage.Status.Received;
					}

					linkedHeader = header;
					ProcessBlocksAndGenerateEmailBody(message, referenceNumberInMessage, identifierCode);
				}
			}
		}

		void ProcessBlocksAndGenerateEmailBody(MQEDIMessage message, ZString referenceNumberInMessage, ZString identifierCode)
		{
			var inBondQPHeader = linkedHeader as IInBondQPHeader;

			var htmlBody = new ZStringBuilder();
			HtmlTableCreator htmlTable = null;

			var dispositionCode = "";
			var dispositionDate = ZDateTime.Empty;
			var ns50s = new List<INBNS50>();
			INBNS30 currentNS30 = null;

			MarkPreviousDispositionsAsSuperseeded(inBondQPHeader);

			for (var index = 0; index < messageBlocks.Count; index++)
			{
				var block = messageBlocks[index];
				if (block is INBNS10)
				{
					currentNS30 = null;
				}
				else
				{
					if (block is INBNS30 ns30)
					{
						currentNS30 = ns30;
						var messageAttacheeIdentifier = ns30.IssuerCodeOfMasterBillNumber + ns30.MasterBillNumber;

						IIMessageAttacheeWithDisposition bill = null;
						if (linkedHeader != null)
						{
							if (inBondQPHeader != null)
							{
								bill = inBondQPHeader.FindBillMatchingNumber(ns30.MasterBillNumber, ns30.HouseBillNumber);
							}
							else if (linkedHeader is IIMessageAttacheeWithDisposition)
							{
								bill = linkedHeader as IIMessageAttacheeWithDisposition;
							}
						}

						dispositionDate = DateTimeParser.GetDateTimeFromZDateAndStringTime(ns30.ActionDate, ns30.ActionTime);
						dispositionCode = ns30.DispositionCode;

						if (htmlTable != null)
						{
							WriteHtmlToIncludingNS50s(htmlBody, htmlTable, ns50s);
							htmlTable = null;
						}

						if (IsDispositionRelatedToBill(index))
						{
							htmlTable = CreateHtmlTableAndSetDispositionCodeAndDate(messageAttacheeIdentifier, dispositionCode, dispositionDate);
							if (bill != null)
							{
								if (message.EM_LinkedObject == null)
								{
									bill.Messages.Add(message);
								}
								UpdateDispositionInformation(bill, dispositionCode, dispositionDate);
							}
						}
					}
					else
					{
						if (block is INBNS50 nbns50)
						{
							ns50s.Add(nbns50);//could be related to bill or container 
						}
						else
						{
							if (block is INBNS60 ns60)//can be repeated 999 for each container per INBNS30
							{
								if (ns60.ActionIndicator == 1)//action is related to a container
								{
									var container = inBondQPHeader != null ?
												inBondQPHeader.FindContainer(ns60.ContainerNumber, currentNS30?.MasterBillNumber ?? ZString.Empty)
												: null;

									var messageAttacheeIdentifier = ns60.ContainerNumber;
									if (container != null)
									{
										if (message.EM_LinkedObject == null)
										{
											container.Messages.Add(message);
										}
										UpdateDispositionInformation(container, dispositionCode, dispositionDate);
									}

									htmlTable = CreateHtmlTableAndSetDispositionCodeAndDate(messageAttacheeIdentifier, dispositionCode, dispositionDate);
									WriteHtmlToIncludingNS50s(htmlBody, htmlTable, ns50s);
									htmlTable = null;
								}
							}
						}
					}
				}
			}

			if (message.EM_LinkedObject == null && linkedHeader != null)
			{
				linkedHeader.Messages.Add(message);
			}

			if (htmlTable != null)
			{
				WriteHtmlToIncludingNS50s(htmlBody, htmlTable, ns50s);
			}

			var jobNumber = "Unknown";
			var uri = "";
			if (linkedHeader != null)
			{
				jobNumber = inBondQPHeader != null ? inBondQPHeader.JobNumber.ToString() : linkedHeader.TopLevelBizObjReferenceNumber;
				if (!string.IsNullOrEmpty(jobNumber))
				{
					uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(linkedHeader);
				}
			}
			else if (!referenceNumberInMessage.IsEmpty)
			{
				var identifier = !identifierCode.IsEmpty ?
				(identifierCode.Length == 3 ? identifierCode + "-" : identifierCode.ToString())
				: "";
				jobNumber += "/" + identifier + referenceNumberInMessage;
			}

			var branch = linkedHeader != null ? linkedHeader.Branch : null;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Status Notification", htmlBody.ToString(), false, branch, linkedHeader as BusinessObject);//this is unsolicited
		}

		protected override ZString GetEmailAddressToSendTo()
		{
			var result = ZString.Empty;

			if (linkedHeader != null)
			{
				var lastMessage = linkedHeader.Messages.LastOutgoingMessage;
				if (lastMessage != null)
				{
					var sender = lastMessage.UserWhoQueuedThisRecord;
					if (sender != null)
					{
						result = sender.GS_EmailAddress;
					}
				}
			}
			return result;
		}

		void WriteHtmlToIncludingNS50s(ZStringBuilder htmlBody, HtmlTableCreator htmlTable, List<INBNS50> ns50s)
		{
			foreach (var ns50 in ns50s)
			{
				htmlTable.WriteRow("", ns50.Remarks);
			}

			ns50s.Clear();
			htmlBody.Append(htmlTable.ToHtml());
		}

		HtmlTableCreator CreateHtmlTableAndSetDispositionCodeAndDate(string messageAttacheeIdentifier, ZString dispositionCode, ZDateTime dispositionDate)
		{
			var result = new HtmlTableCreator(new string[] { "-", "Message for " + messageAttacheeIdentifier });

			var dispositionDesc = DispositionList.ContainsCode(dispositionCode) ? DispositionList.GetDescriptionFromCode(dispositionCode) : "";
			result.WriteRow("Disposition Code", dispositionCode + "(" + dispositionDesc + ")");

			if (dispositionDate.IsValid)
			{
				result.WriteRow("Disposition Date", dispositionDate.ToLongTimeString());
			}

			return result;
		}

		internal CodeDescriptionPairList DispositionList
		{
			get
			{
				if (fDispositionList == null)
				{
					fDispositionList = DispositionCodeListLoader.GetDispositionCodes(linkedHeader?.TransportMode ?? ZString.Empty, Factory);
				}
				return fDispositionList;
			}
		}
		CodeDescriptionPairList fDispositionList;

		bool IsDispositionRelatedToBill(int indexOfNS30)
		{
			var hasNSRelatedToContainer = false; //the absence of NS60 also indicates disposition is related to bill

			for (var index = indexOfNS30 + 1; index < messageBlocks.Count; index++)
			{
				var block = messageBlocks[index];
				if (block is INBNS30)
				{
					break;
				}

				if (block is INBNS60 nbns60)
				{
					hasNSRelatedToContainer |= nbns60.ActionIndicator == 1;
				}
			}

			return !hasNSRelatedToContainer;
		}

		void UpdateDispositionInformation(IIMessageAttacheeWithDisposition attachee, ZString dispositionCode, ZDateTime dispositionDate)
		{
			if (dispositionDate.IsValid)
			{
				attachee.UpdateDispositionInformation(dispositionCode, dispositionDate);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static (HashSet<IMessageAttacheeWithCBPSenderReference> linkHeaders, List<string> additionalKeys) GetLinkedHeaders(BusinessObjectFactory factory, ZGuid companyPK, List<MessageBlock> messageBlocks, ZString entryFilerCode, out ZString referenceNumberInMessage, out ZString identifierCode, bool gatherAdditionalKeys)
		{
			referenceNumberInMessage = ZString.Empty;
			identifierCode = ZString.Empty;
			var additionalKeys = new List<string>();
			var result = new HashSet<IMessageAttacheeWithCBPSenderReference>();
			ZQuery filter;
			var ns10 = messageBlocks.OfType<INBNS10>().FirstOrDefault();
			if (ns10 != null)
			{
				referenceNumberInMessage = ns10.InbondNumber;
				if (!referenceNumberInMessage.IsEmpty)
				{
					identifierCode = entryFilerCode;
					//Fing In-Bond Move Header
					var entryNumber = CusEntryNumber.LoadMostRecentByCreateTime(factory, CusEntryHeaderMessageTypeList.Codes.InBond, referenceNumberInMessage, Core.Constants.CountryCodes.UnitedStates);
					if (entryNumber != null)
					{
						IMessageAttacheeWithCBPSenderReference iMessageAttacher = null;
						switch (entryNumber.CE_ParentTable)
						{
							case JobDeclarationSchema.Constants.TableName:
								var declaration = factory.Load(typeof(Customs.Business.BaseJobDeclaration), entryNumber.CE_ParentID) as JobDeclaration;
								if (declaration != null && declaration.US_EntryFilerCode == identifierCode)
								{
									var query = new ZQuery(CusEntryHeaderSchema.CH_JE, declaration.PK);
									query.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond);
									iMessageAttacher = factory.LoadTop1<CusEntryHeader>(query);
								}
								break;
							case CusEntryHeaderSchema.Constants.TableName:
								iMessageAttacher = factory.Load<Customs.Business.CusEntryHeader>(entryNumber.CE_ParentID) as IMessageAttacheeWithCBPSenderReference;
								break;
							case CusInBondMoveHeaderSchema.Constants.TableName:
								iMessageAttacher = factory.Load<Customs.Business.CusInBondMoveHeader>(entryNumber.CE_ParentID) as IMessageAttacheeWithCBPSenderReference;
								break;
						}

						if (iMessageAttacher != null)
						{
							additionalKeys.Add(USIUniversalCustomsMessageProcessor.GetInBondJobNumberKey(iMessageAttacher.EntryFilerCode, referenceNumberInMessage, iMessageAttacher.Branch.GB_GC));
							result.Add(iMessageAttacher);
						}
					}
				}
			}

			if (result.Count < 1)
			{
				//Find declaration job by Entry Filer Code + Entry Number provided by Customs in block 40
				var ns40 = messageBlocks.OfType<INBNS40>().FirstOrDefault();
				if (ns40 != null && ns40.EntryNumber.Length == EntryNumberLength)
				{
					identifierCode = ns40.EntryNumber.SubstringSafe(0, 3);
					if (identifierCode == USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode)
					{
						referenceNumberInMessage = ns40.EntryNumber.SubstringSafe(3);
						var entryHeader = new CusEntryHeader.Loader(factory).FindByEntryNumberAndFilerCode(companyPK, referenceNumberInMessage, identifierCode, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
						if (entryHeader is IMessageAttacheeWithCBPSenderReference iMessageAttacher)
						{
							result.Add(iMessageAttacher);
						}
					}
				}
			}

			if (result.Count < 1)
			{
				//Find In-Bond Move Header by SCAC + Master Bill Number provided by Customs in block 30
				//may be inactive job header with the same bill number
				var masterBill30 = messageBlocks.OfType<INBNS30>().FirstOrDefault();
				if (masterBill30 != null)
				{
					referenceNumberInMessage = masterBill30.MasterBillNumber;
					identifierCode = masterBill30.IssuerCodeOfMasterBillNumber;

					if (ns10 == null)
					{
						filter = new ZQuery(CusInBondBillSchema.B0_IssuerCode, identifierCode);
						filter.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, referenceNumberInMessage);
						var bills = factory.Load<Customs.Business.CusInBondBill>(filter);
						var inBondBills = bills.Where(x => x.Header?.BH_IsActive ?? false).OfType<Integration.Customs.US.InBond.ICusInBondBill>();
						foreach (var inBondBill in inBondBills)
						{
							filter = new ZQuery(CusInBondMoveDetailSchema.B9_B0, inBondBill.PK);
							filter.MaximumRows = 2;
							var movementDetails = factory.Load<Integration.Customs.US.InBond.ICusInBondMoveDetail>(filter);
							if (movementDetails.Length == 1)
							{
								if (factory.Load<Integration.Customs.US.InBond.ICusInBondMoveHeader>(movementDetails[0].B9_BM) is IMessageAttacheeWithCBPSenderReference entryHeader)
								{
									result.Add(entryHeader);
								}
							}
						}
					}

					//If no In-Bond job created, find a declaration master bill 
					if (result.Count < 1)
					{
						var dbOnlyfilter = new ZDBOnlyQuery(typeof(Bill));
						dbOnlyfilter.AddToFilter(CusDecHouseBillSchema.CU_BillNum, referenceNumberInMessage);
						dbOnlyfilter.AddToFilter(CusDecHouseBillSchema.CU_BillType, Customs.Business.BillTypeList.Codes.MasterBill);

						var declarationFilter = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
						declarationFilter.AddToFilter(JobDeclarationSchema.JE_IsCancelled, false);

						var declarationBranchFilter = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
						var declarationCompanyFilter = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
						declarationCompanyFilter.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico });

						declarationBranchFilter.AddSubQuery(GlbBranchSchema.GB_GC, declarationCompanyFilter, JoinCondition.And);

						declarationFilter.AddSubQuery(JobDeclarationSchema.JE_GB, declarationBranchFilter, JoinCondition.And);

						dbOnlyfilter.AddSubQuery(CusDecHouseBillSchema.CU_JE, declarationFilter, JoinCondition.And);

						var identifierCodeToCompareWith = identifierCode;
						var masterBills = factory.Load<Bill>(dbOnlyfilter).Where(x => x.US_UI_NKBillIssuerSCAC == identifierCodeToCompareWith);
						foreach (var masterBill in masterBills)
						{
							result.Add(masterBill);
						}
					}
				}
			}
			return (result, additionalKeys);
		}
		IMessageAttacheeWithCBPSenderReference linkedHeader;
		const int EntryNumberLength = 11;

		void MarkPreviousDispositionsAsSuperseeded(IInBondQPHeader linkedHeader)
		{
			if (linkedHeader != null)
			{
				for (var index = 0; index < messageBlocks.Count; index++)
				{
					if (messageBlocks[index] is INBNS30 ns30)
					{
						IIMessageAttacheeWithDisposition bill = linkedHeader.FindBillMatchingNumber(ns30.MasterBillNumber, ns30.HouseBillNumber);
						var dispositionDate = DateTimeParser.GetDateTimeFromZDateAndStringTime(ns30.ActionDate, ns30.ActionTime);

						if (IsDispositionRelatedToBill(index))
						{
							if (bill != null)
							{
								MarkPreviousDispositionsInactive(bill, dispositionDate);
							}
						}
					}
				}
			}
		}

		void MarkPreviousDispositionsInactive(IIMessageAttacheeWithDisposition attachee, ZDateTime dispositionDate)
		{
			attachee.MarkPreviousDispositionsInactive(dispositionDate);
		}
	}
}
