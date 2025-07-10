using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class WhsItemPackageStateReturnDetail : AutoWhsItemPackageStateReturnDetail
	{
		public WhsItemPackageStateReturnDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
