using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	public partial class VesselRoutingVoyagesImportPreviewForm : ZChildForm
	{
		public VesselRoutingVoyagesImportPreviewForm(VesselRoutingVoyagesFilter businessEntity) : base(businessEntity)
		{
			FindButton.Click += delegate
			{ PerformSearch(); };
			ClearButton.Click += delegate
			{ PerformClear(); };

			PortPairsGrid.MouseDown += new MouseEventHandler(OnPortPairsGrid_MouseDown);
			PortPairsGrid.MouseUp += new MouseEventHandler(OnPortPairsGrid_MouseUp);
			VoyagesGrid.RemoveAction = RemoveAction.NoRemovePossible;
			PortPairsGrid.RemoveAction = RemoveAction.NoRemovePossible;

			SetupNoPortPairsAvailableLabel();
			SetupVendorDataStatusLabel();

			SearchResultsLabel.Font = OFont.GetFontBold();
			SearchResultsLabel.Text = "";
		}

		public new VesselRoutingVoyagesFilter BusinessEntity
		{
			get { return base.BusinessEntity as VesselRoutingVoyagesFilter; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Find / Clear

		protected void PerformSearch()
		{
			ValidateAll(ValidationType.Light);
			if (BusinessEntityForValidation.HasErrors())
			{
				Globals.Message.ShowWarning(Res.GetString("25a84418-8bbf-4765-843b-fd840c38dd23", "There are errors. Please correct these before searching."), "Errors...");
			}
			else
			{
				using (new ZWaitCursorChanger(this))
				{
					PerformSearchAndUpdateAppearance();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PerformSearchAndUpdateAppearance()
		{
			UpdateSearchResultsText(Res.GetString("Freight|VesselRoutingVoyageImportForm|Searching", "Searching..."), false);
			Application.DoEvents();

			BusinessEntity.PerformSearch();
			FocusVoyagesGridIfVoyagesFound();

			if (BusinessEntity.Voyages.Count == 0)
			{
				UpdateSearchResultsText(Res.GetString("Freight|VesselRoutingVoyageImportForm|ThereAreNoRecordsThatMatchYourSearchCriteria", "There are no records that match your search criteria."), false);
			}
			else
			{
				UpdateSearchResultsText("", false);
			}
		}

		void FocusVoyagesGridIfVoyagesFound()
		{
			if (VoyagesGrid.List.Count > 0)
			{
				ActiveControl = VoyagesGrid;
				VoyagesGrid.Select(0);
			}
		}

		void PerformClear()
		{
			BusinessEntity.ResetToDefaultValues();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool processFind = (keyData == (Keys.Control | Keys.Enter));
			bool processClear = (keyData == (Keys.Alt | Keys.Enter));

			bool result;
			if (processFind)
			{
				PerformSearch();
				result = true;
			}
			else if (processClear)
			{
				PerformClear();
				result = true;
			}
			else
			{
				result = base.ProcessDialogKey(keyData);
			}
			return result;
		}

		#endregion

		#region Focusing the Voyages grid after selecting port pairs

		// This is too difficult to unit test. To functionally test:
		// - Click a voyage row.
		// - Click the 'selected' check box on a port pair.
		// - Press the down arrow to verify the voyages grid still has focus

		void OnPortPairsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			VoyagesGridCurrentRow = VoyagesGrid.CurrentCell.RowNumber;
		}

		void OnPortPairsGrid_MouseUp(object sender, MouseEventArgs e)
		{
			DataGrid.HitTestInfo hit = PortPairsGrid.HitTest(PortPairsGrid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(MousePosition.X, MousePosition.Y, false)));
			if (PortPairsGrid.CurrentCell.ColumnNumber != -1 &&
				PortPairsGrid.CurrentCell.RowNumber != -1 &&
				PortPairsGrid.TableStyles[0].GridColumnStyles[PortPairsGrid.CurrentCell.ColumnNumber].MappingName == VesselRoutingPortPair.Schema.E9_IsSelected)
			{
				VoyagesGrid.Focus();
				BeginInvoke(new MethodInvoker(delegate
				{ VoyagesGrid.CurrentCell = new DataGridCell(VoyagesGridCurrentRow, 0); }));
			}
		}

		int VoyagesGridCurrentRow;

		#endregion

		#region UpdateSearchResultsText

		void UpdateSearchResultsText(string text, bool showWarningIcon)
		{
			SearchResultsLabel.Text = text;
			SearchResultsLabelIconProvider.SetError(SearchResultsLabel, showWarningIcon ? text : "");
		}

		protected ErrorProvider SearchResultsLabelIconProvider
		{
			get
			{
				if (fSearchResultsLabelIconProvider == null)
				{
					fSearchResultsLabelIconProvider = new ErrorProvider();
					fSearchResultsLabelIconProvider.SetIconAlignment(SearchResultsLabel, ErrorIconAlignment.MiddleLeft);
					fSearchResultsLabelIconProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
					fSearchResultsLabelIconProvider.Icon = Icons.GetIcon(IconTypes.Warning);
				}
				return fSearchResultsLabelIconProvider;
			}
		}
		ErrorProvider fSearchResultsLabelIconProvider;

		#endregion

		#region Select All / None Button

		void UnselectAllVoyages()
		{
			foreach (VesselRoutingVoyage voyage in VoyagesGrid.List)
			{
				voyage.DeselectAllPortPairs();
			}
		}

		void OnVoyagesSelectNone_Click(object sender, EventArgs e)
		{
			UnselectAllVoyages();
		}

		void OnPortPairsSelectAll_Click(object sender, EventArgs e)
		{
			foreach (VesselRoutingPortPair portPair in PortPairsGrid.List)
			{
				portPair.E9_IsSelected = true;
			}
		}

		void OnPortPairsSelectNone_Click(object sender, EventArgs e)
		{
			foreach (VesselRoutingPortPair portPair in PortPairsGrid.List)
			{
				portPair.E9_IsSelected = false;
			}
		}

		#endregion

		#region Adding Foreign Ports

		void OnAddForeignPort_Click(object sender, EventArgs e)
		{
			if (CurrentVoyage != null)
			{
				if (CurrentVoyage.ForeignPorts.Contains(CurrentVoyage.E8_ForeignPortToAdd))
				{
					Globals.Message.ShowInformation(Res.GetString("291e9d70-eeac-4416-9881-03c0abf83f38", "This {0} port is already selected", ForeignPortLoadOrDischarge));
				}
				else if (CurrentVoyage.E8_ForeignPortToAdd.IsEmpty || CurrentVoyage.E8_ForeignPortToAdd.StartsWith(Env.CurrentCompany.Country.Code))
				{
					Globals.Message.ShowInformation(Res.GetString("2c478361-7e66-4db2-9d99-9dc0c4919261", "Enter a foreign {0} port", ForeignPortLoadOrDischarge));
				}
				else if (CurrentVoyage.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CurrentVoyage.E8_ForeignPortToAdd) == null)
				{
					Globals.Message.ShowInformation(Res.GetString("af2aff77-f79f-417e-88bf-9ff4680d271c", "Enter a valid foreign {0} port", ForeignPortLoadOrDischarge));
				}
				else
				{
					if (!CurrentVoyage.ForeignPorts.Contains(CurrentVoyage.E8_ForeignPortToAdd))
					{
						CurrentVoyage.ForeignPorts.Add(CurrentVoyage.E8_ForeignPortToAdd);
						CurrentVoyage.PortPairs.Load();
					}
					CurrentVoyage.PortPairs.SelectPortPairsMatchingPort(CurrentVoyage.E8_ForeignPortToAdd);
					CurrentVoyage.E8_ForeignPortToAdd = "";
				}
			}
		}

		void OnRemoveForeignPort_Click(object sender, EventArgs e)
		{
			if (PortPairsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("4e4b0c1a-3b5f-422b-b787-0dbd876ad895", "You must select a port pair whose {0} port you want to remove", ForeignPortLoadOrDischarge));
			}
			else
			{
				VesselRoutingVoyage voyage = (VesselRoutingVoyage)VoyagesGrid.ListManager.GetCurrent();
				foreach (VesselRoutingPortPair portPair in PortPairsGrid.GetSelectedElements<VesselRoutingPortPair>())
				{
					voyage.ForeignPorts.Remove(portPair.E9_RL_NKLoadPort);
					voyage.ForeignPorts.Remove(portPair.E9_RL_NKDischargePort);
				}
				voyage.PortPairs.Load();
			}
		}

		void OnResetPortPairs_Click(object sender, EventArgs e)
		{
			VesselRoutingVoyage currentVoyage = this.CurrentVoyage;
			if (currentVoyage != null)
			{
				currentVoyage.LoadOriginalForeignPortList();
				currentVoyage.PortPairs.Load();
			}
		}

		#endregion

		#region Import Button

		void OnImport_Click(object sender, EventArgs e)
		{
			VesselRoutingVoyage[] voyagesToImport = BusinessEntity.Voyages.GetSelectedVoyages();

			BusinessEntity.Voyages.RunPreSaveValidation();
			if (BusinessEntity.Voyages.HasErrors())
			{
				Globals.Message.ShowWarning(Res.GetString("b58cb99e-8c1c-435b-8cde-86344bae7757", "There are errors. Please correct these before importing."), "Errors...");
			}
			else if (voyagesToImport.Length == 0)
			{
				Globals.Message.ShowWarning(Res.GetString("f5514f62-713f-4986-894e-8aec30bfb3d5", "Select the port pairs you wish to import."), Res.GetString("21e2a4b6-3a9b-4807-a7c2-d58167add5d5", "No port pairs selected"));
			}
			else
			{
				VesselRoutingVoyageImportDirector director = new VesselRoutingVoyageImportDirector(this, voyagesToImport);
				if (!director.Import())
				{
					UnselectAllVoyages();
				}
			}
		}

		#endregion

		#region Form Event Handlers

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (BusinessEntity.Voyages.GetSelectedVoyages().Length > 0)
			{
				string question = Res.GetString("0f01970b-e310-4a19-94c4-d4a8e85def31", "Any port pair selections made will be lost.\r\nAre you sure you want to close the form?");
				if (Globals.Message.Show(question, Res.GetString("c365b1c7-7489-49e5-ad78-231a4348681a", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}

		void OnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Implementation

		void SetupNoPortPairsAvailableLabel()
		{
			NoPortPairsAvailableLabel.DataBindings.Add(new KBinding("IsVisibleForBinding", BusinessEntity, "Voyages.E8_HasNoPortPairs"));
			NoPortPairsAvailableLabel.BackColor = PortPairsGrid.BackgroundColor;
			NoPortPairsAvailableLabel.ForeColor = Color.White;
		}

		void SetupVendorDataStatusLabel()
		{
			VendorDataStatusLabel.Text = Enterprise.Freight.Business.SailingScheduleDataVendor.Instance.Status;
			if (!Enterprise.Freight.Business.SailingScheduleDataVendor.Instance.IsVendorDataCurrent)
			{
				VendorDataStatusLabel.Font = OFont.GetFontBold();
				VendorDataStatusLabel.ForeColor = Color.Red;
			}
		}

		string ForeignPortLoadOrDischarge
		{
			get
			{
				return
					(BusinessEntity.IsPortPairTypeInFilter(PortPairTypes.Export)
						? Res.GetString("Freight|VesselRoutingVoyageImportPreivew|Discharge", "Discharge")
						: Res.GetString("Freight|VesselRoutingVoyageImportPreivewLoad", "Load")).ToLower();
			}
		}

		VesselRoutingVoyage CurrentVoyage
		{
			get { return (VesselRoutingVoyage)VoyagesGrid.ListManager.GetCurrent(); }
		}

		#endregion
	}
}
