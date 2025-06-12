namespace WTG.XNDT.SampleClient
{
	using System;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using WTG.XNDT.SampleClient.ServiceReference;

	public partial class SampleClientForm : Form
	{
		public SampleClientForm()
		{
			this.InitializeComponent();
			this.responseTextBox.MaxLength = 0x7fffffff;
			this.requestTextBox.MaxLength = 0x7fffffff;
		}

		private void UpdateButton_Click(object sender, EventArgs e)
		{
			ManageWebMethodCall((client, request) => client.Update(request));
		}

		private void RetrieveButton_Click(object sender, EventArgs e)
		{
			ManageWebMethodCall((client, request) => client.Retrieve(request));
		}

		void ManageWebMethodCall(ClientMethodCall callWebMethod)
		{
			try
			{
				this.responseTextBox.Text = "Processing...";
				XElement responseXML;
				XElement requestXML = XElement.Parse(this.requestTextBox.Text);
				using (var client = this.CreateServiceClient())
				{
					responseXML = callWebMethod(client, requestXML);
					client.Close();
				}
				this.responseTextBox.Text = responseXML.ToString();
			}
			catch (Exception exception)
			{
				this.responseTextBox.Text = exception.ToString();
			}
		}

		delegate XElement ClientMethodCall(EnterpriseNativeDataServiceClient client, XElement request);
		EnterpriseNativeDataServiceClient CreateServiceClient()
		{
			this.endpointConfigurationName = "WSHttpBinding_EnterpriseNativeDataService";
			this.remoteAddress = this.addressTextBox.Text;
			if (string.IsNullOrEmpty(this.remoteAddress))
			{
				return new EnterpriseNativeDataServiceClient();
			}
			return new EnterpriseNativeDataServiceClient(this.endpointConfigurationName, this.remoteAddress);
		}
	}
}

