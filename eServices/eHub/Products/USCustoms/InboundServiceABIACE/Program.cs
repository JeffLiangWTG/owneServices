using System;
using System.Configuration;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Reflection;
using CargoWise.eServices.USCustoms.MQConfigurationABIACE;

namespace CargoWise.eServices.USCustoms.InboundServiceABIACE
{
	static class Program
	{
		static void Main()
		{
            var inboundService = new CargoWise.eServices.USCustoms.InboundService.InboundService(ConfigurationManager.AppSettings.Get("ServiceName"));
            inboundService.GetInboundConfiguration = (x) => (new  MQABIACEConfiguration(x));
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
