using System;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBill))]
sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
{
	public void TestLookups() => AssertType<AsycudaBillLookups>(bill.Lookups);

	public void TestValidation() => CombineAssertions(() =>
	{
		AssertType<AsycudaBillValidationForRegularBill>("For regular bill", bill.Validation);
		AssertType<AsycudaBillValidationForMasterChild>("For master bill", header.MasterBill.Validation);
	});

	public void TestABL_ForwarderEmail_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_ForwarderEmail)
			.WithCaption("Email Address"));

	public void TestABL_ShipperEmail_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_ShipperEmail)
			.WithCaption("Email Address"));

	public void TestABL_ConsigneeEmail_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_ConsigneeEmail)
			.WithCaption("Email Address"));

	public void TestImportProcedure_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ImportProcedure)
			.WithList($"{nameof(AsycudaBill.Lookups)}.{nameof(AsycudaBillLookups.ImportProcedureCodeList)}")
			.WithCaption("Import Procedure")
			.WithFullDescription("Import Procedure tells how the goods are cleared into Norway.Varying procedures require different previous document codes.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 30));

	public void TestExportProcedure_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ExportProcedure)
			.WithList($"{nameof(AsycudaBill.Lookups)}.{nameof(AsycudaBillLookups.ExportProcedureCodeList)}")
			.WithCaption("Export Procedure")
			.WithFullDescription("Export Procedure tells how the goods are cleared out of EU/to the Norwegian border. Varying procedures require different previous document codes.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 10));

	public void TestMovementReferenceNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(h => h.MovementReferenceNumber)
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35));

	public void TestEmailAddress1_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.EmailAddress1)
			.WithCaption("Address 1")
			.WithFullDescription("Email address for border passing confirmation.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestEmailAddress2_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.EmailAddress2)
			.WithCaption("Address 2")
			.WithFullDescription("Email address for border passing confirmation.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestEmailAddress3_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.EmailAddress3)
			.WithCaption("Address 3")
			.WithFullDescription("Email address for border passing confirmation.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestABL_OA_Forwarder_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_OA_Forwarder)
			.WithCaption("Representative")
			.WithList($"{nameof(AsycudaBill.ABL_OA_Forwarder_ZAddress)}.{nameof(ZAddress.OrgAddress_List)}"));

	public void TestCustomsLevel_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.CustomsLevel)
			.WithCaption("Customs Level"));

	public void TestCustomsLevel() => CombineAssertions(() =>
	{
		var houseBillValue = "House";
		var masterBillValue = "Master";

		bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals("When IsHouseBill is true, CustomsLevel should return HouseBill",
			houseBillValue, bill.CustomsLevel);

		bill.ABL_BolType = AsycudaBill.ChildBolCode;
		var sameBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = sameBillAddress.PK;
		header.MasterBill.ABL_OA_Forwarder = sameBillAddress.PK;
		AssertEquals("When IsMasterBill is true, CustomsLevel should return MasterBill",
			masterBillValue, bill.CustomsLevel);

		bill.ABL_BolType = ZString.Empty;
		AssertEquals("When neither IsHouseBill nor IsMasterBill, CustomsLevel should return an empty string",
			ZString.Empty, bill.CustomsLevel);

		bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		bill.ABL_OA_Forwarder = ZGuid.Empty;
		AssertEquals("When bill type is CLD and forwarder address is empty (IsHouseBill), CustomsLevel should return HouseBill",
			houseBillValue, bill.CustomsLevel);

		var diffBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = diffBillAddress.PK;
		AssertEquals("When bill type is CLD and forwarder address is different (IsMasterBill), CustomsLevel should return MasterBill",
			masterBillValue, bill.CustomsLevel);
	});

	public void TestIsHouseBill() => CombineAssertions(() =>
	{
		bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals("When bill type is STD", true, bill.IsHouseBill);

		bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		bill.ABL_OA_Forwarder = ZGuid.Empty;
		AssertEquals("When bill type is CLD and forwarder address is empty", true, bill.IsHouseBill);

		var sameBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = sameBillAddress.PK;
		header.MasterBill.ABL_OA_Forwarder = sameBillAddress.PK;
		AssertEquals("When bill type is CLD and forwarder address is same as masterbill", true, bill.IsHouseBill);

		var diffBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = diffBillAddress.PK;
		AssertEquals("When bill type is CLD and forwarder address is not same as masterbill", false, bill.IsHouseBill);
	});

	public void TestIsMasterBill() => CombineAssertions(() =>
	{
		bill.ABL_BolType = AsycudaBill.ChildBolCode;
		AssertEquals("When bill type is BOL", true, bill.IsMasterBill);

		bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		bill.ABL_OA_Forwarder = ZGuid.Empty;
		AssertEquals("When bill type is CLD and forwarder address is empty", false, bill.IsMasterBill);

		var sameBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = sameBillAddress.PK;
		header.MasterBill.ABL_OA_Forwarder = sameBillAddress.PK;
		AssertEquals("When bill type is CLD and forwarder address is same as masterbill", false, bill.IsMasterBill);

		var diffBillAddress = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = diffBillAddress.PK;
		AssertEquals("When bill type is CLD and forwarder address is not same as masterbill", true, bill.IsMasterBill);
	});

	public void TestPreviousDocumentValues()
	{
		var previousDocument = bill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("CSI_ParentTableCode", "ABL", previousDocument.CSI_ParentTableCode);
			AssertEquals("CSI_Type", "PRE", previousDocument.CSI_Type);
		});
	}

	public void TestPreviousDocument()
	{
		var previousDocument = bill.PreviousDocuments.AddNew();
		AssertType<PreviousDocument>(previousDocument);
	}

	public void TestTransportDocumentType_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.TransportDocumentType)
			.WithCaption("Trans. Doc. Type")
			.WithFullDescription("Transport document type.")
			.WithList("Lookups.TransportDocumentTypeList")
			.WithMaxLength(4));

	public void TestTransportDocumentType_Persistence() => PersistenceTestHelper.AssertValueIsPersistedInGenAddOnColumn(Factory, bill.TransportDocumentTypeInfo, "TransportDocumentType", (ZString)"TEST");

	public void TestABL_RL_NKOrigin_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_RL_NKOrigin)
			.WithCaption("Place of Acceptance")
			.WithFullDescription("Place of Acceptance code."));

	public void TestABL_RL_NKPortOfLoading_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_RL_NKPortOfLoading)
			.WithList($"{nameof(AsycudaBill.Lookups)}.{nameof(AsycudaBillLookups.PortOfLoadingCodes)}")
			.WithCaption("Place of Loading")
			.WithFullDescription("Location of Loading."));

	public void TestABL_CustomsLoadPort_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_CustomsLoadPort)
			.WithFullDescription("Place of Loading."));

	public void TestABL_RL_NKPortOfDischarge_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_RL_NKPortOfDischarge)
			.WithList($"{nameof(AsycudaBill.Lookups)}.{nameof(AsycudaBillLookups.PortOfDischargeCodes)}")
			.WithCaption("Place of Unloading")
			.WithFullDescription("Location of Unloading."));

	public void TestABL_CustomsDischargePort_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_CustomsDischargePort)
			.WithFullDescription("Place of Unloading."));

	public void TestABL_CustomsOriginPort_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_CustomsOriginPort)
			.WithFullDescription("Place of Acceptance."));

	public void TestABL_RL_NKFinalDestination_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_RL_NKFinalDestination)
			.WithList($"{nameof(AsycudaBill.Lookups)}.{nameof(AsycudaBillLookups.FinalDestinationCodes)}")
			.WithCaption("Place of Delivery")
			.WithFullDescription("Place of Delivery code."));

	public void TestABL_CustomsFinalDestinationPort_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_CustomsFinalDestinationPort)
			.WithFullDescription("Place of Delivery."));

	public void TestMovementReferenceNumber_SaveAndUpdate() => CombineAssertions(() =>
	{
		bill.MovementReferenceNumber = "1234";
		var mrnEntryNumber = GetMrnEntryNumber();
		AssertEquals("MRN Entry Number first time assignment", "1234", mrnEntryNumber.CE_EntryNum);

		bill.MovementReferenceNumber = "4567";
		mrnEntryNumber = GetMrnEntryNumber();
		AssertEquals("MRN Entry Number on update", "4567", mrnEntryNumber.CE_EntryNum);

		CusEntryNumber GetMrnEntryNumber()
			=> CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.Header.AMA_RN_NKCountry, true);
	});

	public void TestMovementReferenceNumber_Load()
	{
		var entryNumber = CusEntryNumber.LoadOrCreate(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.Header.AMA_RN_NKCountry);
		entryNumber.CE_EntryNum = "NEW123";
		AssertEquals("MRN", "NEW123", bill.MovementReferenceNumber);
	}

	public void TestIMovementReferenceNumberSupporter_MovementReferenceNumber() => CombineAssertions(() =>
	{
		var movementReferenceNumberSupporter = (IMovementReferenceNumberSupporter)bill;

		AssertEquals("[Pre-Condition] No Entry", ZString.Empty, movementReferenceNumberSupporter.MovementReferenceNumber);

		bill.MovementReferenceNumber = "123";
		AssertEquals($"{nameof(IMovementReferenceNumberSupporter.MovementReferenceNumber)} Get", "123", movementReferenceNumberSupporter.MovementReferenceNumber);

		movementReferenceNumberSupporter.MovementReferenceNumber = "666";
		AssertEquals($"{nameof(IMovementReferenceNumberSupporter.MovementReferenceNumber)} Value on Update", "666", bill.MovementReferenceNumber);
	});

	public void TestTransportMode()
	{
		ITransportModeProvider transportModeProvider = bill;
		AssertNotNull("[Pre-Condition]", transportModeProvider);
		CombineAssertions(() =>
		{
			AssertEquals("When not set", ZString.Empty, transportModeProvider.TransportMode);

			bill.Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("When set to SEA", TransportTypeList.Codes.Sea, transportModeProvider.TransportMode);
		});

		ITransportModeProvider billWithoutHeader = Factory.New<AsycudaBill>();
		AssertNotNull("Bill without Header", billWithoutHeader);
		AssertEquals("When Bill do not have a Header", ZString.Empty, billWithoutHeader.TransportMode);
	}

	public void TestABL_RL_NKPortOfLoadingDefaultValues() => AssertValue_UNLOCO(nameof(bill.ABL_RL_NKPortOfLoading), nameof(bill.ABL_CustomsLoadPort));

	public void TestABL_RL_NKPortOfDischargeDefaultValues() => AssertValue_UNLOCO(nameof(bill.ABL_RL_NKPortOfDischarge), nameof(bill.ABL_CustomsDischargePort));

	public void TestABL_RL_NKFinalDestinationDefaultValues() => AssertValue_UNLOCO(nameof(bill.ABL_RL_NKFinalDestination), nameof(bill.ABL_CustomsFinalDestinationPort));

	void AssertValue_UNLOCO(ZString propertyLocationCode, ZString propertyDescription)
	{
		var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco1.RL_NameWithDiacriticals = "UNLOCO Name 1";
		var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco2.RL_NameWithDiacriticals = "UNLOCO Name 2";

		PropertyInfo propertyLocoInfo = bill.GetType().GetProperty(propertyLocationCode);
		PropertyInfo propertyDescInfo = bill.GetType().GetProperty(propertyDescription);

		CombineAssertions(() =>
		{
			SetPropertyValue(propertyLocoInfo, unloco1.Code);
			AssertEquals($"{propertyLocationCode} when 'CODE1'", "UNLOCO Name 1", propertyDescInfo.GetValue(bill));

			SetPropertyValue(propertyLocoInfo, unloco2.Code);
			AssertEquals($"{propertyLocationCode} when 'CODE2'", "UNLOCO Name 2", propertyDescInfo.GetValue(bill));

			SetPropertyValue(propertyLocoInfo, ZString.Empty);
			AssertEquals($"{propertyLocationCode} when Empty", ZString.Empty, propertyDescInfo.GetValue(bill));

			SetPropertyValue(propertyLocoInfo, "DE");
			AssertEquals($"{propertyLocationCode} when countrycode", ZString.Empty, propertyDescInfo.GetValue(bill));

			SetPropertyValue(propertyDescInfo, "Random Value");
			SetPropertyValue(propertyLocoInfo, unloco1.Code);
			AssertEquals($"{propertyLocationCode} when explicit set", (ZString)"Random Value", propertyDescInfo.GetValue(bill));
		});

		void SetPropertyValue(PropertyInfo property, ZString propertyValue)
		{
			property.SetValue(bill, Convert.ChangeType(propertyValue, property.PropertyType), null);
		}
	}

	public void TestForwarderAddressGenerator()
	{
		var address = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "TEST";
		address.OA_OH = orgHeader.PK;

		address.Address1 = "1";
		address.Address2 = "2";
		address.City = "abc";
		address.State = "xy";
		address.Postcode = "2341";
		address.OA_RN_NKCountryCode = "GB";

		bill.ABL_OA_Forwarder = address.PK;

		CombineAssertions(() =>
		{
			AssertEquals(bill.ABL_ForwarderCompanyName, "TEST");
			AssertEquals(bill.ABL_ForwarderStreet1, "1");
			AssertEquals(bill.ABL_ForwarderStreet2, "2");
			AssertEquals(bill.ABL_ForwarderCity, "abc");
			AssertEquals(bill.ABL_ForwarderState, "xy");
			AssertEquals(bill.ABL_ForwarderPostCode, "2341");
			AssertEquals(bill.ABL_Forwarder_RN_NKCountryCode, "GB");
		});
	}

	public void TestForwarderOverridablePropertiesReadOnly()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.MainAddress;
		CombineAssertions("When ABL_OA_Forwarder is empty", () =>
		{
			bill.ABL_OA_Forwarder = ZGuid.Empty;
			AssertForwarderPropertiesReadOnly(bill, expectedReadOnly: true);
		});

		CombineAssertions("When ABL_OA_Forwarder is not empty", () =>
		{
			bill.ABL_OA_Forwarder = orgAddress.PK;
		});
	}

	public void TestConsigneePropertiesReadOnly()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.MainAddress;

		CombineAssertions("When IsStandAlone: false", () =>
		{
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("Prerequisite: header.IsStandAlone is false.", expected: false, header.IsStandAlone);
			AssertConsigneePropertiesReadOnly(bill, expectedReadOnly: true);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: False and ABL_OA_Consignee is empty", () =>
		{
			header.AMA_OverrideFreightDefaults = false;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			AssertConsigneePropertiesReadOnly(bill);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: False and ABL_OA_Consignee is not empty", () =>
		{
			header.AMA_OverrideFreightDefaults = false;
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertConsigneePropertiesReadOnly(bill, expectedReadOnly: true);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: true and ABL_OA_Consignee is not empty", () =>
		{
			header.AMA_OverrideFreightDefaults = true;
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertConsigneePropertiesReadOnly(bill);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: true and ABL_OA_Consignee is empty", () =>
		{
			header.AMA_OverrideFreightDefaults = true;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			AssertConsigneePropertiesReadOnly(bill);
		});

		CombineAssertions("When IsStandAlone: true", () =>
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("Prerequisite: header.IsStandAlone is true.", expected: true, header.IsStandAlone);
			AssertConsigneePropertiesReadOnly(bill);
		});
	}

	public void TestShipperPropertiesReadOnly()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.MainAddress;

		CombineAssertions("When IsStandAlone: false", () =>
		{
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("Prerequisite: header.IsStandAlone is false.", expected: false, header.IsStandAlone);
			AssertShipperPropertiesReadOnly(bill, expectedReadOnly: true);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: False and ABL_OA_Shipper is empty", () =>
		{
			header.AMA_OverrideFreightDefaults = false;
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertShipperPropertiesReadOnly(bill);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: False and ABL_OA_Shipper is not empty", () =>
		{
			header.AMA_OverrideFreightDefaults = false;
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertShipperPropertiesReadOnly(bill, expectedReadOnly: true);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: true and ABL_OA_Shipper is not empty", () =>
		{
			header.AMA_OverrideFreightDefaults = true;
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertShipperPropertiesReadOnly(bill);
		});

		CombineAssertions("When AMA_OverrideFreightDefaults: true and ABL_OA_Shipper is empty", () =>
		{
			header.AMA_OverrideFreightDefaults = true;
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertShipperPropertiesReadOnly(bill);
		});

		CombineAssertions("When IsStandAlone: true", () =>
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("Prerequisite: header.IsStandAlone is true.", expected: true, header.IsStandAlone);
			AssertShipperPropertiesReadOnly(bill);
		});
	}

	void AssertForwarderPropertiesReadOnly(AsycudaBill bill, bool expectedReadOnly = false)
	{
		AssertEquals("ABL_ForwarderEmailInfo.ReadOnly", expectedReadOnly, bill.ABL_ForwarderEmailInfo.ReadOnly);
		AssertEquals("ABL_ForwarderPhoneInfo.ReadOnly", expectedReadOnly, bill.ABL_ForwarderPhoneInfo.ReadOnly);
	}

	void AssertConsigneePropertiesReadOnly(AsycudaBill bill, bool expectedReadOnly = false)
	{
		AssertEquals("ABL_ConsigneeEmailInfo.ReadOnly", expectedReadOnly, bill.ABL_ConsigneeEmailInfo.ReadOnly);
		AssertEquals("ABL_ConsigneePhoneInfo.ReadOnly", expectedReadOnly, bill.ABL_ConsigneePhoneInfo.ReadOnly);
		AssertEquals("ABL_ConsigneeRegNoInfo.ReadOnly", expectedReadOnly, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
	}

	void AssertShipperPropertiesReadOnly(AsycudaBill bill, bool expectedReadOnly = false)
	{
		AssertEquals("ABL_ShipperEmailInfo.ReadOnly", expectedReadOnly, bill.ABL_ShipperEmailInfo.ReadOnly);
		AssertEquals("ABL_ShipperPhoneInfo.ReadOnly", expectedReadOnly, bill.ABL_ShipperPhoneInfo.ReadOnly);
		AssertEquals("ABL_ShipperRegNoInfo.ReadOnly", expectedReadOnly, bill.ABL_ShipperRegNoInfo.ReadOnly);
	}

	public void TestForwarderDefaultAddress()
	{
		var org = Factory.New<OrgHeader>();
		var officeAddress = org.Addresses.AddNew(OrgAddressType.Office, true);
		var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);

		bill.ForwarderOrgPK = org.PK;
		AssertEquals("Should be the office address", officeAddress.PK, bill.ABL_OA_Forwarder);
		AssertNotEquals("Should not be the delivery address", deliveryAddress.PK, bill.ABL_OA_Forwarder);
	}

	public void TestABL_BillNumberResourceStringDataForMasterBill()
	{
		bill.ABL_BolType = AsycudaBill.ChildBolCode;
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_BillNumberInfo, new DataBoundBusinessObject(bill));
		AssertNotNull("ResourceStringData For Bill Type = BOL", resourceStringData);
		CombineAssertions("Resource Strings for Bill of Type BOL", () =>
		{
			AssertEquals("Caption", "Bill Number", resourceStringData.Caption);
			AssertEquals("Full Description", "Master Bill Number. All houses (STD bills) on this master must refer to this transport document number when they are submitted (individually) to customs.", resourceStringData.FullDescription);
		});
	}

	public void TestABL_BillNumberAttributes() => CombineAssertions(() =>
	{
		AssertEntity<AsycudaBill>()
			.HasProperty(b => b.ABL_BillNumber)
			.WithCaption("Bill Number")
			.WithAttribute<ResourceStringDataAttribute>(r =>
				r.Caption == "Bill Number"
				&& r.FullDescription == "Master Bill Number. All houses (STD bills) on this master must refer to this transport document number when they are submitted (individually) to customs."
				&& r.IsApplicableMember == nameof(AsycudaBill.IsChildMasterBill));
	});

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject()
	{
		return bill;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return factory.New<AsycudaManifestHeader>().Bills.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var consol = Factory.New<IForwardingConsol>() as BusinessObject;
		header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.SetParent(consol);
		bill = header.Bills.AddNew();
	}

	AsycudaBill bill;
	AsycudaManifestHeader header;
}
