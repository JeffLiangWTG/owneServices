using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	sealed class GlobalOrgSupplierPartDataLoadTest : OrgSupplierPartDataLoadTest
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new GlobalOrgSupplierPartDataLoad();
		}
	}
}
