using System;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	class AirBookingProgressFormManager : DocumentVisualizerProgressManager
	{
		public AirBookingProgressFormManager(Action onUserRequestedCancel)
		{
			this.onUserRequestedCancel = onUserRequestedCancel;
		}

		readonly Action onUserRequestedCancel;

		protected override DocumentVisualizerProgressForm CreateFormCore()
		{
			var form = new DocumentVisualizerProgressForm();
			form.ShowCancelButton = true;
			form.SetMaxTimeInSeconds(FreightDataRegistry.Instance.EBookingApiTimeoutInSeconds.Value);
			form.CancelProgressButtonText = Res.GetString("cc931f78-022d-453f-a0cb-e51e3c194ee1", "Close");
			if (onUserRequestedCancel != null)
			{
				void OnCancelled(object sender, EventArgs e) => onUserRequestedCancel();
				form.Cancelled += OnCancelled;
			}

			return form;
		}
	}
}
