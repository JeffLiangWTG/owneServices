using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidate_NoError_DeactivateConsignment()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.RunPreSaveValidation();
			AssertHasErrors("precondition - has errors when item count is 0", consignment.HVC_ItemCountInfo);

			consignment.HVC_IsActive = false;
			consignment.RunPreSaveValidation();
			AssertNoErrors("deactivate consignment would not check item count", consignment.HVC_ItemCountInfo);

			consignment.HVC_IsActive = true;
			consignment.RunPreSaveValidation();
			AssertHasErrors("activate consignment would check item count", consignment.HVC_ItemCountInfo);
		}

		#region HVC_ConsignmentId

		public void TestValidate_ConsignmentId()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew().HVI_ItemId = "ITEM1";
			consignment.HVC_ConsignmentId = ZString.Empty;
			AssertNoErrors(consignment.HVC_ConsignmentIdInfo);

			Factory.Save();

			consignment.HVC_ConsignmentId = ZString.Empty;
			AssertHasError(consignment.HVC_ConsignmentIdInfo, "Please enter a Consignment ID.");

			consignment.HVC_ConsignmentId = "X1234";
			AssertNoErrors(consignment.HVC_ConsignmentIdInfo);
		}

		public void TestValidate_ConsignmentId_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				Factory.Save();

				consignment.HVC_ConsignmentId = "1234";
				AssertNoErrors(consignment.HVC_ConsignmentIdInfo);

				consignment.HVC_ConsignmentId = ZString.Empty;
				AssertNoErrors("Consignment ID not mandatory for AU Export", consignment.HVC_ConsignmentIdInfo);
			}
		}

		public void TestValidate_ConsignmentId_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsignmentId = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsignmentId();
			AssertHasWarning(consignment.HVC_ConsignmentIdInfo, "Consignment ID should not contain any characters with diacritics.");

			consignment.HVC_ConsignmentId = "CONSIGID";
			consignment.Validation.ValidateHVC_ConsignmentId();
			AssertNoWarnings(consignment.HVC_ConsignmentIdInfo);
		}

		#endregion

		#region Waybill Number

		public void TestValidate_WaybillNumber_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_WaybillNumber = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_WaybillNumber();
			AssertHasWarning(consignment.HVC_WaybillNumberInfo, "Waybill Number should not contain any characters with diacritics.");

			consignment.HVC_WaybillNumber = "WAYBIL1234";
			consignment.Validation.ValidateHVC_WaybillNumber();
			AssertNoWarnings(consignment.HVC_WaybillNumberInfo);
		}

		#endregion

		#region Consignee

		public void TestValidate_ConsigneeName()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ConsigneeName();
			AssertHasError(consignment.HVC_ConsigneeNameInfo, "Please enter a Consignee Name.");

			consignment.HVC_ConsigneeName = "ConsigneeName";
			AssertNoErrors(consignment.HVC_ConsigneeNameInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ConsigneeName = string.Empty;
			consignment.Validation.ValidateHVC_ConsigneeName();
			AssertNoErrors(consignment.HVC_ConsigneeNameInfo);
			AssertEquals(true, consignment.HasMessageErrors);

			consignment.HVC_ConsigneeName = "ConsigneeName";
			consignment.Validation.ValidateHVC_ConsigneeName();
			AssertEquals(false, consignment.HasMessageErrors);
		}

		public void TestValidate_ConsigneeName_AU_Export_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ConsigneeName = "ConsigneeName";
				AssertNoErrors(consignment.HVC_ConsigneeNameInfo);

				consignment.HVC_ConsigneeName = ZString.Empty;
				AssertNoErrors("Consignee Name not mandatory for AU Export", consignment.HVC_ConsigneeNameInfo);
			}
		}

		public void TestValidate_ConsigneeName_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeName = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsigneeName();
			AssertHasWarning(consignment.HVC_ConsigneeNameInfo, "Consignee Name should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeName = "ConsigneeName";
			consignment.Validation.ValidateHVC_ConsigneeName();
			AssertNoWarnings(consignment.HVC_ConsigneeNameInfo);
		}

		public void TestValidate_ConsigneeAddress1()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ConsigneeAddress1();
			AssertHasError(consignment.HVC_ConsigneeAddress1Info, "Please enter a Consignee Address 1.");

			consignment.HVC_ConsigneeAddress1 = "Address1";
			AssertNoErrors(consignment.HVC_ConsigneeAddress1Info);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ConsigneeAddress1 = string.Empty;
			consignment.Validation.ValidateHVC_ConsigneeAddress1();
			AssertNoErrors(consignment.HVC_ConsigneeAddress1Info);
			AssertEquals(true, consignment.HasMessageErrors);

			consignment.HVC_ConsigneeAddress1 = "Address1";
			consignment.Validation.ValidateHVC_ConsigneeAddress1();
			AssertEquals(false, consignment.HasMessageErrors);
		}

		public void TestValidate_ConsigneeAddress1_AU_Export_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ConsigneeAddress1 = "Address1";
				AssertNoErrors(consignment.HVC_ConsigneeAddress1Info);

				consignment.HVC_ConsigneeAddress1 = ZString.Empty;
				AssertNoErrors("Consignee Address 1 not mandatory for AU Export", consignment.HVC_ConsigneeAddress1Info);
			}
		}

		public void TestValidate_ConsigneeAdress1_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeAddress1 = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsigneeAddress1();
			AssertHasWarning(consignment.HVC_ConsigneeAddress1Info, "Consignee Address 1 should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeAddress1 = "ConsigneeAddress1";
			consignment.Validation.ValidateHVC_ConsigneeAddress1();
			AssertNoWarnings(consignment.HVC_ConsigneeAddress1Info);
		}

		public void TestValidate_ConsigneeAddress2_NotMandatory()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeAddress2 = "Address2";
			consignment.Validation.ValidateHVC_ConsigneeAddress2();
			AssertNoErrors(consignment.HVC_ConsigneeAddress2Info);

			consignment.HVC_ConsigneeAddress2 = string.Empty;
			consignment.Validation.ValidateHVC_ConsigneeAddress2();
			AssertNoErrors(consignment.HVC_ConsigneeAddress2Info);
		}

		public void TestValidate_ConsigneeAdress2_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeAddress2 = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsigneeAddress2();
			AssertHasWarning(consignment.HVC_ConsigneeAddress2Info, "Consignee Address 2 should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeAddress2 = "ConsigneeAddress2";
			consignment.Validation.ValidateHVC_ConsigneeAddress2();
			AssertNoWarnings(consignment.HVC_ConsigneeAddress2Info);
		}

		public void TestValidate_ConsigneeCity()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ConsigneeCity();
			AssertHasError(consignment.HVC_ConsigneeCityInfo, "Please enter a Consignee City.");

			consignment.HVC_ConsigneeCity = "City";
			AssertNoErrors(consignment.HVC_ConsigneeCityInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ConsigneeCity = string.Empty;
			consignment.Validation.ValidateHVC_ConsigneeCity();
			AssertNoErrors(consignment.HVC_ConsigneeCityInfo);
			AssertEquals(true, consignment.HasMessageErrors);

			consignment.HVC_ConsigneeCity = "City";
			consignment.Validation.ValidateHVC_ConsigneeCity();
			AssertEquals(false, consignment.HasMessageErrors);

			using (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consignment.HVC_ConsigneeCity = string.Empty;
				AssertNoErrors(consignment.HVC_ConsigneeCityInfo);
			}
		}

		public void TestValidate_ConsigneeCity_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ConsigneeCity = "City";
				AssertNoErrors(consignment.HVC_ConsigneeCityInfo);

				consignment.HVC_ConsigneeCity = ZString.Empty;
				AssertNoErrors("Consignee Address 1 not mandatory for AU Export", consignment.HVC_ConsigneeCityInfo);
			}
		}

		public void TestValidate_ConsigneeCity_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeCity = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsigneeCity();
			AssertHasWarning(consignment.HVC_ConsigneeCityInfo, "Consignee City should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeCity = "ConsigneeCity";
			consignment.Validation.ValidateHVC_ConsigneeCity();
			AssertNoWarnings(consignment.HVC_ConsigneeCityInfo);
		}

		public void TestValidate_ConsigneeState()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeState = "aaa";
			consignment.Validation.ValidateHVC_ConsigneeState();
			AssertHasError(consignment.HVC_ConsigneeStateInfo, "Enter a valid State.");

			consignment.HVC_ConsigneeState = "NSW";
			consignment.Validation.ValidateHVC_ConsigneeState();
			AssertNoErrors(consignment.HVC_ConsigneeStateInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consignment.HVC_ConsigneeState = "aaa";
				consignment.Validation.ValidateHVC_ConsigneeState();
				AssertNoErrors(consignment.HVC_ConsigneeStateInfo);
			}
		}

		public void TestValidate_ConsigneePostCode()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.HVC_ConsigneePostcode = ZString.Empty;
			AssertHasError(consignment.HVC_ConsigneePostcodeInfo, "Please enter a Consignee Postcode.");

			consignment.HVC_ConsigneePostcode = "1234";
			AssertNoErrors(consignment.HVC_ConsigneePostcodeInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ConsigneePostcode = ZString.Empty;
			AssertNoErrors(consignment.HVC_ConsigneePostcodeInfo);
			AssertEquals(true, consignment.HVC_ConsigneePostcodeInfo.HasMessageErrors());

			consignment.HVC_ConsigneePostcode = "1234";
			AssertEquals(false, consignment.HVC_ConsigneePostcodeInfo.HasMessageErrors());
		}

		public void TestValidate_ConsigneePostCode_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ConsigneePostcode = "1234";
				AssertNoErrors(consignment.HVC_ConsigneePostcodeInfo);

				consignment.HVC_ConsigneePostcode = ZString.Empty;
				AssertNoErrors("Consignee Postcode not mandatory for AU Export", consignment.HVC_ConsigneePostcodeInfo);
			}
		}

		public void TestValidate_ConsigneeCountry()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			consignment.Validation.ValidateHVC_RN_NKConsigneeCountryCode();
			AssertNoErrors(consignment.HVC_RN_NKConsigneeCountryCodeInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item.HVI_IsUnmanifestedAtDestination = false;
				consignment.Validation.ValidateHVC_RN_NKConsigneeCountryCode();
				AssertHasError(consignment.HVC_RN_NKConsigneeCountryCodeInfo, "Please enter a Consignee Country/Region Code.");

				consignment.HVC_RN_NKConsigneeCountryCode = "11";
				AssertHasError(consignment.HVC_RN_NKConsigneeCountryCodeInfo, "Enter a valid Consignee Country/Region Code.");

				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertNoErrors(consignment.HVC_RN_NKConsigneeCountryCodeInfo);

				item.HVI_IsUnmanifestedAtDestination = true;
				consignment.HVC_RN_NKConsigneeCountryCode = "11";
				consignment.Validation.ValidateHVC_RN_NKConsigneeCountryCode();
				AssertHasError(consignment.HVC_RN_NKConsigneeCountryCodeInfo, "Enter a valid Consignee Country/Region Code.");

				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertNoErrors(consignment.HVC_RN_NKConsigneeCountryCodeInfo);
				consignment.Validation.ValidateHVC_RN_NKConsigneeCountryCode();
				AssertEquals(true, consignment.HasMessageErrors);

				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment.Validation.ValidateHVC_RN_NKConsigneeCountryCode();
				AssertEquals(false, consignment.HasMessageErrors);
			}
		}

		public void TestValidate_ConsigneeCountry_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			using (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertNoErrors(consignment.HVC_RN_NKConsigneeCountryCodeInfo);

				consignment.HVC_RN_NKConsigneeCountryCode = ZString.Empty;
				AssertHasError("Consignee Country Code is still mandatory for AU Export", consignment.HVC_RN_NKConsigneeCountryCodeInfo, "Please enter a Consignee Country/Region Code.");
			}
		}

		public void TestValidate_ConsigneeEmail()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeEmail = "aaa@@@";
			consignment.Validation.ValidateHVC_ConsigneeEmail();
			AssertHasError(consignment.HVC_ConsigneeEmailInfo, "Email Address is not valid .");

			consignment.HVC_ConsigneeEmail = "aaa@gmail.com";
			consignment.Validation.ValidateHVC_ConsigneeEmail();
			AssertNoErrors(consignment.HVC_ConsigneeEmailInfo);
		}

		public void TestValidate_ConsigneeEmail_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeEmail = "ááá@gmail.com";
			consignment.Validation.ValidateHVC_ConsigneeEmail();
			AssertHasWarning(consignment.HVC_ConsigneeEmailInfo, "Consignee Email should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeEmail = "aaa@gmail.com";
			consignment.Validation.ValidateHVC_ConsigneeEmail();
			AssertNoWarnings(consignment.HVC_ConsigneeEmailInfo);
		}

		public void TestValidate_ConsigneeInstructions_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeInstructions = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ConsigneeInstructions();
			AssertHasWarning(consignment.HVC_ConsigneeInstructionsInfo, "Consignee Instructions should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeInstructions = "ConsigneeInstructions";
			consignment.Validation.ValidateHVC_ConsigneeInstructions();
			AssertNoWarnings(consignment.HVC_ConsigneeInstructionsInfo);
		}

		public void TestValidate_ConsigneeContact_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ConsigneeContact = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperContact();
			AssertHasWarning(consignment.HVC_ConsigneeContactInfo, "Consignee Contact should not contain any characters with diacritics.");

			consignment.HVC_ConsigneeContact = "Consignee Contact";
			consignment.Validation.ValidateHVC_ShipperContact();
			AssertNoWarnings(consignment.HVC_ConsigneeContactInfo);
		}

		#endregion

		#region Shipper

		public void TestValidate_ShipperName()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ShipperName();
			AssertHasError(consignment.HVC_ShipperNameInfo, "Please enter a Shipper Name.");

			consignment.HVC_ShipperName = "ConsignorName";
			AssertNoErrors(consignment.HVC_ShipperNameInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ShipperName = string.Empty;
			consignment.Validation.ValidateHVC_ShipperName();
			AssertNoErrors(consignment.HVC_ShipperNameInfo);
		}

		public void TestValidate_ShipperName_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ShipperName = "ConsignorName";
				AssertNoErrors(consignment.HVC_ShipperNameInfo);

				consignment.HVC_ShipperName = ZString.Empty;
				AssertHasError("Shipper Name is still mandatory for AU Export", consignment.HVC_ShipperNameInfo, "Please enter a Shipper Name.");
			}
		}

		public void TestValidate_ShipperName_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperName = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperName();
			AssertHasWarning(consignment.HVC_ShipperNameInfo, "Shipper Name should not contain any characters with diacritics.");

			consignment.HVC_ShipperName = "ShipperName";
			consignment.Validation.ValidateHVC_ShipperName();
			AssertNoWarnings(consignment.HVC_ShipperNameInfo);
		}

		public void TestValidate_ShipperAddress1()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertHasError(consignment.HVC_ShipperAddress1Info, "Please enter a Shipper Address 1.");

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertNoErrors("Expect allow field to be zero if surplus at destination", consignment.HVC_ShipperAddress1Info);

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.HVC_ShipperAddress1 = "Address1";
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertNoErrors(consignment.HVC_ShipperAddress1Info);
		}

		public void TestValidate_ShipperAddress1_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ShipperAddress1 = "Address1";
				AssertNoErrors(consignment.HVC_ShipperAddress1Info);

				consignment.HVC_ShipperAddress1 = ZString.Empty;
				AssertNoErrors("Shipper Address 1 not mandatory for AU Export", consignment.HVC_ShipperAddress1Info);
			}
		}

		public void TestValidate_ShipperAddress1_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperAddress1 = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertHasWarning(consignment.HVC_ShipperAddress1Info, "Shipper Address 1 should not contain any characters with diacritics.");

			consignment.HVC_ShipperAddress1 = "ShipperAddress1";
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertNoWarnings(consignment.HVC_ShipperAddress1Info);
		}

		public void TestValidate_ShipperAddress2_NotMandatory()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperAddress2 = "Address2";
			consignment.Validation.ValidateHVC_ShipperAddress2();
			AssertNoErrors(consignment.HVC_ShipperAddress2Info);

			consignment.HVC_ShipperAddress2 = string.Empty;
			consignment.Validation.ValidateHVC_ShipperAddress2();
			AssertNoErrors(consignment.HVC_ShipperAddress2Info);
		}

		public void TestValidate_ShipperAddress2_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperAddress2 = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperAddress2();
			AssertHasWarning(consignment.HVC_ShipperAddress2Info, "Shipper Address 2 should not contain any characters with diacritics.");

			consignment.HVC_ShipperAddress2 = "ShipperAddress2";
			consignment.Validation.ValidateHVC_ShipperAddress2();
			AssertNoWarnings(consignment.HVC_ShipperAddress2Info);
		}

		public void TestValidate_ShipperCity()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.Validation.ValidateHVC_ShipperCity();
			AssertHasError(consignment.HVC_ShipperCityInfo, "Please enter a Shipper City.");

			consignment.HVC_ShipperCity = "City";
			AssertNoErrors(consignment.HVC_ShipperCityInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_ShipperCity = string.Empty;
			consignment.Validation.ValidateHVC_ShipperAddress1();
			AssertNoErrors(consignment.HVC_ShipperCityInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consignment.HVC_ShipperCity = string.Empty;
				AssertNoErrors(consignment.HVC_ShipperCityInfo);
			}
		}

		public void TestValidate_ShipperCity_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_IsUnmanifestedAtDestination = false;

				consignment.HVC_ShipperCity = "City";
				AssertNoErrors(consignment.HVC_ShipperCityInfo);

				consignment.HVC_ShipperCity = ZString.Empty;
				AssertNoErrors("Shipper City not mandatory for AU Export", consignment.HVC_ShipperCityInfo);
			}
		}

		public void TestValidate_ShipperCity_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperCity = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperCity();
			AssertHasWarning(consignment.HVC_ShipperCityInfo, "Shipper City should not contain any characters with diacritics.");

			consignment.HVC_ShipperCity = "ShipperCity";
			consignment.Validation.ValidateHVC_ShipperCity();
			AssertNoWarnings(consignment.HVC_ShipperCityInfo);
		}

		public void TestValidate_ShipperState()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_ShipperState = "aaa";
			AssertHasError(consignment.HVC_ShipperStateInfo, "Enter a valid State.");

			consignment.HVC_ShipperState = "NSW";
			AssertNoErrors(consignment.HVC_ShipperStateInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consignment.HVC_ShipperState = "aaa";
				AssertNoErrors(consignment.HVC_ShipperStateInfo);
			}
		}

		public void TestValidate_ShipperPostcode()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.Validation.ValidateHVC_ShipperPostcode();
			var errorMessage =
				@"You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to ""Must Be Entered"".

