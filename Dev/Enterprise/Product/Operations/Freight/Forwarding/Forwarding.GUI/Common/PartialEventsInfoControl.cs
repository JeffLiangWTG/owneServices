using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PartialEventsInfoControl : ZUserControl, IExtendedControl
	{
		public PartialEventsInfoControl()
		{
			InitializeComponent();

			Extensions = new ControlExtensionCollection(this)
			{
				new ZLabelCaptionRenderer()
			};
		}

		PartialEventsInfo eventsInfo;

		public enum EventTypes
		{
			Arrival,
			Departure
		}

		public EventTypes EventsToShow
		{
			get; set;
		}

		ZString[] GetEventCodes()
		{
			return (EventsToShow == EventTypes.Arrival)
				? new[] { (ZString)AutoEvents.FreightUnloadedCode }
				: new[] { (ZString)AutoEvents.FreightLoadedCode };
		}

		ZString GetLocationCode(ForwardingConsol consol)
		{
			var result = ZString.Empty;

			if (consol != null)
			{
				if (EventsToShow == EventTypes.Arrival)
				{
					result = consol.Transports.LastTransportWithTransportMode(Core.Constants.TransportModes.Air)?.DiscPort?.RL_Code ?? ZString.Empty;
				}
				else
				{
					result = consol.Transports.FirstTransportWithTransportMode(Core.Constants.TransportModes.Air)?.LoadPort?.RL_Code ?? ZString.Empty;
				}
			}

			return result;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null)
			{
				base.SetDataBinding(dataSource, dataMember);
			}
			else
			{
				var consol = dataSource as ForwardingConsol;
				eventsInfo = new PartialEventsInfo(consol, GetEventCodes(), GetLocationCode(consol));

				base.SetDataBinding(eventsInfo, "");
			}
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			Visible = eventsInfo == null || !eventsInfo.IsEmpty;
		}

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; }

		#endregion
	}
}
