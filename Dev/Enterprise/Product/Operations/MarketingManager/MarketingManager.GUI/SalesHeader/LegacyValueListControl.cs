using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class LegacyValueListControl : SalesHeaderListControl
	{
		public LegacyValueListControl(OrgOpportunity opptunity)
			: base()
		{
			this.opptunity = opptunity;
		}
		readonly OrgOpportunity opptunity;

		protected override void RefreshStrips()
		{
			base.RefreshStrips();
			if (opptunity.ValueItems.Count > 0 && !HasOpportunityValueStrip)
			{
				AddOpportunityValueStrip();
			}
		}

		protected override void SetupButtons()
		{
			base.SetupButtons();
			AddToolStripDropDownButton.DropDownItems.Add(new ZToolStripMenuItem(Res.GetString("f893397f-bc69-4d25-8ae9-fb3b277c0d24", "Legacy Value Analysis"), (o, e) => { Focus(null, false); }));
		}

		public override void Focus(OrgSalesProduct salesProduct, bool onNewRow)
		{
			if (salesProduct != null)
			{
				base.Focus(salesProduct, onNewRow);
			}
			else
			{
				AddLegacyValueStripIfNeeded();
				StripsPanel.Controls.OfType<LegacyValueStripControl>().First().Focus();
			}
		}

		public void AddLegacyValueStripIfNeeded()
		{
			if (!HasOpportunityValueStrip)
			{
				AddOpportunityValueStrip();
			}
		}

		bool HasOpportunityValueStrip
		{
			get { return StripsPanel.Controls.OfType<LegacyValueStripControl>().Any(); }
		}

		LegacyValueStripControl AddOpportunityValueStrip()
		{
			var headerStripControl = new LegacyValueStripControl();
			headerStripControl.Dock = DockStyle.Top;
			headerStripControl.ReadOnly = ReadOnly;
			headerStripControl.SetDataBinding(opptunity, "");
			StripsPanel.Controls.Add(headerStripControl);

			headerStripControl.Enter += HeaderStripControl_Enter;

			return headerStripControl;
		}
	}
}
