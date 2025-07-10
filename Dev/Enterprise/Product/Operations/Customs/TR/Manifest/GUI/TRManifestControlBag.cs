using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public class TRManifestControlBag : ControlBag
	{
		public static TRManifestControlBag Instance => manifestControlBag.Value;

		TRManifestControlBag()
		{
			TransportTypeDropEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.TransportTypeDropEdit));
			ManifestDescriptionTextBox = RegisterControl(nameof(TRManifestCountrySpecificUserControl.ManifestDescriptionTextBox));
			TIRNumberTextBox = RegisterControl(nameof(TRManifestCountrySpecificUserControl.TIRNumberTextBox));
			InspectionClerkTextBox = RegisterControl(nameof(TRManifestCountrySpecificUserControl.InspectionClerkTextBox));
			InternalInspectionNoTextBox = RegisterControl(nameof(TRManifestCountrySpecificUserControl.InternalInspectionNoTextBox));
			TempStorageStartDateEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.TempStorageStartDateEdit));
			TempStorageDueDateEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.TempStorageDueDateEdit));
			PresentationCustomsOfficeDropEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.PresentationCustomsOfficeDropEdit));
			GlobalManifestStampDutyValueCalcEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.GlobalManifestStampDutyValueCalcEdit));
			MasterBillStampDutyValueCalcEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.MasterBillStampDutyValueCalcEdit));
			TotalStampDutyValueCalcEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.TotalStampDutyValueCalcEdit));
			RegistrationDateEdit = RegisterControl(nameof(TRManifestCountrySpecificUserControl.RegistrationDateEdit));
		}

		public ControlReference TransportTypeDropEdit { get; }
		public ControlReference ManifestDescriptionTextBox { get; }
		public ControlReference TIRNumberTextBox { get; }
		public ControlReference InspectionClerkTextBox { get; }
		public ControlReference InternalInspectionNoTextBox { get; }
		public ControlReference TempStorageStartDateEdit { get; }
		public ControlReference TempStorageDueDateEdit { get; }
		public ControlReference PresentationCustomsOfficeDropEdit { get; }
		public ControlReference GlobalManifestStampDutyValueCalcEdit { get; }
		public ControlReference MasterBillStampDutyValueCalcEdit { get; }
		public ControlReference TotalStampDutyValueCalcEdit { get; }
		public ControlReference RegistrationDateEdit { get; }

		protected override Control CreateTemplate() => new TRManifestCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TRManifestControlBag> manifestControlBag = new Lazy<TRManifestControlBag>(() => new TRManifestControlBag());
	}
}
