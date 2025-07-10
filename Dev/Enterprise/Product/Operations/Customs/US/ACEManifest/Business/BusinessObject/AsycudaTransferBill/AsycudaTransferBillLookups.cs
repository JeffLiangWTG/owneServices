using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferBillLookups : ASYCUDA.Business.AsycudaTransferBillLookups
	{
		public AsycudaTransferBillLookups(AsycudaTransferBill parent) : base(parent)
		{
		}

		new AsycudaTransferBill Parent => (AsycudaTransferBill)base.Parent;

		public CodeDescriptionPairList ManifestBillsList
		{
			get
			{
				var manifestHeader = Parent.TransferHeader?.ArrivalHeader?.ManifestHeader;
				if (manifestHeader != null)
				{
					return Factory.GetCachedValue($"AsycudaTransferBillLookups|ManifestBills_{manifestHeader.AMA_MasterBill}", () =>
					{
						var list = new CodeDescriptionPairList();

						var masterbill = manifestHeader.MasterBill;
						list.Add(new CodeElement(masterbill.PK, masterbill.ABL_BillNumber, ZString.Empty));

						foreach (AsycudaBill bill in manifestHeader.Bills)
						{
							list.Add(new CodeElement(bill.PK, bill.ABL_BillNumber, bill.ABL_GoodsDescription));
						}

						return list;
					}
					, CacheStalenessPolicy.StaleWhenDataTableChanges(AsycudaBill.Schema.TableName, Factory));
				}

				return new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList TransferMessageStatusList => Factory.GetCachedValue<AIMTransferStatusCodes>();
	}
}
