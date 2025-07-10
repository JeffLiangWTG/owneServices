using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondBillLookups : Customs.Business.CusInBondBillLookups
	{
		public CusInBondBillLookups(CusInBondBill parent)
			: base(parent)
		{
		}

		protected new CusInBondBill Parent
		{
			get { return (CusInBondBill)base.Parent; }
		}

		public CodeDescriptionPairList ManifestUnitList
		{
			get
			{
				var bill = Parent;
				var header = bill != null ? bill.Header : null;
				var transportMode = header != null ? header.BH_ImportTransportMode : ZString.Empty;
				return Factory.GetCachedValue<CodeDescriptionPairList>("USAMSCusInBondBillManifestUnitList" + transportMode, delegate
				{
					var list = new ManifestUnitList();
					if (transportMode == TransportTypeList.Codes.VesselContainer ||
						transportMode == TransportTypeList.Codes.VesselNonContainer)
					{
						list.RemoveCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms);
					}
					return list;
				});
			}
		}

		public CodeDescriptionPairList StatusIndicatorList
		{
			get
			{
				var bill = Parent;
				var header = bill.Header;
				return BillOfLadingStatusIndicatorList.GetCachedValue(Factory, header != null && header.IsNVOCCHeader, bill.IsOceanBillType, ZZCustomsFunctionality.IsAMSHBREffective);
			}
		}

		public AMSBillMessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<AMSBillMessageStatusList>(); }
		}

		public TransportTypeList TransportModeList
		{
			get { return Factory.GetCachedValue<TransportTypeList>(); }
		}

		public PaymentMethodCodeList PaymentMethodCodes
		{
			get { return Factory.GetCachedValue<PaymentMethodCodeList>(); }
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList VolumeUnitList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public ConsignorCollection Shippers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public USCarrierCombinedCollection SCACList
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public RefUNLOCOCollection UNLOCOCollection
		{
			get { return new RefUNLOCOCollection(Factory); }
		}
	}
}
