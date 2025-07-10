using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PackingPlugInController))]
	public class PackingPlugInControllerTest : ZControllerBasherTest
	{
		#region TestID

		public virtual void TestID()
		{
			AssertEquals(ControllerIDs.PackingPlugIn, new PackingPlugInController().ID);
		}

		#endregion

		#region TestPlugInTabPageCaption

		public void TestPlugInTabPageCaption()
		{
			AssertEquals("Packing", new PackingPlugInController().PluginTabPageCaption.Caption);
		}

		#endregion

		#region TestPlugIn

		public void TestPlugIn()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPacking>();
			var pluginPackageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);

			using (var form = new ZForm())
			using (var plugins = new PlugIns(pluginPackageJob, form))
			{
				plugins.Add(GetControllerID());
				var plugin = plugins.GetPlugIn(GetControllerID());
				AssertNotNull(plugin);
			}
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(PkgPackageJob), new PackingPlugInController().TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PackingPlugIn;
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		public override void TestDeleteForm()
		{
			Assert("Not Supported", true);
		}

		public override void TestEditForm()
		{
			Assert("Not Supported", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Supported", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Supported", true);
		}

		#endregion
	}
}
