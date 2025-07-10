using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Business.Testing
{
	[TestedType(typeof(QuickPODs))]
	public class QuickPODsTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QuickPODs(Factory);
		}

		#endregion

		public void TestQuickPODCollection()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			AssertNotNull(quickPODs.QuickPODsCollection.AddNew());
		}
	}
}
