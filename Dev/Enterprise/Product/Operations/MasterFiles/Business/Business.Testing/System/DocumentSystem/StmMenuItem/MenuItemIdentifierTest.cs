using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MenuItemIdentifierTest : TestCaseWithFactory
	{
		MenuItemIdentifier identifier;

		public void TestConstructorAndProperties()
		{
			AssertEquals("BusinessContext", BusinessContext.INVALID, identifier.BusinessContext);
			AssertEquals("Name", null, identifier.Name);

			identifier = new MenuItemIdentifier(BusinessContext.HotCheque, "Hot Check");
			AssertEquals("BusinessContext", BusinessContext.HotCheque, identifier.BusinessContext);
			AssertEquals("Name", "Hot Check", identifier.Name);

			identifier.BusinessContext = BusinessContext.JobService;
			identifier.Name = "Job Service";
			AssertEquals("BusinessContext", BusinessContext.JobService, identifier.BusinessContext);
			AssertEquals("Name", "Job Service", identifier.Name);
		}

		public void TestEquals()
		{
			AssertEquals("Identifier should be equal to an another identifier with the same values.", true, identifier.Equals(new MenuItemIdentifier()));
			AssertEquals("Identifier should not be equal to a null value.", false, identifier.Equals(null));
			AssertEquals("Identifier should not be equal to an object of another type.", false, identifier.Equals(""));
			AssertEquals("Identifier should not be equal to another identifier with different values.", false, identifier.Equals(new MenuItemIdentifier(BusinessContext.JobService, "Job Service")));

			identifier.BusinessContext = BusinessContext.JobService;
			identifier.Name = "Job Service";

			AssertEquals("Identifier should not be equal to another identifier with different values.", false, identifier.Equals(new MenuItemIdentifier()));
			AssertEquals("Identifier should not be equal to a null value.", false, identifier.Equals(null));
			AssertEquals("Identifier should not be equal to an object of another type.", false, identifier.Equals(""));
			AssertEquals("Identifier should be equal to an another identifier with the same values.", true, identifier.Equals(new MenuItemIdentifier(BusinessContext.JobService, "Job Service")));
		}

		public void TestGetHashCode()
		{
			AssertEquals("GetHashCode() should not change.", identifier.GetHashCode(), identifier.GetHashCode());
			AssertEquals("GetHashCode() should be equal between identifiers with the same value.", identifier.GetHashCode(), new MenuItemIdentifier().GetHashCode());
			AssertEquals("GetHashCode() should be equal between identifiers with the same value.", new MenuItemIdentifier(BusinessContext.QuotedBooking, "Quoted Booking").GetHashCode(), new MenuItemIdentifier(BusinessContext.QuotedBooking, "Quoted Booking").GetHashCode());
		}

		public void TestGetQuery_InvalidValues()
		{
			identifier.BusinessContext = BusinessContext.Order;
			identifier.Name = "Food";
			var query = identifier.GetQuery();
			var parameters = query.Params;
			AssertEquals("GetQuery().Params.Length", 5, parameters.Length);
			AssertEquals("GetQuery().Params[0].SchemaColumn", StmMenuItemSchema.SU_MenuType, parameters[0].SchemaColumn);

			AssertEquals("GetQuery().Params[0].Value 1", Constants.StmMenuItemTypes.Documents, parameters[0].Value);
			AssertEquals("GetQuery().Params[1].Value 2", Constants.StmMenuItemTypes.WebReports, parameters[1].Value);
			AssertEquals("GetQuery().Params[2].SchemaColumn", StmMenuItemSchema.SU_BusinessContext, parameters[2].SchemaColumn);
			AssertEquals("GetQuery().Params[2].Value", nameof(BusinessContext.Order), parameters[2].Value);
			AssertEquals("GetQuery().Params[3].SchemaColumn", StmMenuItemSchema.SU_MenuName, parameters[3].SchemaColumn);
			AssertEquals("GetQuery().Params[3].Value", "Food", parameters[3].Value);
			AssertEquals("GetQuery().Params[4].SchemaColumn", StmMenuItemSchema.SU_IsSystemDefined, parameters[4].SchemaColumn);
			AssertEquals("GetQuery().Params[4].Value", 1, parameters[4].Value);
		}

		public void TestLoadMenuItem_MultipleResultsFound()
		{
			StmMenuItem menuItem1 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem2 = Factory.New<StmMenuItem>();

			menuItem1.SU_BusinessContext = nameof(BusinessContext.Order);
			menuItem2.SU_BusinessContext = nameof(BusinessContext.Order);
			menuItem1.SU_MenuName = "Food";
			menuItem2.SU_MenuName = "Food";
			menuItem1.SU_IsSystemDefined = true;
			menuItem2.SU_IsSystemDefined = true;

			identifier.BusinessContext = BusinessContext.Order;
			identifier.Name = "Food";

			AssertNull("LoadMenuItem()", identifier.LoadMenuItem<StmMenuItem>(Factory));
			AssertEquals("Exception Message", "More than one menu item was found with the business context \"Order\" and the name \"Food\".", ExceptionReporterTestListener.Instance[ExceptionReporterTestListener.Instance.Count - 1].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestLoadMenuItem_NoResultFound()
		{
			identifier.BusinessContext = BusinessContext.Order;
			identifier.Name = "Fake Order";
			AssertNull("LoadMenuItem()", identifier.LoadMenuItem<StmMenuItem>(Factory));
			AssertEquals("Exception Message", "No menu item was found with the business context \"Order\" and the name \"Fake Order\".", ExceptionReporterTestListener.Instance[ExceptionReporterTestListener.Instance.Count - 1].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestLoadMenuItem_ResultFound()
		{
			StmMenuItem menuItem1 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem2 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem3 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem4 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem5 = Factory.New<StmMenuItem>();

			menuItem1.SU_BusinessContext = nameof(BusinessContext.Organisation);
			menuItem2.SU_BusinessContext = nameof(BusinessContext.Organisation);
			menuItem3.SU_BusinessContext = nameof(BusinessContext.Order);
			menuItem4.SU_BusinessContext = nameof(BusinessContext.Organisation);
			menuItem5.SU_BusinessContext = nameof(BusinessContext.Organisation);

			menuItem1.SU_MenuName = "Smash";
			menuItem2.SU_MenuName = "Smash";
			menuItem3.SU_MenuName = "Smash";
			menuItem4.SU_MenuName = "Crush";
			menuItem5.SU_MenuName = "Smash";

			menuItem1.SU_IsSystemDefined = true;
			menuItem2.SU_IsSystemDefined = false;
			menuItem3.SU_IsSystemDefined = true;
			menuItem4.SU_IsSystemDefined = true;
			menuItem5.SU_IsSystemDefined = true;

			menuItem5.SU_MenuType = Constants.StmMenuItemTypes.OperationalActions;

			identifier.BusinessContext = BusinessContext.Organisation;
			identifier.Name = "Smash";

			AssertEquals("LoadMenuItem()", menuItem1, identifier.LoadMenuItem<StmMenuItem>(Factory));
		}

		public static void TestMenuItemExists(BusinessContext businessContext, string name)
		{
			MenuItemIdentifier identifier = new MenuItemIdentifier(businessContext, name);
			StmMenuItem[] menuItems = new BusinessObjectFactory().Load<StmMenuItem>(identifier.GetQuery());

			if (menuItems.Length > 1)
			{
				Fail("More than one menu item was found with the business context \"" + identifier.BusinessContext +
					"\" and the name \"" + identifier.Name +
					"\". MenuItemIdentifiers can only be used for menu items that have a unique name and business context pair.");
			}
			else if (menuItems.Length == 0)
			{
				Fail("No menu item was found with the business context \"" + identifier.BusinessContext +
					"\" and the name \"" + identifier.Name +
					"\". Please check that the business context and name are correct. If you have removed the menu item, you must write a transformation to update any existing records that are dependent on that menu item (eDocsProviders, for example).");
			}
			else
			{
				AssertionCount++;
			}
		}
	}
}
