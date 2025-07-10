using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing;

sealed class ValidationToolActionSourceListProviderTest : TestCaseWithFactory
{
	public void TestGetActionSourceList_NoExceptionForCRD()
	{
		AssertNoExceptionThrown(() =>
		{
			var provider = (IValidationToolActionSourceListProvider)new ValidationToolActionSourceListProvider();
			_ = (CodeDescriptionPairList)provider.GetActionSourceList(ControllerIDs.Customs.ConsolidatedDeclaration, Core.Constants.CountryCodes.Australia);
		});
	}

	public void TestGetActionSourceList_ControllerId() => CombineAssertions(() =>
	{
		var provider = (IValidationToolActionSourceListProvider)new ValidationToolActionSourceListProvider();
		var list = (CodeDescriptionPairList)provider.GetActionSourceList(ControllerIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.Australia);
		AssertEquals("File", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("File >")));
		AssertEquals("Edit", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Edit >")));
		AssertEquals("Actions", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Actions >")));
		AssertEquals("Brokerage", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Brokerage >")));
		AssertEquals("Landed Costing", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Landed Costing >")));
		AssertEquals("Job Invoicing", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Job Invoicing >")));
		AssertEquals("Documents", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Documents >")));
		AssertEquals("Port Transport", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Port Transport >")));
		AssertEquals("Help", true, list.Cast<CodeDescriptionPair>().Any(x => x.Description.StartsWith("Help >")));
	});

	public void TestGetActionSourceList_TraverseZMenuItem()
	{
		var menu = new ZMenuItem("&Brokerage");
		var item1 = new ZMenuItem("&Item 1");
		var item11 = new ZMenuItem("&Item 1.1");
		item11.MenuItems.Add(new ZMenuItem("&Item 1.1.1"));
		item11.MenuItems.Add(new ZMenuItem(ZMenuItem.Separator));
		item11.MenuItems.Add(new ZMenuItem("&Item 1.1.2"));
		item1.MenuItems.Add(item11);
		item1.MenuItems.Add(ZMenuItem.Separator);
		item1.MenuItems.Add(new ZMenuItem("&Item 1.2"));
		menu.MenuItems.Add(item1);
		menu.MenuItems.Add(ZMenuItem.Separator);
		menu.MenuItems.Add(new ZMenuItem("&Item 2"));
		var factory = new ValidationToolActionSourceListProvider();
		var source = factory.GetActionSourceList(menu);

		AssertEquals("""
Item 1.1.1 - Brokerage > Item 1 > Item 1.1 > Item 1.1.1
Item 1.1.2 - Brokerage > Item 1 > Item 1.1 > Item 1.1.2
Item 1.2 - Brokerage > Item 1 > Item 1.2
Item 2 - Brokerage > Item 2
""", source.GetHumanReadableListOfElements());
	}
}
