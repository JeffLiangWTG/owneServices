using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CardTypeDeciderTest : TestCase
	{
		public void TestIdentifyCardType()
		{
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("1234567890097846"));
			AssertEquals(CreditCardTypeList.Codes.AmExpress, CardTypeDecider.IdentifyCardType("3434567890097846"));
			AssertEquals(CreditCardTypeList.Codes.AmExpress, CardTypeDecider.IdentifyCardType("3734567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Bankcard, CardTypeDecider.IdentifyCardType("5610567890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("5612567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Bankcard, CardTypeDecider.IdentifyCardType("5602217890097846"));
			AssertEquals(CreditCardTypeList.Codes.Bankcard, CardTypeDecider.IdentifyCardType("5602237890097846"));
			AssertEquals(CreditCardTypeList.Codes.Bankcard, CardTypeDecider.IdentifyCardType("5602257890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("5602267890097846"));
			AssertEquals(CreditCardTypeList.Codes.ChinaUnionPay, CardTypeDecider.IdentifyCardType("622126678900097846"));
			AssertEquals(CreditCardTypeList.Codes.ChinaUnionPay, CardTypeDecider.IdentifyCardType("622726678900097846"));
			AssertEquals(CreditCardTypeList.Codes.ChinaUnionPay, CardTypeDecider.IdentifyCardType("622925678900907846"));
			AssertEquals(CreditCardTypeList.Codes.DinClub, CardTypeDecider.IdentifyCardType("3000057890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClub, CardTypeDecider.IdentifyCardType("3010057890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClub, CardTypeDecider.IdentifyCardType("3050057890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("3060057890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClubenRoute, CardTypeDecider.IdentifyCardType("2014057890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClubenRoute, CardTypeDecider.IdentifyCardType("2149057890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClubInt, CardTypeDecider.IdentifyCardType("3634567890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClubUSCanada, CardTypeDecider.IdentifyCardType("5434567890097846"));
			AssertEquals(CreditCardTypeList.Codes.DinClubUSCanada, CardTypeDecider.IdentifyCardType("5534567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("6011256789009846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("6011356789009846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("6011456789009846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("6011556789007846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601174567890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("601175567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601177567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601178567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601179567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601186567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601196567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("601199567890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("6022125567890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("644212667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("649212667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Discover, CardTypeDecider.IdentifyCardType("652212667890097846"));
			AssertEquals(CreditCardTypeList.Codes.JCB, CardTypeDecider.IdentifyCardType("352812667890097846"));
			AssertEquals(CreditCardTypeList.Codes.JCB, CardTypeDecider.IdentifyCardType("358812667890097846"));
			AssertEquals(CreditCardTypeList.Codes.JCB, CardTypeDecider.IdentifyCardType("358912667890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("350812667890097846"));
			AssertEquals(CreditCardTypeList.Codes.JCBobsolete, CardTypeDecider.IdentifyCardType("180012667890097846"));
			AssertEquals(CreditCardTypeList.Codes.JCBobsolete, CardTypeDecider.IdentifyCardType("213112667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Mastercard, CardTypeDecider.IdentifyCardType("513112667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Mastercard, CardTypeDecider.IdentifyCardType("523112667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Mastercard, CardTypeDecider.IdentifyCardType("533112667890097846"));
			AssertEquals(ZString.Empty, CardTypeDecider.IdentifyCardType("563112667890097846"));
			AssertEquals(CreditCardTypeList.Codes.Visa, CardTypeDecider.IdentifyCardType("413112667890097846"));
			AssertEquals(CreditCardTypeList.Codes.VisaEl, CardTypeDecider.IdentifyCardType("417500667890097846"));
			AssertEquals(CreditCardTypeList.Codes.VisaEl, CardTypeDecider.IdentifyCardType("491712667890097846"));
			AssertEquals(CreditCardTypeList.Codes.VisaEl, CardTypeDecider.IdentifyCardType("491312667890097846"));
			AssertEquals(CreditCardTypeList.Codes.VisaEl, CardTypeDecider.IdentifyCardType("450812667890097846"));
			AssertEquals(CreditCardTypeList.Codes.VisaEl, CardTypeDecider.IdentifyCardType("484412667890097846"));
		}
	}
}
