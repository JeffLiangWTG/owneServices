using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class OpportunityProspectiveTradeProfileForm : ProspectiveTradeProfileForm
	{
		public OpportunityProspectiveTradeProfileForm()
		{
			InitializeComponent();
		}

		public OpportunityProspectiveTradeProfileForm(OrgOpportunity opportunity)
			: base(opportunity)
		{
			InitializeComponent();

			Env.Licence.SalesValueAnalysis.Login(this);

			this.businessEntityForValidation = new OpportunityProspectiveTradeProfileBizObjForValidation();
			this.businessEntityForValidation.RegisterEditableChildObjects(opportunity.ValueItems, opportunity.ProspectiveSalesHeaderCollection);

			this.Controls.Remove(this.bottomPanel);
			this.Controls.Remove(this.salesHeaderListControl);
			this.salesHeaderListControl = new LegacyValueListControl(opportunity);
			this.salesHeaderListControl.Dock = DockStyle.Fill;
			this.salesHeaderListControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.salesHeaderListControl, ".");
			this.Controls.Add(this.salesHeaderListControl);
			this.Controls.Add(this.bottomPanel);
		}

		#region Focus

		public override void Focus(OrgSalesProduct salesProduct, bool onNewRow)
		{
			if (salesProduct != null)
			{
				base.Focus(salesProduct, onNewRow);
			}
			else
			{
				((LegacyValueListControl)this.salesHeaderListControl).Focus(null, onNewRow);
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region HasChanges

		public override IBusiness BusinessEntityForHasChanges
		{
			get { return businessEntityForValidation; }
		}

		#endregion

		#region Validation

		protected override IBusiness BusinessEntityForValidation
		{
			get { return businessEntityForValidation ?? base.BusinessEntityForValidation; }
		}
		readonly OpportunityProspectiveTradeProfileBizObjForValidation businessEntityForValidation;

#if DEBUG
		internal
#endif
		class OpportunityProspectiveTradeProfileBizObjForValidation : NonPersistentBusinessObject
		{
			public void RegisterEditableChildObjects(params IBusiness[] children)
			{
				foreach (var child in children)
				{
					RegisterEditableChildObject(child);
				}
			}
		}

		#endregion
	}
}
