using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReasonCollection))]
	sealed class DefaultEPaymentReasonCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultEPaymentReasonCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		public void TestAllowNew()
		{
			var collection = new DefaultEPaymentReasonCollection();
			Assert("Users should be able to add new items to the collection.", collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new DefaultEPaymentReasonCollection();
			Assert("Users should be able to remove items from the collection.", collection.AllowRemove);
		}

		public void TestPopulateDefaultPaymentReasonForAllProviders()
		{
			var collection = new DefaultEPaymentReasonCollection();
			AssertEquals(0, collection.Count);
			collection.PopulateDefaultPaymentReasonsForAllProviders();
			AssertEquals(1, collection.Count);
			AssertNotNull(collection.Cast<DefaultEPaymentReason>().FirstOrDefault(r => r.ProviderCode == EPaymentProviderCodes.Codes.OFX && r.ReasonCode == "SVT" && r.ReasonDescription == "Services trade"));
		}

		#region Implementation

		protected override DefaultEPaymentReasonCollection GetCollectionToTest() => new DefaultEPaymentReasonCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DefaultEPaymentReason();

		#endregion

	}
}
