using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class CartageLegControl : ZUserControl
	{
		public CartageLegControl()
		{
			InitializeComponent();

			SetDataSourceBinding("CartageLegHasWaitPoint", "HasWaitPoint");

			DistanceButton.Click += delegate
			{ if (CartageLeg != null) { CartageLeg.SetCalculatedDistance((INotifications)ParentForm); } };

			if (ShowLegSignatures)
			{
				drawer = new SignatureDrawer(SignaturePictureBox, true);
			}
			else
			{
				SignaturePictureBox.Visible = false;
			}

			UpdateAddressStrategy();

			DistanceButton.AllowOverlap(zPanel7);
			DeliveryAddressSelectionDropDown.AllowOverlap(CartageLegDeliveryGroupBox);
			zLabel3.AllowOverlap(CartageLegDeliveryGroupBox);
			PickupAddressSelectionDropDown.AllowOverlap(CartageLegPickupGroupBox);
			AddressGroupBoxCaption.AllowOverlap(CartageLegPickupGroupBox);
			DistanceButton.AllowOverlap(zPanel7);

			DistanceButton.AllowOutsideOfParent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning && !(ParentForm is INotifications))
			{
				throw new NotSupportedException("CartageLegControl can only be added to Forms that implement INotifications");
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UpdateAddressStrategy();
			UpdateSignature();
		}

		void UpdateAddressStrategy()
		{
			if (CartageLeg != null)
			{
				PickupAddressHelper.Parent = new PickupLegQuickAddressHelperStrategy(CartageLeg);
				WaitPointAddressHelper.Parent = new WaitPointLegQuickAddressHelperStrategy(CartageLeg);
				DeliveryAddressHelper.Parent = new DeliveryLegQuickAddressHelperStrategy(CartageLeg);
			}
			else
			{
				PickupAddressHelper.Parent = null;
				WaitPointAddressHelper.Parent = null;
				DeliveryAddressHelper.Parent = null;
			}
		}

		QuickAddressHelper PickupAddressHelper
		{
			get { return pickupAddressHelper ?? (pickupAddressHelper = new QuickAddressHelper(PickupAddressSelectionDropDown)); }
		}
		QuickAddressHelper pickupAddressHelper;

		QuickAddressHelper WaitPointAddressHelper
		{
			get { return waitPointAddressHelper ?? (waitPointAddressHelper = new QuickAddressHelper(WaitPointAddressSelectionDropDown)); }
		}
		QuickAddressHelper waitPointAddressHelper;

		QuickAddressHelper DeliveryAddressHelper
		{
			get { return deliveryAddressHelper ?? (deliveryAddressHelper = new QuickAddressHelper(DeliveryAddressSelectionDropDown)); }
		}
		QuickAddressHelper deliveryAddressHelper;

		void UpdateSignature()
		{
			if (ShowLegSignatures)
			{
				drawer.Leg = CartageLeg;
			}
		}
		readonly SignatureDrawer drawer;

		bool ShowLegSignatures
		{
			get
			{
				if (DesignModeFinder.IsDesigning)
				{
					return true;
				}

				if (!showLegSignatures.HasValue)
				{
					showLegSignatures = TransportRegistry.Instance.ShowLegSignatures.Value;
				}
				return showLegSignatures.Value;
			}
		}

		bool? showLegSignatures;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)PickupAddressHelper).Dispose();
				((IDisposable)WaitPointAddressHelper).Dispose();
				((IDisposable)DeliveryAddressHelper).Dispose();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		internal void RemoveQuickAllocationBinding()
		{
			this.BindingSource.SetBindingMember(this.zDateEdit17, "JU_PlannedPickupTime");
			this.BindingSource.SetBindingMember(this.zDateEdit12, "JU_EstimatedDeliveryTime");
			this.BindingSource.SetBindingMember(this.DeliveryETADateEdit, "JU_EstimatedDeliveryTime");
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "WorkSheet+EY_RQ_Truck");
			this.BindingSource.SetBindingMember(this.DriverCodeFindBox, "WorkSheet+EY_GS_NKTruckDriver");
			this.BindingSource.SetBindingMember(this.zGuidFindBox4, "WorkSheet+EY_OH_TransportCo");
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZBool CartageLegHasWaitPoint
		{
			get { return cartageLegHasWaitPoint; }
			set
			{
				if (cartageLegHasWaitPoint != value)
				{
					cartageLegHasWaitPoint = value;
					LegWaitpointPanel.Visible = value;
					DeliveryETADateEdit.Visible = !value;
				}
			}
		}
		ZBool cartageLegHasWaitPoint;

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			List<PropertyDescriptor> result = new List<PropertyDescriptor>();

			result.AddRange(new ControlPropertyDescriptorBuilder<CartageLegControl>()
			.Property("CartageLegHasWaitPoint", ZBool.False, false)
			.Result);

			return result.ToArray();
		}

		CommonCartageLeg CartageLeg
		{
			get { return (CommonCartageLeg)CurrentDataItem; }
		}
	}
}

#if DEBUG
namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageLegControl
	{
		public QuickAddressHelper PickupAddressHelperForTest
		{
			get { return PickupAddressHelper; }
		}

		public QuickAddressHelper WaitPointAddressHelperForTest
		{
			get { return WaitPointAddressHelper; }
		}

		public QuickAddressHelper DeliveryAddressHelperForTest
		{
			get { return DeliveryAddressHelper; }
		}
	}
}
#endif
