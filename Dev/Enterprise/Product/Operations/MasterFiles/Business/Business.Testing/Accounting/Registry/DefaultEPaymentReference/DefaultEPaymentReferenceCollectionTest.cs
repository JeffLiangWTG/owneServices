using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReferenceCollection))]
	sealed class DefaultEPaymentReferenceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultEPaymentReferenceCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, collection.AllowRemove);
		}

		/* This function is used to test if the default values are set for payment reference in the registry.
		 * The default values are: Provider-OFX , Reference Type- INV
		 */
		public void TestPopulateDefaultPaymentReferenceForAllProviders()
		{
			var collection = new DefaultEPaymentReferenceCollection();
			AssertEquals(0, collection.Count);
			collection.PopulateDefaultPaymentReferenceForAllProviders();
			AssertEquals(1, collection.Count);
			AssertEquals(collection[0].ProviderCode, EPaymentProviderCodes.Codes.OFX);
			AssertEquals(collection[0].ReferenceType, "INV");
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override DefaultEPaymentReferenceCollection GetCollectionToTest() => new DefaultEPaymentReferenceCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DefaultEPaymentReference();
	}
}
