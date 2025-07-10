using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public class RatingForm : ZForm     // Could be abstract, but then the designer blows up
	{
		protected RatingForm()
		{
		}

		readonly ZMenuItem importIATARateMenuItem;

		public RatingForm(RatingHeader header)
			: base(header)
		{
			CurrentHeader = header;

			CurrentHeader.InvalidateRateLines += CurrentHeader_InvalidateRateLines;
			Saved += RatingForm_Saved;

			importIATARateMenuItem = new ZMenuItem(RatingFormActionsMenu.ImportIATATACT, ImportIATATACTRatesEvent);

			ActionsMenuItem.MenuItems.Add(importIATARateMenuItem);

			if (CurrentHeader.SupportsRateEntryPublish())
			{
				var menuDescription = CurrentHeader.IsCosting()
					? RatingFormActionsMenu.PublishAllCostsAsGlobalCosts
					: RatingFormActionsMenu.PublishAllRatesAsGlobalRates;

				ActionsMenuItem.MenuItems.Add(new ZMenuItem(menuDescription, PublishAll));
			}

			RefreshActionMenuItem();

			if (RateDataImporter.IsActive)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
				{
					ActionsMenuItem.MenuItems.Add(new ZMenuItem(RatingFormActionsMenu.ImportForwardAirCosts, ImportForwardingAirCostsEvent));
				}
			}

			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);

			DisplayModeChanged += new DisplayModeChangedEventHandler(ParentForm_DisplayModeChanged);
		}

		void ParentForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				// This is the better for performance, compared to SetReadOnlyIncludingChildren(true).
				// SetReadOnlyIncludingChildren would set readOnly to all loaded RateEntries/RateLines/RateLineItems 2x i.e. here and later in the ZForm > OnLoad > SetAllControlsReadOnly
				CurrentHeader.IsFormDelete = true;
			}
		}

		protected override void HandleSaveException(Exception ex)
		{
			string errorMessage = null;

			try
			{
				errorMessage = RateEntryCollectionValidator.HandleOverlapException(CurrentHeader, ex);
			}
			catch (Exception ex2) //for example, unhandled concurrency error (WI00214397). ZForm.HandleSaveException will try/catch and show proper error message to user.
			{
				if (ex2.IsCriticalException())
				{ throw; }
				base.HandleSaveException(ex);
			}
			if (string.IsNullOrEmpty(errorMessage))
			{
				return;
			}

			Globals.Message.ShowError(errorMessage);
		}

		protected void RatingForm_Saved(object sender, EventArgs e)
		{
			RefreshActionMenuItem();
		}

		void RefreshActionMenuItem()
		{
			if (RateDataImporter.IsActive)
			{
				if (RateDataImporter.EnableIATARateImport)
				{
					importIATARateMenuItem.Visible = true;
				}
				else
				{
					importIATARateMenuItem.Visible = false;
				}
			}
			else
			{
				importIATARateMenuItem.Visible = false;
			}
		}

		public List<MenuItem> ExportToXmlMenuItem
		{
			get { return new ExportToXmlMenuItemSet<RatingHeader>(() => Exporter, CurrentHeader); }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public IXmlDataTransferExporter Exporter
		{
			get { return new XmlDataTransferExporter(ValueObjectDataAdapter, true); }
		}

		public RateDataImporter RateDataImporter
		{
			get { return fRateDataImporter ?? (fRateDataImporter = RateDataImporter.New(CurrentHeader)); }
		}
		RateDataImporter fRateDataImporter;

		protected virtual IValueObjectDataAdapter ValueObjectDataAdapter
		{
			get { throw new InvalidOperationException("Please specify a ValueObjectDataAdapter for exporting a rate of type " + CurrentHeader.TH_RateType); }
		}

		public RatingHeader CurrentHeader { get; }

		public override string FormCaption
		{
			get
			{
				if (CurrentHeader != null)
				{
					ZString result = CurrentHeader.HumanReadableName;
					if (CurrentHeader.IsTariff())
					{
						result += " " + CurrentHeader.TH_GlobalRateLevel;
					}
					else if (CurrentHeader.IsQuote())
					{
						if (!CurrentHeader.TH_QuoteNumber.IsEmpty)
						{
							result += " " + CurrentHeader.TH_QuoteNumber;
						}
					}
					else if (!CurrentHeader.IsDeleted && CurrentHeader.Header != null)
					{
						result += " " + CurrentHeader.Header.OH_Code;
					}

					return result;
				}

				return base.FormCaption;
			}
		}

		protected override IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return CurrentHeader;
		}

		internal BaseTabControl BaseTabControl
		{
			get { return (BaseTabControl)TopLevelTabControl.Parent; }
		}

		#region WiseRates Menu Items

		protected void InitializeRatesServiceMenuItems()
		{
			if (rateProviderAccessAndMenu != null)
			{
				return;
			}

			var ratesServiceMenu = new ZMenuItem(ResString.GetMultilingualString("9a7493bf-8a48-4e1d-8355-3f05a62af211", "Rates Service"));
			MainMenu.MenuItems.Add(3, ratesServiceMenu);

			rateProviderAccessAndMenu = new RateProviderAccessAndMenu(ratesServiceMenu, CurrentHeader);
		}

		RateProviderAccessAndMenu rateProviderAccessAndMenu;

		#endregion

		#region HighlightSelectedEntry

		public BusinessObject[] SelectSingleEntry(RateEntry entry)
		{
			CurrentHeader.ReloadCollectionsWithNoFilter();

			var tabControl = BaseTabControl;
			if (tabControl != null)
			{
				var tabPage = tabControl.FindSelectedEntryTabPage(TopLevelTabControl);
				if (tabPage != null)
				{
					var grid = tabPage.RateEntryGrid;
					grid.SelectSingleElement(entry);
					return grid.SelectedElements;
				}
			}

			return null;
		}

		#endregion

		#region Data Import

		void ImportForwardingAirCostsEvent(object sender, EventArgs e)
		{
			ImportRates(false, true, RateDataImporter.ImportForwardAirRates);
		}

		void ImportIATATACTRatesEvent(object sender, EventArgs e)
		{
			RateDataImporter.Rounding = RatingRoundingTypes.Chargeable;
			RateDataImporter.IsJobLevelCharge = true;

			ImportRates(true, false, RateDataImporter.ImportIATA_TACT);
		}

		void ImportRates(bool showClearRatesOptions, bool modal, Action<string, Stream> importAction)
		{
			if (!CurrentHeader.IsInDatabase || CurrentHeader.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("DA03CF18-5097-4306-810B-5EB3A08D6D06", "Please save changes before importing."));
				return;
			}

			RateDataImporter.ShowClearRatesOptions = showClearRatesOptions;
			var importForm = new DataImportForm(RateDataImporter, importAction);

			if (modal)
			{
				ZFormModaliser.ShowDialogAndDispose(importForm);
			}
			else
			{
				importForm.ParentRatingFormToClose = this;
				importForm.Show();
#if DEBUG
				LastImportFormShown = importForm;
#endif
			}
		}

		#endregion

		#region Publish All

		void PublishAll(object sender, EventArgs e)
		{
			if (!CurrentHeader.IsInDatabase || CurrentHeader.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("51652075-600c-43ae-afe0-697a5271c967", "Please save changes before publishing rates."));
				return;
			}

			if (CurrentHeader.IsCosting() && !Env.Security.GlobalCostingRatesPublishAndUnpublish.IsAllowed)
			{
				Env.Security.GlobalCostingRatesPublishAndUnpublish.ShowError();
				return;
			}

			if (CurrentHeader.IsClientRate() && (CurrentHeader.Header == null || !Env.Security.GlobalClientRatesPublishAndUnpublish.IsAllowed))
			{
				Env.Security.GlobalClientRatesPublishAndUnpublish.ShowError();
				return;
			}

			if (CurrentHeader.IsLevelOneTariff() && !Env.Security.GlobalTariffRatesPublishAndUnpublish.IsAllowed)
			{
				Env.Security.GlobalTariffRatesPublishAndUnpublish.ShowError();
				return;
			}

			var allRateEntriesToAvoidModificationDuringEnumeration = CurrentHeader.AllEntries.ToList();
			using (CurrentHeader.AllEntriesCollection.SuspendListChanged())
			{
				foreach (var rateEntry in allRateEntriesToAvoidModificationDuringEnumeration)
				{
					rateEntry.IsPublished = true;
				}
			}

			CurrentHeader.Validation.ValidateAll();
		}

		#endregion

		#region Invalidate Rate Lines

		void CurrentHeader_InvalidateRateLines(object sender, EventArgs e)
		{
			if (BaseTabControl.CurrentEntry != null)
			{
				((IBusinessObjectCollectionInternals)BaseTabControl.CurrentEntry.RateLines).FireListResetEvent();
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				CurrentHeader.InvalidateRateLines -= CurrentHeader_InvalidateRateLines;
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

#if DEBUG
		internal DataImportForm LastImportFormShown;
#endif
	}
}
