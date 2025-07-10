using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DeclarationOfValueProviderTest : TestCaseWithFactory
	{
		public void TestCusDeclarationOfValueMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var declarationOfValue = declaration.DeclarationOfValue.ToArray();

				CombineAssertions("Declaration Value Test 1", () =>
				{
					AssertEquals("TypeOfDelivery", "FOB", declarationOfValue[0].TypeOfDelivery);
					AssertEquals("InvoiceDateAndNumber", "30/03/2021 5435345345", declarationOfValue[0].InvoiceDateAndNumber);
					AssertEquals("AgreementDateAndNumber", "13/04/2021 sozlesmeNo", declarationOfValue[0].AgreementDateAndNumber);
					AssertEquals("DecisionOfCustomsOffice", "kararNo 21/04/2021", declarationOfValue[0].DecisionOfCustomsOffice);
					AssertEquals("ConsigneeAndSeller", "VAR", declarationOfValue[0].ConsigneeAndSeller);
					AssertEquals("Relationship", "EVET", declarationOfValue[0].Relationship);
					AssertEquals("Similars", "EVET", declarationOfValue[0].Similars);
					AssertEquals("ConsigneeAndSellerDetails", "Alıcı Satıcı açıklaması", declarationOfValue[0].ConsigneeAndSellerDetails);
					AssertEquals("Restrictions", "EVET", declarationOfValue[0].Restrictions);
					AssertEquals("Act", "EVET", declarationOfValue[0].Act);
					AssertEquals("RestrictionsDetails", "Kısıtlamalar ayrıntıları", declarationOfValue[0].RestrictionsDetails);
					AssertEquals("Royalty", "EVET", declarationOfValue[0].Royalty);
					AssertEquals("RoyaltyConditions", "Koşullar", declarationOfValue[0].RoyaltyConditions);
					AssertEquals("TransitionToSeller", "EVET", declarationOfValue[0].TransitionToSeller);
					AssertEquals("TransitionToSellerConditions", "Koşullar b", declarationOfValue[0].TransitionToSellerConditions);
					AssertEquals("CityPlace", "İSTANBUL", declarationOfValue[0].CityPlace);
					AssertEquals("WrittenContract", "EVET", declarationOfValue[0].WrittenContract);
				});

				var dv1Details = (CusDV1Detail)headerJobDeclaration.DV1Details.FirstOrDefault();
				dv1Details.DV1_Relationship = "N";
				dv1Details.DV1_PriceInfluence = "N";
				dv1Details.DV1_CloseApproximation = "N";
				dv1Details.DV1_Restrictions = "N";
				dv1Details.DV1_Consideration = "N";
				dv1Details.DV1_RoyaltiesLicence = "N";
				dv1Details.DV1_Resale = "N";

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				declarationOfValue = declaration.DeclarationOfValue.ToArray();

				CombineAssertions("Declaration Value Test 2", () =>
				{
					AssertEquals("ConsigneeAndSeller", "YOK", declarationOfValue[0].ConsigneeAndSeller);
					AssertEquals("Relationship", "HAYIR", declarationOfValue[0].Relationship);
					AssertEquals("Similars", "HAYIR", declarationOfValue[0].Similars);
					AssertEquals("ConsigneeAndSellerDetails", string.Empty, declarationOfValue[0].ConsigneeAndSellerDetails);
					AssertEquals("Restrictions", "HAYIR", declarationOfValue[0].Restrictions);
					AssertEquals("Act", "HAYIR", declarationOfValue[0].Act);
					AssertEquals("RestrictionsDetails", string.Empty, declarationOfValue[0].RestrictionsDetails);
					AssertEquals("Royalty", "HAYIR", declarationOfValue[0].Royalty);
					AssertEquals("RoyaltyConditions", string.Empty, declarationOfValue[0].RoyaltyConditions);
					AssertEquals("TransitionToSeller", "HAYIR", declarationOfValue[0].TransitionToSeller);
					AssertEquals("TransitionToSellerConditions", string.Empty, declarationOfValue[0].TransitionToSellerConditions);
				});
			}
		}
	}
}
