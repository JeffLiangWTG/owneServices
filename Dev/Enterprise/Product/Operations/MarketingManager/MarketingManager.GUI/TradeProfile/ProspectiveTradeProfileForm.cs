using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ProspectiveTradeProfileForm : ZChildForm
	{
		public ProspectiveTradeProfileForm()
		{
			InitializeComponent();
		}

		public ProspectiveTradeProfileForm(ISalesValueAssociatedEntity entity)
			: base(entity)
		{
			InitializeComponent();

			var bizObj = entity as BusinessObject;
			if (bizObj != null)
			{
				bizObj.RegisterEditableChildObject(entity.ProspectiveSalesHeaderCollection);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				var entity = BusinessEntity as BusinessObject;
				if (entity != null && entity.ReadOnly)
				{
					SaveButtonUserControl.SaveButton.Visible = false;
					SaveButtonUserControl.SaveAndCloseButton.Visible = false;
					SaveButtonUserControl.CloseButton.Click += (sender, eventArgs) => { this.Close(); };
				}
				else
				{
					ZFormPostingButtonsStrategy.SetupPosting(this, SaveButtonUserControl);
				}
			}
		}

		#region Focus

		public virtual void Focus(OrgSalesProduct salesProduct, bool onNewRow)
		{
			if (salesProduct != null)
			{
				var salesProductInLocalFactory = BusinessEntity.Factory.Load<OrgSalesProduct>(salesProduct.PK);
				if (salesProductInLocalFactory != null)
				{
					this.salesHeaderListControl.Focus(salesProductInLocalFactory, onNewRow);
				}
			}
		}

		public void Focus(EntitySalesWrapper entitySales)
		{
			if (entitySales != null)
			{
				var salesInLocalFactory = BusinessEntity.Factory.Load<OrgSales>(entitySales.PK);
				if (salesInLocalFactory != null)
				{
					this.salesHeaderListControl.Focus(entitySales);
				}
			}
		}

		#endregion

		#region BusinessEntity

		public new ISalesValueAssociatedEntity BusinessEntity
		{
			get { return (ISalesValueAssociatedEntity)base.BusinessEntity; }
		}

		#endregion

		#region Caption

		public override string FormCaption
		{
			get { return Res.GetString("bc671f14-4c25-4416-950e-abd449f8c3d2", "{0}: Estimate Values", BusinessEntity.ID); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region IPostingButtonsProvider Members

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var entity = (ISalesValueAssociatedEntity)base.DataSource;

			if (entity?.ProspectiveSalesHeaderCollection is SalesHeaderCollection collection)
			{
				var supercedingWarningMessage = collection.SupercedingWarningMessage;
				if (!supercedingWarningMessage.IsEmpty)
				{
					var dialogResult = TradeDetailCommitmentUpdaterGUIManager.ShowSupercedingConfirmation(supercedingWarningMessage);

					if (DialogResult.No == dialogResult)
					{
						return ContinueWithSave.No;
					}
				}
			}

			return base.ShowPreSaveDialogs();
		}
	}
}
