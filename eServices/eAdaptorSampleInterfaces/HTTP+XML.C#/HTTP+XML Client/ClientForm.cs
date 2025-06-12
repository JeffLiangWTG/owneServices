using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Winform_Client
{
	public partial class ClientForm : System.Windows.Forms.Form
	{
		public ClientForm()
		{
			InitializeComponent();
			this.uriTextBox.Text = "http://sydco-wbjg-1/OdysseyBen/Services/eAdaptor";
			this.userNameTextBox.Text = "abc";
			this.passwordTextBox.Text = "123";

			this.sendMessageTextBox.Text = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsDeclaration</Type>
          <Key>B00001000</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>BEN</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>BEN</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			receivedMessageTextBox.Text = "Posting...\r\n";

			HTTPPostXMLMessage();

			Log("HTTP POST Complete.");
		}

		void Log(string text)
		{
			receivedMessageTextBox.AppendText(text + "\r\n");
		}

		void HTTPPostXMLMessage()
		{
			// It may be instructive to view the output of this sample in Microsoft Fiddler (http://www.fiddlertool.com).
			// This will allow you to see the raw POST and Reponse with all HTTP Headers and your XML body content.

			var uri = new Uri(uriTextBox.Text);
			var client = new HttpXmlClient(uri, compressCheckBox.Checked, userNameTextBox.Text, passwordTextBox.Text);

			using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(sendMessageTextBox.Text)))
			{
				Log("Begin POST to " + uri);
				Log("        <<<------------------------------------------------- Begin Message Body ------------------------------------------------->>>");
				Log(sendMessageTextBox.Text);
				Log("        <<<-------------------------------------------------- End Message Body -------------------------------------------------->>>");
				Log("");
				Log("Waiting Response...");
				Log("");

				try
				{
					var response = client.Post(sourceStream);
					var responseStatus = response.StatusCode;

					Log((responseStatus == HttpStatusCode.OK ? "Response Received" : "ERROR RESPONSE RECEIVED") + ", Status:- " + (int)responseStatus + " - " + response.ReasonPhrase);

					if (response.Content != null)
					{
						var stream = response.Content.ReadAsStreamAsync().Result;

						if (response.Content.Headers.ContentEncoding.Contains("gzip", StringComparer.InvariantCultureIgnoreCase))
						{
							stream = new GZipStream(stream, CompressionMode.Decompress);
						}

						using (var reader = new StreamReader(stream))
						{
							Log("        <<<------------------------------------------------- Begin Response Body ------------------------------------------------->>>");
							Log(reader.ReadToEnd());
							Log("        <<<-------------------------------------------------- End Response Body -------------------------------------------------->>>");
						}
					}
					Log("");
				}
				catch (Exception exception)
				{
					Log("EXCEPTION THROWN DURING POST!!!!");
					Log(exception.ToString());
					Log("");
				}
			}
		}
	}
}
