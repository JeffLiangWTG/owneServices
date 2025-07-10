using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestHumanReadableShortcutName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN12345";
			AssertEquals("US Air AMS MAN12345", header.HumanReadableShortcutName);

			header.AMA_MasterBill = "HS08152402";
			AssertEquals("US Air AMS MAN12345 - HS08152402", header.HumanReadableShortcutName);
		}

		public void TestIsDeconsolidatorEnabledCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("IsDeconsolidatorEnabled should be true by default in US", header.IsDeconsolidatorEnabled);
		}

		public void TestIAsycudaManifestHeaderMembers()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Test1";
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			header.AMA_OA_DeconsolidateAddress = orgAddress1.PK;
			AssertEquals(string.Empty, ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "Test2";
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			header.AMA_OA_DeconsolidateAddress = orgAddress2.PK;
			AssertEquals("TEST123", ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);
			var orgAddress3 = orgHeader2.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Test Address 2";
			header.AMA_OA_DeconsolidateAddress = orgAddress3.PK;
			AssertEquals(string.Empty, ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);

			header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			Factory.Save();
			AssertEquals(string.Empty, ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgHeader1.PK;
			orgAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST001", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			AssertEquals("TEST001", ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader2.PK;
			orgHeader2.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST002", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			AssertEquals("TEST002", ((Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader)header).AirAMSOriginatorCode);
		}

		public void TestFIRMSCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Test1";
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			header.AMA_OA_DeconsolidateAddress = orgAddress1.PK;
			AssertEquals(string.Empty, header.FIRMSCode);

			var orgAddress2 = orgHeader1.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			header.AMA_OA_DeconsolidateAddress = orgAddress2.PK;
			AssertEquals("TEST123", header.FIRMSCode);

			var orgAddress3 = orgHeader1.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Test Address 3";
			header.AMA_OA_DeconsolidateAddress = orgAddress3.PK;
			AssertEquals(string.Empty, header.FIRMSCode);
		}

		public void TestDefaultAMA_OA_DeconsolidateAddressWhenOnlyOneAMOCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress);

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Test1";
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgHeader1.PK;
			AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress);

			var orgAddress2 = orgHeader1.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST001", Core.Constants.CountryCodes.UnitedStates);
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = ZGuid.Empty;
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgHeader1.PK;
			AssertEquals(orgAddress2.PK, header.AMA_OA_DeconsolidateAddress);

			var orgAddress3 = orgHeader1.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Test Address 3";
			orgAddress3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST002", Core.Constants.CountryCodes.UnitedStates);
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = ZGuid.Empty;
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgHeader1.PK;
			AssertEquals(ZGuid.Empty, header.AMA_OA_DeconsolidateAddress);
		}

		public void TestSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.SetParent(consol);

			AssertType<AsycudaManifestHeaderSynchroniser>(header.Synchroniser);
		}

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestIsExpressCourier()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.IsExpressCourier = true;
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			Assert(header.IsExpressCourier);

			var bill1 = header.Bills.AddNew();
			bill1.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Informal;
			bill1.CustomsEntryNumber = "11122233344";
			var bill2 = header.Bills.AddNew();
			bill2.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.PersonalShipment;
			bill2.CustomsEntryNumber = "22233344455";
			header.IsExpressCourier = true;
			AssertEquals(true, header.Bills.All(x => !x.CustomsEntryNumberType.IsEmpty && !x.CustomsEntryNumber.IsEmpty));
			header.IsExpressCourier = false;
			AssertEquals(true, header.Bills.All(x => x.CustomsEntryNumberType.IsEmpty && x.CustomsEntryNumber.IsEmpty));
		}

		public void TestAMA_TransportMode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Core.Constants.TransportModes.Air, header.AMA_TransportMode);
		}

		public void TestAMA_VoyageMaxLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Voyage = "KLM1234K";
			AssertEquals("KLM1234K", header.AMA_Voyage);
			AssertExceptionThrown<MaxLengthExceededException>(() => header.AMA_Voyage = "KLM12345K");
			ErrorReporter.Clear();
		}

		public void TestGetNewMessageChooser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			AssertType<MessageChooser>("Default message chooser type.", header.GetNewMessageChooser(items, string.Empty, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FRI, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FXI, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FRC, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FXC, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FRX, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FXX, false));
			AssertType<AIMMessageChooser>("Should return AIMMessageChooser for Air AMS Report Message.", header.GetNewMessageChooser(items, AIMMessageSubTypes.FSQ, false));
		}

		public void TestAMA_TransportMode_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(true, header.AMA_TransportModeInfo.ReadOnly);
		}

		public void TestIValidateForCustomsMessagingSupporter_GetEntityToValidate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(header, ((IValidateForCustomsMessagingSupporter)header).GetEntityToValidate(""));
		}

		public void TestAMA_JobReference()
		{
			TestConnection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.ManifestJobReference.SetNext(Factory, 6789);
				var header = Factory.New<AsycudaManifestHeader>();
				_ = header.Bills.AddNew();
				Factory.Save();
				AssertEquals("MAN0006789", header.AMA_JobReference);
			}
			finally
			{
				TestConnection.RollbackTransaction(); // Updating next number fountain value for the test
			}
		}

		public void TestClusterKeyUnitedStates()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header1.Bills.AddNew();
			var arrivalHeader = header1.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FH1";
			Factory.Save();
			AssertNotEquals("Cluster key is not 0", 0, header1.AMA_ClusterKey);
			AssertEquals(bill1.ABL_ClusterKey, header1.AMA_ClusterKey);
			AssertEquals(arrivalHeader.ATH_ClusterKey, header1.AMA_ClusterKey);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestCarrierCodeDefaulting()
		{
			CombineAssertions(() =>
			{
				AssertAirlineLetterCodeToCarrierCode("AA", "ABC", "AA");
				AssertAirlineLetterCodeToCarrierCode("A0", "ABC", "ABC");
				AssertAirlineLetterCodeToCarrierCode("A8", "ABC", "ABC");
				AssertAirlineLetterCodeToCarrierCode(ZString.Empty, "ABC", "ABC");
				AssertAirlineLetterCodeToCarrierCode(ZString.Empty, ZString.Empty, ZString.Empty);
			});
		}

		void AssertAirlineLetterCodeToCarrierCode(ZString twoCharacterCode, ZString threeLetterCode, ZString expectCarrierCode)
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			airline.RM_TwoCharacterCode = twoCharacterCode;
			airline.RM_ThreeLetterCode = threeLetterCode;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Carrier";
			carrier.OH_IsAirLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			var carrierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierOrgAddress.OA_OH = carrier.PK;
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertEquals(expectCarrierCode, header.AMA_CarrierCode);
		}

		public void TestDefaultVaules()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Test1";
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "Test2";
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "Test3";
			var orgAddress3 = orgHeader3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Test Address 3";
			orgAddress3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			var orgAddress4 = orgHeader3.Addresses.AddNew();
			orgAddress4.OA_Address1 = "Test Address 4";
			orgAddress4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST001", Core.Constants.CountryCodes.UnitedStates);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAA";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company.PK;
			branch1.GB_OH_OrgProxy = orgHeader1.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_GC = company.PK;
			branch2.GB_OH_OrgProxy = orgHeader2.PK;
			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "EEE";
			branch3.GB_GC = company.PK;
			branch3.GB_OH_OrgProxy = orgHeader3.PK;
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";
			Factory.Save();

			using (Env.SetTemporaryUserContext(company.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertEquals("AMA_Nature", ShipmentTypeList.Codes.Import23, header1.AMA_Nature);
				AssertEquals("AMA_TransportMode", Core.Constants.TransportModes.Air, header1.AMA_TransportMode);
				AssertEquals("AMA_OA_DeconsolidateAddress_ZAddress.OrgPK", ZGuid.Empty, header1.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
			}

			using (Env.SetTemporaryUserContext(company.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertEquals("AMA_OA_DeconsolidateAddress_ZAddress.OrgPK", orgHeader2.PK, header2.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
				AssertEquals("AMA_OA_DeconsolidateAddress", orgAddress2.PK, header2.AMA_OA_DeconsolidateAddress);
			}

			using (Env.SetTemporaryUserContext(company.PK.ToGuid(), branch3.PK.ToGuid(), department.PK.ToGuid()))
			{
				var header3 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertEquals("AMA_OA_DeconsolidateAddress_ZAddress.OrgPK", orgHeader3.PK, header3.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
				AssertEquals("AMA_OA_DeconsolidateAddress", ZGuid.Empty, header3.AMA_OA_DeconsolidateAddress);
			}
		}

		public void TestBillStatus()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.MasterBill.ABL_BillStatus = "BC";
			AssertEquals("BC", header1.BillStatus);
		}

		public void TestBillStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);
			Factory.Save();

			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.MasterBill.ABL_BillStatus = "Z1";
			AssertEquals("Z1 DESC", header1.BillStatusDescription);
		}

		public void TestArrivalHeaders()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>>(header.ArrivalHeaders);
			AssertEquals(typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
