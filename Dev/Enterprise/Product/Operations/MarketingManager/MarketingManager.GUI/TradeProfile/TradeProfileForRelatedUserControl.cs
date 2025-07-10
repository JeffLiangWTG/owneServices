using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeProfileForRelatedUserControl : ZUserControl, IReadOnlyToggleControl
	{
		public TradeProfileForRelatedUserControl()
		{
			InitializeComponent();
			TypeDescriptor.AddAttributes(newProspectValueButton, new CanBeReadOnlyUIAttribute());
			TypeDescriptor.AddAttributes(editProspectValuesButton, new CanBeReadOnlyUIAttribute());
			TypeDescriptor.AddAttributes(UpdateProspectStatusButton, new CanBeReadOnlyUIAttribute());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			dynamicTradeLanesControl.SetReadOnlyIncludingChildren();

			salesValueAnalysisControl.GridRowDoubleClicked += SalesValueAnalysisControl_GridDoubleClicked;
		}

		#region CurrentDataItem

		public new BusinessObject CurrentDataItem
		{
			get { return (BusinessObject)base.CurrentDataItem; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			SetSalesValueAnalysisCollection(dataSource != null ? new OpportunitySalesValueAnalysisCollection((OrgOpportunity)dataSource) : null);
			if (SalesValueAnalysisCollection != null)
			{
				SalesValueAnalysisCollection.Refresh();
			}

			SetupProspectValueButtons();
			SetupSplitContainer();

			base.SetDataBinding(dataSource, dataMember);
		}

		public OpportunitySalesValueAnalysisCollection SalesValueAnalysisCollection
		{
			get { return salesValueAnalysisCollection; }
		}
		OpportunitySalesValueAnalysisCollection salesValueAnalysisCollection;

		public void SetSalesValueAnalysisCollection(OpportunitySalesValueAnalysisCollection collection)
		{
			if (salesValueAnalysisCollection != collection)
			{
				salesValueAnalysisCollection = collection;
				salesValueAnalysisControl.SetDataBinding(collection, null);
				dynamicTradeLanesControl.SetDataBinding(collection, null);
			}
		}

		#endregion

		#region SetupSplitContainer

		void SetupSplitContainer()
		{
			if (SalesValueAnalysisCollection != null)
			{
				this.splitContainer.Panel2Collapsed = SalesValueAnalysisCollection.Count == 0;
			}
		}

		#endregion

		#region SalesValueAnalysisControl

		void SalesValueAnalysisControl_GridDoubleClicked(object sender, EventArgs e)
		{
			if (!ReadOnly)
			{
				EditProspectValuesButton_Click(null, EventArgs.Empty);
			}
		}

		#endregion

		#region IReadOnlyToggleControl Members

		[DefaultValue(false)]
		public bool ReadOnly { get; set; }

		#endregion

		#region SetupProspectValueButtons

		void SetupProspectValueButtons()
		{
			newProspectValueButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			editProspectValuesButton.Image = Icons.GetImage(IconTypes.EditButtonRest);
			UpdateProspectStatusButton.Image = Icons.GetImage(IconTypes.ActionsButtonRest);

			newProspectValueButton.DropDown.Items.Clear();

			if (SalesValueAnalysisCollection != null)
			{
				var products = GetProducts();
				foreach (var product in products)
				{
					newProspectValueButton.DropDown.Items.Add(new ZToolStripMenuItem(EscapeMnemonics(product.MP_NameMultilingual), (o, e) => { ShowProspectiveTradeProfileForm(product, true); }));
				}

				if (CurrentDataItem is OrgOpportunity)
				{
					newProspectValueButton.DropDown.Items.Add(new ZToolStripMenuItem(Res.GetString("77f38bd5-3e3b-4046-a11c-9ba8f5c0a311", "Legacy Value Analysis"), (o, e) => { ShowProspectiveTradeProfileForm(null, false); }));
					newProspectValueButton.Enabled = true;
				}
				else
				{
					newProspectValueButton.Enabled = products.Any();
				}

				editProspectValuesButton.Enabled = (SalesValueAnalysisCollection.Count > 0);
				UpdateProspectStatusButton.Enabled = (SalesValueAnalysisCollection.Count > 0);
			}

			if (CurrentDataItem != null && CurrentDataItem.ReadOnly)
			{
				newProspectValueButton.Enabled = false;
				editProspectValuesButton.Enabled = false;
				UpdateProspectStatusButton.Enabled = false;
			}
		}

		IOrderedEnumerable<OrgSalesProduct> GetProducts()
		{
			var factory = SalesValueAnalysisCollection.Factory;
			var allProducts = factory.Load<OrgSalesProduct>(new ZQuery());
			return allProducts.OrderBy(x => x.MP_NameMultilingual);
		}

		static MultilingualString EscapeMnemonics(MultilingualString text)
		{
			return text.Replace("&", "&&");
		}

		#endregion

		#region EditProspectValuesButton

		void EditProspectValuesButton_Click(object sender, EventArgs e)
		{
			OrgSalesProduct salesProduct = null;
			if (SalesValueAnalysisCollection != null && SalesValueAnalysisCollection.Count > 0 && salesValueAnalysisControl.Grid.ListManager != null)
			{
				var salesValueAnalysis = salesValueAnalysisControl.Grid.ListManager.GetCurrent() as OpportunitySalesValueAnalysis;
				if (salesValueAnalysis != null && salesValueAnalysis.SalesHeader != null)
				{
					salesProduct = salesValueAnalysis.SalesHeader.SalesProduct;
				}
			}

			ShowProspectiveTradeProfileForm(salesProduct, false);
		}

		#endregion

		#region UpdateProspectStatusButton

		void UpdateProspectStatusButton_Click(object sender, EventArgs e)
		{
			if (DataSource is OrgOpportunity opportunity)
			{
				new TradeDetailCommitmentUpdaterGUIManager().ShowForm(opportunity, e);
				RefreshCalculatedEstimatedValues();
			}
		}

		#endregion

		#region RefreshCalculatedEstimatedValues

		public void RefreshCalculatedEstimatedValues()
		{
			if (DataSource is OrgOpportunity opportunity)
			{
				opportunity.RefreshCalculatedEstimatedValues();
				salesValueAnalysisControl.Grid.Refresh();
				dynamicTradeLanesControl.Refresh();
			}
		}

		#endregion

		#region RefreshSalesValueAnalysisCollection

		public void RefreshSalesValueAnalysisCollection()
		{
			SalesValueAnalysisCollection?.Refresh();
		}

		#endregion

		#region ShowProspectiveTradeProfileForm

		void ShowProspectiveTradeProfileForm(OrgSalesProduct salesProduct, bool onNewRow)
		{
			var related = CurrentDataItem;
			if (related == null)
			{
				return;
			}

			if (!related.IsInDatabase || related.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("dc18c17f-8f07-46b3-a952-e292f1263049", "Cannot add or edit estimate values until {0} is saved.", related.HumanReadableName),
					CannotAddOrEditProspectValuesCaption);
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var relatedInNewFactory = newFactory.Load(related.TablePrefix, related.PK) as ISalesValueAssociatedEntity;
			if (relatedInNewFactory == null)
			{
				Globals.Message.ShowError(
					Res.GetString("f8c2feb2-622a-4375-b466-4a3076a5c0b2", "{0} has been deleted.", related.HumanReadableName),
					CannotAddOrEditProspectValuesCaption);
				return;
			}

			ProspectiveTradeProfileForm form;
			if (related is OrgOpportunity)
			{
				form = new OpportunityProspectiveTradeProfileForm((OrgOpportunity)relatedInNewFactory);
			}
			else
			{
				form = new ProspectiveTradeProfileForm(relatedInNewFactory);
			}

			EventHandler formShownHandler = null;
			formShownHandler = (o, e) =>
			{
				form.Focus(salesProduct, onNewRow);
				form.Shown -= formShownHandler;
			};
			form.Shown += formShownHandler;
			form.FormClosed += ProspectiveTradeProfilesForm_FormClosed;

			ZFormModaliser.Show(form, ParentForm);
		}

		void ProspectiveTradeProfilesForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			((ZForm)sender).FormClosed -= ProspectiveTradeProfilesForm_FormClosed;
			if (salesValueAnalysisCollection != null)
			{
				salesValueAnalysisCollection.Refresh(true);
				SetupSplitContainer();
				SetupProspectValueButtons();
			}

			RefreshCalculatedEstimatedValues();
		}

		static string CannotAddOrEditProspectValuesCaption
		{
			get { return Res.GetString("00b30513-1432-49cd-8406-68cef6581e9a", "Cannot Add / Edit Estimate Values"); }
		}

		#endregion
	}
}
