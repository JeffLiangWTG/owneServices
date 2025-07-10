using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business.Testing
{
	abstract class GlbCompanyCampaignSubscriptionCollectionTestBase<TCollection> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TCollection : class, IGlbCompanyCampaignSubscriptionCollection
	{
		public abstract void TestDeleteAll();

		public virtual void TestValidateAllMembersEmpty()
		{
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);
			collection.ValidateAllMembers();
			AssertEquals(0, collection.Count);
		}

		public virtual void TestValidateAllMembers()
		{
			var collection = GetCollectionToTest();
			var subscriptionsTuple = CreateBusinessObjectsToValidate(collection);

			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item1);
			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item2);
			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item3);

			AssertEquals(3, collection.Count);
			collection.ValidateAllMembers();

			AssertHasRowError("Subscription is NOT valid, should have errors", subscriptionsTuple.Item1, "Subscription must have unique set of (Media Category and Media Type).");
			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item2);
			AssertHasRowError("Subscription is NOT valid, should have errors", subscriptionsTuple.Item3, "Subscription must have unique set of (Media Category and Media Type).");

			subscriptionsTuple.Item3.Delete();

			AssertEquals(2, collection.Count);
			collection.ValidateAllMembers();

			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item1);
			AssertNoRowErrors("Campaign Category is valid, should not have errors", subscriptionsTuple.Item2);
			AssertEquals(true, subscriptionsTuple.Item3.IsDeleted);
		}

		protected abstract Tuple<BusinessObject, BusinessObject, BusinessObject> CreateBusinessObjectsToValidate(TCollection collection);
	}
}
