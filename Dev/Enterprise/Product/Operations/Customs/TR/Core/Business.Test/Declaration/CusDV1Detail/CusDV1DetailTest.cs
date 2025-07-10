using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusDV1Detail))]
	sealed class CusDV1DetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFieldsMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DV1_ContractNumberInfoMaxLength", 20, dv1Detail.DV1_ContractNumberInfo.MaxLength);
				AssertEquals("DV1_CloseApproximationInfoMaxLength", 1, dv1Detail.DV1_CloseApproximationInfo.MaxLength);
				AssertEquals("DV1_PlaceInfo.MaxLength", 50, dv1Detail.DV1_PlaceInfo.MaxLength);
				AssertEquals("DV1_CustomsDecisionNumberInfoMaxLength", 100, dv1Detail.DV1_CustomsDecisionNumberInfo.MaxLength);
			});
		}

		public void TestTRSetDefaultValues()
		{
			AssertEquals("DV1_CloseApproximation Default", YesNoList.Codes.No, dv1Detail.DV1_CloseApproximation);
		}

		public void TestDV1_CloseApproximation_SetBlank()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertEquals("N", dv1Detail.DV1_CloseApproximation);
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DV1_ContractNumber Caption", "Contract Number", dv1Detail.DV1_ContractNumberInfo.Description);
				AssertEquals("DV1_ContractDate Caption", "Contract Date", dv1Detail.DV1_ContractDateInfo.Description);
				AssertEquals("DV1_Relationship Caption", "Relationship", dv1Detail.DV1_RelationshipInfo.Description);
				AssertEquals("DV1_PriceInfluence Caption", "Price influenced?", dv1Detail.DV1_PriceInfluenceInfo.Description);
				AssertEquals("DV1_CloseApproximation Caption", "Close Approximation", dv1Detail.DV1_CloseApproximationInfo.Description);
				AssertEquals("DV1_RelationDetails Caption", "Relation Details", dv1Detail.DV1_RelationDetailsInfo.Description);
				AssertEquals("DV1_Restrictions Caption", "Restrictions", dv1Detail.DV1_RestrictionsInfo.Description);
				AssertEquals("DV1_Consideration Caption", "Conditions", dv1Detail.DV1_ConsiderationInfo.Description);
				AssertEquals("DV1_RestrictionConsiderationDetails Caption", "Restriction Details", dv1Detail.DV1_RestrictionConsiderationDetailsInfo.Description);
				AssertEquals("DV1_RoyaltiesLicence Caption", "License Fees", dv1Detail.DV1_RoyaltiesLicenceInfo.Description);
				AssertEquals("DV1_RoyaltiesLicenceDetails Caption", "License Details", dv1Detail.DV1_RoyaltiesLicenceDetailsInfo.Description);
				AssertEquals("DV1_Resale Caption", "Resale", dv1Detail.DV1_ResaleInfo.Description);
				AssertEquals("DV1_ResaleDetails Caption", "Resale Details", dv1Detail.DV1_ResaleDetailsInfo.Description);
				AssertEquals("DV1_Place Caption", "Place", dv1Detail.DV1_PlaceInfo.Description);
				AssertEquals("DV1_CustomsDecisionNumber Caption", "Customs Decision Number", dv1Detail.DV1_CustomsDecisionNumberInfo.Description);
				AssertEquals("DV1_CustomsDecisionDate Caption", "Customs Decision Date", dv1Detail.DV1_CustomsDecisionDateInfo.Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			dv1Detail = declaration.DV1Details.AddNew();
		}
		CusDV1Detail dv1Detail;
		JobDeclaration declaration;
	}
}
