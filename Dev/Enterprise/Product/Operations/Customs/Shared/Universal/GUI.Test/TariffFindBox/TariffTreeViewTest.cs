using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	sealed class TariffTreeViewTest : TestCaseWithFactory
	{
#if !WINZOR
		[RequiresSTA]
		public void TestHighlightMatchedText()
		{
			CreateZZTariffData(Factory);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.SouthAfrica, "1P1", null, null);
			helper.PartialDescription = "HELLO";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var treeView = form.Controls.Find("TariffTreeView", true)[0] as TariffTreeView;
				var filterStripBizO = treeView.filterBusinessObject;
				var description = filterStripBizO.ActiveModuleFilters.OfType<ModuleTextFilter>().First(x => x.Description.StartsWith(Constants.RefCusTariffFilters.DefaultLanguageDescription, StringComparison.OrdinalIgnoreCase));
				AssertEquals("PreCondition: PartialDescription must be set", "HELLO", description.Property);
				CombineAssertions(() =>
				{
					var bmpFromScreen = new Bitmap(treeView.Width, treeView.Height);
					treeView.DrawToBitmap(bmpFromScreen, new Rectangle(0, 0, treeView.Width, treeView.Height));
					AssertEquals("Must have a yellow hightlight", Color.FromArgb(255, 255, 255, 0), bmpFromScreen.GetPixel(ControlDpiScalingHelper.ScaleToCurrentDpiX(62), ControlDpiScalingHelper.ScaleToCurrentDpiY(4)));
					description.Property = "CARP";
					(form.Controls.Find("ToolStrip", true)[0] as ZToolStrip)?.Items.OfType<ZToolStripSplitButton>().FirstOrDefault(x => x.Name == "ToolStripFindDropButton")?.PerformClick();
					treeView.DrawToBitmap(bmpFromScreen, new Rectangle(0, 0, treeView.Width, treeView.Height));
					AssertNotEquals("Must not have a yellow hightlight", Color.FromArgb(255, 255, 255, 0), bmpFromScreen.GetPixel(ControlDpiScalingHelper.ScaleToCurrentDpiX(62), ControlDpiScalingHelper.ScaleToCurrentDpiY(4)));
					description.Property = "HELLO.+";
					(form.Controls.Find("ToolStrip", true)[0] as ZToolStrip)?.Items.OfType<ZToolStripSplitButton>().FirstOrDefault(x => x.Name == "ToolStripFindDropButton")?.PerformClick();
					treeView.DrawToBitmap(bmpFromScreen, new Rectangle(0, 0, treeView.Width, treeView.Height));
					AssertNotEquals("Must not have a yellow hightlight", Color.FromArgb(255, 255, 255, 0), bmpFromScreen.GetPixel(ControlDpiScalingHelper.ScaleToCurrentDpiX(62), ControlDpiScalingHelper.ScaleToCurrentDpiY(4)));
				});
			}
		}

		static void CreateZZTariffData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var zzCountry = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			zzCountry.ZZZ_Description = "Universal Customs";
			var zaCountry = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			zaCountry.ZZZ_Description = "South Africa";
			zaCountry.ZZZ_ZZZ_Grouping = zzCountry.PK;
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "NGT";
			factory.Save();
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "291611", ZDateTime.BrettsBirthday, ZDateTime.UtcToday.AddYears(5), "HELLO AND ACID AND ITS SALTS:", compositeKey: "A", nomenclatureGroupType: "NGT");
			factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "29161130", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "BUTYL ACRYLATE", compositeKey: "A.A");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "29161140", new ZDateTime(2012, 01, 01), new ZDateTime(2016, 12, 31, 23, 59, 00), "CARP (CYPRINUS CARPIO,ACID CARASSIUS CARASSIUS, CTENOPHARYNGODON IDELLUS, HYPOPHTHALMICHTHYS SPP., CIRRHINUS SPP., MYLOPHARYNGODON PICEUS)", compositeKey: "A.B");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "29161150", new ZDateTime(2017, 01, 01), new ZDateTime(2079, 06, 06), "CARP (CYPRINUS SPP., CARASSIUS SPP.,ACID CTENOPHARYNGODON IDELLUS, HYPOPHT HALMICHTHYS SPP., CIRRHINUS SPP., MYLOPHARYNGODON PICEUS, CATLA CATLA, LABEO SPP., OSTEOCHILUS HASSELTI, LEPTOBARBUS HOEVENI, MEGALOBRAMA SP P.)", compositeKey: "A.C");
			factory.Save();
		}
#endif
	}
}
