using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSChargeCodeGroupCollection))]
	sealed class AccPOSChargeCodeGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			return new AccPOSChargeCodeGroupCollection(company);
		}

		#endregion
	}
}
