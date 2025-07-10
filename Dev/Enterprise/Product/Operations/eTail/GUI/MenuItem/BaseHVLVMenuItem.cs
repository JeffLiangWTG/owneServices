using System;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public abstract class BaseHVLVMenuItem : ZMenuItem
	{
		public BaseHVLVMenuItem(string text, ForwardingShipment shipment)
			: base(text)
		{
			this.shipment = shipment;
		}

		public BaseHVLVMenuItem(MultilingualString caption, ForwardingShipment shipment)
			: base(caption)
		{
			this.shipment = shipment;
		}

		protected readonly ForwardingShipment shipment;

		protected HVLVConsignmentHeader Header => shipment.GetOrCreateHVLVConsignmentHeader();

		protected sealed override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			MenuAction.Invoke();
		}

		protected abstract Action MenuAction { get; }

		public virtual void UpdateVisibilityAndCaption()
		{
		}

		protected virtual bool PreValidateData()
		{
			return true;
		}

		protected ZForm Form => (ZForm)ParentControl.FindForm();
	}
}
