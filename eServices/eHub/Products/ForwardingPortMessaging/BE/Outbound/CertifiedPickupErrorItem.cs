using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    [Serializable]
    public class CertifiedPickupErrorItem
    {

        [JsonProperty("$.releaseIdentification")]
        public string[] ReleaseIdentification;

        [JsonProperty("$.terminalCode")]
        public string[] TerminalCode;

        [JsonProperty("$.actionType")]
        public string[] ActionType;

        [JsonProperty("$.billOfLadingNumbers")]
        public string[] BillOfLadingNumbers;

        [JsonProperty("$.releaseFrom.identificationType")]
        public string[] ReleaseFromIdentificationType;

        [JsonProperty("$.releaseFrom.identificationCode")]
        public string[] ReleaseFromIdentificationCode;

        [JsonProperty("$.releaseTo.identificationType")]
        public string[] ReleaseToIdentificationType;

        [JsonProperty("$.releaseTo.identificationCode")]
        public string[] ReleaseToIdentificationCode;

        [JsonProperty("$.equipmentNumber")]
        public string[] EquipmentNumber;

        [JsonProperty("$.portLoCode")]
        public string[] PortLoCode;

        public string GetErrors()
        {
            var errorBuilder = new StringBuilder();
            if (ReleaseIdentification != null && ReleaseIdentification.Length > 0)
            {
                errorBuilder.Append(string.Format("ReleaseIdentification: {0};", ReleaseIdentification[0]));
            }
            if (TerminalCode != null && TerminalCode.Length > 0)
            {
                errorBuilder.Append(string.Format("TerminalCode: {0};", TerminalCode[0]));
            }
            if (ActionType != null && ActionType.Length > 0)
            {
                errorBuilder.Append(string.Format("ActionType: {0};", ActionType[0]));
            }
            if (BillOfLadingNumbers != null && BillOfLadingNumbers.Length > 0)
            {
                errorBuilder.Append(string.Format("BillOfLadingNumbers: {0};", BillOfLadingNumbers[0]));
            }
            if (ReleaseFromIdentificationType != null && ReleaseFromIdentificationType.Length > 0)
            {
                errorBuilder.Append(string.Format("ReleaseFromIdentificationType: {0};", ReleaseFromIdentificationType[0]));
            }
            if (ReleaseFromIdentificationCode != null && ReleaseFromIdentificationCode.Length > 0)
            {
                errorBuilder.Append(string.Format("ReleaseFromIdentificationCode: {0};", ReleaseFromIdentificationCode[0]));
            }
            if (ReleaseToIdentificationType != null && ReleaseToIdentificationType.Length > 0)
            {
                errorBuilder.Append(string.Format("ReleaseToIdentificationType: {0};", ReleaseToIdentificationType[0]));
            }
            if (ReleaseToIdentificationCode != null && ReleaseToIdentificationCode.Length > 0)
            {
                errorBuilder.Append(string.Format("ReleaseToIdentificationCode: {0};", ReleaseToIdentificationCode[0]));
            }

            if (EquipmentNumber != null && EquipmentNumber.Length > 0)
            {
                errorBuilder.Append(string.Format("EquipmentNumber: {0};", EquipmentNumber[0]));
            }
            if (PortLoCode != null && PortLoCode.Length > 0)
            {
                errorBuilder.Append(string.Format("PortLoCode: {0};", PortLoCode[0]));
            }
            return errorBuilder.ToString();
        }

    }
}
