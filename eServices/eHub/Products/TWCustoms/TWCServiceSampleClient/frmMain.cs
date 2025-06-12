using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace CargoWise.eHub.Products.TWCustoms.TWCServiceSampleClient
{


	public partial class frmMain : Form
    {
        HttpClient httpc = new HttpClient();
		//string baseUrl = @"http://au2sp-stwc-401:9090";

		public frmMain()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log("Unhandled Exception", null, typeof(Exception).IsAssignableFrom(e.ExceptionObject.GetType()) ? ((Exception)e.ExceptionObject) : null);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                log("Send request sent.... waiting for response.", null, null);
                
                TWCPluginService.Client twcc = new TWCPluginService.Client(httpc);
                twcc.BaseUrl = AddressTextBox.Text;

                var req = new TWCPluginService.TWCPluginServiceSendRequest()
                {
                    ClientRegistrationId = new Guid(txtRegistrationId.Text),
                    RegistrationConfiguration = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(txtConfigXML.Text)),
                    MessageId = txtMessageId.Text,
                    MessageFormat = txtMessageType.Text,
                    MessageBodyBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(txtMessageBody.Text))
                };

                if (!String.IsNullOrEmpty(txtAttachmentAddress.Text)) {
                    TWCPluginService.TWCPluginServiceSendRequestAttachment att = new TWCPluginService.TWCPluginServiceSendRequestAttachment() {
                        AttachmentName = txtAttachmentName.Text,
                        AttachmentFileType = txtAttachmentFormat.Text,
                        AttachmentDataBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(txtAttachmentAddress.Text))
                    };
                    req.Attachments = new List<TWCPluginService.TWCPluginServiceSendRequestAttachment>();
                    req.Attachments.Add(att);
                }

                var res = twcc.SendMessageUsingPOSTAsync(req);

                var result = await res;
                log("Send request complete.", result, null);
            }
            catch (Exception ex)
            {
                log("Send Exception", null, ex );
            }
        }


        public void log(string Desc, TWCPluginService.TWCPluginServiceResponse response, Exception ex)
        {
            if (InvokeRequired)
            {
                this.Invoke(
                    (MethodInvoker)delegate
                    {
                        log(Desc, response, ex);
                    }
                    );
            }
            else
            {
                string strlog = $"{DateTime.Now} - {Desc} \n";
                if (response != null)
                {
                    strlog +=
                        $"  ResponseCode:{response.ResponseCode} \n" +
                        $"  ResponseMessage:{response.ResponseMessage} ";
                }
                if (ex != null)
                {
                    strlog +=
                        $"  HResult:{ex.HResult} \n" +
                        $"  Error Message:{ex.Message} " +
                        $"  Stack Trace:{ex.StackTrace.ToString()}";
                }
                txtLog.Text = strlog + "\r\n" + txtLog.Text ;
            }
        }

        private async void BtnReceive_Click(object sender, EventArgs e)
        {
            log("Receive request sent... waiting for response.", null, null);
            TWCPluginService.Client twcc = new TWCPluginService.Client(httpc);
            twcc.BaseUrl = AddressTextBox.Text;

            var res = twcc.ReceiveMessageUsingPOSTAsync(new TWCPluginService.TWCPluginServiceReceiveRequest()
            {
                ClientRegistrationId = new Guid(txtRegistrationId.Text),
                RegistrationConfiguration = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(txtConfigXML.Text)),
            });

            try
            {
                var result = await res;
                log("Receive request sent.", result, null);
            }
            catch (Exception ex)
            {

                log("Receive request failed.", null, ex);
            }
        }

        private void TxtConfigXML_TextChanged(object sender, EventArgs e)
        {

        }

        private void BtnAttachment_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                txtAttachmentAddress.Text = ofd.FileName;
            }
        }
    }
}
