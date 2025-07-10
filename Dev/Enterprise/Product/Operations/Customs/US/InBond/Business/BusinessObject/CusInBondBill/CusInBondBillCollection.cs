using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondBillCollection : Customs.Business.CusInBondBillCollection<CusInBondBill>
	{
		public CusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}

		public CodeDescriptionPairList MasterBillsAndHouseBillsList
		{
			get
			{
				if (masterBillsAndHouseBillsCached == null)
				{
					masterBillsAndHouseBillsCached = new CachedProperty<CodeDescriptionPairList>(Factory, () =>
						{
							var result = new CodeDescriptionPairList();
							foreach (CusInBondBill bill in this)
							{
								result.AddPair(bill.PK, bill.BillUniqueCode, bill.BillUniqueCode);
							}
							return result;
						});
				}
				return masterBillsAndHouseBillsCached.Value;
			}
		}
		CachedProperty<CodeDescriptionPairList> masterBillsAndHouseBillsCached;

		protected override void DefaultFromPreviousBill(CusInBondBill newBill, CusInBondBill previousBill)
		{
			if (!previousBill.IsDeleted)
			{
				newBill.B0_IssuerCode = previousBill.B0_IssuerCode;
				newBill.B0_ManifestUQ = previousBill.B0_ManifestUQ;
				newBill.B0_WeightUQ = previousBill.B0_WeightUQ;
				newBill.B0_PortOfLadingKCode = previousBill.B0_PortOfLadingKCode;
				newBill.B0_VolumeUQ = previousBill.B0_VolumeUQ;
				newBill.B0_PlaceOfReceiptDCode = previousBill.B0_PlaceOfReceiptDCode;
				var args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentID });
				newBill.ForeignShipper.CopyPersistentValuesFrom(previousBill.ForeignShipper, args);
				newBill.ForeignShipper.HasChanges = !newBill.ForeignShipper.IsEmpty;
				newBill.Consignee.CopyPersistentValuesFrom(previousBill.Consignee, args);
				newBill.Consignee.HasChanges = !newBill.Consignee.IsEmpty;
				newBill.NotifyParty.CopyPersistentValuesFrom(previousBill.NotifyParty, args);
				newBill.NotifyParty.HasChanges = !newBill.NotifyParty.IsEmpty;
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var master = (CusInBondHeader)Relationship.Master;
				return master != null && !master.ShouldSynchronise;
			}
		}

		protected override void SetDefaultsForNewElementCore(CusInBondBill newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.SetIssuerCodeAndFTZForeignPortOfLadingIfRequired();
		}
	}
}
