using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAddInfoModuleTextFilter))]
	sealed class CusAddInfoModuleTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusAddInfoModuleTextFilter("Route Of Entry", CusAddInfoSchema.B7_AddInfoData, "RouteOfEntry", typeof(BaseJobDeclaration), CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties);
		}
	}
}
