using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;

namespace Enterprise.TransportConsignment.GUI
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.TransportConsignment.GUI.XmlSerializers")]
	public class DtbRoutePlannerScreenLayout
	{
		/// <summary>
		/// Necessary for XmlSerializer.
		/// </summary>
		public DtbRoutePlannerScreenLayout()
		{
		}

		// view modes
		public DtbRoutePlannerViewMode PlannerViewMode { get; set; }
		public RunSheetView RunSheetView { get; set; }
		public RunSheetDay RunSheetDay { get; set; }
		public ZDate? CustomDate { get; set; }

		// button states
		public bool IsConsignmentDetailsButtonChecked { get; set; }
		public bool IsRunSheetDetailsButtonChecked { get; set; }
		public bool IsFiltersButtonChecked { get; set; }
		public bool IsRunSheetsArrowButtonChecked { get; set; }

		// splitter positions
		public int ConsignmentHorizontalSplitterPosition { get; set; }
		public int ConsignmentVerticalSplitterPosition { get; set; }
		public int RunSheetVerticalSplitterPosition { get; set; }
		public int RunSheetHorizontalSplitterPosition { get; set; }
	}
}
