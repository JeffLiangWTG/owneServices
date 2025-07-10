using System;
using CargoWise.Common;
using CargoWise.Windows.UI.Layout;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI
{
	internal class TransportFlightStatusVisibilityProvider : IVisibilityProvider, IDisposable
	{
		readonly Transport transport;

		public TransportFlightStatusVisibilityProvider(Transport transport)
		{
			Argument.NotNull(transport, nameof(transport));

			this.transport = transport;
			transport.JW_TransportModeInfo.ValueChanged += UpdateFlightStatusVisibility;
		}

		void UpdateFlightStatusVisibility(object sender, EventArgs e)
		{
			VisibleChanged?.Invoke(sender, e);
		}

		public bool Visible => !transport.IsDeleted && transport.JW_TransportMode == Core.Constants.TransportModes.Air;

		public event EventHandler VisibleChanged;

		public void Dispose()
		{
			transport.JW_TransportModeInfo.ValueChanged -= UpdateFlightStatusVisibility;
		}
	}
}
