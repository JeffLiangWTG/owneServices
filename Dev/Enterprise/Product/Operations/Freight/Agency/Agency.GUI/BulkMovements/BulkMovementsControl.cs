using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BulkMovementsControl : ZUserControl
	{
		public BulkMovementsControl()
		{
			InitializeComponent();
		}

		public void PerformAttachClick()
		{
			attachButton.PerformClick();
		}

		public void PerformDetachClick()
		{
			detachButton.PerformClick();
		}

		void DoAttach()
		{
			string dialogCaption = Res.GetString("2c5afb33-e366-48bc-9e4c-4ed1e551754b", "Attach");

			BulkMovementsHeader header = (BulkMovementsHeader)CurrentDataItem;

			BulkMovementsChild[] children;

			if ((children = childrenGrid.GetSelectedElements<BulkMovementsChild>()).Length == 0)
			{
				Globals.Message.Show(Res.GetString("7ca94216-95c8-4275-bb70-a568e4a30cf8", "No movements selected."), dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Array.TrueForAll(children, (c) => c.VoyagePK.IsEmpty))
			{
				string messageText = Res.GetString("75ee8117-16dc-4a52-8372-b11df72e0fbc", "At least one selected movement is already attached.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else
			{
				VoyageIFindBox.SelectSeaVoyage((ZForm)FindForm(), header.Factory, delegate(ZGuid voyagePK)
				{
					foreach (BulkMovementsChild child in children)
					{
						child.VoyagePK = voyagePK;
					}
				});
			}
		}

		void DoDetach()
		{
			string dialogCaption = Res.GetString("82878bfb-76a7-49a0-8474-39f826873333", "Detach");

			BulkMovementsChild[] children;

			if ((children = childrenGrid.GetSelectedElements<BulkMovementsChild>()).Length == 0)
			{
				Globals.Message.Show(Res.GetString("7ca94216-95c8-4275-bb70-a568e4a30cf8", "No movements selected."), dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (DialogResult.Yes == Globals.Message.Show(
				Res.GetString("9a80f7d1-48fe-42e1-8319-c83fb9868867", "Detach {0} movements?", children.Length),
				dialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes))
			{
				foreach (BulkMovementsChild child in children)
				{
					child.VoyagePK = ZGuid.Empty;
				}
			}
		}

		void attachButton_Click(object sender, EventArgs e)
		{
			DoAttach();
		}

		void detachButton_Click(object sender, EventArgs e)
		{
			DoDetach();
		}
	}
}


