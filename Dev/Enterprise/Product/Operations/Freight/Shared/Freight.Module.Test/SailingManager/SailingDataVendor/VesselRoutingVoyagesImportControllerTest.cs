using Enterprise.Environment;
using Enterprise.Freight.SailingDataVendor.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Module.Testing
{
	[TestedType(typeof(SailingScheduleImportingController))]
	sealed class VesselRoutingVoyagesImportControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.SailingScheduleImporting, Controller.CheckPointForNew);
		}

		public void TestGetForm()
		{
			using (VesselRoutingVoyagesImportPreviewForm shownForm = (VesselRoutingVoyagesImportPreviewForm)Controller.ShowNewForm())
			{
				AssertNotNull("Correct form type should be shown", shownForm);
			}
		}

		#region Test Classes

		class TestSailingScheduleImportingController : SailingScheduleImportingController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}
		}

		#endregion

		#region Implementation

		new TestSailingScheduleImportingController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new TestSailingScheduleImportingController();
				}
				return fController;
			}
		}
		TestSailingScheduleImportingController fController;

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SailingScheduleImporting;
		}

		#endregion
	}
}
