using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI
{
	public partial class VoyageDetailsControl : ZUserControl
	{
		public VoyageDetailsControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				IsArchivedCheckBox.Enabled = Env.Security.SailingScheduleManuallyArchive.IsAllowed;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(jv_RV_NKVesselBoundCodeFindBox, new SuppressFormsLocalizedTestAttribute());
#endif

			detailsExchangeSplitter.Panel1.AllowOutsideOfParent();
			detailsExchangeSplitter.Panel2.AllowOutsideOfParent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			SetDecimalsForExchangeRate();
			base.OnCurrentDataItemChanging(e);
		}

		void SetDecimalsForExchangeRate()
		{
			if (GlbCompany.CurrentCompany.GC_IsReciprocal)
			{
				foreach (ZGridColumnInfo info in exRatesGrid.ColumnStyles.ToArray())
				{
					if (info.ColumnName == JobVoyageExRateSchema.E8_VoyageExchangeRate.Name)
					{
						((ZCalcEditColumnStyleInfo)info).Decimals = 6;
					}
				}
			}
		}

		#region Binding / Layout

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			JobVoyage voyage = dataSource == null ? null : (JobVoyage)BindingContext[dataSource, dataMember].GetCurrent();
			if (voyage != null)
			{
				SetForTransportMode_PreBind(voyage.JV_AirSeaRoad);
			}
			base.SetDataBinding(dataSource, dataMember);
			if (voyage != null)
			{
				SetForTransportMode_PostBind(voyage.JV_AirSeaRoad);
				SetupVendorDataStatusLabel(voyage);
			}
		}

		void SetForTransportMode_PreBind(ZString transportMode)
		{
			if (transportMode == Core.Constants.TransportModes.Air)
			{
				SetForAir_PreBind();
			}
		}

		void SetForAir_PreBind()
		{
			for (int i = jobVoyDestinationBoundGrid.ColumnStyles.Count - 1; i >= 0; i--)
			{
				ZString columnName = ((ZGridColumnInfo)jobVoyDestinationBoundGrid.ColumnStyles[i]).ColumnName;
				if (columnName == VoyageDestination.Schema.JB_AvailabilityDate || columnName == VoyageDestination.Schema.JB_StorageDate)
				{
					jobVoyDestinationBoundGrid.ColumnStyles.RemoveAt(i);
				}
			}

			AddOnlineScheduleStatusColumn();
		}

		void AddOnlineScheduleStatusColumn()
		{
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)((System.Collections.IList)((JobVoyage)null).Sailings).SyncRoot).JX_OnlineScheduleStatus);
			var onlineScheduleStatusColumn = new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("VoyageDetailsControl|e2dc7229-4618-4012-a214-c099b5e1cfee", "Global Schedule Matching"),
				ColumnName = JobSailing.Schema.JX_OnlineScheduleStatus,
				IsMandatory = true,
				IsReadOnly = true,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};

			jobSailingBoundGrid.ColumnStyles.Add(onlineScheduleStatusColumn);
		}

		void SetForTransportMode_PostBind(ZString transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					SetForAir_PostBind();
					break;

				case Core.Constants.TransportModes.Sea:
					SetForSea_PostBind();
					break;

				case Core.Constants.TransportModes.Rail:
					SetForRail_PostBind();
					break;

				case Core.Constants.TransportModes.Road:
					SetForRoad_PostBind();
					break;
			}

			MenuItem loadList = new ZMenuItem(ResString.GetMultilingualString("Freight.Sailing.ViewLoadLists", "View Load Lists"));
			loadList.Click += new EventHandler(LoadList_Click);

			jobSailingBoundGrid.ContextMenu.MenuItems.Add(0, loadList);
			jobSailingBoundGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem("-"));
		}

		void SetForSea_PostBind()
		{
			jobSailingBoundGrid.CopyToText = Res.GetString("Freight|VoyageDetailsControl|CopyToSailingsWithTheSameLoad", "Copy To Sailings With the Same Load");
			rg_CodeBoundTextBox.Visible = true;

			isAllCargoCheckBox.Visible = false;
			jv_RV_NKVesselBoundCodeFindBox.Visible = true;
			jv_VesselJourneyNameTextBox.Visible = false;
			jv_VoyageTypeDropEdit.Visible = true;
			jv_VoyageFlightBoundTextEdit.Name = "JV_VoyageFlightBoundTextEdit";
			jv_AircraftTypeBoundTextEdit.Visible = false;

			jv_RV_NKVesselBoundCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("Freight|VoyageDetailsControl|Vessel", "Vessel");

			voyageInfoGroupBox.Text = Res.GetString("Freight|VoyageDetailsControl|VoyageDetails", "Voyage Details");
			sailingsGroupBox.Text = Res.GetString("Freight|VoyageDetailsControl|SailingSchedule", "Sailing Schedule");

			jobSailingBoundGrid.Name = "JobSailingBoundGrid";
			jobSailingBoundGrid.RunAfterBind(delegate
			{
				jobSailingBoundGrid.Columns[JobSailingSchema.Constants.JX_DepotReceivalCommences].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSRecvStart", "CFS Receival Start");
				jobSailingBoundGrid.Columns[JobSailingSchema.Constants.JX_DepotCutOff].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSCutOff", "CFS Cut Off");
			});

			SetMainDateSizes(70);
			ChangeDateTimeFormat(ZDateTimePickerFormat.Short);

			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressCode].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|DepCTOAddress", "Dep. CTO Address");
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressOrg].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|DepartureCTO", "Departure CTO");
				jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_ReceivalCommences].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CTORecStart", "CTO Receival Start");
				jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_CutOff].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CTOCutOff", "CTO Cut Off");
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressCode].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ArrCTOAddress", "Arr. CTO Address");
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressOrg].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ArrivalCTO", "Arrival CTO");
			});
		}

		void SetForAir_PostBind()
		{
			TradeLanesTabPage.TabVisible = false;
			jobSailingBoundGrid.CopyToText = Res.GetString("Freight|VoyageDetailsControl|CopyToFlightsWithTheSameLoad", "Copy To Flights With the Same Load");
			jv_RV_NKVesselBoundCodeFindBox.Visible = false;
			jv_VesselJourneyNameTextBox.Visible = false;
			jv_VoyageTypeDropEdit.Location = jv_VoyageFlightBoundTextEdit.Location;
			jv_VoyageFlightBoundTextEdit.Location = jv_RV_NKVesselBoundCodeFindBox.Location;
			jv_VoyageFlightBoundTextEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("Freight|VoyageDetailsControl|FlightNo", "Flight No.");
			jv_VoyageFlightBoundTextEdit.Name = "JV_FlightBoundTextEdit";
			voyageInfoGroupBox.Text = Res.GetString("Freight|VoyageDetailsControl|FlightDetails", "Flight Details");
			sailingsGroupBox.Text = Res.GetString("Freight|VoyageDetailsControl|FlightSchedule", "Flight Schedule");

			jv_AircraftTypeBoundTextEdit.Visible = true;
			jv_AircraftTypeBoundTextEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("Freight|VoyageDetailsControl|AircraftType", "Aircraft Type");

			jobSailingBoundGrid.Name = "JobFlightBoundGrid";
			jobSailingBoundGrid.RunAfterBind(delegate
			{
				jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotReceivalCommences].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSRecvStart", "CFS Receival Start");
				jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotCutOff].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSCutOff", "CFS Cut Off");
				jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotStorageDate].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSStor", "CFS Storage Start");
				jobSailingBoundGrid.Columns[JobSailing.Schema.JX_DepotAvailabilityDate].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|CFSAvail", "CFS Available");
				jobSailingBoundGrid.Columns[JobSailing.Schema.JX_ReservedMasterBill].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|RsrvdMaster", "Rsrvd. Master");
			});

			SetMainDateSizes(110);
			ChangeDateTimeFormat(ZDateTimePickerFormat.Long);

			var validateFlightMenuItem = new ZMenuItem(ResString.GetMultilingualString("b26b52fb-de1f-4761-b9a9-aa38cd33f0d0", "Validate Flight against Global Flight Schedule"));
			validateFlightMenuItem.Click += delegate
			{
				var currentJobSailing = (JobSailing)jobSailingBoundGrid.ListManager.GetCurrent();
				currentJobSailing?.TryMatchAgainstOnlineFlights();
			};
			jobSailingBoundGrid.ContextMenu.MenuItems.Add(validateFlightMenuItem);

			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressCode].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|DepDepotAddress", "Dep. Depot Address");
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressOrg].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|DepDepot", "Dep. Depot");
				jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_ReceivalCommences].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ULDCargoRecStart", "ULD Cargo Rec. Start");
				jobVoyOriginBoundGrid.Columns[JobVoyOriginSchema.Constants.JA_CutOff].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ULDCargoCutOff", "ULD Cargo Cut Off");
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressCode].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ArrDepotAddress", "Arr. Depot Address");
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressOrg].ColumnStyle.HeaderText = Res.GetString("Freight|VoyageDetailsControl|ArrDepot", "Arr. Depot");
			});
		}

		#region Resourse strings texts

		string CopyToSectorsWithTheSameLoadText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|CopyToSectorsWithTheSameLoad", "Copy To Sectors With the Same Load"); }
		}

		string JourneyDetailsText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|JourneyDetails", "Journey Details"); }
		}

		string SectorScheduleText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|SectorSchedule", "Sector Schedule"); }
		}

		string DepTermAddressText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|DepTermAddress", "Dep. Term. Address"); }
		}

		string DepTerminalText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|DepTerminal", "Dep. Terminal"); }
		}

		string ArrTermAddressText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|ArrTermAddress", "Arr. Term. Address"); }
		}

		string ArrTerminalText
		{
			get { return Res.GetString("Freight|VoyageDetailsControl|ArrTerminal", "Arr. Terminal"); }
		}

		#endregion

		void SetForRail_PostBind()
		{
			TradeLanesTabPage.TabVisible = false;
			jobSailingBoundGrid.CopyToText = CopyToSectorsWithTheSameLoadText;
			isAllCargoCheckBox.Visible = false;
			jv_RV_NKVesselBoundCodeFindBox.Visible = false;
			jv_VesselJourneyNameTextBox.Visible = true;
			jv_VesselJourneyNameTextBox.Location = jv_RV_NKVesselBoundCodeFindBox.Location;
			jv_VesselJourneyNameTextBox.CaptionResourceString = Res.GetData("Freight|VoyageDetailsControl|Journey", "Journey", "The name of the Rail Journey that this Voyage will be on.");
			jv_VoyageFlightBoundTextEdit.Name = "JV_JourneyNoBoundTextEdit";
			voyageInfoGroupBox.Text = JourneyDetailsText;
			sailingsGroupBox.Text = SectorScheduleText;
			jobSailingBoundGrid.Name = "JobRailBoundGrid";
			jv_VoyageFlightBoundTextEdit.CaptionResourceString = Res.GetData("Freight|VoyageDetailsControl|JV_VoyageFlight|Rail", "Journey No.", "The Rail Journey reference number.");
			jv_AircraftTypeBoundTextEdit.Visible = false;

			SetMainDateSizes(110);
			ChangeDateTimeFormat(ZDateTimePickerFormat.Long);

			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressCode].ColumnStyle.HeaderText = DepTermAddressText;
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressOrg].ColumnStyle.HeaderText = DepTerminalText;
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressCode].ColumnStyle.HeaderText = ArrTermAddressText;
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressOrg].ColumnStyle.HeaderText = ArrTerminalText;
			});
		}

		void SetForRoad_PostBind()
		{
			TradeLanesTabPage.TabVisible = false;
			jobSailingBoundGrid.CopyToText = CopyToSectorsWithTheSameLoadText;
			isAllCargoCheckBox.Visible = false;
			jv_RV_NKVesselBoundCodeFindBox.Visible = false;
			jv_VesselJourneyNameTextBox.Visible = false;
			jv_VoyageTypeDropEdit.Location = jv_VoyageFlightBoundTextEdit.Location;
			jv_VoyageFlightBoundTextEdit.Location = jv_RV_NKVesselBoundCodeFindBox.Location;
			jv_VoyageFlightBoundTextEdit.CaptionResourceString = Res.GetData("Freight|VoyageDetailsControl|JV_VoyageFlight|Road", "Truck Ref.", "The Truck Journey reference number.");
			jv_VoyageFlightBoundTextEdit.Name = "JV_TruckBoundTextEdit";
			voyageInfoGroupBox.Text = JourneyDetailsText;
			sailingsGroupBox.Text = SectorScheduleText;
			jobSailingBoundGrid.Name = "JobRoadBoundGrid";
			jv_AircraftTypeBoundTextEdit.Visible = false;

			SetMainDateSizes(110);
			ChangeDateTimeFormat(ZDateTimePickerFormat.Long);

			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressCode].ColumnStyle.HeaderText = DepTermAddressText;
				jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_Calc_DepartureCTOAddressOrg].ColumnStyle.HeaderText = DepTerminalText;
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressCode].ColumnStyle.HeaderText = ArrTermAddressText;
				jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_Calc_ArrivalCTOAddressOrg].ColumnStyle.HeaderText = ArrTerminalText;
			});
		}

		void ChangeDateTimeFormat(ZDateTimePickerFormat format)
		{
			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				((ZDateEditColumnStyle)jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_E_DEP].ColumnStyle).DateTimeFormat = format;
				((ZDateEditColumnStyle)jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_A_DEP].ColumnStyle).DateTimeFormat = format;
				((ZDateEditColumnStyle)jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_E_ARV].ColumnStyle).DateTimeFormat = format;
				((ZDateEditColumnStyle)jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_A_ARV].ColumnStyle).DateTimeFormat = format;
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				((ZDateEditColumnStyle)jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_E_ARV].ColumnStyle).DateTimeFormat = format;
				((ZDateEditColumnStyle)jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_A_ARV].ColumnStyle).DateTimeFormat = format;
			});

			jobSailingBoundGrid.Invalidate();
			jobVoyDestinationBoundGrid.Invalidate();
			jobVoyOriginBoundGrid.Invalidate();
		}

		void SetMainDateSizes(int size)
		{
			jobVoyOriginBoundGrid.RunAfterBind(delegate
			{
				ControlDpiScalingHelper.SetWidth(jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_E_DEP].ColumnStyle, size, true);
				ControlDpiScalingHelper.SetWidth(jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_A_DEP].ColumnStyle, size, true);
				ControlDpiScalingHelper.SetWidth(jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_E_ARV].ColumnStyle, size, true);
				ControlDpiScalingHelper.SetWidth(jobVoyOriginBoundGrid.Columns[VoyageOrigin.Schema.JA_A_ARV].ColumnStyle, size, true);
			});
			jobVoyDestinationBoundGrid.RunAfterBind(delegate
			{
				ControlDpiScalingHelper.SetWidth(jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_E_ARV].ColumnStyle, size, true);
				ControlDpiScalingHelper.SetWidth(jobVoyDestinationBoundGrid.Columns[VoyageDestination.Schema.JB_A_ARV].ColumnStyle, size, true);
			});
		}

		void LoadList_Click(object sender, EventArgs e)
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.LoadList);
			BusinessObject[] selectedSailings = jobSailingBoundGrid.SelectedElements;
			int maximumAllowedViews = 5;

			if (selectedSailings.Length > maximumAllowedViews)
			{
				Globals.Message.ShowWarning(Res.GetString("c310ea53-d03f-48ef-b0b2-2a592cb8fb44", "Please select no more than {0} Load Lists to view. Viewing more at the same time can put unnecessary strain on the system.", maximumAllowedViews));
			}
			else
			{
				foreach (BusinessObject element in selectedSailings)
				{
					controller.ShowEditForm(element);
				}
			}
		}

		void SetupVendorDataStatusLabel(JobVoyage voyage)
		{
			VendorDataStatusLabel.Text = "";
			if (Business.SailingScheduleDataVendor.Instance.IsEnabled && voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
			{
				VendorDataStatusLabel.Text = Business.SailingScheduleDataVendor.Instance.Status;
				if (!Business.SailingScheduleDataVendor.Instance.IsVendorDataCurrent)
				{
					VendorDataStatusLabel.Font = OFont.GetFontBold();
					VendorDataStatusLabel.ForeColor = Color.Red;
				}
			}
		}

		#endregion
	}
}
