using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDocumentsChargeOrder))]
	public class RatingDocumentsChargeOrderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAC_Desc()
		{
			var ratingDocumentsChargeOrder = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = ZGuid.Empty;
			AssertEquals("AC_Desc must be empty", ZString.Empty, ratingDocumentsChargeOrder.AC_Desc);
			var collection = new AccChargeCodeCollection(Factory, ((IBusinessObjectCollection)ratingDocumentsChargeOrder.Lookups.ChargeCodes).CompleteFilter);
			collection.Load();
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = collection[0].PK;
			AssertEquals("AC_Desc", collection[0].AC_Desc, ratingDocumentsChargeOrder.AC_Desc);

			var validCharges = new List<ZGuid>();
			foreach (AccChargeCode charge in collection)
			{
				validCharges.Add(charge.PK);
			}
			var invalidCharge = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, validCharges.ToArray()));
			AssertNotNull("Precondition: Should be at least one charge not in the Lookup list", invalidCharge);
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = invalidCharge.PK;

			AssertEquals("AC_Desc must be empty", ZString.Empty, ratingDocumentsChargeOrder.AC_Desc);
		}

		public void TestAC_PrintSequence()
		{
			var ratingDocumentsChargeOrder = Factory.New<RatingDocumentsChargeOrder>();
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = ZGuid.Empty;
			AssertEquals("AC_PrintSequence must be 0", ZShort.Zero, ratingDocumentsChargeOrder.AC_PrintSequence);
			var collection = new AccChargeCodeCollection(Factory, ((IBusinessObjectCollection)ratingDocumentsChargeOrder.Lookups.ChargeCodes).CompleteFilter);
			collection.Load();
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = collection[0].PK;
			AssertEquals("AC_PrintSequence", collection[0].AC_PrintSequence, ratingDocumentsChargeOrder.AC_PrintSequence);

			var validCharges = new List<ZGuid>();
			foreach (AccChargeCode charge in collection)
			{
				validCharges.Add(charge.PK);
			}
			var invalidCharge = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, validCharges.ToArray()));
			AssertNotNull("Precondition: Should be at least one charge not in the Lookup list", invalidCharge);
			ratingDocumentsChargeOrder.RCO_AC_ChargeCode = invalidCharge.PK;

			AssertEquals("AC_PrintSequence must be 0", ZShort.Zero, ratingDocumentsChargeOrder.AC_PrintSequence);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBusinessObjectForTest();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetBusinessObjectForTest();

		#region implementation

		RatingDocumentsChargeOrder GetBusinessObjectForTest()
		{
			var result = Factory.NewWithValidTestData<RatingDocumentsChargeOrder>();
			result.RCO_DocumentType = "ALL";
			result.RCO_PrintOrder = 1;
			return result;
		}

		protected RatingDocumentsChargeOrder RatingDocumentsChargeOrder;

		protected override void SetUp()
		{
			base.SetUp();
			RatingDocumentsChargeOrder = GetBusinessObjectForTest();
		}
		#endregion
	}
}
