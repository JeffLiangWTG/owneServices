using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeader))]
	sealed class CusTWControllingMessageHeaderBaseTest : CargoWise.EntityFramework.Testing.BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusTWControllingMessageHeader>();
	}
}
