using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AutoRateDateReadOnlyHelper
	{
		public static bool Location_ReadOnly(ZPropertyInfo directionPropertyInfo) => directionPropertyInfo.ReadOnly || (ZString)directionPropertyInfo.Value == "ALL";

		#region ContainerMode

		public static bool ContainerMode_ReadOnly(string jobType, string mode)
			=> !EditableJobTypesList.Contains(jobType)
			|| !EditableTransportModesListByJobType(jobType).Contains(mode);

		static ZString[] EditableJobTypesList => new ZString[]
		{
			JobInvoicingConsumerTypes.ForwardingConsolCode,
			JobInvoicingConsumerTypes.GatewayConsolCode,
			JobInvoicingConsumerTypes.ShipmentCode,
			JobInvoicingConsumerTypes.QuotedBookingCode,
		};

		static string[] EditableTransportModesListByJobType(string jobType) => jobType switch
		{
			JobInvoicingConsumerTypes.ForwardingConsolCode or JobInvoicingConsumerTypes.GatewayConsolCode => new[]
			{
				Constants.TransportModes.Sea,
				Constants.TransportModes.Air,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail,
			},
			JobInvoicingConsumerTypes.ShipmentCode => new[]
			{
				Constants.TransportModes.Sea,
				Constants.TransportModes.Air,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail,
				Constants.TransportModes.AirSea,
				Constants.TransportModes.SeaAir,
				Constants.TransportModes.Courier,
			},
			JobInvoicingConsumerTypes.QuotedBookingCode => new[]
			{
				Constants.TransportModes.Sea,
				Constants.TransportModes.Air,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail,
				Constants.TransportModes.Courier,
			},
			_ => Array.Empty<string>()
		};

		#endregion

		public static void UpdateFieldIfShouldBeReadOnly(bool fieldReadOnly, ZPropertyInfo propertyInfo)
		{
			if (fieldReadOnly)
			{
				var valueAsString = (ZString)propertyInfo.Value;
				if (!valueAsString.IsEmpty)
				{
					propertyInfo.Value = ZString.Empty;
				}
			}
		}
	}
}
