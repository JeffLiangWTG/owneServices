using NUnit.Framework;
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		[ExpectNoExceptions]
		public void TestContainer()
		{
			CusContainer parent = Factory.New<CusContainer>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.Container));
		}

		[ExpectNoExceptions]
		public override void TestCO_FCL_LCL_NCT_List()
		{
			CusContainerLookups lookups = new CusContainerLookups(Factory.New<CusContainer>());
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.ZArchitecture.Core.CodeDescriptionPairList)), "Lookups.CO_FCL_LCL_NCT_List - should not be [null]");
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.ContainsCode(TWContainerModesList.Codes.Empty), NUnit.Framework.Is.True, "CO_FCL_LCL_NCT_List.ContainsCode(EMP)");
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.ContainsCode(TWContainerModesList.Codes.LCL), NUnit.Framework.Is.True, "CO_FCL_LCL_NCT_List.ContainsCode(LCL)");
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.ContainsCode(TWContainerModesList.Codes.FCL), NUnit.Framework.Is.True, "CO_FCL_LCL_NCT_List.ContainsCode(FCL)");
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.ContainsCode(TWContainerModesList.Codes.GRP), NUnit.Framework.Is.True, "CO_FCL_LCL_NCT_List.ContainsCode(GRP)");
			NUnit.Framework.Assert.That(lookups.CO_FCL_LCL_NCT_List.ContainsCode(TWContainerModesList.Codes.BCN), NUnit.Framework.Is.True, "CO_FCL_LCL_NCT_List.ContainsCode(BCN)");
		}

		#region Implementation
		protected override Customs.Business.CusContainerLookups GetCusContainerLookups()
		{
			return new CusContainerLookups(Factory.New<CusContainer>());
		}
		#endregion
	}
}
