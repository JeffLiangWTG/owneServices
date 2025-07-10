using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public static class IPortMessagingExtensions
	{
		public static bool HasData(this IPortMessaging portMessaging)
		{
			if (portMessaging == null)
			{
				return false;
			}

			return ((BusinessObject)portMessaging).ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(IsEntered);
		}

		static bool IsEntered(ZPropertyInfo propertyInfo)
		{
			if (PropertiesToExclude.Contains(propertyInfo.Name))
			{
				return false;
			}

			var value = propertyInfo.Value;

			if (value is ZBool)
			{
				return (ZBool)value;
			}

			return !value.IsEmpty;
		}

#if DEBUG
		public
#endif
		static HashSet<string> PropertiesToExclude
		{
			get
			{
				if (propertiesToExclude == null)
				{
					propertiesToExclude = new HashSet<string>();

					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.PK);
					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.JLM_JL_PackLine);
					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.JLM_SystemCreateTimeUtc);
					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.JLM_SystemCreateUser);
					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.JLM_SystemLastEditTimeUtc);
					propertiesToExclude.Add(JobPackLinePortMessagingSchema.Constants.JLM_SystemLastEditUser);
					propertiesToExclude.Add(PackLinePortMessaging.Schema.DGTechnicalName);

					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.PK);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_JS_Shipment);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_SystemCreateTimeUtc);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_SystemCreateUser);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_SystemLastEditTimeUtc);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_SystemLastEditUser);
					propertiesToExclude.Add(JobShipmentPortMessagingSchema.Constants.JSM_ForwardingCustomsOfficeCode);
				}

				return propertiesToExclude;
			}
		}

		[ThreadStatic]
		static HashSet<string> propertiesToExclude;
	}
}
