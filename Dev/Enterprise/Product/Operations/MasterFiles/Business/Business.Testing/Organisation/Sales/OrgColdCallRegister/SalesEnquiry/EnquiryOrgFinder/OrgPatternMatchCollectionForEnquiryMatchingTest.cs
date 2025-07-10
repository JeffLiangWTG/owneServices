using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchCollectionForEnquiryMatching))]
	sealed class OrgPatternMatchCollectionForEnquiryMatchingTest : BusinessObjectCollectionTestCase
	{
		public void TestFindingSimilarOrganisations_ShouldReturnMatchingOrganisationsIrrespectiveOfUnloco()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";
			johnSmithApples1.MainAddress.OA_Email = "Email@test.com";

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JAHN SMYTHE APLES";
			johnSmithApples2.OH_RL_NKClosestPort = "SGSIN";
			johnSmithApples2.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples2.MainAddress.OA_City = "Appleville";

			OrgHeader johnSmithApples3 = OrgHeader.New(Factory);
			johnSmithApples3.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples3.OH_RL_NKClosestPort = "YYYYY";
			johnSmithApples3.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples3.MainAddress.OA_City = "Appleville";

			Factory.Save(); // Gen Pattern Matches etc.

			OrgHeader johnSmithApples4 = Factory.New<OrgHeaderWithOrgPatternMatchCollectionForEnquiryMatching>();
			johnSmithApples4.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples4.OH_RL_NKClosestPort = "XXXXX";
			johnSmithApples4.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples4.MainAddress.OA_City = "Appleville";
			johnSmithApples4.MainAddress.OA_Email = "Email@test.com";

			johnSmithApples4.SimilarOrgFinder.FindSimilarOrganisations(new ZQuery(), false, false);
			AssertContainsExactElementsInAnyOrder("There are two similar orgs", new[] { johnSmithApples1.PK, johnSmithApples3.PK }, johnSmithApples4.SimilarOrgMatches.Cast<OrgPatternMatch>().Select(match => match.Header.PK));
		}

		public void TestFindingSimilarOrganisations_ShouldReturnOrganisationsWhereBusinessRegNoIsTheSameOrAnotherValueIsTheSame()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.PrimaryRegistrationNumber.Number = "123 123 123 12";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";
			johnSmithApples1.MainAddress.OA_Email = "Email@test.com";

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JAHN SMYTHE APLES";
			johnSmithApples2.PrimaryRegistrationNumber.Number = "9";
			johnSmithApples2.OH_RL_NKClosestPort = "AUMEL";
			johnSmithApples2.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples2.MainAddress.OA_City = "Aplevile";

			OrgHeader johnSmithApples3 = OrgHeader.New(Factory);
			johnSmithApples3.OH_FullName = "JAHN SMYTHE APLES";
			johnSmithApples3.PrimaryRegistrationNumber.Number = "123 123 123 12";
			johnSmithApples3.OH_RL_NKClosestPort = "AUMEL";
			johnSmithApples3.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples3.MainAddress.OA_City = "Aplevile";

			Factory.Save(); // Gen Pattern Matches etc.

			// EVERYTHING exact match except Business Reg No
			OrgHeader johnSmithApples4 = Factory.New<OrgHeaderWithOrgPatternMatchCollectionForEnquiryMatching>();
			johnSmithApples4.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples4.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples4.PrimaryRegistrationNumber.Number = "9";
			johnSmithApples4.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples4.MainAddress.OA_City = "Appleville";
			johnSmithApples4.MainAddress.OA_Email = "Email@test.com";

			johnSmithApples4.SimilarOrgFinder.FindSimilarOrganisations(new ZQuery(), false, false);
			AssertContainsExactElementsInAnyOrder("There are two similar orgs", new[] { johnSmithApples1.PK, johnSmithApples2.PK }, johnSmithApples4.SimilarOrgMatches.Cast<OrgPatternMatch>().Select(match => match.Header.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgPatternMatchCollectionForEnquiryMatching(Factory, new ZQuery());
		}

		protected override void SetUp()
		{
			base.SetUp();

			OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgMatchThresholds.Codes.Low);
		}

		#region Classes

		class OrgHeaderWithOrgPatternMatchCollectionForEnquiryMatching : OrgHeader
		{
			public OrgHeaderWithOrgPatternMatchCollectionForEnquiryMatching(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override OrgPatternMatchCollection GetNewPatternMatchesForThisOrgCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			{
				return new OrgPatternMatchCollectionForEnquiryMatching(factory, additionalFilter);
			}
		}

		#endregion

		#endregion
	}
}
