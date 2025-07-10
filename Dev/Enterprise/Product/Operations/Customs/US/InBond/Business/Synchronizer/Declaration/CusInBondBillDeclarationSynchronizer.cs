using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondBillDeclarationSynchronizer : BusinessObjectSynchroniser
	{
		internal CusInBondBillDeclarationSynchronizer(CusInBondBill destination, US.Business.Bill source)
			: base(destination, source)
		{
			header = destination.Header;
			declarationSource = source.Declaration;
			isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
		}
		readonly bool isAMSHBREffective;

		public new CusInBondBill Destination
		{
			get { return (CusInBondBill)base.Destination; }
		}

		public new US.Business.Bill Source
		{
			get { return (US.Business.Bill)base.Source; }
		}

		readonly CusInBondHeader header;
		readonly JobDeclaration declarationSource;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				masterBillIssuerCodeFieldSynchroniser = new FieldSynchroniser(Destination.B0_IssuerCodeInfo, GetMasterBillIssuerCode, GetMasterBillIssuerCodeRelatedInfos);
				Synchronisers.Add(masterBillIssuerCodeFieldSynchroniser);
				masterBillFieldSynchroniser = new FieldSynchroniser(Destination.B0_MasterBillNumberInfo, GetMasterBill, GetMasterBillRelatedInfos);
				Synchronisers.Add(masterBillFieldSynchroniser);
				houseBillFieldSynchroniser = new FieldSynchroniser(Destination.B0_HouseBillNumberInfo, GetHouseBill, GetHouseBillRelatedInfos);
				Synchronisers.Add(houseBillFieldSynchroniser);
				houseBillIssuerCodeFieldSynchroniser = new FieldSynchroniser(Destination.B0_HouseBillIssuerCodeInfo, GetHouseBillIssuerCode, GetHouseBillIssuerCodeRelatedInfos);
				Synchronisers.Add(houseBillIssuerCodeFieldSynchroniser);
				manifestQtyFieldSynchroniser = new FieldSynchroniser(Destination.B0_ManifestQtyInfo, GetManifestQty, GetManifestQtyRelatedInfos);
				Synchronisers.Add(manifestQtyFieldSynchroniser);
				manifestUQFieldSynchroniser = new FieldSynchroniser(Destination.B0_ManifestUQInfo, GetManifestUQ, GetManifestUQRelatedInfos);
				Synchronisers.Add(manifestUQFieldSynchroniser);
				Synchronisers.Add(new FieldSynchroniser(Destination.B0_WeightInfo, Source.US_WeightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.B0_WeightUQInfo, Source.US_WeightUQInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.B0_VolumeInfo, Source.US_VolumeInfo, true));
				Synchronisers.Add(new FieldSynchroniser(Destination.B0_VolumeUQInfo, GetVolumeUQ, GetVolumeUQRelatedInfos, true));

				Synchronisers.Add(new JobDocAddressSynchroniser(Destination.ForeignShipper, declarationSource.SupplierDocumentaryAddress));
				Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignee, declarationSource.ImporterDocumentaryAddress));
				Synchronisers.Add(new JobDocAddressSynchroniser(Destination.NotifyParty, declarationSource.NotifyPartyDocumentaryAddress));

				var movementHeader = header.MovementHeader;
				CusInBondMoveDetail moveDetail = null;
				foreach (CusInBondMoveDetail existingMoveDetail in movementHeader.MovementDetails.ToArray())
				{
					CusInBondMoveDetail moveDetailThatShouldBeDeleted = null;
					if (existingMoveDetail.B9_B0 == Destination.PK)
					{
						if (moveDetail == null)
						{
							moveDetail = existingMoveDetail;
						}
						else
						{
							if (moveDetail.B9_SeqNo > existingMoveDetail.B9_SeqNo)
							{
								moveDetailThatShouldBeDeleted = moveDetail;
								moveDetail = existingMoveDetail;
							}
							else
							{
								moveDetailThatShouldBeDeleted = existingMoveDetail;
							}
						}
						if (moveDetailThatShouldBeDeleted != null && !moveDetailThatShouldBeDeleted.ActiveInMessaging)
						{
							moveDetailThatShouldBeDeleted.Delete();
						}
					}
				}
				Synchronisers.Add(new CusInBondMoveDetailDeclarationSynchronizer(moveDetail ?? movementHeader.MovementDetails.AddNew(), Source, Destination));

				declarationSource.Bills.CountChanged -= Bills_CountChanged;
				declarationSource.Bills.CountChanged += Bills_CountChanged;

				Source.CU_CU_ParentBillInfo.ValueChanged -= CU_CU_ParentBill_ValueChanged;
				Source.CU_CU_ParentBillInfo.ValueChanged += CU_CU_ParentBill_ValueChanged;
			}
		}
		FieldSynchroniser manifestQtyFieldSynchroniser;
		FieldSynchroniser manifestUQFieldSynchroniser;
		FieldSynchroniser masterBillIssuerCodeFieldSynchroniser;
		FieldSynchroniser masterBillFieldSynchroniser;
		FieldSynchroniser houseBillIssuerCodeFieldSynchroniser;
		FieldSynchroniser houseBillFieldSynchroniser;

		protected override void UnHookSynchronisers()
		{
			declarationSource.Bills.CountChanged -= Bills_CountChanged;
			Source.CU_CU_ParentBillInfo.ValueChanged -= CU_CU_ParentBill_ValueChanged;
			base.UnHookSynchronisers();
		}

		void Bills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(manifestQtyFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(manifestUQFieldSynchroniser);
		}

		void CU_CU_ParentBill_ValueChanged(object sender, EventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(masterBillIssuerCodeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(masterBillFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(houseBillIssuerCodeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(houseBillFieldSynchroniser);
		}

		#region B0_HouseBillIssuerCode

		IZType GetHouseBillIssuerCode()
		{
			var result = ZString.Empty;
			if (isAMSHBREffective && declarationSource.IsSea && Source.IsHouseBill)
			{
				result = Source.US_UI_NKBillIssuerSCAC;
			}
			return result.KeepAlphanumericCharacters();
		}

		IEnumerable<ZPropertyInfo> GetHouseBillIssuerCodeRelatedInfos()
		{
			if (isAMSHBREffective)
			{
				yield return declarationSource.JE_TransportModeInfo;
				yield return Source.CU_BillTypeInfo;
				yield return Source.US_UI_NKBillIssuerSCACInfo;
			}
		}

		#endregion

		#region B0_HouseBillNumber

		IZType GetHouseBill()
		{
			var result = ZString.Empty;
			if (Source.IsHouseBill)
			{
				result = Source.CU_BillNum;
			}
			return result.KeepAlphanumericCharacters();
		}

		IEnumerable<ZPropertyInfo> GetHouseBillRelatedInfos()
		{
			yield return Source.CU_BillTypeInfo;
			yield return Source.CU_BillNumInfo;
		}

		#endregion

		#region B0_IssuerCode

		IZType GetMasterBillIssuerCode()
		{
			var result = ZString.Empty;
			if (Source.IsHouseBill)
			{
				var parentBill = Source.ParentBill;
				if (parentBill != null && parentBill.IsMasterBill)
				{
					result = parentBill.US_UI_NKBillIssuerSCAC;
				}
			}
			else
			{
				result = Source.US_UI_NKBillIssuerSCAC;
			}
			return result.KeepAlphanumericCharacters();
		}

		IEnumerable<ZPropertyInfo> GetMasterBillIssuerCodeRelatedInfos()
		{
			yield return Source.US_UI_NKBillIssuerSCACInfo;
			var parentBill = Source.ParentBill;
			if (parentBill != null)
			{
				yield return parentBill.US_UI_NKBillIssuerSCACInfo;
			}
			yield return Source.US_UI_NKBillIssuerSCACInfo;
		}

		#endregion

		#region B0_MasterBillNumber

		IZType GetMasterBill()
		{
			var result = ZString.Empty;
			if (Source.IsHouseBill)
			{
				var parentBill = Source.ParentBill;
				if (parentBill != null && parentBill.IsMasterBill)
				{
					result = parentBill.CU_BillNum;
				}
			}
			else
			{
				result = Source.CU_BillNum;
			}
			return result.KeepAlphanumericCharacters();
		}

		IEnumerable<ZPropertyInfo> GetMasterBillRelatedInfos()
		{
			yield return Source.CU_BillTypeInfo;
			var parentBill = Source.ParentBill;
			if (parentBill != null)
			{
				yield return parentBill.CU_BillNumInfo;
			}
			yield return Source.CU_BillNumInfo;
		}

		#endregion

		#region B0_ManifestQty

		IZType GetManifestQty()
		{
			if (declarationSource.Bills.FindByBillType(BillTypeList.Codes.MasterBill).Length == 1)
			{
				return declarationSource.JE_TotalNoOfPacks;
			}
			else
			{
				return Source.CU_NoOfPacks.ToZInt();
			}
		}

		IEnumerable<ZPropertyInfo> GetManifestQtyRelatedInfos()
		{
			yield return Source.CU_NoOfPacksInfo;
			yield return declarationSource.JE_TotalNoOfPacksInfo;
			foreach (var bill in declarationSource.Bills.OfType<US.Business.Bill>())
			{
				yield return bill.CU_BillTypeInfo;
			}
		}

		#endregion

		#region B0_ManifestUQ

		IZType GetManifestUQ()
		{
			if (declarationSource.Bills.FindByBillType(BillTypeList.Codes.MasterBill).Length == 1)
			{
				return declarationSource.JE_TotalNoOfPacksPackType;
			}
			else
			{
				return Source.CU_PackType;
			}
		}

		IEnumerable<ZPropertyInfo> GetManifestUQRelatedInfos()
		{
			yield return Source.CU_PackTypeInfo;
			yield return declarationSource.JE_TotalNoOfPacksPackTypeInfo;
			foreach (var bill in declarationSource.Bills.OfType<US.Business.Bill>())
			{
				yield return bill.CU_BillTypeInfo;
			}
		}

		#endregion

		#region B0_VolumeUQ

		IZType GetVolumeUQ()
		{
			return (ZString)VolumeUnitList.ConvertFromStandardCode(Source.US_VolumeUQ);
		}

		IEnumerable<ZPropertyInfo> GetVolumeUQRelatedInfos()
		{
			yield return Source.US_VolumeUQInfo;
		}

		#endregion
	}
}
