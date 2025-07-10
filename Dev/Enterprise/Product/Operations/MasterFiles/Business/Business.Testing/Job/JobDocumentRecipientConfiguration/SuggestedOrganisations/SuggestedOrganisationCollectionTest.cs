using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SuggestedOrganisationCollection))]
	sealed class SuggestedOrganisationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SuggestedOrganisationCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowRemove);
		}

		public void TestReadOnly()
		{
			var collection = GetCollectionToTest();
			Assert(collection.ReadOnly);
		}

		public void TestAddNewThrowsException()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		#region Implementation

		protected override SuggestedOrganisationCollection GetCollectionToTest() => new SuggestedOrganisationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgHeader = Factory.New<OrgHeader>();
			return new SuggestedOrganisation((NoResString)"Consginee", orgHeader);
		}

		#endregion
	}
}
