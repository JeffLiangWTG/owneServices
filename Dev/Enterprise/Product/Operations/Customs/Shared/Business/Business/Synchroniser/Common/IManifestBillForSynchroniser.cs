using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.Business
{
	public interface IManifestBillForSynchroniser
	{
		IManifestBillAddress Consignee { get; }
		IManifestBillAddress ForeignShipper { get; }
		IManifestBillAddress NotifyParty1 { get; }
		bool IsBillAlreadyOnFile { get; }
		ZPropertyInfo MasterBillNumberInfo { get; }
		ZPropertyInfo PortOfLadingInfo { get; }
		ZPropertyInfo PlaceOfReceiptInfo { get; }
		ZPropertyInfo LastForeignPortInfo { get; }
		ZPropertyInfo WeightInfo { get; }
		ZPropertyInfo WeightUQInfo { get; }
		ZPropertyInfo VolumeInfo { get; }
		ZPropertyInfo VolumeUQInfo { get; }
		ZPropertyInfo ManifestQtyInfo { get; }
		ZPropertyInfo ManifestUQInfo { get; }
		bool IsDeleted { get; }
		void Delete();
		IManifestHeaderForSynchroniser Header { get; }
	}
}
