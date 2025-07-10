using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentCustomsEntryNumberValidation))]
	internal class AgencyShipmentCustomsEntryNumberValidationTest : ShipmentCustomsEntryNumberValidationTest
	{
		public override void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentCustomsEntryNumberValidation(null));
			AssertNoExceptionThrown(() => new ShipmentCustomsEntryNumberValidation(new AgencyShipmentCustomsEntryNumber(Factory.New<AgencyShipment>())));
		}

		public override void TestCheckEntryType()
		{
			base.TestCheckEntryType();
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AgencyShipment shipment = customsEntryNumber.Shipment;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("prerequisite", 2, customsEntryNumber.EntryType_List.Count);
			customsEntryNumber.EntryType = "XRN";
			AssertHasError(customsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");
			customsEntryNumber.EntryType = ZString.Empty;
			AssertNoError(customsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");
		}

		public override void TestCheckEntryNumber()
		{
			base.TestCheckEntryType();
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AgencyShipment shipment = customsEntryNumber.Shipment;
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals("prerequisite", false, customsEntryNumber.EntryNumberInfo.ReadOnly);
			customsEntryNumber.EntryType = "COC";
			customsEntryNumber.EntryNumber = ZString.Empty;
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, "Entry Number is not specified");
			customsEntryNumber.EntryNumber = "11111";
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, "Entry Number is not specified");
			customsEntryNumber.EntryType = "CAN";
			customsEntryNumber.Validation.ValidateEntryNumber();
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
			customsEntryNumber.EntryNumber = "AAAAJH4EF";
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, "CAN must be 9 characters.");
			customsEntryNumber.EntryType = "CCN";
			customsEntryNumber.EntryNumber = "1234567890123456";
			AssertHasWarning(customsEntryNumber.EntryNumberInfo, "Entry Number is too long, only the first 15 characters will be transferred to the manifest.");
			customsEntryNumber.EntryNumber = "123456789012345";
			AssertNoWarning(customsEntryNumber.EntryNumberInfo, "Entry Number is too long, only the first 15 characters will be transferred to the manifest.");
		}

		protected override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			return new AgencyShipmentCustomsEntryNumber(shipment);
		}
	}
}
