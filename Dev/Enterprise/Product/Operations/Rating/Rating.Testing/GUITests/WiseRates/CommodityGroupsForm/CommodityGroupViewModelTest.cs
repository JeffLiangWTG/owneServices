using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.Testing.GUITests.WiseRates.CommodityGroupsForm
{
	[TestedType(typeof(CommodityGroupViewModel))]
	public class CommodityGroupViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommodityGroupViewModel
			{
				UniversalGroup = "GENL",
				UniversalGroupDescription = "General",
				CommodityCode = "GEN",
				CommodityDescription = "General"
			};
		}

		#endregion
	}
}
