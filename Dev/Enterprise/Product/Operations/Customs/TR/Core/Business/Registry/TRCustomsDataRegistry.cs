using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	public sealed class TRCustomsDataRegistry : RegistryItemSet, Integration.Customs.TR.ITRCustomsDataRegistry
	{
		#region Construction

		public static TRCustomsDataRegistry Instance => instance ?? (instance = new TRCustomsDataRegistry());

		[ThreadStatic]
		static TRCustomsDataRegistry instance;

		TRCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Turkey { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("CA28327E-6C94-4A83-B4A5-7DF60F3F0F90", "Turkey")); } }
			public static MultilingualString Customs_Turkey_Manifest { get { return CombineCategories(Customs_Turkey, ResString.GetMultilingualString("D99B31C1-CFFA-487A-8D59-5FFB96166D0D", "Manifest")); } }
			public static MultilingualString Customs_Turkey_NCTS { get { return CombineCategories(Customs_Turkey, ResString.GetMultilingualString("5F617889-F728-42DF-8907-91C9580CF345", "NCTS")); } }
			public static MultilingualString Customs_Turkey_Declaration { get { return CombineCategories(Customs_Turkey, ResString.GetMultilingualString("4E69C464-3A3D-4679-85CF-391C6575FD35", "Declaration")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem TRTestingSystem
		{
			get
			{
				return GetItem("IsTRTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsTRTesting",
						Categories.Customs_Turkey,
						ResString.GetMultilingualString("16FAB7AD-1E8D-45B5-AF0E-F2D5E220D8BD", "Is TR Testing System?"),
						ResString.GetMultilingualString("6F8C1551-42F7-4F2A-9B4B-E9BCC5FF3FE2", "TR messages be sent to the test rather than production system?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public ZBool IsTRTestingSystem
		{
			get { return Instance.TRTestingSystem.Value; }
		}

		public BooleanRegistryItem ExposeETradeModule
		{
			get
			{
				return GetItem("ExposeETradeModule", delegate
				{
					return new BooleanRegistryItem(
						"ExposeETradeModule",
						Categories.Customs_Turkey,
						ResString.GetMultilingualString("0A2B24FC-E172-4FF4-A393-11EC89E29CED", "Expose E Trade Module?"),
						ResString.GetMultilingualString("AA7BFCA7-B1E0-4E5D-8470-A1A85349A67F", "Expose E Trade Module?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem ExposeSimplifiedProcedureTransitSystemModule
		{
			get
			{
				return GetItem("ExposeSimplifiedProcedureTransitSystemModule", delegate
				{
					return new BooleanRegistryItem(
						"ExposeSimplifiedProcedureTransitSystemModule",
						Categories.Customs_Turkey,
						ResString.GetMultilingualString("4CF2693B-C064-495C-BE3F-18520BE1E982", "Expose Simplified Procedure Transit System Module?"),
						ResString.GetMultilingualString("27D47919-C492-47C6-A599-1AA8BCBCA0A4", "Expose Simplified Procedure Transit System Module?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		IRegistryItem Integration.Customs.TR.ITRCustomsDataRegistry.ExposeETradeModule
		{
			get { return ExposeETradeModule; }
		}

		IRegistryItem Integration.Customs.TR.ITRCustomsDataRegistry.ExposeSimplifiedProcedureTransitSystemModule
		{
			get { return ExposeSimplifiedProcedureTransitSystemModule; }
		}

		public StampDutyLedgerNumberCustomizationRegistryItem StampDutyLedgerNumberCustomization
		{
			get
			{
				return GetItem("TRStampDutyLedgerNumberCustomization", delegate
				{
					return new StampDutyLedgerNumberCustomizationRegistryItem(
						"TRStampDutyLedgerNumberCustomization",
						Categories.Customs_Turkey,
						ResString.GetMultilingualString("6F53D7D9-242B-4523-9439-C467931CE342", "Stamp Duty Ledger Number Customization"),
						ResString.GetMultilingualString("C26FBDE8-8771-437C-A810-5DA85F5AAA37", "Override this value to customize how Stamp Duty Ledger numbers are formatted"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new StampDutyLedgerNumberCustomizationRegistrySetting())
					{
						CountryFilterPKs = CountryFilterPKs.Turkey
					};
				});
			}
		}

		public ManifestGroupNotificationRegistryItem TRMANGroupNotification
		{
			get
			{
				return GetItem("TRMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"TRMANGroupNotification",
						Categories.Customs_Turkey_Manifest,
						ResString.GetMultilingualString("8BA2BF28-1D83-4C9A-B1CF-576E46311326", "Notification Group"),
						ResString.GetMultilingualString("3BB3D267-9039-4282-9BB5-8C62F47845FB", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					result.CountryFilterPKs = CountryFilterPKs.Turkey;
					return result;
				}
				);
			}
		}

		public BooleanRegistryItem EnableTRNCTS
		{
			get
			{
				return GetItem("EnableTRNCTS", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableTRNCTS",
						Categories.Customs_Turkey_NCTS,
						ResString.GetMultilingualString("F1B3DF42-D8BD-4D7B-88CF-EB9A92D9A772", "Enable Turkey NCTS"),
						ResString.GetMultilingualString("5ADDE855-957E-458B-8249-C916094F7808", "Enable Turkey NCTS?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.TR.ITRCustomsDataRegistry.EnableTRNCTS
		{
			get { return EnableTRNCTS; }
		}

		public BooleanRegistryItem SendTRNCTSMessageWithoutSign
		{
			get
			{
				return GetItem("SendTRNCTSMessageWithoutSign", delegate
				{
					var result = new BooleanRegistryItem(
						"SendTRNCTSMessageWithoutSign",
						Categories.Customs_Turkey_NCTS,
						ResString.GetMultilingualString("969FB94F-8787-4F33-9E8A-58AFA2A61725", "Send NCTS Message Without E-signature"),
						ResString.GetMultilingualString("9F3E1EB7-4073-4993-99AC-C58FB27B4667", "Send NCTS Message Without E-signature?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.TR.ITRCustomsDataRegistry.SendTRNCTSMessageWithoutSign
		{
			get { return SendTRNCTSMessageWithoutSign; }
		}

		public ZBool IsSendTRNCTSMessageWithoutSign
		{
			get { return Instance.SendTRNCTSMessageWithoutSign.Value; }
		}

		public FTPSettingsRegistryItem FTPSettings => GetItem("ExportUnionFTPSettings", () =>
		{
			return new FTPSettingsRegistryItem
			(
				"ExportUnionFTPSettings",
				Categories.Customs_Turkey,
				ResString.GetMultilingualString("CD0DA80A-AAC3-4447-8427-382CD1B530EF", "Export Union FTP Settings"),
				ResString.GetMultilingualString("AA510D39-6EB6-446D-B0A8-D4A4EED4D5D1", "The registry item stores the FTP settings for sending messages to the Export Union."),
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport
			)
			{
				CountryFilterPKs = CountryFilterPKs.Turkey,
				OnUpdateAction = RegistryActionHandler.OnUpdateAction,
				OnAllValuesSavedAction = RegistryActionHandler.OnAllValuesSaved
			};
		});

		IRegistryItem Integration.Customs.TR.ITRCustomsDataRegistry.SendTRDeclarationWithComma
		{
			get { return SendTRDeclarationWithComma; }
		}

		public BooleanRegistryItem SendTRDeclarationWithComma
		{
			get
			{
				return GetItem("SendTRDeclarationWithComma", delegate
				{
					var result = new BooleanRegistryItem(
						"SendTRDeclarationWithComma",
						Categories.Customs_Turkey_Declaration,
						ResString.GetMultilingualString("82FE0D5E-E58B-4D54-85B1-2A30577E9A44", "Use comma in Ratio field in Declaration message"),
						ResString.GetMultilingualString("D523F545-2950-42FB-91AB-49738FB61A36", "Use comma in Ratio field in Declaration message?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		public NCTSPhase5CredentialsRegistryItem NCTSPhase5Credentials
		{
			get
			{
				return GetItem("NCTSPhase5Credentials", delegate
				{
					return new NCTSPhase5CredentialsRegistryItem(
						"NCTSPhase5Credentials",
						Categories.Customs_Turkey_NCTS,
						ResString.GetMultilingualString("63CA02E4-8A38-4B39-A8DA-0ED372C29D1F", "NCTS Phase 5 Credentials"),
						ResString.GetMultilingualString("789BCBE2-E23F-4F97-B6B0-867F83907CCF", "NCTS Phase 5 Credentials"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public ITRCustomsDataRegistryActionHandler RegistryActionHandler => registryActionHandler ??= new TRCustomsDataRegistryActionHandler();
		ITRCustomsDataRegistryActionHandler registryActionHandler;
	}
}
