using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	class MessageBuilderTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			sendTestMessagesToCustoms = false;
			var carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "RWRS"));
			if (carrier == null)
			{
				carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = "RWRS";
				carrier.UI_Name = "ACE M1 TEST CARRIER";
			}

			carrier.UI_ModeOfTransportation = "20";
			var carrier2 = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "XXXS"));
			if (carrier2 == null)
			{
				carrier2 = Factory.New<USCarrierCombined>();
				carrier2.UI_Code = "XXXS";
				carrier2.UI_Name = "ACE M1 TEST CARRIER 2";
			}

			carrier2.UI_ModeOfTransportation = "10";
			var carrier3 = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "XXXY"));
			if (carrier3 == null)
			{
				carrier3 = Factory.New<USCarrierCombined>();
				carrier3.UI_Code = "XXXY";
				carrier3.UI_Name = "ACE M1 TEST CARRIER 3";
			}

			carrier3.UI_ModeOfTransportation = "70";
			var carrier4 = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "XXXW"));
			if (carrier4 == null)
			{
				carrier4 = Factory.New<USCarrierCombined>();
				carrier4.UI_Code = "XXXW";
				carrier4.UI_Name = "ACE M1 TEST CARRIER 2";
			}

			carrier4.UI_ModeOfTransportation = "30";
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "00000002"));
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "00000002";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
				tariff.UE_Unit1 = "KG";
			}

			Factory.Save();
		}

		protected bool sendTestMessagesToCustoms;
	}
}
