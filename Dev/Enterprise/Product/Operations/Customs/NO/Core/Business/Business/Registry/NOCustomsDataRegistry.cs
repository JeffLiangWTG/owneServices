using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.NO;
using ResString = Enterprise.Customs.NO.Business.ResString;

namespace Enterprise.Customs.NO.Registry;

public sealed class NOCustomsDataRegistry : RegistryItemSet, INOCustomsRegistry, INOTemporaryStorageRegistry
{
	public static NOCustomsDataRegistry Instance => instance ??= new NOCustomsDataRegistry();

	[ThreadStatic]
	static NOCustomsDataRegistry instance;

	NOCustomsDataRegistry()
	{
	}

	#region Categories

	public abstract class Categories : RawDataRegistry.Categories
	{
		public static MultilingualString Customs_Norway_Manifest => CombineCategories(Customs_Norway, ResString.GetMultilingualString("9D8A2C8A-D9BC-4EE8-B190-EA2BFD01F2EA", "Manifest"));

		public static MultilingualString Customs_Norway_TemporaryStorage => CombineCategories(Customs_Norway, ResString.GetMultilingualString("9B4BAF46-EBA9-4A67-B85A-D04B13815B08", "Temporary Storage"));

		public static MultilingualString Customs_Norway_Import => CombineCategories(Customs_Norway, ResString.GetMultilingualString("130F1AB5-EF97-4F65-AF0E-BD840A856AB6", "Import"));

		public static MultilingualString Customs_Norway_Export => CombineCategories(Customs_Norway, ResString.GetMultilingualString("23F69D89-02B4-A1B3-452C-DFAE58619054", "Export"));

		public static MultilingualString Customs_Norway_FTPSettings => CombineCategories(Customs_Norway, ResString.GetMultilingualString("823E6FCE-FB06-4167-831B-123F85AE4A9D", "FTP Settings"));
	}

	#endregion

	public NodiRegistryItem CustomsNodiId => GetItem("CustomsNodiId", () => new NodiRegistryItem(
		"CustomsNodiId",
		CustomsDataRegistry.Categories.Customs_Norway,
		ResString.GetMultilingualString("16209A1E-C0D0-4462-AA64-FA2408B65BFA", "Customs NODI ID"),
		ResString.GetMultilingualString("E9B36604-EEF8-4B47-8F1A-656C84552BCE", "Customs Mailbox ID used for submitting messages."),
		RegistryStorageFlags.System,
		new NodiRegistryCollection().DefaultCollection));

	public BooleanRegistryItem EnableTestMessages => GetItem("EnableTestMessages", () => new BooleanRegistryItem(
		"EnableTestMessages",
		CustomsDataRegistry.Categories.Customs_Norway,
		ResString.GetMultilingualString("D960E514-4252-44A1-AFA2-089C785B1F58", "Test Environment"),
		ResString.GetMultilingualString("BF0B6DEE-A911-46CF-9BED-ACFE8C558A4C", "Enable test messages to Norwegian customs?"),
		RegistryStorageFlags.Company,
		IsProductionSystem() ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
		false));

	public ManifestGroupNotificationRegistryItem NoMANGroupNotification => GetItem("NOMANGroupNotification",
		() => new ManifestGroupNotificationRegistryItem(
			"NOMANGroupNotification",
			Categories.Customs_Norway_Manifest,
			ResString.GetMultilingualString("D6477D52-622B-4899-A13D-AB73B77969EF", "Notification Group"),
			ResString.GetMultilingualString("BE4ABF91-924D-4628-8E66-E497BFC4A1F5",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
			RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
			RegistryOptions.Default,
			ManifestGroupNotification.Default));

	public BooleanRegistryItem EnableNOManifests => GetItem("EnableNOManifests",
		() => new BooleanRegistryItem(
			"EnableNOManifests",
			Categories.Customs_Norway_Manifest,
			ResString.GetMultilingualString("EB9E8272-33B2-4D45-9410-D5B8C0EDE3B9", "Enable Norway Manifest"),
			ResString.GetMultilingualString("6BC6E9FD-6AB3-4DD0-80DA-0AFA439E6091", "Enable Norway Manifest?"),
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
			false));

	public GroupNotificationRegistryItem<GroupNotification> SendImportErrorsTo => GetItem("NOSendImportErrorsTo",
		() => new GroupNotificationRegistryItem<GroupNotification>(
			"NOSendImportErrorsTo",
			Categories.Customs_Norway_Import,
			ResString.GetMultilingualString("3E07A7E7-CF62-4352-99C1-4A16EAE8D2FC", "Send Import Errors To"),
			ResString.GetMultilingualString("57F85C71-D58C-47F1-B7FA-C50BE18DBBB2", "Send Import errors to staff member, nominated group or both"),
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)));

