using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	sealed class NctsHeaderTest : NctsHeaderAbstractTest
	{
		public void TestNctsHeaderValidation()
		{
			AssertType<NctsHeaderValidation>(nctsHeader.Validation);

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsHeaderPhase5Validation>(nctsHeader2.Validation);
		}

		public void TestNctsHeaderLookups()
		{
			AssertType<NctsHeaderLookups>(nctsHeader.Lookups);
		}

		public void TestGetEquipmentsChild()
		{
			AssertType<CusInBondEquipmentCollection>(nctsHeader.Equipments);
		}

		public void TestGetManifestsToOpenListChild()
		{
			AssertType<NctsManifestsToOpenCollection>(nctsHeader.ManifestsToOpenList);
		}

		public void TestGuarantees_Phase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				nctsHeader.Guarantees.AddNew();
				var result = nctsHeader.Guarantees;
				AssertEquals("Parent", nctsHeader.MovementHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
			});
		}

		public void TestGuarantees_Phase4()
		{
			CombineAssertions(() =>
			{
				nctsHeader.Guarantees.AddNew();
				var result = nctsHeader.Guarantees;
				AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
			});
		}

		public void TestGetStampDutyCargoDescsChild()
		{
			AssertType<StampDutyCargoDescCollection>(nctsHeader.StampDutyCargoDescs);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsHeader).GetCusSupportingInfoTypes();
			AssertEquals(typeof(NctsManifestsToOpen), actualTypes[CusSupportingInfoTypeList.Codes.MTO]);
			AssertEquals(typeof(CommonPreviousDocument), actualTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals(typeof(NctsAdditionalInfo), actualTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals(typeof(RequestedDocument), actualTypes[Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument]);
		}

		public void TestGetFetchStrategies()
		{
			var expectedTypes = new[]
			{
				typeof(CusAddInfoTypeSupporterFetchStrategy),
				typeof(CusCodeDataTypeSupporterFetchStrategy),
				typeof(CusSupportingInfoTypeSupporterFetchStrategy)
			};
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)nctsHeader).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestTrailer1AndTrailer2()
		{
			nctsHeader.Trailer1 = "AB1234CD";
			nctsHeader.Trailer2 = "AB5678CD";

			Factory.Save();

			AssertEquals(nctsHeader.Equipment1.BJ_RegistrationNumber, "AB1234CD");
			AssertEquals(nctsHeader.Equipment2.BJ_RegistrationNumber, "AB5678CD");

			nctsHeader.Trailer2 = ZString.Empty;
			nctsHeader.Trailer1 = ZString.Empty;

			Factory.Save();

			var query1 = new ZQuery(CusInBondEquipmentSchema.BJ_BH_Header, nctsHeader.PK);
			query1.AddToFilter(CusInBondEquipmentSchema.BJ_ACEID, "Trailer1");
			var equipment1 = Factory.LoadTop1<CusInBondEquipment>(query1);
			AssertNull(equipment1);

			var query2 = new ZQuery(CusInBondEquipmentSchema.BJ_BH_Header, nctsHeader.PK);
			query2.AddToFilter(CusInBondEquipmentSchema.BJ_ACEID, "Trailer2");
			var equipment2 = Factory.LoadTop1<CusInBondEquipment>(query2);
			AssertNull(equipment2);
		}

		public void TestBM_CustomsStatus()
		{
			var header = nctsHeader as Integration.Customs.TR.ICusInBondHeader;
			nctsHeader.MovementHeader.BM_CustomsStatus = "CTR";
			AssertEquals("Test getter", "CTR", header.BM_CustomsStatus);

			header.BM_CustomsStatus = "MRN";
			AssertEquals("Test setter", "MRN", nctsHeader.MovementHeader.BM_CustomsStatus);
		}

		public void TestSetDefaltValues()
		{
			AssertEquals(StampDutyStatusCodeList.Codes.D1, nctsHeader.StampDutyStatus);
			var customsOfficesForDepartureCode = nctsHeader.CustomsOfficesForDeparture.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).FirstOrDefault().CY_Code;
			AssertEquals(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit, customsOfficesForDepartureCode);
		}

		public void TestCloneCustomsOffices()
		{
			var customsOffice = nctsHeader.CustomsOffices.FirstOrDefault();
			customsOffice.CY_ParentID = nctsHeader.PK;
			customsOffice.CY_Data = "TR066666";

			var officeTXT = nctsHeader.CustomsOfficesForDeparture.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			CombineAssertions("Original NCTS", () =>
			{
				AssertEquals("NCTS Office Type TXT Count", 1, officeTXT.Count());
				AssertEquals("NCTS Office Type TXT Code", "TR066666", officeTXT.First().CY_Data);
			});

			var clonedNCTS = (NctsHeader)nctsHeader.TemplateCopy();
			officeTXT = clonedNCTS.CustomsOfficesForDeparture.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			CombineAssertions("Cloned NCTS", () =>
			{
				AssertEquals("NCTS Office Type TXT Count", 1, officeTXT.Count());
				AssertEquals("NCTS Office Type TXT Code", "TR066666", officeTXT.First().CY_Data);
			});
		}

		public void TestSetMaxLengthForStampDutyStatus()
		{
			AssertEquals(1, nctsHeader.StampDutyStatus.Length);
		}

		public void TestStampDutyPropertyName()
		{
			AssertEquals("Stamp Duty", nctsHeader.StampDutyInfo.HumanReadableName);
		}

		public void TestStampDutyStatusPropertyName()
		{
			AssertEquals("Stamp Duty Status", nctsHeader.StampDutyStatusInfo.HumanReadableName);
		}

		public void TestRegistrationDatePropertyName()
		{
			AssertEquals("Stamp Duty Ledger Registration Date", nctsHeader.RegistrationDateInfo.HumanReadableName);
		}

		public void TestLrnRegistrationNumberAndDate()
		{
			nctsHeader.LrnRegistrationNumber = "1111111111";
			nctsHeader.LrnRegistrationDate = new ZDate(2024, 01, 01);
			AssertNotNull(nctsHeader.LrnRegistrationNumber);
			AssertNotNull(nctsHeader.LrnRegistrationDate);

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader2, CusEntryNumberTypes.Standard.LocalReferenceNumber, nctsHeader2.CountryCode);
			entryNum.CE_EntryNum = "222222222";
			entryNum.CE_IssueDate = new ZDate(2024, 02, 02);
			AssertEquals("222222222", nctsHeader2.LrnRegistrationNumber);
			AssertEquals(new ZDate(2024, 02, 02), nctsHeader2.LrnRegistrationDate);
		}

		public void TestArrivalMrnFromUserAndMrnIssueDateFromUser()
		{
			nctsHeader.ArrivalMrnFromUser = "1111111111";
			nctsHeader.MrnIssueDateFromUser = new ZDate(2024, 01, 01);
			AssertNotNull(nctsHeader.ArrivalMrnFromUser);
			AssertNotNull(nctsHeader.MrnIssueDateFromUser);

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader2, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader2.CountryCode);
			entryNum.CE_EntryNum = "222222222";
			entryNum.CE_IssueDate = new ZDate(2024, 02, 02);
			AssertEquals("222222222", nctsHeader2.ArrivalMrnFromUser);
			AssertEquals(new ZDate(2024, 02, 02), nctsHeader2.MrnIssueDateFromUser);
		}

		public void TestCaptions()
		{
			var captions = new (string PropertyName, string ExpectedCaption, string ExpectedMediumCaption, string ExpectedShortCaption)[]
			{
				(nameof(NctsHeader.Trailer1), "Trailer 1", null, null),
				(nameof(NctsHeader.Trailer2), "Trailer 2", null, null),
				(nameof(NctsHeader.StampDuty), "Stamp Duty", null, null),
				(nameof(NctsHeader.StampDutyStatus), "Stamp Duty Status", null, null),
				(nameof(NctsHeader.RegistrationDate), "Stamp Duty Ledger Registration Date", null, "Registration Date"),
				(nameof(NctsHeader.LrnRegistrationDate), "Issue Date", "Issue Date", "Issue Date"),
				(nameof(NctsHeader.MrnIssueDateFromUser), "MRN Date", "MRN Date", "MRN Date")
			};

			foreach (var (propertyName, expectedCaption, expectedMediumCaption, expectedShortCaption) in captions)
			{
				if (expectedCaption != null)
				{
					AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsHeader), propertyName, false, attribute => attribute.Caption == expectedCaption);
				}
				if (expectedMediumCaption != null)
				{
					AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsHeader), propertyName, false, attribute => attribute.MediumCaption == expectedMediumCaption);
				}
				if (expectedShortCaption != null)
				{
					AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NctsHeader), propertyName, false, attribute => attribute.ShortCaption == expectedShortCaption);
				}
			}
		}

		public void TestLrnRegistrationNumber_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(nctsHeader.LrnRegisterNumberInfo, NctsHeader.Phase4CaptionKey, "LRN", "LRN", "LRN");
		}

		public void TestLrnRegistrationNumber_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(nctsHeader.LrnRegisterNumberInfo, NctsHeader.Phase5CaptionKey, "Local Registration Number", "", "LRN");
		}

		public void TestReadOnlyFields()
		{
			CombineAssertions("Read-Only", () =>
			{
				AssertEquals("Lrn Registration Number", true, nctsHeader.LrnRegisterNumberInfo.ReadOnly);
				AssertEquals("Lrn Registration Date", true, nctsHeader.LrnRegistrationDateInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => nctsHeader;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			temporaryNcts4Setting = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true);
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}
		NctsHeader nctsHeader;

		protected override void TearDown()
		{
			base.TearDown();
			temporaryNcts4Setting?.Dispose();
			temporaryNcts4Setting = null;
		}
		IDisposable temporaryNcts4Setting;
	}
}
