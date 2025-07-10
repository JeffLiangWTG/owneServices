using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemporaryOrganisationCreatorTest : TestCaseWithDummy
	{
		class TestCollection : OrgHeaderCollection, IValidateForController
		{
			public bool MarkErrors;
			public bool SetDefaultsCalled;

			public TestCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new OrgHeader this[int index]
			{
				get { throw new Exception("What do you think you're doing?"); }
			}

			protected override void SetDefaultsForNewChild(BusinessObject child)
			{
				base.SetDefaultsForNewChild(child);
				var orgHeader = (OrgHeader)child;
				orgHeader.OH_IsConsignor = true;
				SetDefaultsCalled = true;
			}

			public override bool AllowNewTemporaryOrganisations
			{
				get { return true; }
			}

			#region IValidateForController Members

			public void ValidateEntityOnSaving(IBusiness entity)
			{
				if (MarkErrors)
				{
					((BusinessObject)entity).AddRowError("Test error");
				}
			}

			#endregion

		}

		public void TestGetTemporaryOrganisationType()
		{
			var collection = new TestCollection(Factory);
			var tempOrg = (OrgHeader)TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, collection);
			AssertNotNull("Temporary organisation should be loaded with reflection", tempOrg);
			Assert("SetDefaultsForNewChild should be called for new object", collection.SetDefaultsCalled);

			collection.ValidateEntityOnSaving(tempOrg);
			Assert("No errors should be created", !tempOrg.HasErrors);

			collection.MarkErrors = true;
			collection.ValidateEntityOnSaving(tempOrg);
			Assert("Errors should be created", tempOrg.HasErrors());
		}
	}
}
