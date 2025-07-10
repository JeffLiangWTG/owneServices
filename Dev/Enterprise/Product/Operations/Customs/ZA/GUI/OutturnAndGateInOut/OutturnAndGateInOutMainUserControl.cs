using System;
using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutMainUserControl : ZUserControl
	{
		public OutturnAndGateInOutMainUserControl()
		{
			InitializeComponent();
			zCodeFindBoxVessel.PopupSelected -= ZCodeFindBoxVessel_PopupSelected;
			zCodeFindBoxVessel.PopupSelected += ZCodeFindBoxVessel_PopupSelected;

#if DEBUG
			TypeDescriptor.AddAttributes(zDropEditWithFixedWidthCustomsOffice, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zDropEditWithFixedWidthCustomsStatus, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(GateInOutStatusDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zDropEditWithFixedWidthExcessIndicator, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void AMA_ManifestTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var enabled = manifestHeader.AMA_ManifestType.IsEmpty;
			zDropGateInOutMessageType.Enabled = enabled;
			zDateEditGateInOutDate.Enabled = enabled;
			GateInOutStatusDropEdit.Enabled = enabled;
		}

		void GateInOutMessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var enabled = manifestHeader.GateInOutMessageType.IsEmpty;
			zDropEditWithFixedWidthManifestType.Enabled = enabled;
			zDateEditFullyLoadedUnloadedDate.Enabled = enabled;
			zDateEditUnpacked.Enabled = enabled;
			zDropEditWithFixedWidthExcessIndicator.Enabled = enabled;
		}

		void ZCodeFindBoxVessel_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1
				&& e.SelectedBusinessObjects[0] is RefVessel vessel
				&& ManifestHeader is AsycudaManifestHeader header)
			{
				header.DefaultVesselValues(vessel);
			}
		}

		internal AsycudaManifestHeader ManifestHeader
		{
			get => manifestHeader;
			set
			{
				UnWireHandlersFromBiz(manifestHeader);
				manifestHeader = value;
				WireHandlersFromBiz(manifestHeader);
			}
		}
		AsycudaManifestHeader manifestHeader;

		void WireHandlersFromBiz(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				header.AMA_TransportModeInfo.ValueChanged -= ManifestHeader_OnTransportModeChanged;
				header.AMA_TransportModeInfo.ValueChanged += ManifestHeader_OnTransportModeChanged;
				ManifestHeader_OnTransportModeChanged(header, EventArgs.Empty);

				header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
				header.AMA_ManifestTypeInfo.ValueChanged += AMA_ManifestTypeInfo_ValueChanged;
				AMA_ManifestTypeInfo_ValueChanged(header, EventArgs.Empty);

				header.GateInOutMessageTypeInfo.ValueChanged -= GateInOutMessageTypeInfo_ValueChanged;
				header.GateInOutMessageTypeInfo.ValueChanged += GateInOutMessageTypeInfo_ValueChanged;
				GateInOutMessageTypeInfo_ValueChanged(header, EventArgs.Empty);
			}
		}

		void UnWireHandlersFromBiz(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				header.AMA_TransportModeInfo.ValueChanged -= ManifestHeader_OnTransportModeChanged;
				header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
				header.GateInOutMessageTypeInfo.ValueChanged -= GateInOutMessageTypeInfo_ValueChanged;
			}
		}

		void ManifestHeader_OnTransportModeChanged(object sender, EventArgs e)
		{
			zTabPageContainer.TabVisible = ManifestHeader?.AMA_ContainerModeVisible ?? ZBool.False;
			zTextBoxVoyage.GetExtension<ILabelCaptionRenderer>().Caption = ManifestHeader?.VoyageFlightNoLabel ?? Res.GetString("{5046D938-0F0D-4AC3-BF11-29EF12584DBC}", "Flight/Voyage");
		}
	}
}
