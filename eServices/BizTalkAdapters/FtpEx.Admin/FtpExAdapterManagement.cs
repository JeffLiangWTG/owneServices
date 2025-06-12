using System;
using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.BizTalk.Adapter.Framework;

namespace CargoWise.eHub.BizTalkAdapters.FtpEx.Admin
{
    public class FtpExAdapterManagement :
        TransferrerAdapterManagement,
        IAdapterConfig,
        IAdapterConfigValidation
    {
        public override string Scheme() { return "ftpex"; }

        #region IAdapterConfig
        public override string GetConfigSchema(ConfigType configType)
        {
            switch (configType)
            {
                case ConfigType.TransmitHandler:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExTransmitHandler.xsd");
                case ConfigType.TransmitLocation:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExTransmitLocation.xsd");
                case ConfigType.ReceiveHandler:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExReceiveHandler.xsd");
                case ConfigType.ReceiveLocation:
                    return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExReceiveLocation.xsd");
                default:
                    return String.Empty;
            }
        }
        #endregion IAdapterConfig
    }
}
