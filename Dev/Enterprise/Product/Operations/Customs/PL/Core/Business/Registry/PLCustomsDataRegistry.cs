using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

public sealed class PLCustomsDataRegistry : RegistryItemSet
{
	#region Construction

	public static PLCustomsDataRegistry Instance => instance ?? (instance = new PLCustomsDataRegistry());

	[ThreadStatic]
	static PLCustomsDataRegistry instance;

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Poland;
	}

	public override bool IsForProductivityWise => false;

	#endregion

	public class Categories : CustomsDataRegistry.Categories
	{
		static readonly MultilingualString EmailChannel = ResString.GetMultilingualString("6EF79B94-560B-4F2F-821C-5CC769C06350", "Email Channel");
		public static MultilingualString Customs_PL_PUESC => CombineCategories(Customs_Poland, ResString.GetMultilingualString("F23D2E5A-7AC9-4AA6-99E9-CFBFCB678257", "PUESC"));
		public static MultilingualString Customs_PL_PUESC_WebService => CombineCategories(Customs_PL_PUESC, ResString.GetMultilingualString("9930590E-6135-41B1-8060-F942D8761EE6", "Web Service"));
		public static MultilingualString Customs_PL_PUESC_EmailChannel => CombineCategories(Customs_PL_PUESC, EmailChannel);
		public static MultilingualString Customs_PL_PCS_EmailChannel => CombineCategories(Customs_Poland,
			ResString.GetMultilingualString("D4C45D49-39BF-470B-ACAE-8FCC70CA9815", "PCS"),
			EmailChannel);
		public static MultilingualString Customs_PL_EAttachments_EmailChannel => CombineCategories(Customs_Poland,
			ResString.GetMultilingualString("FE5B5B0A-3E77-4D1B-B0FE-EB4EAA9E4D08", "E-Attachments"),
			EmailChannel);
		public static MultilingualString Customs_PL_CommunicationEmailChannel => CombineCategories(Customs_Poland,
			ResString.GetMultilingualString("A6FE741E-17F9-4500-BD33-59FEFDF041BA", "Communication Email Channel"));
		public static MultilingualString Customs_PL_DefaultCommunicationChannel => CombineCategories(Customs_Poland,
			ResString.GetMultilingualString("A0E2D038-D0C2-45C7-92C5-D89E76CF4BB3", "Default Communication Channel"));
	}

	public IntRegistryItem PUESCSendMaxRetryCount => GetItem("PUESCWebServiceRequestMaxRetryCount", () => new IntRegistryItem(
		"PUESCWebServiceRequestMaxRetryCount",
		Categories.Customs_PL_PUESC_WebService,
		ResString.GetMultilingualString("CCAFB9D9-5866-49B9-A2B4-1EAA15C00204", "PUESC request max retry count"),
		ResString.GetMultilingualString("E9440CD6-8C53-4336-BC47-772C2C168F32", "This is the maximum attempts that will be made to communicate with PUESC Web Service to send messages"),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		PLRegistryDataConstants.PUESCSendMaxRetryCount
	));

	public IntRegistryItem PUESCRequestTimeOut => GetItem("PUESCWebServiceRequestTimeOut", () => new IntRegistryItem(
		"PUESCWebServiceRequestTimeOut",
		Categories.Customs_PL_PUESC_WebService,
		ResString.GetMultilingualString("6BE577AE-C238-4697-B853-4AA7B84F1E96", "PCT Service timeout"),
		ResString.GetMultilingualString("8D684963-0EE0-4BDC-8FA5-0E87C68872BF", "Automatic retry time for documents retrieval request (minutes)"),
		RegistryStorageFlags.System,
		RegistryOptions.Default,
		PLRegistryDataConstants.PUESCWebServiceRequestTimeOut
	));

	public StringRegistryItem PUESCEmailChannel => GetItem("PUESCEmailChannel", () => new StringRegistryItem(
		"PUESCEmailChannel",
		Categories.Customs_PL_PUESC_EmailChannel,
		ResString.GetMultilingualString("E5F68804-37C9-4799-A157-4B9E9EEECABF", "PUESC Email Address"),
		ResString.GetMultilingualString("B4CA51C6-C422-4114-AE41-A1D61DA3F4E3", "This is the Default Email Address for PUESC Email Channel"),
		RegistryStorageFlags.Company,
		RegistryOptions.Default,
		IsProductionSystem ? PLRegistryDataConstants.PUESCEmailAddressProd : PLRegistryDataConstants.PUESCEmailAddressTest
	));

	public StringRegistryItem PCSEmailChannel => GetItem("PCSEmailChannel", () => new StringRegistryItem(
		"PCSEmailChannel",
		Categories.Customs_PL_PCS_EmailChannel,
		ResString.GetMultilingualString("E78B3A31-EC0C-457A-9F8B-D94A66B67821", "PCS Email Address"),
		ResString.GetMultilingualString("C06D95B4-912B-43BF-B01D-E0156BABD85C", "This is the Email Address for PCS Email Channel"),
		RegistryStorageFlags.Company,
		RegistryOptions.Default,
		IsProductionSystem ? PLRegistryDataConstants.PCSEmailAddressProd : string.Empty
	));

	public StringRegistryItem EAttachmentsEmailChannel => GetItem("EAttachmentsEmailChannel", () => new StringRegistryItem(
		"EAttachmentsEmailChannel",
		Categories.Customs_PL_EAttachments_EmailChannel,
		ResString.GetMultilingualString("EBF18354-41E7-4F5B-BDC0-F03C7EA3BBCA", "E-Attachments Email Address"),
		ResString.GetMultilingualString("F153EDED-29A7-4FC6-AD8B-8A016A48B45E", "This is the Email Address for E-Attachments email channel"),
		RegistryStorageFlags.Company,
		RegistryOptions.Default,
		IsProductionSystem ? PLRegistryDataConstants.EAttachmentsEmailAddressProd : string.Empty
	));

	public StringRegistryItem CommunicationEmailChannelEmailAddress => GetItem("CommunicationEmailChannelEmailAddress", () => new StringRegistryItem(
		"CommunicationEmailChannelEmailAddress",
		Categories.Customs_PL_CommunicationEmailChannel,
		ResString.GetMultilingualString("F1C1E88E-1135-4B8B-8D2C-E611AB02A8B3", "Email Address"),
		ResString.GetMultilingualString("D90F4738-C07E-40C3-89DB-E1997C0903B4", "This is the Email Address for Communication email channel"),
		RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
		RegistryOptions.IsValueMandatory
	));

	public PLDefaultCommunicationChannelNCTSP5RegistryItem DefaultCommunicationChannelNCTSP5 => GetItem("DefaultCommunicationChannelNCTSP5", () => new PLDefaultCommunicationChannelNCTSP5RegistryItem(
		"DefaultCommunicationChannelNCTSP5",
		Categories.Customs_PL_DefaultCommunicationChannel,
		ResString.GetMultilingualString("60DD2B0A-C092-45AC-90F6-8503548E33B1", "NCTS P5"),
		ResString.GetMultilingualString("NPBO:Enterprise.Customs.PL.Business.Registry.DefaultCommunicationChannelNCTSP5.PLDefaultCommunicationChannelNCTSP5|IsSeapIDHint", "Enable this option to default the Communication Channel SEAP ID or Email Channel"),
		RegistryStorageFlags.Company,
		RegistryOptions.Default,
		new PLDefaultCommunicationChannelNCTSP5()
	));

	bool IsProductionSystem => CachedValueHelper.GetValue(ref isProductionSystem, () =>
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
		return registrationKey.DatabaseType == DatabaseTypes.Codes.Production;
	});
	CachedValue<bool> isProductionSystem;
}
