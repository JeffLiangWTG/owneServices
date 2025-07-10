using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCDetailAddInfo))]
	sealed class USHFCDetailAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var detail = Factory.New<USHFCDetail>();
			return new USHFCDetailAddInfo(detail.B7_AddInfoDataInfo);
		}
	}
}
