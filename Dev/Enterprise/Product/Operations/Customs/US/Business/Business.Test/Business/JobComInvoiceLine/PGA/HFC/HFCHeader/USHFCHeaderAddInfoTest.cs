using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCHeaderAddInfo))]
	sealed class USHFCHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<USHFCHeader>();
			return new USHFCHeaderAddInfo(header.B7_AddInfoDataInfo);
		}
	}
}
