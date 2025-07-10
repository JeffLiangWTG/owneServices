using ChargeTypes = Enterprise.Customs.PL.Business.Declaration.PLCustomsChargeTypeList.Codes;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ChargesProviderHelperTests : NUnit.Framework.TestCase
{
	public void TestIsListed()
	{
		var testList = new (string, string)[]
		{
			(string.Empty, ChargeTypes.AB),
			(IncoTerms.DeliveredAtPlace, ChargeTypes.AD),
			(IncoTerms.DeliveredAtTerminal, ChargeTypes.AD),
			(IncoTerms.DeliveredDutyPaid, ChargeTypes.AD),
			(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.AD),
			(IncoTerms.CostInsuranceAndFreight, ChargeTypes.AD),
		};

		Assert(testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.AB));
		Assert(testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.AB));
		Assert(testList.IsListed(string.Empty, ChargeTypes.AB));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.AD));
		Assert(testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.AD));
		Assert(!testList.IsListed(string.Empty, ChargeTypes.AD));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.AE));
		Assert(!testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.AE));
		Assert(!testList.IsListed(string.Empty, ChargeTypes.AE));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, string.Empty));
		Assert(!testList.IsListed(IncoTerms.DeliveredAtPlace, string.Empty));
		Assert(!testList.IsListed(string.Empty, string.Empty));
	}
}
