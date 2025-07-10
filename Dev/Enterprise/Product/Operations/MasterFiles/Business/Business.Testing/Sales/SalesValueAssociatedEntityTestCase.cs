using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(ISalesValueAssociatedEntity), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class SalesValueAssociatedEntityTestCase : TestCaseWithFactory
	{
		public void TestSalesHeaderCollectionIsNotChildEditable()
		{
			var entity = GetNewEntity();
			AssertEquals("Should not be child editable by default. Should only be added as child editable on edit forms", false, ((BusinessObject)entity).IsRegisteredEditableChildObject(entity.ActualAndProspectiveSalesHeaderCollection));
		}

		public void TestProspectiveSalesHeaderCollectionIsNotChildEditable()
		{
			var entity = GetNewEntity();
			AssertEquals("Should not be child editable by default. Should only be added as child editable on edit forms", false, ((BusinessObject)entity).IsRegisteredEditableChildObject(entity.ProspectiveSalesHeaderCollection));
		}

		protected abstract ISalesValueAssociatedEntity GetNewEntity();
	}
}