	public GroupNotificationRegistryItem<GroupNotification> SendImportAcknowledgementsTo => GetItem("NOSendImportAcknowledgementsTo",
		() => new GroupNotificationRegistryItem<GroupNotification>(
			"NOSendImportAcknowledgementsTo",
			Categories.Customs_Norway_Import,
			ResString.GetMultilingualString("1799DD89-7C91-44D5-84F2-3B92F0B826F8", "Send Import Acknowledgements To"),
			ResString.GetMultilingualString("B79CA7AA-FA55-4D93-998E-88ADDA660E4E", "Send Import acknowledgements to staff member, nominated group or both"),
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)));

	public GroupNotificationRegistryItem<GroupNotification> SendExportErrorsTo => GetItem("NOSendExportErrorsTo",
		() => new GroupNotificationRegistryItem<GroupNotification>(
			"NOSendExportErrorsTo",
			Categories.Customs_Norway_Export,
			ResString.GetMultilingualString("5C8CF175-599F-13A3-4FCC-5E9E1073A1EF", "Send Export Errors To"),
			ResString.GetMultilingualString("D810AF71-330F-A588-48B0-8DC4B99A56A0", "Send Export errors to staff member, nominated group or both"),
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.IsOnlyForController,
			new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)));

	public GroupNotificationRegistryItem<GroupNotification> SendExportAcknowledgementsTo => GetItem("NOSendExportAcknowledgementsTo",
		() => new GroupNotificationRegistryItem<GroupNotification>(
			"NOSendExportAcknowledgementsTo",
			Categories.Customs_Norway_Export,
			ResString.GetMultilingualString("82716AA6-5451-3089-4A1D-FC836B518BF2", "Send Export Acknowledgements To"),
			ResString.GetMultilingualString("05FC5F5F-B41A-26B6-4A01-62928C2281CC", "Send Export acknowledgements to staff member, nominated group or both"),
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.IsOnlyForController,
			new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)));

	IRegistryItem INOCustomsRegistry.EnableNOManifests => EnableNOManifests;

	public override bool IsForProductivityWise => false;

	public BooleanRegistryItem EnableTemporaryStorageRegister => GetItem("EnableTemporaryStorageRegister",
		() => new BooleanRegistryItem(
			"EnableTemporaryStorageRegister",
			Categories.Customs_Norway_TemporaryStorage,
			ResString.GetMultilingualString("78B4CD7C-D261-4F1F-9A52-EEF08650F4EA", "Enable Register"),
			ResString.GetMultilingualString("C8123589-9883-49DD-9CB0-194DC8EA6932", "Set to YES to Enable Temporary Storage - Register"),
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false));

	bool INOTemporaryStorageRegistry.IsTemporaryStorageRegisterEnabled => EnableTemporaryStorageRegister.Value;

	public FTPSettingsCustomsRegistryItem FTPSettingsCustoms => GetItem("NOFTPSettingsCustoms",
		() => new FTPSettingsCustomsRegistryItem(
			"NOFTPSettingsCustoms",
			Categories.Customs_Norway_FTPSettings,
			ResString.GetMultilingualString("DFCD375E-D685-4BD1-9B3A-2A88BD16B006", "Customs"),
			ResString.GetMultilingualString("ED40B3B6-6146-4794-A606-741B0F90FD17", "The FTP server settings for sending and receiving declaration entry messages to and from Norwegian customs."),
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForController,
			FTPSettingsCustomsRegistry.DefaultValues)
		{
			CountryFilterPKs = CountryFilterPKs.Norway,
			OnUpdateAction = RegistryActionHandler.UpdateFtpCustomsSettings,
			OnAllValuesSavedAction = RegistryActionHandler.Save
		});

	public FTPSettingsEMMADocRegistryItem FTPSettingsEMMADoc => GetItem("NOFTPSettingsEMMADoc",
		() => new FTPSettingsEMMADocRegistryItem(
			"NOFTPSettingsEMMADoc",
			Categories.Customs_Norway_FTPSettings,
			ResString.GetMultilingualString("E04922A0-060A-4CA3-BEDD-CD0340A5A312", "EMMA Doc"),
			ResString.GetMultilingualString("107D49EE-D8CE-4713-8826-D42D41C19DE5", "The FTP server settings for sending declaration entry and attachments to EMMA Doc."),
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForController,
			FTPSettingsEMMADocRegistry.DefaultValues)
		{
			CountryFilterPKs = CountryFilterPKs.Norway,
		});

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Norway;
	}

	ZBool IsProductionSystem() => ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production;

	IFTPSettingsRegistryActionHandler RegistryActionHandler => registryActionHandler ??= new FTPSettingsRegistryActionHandler();

	IFTPSettingsRegistryActionHandler registryActionHandler;
}
