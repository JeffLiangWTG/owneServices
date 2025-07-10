using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LaceyCountryAddInfo))]
	public class LaceyCountryAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var license = Factory.New<LaceyCountry>();
			var addInfo = new LaceyCountryAddInfo(license.B7_AddInfoDataInfo);
			return addInfo;
		}
	}
}
