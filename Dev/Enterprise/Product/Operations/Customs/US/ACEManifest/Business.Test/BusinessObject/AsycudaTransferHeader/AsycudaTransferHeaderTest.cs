using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransferHeader))]
	class AsycudaTransferHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			AssertEquals("ATF_TransferType Defaults to Empty requiring the Transfer Type to be set", ZString.Empty, transferHeader.ATF_TransferType);
			AssertEquals("Defaults to Header PortOfDischarge", "USLAX", transferHeader.ATF_RL_NKDestinationPortCode);
		}

		public void TestReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			Assert(!transferHeader.ReadOnly);
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_MessageStatus = "";
			Assert(!transferHeader.ReadOnly);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferSent;
			Assert(transferHeader.ReadOnly);
			transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
			Assert(!transferHeader.ReadOnly);
		}

		public void TestCreateNewAsycudaTransferBillCollection()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>), transferHeader.TransferBills.GetType());
			AssertEquals(typeof(ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>), ((ASYCUDA.Business.AsycudaTransferHeader)transferHeader).TransferBills.GetType());
		}

		public void TestGetNewLookups()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(AsycudaTransferHeaderLookups), transferHeader.Lookups.GetType());
			AssertEquals(typeof(AsycudaTransferHeaderLookups), ((ASYCUDA.Business.AsycudaTransferHeader)transferHeader).Lookups.GetType());
		}

		public void TestGetNewValidation()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			AssertEquals(typeof(AsycudaTransferHeaderValidation), transferHeader.Validation.GetType());
			AssertEquals(typeof(AsycudaTransferHeaderValidation), ((ASYCUDA.Business.AsycudaTransferHeader)transferHeader).Validation.GetType());
		}

		public void TestArrivalHeader()
		{
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			AssertEquals(typeof(AsycudaTransferHeader), transferHeader.GetType());
			AssertEquals(typeof(AsycudaArrivalHeader), transferHeader.ArrivalHeader.GetType());
			AssertEquals(typeof(AsycudaArrivalHeader), ((ASYCUDA.Business.AsycudaTransferHeader)transferHeader).ArrivalHeader.GetType());
		}

		public void TestATF_RL_NKDestinationPortCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			AssertEquals("Defaults to Header PortOfDischarge", "USLAX", transferHeader.ATF_RL_NKDestinationPortCode);
			transferHeader.ATF_RL_NKDestinationPortCode = "USCHI";
			AssertEquals("Can store alternate port code", "USCHI", transferHeader.ATF_RL_NKDestinationPortCode);
		}

		public void TestATF_CarrierID()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
				var address2 = carrier.Addresses.AddNew();
				address2.OA_Address1 = "Park Lane";
				address2.OA_RL_NKRelatedPortCode = "USSFO";
				address2.OA_City = "San Francisco";
				address2.OA_State = "CA";
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RL_NKPortOfDischarge = "USLAX";
				var arrivalHeader = header.ArrivalHeaders.AddNew();
				var transferHeader1 = arrivalHeader.TransferHeaders.AddNew();
				transferHeader1.InBondCarrierOrgPK = carrier.PK;
				transferHeader1.ATF_OA_Carrier = address2.PK;
				AssertEquals("Should not be defaulted because EIN format is wrong", ZString.Empty, transferHeader1.ATF_CarrierID);
				AssertEquals("Carrier code", "ABCD", transferHeader1.ATF_OnwardCarrier);
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "OKD2", Core.Constants.CountryCodes.UnitedStates);
				var code = carrier.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
				code.OK_CustomsRegNo = "13-150279800";
				var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
				transferHeader2.InBondCarrierOrgPK = carrier.PK;
				transferHeader2.ATF_OA_Carrier = address2.PK;
				AssertEquals("Should be defaulted, because EIN is correct", "13-150279800", transferHeader2.ATF_CarrierID);
				AssertEquals("Carrier code", "OKD2", transferHeader2.ATF_OnwardCarrier);
				transferHeader2.ATF_CarrierID = "12-123456789";
				AssertEquals("Set ATF_CarrierID and ATF_OA_Carrier_ZAddress should be cleared", true, transferHeader2.ATF_OA_Carrier_ZAddress.OrgPK.IsEmpty);
				AssertEquals("Set ATF_CarrierID and ATF_OA_Carrier should be cleared", true, transferHeader2.ATF_OA_Carrier.IsEmpty);
				AssertEquals("Set ATF_CarrierID persisted", "12-123456789", transferHeader2.ATF_CarrierID);
			}
		}

		public void TestATF_DestinationWarehouseID()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var airline = Factory.NewWithValidTestData<RefAirline>();
				airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "ABC";
				airline.RM_ThreeLetterCode = "AAA";
				airline.RM_TwoCharacterCode = "";
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var address1 = carrier.Addresses.AddNew();
				address1.OA_Address1 = "Park Lane";
				address1.OA_RL_NKRelatedPortCode = "USSFO";
				address1.OA_City = "San Francisco";
				address1.OA_State = "CA";
				var address2 = carrier.Addresses.AddNew();
				address2.OA_Address1 = "Main Street";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address2.OA_City = "Los Angeles";
				address2.OA_State = "CA";
				address1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "A-01", Core.Constants.CountryCodes.UnitedStates);
				address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "1234", Core.Constants.CountryCodes.UnitedStates);
				carrier.MiscServ.OM_RM_Airline = ZGuid.Empty;
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RL_NKPortOfDischarge = "USLAX";
				var arrivalHeader = header.ArrivalHeaders.AddNew();
				var transferHeader = arrivalHeader.TransferHeaders.AddNew();
				transferHeader.DestinationWarehouseOrgPK = carrier.PK;
				transferHeader.ATF_OA_DestinationWarehouse = address1.PK;
				AssertEquals("Invalid Firms code and no airline codes", ZString.Empty, transferHeader.ATF_DestinationWarehouseID);
				transferHeader.ATF_OA_DestinationWarehouse = address2.PK;
				AssertEquals("Valid Firms code ignore airline codes", "1234", transferHeader.ATF_DestinationWarehouseID);
				carrier.MiscServ.OM_RM_Airline = airline.PK;
				transferHeader.ATF_OA_DestinationWarehouse = ZGuid.Empty;
				transferHeader.ATF_OA_DestinationWarehouse = address1.PK;
				AssertEquals("Invalid Firms code gets 3 char airline code", "AAA", transferHeader.ATF_DestinationWarehouseID);
				airline.RM_TwoCharacterCode = "AA";
				transferHeader.ATF_OA_DestinationWarehouse = ZGuid.Empty;
				transferHeader.ATF_OA_DestinationWarehouse = address1.PK;
				AssertEquals("Invalid Firms code gets 2 char airline code", "AA", transferHeader.ATF_DestinationWarehouseID);
				transferHeader.ATF_OA_DestinationWarehouse = address2.PK;
				AssertEquals("Valid Firms code ignore airline codes", "1234", transferHeader.ATF_DestinationWarehouseID);
				AssertEquals("ATF_OA_DestinationWarehouse has been set", false, transferHeader.ATF_OA_DestinationWarehouse.IsEmpty);
				transferHeader.ATF_DestinationWarehouseID = "XYZ";
				AssertEquals("Set ATF_DestinationWarehouseID and ATF_OA_DestinationWarehouse_ZAddress should be cleared", true, transferHeader.ATF_OA_DestinationWarehouse_ZAddress.OrgPK.IsEmpty);
				AssertEquals("Set ATF_DestinationWarehouseID and ATF_OA_DestinationWarehouse should be cleared", true, transferHeader.ATF_OA_DestinationWarehouse.IsEmpty);
				AssertEquals("Set ATF_CarrierID persisted", "XYZ", transferHeader.ATF_DestinationWarehouseID);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_RL_NKPortOfDischarge = "";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			return transferHeader;
		}
	}
}
