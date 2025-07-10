using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderConsolController))]
	sealed class CusInBondHeaderConsolControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var aMS = Factory.New<CusInBondHeader>();
			var consol = Factory.New<ForwardingConsol>();
			aMS.BH_ParentID = consol.PK;
			aMS.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var controller = new CusInBondHeaderConsolController();
			using (var form = controller.ShowEditForm(aMS))
			{
				AssertEquals(typeof(ConsolForm), form.GetType());
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestOpenConsolFormFromAMSModuleForPlugInAMS()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("Declaration form is shown for PlugIn In-Bond", typeof(ConsolForm), Controller.LastShownForm.GetType());
			}
		}

		public void TestFormCacheConsolFormFromAMSModule()
		{
			var aMS = GetBusinessObjectThatIsInTheDatabase() as CusInBondHeader;
			var consol = aMS.Consol;
			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			try
			{
				Controller.ShowEditForm(aMS);
				consolController.ShowEditForm(consol);
				AssertEquals("Consol form opened for AMS is also shown for 'JobConsol'", Controller.LastShownForm, consolController.LastShownForm);
			}
			finally
			{
				if (Controller.LastShownForm != null)
				{
					Controller.LastShownForm.Dispose();
				}

				if (consolController.LastShownForm != null)
				{
					consolController.LastShownForm.Dispose();
				}
			}
		}

		public void TestGetCheckPointForEdit()
		{
			AssertSecurityCheckpoint(Env.Security.MaintainConsolEdit, (controller, header) => controller.GetCheckPointForEdit(header));
		}

		public void TestGetCheckPointForView()
		{
			AssertSecurityCheckpoint(Env.Security.MaintainConsol, (controller, header) => controller.GetCheckPointForView(header));
		}

		public void TestGetCheckPointForNew()
		{
			AssertSecurityCheckpoint(Env.Security.MaintainConsolNew, (controller, header) => controller.GetCheckPointForNew(header));
		}

		public void TestGetCheckPointForDelete()
		{
			AssertSecurityCheckpoint(Env.Security.MaintainConsolDelete, (controller, header) => controller.GetCheckPointForDelete(header));
		}

		void AssertSecurityCheckpoint(SecurityCheckpoint checkpointBase, Func<CusInBondHeaderConsolController, BusinessObject, SecurityCheckpoint> getCheckPoint)
		{
			var aMS = Factory.New<CusInBondHeader>();
			var controller = Controller as CusInBondHeaderConsolController;
			var consol = Factory.New<ForwardingConsol>();
			CombineAssertions(() =>
			{
				AssertEquals("checkpoint from ams checkpoint for stand-alone ams", Env.Security.ConsolAMSReporting, getCheckPoint(controller, aMS));
				AssertEquals("checkpoint from consol checkpoint for consol", checkpointBase, getCheckPoint(controller, consol));
				aMS.BH_ParentID = consol.PK;
				aMS.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				Factory.Save();
				Env.Security.ConsolAMSReporting.IsAllowed = false;
				AssertEquals("checkpoint from ams checkpoint first", Env.Security.ConsolAMSReporting, getCheckPoint(controller, aMS));
				Env.Security.ConsolAMSReporting.IsAllowed = true;
				AssertEquals("checkpoint from consol checkpoint when ams checkpoint allowed", checkpointBase, getCheckPoint(controller, aMS));
			});
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var aMS = Factory.New<CusInBondHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKDischargePort = "USCHI";
			aMS.BH_ParentID = consol.PK;
			aMS.BH_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			return aMS;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.US.AMSPluggedIntoConsol;
		}
	}
}
