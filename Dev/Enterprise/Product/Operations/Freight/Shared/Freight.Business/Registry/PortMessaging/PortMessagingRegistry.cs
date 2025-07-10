using System;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public sealed class PortMessagingRegistry : RegistryItemSet, IPortMessagingRegistry
	{
		PortMessagingRegistry()
		{
		}

		#region Instance

		public static PortMessagingRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new PortMessagingRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static PortMessagingRegistry fInstance;

		#endregion

		#region Implementation

		public override bool IsForProductivityWise => false;

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Freight_PortMessaging => CombineCategories(Freight, ResString.GetMultilingualString("A27CFF2D-4641-4161-A32F-AAD6A40067E7", "Port Messaging"));
			public static MultilingualString Freight_PortMessaging_France => CombineCategories(Freight_PortMessaging, ResString.GetMultilingualString("DA040CE3-973F-477E-9505-33AD560535C8", "France"));
			public static MultilingualString Freight_PortMessaging_South_Africa => CombineCategories(Freight_PortMessaging, ResString.GetMultilingualString("f292c976-a4c3-47ae-94eb-438800f724a9", "South Africa"));
			public static MultilingualString Freight_PortMessaging_Germany => CombineCategories(Freight_PortMessaging, ResString.GetMultilingualString("e421d0af-3366-4d66-8176-efd49e0dfc8c", "Germany"));
			public static MultilingualString Freight_PortMessaging_Germany_Dakosy => CombineCategories(Freight_PortMessaging_Germany, ResString.GetMultilingualString("8337dee1-efab-4c7c-8072-95bc967a9e23", "Dakosy"));
			public static MultilingualString Freight_PortMessaging_Netherlands => CombineCategories(Freight_PortMessaging, ResString.GetMultilingualString("d8927dfc-fe12-411f-b242-989af6c9d401", "Netherlands"));
		}

		#endregion

		#region Port Community System Codes of Forwarder and Agent

		public CommunitySystemCodesOfForwarderAndAgentRegistryItem CommunitySystemCodesOfForwarderAndAgent
		{
			get
			{
				return GetItem("CommunitySystemCodesOfForwarderAndAgent", () =>
				{
					var result = new CommunitySystemCodesOfForwarderAndAgentRegistryItem(
						"CommunitySystemCodesOfForwarderAndAgent",
						Categories.Freight_PortMessaging_France,
						ResString.GetMultilingualString("0750FAA5-90EF-4690-BD14-906F9445C558", "Port Community System Codes of Forwarder and Agent"),
						ResString.GetMultilingualString("5E2DA8F2-EFAD-467E-B49B-901B7F3D01D1", @"This list defines Forwarder and Agent code assigned by the Port Community System for each port for your organization. You must add the Forwarder and Agent code for each port at company level."),
						RegistryStorageFlags.Company,
						new CommunitySystemCodesOfForwarderAndAgentCollection());
					result.CountryFilterPKs = PortMessagingCountryList.FranceAndOverseasDepartments;
					return result;
				});
			}
		}

		public BooleanRegistryItem AllowToSendExportNotification
		{
			get
			{
				return GetItem("AllowToSendExportNotification", delegate
				{
					var result = new BooleanRegistryItem(
						"AllowToSendExportNotification",
						Categories.Freight_PortMessaging_France,
						ResString.GetMultilingualString("6f9c25c5-3e2f-4f98-b5fe-4ac5aec74632", "Allow to send Export Notification (755) to Cargo Information Network"),
						ResString.GetMultilingualString("9f714b29-c408-40fb-8335-99129283c266", "If Yes, CargoWise will allow to send the Export Notification (755) message to the Cargo Information Network (CIN) in France. Sending the Export Notification 755 is required to link ECS MRN numbers to your Air Waybill.\r\nYou should enable this Registry when you are NOT using the Customs Brokerage Module of CargoWise."),
						RegistryStorageFlags.Company,
						false);
					result.CountryFilterPKs = PortMessagingCountryList.France;
					return result;
				});
			}
		}
		BooleanRegistryItem IPortMessagingRegistry.AllowToSendExportNotification => AllowToSendExportNotification;

		public BooleanRegistryItem AllowToSendExportNotificationToCargonaut
		{
			get
			{
				return GetItem("AllowToSendExportNotificationToCargonaut", delegate
				{
					var cargonautString = (NoResString)"Cargonaut"; // port community system name
					var schipholString = (NoResString)"Schiphol"; // airport name
					var result = new BooleanRegistryItem(
						"AllowToSendExportNotificationToCargonaut",
						Categories.Freight_PortMessaging_Netherlands,
						ResString.GetMultilingualString("c961db71-838f-4851-898a-5878e4ab5389", @"Allow to send Export Notification (755) to {0}", cargonautString),
						ResString.GetMultilingualString("28f5d563-b7a9-494c-920b-ce0504ac25e1", @"If Yes, it will be able to send the Export Notification {0} (NL) message to the Port Community System of {1} Airport in the Netherlands.", cargonautString, schipholString),
						RegistryStorageFlags.Company,
						false);
					result.CountryFilterPKs = new[] { Core.Constants.CountryGuids.Netherlands };
					return result;
				});
			}
		}

		BooleanRegistryItem IPortMessagingRegistry.AllowToSendExportNotificationToCargonaut => AllowToSendExportNotificationToCargonaut;

		#endregion

		#region TNPA Account Number

		public TNPAAccountNumberRegistryItem TNPAAccountNumber
		{
			get
			{
				return GetItem("TNPAAccountNumber", () =>
				{
					var result = new TNPAAccountNumberRegistryItem(
						"TNPAAccountNumber",
						Categories.Freight_PortMessaging_South_Africa,
						ResString.GetMultilingualString("9c825eb9-954f-4793-a87c-8104e1585b75", "TNPA Account Number"),
						ResString.GetMultilingualString("97dbc959-bd63-43d4-a692-e10caf3f1c1d", @"This list defines TNPA Account Numbers for each port for your company. You must add the TNPA Account Numbers for each port at branch level."),
						RegistryStorageFlags.Branch,
						new TNPAAccountNumberCollection());
					result.CountryFilterPKs = new[] { Core.Constants.CountryGuids.SouthAfrica };
					return result;
				});
			}
		}

		#endregion

		#region EU HS Code Effective Date

		public DateTimeRegistryItem EORIAndLRNEffectiveDate
		{
			get
			{
				return GetItem("EORIAndLRNEffectiveDate", delegate
				{
					return new DateTimeRegistryItem(
						"EORIAndLRNEffectiveDate",
						Categories.Freight_PortMessaging_Germany_Dakosy,
						ResString.GetMultilingualString("7cd6716d-ac6f-4b59-805d-c39d95065ae9", "Enable ATLAS AES1 Release 3.0, one-stage AES1 procedure requirements"),
						ResString.GetMultilingualString("fee71f7e-d67f-4629-aee9-947d71be24a7", "ATLAS AES Release 3.0, one-stage AES procedure requirements to declare EORI and LRN Local Reference Number will be enabled from the date set below. Override to change the date."),
						RegistryStorageFlags.System,
						new DateTime(2023, 09, 06));
				});
			}
		}

		#endregion

		#region Enable Port Order FormBuilder Form

		public BooleanRegistryItem EnablePortOrderFormBuilderForm
		{
			get
			{
				return GetItem("EnablePortOrderFormBuilderForm", delegate
				{
					return new BooleanRegistryItem(
						"EnablePortOrderFormBuilderForm",
						Categories.Freight_PortMessaging_Germany_Dakosy,
						(NoResString)"Enable Port Order FormBuilder Form",
						(NoResString)"Enable this registry to use FormBuilder Form to send Dakosy Port Orders",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion
	}
}
