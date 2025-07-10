using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ProfitShareAgreementControl : ZUserControl
	{
		public ProfitShareAgreementControl()
		{
			InitializeComponent();
			ProfitShareGrid.SelectedRowsChangedInMouseDown += ProfitShareGrid_SelectedRowsChangedInMouseDown;
		}

		void ProfitShareGrid_SelectedRowsChangedInMouseDown(object sender, System.EventArgs e)
		{
			if (ProfitShareGrid.SelectedRowCount <= 0)
			{
				return;
			}
			var details = ProfitShareGrid.GetFirstSelectedRow() as OrgProfitShareDetails;
			var agreement = details?.OrgProfitShareHeader;
			if (agreement != null && AgreementGrid != null)
			{
				AgreementGrid.SelectSingleElement(agreement);
				AgreementGrid.Invalidate();
			}
		}

		public void SetAgreementGrid(ZDisplayGrid agreementGrid)
		{
			AgreementGrid = agreementGrid;
		}
		ZDisplayGrid AgreementGrid { get; set; }

		#region Client Override Column

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool IncludeOrgOverrideColumn
		{
			get { return fIncludeOrgOverrideColumn; }
			set
			{
				fIncludeOrgOverrideColumn = value;
				if (!IncludeOrgOverrideColumn)
				{
					ProfitShareGrid.RemoveFromAvailableColumns(OrgProfitShareDetails.Schema.O4_OrgOverrideType);
					ProfitShareGrid.RemoveFromAvailableColumns(OrgProfitShareDetails.Schema.O4_OH_OrgOverride);
				}
			}
		}

		bool fIncludeOrgOverrideColumn;

		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				BindingManagerBase bindingManager = BindingContext[dataSource, dataMember];
				CurrencyManager currencyManager = bindingManager as CurrencyManager;

				if (currencyManager != null)
				{
					currencyManager.CurrentChanged -= ListManager_CurrentChanged;
				}

				SelectedProfitShareDetails = null;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				BindingManagerBase bindingManager = BindingContext[dataSource, dataMember];
				CurrencyManager currencyManager = bindingManager as CurrencyManager;

				if (currencyManager != null)
				{
					currencyManager.CurrentChanged += ListManager_CurrentChanged;
					SelectedProfitShareDetails = currencyManager.Count > 0 ? (OrgProfitShareDetails)currencyManager.GetCurrent() : null;
				}
			}
		}

		OrgProfitShareDetails SelectedProfitShareDetails
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return selectedProfitShareDetails; }
			set
			{
				if (selectedProfitShareDetails != value)
				{
					if (selectedProfitShareDetails != null)
					{
						UnHookProfitShareDetails(selectedProfitShareDetails);
					}

					selectedProfitShareDetails = value;

					if (selectedProfitShareDetails != null)
					{
						HookProfitShareDetails(selectedProfitShareDetails);
					}

					SetGatewayConsolProfitTabPageVisibility();
				}
			}
		}
		OrgProfitShareDetails selectedProfitShareDetails;

		void UnHookProfitShareDetails(OrgProfitShareDetails profitShare)
		{
			profitShare.O4_GatewayProfitApportionmentMethodInfo.ValueChanged -= O4_GatewayProfitApportionmentMethodInfo_ValueChanged;
		}

		void HookProfitShareDetails(OrgProfitShareDetails profitShare)
		{
			profitShare.O4_GatewayProfitApportionmentMethodInfo.ValueChanged += O4_GatewayProfitApportionmentMethodInfo_ValueChanged;
		}

		void O4_GatewayProfitApportionmentMethodInfo_ValueChanged(object sender, System.EventArgs e)
		{
			SetGatewayConsolProfitTabPageVisibility();
		}

		void ListManager_CurrentChanged(object sender, System.EventArgs e)
		{
			if (ProfitShareGrid.ListManager != null && ProfitShareGrid.ListManager.Count > 0)
			{
				SelectedProfitShareDetails = (OrgProfitShareDetails)ProfitShareGrid.ListManager.GetCurrent();
			}
			else
			{
				SelectedProfitShareDetails = null;
			}
		}

		void SetGatewayConsolProfitTabPageVisibility()
		{
			var visible = !SelectedProfitShareDetails?.O4_GatewayProfitApportionmentMethod.IsEmpty ?? false;
			PartyDetailsControl.SetGatewayConsolProfitRedistributionTabPageVisibility(visible);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ProfitShareGrid.SelectedRowsChangedInMouseDown -= ProfitShareGrid_SelectedRowsChangedInMouseDown;
			}
			base.Dispose(disposing);
		}

		#endregion

		/// <summary>
		/// Expose ProfitShareGrid for OrgProfitShareForm to access it.
		/// </summary>
		internal ZGrid ProfitShareGrid { get; private set; }
		internal ProfitSharePartyDetailsControl PartyDetailsControl { get; private set; }
	}
}
