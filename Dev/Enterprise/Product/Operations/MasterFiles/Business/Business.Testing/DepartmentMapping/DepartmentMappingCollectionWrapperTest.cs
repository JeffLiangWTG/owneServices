using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DepartmentMappingCollectionWrapper))]
	sealed class DepartmentMappingCollectionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DepartmentMappingCollectionWrapper bizO = new DepartmentMappingCollectionWrapper("");
			return bizO;
		}
	}
}
