using System;
using Enterprise.Freight.GUI;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	public partial class VesselSelectForm : ZChildForm
	{
		public VesselSelectForm(QueryUserSelectVesselFromLloydsNumber businessEntity)
			: base(businessEntity)
		{
		}

		public new QueryUserSelectVesselFromLloydsNumber BusinessEntity
		{
			get { return (QueryUserSelectVesselFromLloydsNumber)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Implementation

		void OnOK_Click(object sender, EventArgs e)
		{
			CommitSelection();
		}

		void OnVesselsGrid_DoubleClick(object sender, EventArgs e)
		{
			CommitSelection();
		}

		void CommitSelection()
		{
			RefVessel[] selectedVessels = VesselsGrid.GetSelectedElements<RefVessel>();
			if (selectedVessels.Length == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("6174910b-1bf2-465a-8d23-74707f82011c", "Select a vessel from the list"));
			}
			else
			{
				BusinessEntity.SelectedVessel = selectedVessels[0];
				Close();
			}
		}

		#endregion
	}
}
