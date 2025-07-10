using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccSurchargeBasis))]
	sealed class AccSurchargeBasisTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var config = Factory.New<AccSurchargeConfiguration>();
			config.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			var basis = Factory.New<AccSurchargeBasis>();
			basis.ASB_ASC_SurchargeConfiguration = config.PK;
			return basis;
		}
	}
}
