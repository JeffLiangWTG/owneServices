using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Confirmations.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ConsolidatedTransportBookingControl : ZUserControl
	{
		public ConsolidatedTransportBookingControl()
		{
			InitializeComponent();

			CustomerAddressControl.AllowOutsideOfParent();
		}

		#region Overrides

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CommonConsolidatedTransportBooking booking = (CommonConsolidatedTransportBooking)dataSource;
			if (booking != null)
			{
				DataBindings.RemoveBinding(nameof(ConfirmContainer));
			}

			base.SetDataBinding(dataSource, dataMember);

			if (booking != null)
			{
				DataBindings.Add(new KBinding(nameof(ConfirmContainer), booking, "Confirms.ContainerPK"));
			}

			ConfirmContainer = ZGuid.Empty;
		}

		#endregion

		#region ConfirmContainer

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZGuid ConfirmContainer
		{
			get { return confirmContainer; }
			set
			{
				if (confirmContainer != value)
				{
					confirmContainer = value;
					ConfirmPackLineContainerSplitContainer.Panel1Collapsed = !value.IsEmpty;
					ConfirmPackLineContainerSplitContainer.Panel2Collapsed = value.IsEmpty;
				}
			}
		}
		ZGuid confirmContainer = ZGuid.Missing;

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ConsolidatedTransportBookingControl>()
			.Property("ConfirmContainer", ZGuid.Empty, false)
			.Result;
		}

		#endregion

	}
}
