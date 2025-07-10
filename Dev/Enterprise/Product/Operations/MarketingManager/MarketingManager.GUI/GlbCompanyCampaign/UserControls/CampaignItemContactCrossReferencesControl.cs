using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignItemContactCrossReferencesControl : ZUserControl
	{
		public CampaignItemContactCrossReferencesControl()
		{
			InitializeComponent();
		}

		#region DataBinding

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			Grid.SetDataBinding(ContactCrossReferences, "");
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var campaignItem = CurrentDataItem as GlbCompanyCampaignItem;
			ContactCrossReferences.Load(campaignItem);
		}

		#endregion

		#region Events

		#region CrossReferenceSelected

		void grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Clicks > 1)
			{
				var hitInfo = Grid.HitTest(e.X, e.Y);
				if (hitInfo.Row >= 0 &&
					hitInfo.Row < Grid.ListManager.List.Count)
				{
					OnCrossReferenceSelected((OrgContactCampaignReferences)Grid.ListManager.List[hitInfo.Row]);
				}
			}
		}

		void OnCrossReferenceSelected(OrgContactCampaignReferences crossReference)
		{
			if (CrossReferenceSelected != null)
			{
				CrossReferenceSelected(this, new CrossReferenceEventArgs(crossReference));
			}
		}
		public event EventHandler<CrossReferenceEventArgs> CrossReferenceSelected;

		#endregion

		#endregion

		#region Implementation

		readonly CampaignItemContactCrossReferences ContactCrossReferences = new CampaignItemContactCrossReferences();

		#region Classes

		public class CrossReferenceEventArgs
		{
			public CrossReferenceEventArgs(OrgContactCampaignReferences crossReference)
			{
				CrossReference = crossReference;
			}

			public readonly OrgContactCampaignReferences CrossReference;
		}

		#endregion

		#endregion
	}
}
