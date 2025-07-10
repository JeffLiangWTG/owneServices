using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public partial class ConsolidateChargeCodeForm : ZChildForm
	{
		ConsolidateChargeCodeForm()
		{
			InitializeComponent();
		}

		protected AccChargeCodesForConsolidationCollection chargeCodeCollection;
		protected ConsolidateChargeCodeFilterBusinessObject filterBusinessObject;
		protected AccChargeCodeFilterControl filterControl;

		public ConsolidateChargeCodeForm(BusinessObjectFactory factory) : this()
		{
			chargeCodeCollection = new AccChargeCodesForConsolidationCollection(factory);
			filterBusinessObject = new ConsolidateChargeCodeFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBusinessObject).LayoutContext = ModuleIDs.AccGlobalChargeCode.Name + ".ConsolidateChargeCodeForm";
			filterControl = new AccChargeCodeFilterControl(chargeCodeCollection, filterBusinessObject, AccChargeCodeFilterControl.Mode.Consolidation);
			filterControl.Dock = System.Windows.Forms.DockStyle.Fill;
			filterControl.PerformSearch += filterControl_PerformSearch;
			FilterControlPanel.Controls.Add(filterControl);
			SetupContextMenu();
		}

		void filterControl_PerformSearch(object sender, EventArgs e)
		{
			chargeCodeCollection.AdditionalFilter = filterBusinessObject.Filter;
		}

		void SetupContextMenu()
		{
			filterControl.Grid.ContextMenu.MenuItems.Add(
				new ZMenuItem(ResString.GetMultilingualString("ConsolidateChargeCodeForm|df4164e7-62a0-4ce3-8c96-4dd25e75cdf8", "Consolidate"),
					new EventHandler(OnConsolidateClicked)));
		}

		protected void OnConsolidateClicked(object sender, EventArgs e)
		{
			if (filterControl.Grid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("ConsolidateChargeCodeForm|4a47cf4b-3ff3-4a9c-8ac1-236050f9503a", "Please select one or more Charge Codes to consolidate"));
				return;
			}

			var chargeCodesToConsolidatePKs = filterControl.Grid.SelectedElements.Cast<AccChargeCode>().Select(c => c.PK).ToArray();
			var consolidateFactory = new BusinessObjectFactory();
			var chargeCodesInConsolidateFactory = consolidateFactory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, chargeCodesToConsolidatePKs));
			var consolidator = new AccGlobalChargeCodeConsolidator(consolidateFactory);
			var globalChargeCode = consolidator.ConsolidateToGlobal(chargeCodesInConsolidateFactory);
			var globalChargeCodeForm = new AccGlobalChargeCodeForm(globalChargeCode);
			globalChargeCodeForm.ControllerID = ControllerIDs.AccGlobalChargeCode;

			globalChargeCodeForm.Show();
#if DEBUG
			LastOpenedForm = globalChargeCodeForm;
#endif
		}

		void Close_Button_Click(object sender, EventArgs e)
		{
			Close();
		}

#if DEBUG
		public AccChargeCodeForm LastOpenedForm;
#endif
	}
}
