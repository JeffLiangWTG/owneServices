using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentCustomsEntryNumber))]
	internal class AgencyShipmentCustomsEntryNumberTest : CommonShipmentCustomsEntryNumberTest
	{
		public override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new AgencyShipmentCustomsEntryNumber(Factory.New<AgencyShipment>());
		}

		public override void TestBizObjectFields()
		{
			base.TestBizObjectFieldsCore(GetNewShipmentCustomsEntryNumber());
		}

		public override void TestEntryType()
		{
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AgencyShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals("prerequisite", ZString.Empty, cusEntryNumber.CE_EntryType);
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			cusEntryNumber.CE_EntryType = "COC";
			AssertEquals("COC", customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, customsEntryNumber.EntryTypeInfo.ReadOnly);
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			customsEntryNumber.Reset();
			shipment.CusEntryNumbers.AddNew();
			AssertEquals(2, shipment.CusEntryNumbers.Count);
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
			AssertEquals(true, customsEntryNumber.EntryTypeInfo.ReadOnly);
		}

		public override void TestValidation()
		{
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AssertNotNull(customsEntryNumber.Validation);
			AssertEquals(typeof(AgencyShipmentCustomsEntryNumberValidation), customsEntryNumber.Validation.GetType());
		}

		public void TestClearEntryNumberWhenSettingEmptyEntryType()
		{
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AgencyShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_EntryType = "COC";
			cusEntryNumber.CE_EntryNum = "11111";
			AssertEquals("prerequisite", "COC", customsEntryNumber.EntryType);
			AssertEquals("prerequisite", "11111", customsEntryNumber.EntryNumber);
			customsEntryNumber.EntryType = ZString.Empty;
			AssertEquals(ZString.Empty, customsEntryNumber.EntryNumber);
		}

		public void TestClearEntryTypeWhenSettingEmptyEntryNumber()
		{
			AgencyShipmentCustomsEntryNumber customsEntryNumber = (AgencyShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AgencyShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_EntryType = "COC";
			cusEntryNumber.CE_EntryNum = "11111";
			AssertEquals("prerequisite", "COC", customsEntryNumber.EntryType);
			AssertEquals("prerequisite", "11111", customsEntryNumber.EntryNumber);
			customsEntryNumber.EntryNumber = ZString.Empty;
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
		}
	}
}
