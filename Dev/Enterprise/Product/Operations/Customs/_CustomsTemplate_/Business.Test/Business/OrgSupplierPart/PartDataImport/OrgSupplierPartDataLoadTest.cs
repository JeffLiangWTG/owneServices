using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();
	}
}
