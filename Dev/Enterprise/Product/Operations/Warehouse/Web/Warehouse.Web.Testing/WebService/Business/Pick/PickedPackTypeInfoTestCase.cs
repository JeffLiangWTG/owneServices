using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.Testing.WebService.Business
{
	[TestedType(typeof(PickedPackTypeInfo))]
	public class PickedPackTypeInfoTestCase : DataObjectInfoTestCase<PickedPackTypeInfo>
	{
		protected new PickedPackTypeInfo Parent => (PickedPackTypeInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PickedPackTypeInfo();
		}
	}
}
