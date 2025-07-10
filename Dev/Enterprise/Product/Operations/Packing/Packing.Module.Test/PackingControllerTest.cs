using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PackingController))]
	public class PackingControllerTest : ZControllerBasherTest
	{
		#region TestID

		public void TestID()
		{
			AssertEquals(ControllerIDs.Packing, new PackingController().ID);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Packing, new PackingController().ModuleID);
		}

		#endregion

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.PackingEdit, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.PackingView, Controller.GetCheckPointForView(null));
		}

		#endregion

		#region TestLoadBusinessEntity

		public void TestLoadBusinessEntity()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPacking>();
			var modulePackageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			modulePackageJob.Packages.AddNew();
			modulePackageJob.KJ_IsFinalized = true;
			Factory.Save();

			using (var form = (PackingForm)new PackingController().ShowEditForm(modulePackageJob))
			{
				var formPackageJob = (PkgPackageJob)form.BusinessEntity;
				AssertNotEquals("Precondition - If the module package job now matches the form package job, this code is unnecessary.", modulePackageJob, formPackageJob);
				AssertEquals("The module should use the PkgPackageJob loader to initalise the ReadOnly state.", true, formPackageJob.ReadOnly);
			}
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(PkgPackageJob), new PackingController().TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Packing;
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		#endregion
	}
}
