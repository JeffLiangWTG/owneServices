using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module.Testing
{
	[TestedType(typeof(ManifestBusinessObject))]
	sealed class ManifestBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "UU1";
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.China;
			header2.AMA_JobReference = "UU2";

			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header3.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TRETrade;
			header3.AMA_JobReference = "UU3";

			var collection = new AsycudaManifestModuleCollection(Factory);
			var filter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				Assert("match", header1.MatchesFilter(filter));
				Assert("application code not match", !header2.MatchesFilter(filter));
				Assert("country not match", !header3.MatchesFilter(filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ManifestBusinessObject();
		}
	}
}
