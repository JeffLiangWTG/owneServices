using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public partial class ValueAnalysisFilterControl : ZFilterStripControl
	{
		public ValueAnalysisFilterControl()
		{
			InitializeComponent();
		}

		public ValueAnalysisFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, string context, string product)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ProductCode = product;
			UpdateGridColumns();

			ShouldRunSearchOnStripsInitialized = false;
		}

		string ProductCode { get; set; }

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ValueAnalysisFilterStrip();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (FindForm() is ZForm form)
			{
				Env.Licence.SalesValueAnalysis.Login(form);
			}
		}

		class ValueAnalysisFilterStrip : ZFilterStrip
		{
			protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				Control[] result;
				if (currentModuleFilter is ValueAnalysisQuantityFilter)
				{
					result = new Control[] { new ValueAnalysisQuantityFilterControl(this) };
					FilterControlBindingSource.SetBindingMember(result[0], ".");
					SetHeight(PreferredHeight * 2);
				}
				else
				{
					result = base.GetCurrentFilterControls(currentModuleFilter);
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is a column name")]
		void UpdateGridColumns()
		{
			switch (ProductCode)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
					Grid.RemoveFromAvailableColumns("VVA_Service", "VVA_Warehouse");
					break;
				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					Grid.RemoveFromAvailableColumns("VVA_Service", "VVA_Warehouse", "Location");
					break;
				case SystemDefinedSalesProductList.Codes.LinerAgency:
					Grid.RemoveFromAvailableColumns("VVA_Service", "VVA_Warehouse", "Location", "VVA_TradeMode");
					break;
				case SystemDefinedSalesProductList.Codes.Transport:
					Grid.RemoveFromAvailableColumns("VVA_Service", "VVA_Warehouse", "Location", "VVA_TradeMode");
					break;
				case SystemDefinedSalesProductList.Codes.Warehouse:
					Grid.RemoveFromAvailableColumns("VVA_TradeMode", "VVA_TradeType");
					break;
			}
		}

		void grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (grid.SelectedElements.Length == 1)
			{
				var valueAnalysisRow = (ViewValueAnalysis)grid.SelectedElements[0];
				if (!valueAnalysisRow.VVA_OH_Primary.IsEmpty)
				{
					var organisation = GridCollection.Factory.Load<OrgHeader>(valueAnalysisRow.VVA_OH_Primary);
					ZControllerFactory.Create(ControllerIDs.Organisation).ShowEditForm(organisation);
				}
				else
				{
					ZArchitecture.Environment.Globals.Message.ShowError(Res.GetString("b028c00e-9f70-40c0-97a0-dcd6f9c82fb1", "This data row does not have a client."));
				}
			}
		}
	}
}
