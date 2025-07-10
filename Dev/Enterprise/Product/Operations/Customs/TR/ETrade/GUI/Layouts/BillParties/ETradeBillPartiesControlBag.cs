using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public class ETradeBillPartiesControlBag : ControlBag
	{
		public static ETradeBillPartiesControlBag Instance => instance ?? (instance = new ETradeBillPartiesControlBag());

		[ThreadStatic]
		static ETradeBillPartiesControlBag instance;

		public ETradeBillPartiesControlBag()
		{
			SupplementaryDeclarationNameTextBox = RegisterControl("SupplementaryDeclarationNameTextBox");
			SupplementaryDeclarationRegNoIdNoTextBox = RegisterControl("SupplementaryDeclarationRegNoIdNoTextBox");
			SupplementaryDeclarationDeliveryDateDateEdit = RegisterControl("SupplementaryDeclarationDeliveryDateDateEdit");
			DeliveryPartyDetailsSeparatorUserControl = RegisterControl("DeliveryPartyDetailsSeparatorUserControl");
		}

		public ControlReference SupplementaryDeclarationNameTextBox { get; }

		public ControlReference SupplementaryDeclarationRegNoIdNoTextBox { get; }

		public ControlReference SupplementaryDeclarationDeliveryDateDateEdit { get; }

		public ControlReference DeliveryPartyDetailsSeparatorUserControl { get; }

		protected override Control CreateTemplate() => new ETradeBillPartiesUserControl();
	}
}
