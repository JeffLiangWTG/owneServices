using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWProductLabelRange))]
	sealed class CusTWProductLabelRangeBaseTest : CargoWise.EntityFramework.Testing.BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusTWProductLabelRange>();
	}
}
