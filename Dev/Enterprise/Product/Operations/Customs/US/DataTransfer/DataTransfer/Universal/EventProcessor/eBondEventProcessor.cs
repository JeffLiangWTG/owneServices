using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using AutoJobDeclaration = Enterprise.Customs.Business.AutoJobDeclaration;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	class eBondEventProcessor : BaseBondEventProcessor
	{
		public eBondEventProcessor(Event eventDataObject, BusinessObjectFactory factory) : base(eventDataObject, factory)
		{
		}

		public override BusinessObject[] Process()
		{
			JobDeclaration[] result = null;

			var query = GetFilterForEntryNum(eventDataObject);
			if (query != null)
			{
				result = factory.Load<JobDeclaration>(query);
			}
			var autoSendMessageNote = ZString.Empty;
			var uri = string.Empty;
			var jobNumber = ZString.Empty;
			GlbBranch branch = null;
			string[] emailAddressToSendTo = null;
			var declaration = result != null ? result.FirstOrDefault() : null;
			if (declaration != null)
			{
				UpdateJobDeclaration(declaration);

				if (eBondLogger.CanAutoSendMessage(declaration))
				{
					if (declaration.IsACE)
					{
						AutoSendMessage sendMessage = null;
						var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
						var ensAth = ensEntry != null ? eBondLogger.GetAutoSendEvent(ensEntry.Logs) : null;
						if (ensAth != null)
						{
							sendMessage = new ACEENSAutoSendEventProcessor();
							autoSendMessageNote = sendMessage.Process(declaration, ensAth);
						}

						var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
						var cargoReleaseAth = simplifiedEntry != null ? eBondLogger.GetAutoSendEvent(simplifiedEntry.Logs) : null;

						if (cargoReleaseAth != null)
						{
							sendMessage = new ACECargoReleaseAutoSendEventProcessor();
							autoSendMessageNote += sendMessage.Process(declaration, cargoReleaseAth);
						}
					}
				}
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration);
				jobNumber = declaration.JE_DeclarationReference;
				branch = declaration.Branch;
				emailAddressToSendTo = GetRecipient(declaration);
				LinkMessage(declaration.EntryFilerCode + declaration.DecEntryNumber, declaration);
			}
			else
			{
				var bondedOrganisations = GetBondedOrganisations(GetBondNumber());
				if (bondedOrganisations.Length > 0)
				{
					LinkMessage(ZString.Empty, bondedOrganisations[0]);
				}
			}
			SendNotification(uri, jobNumber, autoSendMessageNote, branch ?? GlbBranch.CurrentBranch, emailAddressToSendTo ?? System.Array.Empty<string>());

			return result;
		}

		protected override string GetAdditionalEmailSubject(string additionalSubject)
		{
			var bondNumber = GetBondNumber();
			if (!bondNumber.IsEmpty)
			{
				var bondedOrganisations = GetBondedOrganisations(bondNumber);
				if (bondedOrganisations.Length > 0)
				{
					additionalSubject += bondedOrganisations.Length > 1 ? " for Multiple Organisations" : " for " + bondedOrganisations[0].OH_Code.ToString();
				}
			}
			return additionalSubject;
		}

		OrgHeader[] GetBondedOrganisations(ZString bondNumber)
		{
			var bondedOrganisations = System.Array.Empty<OrgHeader>();
			if (!bondNumber.IsEmpty)
			{
				var bondNumberSubQuery = new ZDBOnlySubQuery(typeof(Business.CusBondDetail), CusBondDetailSchema.PW_ParentID);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_BondNumber, bondNumber);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ApplicationCodeList.Codes.UsaInBond);
				var orgFilter = new ZDBOnlyQuery(typeof(OrgHeader));
				orgFilter.AddSubQuery(bondNumberSubQuery, JoinCondition.And);
				orgFilter.OrderBy = OrgHeader.Schema.OH_SystemCreateTimeUtc;
				bondedOrganisations = factory.Load<OrgHeader>(orgFilter);
			}
			return bondedOrganisations;
		}

		ZQuery GetFilterForEntryNum(IXmlEventValueObject xmlEventDataObject)
		{
			ZQuery result = null;
			var entryNumber = xmlEventDataObject.Context.EntryNumber;
			if (!entryNumber.IsEmpty)
			{
				var declarationFilter = new ZDBOnlyQuery(typeof(JobDeclaration));
				var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AutoJobDeclaration.Schema.TableName);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber.SubstringSafe(3));
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_Category, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates });
				declarationFilter.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

				var entryFilerSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
				entryFilerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, AutoUSAddInfo.Schema.US_EntryFilerCode);
				entryFilerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, entryNumber.SubstringSafe(0, 3));
				declarationFilter.AddSubQuery(entryFilerSubQuery, JoinCondition.And);

				var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
				companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				declarationFilter.AddSubQuery(companySubQuery, JoinCondition.And);
				declarationFilter.OrderBy = JobDeclaration.Schema.JE_SystemCreateTimeUtc;
				result = declarationFilter;
			}
			return result;
		}

		void UpdateJobDeclaration(JobDeclaration declaration)
		{
			var updateCollection = eventDataObject.AdditionalFieldsToUpdateCollection;

			if (updateCollection != null && declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				var designationType = GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.DesignationType);
				var dispositionCode = GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.DispositionCode);
				var bondNumber = GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.CBPBondNumber);
				var bondAmount = GetFielsInUpdateCollection(updateCollection, BondStatusNotificationMessageProcessor.BondAmount);

				if (designationType == BondDesignationCodeList.Codes.BasicBond)
				{
					declaration.US_CBPBondNo = bondNumber;
					UpdatePrimaryBondDetails(declaration, dispositionCode, bondAmount);
				}
				else if (designationType == "A")
				{
					declaration.US_CBPBondNo2 = bondNumber;
					UpdateSecondaryBondDetails(declaration, dispositionCode, bondAmount);
				}
				else
				{
					if (bondNumber == declaration.US_CBPBondNo)
					{
						UpdatePrimaryBondDetails(declaration, dispositionCode, bondAmount);
					}
					else if (bondNumber == declaration.US_CBPBondNo2)
					{
						UpdateSecondaryBondDetails(declaration, dispositionCode, bondAmount);
					}
				}
			}
		}

		void UpdatePrimaryBondDetails(JobDeclaration declaration, string dispositionCode, string bondAmountS)
		{
			declaration.US_BondDispositionCode = dispositionCode;
			ZDecimal bondAmount;
			if (ZDecimal.TryParse(bondAmountS, out bondAmount))
			{
				declaration.US_BondAmount = bondAmount;
			}
		}

		void UpdateSecondaryBondDetails(JobDeclaration declaration, string dispositionCode, string bondAmountS)
		{
			declaration.US_BondDispositionCode2 = dispositionCode;
			ZDecimal bondAmount;
			if (ZDecimal.TryParse(bondAmountS, out bondAmount))
			{
				declaration.US_BondAmount2 = bondAmount;
			}
		}

		string[] GetRecipient(JobDeclaration declaration)
		{
			var result = new List<string>();
			if (declaration != null)
			{
				if (eBondLogger.CanAutoSendMessage(declaration))
				{
					result.AddRange(eBondLogger.GetLoggedATHUser(declaration));
				}

				if (result.Count == 0)
				{
					var ensSender = GetUserEmailWhoSentLastMessage(declaration.ActiveEntryHeaders.EntrySummaryEntry, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
					if (!ensSender.IsEmpty)
					{
						result.Add(ensSender);
					}
				}

				if (result.Count == 0)
				{
					var seSender = GetUserEmailWhoSentLastMessage(declaration.ActiveEntryHeaders.SimplifiedEntry, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
					if (!seSender.IsEmpty)
					{
						result.Add(seSender);
					}
				}
				if (result.Count == 0)
				{
					var cusAgent = declaration.CusAgent;
					if (cusAgent != null && !cusAgent.GS_EmailAddress.IsEmpty)
					{
						result.Add(cusAgent.GS_EmailAddress);
					}
				}
			}
			return result.ToArray();
		}

		ZString GetUserEmailWhoSentLastMessage(CusEntryHeader entry, string messageType)
		{
			var result = ZString.Empty;
			if (entry != null)
			{
				var lastMessage = entry.Messages.GetLastMessage(Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport, messageType, Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit);

				if (lastMessage != null)
				{
					var messageSender = lastMessage.UserWhoQueuedThisRecord;
					if (messageSender != null)
					{
						result = messageSender.GS_EmailAddress;
					}
				}
			}
			return result;
		}
	}
}
