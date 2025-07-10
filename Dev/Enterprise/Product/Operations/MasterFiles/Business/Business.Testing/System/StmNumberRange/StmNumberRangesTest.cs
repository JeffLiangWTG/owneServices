using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmNumberRange))]
	sealed class StmNumberRangesTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		#region GetNewBusinessObjectForDeleteTest

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = factory.New<StmNumberRange>();
			bizo.SNR_Owner = GlbCompany.CurrentCompany.PK;
			bizo.SNR_Name = "DeleteTest";
			return bizo;
		}

		#endregion

		#endregion
	}
}
