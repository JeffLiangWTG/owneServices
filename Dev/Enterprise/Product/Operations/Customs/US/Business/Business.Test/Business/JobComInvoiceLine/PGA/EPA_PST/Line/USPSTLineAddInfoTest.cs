using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USPSTLineAddInfo))]
	sealed class USPSTLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var pesticideLine = Factory.New<PesticideLine>();
			return new USPSTLineAddInfo(pesticideLine.B7_AddInfoDataInfo);
		}
	}
}
