using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LicenseAddInfo))]
	sealed class LicenseAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var license = Factory.New<License>();
			return new LicenseAddInfo(license.B7_AddInfoDataInfo);
		}
	}
}
