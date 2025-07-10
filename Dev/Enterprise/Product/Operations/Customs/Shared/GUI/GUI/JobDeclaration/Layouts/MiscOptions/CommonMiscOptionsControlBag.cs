using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonMiscOptionsControlBag : ControlBag
	{
		public static CommonMiscOptionsControlBag Instance => instance ?? (instance = new CommonMiscOptionsControlBag());

		[ThreadStatic]
		static CommonMiscOptionsControlBag instance;

		protected override Control CreateTemplate() => new CommonMiscOptionsLayoutUserControl();

		CommonMiscOptionsControlBag()
		{
			BranchGuidFindBox = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.BranchGuidFindBox));
			BrokerCodeFindBox = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.BrokerCodeFindBox));
			PaymentPartyDropEdit = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.PaymentPartyDropEdit));
			PaidByDropEdit = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.PaidByDropEdit));
			MergeByDropEdit = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.MergeByDropEdit));
			RelatedDeclarationsUserControl = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.RelatedDeclarationsUserControl));
			EntryAuthorisationDateEdit = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.EntryAuthorisationDateEdit));
			RepresentationDropEdit = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.RepresentationDropEdit));
			MiscellaneousOptionsSeparatorUserControl = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.MiscellaneousOptionsSeparatorUserControl));
			DefermentAccountNumberTextBox = RegisterControl(nameof(CommonMiscOptionsLayoutUserControl.DefermentAccountNumberTextBox));
		}

		public ControlReference BranchGuidFindBox { get; }
		public ControlReference BrokerCodeFindBox { get; }
		public ControlReference PaymentPartyDropEdit { get; }
		public ControlReference MergeByDropEdit { get; }
		public ControlReference RelatedDeclarationsUserControl { get; }
		public ControlReference EntryAuthorisationDateEdit { get; }
		public ControlReference RepresentationDropEdit { get; }
		public ControlReference PaidByDropEdit { get; }
		public ControlReference MiscellaneousOptionsSeparatorUserControl { get; }
		public ControlReference DefermentAccountNumberTextBox { get; }
	}
}
