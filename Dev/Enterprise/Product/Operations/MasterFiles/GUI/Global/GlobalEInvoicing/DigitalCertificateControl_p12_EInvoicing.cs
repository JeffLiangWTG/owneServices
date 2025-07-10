using System;
using System.ComponentModel;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DigitalCertificateControl_p12_EInvoicing : DigitalCertificateControl_p12
	{
		public DigitalCertificateControl_p12_EInvoicing()
		{
			InitializeComponent();
		}

		#region Register Button

		[Browsable(true), DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool AllowRegister
		{
			get { return RegisterButton.Visible; }
			set { RegisterButton.Visible = value; }
		}

		[Browsable(true)]
		public event EventHandler OnDataRegister
		{
			add { fOnDataRegister += value; }
			remove { fOnDataRegister -= value; }
		}
		EventHandler fOnDataRegister;

		void RegisterButton_Click(object sender, EventArgs e)
		{
			fOnDataRegister?.Invoke(this, new EventArgs());
		}

		[Browsable(true)]
		public event EventHandler<DataRegistrationEventArgs> DataRegistered;

		public void OnDataRegistered(object sender, DataRegistrationEventArgs e)
		{
			DataRegistered?.Invoke(sender, e);
		}

		public class DataRegistrationEventArgs : EventArgs
		{
			public DataRegistrationEventArgs(string certificateData, string secret, string requestId)
			{
				this.CertificateData = certificateData;
				this.Secret = secret;
				this.RequestId = requestId;
			}

			public readonly string CertificateData;
			public readonly string Secret;
			public readonly string RequestId;
		}

		#endregion
	}
}
