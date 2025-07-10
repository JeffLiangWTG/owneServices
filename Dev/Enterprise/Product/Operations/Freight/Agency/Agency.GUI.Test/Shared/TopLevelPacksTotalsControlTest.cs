using System;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class TopLevelPacksTotalsControlTest : BaseAgencyTest
	{
		public void TestUpdateTotals()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			using (var form = new ZForm(shipment))
			{
				var control = new TopLevelPacksTotalsControl();
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(shipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				bool totalPacksUpdated = false;
				shipment.TopLevelPacksTotalPacksInfo.ValueChanged += (s, e) => totalPacksUpdated = true;
				bool totalVolumeUpdated = false;
				shipment.TopLevelPacksTotalVolumeInShipmentVolumeUnitInfo.ValueChanged += (s, e) => totalVolumeUpdated = true;
				bool totalWeightUpdated = false;
				shipment.TopLevelPacksTotalWeightInShipmentWeightUnitInfo.ValueChanged += (s, e) => totalWeightUpdated = true;
				Action<Action> assertTotalsUpdated = (triggeringAction) =>
				{
					totalPacksUpdated = false;
					totalVolumeUpdated = false;
					totalWeightUpdated = false;
					triggeringAction();
					AssertEquals(true, totalPacksUpdated);
					AssertEquals(true, totalVolumeUpdated);
					AssertEquals(true, totalWeightUpdated);
				};
				AgencyShipmentContainer topLevelPack = null;
				assertTotalsUpdated(() => topLevelPack = shipment.TopLevelPacks.AddNew());
				AssertEquals(true, shipment.TopLevelPacks.Contains(topLevelPack));
				assertTotalsUpdated(() => topLevelPack.JC_ContainerCount = 10);
				assertTotalsUpdated(() => topLevelPack.JC_GrossVolume = 10);
				assertTotalsUpdated(() => topLevelPack.JC_GrossWeight = 10);
			}
		}
	}
}
