using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEEntrySummaryQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateQueryFromReconDeclaration()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			entryHeader.EntryNumber = "32377722";

			var aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
			var message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("Message Text should contains only J1 block",
				"B      XJ5JC                                               <<MSGNO PLACEHOLDER>>J1   SV9  32377722                                                              Y      XJ5JC",
				message.EM_MessageText);
			AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, message.EM_MessageType);
			AssertEquals("Message is attached to entry", entryHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("Message owner is empty", ZString.Empty, message.EM_MessageOwner);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				AssertEquals("Message Text should contains only J1 block",
					"B      XJ5EQ                                               <<MSGNO PLACEHOLDER>>J1   SV9  32377722                                                              Y      XJ5EQ",
					message.EM_MessageText);
				AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery, message.EM_MessageType);
				AssertEquals("Message is attached to entry", entryHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message owner is ACE", "ACE", message.EM_MessageOwner);
			}
		}

		public void TestGenerateQueryFromDeclaration()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "32377722";

			var aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
			var message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("Message Text should contains only J1 block",
				"B      XJ5JC                                               <<MSGNO PLACEHOLDER>>J1   SV9  32377722                                                              Y      XJ5JC",
				message.EM_MessageText);
			AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, message.EM_MessageType);
			AssertEquals("Message is attached to entry", entryHeader.PK, message.EM_LinkUniqueID);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				AssertEquals("Message Text should contains only J1 block",
					"B      XJ5EQ                                               <<MSGNO PLACEHOLDER>>J1   SV9  32377722                                                              Y      XJ5EQ",
					message.EM_MessageText);
				AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery, message.EM_MessageType);
				AssertEquals("Message is attached to entry", entryHeader.PK, message.EM_LinkUniqueID);
			}
		}

		public void TestGenerateQueryFromDeclarationForRemoteLocationFiling()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "XJ5" });
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "3902");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "2704";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "3902";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "32377722";

			var aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
			var message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			//not 2704, but 3902
			AssertContains("B  3902XJ5JC                                               ", message.EM_MessageText);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				//not 2704, but 3902
				AssertContains("B  3902XJ5EQ                                               ", message.EM_MessageText);
			}
		}

		[TestDate(2009, 1, 5, 0, 7, 3)]
		public void TestDateTimeWith12hFormatForJ2()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var entrySummaryQueryBizObj = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryBizObj.EntryNumber = "";
			entrySummaryQueryBizObj.CriteriaCode = CriteriaCodeList.Codes.PSC;
			entrySummaryQueryBizObj.DateFrom = ZDateTime.Today.AddDays(-7);
			entrySummaryQueryBizObj.DateTo = ZDateTime.Today.AddDays(-1);
			entrySummaryQueryBizObj.ConsumptionEntrySummaries = true;
			entrySummaryQueryBizObj.FTAReconSummaries = true;
			entrySummaryQueryBizObj.OtherReconSummaries = true;
			entrySummaryQueryBizObj.DrawbackSummaries = true;
			entrySummaryQueryBizObj.NAFTADutyDeferralSummaries = true;
			var aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
			var message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("J2 block has DateTime with format (MMddyyhhmmsstt)",
				"B      XJ5JC                                               <<MSGNO PLACEHOLDER>>J2 PSC 122908120000AM010409115959PM YYYYY                                       Y      XJ5JC",
				message.EM_MessageText);

			AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, message.EM_MessageType);
			AssertEquals("Standalone query", ZGuid.Empty, message.EM_LinkUniqueID);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				entrySummaryQueryBizObj.CollectionBillInformationCode = CollectionBillInformationCodeList.Codes._01;
				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				AssertEquals("J2 block has DateTime with format (MMddyyhhmmsstt)",
				"B      XJ5EQ                                               <<MSGNO PLACEHOLDER>>J2 PSC 122908120000AM010409115959PM YYYYY1                                      Y      XJ5EQ",
				message.EM_MessageText);

				AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery, message.EM_MessageType);
				AssertEquals("Standalone query", ZGuid.Empty, message.EM_LinkUniqueID);
			}
		}

		[TestDate(2011, 02, 17)]
		public void TestGenerateStandaloneSummaryQuery()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var entrySummaryQueryBizObj = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryBizObj.EntryNumber = "";
			entrySummaryQueryBizObj.CriteriaCode = CriteriaCodeList.Codes.PSC;
			entrySummaryQueryBizObj.DateFrom = ZDateTime.Today.AddDays(-7);
			entrySummaryQueryBizObj.DateTo = ZDateTime.Today.AddDays(-1);
			entrySummaryQueryBizObj.ConsumptionEntrySummaries = true;
			entrySummaryQueryBizObj.FTAReconSummaries = true;
			entrySummaryQueryBizObj.OtherReconSummaries = true;
			entrySummaryQueryBizObj.DrawbackSummaries = true;
			entrySummaryQueryBizObj.NAFTADutyDeferralSummaries = true;
			var aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
			var message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("Message generated should contains only J2 block",
				"B      XJ5JC                                               <<MSGNO PLACEHOLDER>>J2 PSC 021011120000AM021611115959PM YYYYY                                       Y      XJ5JC",
				message.EM_MessageText);

			AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, message.EM_MessageType);
			AssertEquals("Standalone query", ZGuid.Empty, message.EM_LinkUniqueID);

			entrySummaryQueryBizObj.EntryNumber = "00015478";
			aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
			message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("Message generated should contains only J1 block, because Entry Number exists, even other criteria have entered",
				"B      XJ5JC                                               <<MSGNO PLACEHOLDER>>J1   XJ5  00015478                                                              Y      XJ5JC",
				message.EM_MessageText);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				entrySummaryQueryBizObj.EntryNumber = "";
				entrySummaryQueryBizObj.CollectionBillInformationCode = CollectionBillInformationCodeList.Codes._01;
				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				AssertEquals("Message generated should contains only J2 block",
				"B      XJ5EQ                                               <<MSGNO PLACEHOLDER>>J2 PSC 021011120000AM021611115959PM YYYYY1                                      Y      XJ5EQ",
				message.EM_MessageText);

				AssertEquals("EM_MessageType", ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery, message.EM_MessageType);
				AssertEquals("Standalone query", ZGuid.Empty, message.EM_LinkUniqueID);

				var entryNumbers = entrySummaryQueryBizObj.EntryNumbers;
				entryNumbers.AddNew().EntryNumber = "00000001";
				entryNumbers.AddNew().EntryNumber = "00000002";
				entryNumbers.AddNew().EntryNumber = "00000003";
				entryNumbers.AddNew().EntryNumber = "00000004";
				entryNumbers.AddNew().EntryNumber = "00000005";
				entryNumbers.AddNew().EntryNumber = "00000006";

				aceEntrySummaryQueryMessageBuilder = new ACEEntrySummaryQueryMessageBuilder(entrySummaryQueryBizObj);
				message = aceEntrySummaryQueryMessageBuilder.PopulateMessage();

				AssertEquals("Message generated should contains only J1 block, because Entry Number exists, even other criteria have entered",
				"B      XJ5EQ                                               <<MSGNO PLACEHOLDER>>J1   XJ5  00000001XJ5  00000002XJ5  00000003XJ5  00000004XJ5  00000005          J1   XJ5  00000006                                                              Y      XJ5EQ",
				message.EM_MessageText);
			}
		}
	}
}
