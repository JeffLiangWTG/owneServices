using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class RadionuclideElementConstantsTest : TestCaseWithFactory
	{
		public void TestRadionuclideElementSuffixListGeneration()
		{
			var actiniumCategoryList = new RadionuclideElementSuffixList(RadionuclideElementConstants.Codes.Actinium);

			var actiniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium225a, "Actinium - 225 (a)");
			var actiniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium227a, "Actinium - 227 (a)");
			var actiniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium228, "Actinium - 228");

			var germaniumCategoryList = new RadionuclideElementSuffixList(RadionuclideElementConstants.Codes.Germanium);

			var germaniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium68a, "Germanium - 68 (a)");
			var germaniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium71, "Germanium - 71");
			var germaniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium77, "Germanium - 77");

			var kryptonCategoryList = new RadionuclideElementSuffixList(RadionuclideElementConstants.Codes.Krypton);

			var kryptonCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton79, "Krypton - 79");
			var kryptonCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton81, "Krypton - 81");
			var kryptonCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton85, "Krypton - 85");
			var kryptonCodePair4 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton85m, "Krypton - 85m");
			var kryptonCodePair5 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton87, "Krypton - 87");

			var uraniumCategoryList = new RadionuclideElementSuffixList(RadionuclideElementConstants.Codes.Uranium);

			var uraniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium230FastLungAbsorptionad, "Uranium - 230 (fast lung absorption) (a)(d)");
			var uraniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium232SlowLungAbsorptionf, "Uranium - 232 (slow lung absorption) (f)");
			var uraniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium234FastLungAbsorptiond, "Uranium - 234 (fast lung absorption) (d)");
			var uraniumCodePair4 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium236FastLungAbsorptiond, "Uranium - 236 (fast lung absorption) (d)");
			var uraniumCodePair5 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.UraniumNat, "Uranium - (nat)");
			var uraniumCodePair6 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.UraniumEnriched, "Uranium - (enriched to 20% or less)(g)");

			CombineAssertions("All codes are present in the list", () =>
			{
				AssertCollectionContains(actiniumCodePair1.ToString(), actiniumCodePair1, actiniumCategoryList);
				AssertCollectionContains(actiniumCodePair2.ToString(), actiniumCodePair2, actiniumCategoryList);
				AssertCollectionContains(actiniumCodePair3.ToString(), actiniumCodePair3, actiniumCategoryList);
				AssertCollectionNotContains(germaniumCodePair1.ToString(), germaniumCodePair1, actiniumCategoryList);

				AssertCollectionContains(germaniumCodePair1.ToString(), germaniumCodePair1, germaniumCategoryList);
				AssertCollectionContains(germaniumCodePair2.ToString(), germaniumCodePair2, germaniumCategoryList);
				AssertCollectionContains(germaniumCodePair3.ToString(), germaniumCodePair3, germaniumCategoryList);
				AssertCollectionNotContains(kryptonCodePair1.ToString(), kryptonCodePair1, germaniumCategoryList);

				AssertCollectionContains(kryptonCodePair1.ToString(), kryptonCodePair1, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair2.ToString(), kryptonCodePair2, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair3.ToString(), kryptonCodePair3, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair4.ToString(), kryptonCodePair4, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair5.ToString(), kryptonCodePair5, kryptonCategoryList);
				AssertCollectionNotContains(uraniumCodePair1.ToString(), uraniumCodePair1, kryptonCategoryList);

				AssertCollectionContains(uraniumCodePair1.ToString(), uraniumCodePair1, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair2.ToString(), uraniumCodePair2, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair3.ToString(), uraniumCodePair3, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair4.ToString(), uraniumCodePair4, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair5.ToString(), uraniumCodePair5, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair6.ToString(), uraniumCodePair6, uraniumCategoryList);
				AssertCollectionNotContains(actiniumCodePair1.ToString(), actiniumCodePair1, uraniumCategoryList);
			});
		}
	}
}
