using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.Testing.GUITests.WiseRates.CommodityGroupsForm
{
	[TestedType(typeof(CommodityGroupViewModelCollection))]
	class CommodityGroupViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommodityGroupViewModelCollection>
	{
		protected override CommodityGroupViewModelCollection GetCollectionToTest()
		{
			return new CommodityGroupViewModelCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommodityGroupViewModel
			{
				UniversalGroup = "GENL",
				UniversalGroupDescription = "General",
				CommodityCode = "GEN",
				CommodityDescription = "General"
			};
		}
	}
}
