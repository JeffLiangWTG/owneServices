using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonHeaderDetailsControlBag : ControlBag
	{
		public static CommonHeaderDetailsControlBag Instance => instance ?? (instance = new CommonHeaderDetailsControlBag());

		[ThreadStatic]
		static CommonHeaderDetailsControlBag instance;

		CommonHeaderDetailsControlBag()
		{
			EntryTypeDropEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.EntryTypeDropEdit));
			EntryStatusTextBox = RegisterControl(nameof(CommonHeaderDetailsUserControl.EntryStatusTextBox));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(CommonHeaderDetailsUserControl.CustomsOfficeCodeFindBox));
			PeriodFromDateEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.PeriodFromDateEdit));
			PeriodToDateEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.PeriodToDateEdit));
			DeclarantAddressControl = RegisterControl(nameof(CommonHeaderDetailsUserControl.DeclarantAddressControl));
			RepresentativeAddressControl = RegisterControl(nameof(CommonHeaderDetailsUserControl.RepresentativeAddressControl));
			BuyingAgentAddressControl = RegisterControl(nameof(CommonHeaderDetailsUserControl.BuyingAgentAddressControl));
			AuthorizationNumberGuidDropEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.AuthorizationNumberGuidDropEdit));
			DeclarationTypeDropEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.DeclarationTypeDropEdit));
			DeclarantTypeDropEdit = RegisterControl(nameof(CommonHeaderDetailsUserControl.DeclarantTypeDropEdit));
			MessageStatusTextBox = RegisterControl(nameof(CommonHeaderDetailsUserControl.MessageStatusTextBox));
		}

		protected override Control CreateTemplate()
		{
			return new CommonHeaderDetailsUserControl();
		}

		public ControlReference EntryTypeDropEdit { get; }
		public ControlReference EntryStatusTextBox { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference PeriodFromDateEdit { get; }
		public ControlReference PeriodToDateEdit { get; }
		public ControlReference DeclarantAddressControl { get; }
		public ControlReference RepresentativeAddressControl { get; }
		public ControlReference BuyingAgentAddressControl { get; }
		public ControlReference AuthorizationNumberGuidDropEdit { get; }
		public ControlReference DeclarationTypeDropEdit { get; }
		public ControlReference DeclarantTypeDropEdit { get; }
		public ControlReference MessageStatusTextBox { get; }
	}
}
