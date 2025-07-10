using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CusStatementModule))]
	sealed class CusStatementModuleTest : ZModuleBasherWithFetchHintsTest
	{
		public void TestGetNewActionMenuItems()
		{
			using (CusStatementModule module = new CusStatementModule())
			{
				var formActionsMenu = module.FormActionMenu;
				Assert("Must have ReqDoc action", module.ActionsMenuItem.MenuItems.OfType<MenuItem>().Any(x => x.Text.Equals("Customs Statement Request (REQDOC)")));
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (CusStatementModule module = new CusStatementModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsStatement), module.SecurityCheckpoint);
			}
		}

		public void TestStatementModuleAllows()
		{
			using (CusStatementModule module = new CusStatementModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowView", false, module.AllowView);
				AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
			}
		}

		public override void TestModuleShowsAndCanSearch()
		{
			CreateStatementForFetchHintTest(0);
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.CustomsStatement;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override bool HasController() => true;

		protected override ZFilterModule CreateModuleForFetchHintsTest() => new CusStatementModule();

		protected override void SetupDataForFetchHintsTest()
		{
			for (int i = 0; i < 20; i++)
			{
				CreateStatementForFetchHintTest(i);
			}

			Factory.Save();
		}

		void CreateStatementForFetchHintTest(int i)
		{
			var number = i.ToString();
			var charge = Factory.NewWithValidTestData<CusStatementLineCharge>();
			charge.B4_ChargeType = "V";
			charge.B4_ChargeAmount = i;
			charge.Line.Header.B2_AccountNo = number;
			charge.Line.Header.B2_ProcessDate = ZDateTime.Now;
			charge.Line.B3_EntryNum = number;
		}
	}
}
