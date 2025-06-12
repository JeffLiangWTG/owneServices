using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Microsoft.BizTalk.Adapter.Framework;

namespace CargoWise.eHub.BizTalkAdapters.FtpExPolling.Admin
{
	public class FtpExPollingAdapterManagement :
		AdapterManagement,
		IAdapterConfig,
		IAdapterConfigValidation
    {
        protected override string Scheme() { return "ftpex-polling"; }

        public override string GetConfigSchema(ConfigType configType)
        {
            switch (configType)
            {
                case ConfigType.TransmitHandler:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpExPolling.Admin.FtpExPollingTransmitHandler.xsd");
                case ConfigType.TransmitLocation:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpExPolling.Admin.FtpExPollingTransmitLocation.xsd");
                default:
                    return String.Empty;
            }
        }

		protected override string ValidateTransmitLocation(System.Xml.XmlDocument configXml)
		{
			SetNamedUri(configXml);
			return configXml.OuterXml;
		}
    }
}
