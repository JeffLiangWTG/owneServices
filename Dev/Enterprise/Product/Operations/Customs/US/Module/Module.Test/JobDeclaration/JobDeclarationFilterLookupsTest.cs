using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestApplicationCodeList()
		{
			var applicationCodeList = lookups.ApplicationCodeList();
			AssertEquals("ApplicationCodeList of correct type.", typeof(CodeDescriptionPairList), applicationCodeList.GetType());
			AssertEquals("ApplicationCodeList should have 4 values.", 4, applicationCodeList.Count);
			AssertEquals(JobApplicationCodeList.Descriptions.ACE, applicationCodeList["ACE"].Description);
			AssertEquals(JobApplicationCodeList.Descriptions.ACS, applicationCodeList["ACS"].Description);
			AssertEquals(Customs.Business.DeclarationApplicationCodeList.Descriptions.Builtin, applicationCodeList["BLT"].Description);
			AssertEquals(Customs.Business.DeclarationApplicationCodeList.Descriptions.Interfaced, applicationCodeList["ITF"].Description);
			AssertSame(applicationCodeList, filterBizObj.Factory.GetCachedValue<CodeDescriptionPairList>("USDeclarationApplicationCodeList", () => null));
		}

		public void TestMessageTypeList()
		{
			var result = lookups.MessageTypeList;
			AssertEquals("MessageTypeList", typeof(USJobMessageTypeList), result.GetType());
			AssertEquals("EXP, FTZ, IMP, IMX, MSC", result.CodesAsString);
		}

		public void TestInsuranceDispositionCodeList()
		{
			AssertEquals("InsuranceDispositionCodeList", typeof(InsuranceDispositionCodeList), lookups.InsuranceDispositionCodeList.GetType());
		}

		public void TestTransportTypeList()
		{
			AssertEquals("TransportTypeList", typeof(TransportTypeList), lookups.TransportTypeList.GetType());
			AssertEquals("AIR, AUT, BWB, FIX, MAI, PHC, PED, RAI, ROA, SEA, TRK", lookups.TransportTypeList.CodesAsString);
		}

		public void TestContainerModeList()
		{
			AssertEquals("BBK, BLK, CNT, LQD, NCT", lookups.ContainerModeList.CodesAsString);
		}

		public void TestEntryModesList()
		{
			AssertNotNull(lookups.EntryModes);
		}

		public void TestEntryStatusLists()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var filter = (ModuleTextFilter)filterBizObj[DeclarationFilterConstants.ShipmentType];
				filter.Property = JobMessageTypeList.Codes.Export;
				AssertEquals("Should come from " + nameof(AESDirectCustomsEntryStatus), new AESDirectCustomsEntryStatus().Count + 2, lookups.EntryStatusList().Count);
				filter.Property = JobMessageTypeList.Codes.Import;
				AssertEquals("Import Entry Status", new ImportEntryStatusList().Count + 2, lookups.EntryStatusList().Count);
				filter.Property = "";
				AssertEquals("All entry status", new AESDirectCustomsEntryStatus().Count + new ImportEntryStatusList().Count + 2, lookups.EntryStatusList().Count);
			}
		}

		public void TestEntrySummaryActionsList()
		{
			var list = lookups.EntrySummaryActionsList;
			AssertNotNull(list);
			AssertEquals(2, list.Count);
		}

		public void TestLocationOfGoodsList()
		{
			AssertEquals("LocationOfGoodsList", typeof(BondedWarehouseCollection), lookups.LocationOfGoodsList.GetType());
		}

		public void TestElectronicInvoiceStatusListForFilter()
		{
			var list = lookups.ElectronicInvoiceStatusListForFilter;
			AssertNotNull(list);
			var messageStatusListEI = new MessageStatusListEI();
			AssertEquals("ElectronicInvoiceStatusListForFilter should contain MessageStatusListEI.Count elements", messageStatusListEI.Count, list.Count);
			Assert("ElectronicInvoiceStatusListForFilter should contain DeclarationFilterConstants.MessageStatus.NotSentForFilter = \"NOT\"", list.ContainsCode(DeclarationFilterConstants.MessageStatus.NotSentForFilter));
		}

		public void TestMessageStatusListForBLU()
		{
			AssertEquals("MessageStatusListForBLU", typeof(MessageStatusListBLU), lookups.MessageStatusListForBLU.GetType());
			AssertEquals("Should be 3 elements in this list", 3, lookups.MessageStatusListForBLU.Count);
		}

		public void TestFDAMsgStatusList()
		{
			AssertEquals("FDAMsgStatusList", typeof(FDAStatusList), lookups.FDAMsgStatusList.GetType());
		}

		public void TestFDAStatusList()
		{
			AssertEquals("FDAStatusList", typeof(FDAEntryLevelDispositionCodeList), lookups.FDAStatusList.GetType());
		}

		public void TestReleaseStatusList()
		{
			AssertEquals("ReleaseStatusList", typeof(CRLReleaseStatusList), lookups.ReleaseStatusList.GetType());
			Assert(!lookups.ReleaseStatusList.ContainsCode(Enterprise.Customs.US.Business.CRLReleaseStatusList.Codes.HLD));
		}

		public void TestInbondCommonTypeList()
		{
			AssertEquals("InbondCommonTypeList", Factory.GetCachedValue<InbondCommonTypeList>(), lookups.InbondCommonTypeList);
		}

		public void TestISFBillStatusList()
		{
			AssertEquals("ISFBillStatusList should contain multiple", "Multiple bills have different statuses.", lookups.ISFBillStatusList.GetDescriptionFromCode(Common.US.ISF.ISFStatusHelper.Multiple));
		}

		public void TestSEBillStatusList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), lookups.SEBillStatusList.GetType());
		}

		public void TestHLDOrEXMStatusList()
		{
			AssertEquals(typeof(HLDOrEXMStatusList), lookups.HLDOrEXMStatusList.GetType());
		}

		public void TestMessageStatusListForCRL()
		{
			AssertEquals("MessageStatusListForCRL", typeof(MessageStatusListCRL), lookups.MessageStatusListForCRL.GetType());
			AssertEquals("Should be 34 Cargo Release related status codes in this list", 37, lookups.MessageStatusListForCRL.Count);
			AssertEquals("Not Sent code description should be: 'Not Sent - only valid for exact match'", DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription, lookups.MessageStatusListForCRL.GetDescriptionFromCode("NOT"));
		}

		public void TestMessageStatusListForENS()
		{
			AssertEquals("MessageStatusListForENS", typeof(MessageStatusListENS), lookups.MessageStatusListForENS.GetType());
			AssertEquals("Should be 21 Entry Summary related status codes in this list", 21, lookups.MessageStatusListForENS.Count);
			AssertEquals("Not Sent code description should be: 'Not Sent - only valid for exact match'", DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription, lookups.MessageStatusListForENS.GetDescriptionFromCode("NOT"));
			AssertEquals("Entry Cancelled status", ImportMessageStatusList.Descriptions.EntrySummaryCanceled, lookups.MessageStatusListForENS.GetDescriptionFromCode(ImportMessageStatusList.Codes.EntrySummaryCanceled));
		}

		public void TestMessageStatusListForINB()
		{
			AssertEquals("MessageStatusListForINB", typeof(MessageStatusListIT), lookups.MessageStatusListForINB.GetType());
			AssertEquals("Should be 22 IT related status codes in this list", 22, lookups.MessageStatusListForINB.Count);
			AssertEquals("Not Sent code description should be: 'Not Sent - only valid for exact match'", DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription, lookups.MessageStatusListForINB.GetDescriptionFromCode("NOT"));
		}

		public void TestLists()
		{
			AssertEquals("PaymentTypeList", typeof(PaymentTypeList), lookups.PaymentTypeList.GetType());
			AssertEquals("TaxDeferIndicatorList", typeof(TaxDeferIndicatorList), lookups.TaxDeferIndicatorList.GetType());
			AssertEquals("RegionalPorts", typeof(ZZRefCusCodeListCombinedCollection), lookups.RegionalPorts.GetType());
			AssertEquals("AESResponseCodeList", typeof(ZZRefCusCodeListCombinedCollection), lookups.AESResponseCodeList.GetType());
		}

		public void TestReconIssueList()
		{
			AssertEquals(false, lookups.ReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
		}

		public void TestJobApplicationCodeList()
		{
			AssertEquals("ApplicationCodeList", typeof(JobApplicationCodeList), lookups.JobApplicationCodeList.GetType());
		}

		public void TestJobHeaderStatusList()
		{
			AssertEquals("JobHeaderStatusList", typeof(JobHeaderStatusList), lookups.JobHeaderStatusList.GetType());
		}

		public void TestNotifyParties()
		{
			AssertEquals("NotifyParties", typeof(OrgHeaderCollection), lookups.NotifyParties.GetType());
		}

		public void TestSoldToParties()
		{
			AssertEquals("SoldToParties", typeof(OrgHeaderCollection), lookups.SoldToParties.GetType());
		}

		public void TestMessageStatusListsForFTZ()
		{
			Assert(lookups.FTZAdmissionStatusList.ContainsCode(FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd));
			Assert(!lookups.FTZAdmissionStatusList.ContainsCode(FTZMessageStatusList.Codes.ClearPermitToTransfer));
			Assert(lookups.FTZConcurrenceStatusList.ContainsCode(FTZMessageStatusList.Codes.ClearConcurrence));
			Assert(!lookups.FTZConcurrenceStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd));
			Assert(lookups.FTZDeliveryOfGoodsStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingDeliveryOfGoods));
			Assert(!lookups.FTZDeliveryOfGoodsStatusList.ContainsCode(FTZMessageStatusList.Codes.ClearConcurrence));
			Assert(lookups.FTZGoodsArrivalStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingGoodsArrival));
			Assert(!lookups.FTZGoodsArrivalStatusList.ContainsCode(FTZMessageStatusList.Codes.ClearFTZAdmissionAmend));
			Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.ClearPermitToTransfer));
			Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.ErrorGoodsArrival));
			Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer));
			Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferCancelAccepted));
			Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized));
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				filterBizObj.Factory.ClearCachedValue<CodeDescriptionPairList>("FTZPTTMessageStatusList");
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival));
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferUnArrived));
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized));
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival));
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferArrived));
				Assert(lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferUnArrived));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				filterBizObj.Factory.ClearCachedValue<CodeDescriptionPairList>("FTZPTTMessageStatusList");
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival));
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferUnArrived));
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized));
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival));
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferArrived));
				Assert(!lookups.FTZPTTStatusList.ContainsCode(FTZMessageStatusList.Codes.PermitToTransferUnArrived));
			}
		}

		public void TestSPIList()
		{
			AssertEquals(39, lookups.SPIList.Count);
			Assert(lookups.SPIList.ContainsCode(SpecialProgramList.Codes.S));
			Assert(lookups.SPIList.ContainsCode(SpecialProgramList.Codes.SPlus));
			Assert(lookups.SPIList.ContainsCode(SpecialProgramList.Codes.CA));
			Assert(lookups.SPIList.ContainsCode(SpecialProgramList.Codes.MX));
		}

		public void TestAESSeverityList()
		{
			var newFactory = new BusinessObjectFactory();
			var cusCodeHelper = new UniversalReferenceTestDataHelper(newFactory);
			var aessv = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESSeverityIndicator;
			var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType(aessv, "Customs Status");

			var space = DeclarationFilterConstants.FilterCodeAndDesc.Space;
			var expectedCodes = new[] { space, "I", "W", "V", "C", "F" };
			foreach (var code in expectedCodes)
			{
				cusCodeHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, aessv, code, "TST DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			}
			newFactory.Save();

			AssertEquals(expectedCodes.Length, lookups.AESSeverityList.Count);
			AssertContainsExactElementsInExactOrder(new[] { "C", "F", "I", "V", "W", space }, lookups.AESSeverityList.Select(x => x.ZZD_Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}
