using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class TopLevelPacksControlTest : BaseAgencyTest
	{
		public void TestAdditionalColumns_UNDG()
		{
			RunControlTest(control => AssertEquals(true, GetPacksGrid(control).Columns.Contains("UNDGs+UNDGSubstanceManagerGuid+Value")));
		}

		public void TestHarmonisedCodeColumn()
		{
			RunControlTest(control =>
			{
				var harmonisedCodeColumnInfo = (TariffColumnStyleInfo)GetPacksGrid(control).ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == AgencyShipmentContainer.Schema.JC_HarmonisedCode);
				AssertNotNull(harmonisedCodeColumnInfo);
				AssertNull(harmonisedCodeColumnInfo.GetCountryCode?.Invoke());
				AssertEquals("WCO", harmonisedCodeColumnInfo.GetDataGrouping());
				AssertEquals("HSN", harmonisedCodeColumnInfo.TariffType);
			});
		}

		public void TestShowWorkflowForm()
		{
			RunControlTest(control =>
			{
				((ZForm)control.ParentForm).ControllerID = ControllerIDs.AgencyBooking;
				var shipment = control.CurrentDataItem as AgencyShipment;
				shipment.ShippingContainers.AddNew();
				new AgencyContainerWorkflowFormHelperTest().AssertMenuItems(GetPacksGrid(control), shipment.ShippingContainers[0]);
			});
		}

		public void TestRemoveColumnsWhichAreUnavailableOnBookings()
		{
			var columnsToBeRemoved = new[] { JobContainerSchema.Constants.JC_ArrivalCartageComplete, JobContainerSchema.Constants.JC_ArrivalSlotDateTime, JobContainerSchema.Constants.JC_ArrivalSlotReference, JobContainerSchema.Constants.JC_DepartureCartageComplete, JobContainerSchema.Constants.JC_DepartureSlotDateTime, JobContainerSchema.Constants.JC_DepartureSlotReference, JobContainerSchema.Constants.JC_FCLOnBoardVessel, JobContainerSchema.Constants.JC_FCLUnloadFromVessel, JobContainerSchema.Constants.JC_FCLWharfGateIn, JobContainerSchema.Constants.JC_FCLWharfGateOut, JobContainerSchema.Constants.JC_ContainerImportDORelease, JobContainerSchema.Constants.JC_StowagePosition, };
			RunControlTest(control =>
			{
				foreach (string columnName in columnsToBeRemoved)
				{
					AssertEquals(true, GetPacksGrid(control).Columns.Contains(columnName));
				}

				control.RemoveColumnsWhichAreUnavailableOnBookings();
				foreach (string columnName in columnsToBeRemoved)
				{
					AssertEquals(true, GetPacksGrid(control).Columns.Contains(columnName));
				}
			});
		}

		#region Implementation
		void RunControlTest(Action<TopLevelPacksControl> testAction)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			using (ZForm form = new ZForm(shipment))
			{
				var control = new TopLevelPacksControl();
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(shipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				testAction(control);
			}
		}

		ZGrid GetPacksGrid(TopLevelPacksControl control)
		{
			return (ZGrid)control.Controls.Find("TopLevelPacksGrid", true)[0];
		}
		#endregion
	}
}
