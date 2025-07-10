using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(UniversalCommodityCodeBizo))]
	public class UniversalCommodityCodeBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var bizo = new UniversalCommodityCodeBizo(Factory);
			AssertEquals("", bizo.RH_UniversalCommodityGroup);
			AssertEquals("", bizo.RH_UniversalCommodityGroupDescription);
			AssertEquals("", bizo.RH_Code);
			AssertEquals("", bizo.RH_Description);

			bizo.RH_UniversalCommodityGroup = "Aerospace";
			bizo.RH_UniversalCommodityGroupDescription = "A class aerospace related commodities.";
			bizo.RH_Code = "AIRC";
			bizo.RH_Description = "AIRCRAFT";

			AssertEquals("Aerospace", bizo.RH_UniversalCommodityGroup);
			AssertEquals("A class aerospace related commodities.", bizo.RH_UniversalCommodityGroupDescription);
			AssertEquals("AIRC", bizo.RH_Code);
			AssertEquals("AIRCRAFT", bizo.RH_Description);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UniversalCommodityCodeBizo(Factory);
		}

		#endregion
	}
}
