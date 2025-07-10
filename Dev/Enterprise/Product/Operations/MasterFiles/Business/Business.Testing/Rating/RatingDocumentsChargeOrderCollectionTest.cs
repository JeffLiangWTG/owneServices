using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDocumentsChargeOrderCollection))]
	public class RatingDocumentsChargeOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<RatingDocumentsChargeOrderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<RatingDocumentsChargeOrder>();
			result.RCO_OH_Client = Organisation.PK;
			result.RCO_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			return result;
		}

		protected override RatingDocumentsChargeOrderCollection GetCollectionToTest()
		{
			return new RatingDocumentsChargeOrderCollection(Factory, Organisation);
		}

		public void TestDefaultsForChildren()
		{
			var ratingDocumentsChargeOrder = GetCollectionToTest().AddNew();
			AssertEquals("AI_OH_Client", Organisation.PK, ratingDocumentsChargeOrder.RCO_OH_Client);
		}

		public void TestFilterForOrganisationAndCurrentCompany()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST1";
			var ratingDocumentsChargeOrder1 = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder1.RCO_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			ratingDocumentsChargeOrder1.RCO_OH_Client = organisation.PK;
			ratingDocumentsChargeOrder1.RCO_DocumentType = "ALL";
			ratingDocumentsChargeOrder1.RCO_PrintOrder = 1;
			organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST2";
			var ratingDocumentsChargeOrder2 = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder2.RCO_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			ratingDocumentsChargeOrder2.RCO_OH_Client = organisation.PK;
			ratingDocumentsChargeOrder2.RCO_DocumentType = "ALL";
			ratingDocumentsChargeOrder2.RCO_PrintOrder = 1;
			var ratingDocumentsChargeOrder3 = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder3.RCO_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			ratingDocumentsChargeOrder3.RCO_OH_Client = organisation.PK;
			ratingDocumentsChargeOrder3.RCO_DocumentType = "ALL";
			ratingDocumentsChargeOrder3.RCO_PrintOrder = 1;
			Factory.Save();
			var clientRatingDocumentsOrderCollection = new RatingDocumentsChargeOrderCollection(Factory, organisation);
			AssertEquals("Must be 1", 1, clientRatingDocumentsOrderCollection.Count);
		}

		public void TestAllowsMissingChargeCodes()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDocumentsChargeOrder = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder.RCO_OH_Client = organisation.PK;
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = ZGuid.Missing;

			var clientRatingDocumentsOrderCollection = new RatingDocumentsChargeOrderCollection(Factory, organisation);
			AssertEquals("Count of elements", 1, clientRatingDocumentsOrderCollection.Count);
			AssertCollectionContains(ratingDocumentsChargeOrder, clientRatingDocumentsOrderCollection);

			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = ZGuid.Invalid;
			clientRatingDocumentsOrderCollection = new RatingDocumentsChargeOrderCollection(Factory, organisation);
			AssertEquals("Count of elements", 1, clientRatingDocumentsOrderCollection.Count);
			AssertCollectionContains(ratingDocumentsChargeOrder, clientRatingDocumentsOrderCollection);
		}

		#region Implementation

		protected OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.OH_Code = "TESTORG";
				}
				return fOrganisation;
			}
		}

		OrgHeader fOrganisation;

		#endregion
	}
}
