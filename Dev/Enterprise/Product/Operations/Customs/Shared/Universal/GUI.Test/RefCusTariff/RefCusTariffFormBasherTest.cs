using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusTariffForm))]
	class RefCusTariffFormBasherTest : ZFormBasherTest
	{
		public void TestEffectiveFieldsAreNotReadOnly()
		{
			using (var form = new RefCusTariffForm(cusTariff))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				var tariffCodeTextBox = form.FindSingle<ZTextBox>("ZZ1_TariffCodeTextBox");
				AssertEquals("tariffCodeTextBox", true, tariffCodeTextBox.ReadOnly);
				var effectiveDateDateEdit = form.FindSingle<ZDateEdit>("EffectiveDateDateEdit");
				AssertEquals("effectiveDateDateEdit", false, effectiveDateDateEdit.ReadOnly);
				var effectiveDataGroupingDropEdit = form.FindSingle<ZDropEdit>("EffectiveDataGroupingDropEdit");
				AssertEquals("effectiveDataGroupingDropEdit", false, effectiveDataGroupingDropEdit.ReadOnly);
				var ratesApplyToCountryFindBox = form.FindSingle<ZCodeFindBox>("RatesApplyToCountryFindBox");
				AssertEquals("ratesApplyToCountryFindBox", false, ratesApplyToCountryFindBox.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestIncludeGeneralTariffsMenuItem()
		{
			using (var form = new ZForm())
			{
				var testCollectionView = new TariffRelationshipViewCollection(cusTariff);
				var testMenuItem = new IncludeGeneralTariffsMenuItem(testCollectionView);
				form.Menu.MenuItems.Add(testMenuItem);
				bool found = false;
				foreach (MenuItem menuItem in form.Menu.MenuItems)
				{
					if (menuItem is IncludeGeneralTariffsMenuItem item)
					{
						found = true;
						var mainMenu = item;
						var checkBoxMenuItem = mainMenu.MenuItems[0];
						AssertEquals(testCollectionView.ExcludeGeneralTariffs, checkBoxMenuItem.Checked);
						bool initial = testCollectionView.ExcludeGeneralTariffs;
						checkBoxMenuItem.PerformClick();
						AssertNotEquals(initial, testCollectionView.ExcludeGeneralTariffs);
						break;
					}
				}

				Assert("Should have AddIncludeGeneralChildTariffsMenuItem", found);
			}
		}

		protected override Form GetFormToBashCore() => new RefCusTariffForm(Factory.New<TariffView>());

		protected override void SetUp()
		{
			base.SetUp();
			s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}
		RefCusTariffType s1p1TariffType;
		UniversalReferenceTestDataHelper helper => new UniversalReferenceTestDataHelper(Factory);
		TariffView cusTariff => helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10, 9, 25, 00), new ZDateTime(2079, 06, 06, 14, 55, 00), "dummy Description 0");
	}
}
