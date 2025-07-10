using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ZAManifestControlBag : ControlBag
	{
		public static ZAManifestControlBag Instance => manifestControlBag.Value;

		ZAManifestControlBag()
		{
			PlaceOfEntryDropEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.PlaceOfEntryDropEdit));
			PlaceOfExitDropEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.PlaceOfExitDropEdit));
			DateAtCustomsOfficeDateEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.DateAtCustomsOfficeDateEdit));
			VesselCodeFindBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.VesselCodeFindBox));
			RadioCallSignCodeFindBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.RadioCallSignCodeFindBox));
			EstLoadDateEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.EstLoadDateEdit));
			MasterCarrierCodeTextBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.MasterCarrierCodeTextBox));
			SeparatorTextUserControl = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.SeparatorTextUserControl));
			VoyageFlightTextBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.VoyageFlightTextBox));
			TssVesselCodeFindBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.TssVesselCodeFindBox));
			TssRadioCallSignCodeFindBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.TssRadioCallSignCodeFindBox));
			TssCargoCarrierCodeCodeFindBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.TssCargoCarrierCodeCodeFindBox));
			DateOfDepartureDateEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.DateOfDepartureDateEdit));
			CaseNumberManifestHeaderGroupBox = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.CaseNumberManifestHeaderGroupBox));
			CallPurposeCodeDropEdit = RegisterControl(nameof(ZAManifestCountrySpecificUserControl.CallPurposeCodeDropEdit));
		}

		public ControlReference PlaceOfEntryDropEdit { get; }
		public ControlReference PlaceOfExitDropEdit { get; }
		public ControlReference DateAtCustomsOfficeDateEdit { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference RadioCallSignCodeFindBox { get; }
		public ControlReference EstLoadDateEdit { get; }
		public ControlReference MasterCarrierCodeTextBox { get; }
		public ControlReference SeparatorTextUserControl { get; }
		public ControlReference VoyageFlightTextBox { get; }
		public ControlReference TssVesselCodeFindBox { get; }
		public ControlReference TssRadioCallSignCodeFindBox { get; }
		public ControlReference TssCargoCarrierCodeCodeFindBox { get; }
		public ControlReference DateOfDepartureDateEdit { get; }
		public ControlReference CaseNumberManifestHeaderGroupBox { get; }
		public ControlReference CallPurposeCodeDropEdit { get; }

		protected override Control CreateTemplate() => new ZAManifestCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ZAManifestControlBag> manifestControlBag = new Lazy<ZAManifestControlBag>(() => new ZAManifestControlBag());
	}
}
