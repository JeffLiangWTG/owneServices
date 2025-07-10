using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class TransportJobLinkManager : IDisposable
	{
		#region Construction

		public TransportJobLinkManager(ITransportJobLinkProvider transportParent, ZLinkLabel label)
		{
			TransportParent = Argument.NotNull(transportParent, "transportParent");
			TransportJobLinkLabel = Argument.NotNull(label, "label");
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected readonly ITransportJobLinkProvider TransportParent;
		protected readonly ZLinkLabel TransportJobLinkLabel;

		#endregion

		#region Manage / Update Link

		public void ManageLink()
		{
			UpdateLink();
			TransportParent.TransportJobCreatedAndSaved += new EventHandler(TransportParent_TransportJobCretedAndSaved);
		}

		// Tested in ReleaseEntryForm.cs
		public void UnhookLinkOnPreviousOrderAndHookToCurrentOrder(ITransportJobLinkProvider previousOrder, ITransportJobLinkProvider currentOrder)
		{
			if (previousOrder != null)
			{
				previousOrder.TransportJobCreatedAndSaved -= new EventHandler(TransportParent_TransportJobCretedAndSaved);
			}

			if (currentOrder != null)
			{
				currentOrder.TransportJobCreatedAndSaved += new EventHandler(TransportParent_TransportJobCretedAndSaved);
			}
		}

		public void UpdateLink()
		{
			var transportJobResult = TransportParent.TransportJobResult;
			var transportJob = transportJobResult != null ? transportJobResult.TransportJob : null;

			if (transportJob != null)
			{
				TransportJobLinkLabel.LinkColor = Color.Red;
				TransportJobLinkLabel.Text = transportJob.JobNumber;
				TransportJobLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(Label_LinkClicked);
			}
			else
			{
				var overridenText = transportJobResult != null ? transportJobResult.ReasonForEmptyOverride : null;
				TransportJobLinkLabel.LinkColor = SystemColors.WindowText;
				TransportJobLinkLabel.Text = overridenText ?? Res.GetString("84a18033-04b5-4259-836a-3f4c8ba65102", "No Transport Job exists.");
				TransportJobLinkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(Label_LinkClicked);
			}
		}

		void TransportParent_TransportJobCretedAndSaved(object sender, EventArgs e)
		{
			UpdateLink();
		}

		#endregion

		#region HandleTransportJobLinkClick

		void Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			HandleTransportJobLinkClick();
		}

		void HandleTransportJobLinkClick()
		{
			var transportJobResult = TransportParent.TransportJobResult;
			var transportJob = transportJobResult != null ? transportJobResult.TransportJob : null;

			if (transportJob != null)
			{
				Controller = ZControllerFactory.Create(transportJob.ControllerID);
				Controller.ShowEditForm((BusinessObject)transportJob);
			}
		}

#if DEBUG // for testing
		public
#endif
 ZController Controller;

		#endregion

		#region IDisposable Members

		bool disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					TransportParent.TransportJobCreatedAndSaved -= new EventHandler(TransportParent_TransportJobCretedAndSaved);
					TransportJobLinkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(Label_LinkClicked);
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}

				disposed = true;
			}
		}

		#endregion
	}
}
