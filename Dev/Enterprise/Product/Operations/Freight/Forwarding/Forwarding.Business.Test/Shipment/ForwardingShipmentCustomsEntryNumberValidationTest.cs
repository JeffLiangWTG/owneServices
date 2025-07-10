using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCustomsEntryNumberValidation))]
	class ForwardingShipmentCustomsEntryNumberValidationTest : ShipmentCustomsEntryNumberValidationTest
	{
		public override void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ForwardingShipmentCustomsEntryNumberValidation(null));
			AssertNoExceptionThrown(() => new ShipmentCustomsEntryNumberValidation(new ForwardingShipmentCustomsEntryNumber(Factory.New<ForwardingShipment>())));
		}

		public void TestCheckEntryNumber_WhenOriginCountryNotUSAndTerritory()
		{
			var errorForMissing = MandatoryValidation.YouHaveNotEnteredMessage("ITN");
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USORF";
				shipment.JS_RL_NKDestination = "CNYTN";
				Assert(!shipment.UseImportEntryTypeList);
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				AssertHasMessageError(shipment.CustomsEntryNumberForBindingInfo, errorForMissing);

				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "PRABS";
				shipment.JS_RL_NKDestination = "CNYTN";
				Assert(!shipment.UseImportEntryTypeList);
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				AssertHasMessageError(shipment.CustomsEntryNumberForBindingInfo, errorForMissing);

				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "VIAGL";
				shipment.JS_RL_NKDestination = "CNYTN";
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				AssertHasMessageError(shipment.CustomsEntryNumberForBindingInfo, errorForMissing);

				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CNYAN";
				shipment.JS_RL_NKDestination = "CNYTN";
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				AssertNoMessageError(shipment.CustomsEntryNumberForBindingInfo, errorForMissing);
			}
		}

		public override void TestCheckEntryNumber()
		{
			ForwardingShipmentCustomsEntryNumber customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			ForwardingShipment shipment = customsEntryNumber.Shipment;

			customsEntryNumber.EntryType = CusEntryNumberTypes.Brazil.DUE;
			customsEntryNumber.EntryNumber = "69BR5689541979";
			AssertNoMessageError(customsEntryNumber.EntryNumberInfo, "The entered DUE code does not match the required format: <year, 2>BR<Number, 9><CheckDigit, 1>.");
			customsEntryNumber.EntryNumber = "6BR5689541850";
			AssertHasMessageError(customsEntryNumber.EntryNumberInfo, "The entered DUE code does not match the required format: <year, 2>BR<Number, 9><CheckDigit, 1>.");
			customsEntryNumber.EntryNumber = "69BR5689541975";
			AssertHasMessageError(customsEntryNumber.EntryNumberInfo, "The check digit for the entered DUE code is incorrect, it should be 9.");

			customsEntryNumber.EntryType = "CAN";
			customsEntryNumber.EntryNumber = "12345";
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, "This Shipment is not attached to a Consolidation");
			AssertHasMessageError(customsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			customsEntryNumber.EntryNumber = "AAAAJH4EF";
			AssertNoMessageError(customsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			shipment.Consols.AddNew();
			customsEntryNumber.Validation.ValidateEntryNumber();
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, "This Shipment is not attached to a Consolidation");
		}

		public void TestCheckEntryNumber_EntryNumberForBindingInfoHasAllErrorsWhenEntryNumberMoreThan1()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "MRN";
			cusEntryNum1.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "10AD00000117716870";
			cusEntryNum2.CE_EntryType = "MRN";
			cusEntryNum2.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "CAN";
			cusEntryNum3.CE_ParentTable = ForwardingShipment.Schema.TableName;

			var error1 = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";
			var error2 = "MRN does not have a valid check (last) digit. The check digit should be 9";
			var error3 = "CAN must be 9 characters.";

			shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
			var entryNumberForBindingInfo = shipment.ShipmentCustomsEntryNumber.EntryNumberInfo;
			AssertHasMessageError(entryNumberForBindingInfo, error1);
			AssertHasMessageError(entryNumberForBindingInfo, error2);
			AssertHasMessageError(entryNumberForBindingInfo, error3);
		}

		public void TestCheckEntryNumber_WhenCoLoadMasterHasCAN()
		{
			ForwardingShipment master = Factory.New<ForwardingShipment>();
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = master.PK;

			ForwardingShipmentCustomsEntryNumber masterCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(master);
			ForwardingShipmentCustomsEntryNumber shipmentCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment);

			masterCustomsEntryNumber.EntryType = "CAN";
			masterCustomsEntryNumber.EntryNumber = "AAAAJH4EF";

			shipmentCustomsEntryNumber.EntryType = "CAN";
			shipmentCustomsEntryNumber.EntryNumber = "";

			AssertNoMessageError(shipmentCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			shipmentCustomsEntryNumber.EntryNumber = "1";
			AssertHasMessageError(shipmentCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
		}

		public void TestCheckEntryNumber_WhenCoLoadMasterHasNoCAN()
		{
			ForwardingShipment master = Factory.New<ForwardingShipment>();
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = master.PK;

			ForwardingShipmentCustomsEntryNumber masterCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(master);
			ForwardingShipmentCustomsEntryNumber shipmentCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment);

			masterCustomsEntryNumber.EntryType = "CAN";
			masterCustomsEntryNumber.EntryNumber = "";

			shipmentCustomsEntryNumber.EntryType = "CAN";
			shipmentCustomsEntryNumber.EntryNumber = "";

			AssertHasMessageError(shipmentCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
		}

		public void TestCheckEntryNumber_WhenCoLoadsHaveCAN()
		{
			ForwardingShipment master = Factory.New<ForwardingShipment>();
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = master.PK;

			ForwardingShipmentCustomsEntryNumber masterCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(master);
			ForwardingShipmentCustomsEntryNumber shipmentCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment);

			shipmentCustomsEntryNumber.EntryType = "CAN";
			shipmentCustomsEntryNumber.EntryNumber = "AAAAJH4EF";

			masterCustomsEntryNumber.EntryType = "CAN";
			masterCustomsEntryNumber.EntryNumber = "";

			AssertNoMessageError(masterCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			masterCustomsEntryNumber.EntryNumber = "1";
			AssertHasMessageError(masterCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
		}

		public void TestCheckEntryNumber_WhenOneCoLoadHasNoCAN()
		{
			ForwardingShipment master = Factory.New<ForwardingShipment>();
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_JS_ColoadMasterShipment = master.PK;

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_JS_ColoadMasterShipment = master.PK;

			ForwardingShipmentCustomsEntryNumber masterCustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(master);
			ForwardingShipmentCustomsEntryNumber shipment1CustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment1);
			ForwardingShipmentCustomsEntryNumber shipment2CustomsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment2);

			shipment1CustomsEntryNumber.EntryType = "CAN";
			shipment1CustomsEntryNumber.EntryNumber = "AAAAJH4EF";

			shipment2CustomsEntryNumber.EntryType = "CAN";
			shipment2CustomsEntryNumber.EntryNumber = "";

			masterCustomsEntryNumber.EntryType = "CAN";
			masterCustomsEntryNumber.EntryNumber = "";

			AssertHasMessageError(masterCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			shipment2.CustomsEntryNumber = "AAAAJH4EF";
			masterCustomsEntryNumber.Validation.ValidateEntryNumber();
			AssertNoMessageError(masterCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");

			masterCustomsEntryNumber.EntryNumber = "1";
			AssertHasMessageError(masterCustomsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
		}

		public void TestCheckEntryNumber_MRN_ZA()
		{
			var error = Customs.Common.ZA.ZAValidationConstants.InvalidMRNLength;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var cusEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
				cusEntryNumber.EntryType = "MRN";
				CombineAssertions(() =>
				{
					cusEntryNumber.EntryNumber = "XXX";
					AssertHasMessageError("length < 18", cusEntryNumber.EntryNumberInfo, error);

					cusEntryNumber.EntryNumber = ZString.Empty.PadLeft(18, 'X');
					AssertNoMessageError("length = 18", cusEntryNumber.EntryNumberInfo, error);

					cusEntryNumber.EntryNumber = ZString.Empty.PadLeft(19, 'X');
					AssertNoMessageError("length > 18", cusEntryNumber.EntryNumberInfo, error);
				});
			}
		}

		public void TestCheckEntryNumber_ITN_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USORF";
				shipment.JS_RL_NKDestination = "CNYTN";

				var error = "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";
				var errorForMissinig = MandatoryValidation.YouHaveNotEnteredMessage("ITN");

				var customsEntryNumber = new ForwardingShipmentCustomsEntryNumber(shipment);
				customsEntryNumber.EntryType = CusEntryNumberTypes.UnitedStates.ITN;
				AssertHasMessageError(customsEntryNumber.EntryNumberInfo, errorForMissinig);

				customsEntryNumber.EntryNumber = "69BR5689541979";
				AssertHasMessageError(customsEntryNumber.EntryNumberInfo, error);

				customsEntryNumber.EntryNumber = "X20240230123456";
				AssertHasMessageError(customsEntryNumber.EntryNumberInfo, error);

				customsEntryNumber.EntryNumber = "X20240229123456";
				AssertNoMessageError(customsEntryNumber.EntryNumberInfo, error);

				customsEntryNumber.EntryNumber = "X20230229123456";
				AssertHasMessageError(customsEntryNumber.EntryNumberInfo, error);

				shipment.DocsAndCartage.JP_ExportStatement = "LOW";
				customsEntryNumber.EntryNumber = "";
				AssertNoMessageErrors(customsEntryNumber.EntryNumberInfo);

				shipment.DocsAndCartage.JP_ExportStatement = "";
				customsEntryNumber.EntryNumber = "NO EEI §30.37(a)";
				AssertNoMessageErrors(customsEntryNumber.EntryNumberInfo);

				shipment.JS_RL_NKDestination = "USLAX";
				shipment.IsDomesticFreight = true;
				customsEntryNumber.EntryNumber = "69BR5689541979";
				AssertNoMessageErrors(customsEntryNumber.EntryNumberInfo);

				customsEntryNumber.EntryNumber = "";
				AssertNoMessageErrors(customsEntryNumber.EntryNumberInfo);
			}
		}

		public void TestMRNWithPackLineExportRefNumber()
		{
			var customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			var shipment = customsEntryNumber.Shipment;
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			var expectedWarning = "Another Export Reference Number exists on a pack line level. This shipment level MRN will only apply to packs that have no MRN entered. If you are planning on obtaining different MRN numbers for the remaining packs, please remove MRN from the shipment level.";

			customsEntryNumber.EntryType = "MRN";

			customsEntryNumber.EntryNumber = "M12345";
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);

			packLine1.JL_ExportRefNumber = "M001";
			packLine2.JL_ExportRefNumber = string.Empty;
			customsEntryNumber.Validation.ValidateEntryNumber();
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);

			packLine1.JL_ExportRefNumber = string.Empty;
			packLine2.JL_ExportRefNumber = "M002";
			customsEntryNumber.Validation.ValidateEntryNumber();
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);

			customsEntryNumber.EntryNumber = string.Empty;
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);

			customsEntryNumber.EntryType = "CAN";

			customsEntryNumber.EntryNumber = "M12345";
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);

			customsEntryNumber.EntryNumber = string.Empty;
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, expectedWarning);
		}

		public void TestMRNNumberFormatWarning()
		{
			var cusEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			var shipment = cusEntryNumber.Shipment;

			var error1 = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";

			var error2 = "MRN does not have a valid check (last) digit. The check digit should be 9";

			cusEntryNumber.EntryType = "MRN";
			cusEntryNumber.EntryNumber = "Xylotrupes gideon";
			AssertHasMessageError(cusEntryNumber.EntryNumberInfo, error1);

			cusEntryNumber.EntryNumber = "0123456";
			AssertHasMessageError(cusEntryNumber.EntryNumberInfo, error1);

			cusEntryNumber.EntryNumber = "10AD00000117716870";
			AssertHasMessageError(cusEntryNumber.EntryNumberInfo, error2);

			cusEntryNumber.EntryNumber = "10BE10100016194298";
			AssertNoMessageError(cusEntryNumber.EntryNumberInfo, error1);
			AssertNoMessageError(cusEntryNumber.EntryNumberInfo, error2);
		}

		public void TestMRNNumberFormatWarningGermany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var cusEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
				var shipment = cusEntryNumber.Shipment;

				var error1 =
					@"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";

				cusEntryNumber.EntryType = "MRN";
				cusEntryNumber.EntryNumber = "ATCXylotrupes gideon";
				AssertNoMessageError("DE AT", cusEntryNumber.EntryNumberInfo, error1);

				cusEntryNumber.EntryNumber = "Xylotrupes gideon";
				AssertHasMessageError("DE non AT", cusEntryNumber.EntryNumberInfo, error1);
			}
		}

		protected override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new ForwardingShipmentCustomsEntryNumber(Factory.New<ForwardingShipment>());
		}

		public void TestGermanLRNToLong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
				var shipment = customsEntryNumber.Shipment;

				var expectedError = "Local Reference Number has a maximum limit of 22 characters.";

				customsEntryNumber.EntryType = "LRN";
				customsEntryNumber.EntryNumber = "LRN123";

				AssertNoError(customsEntryNumber.EntryNumberInfo, expectedError);

				customsEntryNumber.EntryNumber = string.Empty;
				AssertNoError(customsEntryNumber.EntryNumberInfo, expectedError);

				customsEntryNumber.EntryNumber = "LRN123456789012345678901234567890";
				AssertHasError(customsEntryNumber.EntryNumberInfo, expectedError);
			}
		}
	}
}
