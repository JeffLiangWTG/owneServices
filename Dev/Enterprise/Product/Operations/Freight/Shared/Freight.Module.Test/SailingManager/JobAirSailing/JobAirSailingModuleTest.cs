using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobAirSailingModule))]
	sealed class JobAirSailingModuleTest : ZModuleBasherTest
	{
		#region Bulk Copy

		public void TestCopyMenuItems()
		{
			using (JobAirSailingModule module = new JobAirSailingModule())
			{
				AssertEquals(2, module.ToolBarButtons.FindByText("&Copy").DropDownMenu.MenuItems.Count);
				MenuItem singleCopy = module.ToolBarButtons.FindByText("&Copy").DropDownMenu.MenuItems[0];
				AssertEquals("&Single Copy", singleCopy.Text);
				MenuItem bulkCopy = module.ToolBarButtons.FindByText("&Copy").DropDownMenu.MenuItems[1];
				AssertEquals("&Bulk Copy", bulkCopy.Text);
			}
		}

		public void TestBulkCopyClick()
		{
			using (JobAirSailingModule module = new JobAirSailingModule())
			using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
			{
				popup.Show();

				module.BulkCopyClick(null, null);
				AssertNull("BulkCopy Form should not be shown", ZFormModaliser.LastFormShownDialogForTest);

				module.GridCollection_ExposedForTest.Add(Sailing);
				module.Grid_ExposedForTest.Select(0);
				module.BulkCopyClick(null, null);
				AssertNotNull("BulkCopy Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("BulkCopy form type", typeof(BulkScheduleCopyForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("BulkCopyCriteria type", "ForwardingBulkCopyCriteria", ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType().Name);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobAirSailing;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Sailing = CreateSailing();
		}

		BaseJobSailing CreateSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "CX123";

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 12, 12, 12, 0, 0);
			origin.JA_DocumentaryCutoff = new ZDateTime(2006, 12, 13, 13, 45, 0);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = new ZDateTime(2006, 12, 13, 10, 59, 0);

			BaseJobSailing sailing = Factory.New<BaseJobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2006, 10, 12, 12, 0, 0);
			sailing.JX_DepotCutOff = sailing.JX_DepotReceivalCommences;
			sailing.JX_DepotStorageDate = sailing.JX_JA_DocumentaryCutoff;
			Factory.Save();

			return sailing;
		}

		BaseJobSailing Sailing;

		#endregion
	}
}
