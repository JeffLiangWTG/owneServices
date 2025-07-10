using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public sealed class TWBillPartiesControlBag : ControlBag
	{
		public static TWBillPartiesControlBag Instance => instance ?? (instance = new TWBillPartiesControlBag());

		[ThreadStatic]
		static TWBillPartiesControlBag instance;

		TWBillPartiesControlBag()
		{
			ShipperAddressUserControl = RegisterControl("ShipperAddressUserControl");
			ConsigneeAddressUserControl = RegisterControl("ConsigneeAddressUserControl");
		}

		protected override Control CreateTemplate()
		{
			return new TWBillPartiesUserControl();
		}

		public ControlReference ShipperAddressUserControl { get; }

		public ControlReference ConsigneeAddressUserControl { get; }
	}
}
