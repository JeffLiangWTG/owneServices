using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class ConsolidatedDeclarationControlBag : ControlBag
	{
		public static ConsolidatedDeclarationControlBag Instance => instance ?? (instance = new ConsolidatedDeclarationControlBag());

		[ThreadStatic]
		static ConsolidatedDeclarationControlBag instance;

		ConsolidatedDeclarationControlBag()
		{
			JobNumberTextBox = RegisterControl(nameof(JobNumberTextBox));
			EntryNumberTextBox = RegisterControl(nameof(EntryNumberTextBox));
			EntryStatusTextBox = RegisterControl(nameof(EntryStatusTextBox));
			MessageTypeDropEdit = RegisterControl(nameof(MessageTypeDropEdit));
			MessageSubTypeDropEdit = RegisterControl(nameof(MessageSubTypeDropEdit));
			TransportModeDropEdit = RegisterControl(nameof(TransportModeDropEdit));
			ImporterGuidFindBox = RegisterControl(nameof(ImporterGuidFindBox));
			VesselCodeFindBox = RegisterControl(nameof(VesselCodeFindBox));
			VoyageFlightNoTextBox = RegisterControl(nameof(VoyageFlightNoTextBox));
			DischargeETADateEdit = RegisterControl(nameof(DischargeETADateEdit));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(PortOfLoadingCodeFindBox));
			PortOfDischargeCodeFindBox = RegisterControl(nameof(PortOfDischargeCodeFindBox));
			EntryPeriodDateEdit = RegisterControl(nameof(EntryPeriodDateEdit));
		}

		public ControlReference JobNumberTextBox { get; }
		public ControlReference EntryNumberTextBox { get; }
		public ControlReference EntryStatusTextBox { get; }
		public ControlReference MessageTypeDropEdit { get; }
		public ControlReference MessageSubTypeDropEdit { get; }
		public ControlReference TransportModeDropEdit { get; }
		public ControlReference ImporterGuidFindBox { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference VoyageFlightNoTextBox { get; }
		public ControlReference DischargeETADateEdit { get; }
		public ControlReference PortOfLoadingCodeFindBox { get; }
		public ControlReference PortOfDischargeCodeFindBox { get; }
		public ControlReference EntryPeriodDateEdit { get; }

		protected override Control CreateTemplate()
		{
			return new ConsolidatedDeclarationControlBagTemplate();
		}
	}
}
