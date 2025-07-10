using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.SG.Access.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestModuleCollection))]
	class ManifestModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var manifestHeader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var manifestHeader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, "ICR");
			var manifestHeader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, SGManifestTypes.Codes.MGI);
			Factory.Save();
			var collection = new ManifestModuleCollection(Factory);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { manifestHeader3.PK }, collection.Select(x => x.PK));
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManifestModuleCollection(Factory);
		}
		#endregion
	}
}
