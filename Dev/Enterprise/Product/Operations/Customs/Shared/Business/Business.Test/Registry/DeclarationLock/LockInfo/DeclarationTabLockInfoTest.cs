using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationTabLockInfo))]
	sealed class DeclarationTabLockInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Validation

		public void TestValidateTabPage()
		{
			var message = "Enter a valid selection.";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);
			var eventLockInfo = lockConfig.TabInfos.AddNew();

			eventLockInfo.TabPage = "@#^";
			AssertHasError(eventLockInfo.TabPageInfo, message);

			eventLockInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers;
			AssertNoError(eventLockInfo.TabPageInfo, message);

			eventLockInfo.TabPage = ZString.Empty;
			AssertHasError(eventLockInfo.TabPageInfo, "Please enter a value.");
		}

		public void TestValidateTabPage_WarningForDeclaration()
		{
			var warning = "The selection of Declaration will cause controls on a declaration to be locked with the exception of the controls on each of the tabs specified below.\r\n\r\nDeclaration - Services\r\n\r\nDeclaration - Organizations\r\n\r\nDeclaration - Pickup/Delivery\r\n\r\nDeclaration - Orders\r\n\r\nDeclaration - Custom\r\n\r\nDeclaration - Numbers";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);
			lockConfig.DeclarationType = "IMP";
			var eventLockInfo = lockConfig.TabInfos.AddNew();
			eventLockInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;
			AssertHasWarning(eventLockInfo.TabPageInfo, warning);

			eventLockInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Routing;
			AssertNoWarning(eventLockInfo.TabPageInfo, warning);

			eventLockInfo.TabPage = "XXX";
			AssertNoWarning(eventLockInfo.TabPageInfo, warning);
		}

		public void TestValidateTabPage_UniqueCheck()
		{
			var message = "There is an existing item with DNO";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var config = new DeclarationLockConfig(fallbackLevel, Factory);

			var tabLockInfo1 = config.TabInfos.AddNew();
			tabLockInfo1.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers;

			AssertNoError(tabLockInfo1.TabPageInfo, message);

			var tabLockInfo2 = config.TabInfos.AddNew();
			tabLockInfo2.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers;

			tabLockInfo1.ValidateTabPage();
			tabLockInfo2.ValidateTabPage();

			AssertHasError(tabLockInfo1.TabPageInfo, message);
			AssertHasError(tabLockInfo2.TabPageInfo, message);

			tabLockInfo2.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Packing;

			tabLockInfo1.ValidateTabPage();
			tabLockInfo2.ValidateTabPage();

			AssertNoError(tabLockInfo1.TabPageInfo, message);
			AssertNoError(tabLockInfo2.TabPageInfo, message);

			message = "You have chosen all tabs.";

			var tabLockInfo3 = config.TabInfos.AddNew();
			tabLockInfo3.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.All;

			tabLockInfo1.ValidateTabPage();
			tabLockInfo2.ValidateTabPage();
			tabLockInfo3.ValidateTabPage();

			AssertHasError(tabLockInfo1.TabPageInfo, message);
			AssertHasError(tabLockInfo2.TabPageInfo, message);
			AssertNoError(tabLockInfo3.TabPageInfo, message);

			tabLockInfo3.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations;

			tabLockInfo1.ValidateTabPage();
			tabLockInfo2.ValidateTabPage();
			tabLockInfo3.ValidateTabPage();

			AssertNoError(tabLockInfo1.TabPageInfo, message);
			AssertNoError(tabLockInfo2.TabPageInfo, message);
			AssertNoError(tabLockInfo3.TabPageInfo, message);
		}

		#endregion

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);
			return lockConfig.TabInfos.AddNew();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);
			lockConfig.DeclarationType = "IMP";
			var lockInfo = lockConfig.TabInfos.AddNew();
			lockInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			return lockInfo;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
