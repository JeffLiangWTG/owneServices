using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public static class OrganisationTypeDescriptor
	{
#if DEBUG
		internal
#endif
		static readonly ImmutableDictionary<OrganisationTypes, MultilingualString> Translations = new Dictionary<OrganisationTypes, MultilingualString>()
		{
			[OrganisationTypes.None] = Descriptions.None,
			[OrganisationTypes.Debtor] = Descriptions.Debtor,
			[OrganisationTypes.Creditor] = Descriptions.Creditor,
			[OrganisationTypes.Consignor] = Descriptions.Consignor,
			[OrganisationTypes.Consignee] = Descriptions.Consignee,
			[OrganisationTypes.TransportClient] = Descriptions.TransportClient,
			[OrganisationTypes.Carrier] = Descriptions.Carrier,
			[OrganisationTypes.Forwarder] = Descriptions.Forwarder,
			[OrganisationTypes.Broker] = Descriptions.Broker,
			[OrganisationTypes.Services] = Descriptions.Services,
			[OrganisationTypes.Competitor] = Descriptions.Competitor,
			[OrganisationTypes.Sales] = Descriptions.Sales,
			[OrganisationTypes.WarehouseClient] = Descriptions.WarehouseClient,
			[OrganisationTypes.DistributionCentre] = Descriptions.DistributionCentre,
			[OrganisationTypes.ControllingAgent] = Descriptions.ControllingAgent,
			[OrganisationTypes.ControllingCustomer] = Descriptions.ControllingCustomer,
		}.ToImmutableDictionary();

		public static ZString GetString(OrganisationTypes types)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (OrganisationTypes currentType in Enum.GetValues(typeof(OrganisationTypes)))
			{
				if ((currentType & types) != 0)
				{
					if (Translations.TryGetValue(currentType, out var typeName))
					{
						if (result.Length > 0)
						{
							result.Append(", ");
						}

						result.Append(typeName);
					}
				}
			}

			return result.ToString();
		}

		public static class Descriptions
		{
			public static MultilingualString None => ResString.GetMultilingualString("OrganisationTypes|None", "None");
			public static MultilingualString Debtor => ResString.GetMultilingualString("OrganisationTypes|Debtor", "Debtor");
			public static MultilingualString Creditor => ResString.GetMultilingualString("OrganisationTypes|Creditor", "Creditor");
			public static MultilingualString Consignor => ResString.GetMultilingualString("OrganisationTypes|Consignor", "Consignor");
			public static MultilingualString Consignee => ResString.GetMultilingualString("OrganisationTypes|Consignee", "Consignee");
			public static MultilingualString TransportClient => ResString.GetMultilingualString("OrganisationTypes|TransportClient", "Transport Client");
			public static MultilingualString Carrier => ResString.GetMultilingualString("OrganisationTypes|Carrier", "Carrier");
			public static MultilingualString Forwarder => ResString.GetMultilingualString("OrganisationTypes|Forwarder", "Forwarder");
			public static MultilingualString Broker => ResString.GetMultilingualString("OrganisationTypes|Broker", "Broker");
			public static MultilingualString Services => ResString.GetMultilingualString("OrganisationTypes|Services", "Services");
			public static MultilingualString Competitor => ResString.GetMultilingualString("OrganisationTypes|Competitor", "Competitor");
			public static MultilingualString Sales => ResString.GetMultilingualString("OrganisationTypes|Sales", "Sales");
			public static MultilingualString WarehouseClient => ResString.GetMultilingualString("OrganisationTypes|WarehouseClient", "Warehouse Client");
			public static MultilingualString DistributionCentre => ResString.GetMultilingualString("OrganisationTypes|DistributionCentre", "Distribution Center");
			public static MultilingualString ControllingAgent => ResString.GetMultilingualString("OrganisationTypes|ControllingAgent", "Controlling Agent");
			public static MultilingualString ControllingCustomer => ResString.GetMultilingualString("OrganisationTypes|ControllingCustomer", "Controlling Customer");
		}
	}
}
