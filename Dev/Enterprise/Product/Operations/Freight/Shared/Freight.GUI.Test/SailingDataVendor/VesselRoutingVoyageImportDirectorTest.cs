using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.SailingDataVendor.Business;

namespace Enterprise.Freight.SailingDataVendor.GUI.Testing
{
	sealed class VesselRoutingVoyageImportDirectorTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			Director.KeepTickedPortPairs = true;
			AssertEquals("Returns true to keep the 'preview' dialog open", true, Director.Import());
			AssertNotNull("Import progress form should be shown", Director.LastShownForm);

			Director.KeepTickedPortPairs = false;
			AssertEquals("Returns false to close the 'preview' dialog", false, Director.Import());
		}

		#region Test Classes

		class TestVesselRoutingVoyageImportDirector : VesselRoutingVoyageImportDirector
		{
			public TestVesselRoutingVoyageImportDirector(Form parentForm, VesselRoutingVoyage[] voyages)
				: base(parentForm, voyages)
			{
			}

			public bool KeepTickedPortPairs;

			public VesselRoutingVoyageImportForm LastShownForm;
			protected override void ShowImportForm(VesselRoutingVoyageImportForm form)
			{
				form.Show();
				Application.DoEvents();

				LastShownForm = form;
				form.KeepTickedPortPairs = KeepTickedPortPairs;
			}

			protected override void OnImportCompleted()
			{
				base.OnImportCompleted();
				LastShownForm.Dispose();
			}
		}

		#endregion

		#region Implementation

		TestVesselRoutingVoyageImportDirector Director
		{
			get
			{
				if (fDirector == null)
				{
					fDirector = new TestVesselRoutingVoyageImportDirector(null, new VesselRoutingVoyage[] { Voyage });
				}
				return fDirector;
			}
		}
		TestVesselRoutingVoyageImportDirector fDirector;

		VesselRoutingVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					VesselRoutingVoyageCollection voyages = new VesselRoutingVoyageCollection(Factory);
					fVoyage = voyages.AddNew();
					fVoyage.E8_LloydsNumber = "Lloyds";
					fVoyage.E8_Voyage = "Voyage";

					VesselRoutingPortPair portPair = fVoyage.PortPairs.AddNew();
					portPair.E9_RL_NKLoadPort = "MYPKG";
					portPair.E9_RL_NKDischargePort = "AUSYD";
					return fVoyage;
				}
				return fVoyage;
			}
		}
		VesselRoutingVoyage fVoyage;

		#endregion
	}
}
