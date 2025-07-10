using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGlobalChargeCodeModule))]
	sealed class AccGlobalChargeCodeModuleTest : ZModuleBasherTest
	{
		public AccGlobalChargeCodeModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccGlobalChargeCode;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccGlobalChargeCodeModule module = new AccGlobalChargeCodeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlobalChargeCodes, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected override void BashModule(ZFilterModule module)
		{
			var globalChargeCode = AccChargeCode.CreateGlobalChargeCode(Factory);
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			globalChargeCode.AC_Code = "CHARGE";
			globalChargeCode.AC_Desc = "GlobalChargeCode";
			Factory.Save();
			base.BashModule(module);
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccGlobalChargeCodeModuleForTest module = new AccGlobalChargeCodeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Is valid type", filterControl is AccChargeCodeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccGlobalChargeCodeModuleForTest module = new AccGlobalChargeCodeModuleForTest())
			{
				IBusinessObjectCollection chargeCodeCollection = module.NewGridCollection;
				Assert("Invalid type", chargeCodeCollection is AccGlobalChargeCodeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccGlobalChargeCodeModuleForTest module = new AccGlobalChargeCodeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccGlobalChargeCodeFilterBusinessObject);
			}
		}

		#endregion
	}
}
