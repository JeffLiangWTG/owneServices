using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccBankAccountModule))]
	sealed class AccBankAccountModuleTest : ZModuleBasherTest
	{
		public AccBankAccountModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccBankAccount;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			accBankAccount = new AccBankAccountModuleForTest();
			IFilterControl controlForTest = accBankAccount.GetNewFilterControlForTest();
			Assert(controlForTest is AccBankAccountFilterControl);
			controlForTest.Dispose();
			accBankAccount.Dispose();
		}

		public void TestGridCollection()
		{
			accBankAccount = new AccBankAccountModuleForTest();
			IBusinessObjectCollection collectionForTest = accBankAccount.GetNewGridCollectionForTest();
			Assert(collectionForTest is BusinessObjectCollection);
			accBankAccount.Dispose();
		}

		public void TestOnlyCurrentCompanyBankAccountsDiplayed()
		{
			AccBankAccount currentCompanyAccount = Factory.NewWithValidTestData<AccBankAccount>();
			currentCompanyAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			AccBankAccount nonCurrentCompanyAccount = Factory.NewWithValidTestData<AccBankAccount>();
			nonCurrentCompanyAccount.AB_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			Factory.Save();

			accBankAccount = new AccBankAccountModuleForTest();
			BusinessObjectCollection collectionForTest = accBankAccount.GetNewGridCollectionForTest();
			collectionForTest.Load();
			AssertEquals(1, collectionForTest.Count);
			accBankAccount.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			accBankAccount = new AccBankAccountModuleForTest();
			FilterBusinessObject businessForTest = accBankAccount.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			accBankAccount.Dispose();
		}

		public void TestGetNewStandardMenuItems()
		{
			accBankAccount = new AccBankAccountModuleForTest();
			accBankAccount.GetNewStandardMenuItemsForTest();
			var standardMenuItems = accBankAccount.NewMenuItem.MenuItems;
			AssertEquals(5, standardMenuItems.Count);
			foreach (CodeDescriptionPair pair in AccBankAccountLookups.GetBankAccountTypesList())
			{
				var menuText = $"New {pair.Description}";
				AssertNotNull($"There should be a '{menuText}' menu item", standardMenuItems.FindByText(menuText));
			}
			accBankAccount.Dispose();
		}

		#region Implementation

		AccBankAccountModuleForTest accBankAccount;

		#endregion
	}
}
