using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailLookups : US.Business.CusInBondMoveDetailLookups
	{
		public CusInBondMoveDetailLookups(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MasterBillsAndHouseBills
		{
			get
			{
				var header = Parent.Header;
				return header != null ? header.Bills.MasterBillsAndHouseBillsList : Factory.GetCachedValue<CodeDescriptionPairList>();
			}
		}

		public IBusinessObjectCollection Bills
		{
			get
			{
				CusInBondMoveHeader moveHeader = Parent.MoveHeader;
				CusInBondHeader header = moveHeader != null ? moveHeader.Header : null;
				if (header != null)
				{
					return header.Bills;
				}
				else
				{
					return new ActiveBusinessObjectCollection<CusInBondBill>(Factory, ZQuery.NoResultQuery);
				}
			}
		}

		public IBusinessObjectCollection MoveHeaders
		{
			get
			{
				IBusinessObjectCollection result;
				CusInBondHeader header = Parent.InBondHeader;
				if (header == null)
				{
					result = new ActiveBusinessObjectCollection<CusInBondMoveHeader>(Factory, ZQuery.NoResultQuery);
				}
				else
				{
					result = header.FilteredMovementHeaders;
				}
				return result;
			}
		}

		public override ICodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public override ICodeDescriptionPairList CustomsStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public InbondCommonTypeList EntryTypeList
		{
			get { return Factory.GetCachedValue<InbondCommonTypeList>(); }
		}

		public CodeDescriptionPairList PreviousEntryTypeList
		{
			get
			{
				return Factory.GetCachedValue("USCusInBondMoveDetailLookupsPreviousEntryTypeList", () =>
				{
					var list = new CodeDescriptionPairList(Factory.GetCachedValue<InbondCommonTypeList>());
					list.AddPair(US.Business.EntryTypeList.Codes.ConsumptionFTZ, US.Business.EntryTypeList.Descriptions.ConsumptionFTZ);
					list.AddPair(US.Business.EntryTypeList.Codes.Warehouse, US.Business.EntryTypeList.Descriptions.Warehouse);
					return list;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}
	}
}
