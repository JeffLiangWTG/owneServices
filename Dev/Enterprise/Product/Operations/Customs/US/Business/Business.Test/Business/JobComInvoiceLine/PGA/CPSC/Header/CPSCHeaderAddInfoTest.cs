using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCHeaderAddInfo))]
	sealed class CPSCHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var atf = Factory.New<CPSCHeader>();
			return new CPSCHeaderAddInfo(atf.B7_AddInfoDataInfo);
		}
	}
}
