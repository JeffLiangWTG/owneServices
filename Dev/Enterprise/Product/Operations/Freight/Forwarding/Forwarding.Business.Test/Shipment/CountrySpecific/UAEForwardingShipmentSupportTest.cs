using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class UAEForwardingShipmentSupportTest : TestCaseWithFactory
	{
		CusEntryNumber GetUAEDeliveryOrderNumber(ForwardingShipment shipment)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.DeliveryOrderNumber);
			CusEntryNumber[] results = (CusEntryNumber[])shipment.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		public void TestUAEDeliveryOrderNumberGeneration()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			AssertNull(GetUAEDeliveryOrderNumber(shipment));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedArabEmirates);

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AEDXB";
			shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();
			AssertNull(GetUAEDeliveryOrderNumber(shipment));

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AEDXB";
			Factory.Save();
			AssertNull(GetUAEDeliveryOrderNumber(shipment));

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AEDXB";
			Factory.Save();
			CusEntryNumber numEntry = GetUAEDeliveryOrderNumber(shipment);
			AssertNotNull(numEntry);
			ZString num = numEntry.CE_EntryNum;

			ForwardingShipment loadedShipment = NewFactory().Load<ForwardingShipment>(shipment.PK);
			numEntry = GetUAEDeliveryOrderNumber(loadedShipment);
			AssertNotNull(numEntry);
			AssertEquals(num, numEntry.CE_EntryNum);
			loadedShipment.JS_GoodsDescription = "1231243";
			Factory.Save();
			numEntry = GetUAEDeliveryOrderNumber(loadedShipment);
			AssertNotNull(numEntry);
			AssertEquals(num, numEntry.CE_EntryNum);
		}
	}
}
