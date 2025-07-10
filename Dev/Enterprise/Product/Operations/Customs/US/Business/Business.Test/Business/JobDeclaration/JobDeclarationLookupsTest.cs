using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZoneIDList()
		{
			var newFactory = new BusinessObjectFactory();
			var orgA = newFactory.NewWithValidTestData<OrgHeader>();
			var orgB = newFactory.NewWithValidTestData<OrgHeader>();
			var stmNums1 = newFactory.New<OrganisationViewStmNums>();
			stmNums1.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums1.SN_ZoneIDPrefix = "111DD22";
			stmNums1.SN_ClientPrefix = "AAA";
			stmNums1.SN_Owner = orgA.PK;
			var stmNums2 = newFactory.New<OrganisationViewStmNums>();
			stmNums2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums2.SN_ZoneIDPrefix = "333EE44";
			stmNums2.SN_ClientPrefix = "BBB";
			stmNums2.SN_Owner = orgA.PK;
			var stmNums3 = newFactory.New<OrganisationViewStmNums>();
			stmNums3.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums3.SN_ZoneIDPrefix = "999HHH888";
			stmNums3.SN_ClientPrefix = "EEE";
			stmNums3.SN_Owner = orgA.PK;
			var stmNums4 = newFactory.New<OrganisationViewStmNums>();
			stmNums4.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums4.SN_ZoneIDPrefix = "555FF66";
			stmNums4.SN_ClientPrefix = "CCC";
			stmNums4.SN_Owner = orgB.PK;
			var stmNums5 = newFactory.New<OrganisationViewStmNums>();
			stmNums5.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			stmNums5.SN_ZoneIDPrefix = "777GG88";
			stmNums5.SN_Owner = orgB.PK;
			var stmNums6 = newFactory.New<OrganisationViewStmNums>();
			stmNums6.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			stmNums6.SN_ZoneIDPrefix = "000III888";
			stmNums6.SN_Owner = orgB.PK;
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("", declaration.Lookups.ZoneIDList.CodesAsString);

			declaration.IOROrgPK = orgA.PK;
			AssertEquals("111DD22, 333EE44, 999HHH888", declaration.Lookups.ZoneIDList.CodesAsString);
			declaration.IOROrgPK = ZGuid.Empty;
			declaration.WarehouseDocAddress.E2_OA_Address = orgA.MainAddress.PK;
			AssertEquals("", declaration.Lookups.ZoneIDList.CodesAsString);
			declaration.WarehouseDocAddress.E2_OA_Address = orgB.MainAddress.PK;
			AssertEquals("000III888, 777GG88", declaration.Lookups.ZoneIDList.CodesAsString);
		}

		public void TestReleaseStatusList()
		{
			var releaseStatusList = declaration.Lookups.ReleaseStatusList;
			AssertNotNull(releaseStatusList);
			AssertEquals(typeof(CRLReleaseStatusList), releaseStatusList.GetType());
		}

		public void TestCargoManifestStatusDispositionList()
		{
			AssertEquals("Code Type should SO50D."
				, UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode)
				, declaration.Lookups.CargoManifestStatusDispositionList);
		}

		public void TestDispositionCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "04", "DATA REJECTED PER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "06", "DO NOT DEVAN", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "07", "MAY PROCEED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "08", "MOVE TO SECURE HLDNG FCLTY", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "10", "DOCUMENT REQUIRED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "11", "INTENSIVE – EXAM/SAMPLE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "12", "DOCS REQUIRED – SATISFIED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "13", "EXAM- RESOLVED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var dispositionCodeList = declaration.Lookups.DispositionCodeList;
			AssertEquals("If you added new status code please check the filter 'PGA Status' in US Module Filter and US Declaration Report", 11, dispositionCodeList.Count);
		}

		public void TestRejectedMerchandiseReasonList()
		{
			AssertEquals(typeof(DrawbackRejectedMerchandiseReasonList), declaration.Lookups.RejectedMerchandiseReasonList.GetType());
			AssertEquals("DTI, NCS, RRM, SWC", declaration.Lookups.RejectedMerchandiseReasonList.CodesAsString);

			var lookup1 = declaration.Lookups.RejectedMerchandiseReasonList;
			var declaration2 = Factory.New<JobDeclaration>();
			var lookup2 = declaration2.Lookups.RejectedMerchandiseReasonList;
			AssertSame("Rejected Merchandise Reason List cached", lookup2, lookup1);
		}

		public void TestEntryStatusList()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("EntryStatusList type", typeof(ImportEntryStatusList), declaration.Lookups.EntryStatusList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EntryStatusList type", typeof(AESDirectCustomsEntryStatus), declaration.Lookups.EntryStatusList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("EntryStatusList type", typeof(DrawbackSummaryStatusList), declaration.Lookups.EntryStatusList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var protest = new Protest.Protest(declaration);
			AssertEquals("EntryStatusList type", typeof(ProtestStatusCodesList), declaration.Lookups.EntryStatusList.GetType());
		}

		public void TestMessageStatusList()
		{
			this.declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("MessageStatusList type", typeof(ImportMessageStatusList), this.declaration.Lookups.MessageStatusList.GetType());
			this.declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("MessageStatusList type", typeof(AESDirectCustomsEntryStatus), this.declaration.Lookups.MessageStatusList.GetType());
			this.declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("MessageStatusList type", typeof(DrawbackSummaryStatusList), this.declaration.Lookups.MessageStatusList.GetType());
			this.declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("MessageStatusList type", typeof(FTZMessageStatusList), this.declaration.Lookups.MessageStatusList.GetType());

			var reconDec = new ReconDeclaration(this.declaration);
			AssertEquals(typeof(ReconMessageStatusList), this.declaration.Lookups.MessageStatusList.GetType());

			var declaration = Factory.New<JobDeclaration>();
			var protest = new Protest.Protest(declaration);
			AssertEquals(typeof(ProtestMessageStatusList), declaration.Lookups.MessageStatusList.GetType());
		}

		public void TestCargoIdTypeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.Lookups.CargoIdTypeList.ContainsCode(Enterprise.Core.Constants.ContainerModes.NonContainerised));
			AssertEquals(Enterprise.Core.Constants.ContainerModeDescriptions.Containerised + " (Trans. Mode: 11, 21, 31, 41)", declaration.Lookups.CargoIdTypeList.GetDescriptionFromCode(Enterprise.Core.Constants.ContainerModes.Containerised));
			AssertEquals(Enterprise.Core.Constants.ContainerModeDescriptions.BreakBulk, declaration.Lookups.CargoIdTypeList.GetDescriptionFromCode(Enterprise.Core.Constants.ContainerModes.BreakBulk));
			AssertEquals(Enterprise.Core.Constants.ContainerModeDescriptions.Bulk, declaration.Lookups.CargoIdTypeList.GetDescriptionFromCode(Enterprise.Core.Constants.ContainerModes.Bulk));
			AssertEquals(Enterprise.Core.Constants.ContainerModeDescriptions.Liquid, declaration.Lookups.CargoIdTypeList.GetDescriptionFromCode(Enterprise.Core.Constants.ContainerModes.Liquid));
			AssertEquals(Enterprise.Core.Constants.ContainerModeDescriptions.NonContainerised + " (Trans. Mode: 10, 20, 30, 40)", declaration.Lookups.CargoIdTypeList.GetDescriptionFromCode(Enterprise.Core.Constants.ContainerModes.NonContainerised));
		}

		public void TestJE_TotalNoOfPacksPackType_List()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(ShippingOrPackingingUnitList), declaration.Lookups.JE_TotalNoOfPacksPackType_List.GetType());
		}

		public void TestJobMessageTypeList()
		{
			var list = declaration.Lookups.MessageTypeList;
			Assert("Should be cached", object.ReferenceEquals(list, declaration.Lookups.MessageTypeList));
			AssertEquals(5, list.Count);
			AssertEquals("Export", USJobMessageTypeList.Descriptions.Export, list.GetDescriptionFromCode(JobMessageTypeList.Codes.Export));
			AssertEquals("Import", USJobMessageTypeList.Descriptions.Import, list.GetDescriptionFromCode(JobMessageTypeList.Codes.Import));
			AssertEquals("ImportByExternalBroker", USJobMessageTypeList.Descriptions.ImportByExternalBroker, list.GetDescriptionFromCode(JobMessageTypeList.Codes.ImportByExternalBroker));
			AssertEquals("Miscellaneous", USJobMessageTypeList.Descriptions.Miscellaneous, list.GetDescriptionFromCode(JobMessageTypeList.Codes.Miscellaneous));
			AssertEquals("Export", USJobMessageTypeList.Descriptions.Export, list.GetDescriptionFromCode(JobMessageTypeList.Codes.Export));
			AssertEquals("FTZ", USJobMessageTypeList.Descriptions.FTZ, list.GetDescriptionFromCode(JobMessageTypeList.Codes.FTZ));
		}

		public void TestDeclaration()
		{
			AssertEquals(lookups.Declaration, declaration);
		}

		public void TestElectronicInvoiceStatusList()
		{
			AssertNotNull(lookups.ElectronicInvoiceStatusList);
			AssertEquals(typeof(MessageStatusListEI), lookups.ElectronicInvoiceStatusList.GetType());
		}

		public void TestTransportModeTypeList()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals(true, declaration.IsInBondOnly);
			AssertEquals("There are only four modes", 4, declaration.Lookups.TransportTypeList.Count);
			AssertEquals(true, declaration.Lookups.TransportTypeList.ContainsCode(TransportTypeList.Codes.Sea));
			AssertEquals(true, declaration.Lookups.TransportTypeList.ContainsCode(TransportTypeList.Codes.Rail));
			AssertEquals(true, declaration.Lookups.TransportTypeList.ContainsCode(TransportTypeList.Codes.Truck));
			AssertEquals(true, declaration.Lookups.TransportTypeList.ContainsCode(TransportTypeList.Codes.Air));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(typeof(TransportTypeList), declaration.Lookups.TransportTypeList.GetType());
		}

		public void TestITStatusList()
		{
			AssertNotNull(lookups.ITStatusList);
			AssertEquals(typeof(MessageStatusListIT), lookups.ITStatusList.GetType());
		}

		public void TestUSStatesList()
		{
			AssertEquals("USStatesList", typeof(USStatesList), declaration.Lookups.USStatesList.GetType());
		}

		public void TestStatementStatusList()
		{
			AssertEquals("StatementStatusList", typeof(StatementHeaderStatusList), declaration.Lookups.StatementStatusList.GetType());
			AssertEquals(true, declaration.Lookups.StatementStatusList.ContainsCode(StatementHeaderStatusList.Codes.Preliminary));
			AssertEquals(true, declaration.Lookups.StatementStatusList.ContainsCode(StatementHeaderStatusList.Codes.Final));
			AssertEquals(true, declaration.Lookups.StatementStatusList.ContainsCode(StatementHeaderStatusList.Codes.Deleted));
		}

		public void TestPaymentStatusList()
		{
			AssertEquals("PaymentStatusList", typeof(PaymentStatusList), declaration.Lookups.PaymentStatusList.GetType());
			AssertEquals(true, declaration.Lookups.PaymentStatusList.ContainsCode(PaymentStatusList.Codes.PaymentAuthorizationAccepted));
			AssertEquals(true, declaration.Lookups.PaymentStatusList.ContainsCode(PaymentStatusList.Codes.PaymentFailed));
			AssertEquals(true, declaration.Lookups.PaymentStatusList.ContainsCode(PaymentStatusList.Codes.PaymentInProgress));
		}

		public void TestApplicationCodeList()
		{
			AssertApplicationCodeList(JobMessageTypeList.Codes.Export, true, true, false, false);
			AssertApplicationCodeList(JobMessageTypeList.Codes.FTZ, true, true, false, false);
			AssertApplicationCodeList(JobMessageTypeList.Codes.Import, false, false, true, true);
			AssertApplicationCodeList(JobMessageTypeList.Codes.Miscellaneous, false, false, true, true);
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";

			var declarationApplicationCodesIncludeInterface = new[]
			{
				DeclarationApplicationCodeList.Codes.Interfaced,
				DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted,
				DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted
			};

			foreach (var declarationApplicationCode in declarationApplicationCodesIncludeInterface)
			{
				customsInterface.SubmissionType = declarationApplicationCode;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertApplicationCodeList(JobMessageTypeList.Codes.Import, false, true, true, true);
					AssertApplicationCodeList(JobMessageTypeList.Codes.Miscellaneous, false, true, true, true);
				}
			}
		}

		void AssertApplicationCodeList(ZString messageType, bool isContainsBLT, bool isContainsITF, bool isContainsACE, bool isContainsACS)
		{
			declaration.JE_MessageType = messageType;
			AssertEquals(isContainsBLT, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.Builtin));
			AssertEquals(isContainsITF, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.Interfaced));
			AssertEquals(isContainsACE, declaration.Lookups.ApplicationCodeList.ContainsCode(JobApplicationCodeList.Codes.ACE));
			AssertEquals(isContainsACS, declaration.Lookups.ApplicationCodeList.ContainsCode(JobApplicationCodeList.Codes.ACS));
		}

		public void TestOrganizations()
		{
			AssertNotNull(lookups.Organizations);
			AssertEquals(typeof(OrganisationsFindBoxCollection), lookups.Organizations.GetType());
		}

		public void TestCommercialRulingCodeList()
		{
			AssertNotNull(lookups.CommercialRulingCodeList);
			AssertEquals(typeof(CommercialRulingCodeList), lookups.CommercialRulingCodeList.GetType());
		}

		public void TestSPNIDTypeList()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotNull(lookups.SPNIDTypeList);
			AssertEquals(2, lookups.SPNIDTypeList.Count);
			AssertEquals(true, lookups.SPNIDTypeList.ContainsCode(StandAlonePriorNoticeIDTypeList.Codes.BLN));
			AssertEquals(true, lookups.SPNIDTypeList.ContainsCode(StandAlonePriorNoticeIDTypeList.Codes.ENT));

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(2, lookups.SPNIDTypeList.Count);
			AssertEquals(true, lookups.SPNIDTypeList.ContainsCode(StandAlonePriorNoticeIDTypeList.Codes.BLN));
			AssertEquals(true, lookups.SPNIDTypeList.ContainsCode(StandAlonePriorNoticeIDTypeList.Codes.FTZ));
		}

		public void TestTreatedAsDomesticOrigins()
		{
			declaration.JE_MessageType = USJobMessageTypeList.Codes.Export;
			declaration.US_SchDLoading = "51xx";
			AssertEquals(typeof(RefUNLOCOCollection), lookups.Origins.GetType());
			var prPort = Factory.New<RefUNLOCO>();
			prPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			Assert("Contain PR ports", lookups.Origins.Contains(prPort));

			var viPort = Factory.New<RefUNLOCO>();
			viPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			Assert("Contain VI ports", lookups.Origins.Contains(viPort));
		}

		public void TestFinalDestinations()
		{
			AssertEquals(typeof(RefUNLOCOCollection), lookups.FinalDestinations.GetType());
			var prPort = Factory.New<RefUNLOCO>();
			prPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			Assert("Contain PR ports", lookups.FinalDestinations.Contains(prPort));

			var viPort = Factory.New<RefUNLOCO>();
			viPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			Assert("Contain VI ports", lookups.FinalDestinations.Contains(viPort));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			lookups = new JobDeclarationLookups(declaration);
		}

		protected JobDeclarationLookups lookups;
		protected JobDeclaration declaration;
	}
}
