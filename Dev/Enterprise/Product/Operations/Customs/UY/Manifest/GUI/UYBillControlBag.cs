using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.UY.Manifest.GUI
{
	public sealed class UYBillControlBag : ControlBag
	{
		public static UYBillControlBag Instance => billControlBag.Value;

		UYBillControlBag()
		{
			TransshipmentCheckBox = RegisterControl(nameof(UYBillCountrySpecificUserControl.TransshipmentCheckBox));
		}
		public ControlReference TransshipmentCheckBox { get; }

		protected override Control CreateTemplate() => new UYBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UYBillControlBag> billControlBag = new Lazy<UYBillControlBag>(() => new UYBillControlBag());
	}
}
