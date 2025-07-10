namespace Enterprise.Freight.GUI
{
	using System;
	using CargoWise.Windows.UI.Layout;
	using Enterprise.Freight.Business;

	public class TransportCargoOnlyVisibilityProvider : IVisibilityProvider, IDisposable
	{
		public TransportCargoOnlyVisibilityProvider(Transport transport)
		{
			if (transport == null)
			{
				throw new ArgumentNullException("Transport", "Transport must not be null.");
			}

			this.transport = transport;
			transport.JW_IsLinkedInfo.ValueChanged += CargoOnlyVisibleChanged;
			transport.JW_TransportModeInfo.ValueChanged += CargoOnlyVisibleChanged;
		}

		#region IVisibilityDependencyProvider

		public bool Visible
		{
			get
			{
				if (!transport.IsDeleted)
				{
					return transport.JW_TransportMode == Core.Constants.TransportModes.Air;
				}
				else
				{
					return false;
				}
			}
		}

		public event EventHandler VisibleChanged;

		#endregion

		#region IDisposable

		bool isDisposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool dispose)
		{
			if (!isDisposed)
			{
				if (dispose)
				{
					transport.JW_TransportModeInfo.ValueChanged -= CargoOnlyVisibleChanged;
				}
			}

			isDisposed = true;
		}

		#endregion

		#region Implementation

		void CargoOnlyVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (VisibleChanged != null)
			{
				VisibleChanged(sender, eventArgs);
			}
		}

		readonly Transport transport;

		#endregion
	}
}
