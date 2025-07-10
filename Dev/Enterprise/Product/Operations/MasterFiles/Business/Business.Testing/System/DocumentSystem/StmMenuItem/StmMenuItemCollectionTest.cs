using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmMenuItemCollection))]
	public class StmMenuItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuItemCollection(Factory);
		}

		[StressTest]
		public void TestRelationshipFilter()
		{
			StmMenuItem menu1 = Factory.New<StmMenuItem>();
			StmMenuItem menu2 = Factory.New<StmMenuItem>();
			StmMenuItem menu3 = Factory.New<StmMenuItem>();
			StmMenuItem menu4 = Factory.New<StmMenuItem>();
			StmMenuItem menu5 = Factory.New<StmMenuItem>();

			menu1.SU_BusinessContext = nameof(BusinessContext.AgencyBooking);
			menu2.SU_BusinessContext = nameof(BusinessContext.AgencyDocumentation);
			menu3.SU_BusinessContext = nameof(BusinessContext.AgencyDocumentation);
			menu4.SU_BusinessContext = nameof(BusinessContext.AgencyDocumentation);
			menu5.SU_BusinessContext = nameof(BusinessContext.AgencyDocumentation);

			menu1.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			menu2.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			menu3.SU_MenuType = Core.Constants.StmMenuItemTypes.OperationalActions;
			menu4.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			menu5.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;

			menu1.SU_MenuPath = "a";
			menu2.SU_MenuPath = "b";
			menu3.SU_MenuPath = "c";
			menu4.SU_MenuPath = "d";
			menu5.SU_MenuPath = "d";

			menu1.SU_ContactType = "CNR";
			menu2.SU_ContactType = "CNR";
			menu3.SU_ContactType = "CNR";
			menu4.SU_ContactType = "CNR";
			menu5.SU_ContactType = "CNE";

			StmMenuItemCollection collection = new StmMenuItemCollection(Factory);
			collection.Load();
			AssertEquals("Contains(menu1)", true, collection.Contains(menu1));
			AssertEquals("Contains(menu2)", true, collection.Contains(menu2));
			AssertEquals("Contains(menu3)", false, collection.Contains(menu3));
			AssertEquals("Contains(menu4)", true, collection.Contains(menu4));
			AssertEquals("Contains(menu5)", true, collection.Contains(menu5));

			collection = new StmMenuItemCollection(Factory, new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.AgencyDocumentation)));
			collection.Load();
			AssertEquals("Contains(menu1)", false, collection.Contains(menu1));
			AssertEquals("Contains(menu2)", true, collection.Contains(menu2));
			AssertEquals("Contains(menu3)", false, collection.Contains(menu3));
			AssertEquals("Contains(menu4)", true, collection.Contains(menu4));
			AssertEquals("Contains(menu5)", true, collection.Contains(menu5));

			collection.Load(new ZQuery(StmMenuItemSchema.SU_MenuPath, "d"));
			AssertEquals("Contains(menu1)", false, collection.Contains(menu1));
			AssertEquals("Contains(menu2)", false, collection.Contains(menu2));
			AssertEquals("Contains(menu3)", false, collection.Contains(menu3));
			AssertEquals("Contains(menu4)", true, collection.Contains(menu4));
			AssertEquals("Contains(menu5)", true, collection.Contains(menu5));

			collection.Load(new ZQuery(StmMenuItemSchema.SU_ContactType, "CNR"));
			AssertEquals("Contains(menu1)", true, collection.Contains(menu1));
			AssertEquals("Contains(menu2)", true, collection.Contains(menu2));
			AssertEquals("Contains(menu3)", false, collection.Contains(menu3));
			AssertEquals("Contains(menu4)", true, collection.Contains(menu4));
			AssertEquals("Contains(menu5)", false, collection.Contains(menu5));
		}
	}
}
