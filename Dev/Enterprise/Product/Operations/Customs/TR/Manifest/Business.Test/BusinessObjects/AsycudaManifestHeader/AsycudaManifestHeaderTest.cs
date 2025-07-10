using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(ASYCUDA.Business.AsycudaManifestHeader))]
	public partial class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		#region ICusSupportingInfoTypeSupporterMembers

		public void TestGetCusSupportingInfoTypeSupporterMembers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var actualTypes = ((ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes();
			AssertEquals(typeof(ManifestToOpen), actualTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestGetFetchStrategies()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var expectedTypes = new[]
			{
				typeof(CusSupportingInfoTypeSupporterFetchStrategy),
				typeof(CusCodeDataTypeSupporterFetchStrategy)
			};
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)header).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}
		#endregion

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestIsBillRelatedDeclarationsTabVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			CombineAssertions("Export | HAVIHR | ShippingLine", () =>
			{
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_Nature = ShipmentTypeList.Codes.Export22;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | DENIHR | ShippingLine", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | CIKONC | ShippingLine", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | ATAIHR | ShippingLine", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				AssertEquals("Ensure bills should not be saved", false, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Import | ShippingLine", () =>
			{
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				AssertEquals("Ensure bills should not be saved", false, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | HAVIHR | Consolidator", () =>
			{
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | DENIHR | Consolidator", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | CIKONC | Consolidator", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Export | ATAIHR | Consolidator", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				AssertEquals("Ensure bills should not be saved", false, header.IsBillRelatedDeclarationsTabVisible);
			});

			CombineAssertions("Import | Consolidator", () =>
			{
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				AssertEquals("Ensure bills should not be saved", false, header.IsBillRelatedDeclarationsTabVisible);
			});
		}
		public void TestDeletingRelatedDeclarationsOnSaving()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;

			AssertEquals("Should be an export", true, header.IsExport);
			AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);

			var bill1 = header.Bills.AddNew();
			var rd1 = bill1.RelatedDeclarationForExports.AddNew();
			rd1.CSI_ReferenceNumber = "RD1";
			var rd2 = bill1.RelatedDeclarationForExports.AddNew();
			rd2.CSI_ReferenceNumber = "RD2";

			var bill2 = header.Bills.AddNew();
			var rd3 = bill2.RelatedDeclarationForExports.AddNew();
			rd3.CSI_ReferenceNumber = "RD3";
			var rd4 = bill2.RelatedDeclarationForExports.AddNew();
			rd4.CSI_ReferenceNumber = "RD4";

			Factory.Save();

			var checkRD1 = Factory.Load<RelatedDeclarationForExport>(rd1.PK);
			var checkRD2 = Factory.Load<RelatedDeclarationForExport>(rd2.PK);
			var checkRD3 = Factory.Load<RelatedDeclarationForExport>(rd3.PK);
			var checkRD4 = Factory.Load<RelatedDeclarationForExport>(rd4.PK);

			CombineAssertions("Related Decs should exist", () =>
			{
				AssertNotNull("RD1", checkRD1);
				AssertNotNull("RD2", checkRD2);
				AssertNotNull("RD3", checkRD3);
				AssertNotNull("RD4", checkRD4);
			});

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

			AssertEquals("Should still be an export", true, header.IsExport);
			AssertEquals("Ensure bills should be saved", true, header.IsBillRelatedDeclarationsTabVisible);

			Factory.Save();

			checkRD1 = Factory.Load<RelatedDeclarationForExport>(rd1.PK);
			checkRD2 = Factory.Load<RelatedDeclarationForExport>(rd2.PK);
			checkRD3 = Factory.Load<RelatedDeclarationForExport>(rd3.PK);
			checkRD4 = Factory.Load<RelatedDeclarationForExport>(rd4.PK);

			CombineAssertions("Related Decs should exist", () =>
			{
				AssertNotNull("RD1", checkRD1);
				AssertNotNull("RD2", checkRD2);
				AssertNotNull("RD3", checkRD3);
				AssertNotNull("RD4", checkRD4);
			});
		}

		public void TestIsExport()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(manifestHeader.IsExport);

			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Assert(!manifestHeader.IsExport);

			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			Assert(!manifestHeader.IsExport);

			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			Assert(!manifestHeader.IsExport);
		}

		public void TestIsImport()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Assert(manifestHeader.IsImport);

			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(!manifestHeader.IsImport);
		}

		public void TestTIRNumber()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.TIRNumber = "TIR1";
			AssertNotNull(manifestHeader.TIREntryNumber);
			AssertEquals("TIR1", manifestHeader.TIREntryNumber.CE_EntryNum);

			manifestHeader.TIRNumber = "";
			Factory.Save();
			AssertNull(manifestHeader.TIREntryNumber);

			var manifestHeader2 = Factory.New<AsycudaManifestHeader>();
			var entryNum = CusEntryNumber.LoadOrCreate(manifestHeader2, CusEntryNumberTypes.Turkey.TIR, manifestHeader2.AMA_RN_NKCountry);
			entryNum.CE_EntryNum = "TIR2";
			AssertEquals("TIR2", manifestHeader2.TIRNumber);

			entryNum.CE_EntryType = "";
			AssertEquals("", manifestHeader2.TIRNumber);
		}

		public void TestTIRNumberCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				AssertEquals("Groupage Bill No", header.TIRNumberCaption.Caption);

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Groupage Bill No", header.TIRNumberCaption.Caption);

				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("TIR/ATA Carnet No", header.TIRNumberCaption.Caption);

				header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				AssertEquals("TIR/ATA Carnet No", header.TIRNumberCaption.Caption);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestGetCusCodeDataTypes()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaManifestHeader>();
			AssertEquals(1, cusCodeDataTypeSupporter.GetCusCodeDataTypes().Count);
		}

		public void TestICusCodeDataTypeSupporterForHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			ICusCodeDataTypeSupporter supporter = header;
			supporter.AssertType(typeof(VisitedPort), CusCodeDataTypeList.Codes.TRVisitedPort);
			var visitedPort = header.VisitedPorts.AddNew();
			visitedPort.CY_Code = "TRMER-001";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(visitedPort.PK);
			AssertEquals(typeof(VisitedPort), codeData.GetType());
		}

		public void TestCusCodeDataSupporterFetchStrategies()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaManifestHeader>();
			AssertEquals("Base has CusCodeDatTypes so there is at least one Fetch Strategy", true, cusCodeDataTypeSupporter.GetFetchStrategies().Any());
		}

		public void TestLloydsNumber()
		{
			var header = Customs.ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.EMANIF);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_LloydsNumber = "1234567";
			Factory.Save();

			AssertEquals("1234567", header.AMA_LloydsNumber);
		}

		public void TestLloydsNumberCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
			AssertEquals("Reference No", header.LloydsNumberCaption.Caption);

			header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
			AssertEquals("Reference No", header.LloydsNumberCaption.Caption);

			header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			AssertEquals("Reference No", header.LloydsNumberCaption.Caption);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Vessel IMO Number", header.LloydsNumberCaption.Caption);
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Assert(!header.IsNonePackedItemRelationship);
			Assert(!header.IsOnePackedItemRelationship);
			Assert(header.IsManyPackedItemRelationship);

			var bill = header.Bills.AddNew();
			Assert(!bill.IsNonePackedItemRelationship);
			Assert(!bill.IsOnePackedItemRelationship);
			Assert(bill.IsManyPackedItemRelationship);

			var package = bill.Packs.AddNew();
			Assert(!package.IsNonePackedItemRelationship);
			Assert(!package.IsOnePackedItemRelationship);
			Assert(package.IsManyPackedItemRelationship);
		}

		public void TestThatTIRValidationIsCalledWhenValidationIsNotSuspended()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;

			header.TIRNumber = "TIR2";
			AssertNoMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.TIRNumber = "";
			AssertHasMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ManifestType = TRManifestTypes.Codes.ATAITH;
			header.TIRNumber = "TIR2";
			AssertNoMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.TIRNumber = "";
			AssertHasMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.Validation.ValidateTIRNumber();
			header.TIRNumber = "";

			AssertNoMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestLabelsDateAtCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals("Departure Date", header.DateCustomsOfficeLabel.Caption);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertEquals("Arrival Date", header.DateCustomsOfficeLabel.Caption);
		}

		[TestDate(2020, 01, 19)]
		public void TestDateAtCustomsOfficeForTR()
		{
			var header = Customs.ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
			CombineAssertions("SEA Stamp Duty Value", () =>
			{
				AssertEquals(ZDateTime.Empty, header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				AssertEquals(ZDateTime.Empty, header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				AssertEquals(ZDateTime.Empty, header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				AssertEquals(ZDateTime.Empty, header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				AssertEquals(ZDateTime.Empty, header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				AssertEquals(new ZDateTime(2020, 01, 19), header.AMA_DateAtCustomsOffice);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				AssertEquals(new ZDateTime(2020, 01, 19), header.AMA_DateAtCustomsOffice);
			});
		}

		public void TestClearCustomsPorts()
		{
			var header = Customs.ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CustomsLoadPort = "AAAA";
			header.AMA_CustomsDischargePort = "BBBB";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AMA_CustomsLoadPort should be empty.", ZString.Empty, header.AMA_CustomsLoadPort);
			AssertEquals("AMA_CustomsDischargePort should be empty.", ZString.Empty, header.AMA_CustomsDischargePort);
		}

		IDisposable func;
		protected override void SetUp()
		{
			base.SetUp();
			func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TRFOZBY, Core.Constants.CountryCodes.Turkey, ZDateTime.Now, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			func.Dispose();
		}

		public void TestPackedItemUNDGsRelationship()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Assert(header1.SupportUNDGsOnPackedItemLevel);
		}

		public void TestSupportMultipleCustomsNumbers()
		{
			var header1 = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header1.SupportMultipleCustomsNumbers);
		}

		public void TestCalculateStampDuties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			var startDate = ZDateTime.Now.AddDays(-1);
			var endDate = ZDateTime.Now.AddDays(1);
			helper.CreateTaxOrFee("ABS", 0.80000000m, "TR", startDate, endDate, "TR Air Bill Stamp Duty");
			helper.CreateTaxOrFee("GMS", 14.60000000m, "TR", startDate, endDate, "TR Global Manifest Stamp Duty");
			helper.CreateTaxOrFee("OBS", 0.80000000m, "TR", startDate, endDate, "TR Ordino Stamp Duty");
			helper.CreateTaxOrFee("SBS", 19.70000000m, "TR", startDate, endDate, "TR Sea Bill Stamp Duty");

			var seaManifest = Factory.New<AsycudaManifestHeader>();
			seaManifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var seaRegularBill = seaManifest.Bills.AddNew();
			var emptyContainer = seaManifest.Containers.AddNew();
			emptyContainer.ACN_EmptyFullIndicator = ASYCUDA.Business.EmptyFullIndicatorList.Codes.EmptyContainer;
			var seaRegularBillWithEmptyContainer = seaManifest.Bills.AddNew();
			seaRegularBillWithEmptyContainer.Packs.AddNew().ContainerPK = emptyContainer.PK;

			seaManifest.CalculateStampDuties();
			CombineAssertions("SEA Stamp Duty Value", () =>
			{
				AssertEquals("Sea, GMS", 14.60000000m, seaManifest.GlobalManifestStampDutyValue);
				AssertEquals("Sea, SBS", 19.70000000m, seaManifest.MasterBillStampDutyValue);
				AssertEquals("Sea, OBS, Regular bill", 0.80000000m, seaRegularBill.BillStampDutyValue);
				AssertEquals("Sea, ABS, Regular bill", 0m, seaRegularBill.AirBillStampDutyABSValue);
				AssertEquals("Sea, OBS, Regular bill with empty container", 0m, seaRegularBillWithEmptyContainer.BillStampDutyValue);
				AssertEquals("Sea, ABS, Regular bill with empty container", 0m, seaRegularBillWithEmptyContainer.AirBillStampDutyABSValue);
				AssertEquals("Sea, Total stamp duty", 35.10000000m, seaManifest.TotalStampDutyValue);
			});

			var airManifest = Factory.New<AsycudaManifestHeader>();
			airManifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var airRegularBill = airManifest.Bills.AddNew();
			var airRegularBillWithEmptyContainer = airManifest.Bills.AddNew();
			airRegularBillWithEmptyContainer.Packs.AddNew().ContainerPK = emptyContainer.PK;

			airManifest.CalculateStampDuties();
			CombineAssertions("AIR Stamp Duty Value", () =>
			{
				AssertEquals("Air, GMS", 14.60000000m, airManifest.GlobalManifestStampDutyValue);
				AssertEquals("Air, ABS", 0.80000000m, airManifest.MasterBillStampDutyValue);
				AssertEquals("Air, OBS, Regular bill", 0.80000000m, airRegularBill.BillStampDutyValue);
				AssertEquals("Air, ABS, Regular bill", 0.80000000m, airRegularBill.AirBillStampDutyABSValue);
				AssertEquals("Air, OBS, Regular bill with empty container", 0.80000000m, airRegularBillWithEmptyContainer.BillStampDutyValue);
				AssertEquals("Air, ABS, Regular bill with empty container", 0.80000000m, airRegularBillWithEmptyContainer.AirBillStampDutyABSValue);
				AssertEquals("Air, Total stamp duty", 18.60000000m, airManifest.TotalStampDutyValue);
			});
		}

		public void TestDutyFields()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDate.Today;
			header.AMA_TransportMode = "ROA";
			header.GlobalManifestStampDutyValue = 99;
			header.MasterBillStampDutyValue = 88;

			CombineAssertions("DutyFields", () =>
			{
				AssertEquals("ROA | GlobalManifestStampDutyValue should be 99", 99m, header.GlobalManifestStampDutyValue);
				AssertEquals("ROA | MasterBillStampDutyValue should be 0", 0m, header.MasterBillStampDutyValue);
				AssertEquals("ROA | TotalStampDutyValue should be 99", 99m, header.TotalStampDutyValue);

				header.AMA_TransportMode = "AIR";
				header.GlobalManifestStampDutyValue = 77;
				header.MasterBillStampDutyValue = 66;
				AssertEquals("AIR | GlobalManifestStampDutyValue should be 77", 77m, header.GlobalManifestStampDutyValue);
				AssertEquals("AIR | MasterBillStampDutyValue should be 66", 66m, header.MasterBillStampDutyValue);
				AssertEquals("AIR | TotalStampDutyValue should be 143", 143m, header.TotalStampDutyValue);

				header.AMA_TransportMode = "SEA";
				header.GlobalManifestStampDutyValue = 55;
				header.MasterBillStampDutyValue = 44;
				AssertEquals("SEA | GlobalManifestStampDutyValue should be 55", 55m, header.GlobalManifestStampDutyValue);
				AssertEquals("SEA | MasterBillStampDutyValue should be 44", 44m, header.MasterBillStampDutyValue);
				AssertEquals("SEA | TotalStampDutyValue should be 99", 99m, header.TotalStampDutyValue);
			});
		}

		public void TestAMA_VesselNameLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should equals the max length of AMA_VesselName.", 35, header.AMA_VesselNameInfo.MaxLength);
		}

		public void TestAMA_ManifestDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestDescription = "Test desc message";

			CombineAssertions(() =>
			{
				AssertEquals("Test desc message", header.AMA_ManifestDescription);
				AssertEquals("AMA_ManifestDescription: Caption", "Manifest Description", header.AMA_ManifestDescriptionInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			});
		}

		public void TestAMA_InspectionClerk()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_InspectionClerk = "Test InspectionClerk message";

			CombineAssertions(() =>
			{
				AssertEquals("Test InspectionClerk message", header.AMA_InspectionClerk);
				AssertEquals("AMA_InspectionClerk: Caption", "Inspection Clerk", header.AMA_InspectionClerkInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("AMA_InspectionClerk: ReadOnly", true, header.AMA_InspectionClerkInfo.GetAttribute<ReadOnlyAttribute>().IsReadOnly);
			});
		}

		public void TestChangeTransportModeForMasterBillStampDutyValue()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.MasterBillStampDutyValue = 100;
			var tax = manifest.MasterBill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.SBS);

			CombineAssertions("MasterBillStampDutyValue When TransportTypeChange", () =>
			{
				AssertEquals("Sea, MasterBillStampDutyValue should be 100", 100m, manifest.MasterBillStampDutyValue);
				AssertEquals("If AMA_TransportMode is SEA should return SBS", tax.AET_ChargeType, TaxCodeList.Codes.SBS);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Air, MasterBillStampDutyValue should be 100", 100m, manifest.MasterBillStampDutyValue);
				tax = manifest.MasterBill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);
				AssertEquals("If AMA_TransportMode is AIR should return ABS", tax.AET_ChargeType, TaxCodeList.Codes.ABS);
			});
		}

		public void TestAMA_CustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				header.TR_GM_PresentationCustomsOffice = "TR040001";
				AssertEquals("If AMA_CustomOffice value is empty, the value entered in the TR_GM_PresentationCustomsOffice should be assginden automaticly in the AMA_CustomOffice", header.AMA_CustomsOffice, header.TR_GM_PresentationCustomsOffice);

				header.AMA_CustomsOffice = "TR040002";
				header.TR_GM_PresentationCustomsOffice = "TR040005";
				AssertNotEquals(header.AMA_CustomsOffice, header.TR_GM_PresentationCustomsOffice);

				header.AMA_CustomsOffice = ZString.Empty;
				header.TR_GM_PresentationCustomsOffice = "TR040003";
				AssertEquals(header.AMA_CustomsOffice, header.TR_GM_PresentationCustomsOffice);
			});
		}

		public void TestChangingAmendManifestState()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.RegistrationNumber = "22340300IM12345678";
			manifestHeader.RegistrationDate = new ZDateTime(2022, 1, 1);
			var entryNum = CusEntryNumber.Load(manifestHeader, "ASY", manifestHeader.AMA_RN_NKCountry);
			CombineAssertions("Registered Manifest", () =>
			{
				AssertEquals("AMA_JobReference", "C123456", manifestHeader.AMA_JobReference);
				AssertEquals("AMA_MessageStatus", ZString.Empty, manifestHeader.AMA_MessageStatus);
				AssertEquals("CE_EntryNum", "22340300IM12345678", entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2022, 1, 1), entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", ZString.Empty, entryNum.CE_ExpiryDate.ToString());
				AssertEquals("CE_EntryLineReference", ZString.Empty, entryNum.CE_EntryLineReference);
			});

			manifestHeader.ChangeToAmmendManifest();
			entryNum = CusEntryNumber.Load(manifestHeader, "ASY", manifestHeader.AMA_RN_NKCountry);
			CombineAssertions("ChangingAmendManifestState", () =>
			{
				AssertEquals("AMA_JobReference", "C123456-1", manifestHeader.AMA_JobReference);
				AssertEquals("AMA_MessageStatus", ZString.Empty, manifestHeader.AMA_MessageStatus);
				AssertEquals("CE_EntryNum", ZString.Empty, entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", ZDateTime.Now.Date, entryNum.CE_ExpiryDate.Date);
				AssertEquals("CE_EntryLineReference", "22340300IM12345678", entryNum.CE_EntryLineReference);
			});

			manifestHeader.RegistrationNumber = "22340300IM12345679";
			manifestHeader.RegistrationDate = new ZDateTime(2022, 2, 2);
			entryNum = CusEntryNumber.Load(manifestHeader, "ASY", manifestHeader.AMA_RN_NKCountry);
			CombineAssertions("New Registered Manifest", () =>
			{
				AssertEquals("AMA_JobReference", "C123456-1", manifestHeader.AMA_JobReference);
				AssertEquals("AMA_MessageStatus", ZString.Empty, manifestHeader.AMA_MessageStatus);
				AssertEquals("CE_EntryNum", "22340300IM12345679", entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2022, 2, 2), entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", ZDateTime.Now.Date, entryNum.CE_ExpiryDate.Date);
				AssertEquals("CE_EntryLineReference", "22340300IM12345678", entryNum.CE_EntryLineReference);
			});

			manifestHeader.ChangeToAmmendManifest();
			entryNum = CusEntryNumber.Load(manifestHeader, "ASY", manifestHeader.AMA_RN_NKCountry);
			CombineAssertions("ChangingAmendManifestState", () =>
			{
				AssertEquals("AMA_JobReference", "C123456-2", manifestHeader.AMA_JobReference);
				AssertEquals("AMA_MessageStatus", ZString.Empty, manifestHeader.AMA_MessageStatus);
				AssertEquals("CE_EntryNum", ZString.Empty, entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", ZDateTime.Now.Date, entryNum.CE_ExpiryDate.Date);
				AssertEquals("CE_EntryLineReference", "22340300IM12345679", entryNum.CE_EntryLineReference);
			});
		}

		public void TestHumanReadableNameCore()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CombineAssertions("HumanReadableNameCore", () =>
			{
				manifestHeader.RegistrationNumber = "22340300IM12345678";
				AssertEquals("Real Record", "TR Manifest C123456", manifestHeader.HumanReadableName);
				manifestHeader.ChangeToAmmendManifest();
				AssertEquals("First Ammend", "TR Manifest C123456-1", manifestHeader.HumanReadableName);
				manifestHeader.RegistrationNumber = "22340300IM12345679";
				manifestHeader.ChangeToAmmendManifest();
				AssertEquals("Second Ammend", "TR Manifest C123456-2", manifestHeader.HumanReadableName);
			});
		}

		public void TestFindOrCreateCusStatement()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDate.Today;
			header.AMA_TransportMode = "AIR";
			header.AMA_JobReference = "MAN0006789";
			header.RegistrationNumber = "22340300IM12345678";
			header.RegistrationDate = new ZDateTime(2022, 1, 10);

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BIL00001";
			bill1.BillStampDutyValue = 30;
			bill1.AirBillStampDutyABSValue = 40;
			var tax1 = bill1.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.OBS);
			var tax2 = bill1.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BIL00002";
			bill2.BillStampDutyValue = 50;
			bill2.AirBillStampDutyABSValue = 60;
			var tax3 = bill2.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.OBS);
			var tax4 = bill2.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);

			((Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader)header).CreateManifestStatement();
			CombineAssertions("Cus Statement Creation", () =>
			{
				var dueDate = new DateTime(header.RegistrationDate.Year, header.RegistrationDate.Month, 20).AddMonths(1);
				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.GlobalManifest);
				query.AddToFilter(CusStatementHeaderSchema.B2_DueDate, dueDate);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
				var cusStatementHeader = Factory.Load<CusStatementHeader>(query).FirstOrDefault();
				AssertEquals("B2_IsMonthlyStatement", ZBool.True, cusStatementHeader.B2_IsMonthlyStatement);
				AssertEquals("B2_Status", "PRE", cusStatementHeader.B2_Status);
				AssertEquals("B2_StatementType", "M", cusStatementHeader.B2_StatementType);
				AssertEquals("B2_DueDate", new ZDateTime(2022, 2, 20), cusStatementHeader.B2_DueDate);
				AssertEquals("B2_PaymentParty", "BRK", cusStatementHeader.B2_PaymentParty);
				AssertEquals("B2_PeriodStartDate", new ZDateTime(2022, 1, 1), cusStatementHeader.B2_PeriodStartDate);
				AssertEquals("B2_PeriodEndDate", new ZDateTime(2022, 1, 31), cusStatementHeader.B2_PeriodEndDate);

				var statementLine1 = cusStatementHeader.StatementLines.Cast<CusStatementLine>().FirstOrDefault(x => x.B3_BrokerReference == header.AMA_JobReference && x.B3_AssociatedEntry == bill1.ABL_BillNumber);
				AssertEquals("B3_EntryType", "MAN", statementLine1.B3_EntryType);
				AssertEquals("B3_BrokerReference", "MAN0006789", statementLine1.B3_BrokerReference);
				AssertEquals("B3_AssociatedEntry", "BIL00001", statementLine1.B3_AssociatedEntry);
				AssertEquals("B3_EntryNum", "22340300IM12345678", statementLine1.B3_EntryNum);
				AssertEquals("B3_EntryDate", new ZDate(2022, 1, 10), statementLine1.B3_EntryDate);

				var charge1 = statementLine1.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == tax1.AET_ChargeType);
				AssertEquals("B4_ChargeType", tax1.AET_ChargeType, charge1.B4_ChargeType);
				AssertEquals("B4_ChargeAmount", 30m, charge1.B4_ChargeAmount);

				var charge2 = statementLine1.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == tax2.AET_ChargeType);
				AssertEquals("B4_ChargeType", tax2.AET_ChargeType, charge2.B4_ChargeType);
				AssertEquals("B4_ChargeAmount", 40m, charge2.B4_ChargeAmount);

				var statementLine2 = cusStatementHeader.StatementLines.Cast<CusStatementLine>().FirstOrDefault(x => x.B3_BrokerReference == header.AMA_JobReference && x.B3_AssociatedEntry == bill2.ABL_BillNumber);
				AssertEquals("B3_EntryType", "MAN", statementLine2.B3_EntryType);
				AssertEquals("B3_BrokerReference", "MAN0006789", statementLine2.B3_BrokerReference);
				AssertEquals("B3_AssociatedEntry", "BIL00002", statementLine2.B3_AssociatedEntry);
				AssertEquals("B3_EntryNum", "22340300IM12345678", statementLine2.B3_EntryNum);
				AssertEquals("B3_EntryDate", new ZDate(2022, 1, 10), statementLine2.B3_EntryDate);

				var charge3 = statementLine2.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == tax3.AET_ChargeType);
				AssertEquals("B4_ChargeType", tax3.AET_ChargeType, charge3.B4_ChargeType);
				AssertEquals("B4_ChargeAmount", 50m, charge3.B4_ChargeAmount);

				var charge4 = statementLine2.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == tax4.AET_ChargeType);
				AssertEquals("B4_ChargeType", tax4.AET_ChargeType, charge4.B4_ChargeType);
				AssertEquals("B4_ChargeAmount", 60m, charge4.B4_ChargeAmount);
			});
		}

		public void TestIsItineraryTabePageVisible()
		{
			CombineAssertions(() =>
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", manifest.IsItineraryTabePageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", manifest.IsItineraryTabePageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !manifest.IsItineraryTabePageVisible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", manifest.IsItineraryTabePageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", manifest.IsItineraryTabePageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !manifest.IsItineraryTabePageVisible);

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !manifest.IsItineraryTabePageVisible);
			});
		}

		public void TestIsManifestToOpenPageVisible()
		{
			CombineAssertions(() =>
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !manifest.IsManifestToOpenPageVisible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", manifest.IsManifestToOpenPageVisible);

				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !manifest.IsManifestToOpenPageVisible);
			});
		}

		public void TestReadOnlyMemberFieldsForSeaAndGrupaj()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CombineAssertions(() =>
			{
				AssertEquals("IsReadOnlyForSeaAndGrupaj should be true", true, manifest.IsOnlyForSeaAndGrupaj);
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.TransportType), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.AMA_VesselName), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.AMA_RN_NKConveyanceNationality), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.AMA_RL_NKPortOfLoading), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.AMA_CustomsLoadPort), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.AMA_RL_NKPortOfFirstArrival), true, attr => attr.Member == nameof(AsycudaManifestHeader.IsOnlyForSeaAndGrupaj));

				manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				AssertEquals("IsReadOnlyForSeaAndGrupaj should be false", false, manifest.IsOnlyForSeaAndGrupaj);
			});
		}

		public void TestClearPropertiesValueForSeaAndGrupaj()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CombineAssertions(() =>
			{
				AssertEquals("TransportType: default value should be empty", ZString.Empty, manifest.TransportType);
				AssertEquals("AMA_VesselName: default value should be empty", ZString.Empty, manifest.AMA_VesselName);
				AssertEquals("AMA_RN_NKConveyanceNationality: default value should be empty", ZString.Empty, manifest.AMA_RN_NKConveyanceNationality);
				AssertEquals("AMA_RL_NKPortOfLoading: default value should be empty", ZString.Empty, manifest.AMA_RL_NKPortOfLoading);
				AssertEquals("AMA_CustomsLoadPort: default value should be empty", ZString.Empty, manifest.AMA_CustomsLoadPort);
				AssertEquals("AMA_RL_NKPortOfFirstArrival: default value should be empty", ZString.Empty, manifest.AMA_RL_NKPortOfFirstArrival);
			});
		}
	}
}
