using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestFilterStripControl : ZFilterStripControl
	{
		#region Captions

		internal static class Captions
		{
			internal static ResourceStringData Branch
			{
				get { return Res.GetData("9BEAB1B6-EF47-4FB9-969A-7906DD5DBCFC", "Branch"); }
			}

			internal static ResourceStringData JobReference
			{
				get { return Res.GetData("12345678-5c0c-4a7e-8d4c-a6b1b8f58a6f", "Job Ref.", "Job Reference"); }
			}

			internal static ResourceStringData ClientCode
			{
				get { return Res.GetData("12345678-1111-4a7e-8d4c-a6b1b8f58a6f", "Client Code"); }
			}

			internal static ResourceStringData ClientName
			{
				get { return Res.GetData("12345678-2222-4a7e-8d4c-a6b1b8f58a6f", "Client Name"); }
			}

			internal static ResourceStringData ClientGroup
			{
				get { return Res.GetData("12345678-3333-4a7e-8d4c-a6b1b8f58a6f", "Client"); }
			}

			internal static ResourceStringData TripReference
			{
				get { return Res.GetData("345276e3-5c0c-4a7e-8d4c-a6b1b8f58a6f", "Trip Ref.", "Trip Reference"); }
			}

			internal static ResourceStringData MethodOfTransportation
			{
				get { return Res.GetData("15375b4e-2b91-4d22-8bcf-535c9a4cd69a", "MOT", "Method Of Transportation", ""); }
			}

			internal static ResourceStringData CarrierCode
			{
				get { return Res.GetData("e50cc50e-1341-4c8b-b21e-2ed0fb2dab02", "SCAC", "Carrier", "Carrier Code (SCAC)", ""); }
			}

			internal static ResourceStringData EstimatedDateOfArrival
			{
				get { return Res.GetData("3f1cb9a7-db57-4ae9-9ad1-779b354cfe57", "ETA", "Date Of Arrival", "Estimated Date Of Arrival", ""); }
			}

			internal static ResourceStringData FirstExpectedPortOfArrival
			{
				get { return Res.GetData("b0774a46-17c8-41c9-8b4c-3e527fbe6d1e", "Port Of Arrival", "First Expected Port Of Arrival", ""); }
			}

			internal static ResourceStringData FirstExpectedPortOfArrivalScheduleD
			{
				get { return Res.GetData("DE6EE983-46C9-4A5D-B0A8-B23B19648D9F", "Schedule D", "Port Of Arrival (Schedule D)", "First Expected Port Of Arrival (Schedule D)", ""); }
			}

			internal static ResourceStringData TransitDirection
			{
				get { return Res.GetData("718cdf78-9670-45d0-82e9-4396296be5bf", "Direction", "Transit Direction", ""); }
			}

			internal static ResourceStringData MessageStatus
			{
				get { return Res.GetData("7F9ACBF6-0BCF-477F-B48F-CB0EB07FF5C7", "Msg. Status", "Message Status"); }
			}

			internal static ResourceStringData ReleaseStatus
			{
				get { return Res.GetData("F087513F-EC85-404D-8107-986B653106D8", "Rel. Status", "Release Status"); }
			}

			internal static ResourceStringData Conveyance
			{
				get { return Res.GetData("BA251711-0FBC-4718-8E32-94EEDB28867D", "Conveyance"); }
			}
		}

		#endregion

		public eManifestFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			const string BH_ConveyanceID = "Conveyance+BJ_RegistrationNumber";
			const string BH_GBCode = "Branch+GB_Code";
			AddTextColumnStyle(Captions.Branch, BH_GBCode, 60);
			AddTextColumnStyle(Captions.JobReference, AutoCusInBondHeader.Schema.BH_JobReference, 100);
			AddTextColumnStyle(Captions.TripReference, AutoCusInBondHeader.Schema.BH_VoyageNumber, 100);
			AddTextColumnStyle(Captions.MethodOfTransportation, AutoCusInBondHeader.Schema.BH_ImportTransportMode, 35);
			AddTextColumnStyle(Captions.CarrierCode, AutoCusInBondHeader.Schema.BH_CarrierSCAC, 50);
			AddDateColumnStyle(Captions.EstimatedDateOfArrival, AutoCusInBondHeader.Schema.BH_ETA, 140);
			AddTextColumnStyle(Captions.FirstExpectedPortOfArrival, AutoCusInBondHeader.Schema.BH_RL_NKPortUnlading, 90);
			AddTextColumnStyle(Captions.FirstExpectedPortOfArrivalScheduleD, AutoCusInBondHeader.Schema.BH_PortUnladingDCode, 90);
			AddTextColumnStyle(Captions.TransitDirection, Trip.Schema.BH_TransitDirection, 70);
			AddTextColumnStyle(Captions.Conveyance, BH_ConveyanceID, 80);
			AddTextColumnStyle(Captions.MessageStatus, Trip.Schema.BH_MessageStatusCodeDescription, 160);
			AddTextColumnStyle(Captions.ReleaseStatus, Trip.Schema.BH_ReleaseStatusCodeDescription, 160);
			AddTextColumnStyle(Captions.ClientCode, Trip.Schema.ImporterCode, 160);
			AddTextColumnStyle(Captions.ClientName, Trip.Schema.ImporterName, 160);
		}

		void AddTextColumnStyle(ResourceStringData caption, string columnName, int width)
		{
			var newStyleInfo = new ZTextBoxColumnStyleInfo { CaptionResourceString = caption, ColumnName = columnName };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, width, true);
			FilteredGrid.ColumnStyles.Add(newStyleInfo);
		}

		void AddDateColumnStyle(ResourceStringData caption, string columnName, int width)
		{
			var newStyleInfo = new ZDateEditColumnStyleInfo { CaptionResourceString = caption, ColumnName = columnName };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, width, true);
			FilteredGrid.ColumnStyles.Add(newStyleInfo);
		}
	}
}
