using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
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
	public class eManifestShipmentFilterStripControl : ZFilterStripControl
	{
		#region Captions

		internal static class Captions
		{
			internal static ResourceStringData ShipmentType
			{
				get { return Res.GetData("9E19D997-28FA-44A9-891F-F8D9F345CBE3", "Type", "Shipment Type", "Shipment Release Type", ""); }
			}

			internal static ResourceStringData ShipmentControlNumber
			{
				get { return Res.GetData("728B3D82-3022-4E25-8DF6-C41D31284CA5", "SCN", "Shipment Control Number", ""); }
			}

			internal static ResourceStringData PortOfLading
			{
				get { return Res.GetData("CFBDF283-B23C-43F1-8584-811DC6EFCC46", "Loading", "Port of Loading", ""); }
			}

			internal static ResourceStringData PortOfLadingKCode
			{
				get { return Res.GetData("D24C0A5E-BBF0-4BFB-AF26-E052D510F567", "Schedule K", "Port Of Lading (Schedule K)", ""); }
			}

			internal static ResourceStringData Shipper
			{
				get { return Res.GetData("68ACD821-E535-4C9D-8587-4FC0049BABD8", "Shipper"); }
			}

			internal static ResourceStringData Consignee
			{
				get { return Res.GetData("891EE55B-AC87-4554-A875-4AD71FB5D18D", "Consignee"); }
			}

			internal static ResourceStringData Quantity
			{
				get { return Res.GetData("9560EA15-4B31-411B-84B3-B69A5E3C68BF", "Quantity"); }
			}

			internal static ResourceStringData QuantityUQ
			{
				get { return Res.GetData("61692E15-1EED-42F8-A047-A9E6AE0D55D4", "UQ", "Quantity UQ", ""); }
			}

			internal static ResourceStringData Weight
			{
				get { return Res.GetData("1A000CE5-5215-4EEE-A0CE-3072DAD148A9", "Weight", "Gross Weight", ""); }
			}

			internal static ResourceStringData WeightUQ
			{
				get { return Res.GetData("9C958DB2-A4BA-4A0E-B25E-1F76C3ACC4F1", "UQ", "Weight UQ", ""); }
			}
		}

		#endregion

		public eManifestShipmentFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			AddColumnStyle(eManifestFilterStripControl.Captions.TripReference, "Trip+" + Trip.Schema.BH_JobReference, 100);
			AddColumnStyle(Captions.ShipmentType, Shipment.Schema.B0_ShipmentType, 50);
			AddColumnStyle(Captions.ShipmentControlNumber, Shipment.Schema.B0_MasterBillNumber, 140);
			AddColumnStyle(Captions.PortOfLading, Shipment.Schema.B0_RL_NKPortOfLading, 50);
			AddColumnStyle(Captions.PortOfLadingKCode, Shipment.Schema.B0_PortOfLadingKCode, 70);
			AddColumnStyle(Captions.Shipper, "Shipper+Organisation+OH_Code", 100);
			AddColumnStyle(Captions.Consignee, "Consignee+Organisation+OH_Code", 100);
			AddColumnStyle(Captions.Quantity, Shipment.Schema.B0_ManifestQty, 60);
			AddColumnStyle(Captions.QuantityUQ, Shipment.Schema.B0_ManifestUQ, 35);
			AddColumnStyle(Captions.Weight, Shipment.Schema.B0_Weight, 60);
			AddColumnStyle(Captions.WeightUQ, Shipment.Schema.B0_WeightUQ, 35);
			AddColumnStyle(eManifestFilterStripControl.Captions.ReleaseStatus, Shipment.Schema.B0_ReleaseStatusCodeDescription, 160);
		}

		void AddColumnStyle(ResourceStringData caption, string columnName, int width)
		{
			var newStyleInfo = new ZTextBoxColumnStyleInfo { CaptionResourceString = caption, ColumnName = columnName };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, width, true);
			FilteredGrid.ColumnStyles.Add(newStyleInfo);
		}
	}
}