If you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.";

			AssertHasError(consignment.HVC_ShipperPostcodeInfo, errorMessage);

			consignment.HVC_ShipperPostcode = "aaaa";
			AssertNoErrors(consignment.HVC_ShipperPostcodeInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consignment.HVC_ShipperPostcode = string.Empty;
				AssertNoErrors(consignment.HVC_ShipperPostcodeInfo);
			}
		}

		public void TestValidate_ShipperCountry()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			consignment.Validation.ValidateHVC_RN_NKShipperCountryCode();
			AssertNoErrors(consignment.HVC_RN_NKShipperCountryCodeInfo);

			using (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item.HVI_IsUnmanifestedAtDestination = false;
				consignment.Validation.ValidateHVC_RN_NKShipperCountryCode();
				AssertHasError(consignment.HVC_RN_NKShipperCountryCodeInfo, "Please enter a Shipper Country/Region Code.");

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				AssertNoErrors(consignment.HVC_RN_NKShipperCountryCodeInfo);

				consignment.HVC_RN_NKShipperCountryCode = "11";
				item.HVI_IsUnmanifestedAtDestination = true;
				consignment.Validation.ValidateHVC_RN_NKShipperCountryCode();
				AssertHasError(consignment.HVC_RN_NKShipperCountryCodeInfo, "Enter a valid Shipper Country/Region Code.");

				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				AssertNoErrors(consignment.HVC_RN_NKShipperCountryCodeInfo);
			}
		}

		public void TestValidate_ShipperEmail()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ShipperEmail = "aaa@@@";
			AssertHasError(consignment.HVC_ShipperEmailInfo, "Email Address is not valid .");

			consignment.HVC_ShipperEmail = "aaa@gmail.com";
			AssertNoErrors(consignment.HVC_ShipperEmailInfo);
		}

		public void TestValidate_ShipperEmail_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperEmail = "ááá@gmail.com";
			consignment.Validation.ValidateHVC_ShipperEmail();
			AssertHasWarning(consignment.HVC_ShipperEmailInfo, "Shipper Email should not contain any characters with diacritics.");

			consignment.HVC_ShipperEmail = "aaa@gmail.com";
			consignment.Validation.ValidateHVC_ShipperEmail();
			AssertNoWarnings(consignment.HVC_ShipperEmailInfo);
		}

		public void TestValidate_ShipperContact_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperContact = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperContact();
			AssertHasWarning(consignment.HVC_ShipperContactInfo, "Shipper Contact should not contain any characters with diacritics.");

			consignment.HVC_ShipperContact = "ShipperContact";
			consignment.Validation.ValidateHVC_ShipperContact();
			AssertNoWarnings(consignment.HVC_ShipperContactInfo);
		}

		public void TestValidate_ShipperReference_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_ShipperReference = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_ShipperReference();
			AssertHasWarning(consignment.HVC_ShipperReferenceInfo, "Shipper Reference should not contain any characters with diacritics.");

			consignment.HVC_ShipperReference = "ShipperReference";
			consignment.Validation.ValidateHVC_ShipperReference();
			AssertNoWarnings(consignment.HVC_ShipperReferenceInfo);
		}

		public void TestShipperFieldMandatoryValidationWhenDefaultingFromShipment()
		{
			HVLVTestHelper.SetGS1FountainOnOrgProxy(Factory, "1234567");

			AssertShipperFieldMandatoryValidationWhenDefaultingFromShipment(x => x.HVC_ShipperNameInfo, x => x.ValidateHVC_ShipperName(), x => x.Header.OH_FullName = "Barry Dawson and co.");
			AssertShipperFieldMandatoryValidationWhenDefaultingFromShipment(x => x.HVC_ShipperAddress1Info, x => x.ValidateHVC_ShipperAddress1(), x => x.OA_Address1 = "45 Ernie Dingo Lane");
			AssertShipperFieldMandatoryValidationWhenDefaultingFromShipment(x => x.HVC_ShipperCityInfo, x => x.ValidateHVC_ShipperCity(), x => x.OA_City = "Radelaide");

			using (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertShipperFieldMandatoryValidationWhenDefaultingFromShipment(x => x.HVC_RN_NKShipperCountryCodeInfo, x => x.ValidateHVC_RN_NKShipperCountryCode(), x => x.OA_RN_NKCountryCode = "AU");
			}
		}

		void AssertShipperFieldMandatoryValidationWhenDefaultingFromShipment(Func<HVLVConsignment, ZPropertyInfo> getShipperPropertyInfo, Action<HVLVConsignmentValidation> validate, Action<OrgAddress> populateConsignorField)
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var consignor1 = Factory.NewWithValidTestData<OrgAddress>();
			consignor1.Header.OH_FullName = "Company 1";
			populateConsignorField(consignor1);

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;

			var propertyInfo = getShipperPropertyInfo(consignment);
			var expectedMessage = $"Please enter a {propertyInfo.HumanReadableName}.";

			validate(consignment.Validation);
			AssertHasError(propertyInfo, expectedMessage);

			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.PK;
			validate(consignment.Validation);
			AssertNoError($"{Name} can default from the consignor, so it shouldn't be mandatory.", propertyInfo, expectedMessage);

			consignment.HVC_ShipperEmail = "fred@fredscookingutensils.com";
			validate(consignment.Validation);
			AssertHasError("Another Shipper Field was set on the consignment, Shipper Fields are not eligible for defaulting", propertyInfo, expectedMessage);

			consignment.HVC_ShipperEmail = ZString.Empty;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.PK;
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment2.PK;
			validate(consignment.Validation);
			AssertNoError("Multiple Shipments attached but they all have the same consignor, so field can be defaulted.", propertyInfo, expectedMessage);

			var consignor2 = Factory.NewWithValidTestData<OrgAddress>();
			consignor2.Header.OH_FullName = "Company 2";

			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.PK;
			validate(consignment.Validation);
			AssertHasError("Multiple Shipments attached with different consignors, can't default", propertyInfo, expectedMessage);

			Factory.Save();

			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.PK;
			validate(consignment.Validation);
			AssertHasError("Consignment is in the database, Shipper Fields are not eligible for defaulting", propertyInfo, expectedMessage);
		}

		#endregion

		#region Weight and Volume

		public void TestValidate_WeightUQ()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = ZString.Empty;
			AssertHasError(consignment.HVC_WeightUQInfo, "Please enter a Weight Unit.");

			consignment.HVC_WeightUQ = "ZZ";
			AssertHasError(consignment.HVC_WeightUQInfo, "Enter a valid Weight Unit.");

			consignment.HVC_WeightUQ = "KG";
			AssertNoErrors(consignment.HVC_WeightUQInfo);
		}

		public void TestValidate_WeightUQ_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.HVC_WeightUQ = "KG";
				AssertNoErrors(consignment.HVC_WeightUQInfo);

				consignment.HVC_WeightUQ = ZString.Empty;
				AssertNoErrors("Weight Unit not mandatory for AU Export", consignment.HVC_WeightUQInfo);
			}
		}

		public void TestValidate_VolumeUQ()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_VolumeUQ = ZString.Empty;
			AssertHasError(consignment.HVC_VolumeUQInfo, "Please enter a Volume Unit.");

			consignment.HVC_VolumeUQ = "ZZ";
			AssertHasError(consignment.HVC_VolumeUQInfo, "Enter a valid Volume Unit.");

			consignment.HVC_VolumeUQ = "M3";
			AssertNoErrors(consignment.HVC_VolumeUQInfo);
		}

		public void TestValidate_VolumeUQ_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.HVC_VolumeUQ = "M3";
				AssertNoErrors(consignment.HVC_VolumeUQInfo);

				consignment.HVC_VolumeUQ = ZString.Empty;
				AssertNoErrors("Volume Unit not mandatory for AU Export", consignment.HVC_VolumeUQInfo);
			}
		}

		#endregion

		public void TestValidate_LastMileCarrierServiceLevel()
		{
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierOrg.OH_IsShippingProvider = true;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "XXX";
			AssertHasError(consignment.HVC_PL_NKLastMileCarrierServiceLevelInfo, "Enter a valid Last Mile Carrier Service Level.");

			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "STD";
			AssertNoErrors(consignment.HVC_PL_NKLastMileCarrierServiceLevelInfo);
		}

		public void TestValidate_ConsignmentServiceLevel()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_RS_NKServiceLevel = "XXX";
			AssertHasError(consignment.HVC_RS_NKServiceLevelInfo, "Enter a valid selection.");

			consignment.HVC_RS_NKServiceLevel = "D2D";
			AssertNoErrors(consignment.HVC_RS_NKServiceLevelInfo);
		}

		public void TestWhenTotalHVSCustomsValueEqualsHVCGoodsValue_NoWarningMessageIsShown()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = bookingHeader.Consignments.AddNew();

			consignment.HVC_GoodsValue = 100;
			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";

			var consignmentItem = consignment.Items.AddNew();

			var consignmentItemLine1 = consignmentItem.Lines.AddNew();
			var consignmentItemLine2 = consignmentItem.Lines.AddNew();
			var consignmentItemLine3 = consignmentItem.Lines.AddNew();

			consignmentItemLine1.FillWithValidTestData();
			consignmentItemLine1.HVS_CustomsValue = 50;
			consignmentItemLine2.FillWithValidTestData();
			consignmentItemLine2.HVS_CustomsValue = 30;
			consignmentItemLine3.FillWithValidTestData();
			consignmentItemLine3.HVS_CustomsValue = 20;

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertNoWarnings(consignment.HVC_GoodsValueInfo);

			consignmentItemLine1.HVS_CustomsValue = 100;
			consignmentItemLine2.HVS_CustomsValue = 60;
			consignmentItemLine3.HVS_CustomsValue = 40;

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertHasWarning(consignment.HVC_GoodsValueInfo, "Goods Value does not match Item Line Values.");

			consignment.HVC_GoodsValue = 200;

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertNoWarnings(consignment.HVC_GoodsValueInfo);

			consignment.HVC_GoodsValue = 150;

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertHasWarning(consignment.HVC_GoodsValueInfo, "Goods Value does not match Item Line Values.");
		}

		public void TestValidate_GoodsValue_ErrorIfNegative()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_GoodsValue = 10;
			consignment.Validation.ValidateHVC_GoodsValue();
			AssertNoErrors(consignment.HVC_GoodsValueInfo);

			consignment.HVC_GoodsValue = 0;
			consignment.Validation.ValidateHVC_GoodsValue();
			AssertNoErrors(consignment.HVC_GoodsValueInfo);

			consignment.HVC_GoodsValue = -2;
			consignment.Validation.ValidateHVC_GoodsValue();
			AssertHasErrors("Please enter a Goods Value greater than or equal to 0.", consignment.HVC_GoodsValueInfo);
		}

		public void TestValidate_TransportValue_ErrorIfNegative()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_TransportValue = 10;
			consignment.Validation.ValidateHVC_TransportValue();
			AssertNoErrors(consignment.HVC_TransportValueInfo);

			consignment.HVC_TransportValue = 0;
			consignment.Validation.ValidateHVC_TransportValue();
			AssertNoErrors(consignment.HVC_TransportValueInfo);

			consignment.HVC_TransportValue = -2;
			consignment.Validation.ValidateHVC_TransportValue();
			AssertHasErrors("Please enter a Transport Value greater than or equal to 0.", consignment.HVC_TransportValueInfo);
		}

		public void TestValidate_InsuranceValue_ErrorIfNegative()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_InsuranceValue = 10;
			consignment.Validation.ValidateHVC_InsuranceValue();
			AssertNoErrors(consignment.HVC_InsuranceValueInfo);

			consignment.HVC_InsuranceValue = 0;
			consignment.Validation.ValidateHVC_InsuranceValue();
			AssertNoErrors(consignment.HVC_InsuranceValueInfo);

			consignment.HVC_InsuranceValue = -2;
			consignment.Validation.ValidateHVC_InsuranceValue();
			AssertHasErrors("Please enter a Insurance Value greater than or equal to 0.", consignment.HVC_InsuranceValueInfo);
		}

		public void TestValidate_GoodsValueCurrency_ErrorIfNotEnteredAndGoodsValueIsEntered()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_GoodsValue = 0;
			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_GoodsValue = 10;
			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertHasErrors("Please enter a Goods Value Currency when Goods Value is entered.", consignment.HVC_RX_NKGoodsValueCurrencyInfo);
		}

		public void TestValidate_GoodsValueCurrency_ErrorIfNotEnteredAndTransportValueIsEntered()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_TransportValue = 0;
			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_TransportValue = 10;
			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertHasErrors("Please enter a Goods Value Currency when Transport Value is entered.", consignment.HVC_RX_NKGoodsValueCurrencyInfo);
		}

		public void TestValidate_GoodsValueCurrency_ErrorIfNotEnteredAndInsuranceValueIsEntered()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment.HVC_InsuranceValue = 0;
			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_InsuranceValue = 10;
			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

			consignment.HVC_RX_NKGoodsValueCurrency = "";
			consignment.Validation.ValidateHVC_RX_NKGoodsValueCurrency();
			AssertHasErrors("Please enter a Goods Value Currency when Insurance Value is entered.", consignment.HVC_RX_NKGoodsValueCurrencyInfo);
		}

		public void TestWhenNoHVLVItemLinesExist_NoValidationOccurs()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = bookingHeader.Consignments.AddNew();

			consignment.HVC_GoodsValue = 100;
			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";

			var consignmentItem = consignment.Items.AddNew();

			var consignmentItemLine1 = consignmentItem.Lines.AddNew();
			var consignmentItemLine2 = consignmentItem.Lines.AddNew();
			var consignmentItemLine3 = consignmentItem.Lines.AddNew();

			consignmentItemLine1.FillWithValidTestData();
			consignmentItemLine1.HVS_CustomsValue = 100;
			consignmentItemLine2.FillWithValidTestData();
			consignmentItemLine2.HVS_CustomsValue = 60;
			consignmentItemLine3.FillWithValidTestData();
			consignmentItemLine3.HVS_CustomsValue = 40;

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertHasWarning(consignment.HVC_GoodsValueInfo, "Goods Value does not match Item Line Values.");

			consignmentItemLine1.Delete();
			consignmentItemLine2.Delete();
			consignmentItemLine3.Delete();

			consignment.Validation.ValidateHVC_GoodsValue();

			AssertNoWarnings(consignment.HVC_GoodsValueInfo);
		}

		public void TestValidate_GoodsValueCurrency()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_GoodsValue = 10;
			consignment.HVC_RX_NKGoodsValueCurrency = ZString.Empty;
			AssertHasError(consignment.HVC_RX_NKGoodsValueCurrencyInfo, "Please enter a Goods Value Currency.");

			consignment.HVC_RX_NKGoodsValueCurrency = "ZZ";
			AssertHasError(consignment.HVC_RX_NKGoodsValueCurrencyInfo, "Enter a valid Goods Value Currency.");

			consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
			AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);
		}

		public void TestValidate_GoodsValueCurrency_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.HVC_RX_NKGoodsValueCurrency = "AUD";
				AssertNoErrors(consignment.HVC_RX_NKGoodsValueCurrencyInfo);

				consignment.HVC_RX_NKGoodsValueCurrency = ZString.Empty;
				AssertNoErrors("Goods Value Currency not mandatory for AU Export", consignment.HVC_RX_NKGoodsValueCurrencyInfo);
			}
		}

		public void TestValidate_GoodsDescription()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			item.HVI_IsUnmanifestedAtDestination = false;
			consignment.HVC_GoodsDescription = ZString.Empty;
			AssertHasWarning(consignment.HVC_GoodsDescriptionInfo, "You have not entered a Goods Description.");

			consignment.HVC_GoodsDescription = "A bag of lettuce";
			AssertNoErrors(consignment.HVC_GoodsDescriptionInfo);

			item.HVI_IsUnmanifestedAtDestination = true;
			consignment.HVC_GoodsDescription = ZString.Empty;
			AssertNoErrors(consignment.HVC_GoodsDescriptionInfo);

			AssertEquals(false, consignment.HVC_GoodsDescriptionInfo.HasMessageErrors());
			consignment.HVC_GoodsDescription = "A bag of lettuce";
			AssertEquals(false, consignment.HVC_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestValidate_GoodsDescription_AUExport_IsMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.HVC_GoodsDescription = "A bag of lettuce";
				AssertNoErrors(consignment.HVC_GoodsDescriptionInfo);

				consignment.HVC_GoodsDescription = ZString.Empty;
				AssertHasError("Goods Description upgraded to mandatory for AU Export", consignment.HVC_GoodsDescriptionInfo, "Please enter a Goods Description.");
			}
		}

		public void TestValidate_GoodsDescription_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_GoodsDescription = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_GoodsDescription();
			AssertHasWarning(consignment.HVC_GoodsDescriptionInfo, "Goods Description should not contain any characters with diacritics.");

			consignment.HVC_GoodsDescription = "Goods Description";
			consignment.Validation.ValidateHVC_GoodsDescription();
			AssertNoWarnings(consignment.HVC_GoodsDescriptionInfo);
		}

		public void TestValidate_ItemCount()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.Validation.ValidateHVC_ItemCount();
			AssertHasError(consignment.HVC_ItemCountInfo, "Consignment needs to have at least one item.");

			consignment.Items.AddNew();
			AssertNoErrors(consignment.HVC_ItemCountInfo);
		}

		public void TestValidate_ItemCount_AUExport_CannotBeZero()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.Validation.ValidateHVC_ItemCount();
				AssertHasError("At least one item still required for AU EXport", consignment.HVC_ItemCountInfo, "Consignment needs to have at least one item.");

				consignment.Items.AddNew();
				AssertNoErrors(consignment.HVC_ItemCountInfo);
			}
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.HVLVConsignment, Factory.New<HVLVConsignment>().ValidationSection);
		}

		public void TestValidate_PreScreeningStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "CONSIGN1";

			AssertEquals("Precondition", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
			AssertNoErrors("Precondition", consignment.HVC_PreScreeningStatusInfo);

			consignment.HVC_PreScreeningStatus = "XYZ";
			AssertHasError(consignment.HVC_PreScreeningStatusInfo, "Enter a valid Pre-Screening Status.");

			consignment.HVC_PreScreeningStatus = string.Empty;
			AssertHasError(consignment.HVC_PreScreeningStatusInfo, "Please enter a Pre-Screening Status.");

			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			AssertNoErrors(consignment.HVC_PreScreeningStatusInfo);
		}

		public void TestValidate_PreScreeningStatus_AUExport_IsNotMandatory()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_RL_NKOrigin = "AUSYD";
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
				AssertNoErrors(consignment.HVC_PreScreeningStatusInfo);

				consignment.HVC_PreScreeningStatus = ZString.Empty;
				AssertNoErrors("Pre-Screening Status is not mandatory for AU Export", consignment.HVC_PreScreeningStatusInfo);
			}
		}

		public void TestValidate_VendorIdentifier_WarnIfAnyDiacritics()
		{
			var consignment = Factory.New<HVLVConsignment>();

			consignment.HVC_VendorIdentifier = "Dïácrïtïcs";
			consignment.Validation.ValidateHVC_VendorIdentifier();
			AssertHasWarning(consignment.HVC_VendorIdentifierInfo, "Vendor Identifier should not contain any characters with diacritics.");

			consignment.HVC_VendorIdentifier = "Vendor Identifier";
			consignment.Validation.ValidateHVC_VendorIdentifier();
			AssertNoWarnings(consignment.HVC_VendorIdentifierInfo);
		}

		public void TestValidate_HVC_IsActive_CustomsStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "LTTSTORE01";

			CombineAssertions("Preconditions:", () =>
			{
				Assert("Precondition: should be active", consignment.HVC_IsActive);
				Assert("Precondition: should not have customs release status", !consignment.HasCustomsStatus);
			});

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
			Assert("Should have customs release status", consignment.HasCustomsStatus);

			consignment.HVC_IsActive = false;
			AssertHasError(consignment.HVC_IsActiveInfo, "Cannot deactivate Consignment LTTSTORE01 as it has a Customs release status.");
		}

		public void TestValidate_IsActive_ErrorIfConsignmentHasItemsNotOnManagingShipment()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();

			var consignmentWithItemsOnOtherShipment = consignmentHeader1.Consignments.AddNew();
			var consignmentWithAllItemsOnThisShipment = consignmentHeader2.Consignments.AddNew();

			consignmentWithItemsOnOtherShipment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;
			consignmentWithItemsOnOtherShipment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment2.PK;
			consignmentWithAllItemsOnThisShipment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;

			consignmentWithAllItemsOnThisShipment.HVC_IsActive = true;
			consignmentWithItemsOnOtherShipment.HVC_IsActive = true;

			CombineAssertions("Precondition - no errors:", () =>
			{
				AssertEquals("consignmentWithItemsOnOtherShipment:", true, consignmentWithItemsOnOtherShipment.HasItemsNotOnManagingShipment);
				AssertEquals("consignmentWithAllItemsOnThisShipment:", false, consignmentWithAllItemsOnThisShipment.HasItemsNotOnManagingShipment);

				AssertNoError("consignmentWithItemsOnOtherShipment:", consignmentWithItemsOnOtherShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
				AssertNoError("consignmentWithAllItemsOnThisShipment:", consignmentWithAllItemsOnThisShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
			});

			consignmentWithAllItemsOnThisShipment.HVC_IsActive = false;
			consignmentWithItemsOnOtherShipment.HVC_IsActive = false;

			CombineAssertions("Consignment with items not on managing shipment should show error:", () =>
			{
				AssertHasError("consignmentWithItemsOnOtherShipment:", consignmentWithItemsOnOtherShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
				AssertNoError("consignmentWithAllItemsOnThisShipment:", consignmentWithAllItemsOnThisShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
			});

			consignmentWithAllItemsOnThisShipment.HVC_IsActive = true;
			consignmentWithItemsOnOtherShipment.HVC_IsActive = true;

			CombineAssertions("Error should be removed:", () =>
			{
				AssertNoError("consignmentWithItemsOnOtherShipment:", consignmentWithItemsOnOtherShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
				AssertNoError("consignmentWithAllItemsOnThisShipment:", consignmentWithAllItemsOnThisShipment.HVC_IsActiveInfo, "Cannot deactivate Consignment as it has Items that are not attached to this Shipment.");
			});
		}

		#region Test ValidateAll

		public void TestValidateAll_WithNoChanges_DoesNotValidate()
		{
			var consignment = Factory.New<HVLVConsignment_WithConsigneeNameError_ForTest>();
			AssertEquals("Precondition: No changes on consignment", false, consignment.HasChanges);

			consignment.RunPreSaveValidation();
			AssertNoErrors("Validation should not have run, so no errors", consignment);
		}

		public void TestValidateAll_WithChanges_DoesValidate()
		{
			var consignment = Factory.New<HVLVConsignment_WithConsigneeNameError_ForTest>();
			consignment.HasChanges = true;

			consignment.RunPreSaveValidation();
			AssertHasError("Validation should have run, resulting in error", consignment.HVC_ConsigneeNameInfo, HVLVConsignment_WithConsigneeNameError_ForTest.TestErrorMessage);
		}

		public void TestValidateAll_WithChangesToChildren_DoesValidate()
		{
			var consignment = Factory.New<HVLVConsignment_WithConsigneeNameError_ForTest>();
			AssertEquals("Precondition: No changes on consignment", false, consignment.HasChanges);
			var item = consignment.Items.AddNew();
			item.HVI_IsDamaged = true;

			CombineAssertions("Precondition: Changing child should set has changes on parent", () =>
			{
				AssertEquals("Item has changes", true, item.HasChanges);
				AssertEquals("Consignment also has changes", true, consignment.HasChanges);
			});

			consignment.RunPreSaveValidation();
			AssertHasError("Validation should have run, resulting in error", consignment.HVC_ConsigneeNameInfo, HVLVConsignment_WithConsigneeNameError_ForTest.TestErrorMessage);
		}

		#endregion

		class HVLVConsignment_WithConsigneeNameError_ForTest : HVLVConsignment
		{
			public HVLVConsignment_WithConsigneeNameError_ForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override HVLVConsignmentValidation GetNewValidation()
			{
				return new HVLVConsignmentValidation_ForTest(this);
			}

			public static string TestErrorMessage => "This is an error that always appears when validating for tests";

			class HVLVConsignmentValidation_ForTest : HVLVConsignmentValidation
			{
				public HVLVConsignmentValidation_ForTest(AutoHVLVConsignment parent)
					: base(parent)
				{
				}

				protected override void CheckHVC_ConsigneeName()
				{
					Parent.HVC_ConsigneeNameInfo.AddError(TestErrorMessage);
				}
			}
		}
	}
}
