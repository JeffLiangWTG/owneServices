using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USPSTLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductQualifiers()
		{
			var productQualifiers = lookups.ProductQualifiers;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "CAS, PC", productQualifiers.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue("ProductQualifiers", () => new CodeDescriptionPairList()), productQualifiers);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var pesticideLine = Factory.New<PesticideLine>();
			var addInfo = new USPSTLineAddInfo(pesticideLine.B7_AddInfoDataInfo);
			lookups = new USPSTLineAddInfoLookups(addInfo);
		}
		USPSTLineAddInfoLookups lookups;
	}
}
