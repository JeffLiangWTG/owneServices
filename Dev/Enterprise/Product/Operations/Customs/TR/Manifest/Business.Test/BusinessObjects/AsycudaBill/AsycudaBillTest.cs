using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	public partial class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestBillStampDutyValueCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			CombineAssertions("Stamp Duty Caption", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Stamp Duty", bill.BillStampDutyValueCaption.Caption);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Stamp Duty (OBS)", bill.BillStampDutyValueCaption.Caption);
			});
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var roro = bill.RoRo;
			var transshipmentType = bill.TransshipmentType;
			var billStampDutyValue = bill.BillStampDutyValue;
			CombineAssertions("Bill Level Set Default Values", () =>
			{
				AssertEquals("TransshipmentType", ZString.Empty, transshipmentType);
				AssertEquals("RoRo", false, roro);
				AssertEquals("BillStampDutyValue", ZDecimal.Zero, billStampDutyValue);
				AssertEquals("ABL_BolType default value should be STD", Core.Constants.ShipmentTypes.StandardHouse, bill.ABL_BolType);
				AssertEquals("ABL_SpecialCargoCode default value should be No", Universal.CodeDescriptionPairLists.YesNoList.Codes.No, bill.ABL_SpecialCargoCode);
			});
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
			var bill2 = header.Bills.AddNew();
			AssertEquals("ABL_LocationInformation default value should be GEMİ", "GEMİ", bill2.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			var bill3 = header.Bills.AddNew();
			AssertEquals("ABL_LocationInformation default value should be LİMAN", "LİMAN", bill3.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
			var bill4 = header.Bills.AddNew();
			AssertEquals("ABL_LocationInformation default value should be LİMAN", "LİMAN", bill4.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.DIGIHR;
			var bill5 = header.Bills.AddNew();
			AssertEquals(ZString.Empty, bill5.ABL_LocationInformation);
		}

		public void TestCustomsEntryNumberClearAfterTypeChanged()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "123";
			bill.CustomsEntryNumberType = "ABC";
			bill.CustomsEntryNumberType = "PRV";
			AssertEquals("123", bill.CustomsEntryNumber);
			bill.CustomsEntryNumberType = "ABC";
			bill.CustomsEntryNumber = "012345678901234567890123";
			bill.CustomsEntryNumberType = "PRV";
			AssertEquals(ZString.Empty, bill.CustomsEntryNumber);
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Turkey, bill.GetCountryCode());
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackTypeCore());
		}

		public void TestGetCustomsEntryNumberMaxLengthConfig()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CustomsEntryNumberTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "PRV", "Previous Declaration", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumberType = CusEntryNumberTypes.Turkey.PRV;
			AssertEquals(CusEntryNumberTypes.Turkey.PRV, bill.CustomsEntryNumberType);
			AssertEquals(20, bill.CustomsEntryNumberMaxLength);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			ICusCodeDataTypeSupporter supporter = bill;
			supporter.AssertType(typeof(VisitedPort), CusCodeDataTypeList.Codes.TRVisitedPort);
			var visitedPort = bill.VisitedPorts.AddNew();
			visitedPort.CY_Data = "TRMER-001";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(visitedPort.PK);
			AssertEquals(typeof(VisitedPort), codeData.GetType());
		}

		public void TestRegNoReadOnly()
		{
			var factory = new BusinessObjectFactory();
			var orgWithVAT = factory.New<OrgHeader>();
			orgWithVAT.OH_Code = "orgVATCode";
			orgWithVAT.OH_FullName = "orgVATName";
			var addressWithVAT = orgWithVAT.MainAddress;
			addressWithVAT.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456");
			var orgWithoutVAT = factory.New<OrgHeader>();
			orgWithoutVAT.OH_Code = "orgNoVATCode";
			orgWithoutVAT.OH_FullName = "orgNoVATName";
			var addressWithoutVAT = orgWithoutVAT.MainAddress;
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var bill = Factory.New<AsycudaBillForTest>();
			header.Bills.Add(bill);
			bill.ABL_OA_Shipper = ZGuid.Empty;
			factory.Save();
			AssertEquals(false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_OA_Shipper = addressWithVAT.PK;
			AssertEquals(false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_OA_Shipper = addressWithoutVAT.PK;
			AssertEquals(false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_OA_Consignee = addressWithVAT.PK;
			AssertEquals(true, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_ConsigneeRegNo.IsEmpty);
			bill.ABL_OA_Consignee = addressWithoutVAT.PK;
			AssertEquals(false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.ABL_OA_NotifyParty = addressWithVAT.PK;
			AssertEquals(true, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_NotifyPartyRegNo.IsEmpty);
			bill.ABL_OA_NotifyParty = addressWithoutVAT.PK;
			AssertEquals(false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.Header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals(false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_OA_Shipper = addressWithVAT.PK;
			AssertEquals(true, bill.ABL_ShipperRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_ShipperRegNo.IsEmpty);
			AssertEquals(false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_OA_Consignee = addressWithVAT.PK;
			AssertEquals(false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			AssertEquals(false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.ABL_OA_NotifyParty = addressWithVAT.PK;
			AssertEquals(false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
		}

		public void TestIsToOrder()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			Assert(!bill.IsToOrder);
			bill.IsToOrder = ZBool.True;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaBill>(bill.PK).IsToOrder);
		}

		public void TestNotOwned()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			Assert(!bill.NotOwned);
			bill.NotOwned = ZBool.True;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaBill>(bill.PK).NotOwned);
		}

		public void TestABLGoodsLocation()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR1", "Warehouse1", yesterday, tomorrow);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsLocation = "WAR1";
			AssertEquals("WAR1", bill.ABL_GoodsLocation);
			var info = bill.ABL_GoodsLocationInfo;
			AssertEquals(9, info.MaxLength);
		}

		public void TestBillStampDutyValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDate.Today;
			var bill = header.Bills.AddNew();
			CombineAssertions("Bill Stamp Duty Value", () =>
			{
				header.AMA_TransportMode = "AIR";
				bill.BillStampDutyValue = 20;
				AssertEquals("BillStampDutyValue should be 20", 20m, bill.BillStampDutyValue);
				var tax = bill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);
				AssertNull("If AMA_TransportMode is AIR should return null value", tax);
				header.AMA_TransportMode = "SEA";
				bill.BillStampDutyValue = 30;
				AssertEquals("BillStampDutyValue should be 30", 30m, bill.BillStampDutyValue);
				tax = bill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.OBS);
				AssertEquals("AET_ChargeAmount should be 30", tax.AET_ChargeAmount, 30m);
				AssertEquals("AET_ChargeType should be OBS", tax.AET_ChargeType, TaxCodeList.Codes.OBS);
				AssertEquals("There is only one tax record", 1, bill.AsycudaTaxes.Count);
				header.AMA_TransportMode = "ROA";
				Factory.Save();
				var newFactory = NewFactory();
				var reLoadBill = newFactory.Load<AsycudaBill>(bill.PK);
				tax = reLoadBill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS || x.AET_ChargeType == TaxCodeList.Codes.OBS);
				AssertNull("If AMA_TransportMode is not AIR or SEA should return null value", tax);
				header.AMA_TransportMode = "AIR";
				bill.BillStampDutyValue = 20;
				tax = bill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);
				AssertNull("If AMA_TransportMode is AIR should return null value", tax);
				bill.BillStampDutyValue = ZDecimal.Zero;
				Factory.Save();
				newFactory = NewFactory();
				reLoadBill = newFactory.Load<AsycudaBill>(bill.PK);
				tax = reLoadBill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS || x.AET_ChargeType == TaxCodeList.Codes.OBS);
				AssertNull("If AMA_TransportMode is not AIR or SEA should return null value", tax);
			});
		}

		public void TestAirBillStampDutyABSValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDate.Today;
			var bill = header.Bills.AddNew();
			CombineAssertions("Air Bill Stamp Duty (ABS) Value", () =>
			{
				header.AMA_TransportMode = "AIR";
				bill.AirBillStampDutyABSValue = 20;
				AssertEquals("AirBillStampDutyABSValue should be 20", 20m, bill.AirBillStampDutyABSValue);
				var tax = bill.AsycudaTaxes.FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.ABS);
				AssertEquals("AET_ChargeAmount should be 20", tax.AET_ChargeAmount, 20m);
				AssertEquals("AET_ChargeType should be ABS", tax.AET_ChargeType, TaxCodeList.Codes.ABS);
			});
		}

		public void TestABL_ConsigneeRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDate.Today;
			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeRegNo = "Test Vat No";
			bill.IsToOrder = ZBool.True;
			AssertEquals("EMRE", bill.ABL_ConsigneeRegNo);
			AssertEquals(ZBool.False, bill.NotOwned);
			AssertEquals(ZBool.True, bill.ConsigneeRegNoReadOnly);
			bill.ABL_ConsigneeRegNo = "Test Vat No";
			bill.NotOwned = ZBool.True;
			AssertEquals("SAHIP DEGIL", bill.ABL_ConsigneeRegNo);
			AssertEquals(ZBool.False, bill.IsToOrder);
			AssertEquals(ZBool.True, bill.ConsigneeRegNoReadOnly);
			bill.IsToOrder = ZBool.False;
			bill.NotOwned = ZBool.False;
			bill.ABL_ConsigneeRegNo = "Test Vat No";
			AssertEquals("Test Vat No", bill.ABL_ConsigneeRegNo);
			AssertEquals(ZBool.False, bill.ConsigneeRegNoReadOnly);
		}

		public void TestABL_LocationInformation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_LocationInformation = ZString.Empty;
			CombineAssertions(() =>
			{
				bill.ABL_GoodsLocation = "WAR1";
				AssertEquals("WAR1", bill.ABL_GoodsLocation);
				AssertEquals(ZString.Empty, bill.ABL_LocationInformation);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				bill.ABL_GoodsLocation = "WAR2";
				AssertEquals("WAR2", bill.ABL_LocationInformation);
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				bill.ABL_GoodsLocation = "WAR3";
				AssertEquals("WAR3", bill.ABL_LocationInformation);
				header.AMA_ManifestType = TRManifestTypes.Codes.DIGIHR;
				bill.ABL_LocationInformation = ZString.Empty;
				bill.ABL_GoodsLocation = "WAR4";
				AssertEquals(ZString.Empty, bill.ABL_LocationInformation);
			});
		}

		public void TestABL_SpecialCargoCodeMaxLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var info = bill.ABL_SpecialCargoCodeInfo;
			AssertEquals(1, info.MaxLength);
		}

		#region ICusSupportingInfoTypeSupporter
		public void TestGetCusSupportingInfoTypes()
		{
			var bill = Factory.New<AsycudaBill>();
			var actualTypes = ((ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes();
			AssertEquals(typeof(RelatedDeclarationForExport), actualTypes[RelatedDeclarationForExport.RelatedDeclarationForExportType]);
		}

		public void TestGetFetchStrategies()
		{
			var bill = Factory.New<AsycudaBill>();
			var expectedTypes = new[] { typeof(Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy), typeof(Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy) };
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)bill).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void PropertiesForDocWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("ContainerInformation is No", "H", bill.ContainerInformation);
				var container = header.Containers.AddNew();
				pack.ContainerPK = container.PK;
				AssertEquals("ContainerInformation is Yes", "E", bill.ContainerInformation);
				bill.TransshipmentType = "1";
				AssertEquals("IsTransshipment is NO", "H", bill.IsTransshipment);
				bill.TransshipmentType = "4";
				AssertEquals("IsTransshipment is YES", "E", bill.IsTransshipment);
			});
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
		}
		#endregion
	}
}
