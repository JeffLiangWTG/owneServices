using System;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	internal static class PropertyInspector
	{
		public static bool IsAs2Mdn(IBaseMessage message)
		{
			var isAs2Payload = message.Context.ReadPropertyString<EdiIntAS.IsAS2PayloadMessage>();
			return (isAs2Payload != null && !Convert.ToBoolean(isAs2Payload));
		}

		public static bool IsAs2Payload(IBaseMessage message)
		{
			var isAs2Payload = message.Context.ReadPropertyString<EdiIntAS.IsAS2PayloadMessage>();
			return (isAs2Payload != null && Convert.ToBoolean(isAs2Payload));
		}

		/// <summary>
		/// Checks for the existence of http://schemas.microsoft.com.Edi/PropertySchema#IsSystemGeneratedAck to determine if the message
		/// is a functional or technical acknowledgement
		/// </summary>
		public static bool IsSystemGeneratedEdiAck(IBaseMessage message)
		{
			var isAck = message.Context.ReadPropertyString<EDI.IsSystemGeneratedAck>();
			return (isAck != null && Convert.ToBoolean(isAck));
		}
	}
}
