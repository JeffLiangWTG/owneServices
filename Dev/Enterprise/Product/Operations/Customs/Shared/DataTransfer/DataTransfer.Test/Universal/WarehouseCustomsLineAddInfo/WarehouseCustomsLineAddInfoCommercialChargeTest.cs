using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineAddInfoCommercialChargeTest : TestCase
	{
		public void TestWarehouseCustomsLineAddInfoMembers()
		{
			var charge = new CommercialCharge()
			{
				ChargeType = new CodeDescriptionPair() { Code = "ONS" },
				Amount = new ZDecimal(1500.51m),
				Currency = new Currency() { Code = Core.Constants.CurrencyCodes.NewZealand },
				IsDutiable = ZBool.True,
				IsGSTApplicable = ZBool.False,
				IsIncludedInITOT = ZBool.True,
				IsStatisticalValueApplicable = ZBool.True,
				PrepaidCollect = new CodeDescriptionPair() { Code = "PPC" }
			};

			IWarehouseCustomsLineAddInfo lineAddInfo = new WarehouseCustomsLineAddInfoCommercialCharge(charge);
			AssertEquals("Type", "CCT", lineAddInfo.Type);
			AssertEquals("AddInfoData", "Amount=1500.51*ChargeType=ONS*Currency=NZD*IsDutiable=Y*IsGSTApplicable=*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y", lineAddInfo.AddInfoData);
			AssertEquals("NAddInfoData", ZString.Empty, lineAddInfo.NAddInfoData);

			charge.ChargeType = new CodeDescriptionPair() { Code = "OFT" };
			charge.Amount = new ZDecimal(453.45m);
			charge.Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			charge.IsDutiable = ZBool.False;
			charge.IsGSTApplicable = ZBool.True;
			charge.IsIncludedInITOT = ZBool.False;
			charge.IsStatisticalValueApplicable = ZBool.False;
			charge.PrepaidCollect = new CodeDescriptionPair() { Code = "PPC" };

			lineAddInfo = new WarehouseCustomsLineAddInfoCommercialCharge(charge);
			AssertEquals("Type", "CCT", lineAddInfo.Type);
			AssertEquals("AddInfoData", "Amount=453.45*ChargeType=OFT*Currency=AUD*IsDutiable=*IsGSTApplicable=Y*IsIncludedInITOT=*IsStatisticalValueApplicable=", lineAddInfo.AddInfoData);
			AssertEquals("NAddInfoData", ZString.Empty, lineAddInfo.NAddInfoData);
		}
	}
}
