using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.eServices.USCustoms.MQConfigurationAMA;

namespace CargoWise.eServices.USCustoms.InboundServiceAMA
{
    static class Program
    {
        static void Main(string[] args)
        {
			var inboundService = new CargoWise.eServices.USCustoms.InboundService.InboundService(ConfigurationManager.AppSettings.Get("ServiceName"));
			inboundService.GetInboundConfiguration = (x) => (new MQAMAConfiguration(x));

			if (!Environment.UserInteractive)
			{
				ServiceBase.Run(inboundService);
			}
			else
			{
				RunServiceInteractively(inboundService);
			}
		}

		static void RunServiceInteractively(CargoWise.eServices.USCustoms.InboundService.InboundService inboundService)
		{
			string dialogCaption = "US Customs Inbound Service";
			if (MessageBox.Show("Run '" + inboundService.ServiceName + "' service interactively?", dialogCaption, MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				inboundService.StartTask(null);
				MessageBox.Show("Close this dialog to stop running  '" + inboundService.ServiceName + "' service interactively.", dialogCaption);
				inboundService.Stop();
			}
		}
	}
}
