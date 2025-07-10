using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Business.Testing
{
	[TestedType(typeof(QuickPOD))]
	public class QuickPODBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			return new QuickPOD(quickPODs);
		}

		#endregion
	}
}
