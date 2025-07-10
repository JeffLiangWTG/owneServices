using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.InBond.Module
{
	public class CusInBondHeaderFilterLookups
	{
		public CusInBondHeaderFilterLookups(CusInBondHeaderFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		readonly CusInBondHeaderFilterStripBusinessObject filterBizObj;

		BusinessObjectFactory Factory => filterBizObj.Factory;

		public InbondCommonTypeList InbondCommonTypeList
		{
			get { return Factory.GetCachedValue<InbondCommonTypeList>(); }
		}

		public ConsigneeCollection ImporterList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public TransportModeCodes TransportModeList
		{
			get { return Factory.GetCachedValue<TransportModeCodes>(); }
		}

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public ShippingProviderCollection ShippingProviders
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		public CodeDescriptionPairList BillDispostionList
		{
			get
			{
				return Factory.GetCachedValue("BillDispostionList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode));
					var dispositionCodesForAir = DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode).ToArray();
					dispositionCodesForAir.ForEach(x => result.AddPairIfNotExist(x.Code, x.Description));
					result.Sort();
					return result;
				});
			}
		}

		public CodeDescriptionPairList InbondQPMessageStatusListForFilter
		{
			get
			{
				return Factory.GetCachedValue("InbondQPMessageStatusListForFilter", () =>
				{
					var result = CusInBondMoveHeaderLookups.GenerateInbondQPMessageStatusList();
					result.RemoveCode(MessageStatusListIT.Codes.NotSent);
					result.AddPair(CusInBondHeaderFilterStripBusinessObject.FilterConstants.NotSentForFilter, CusInBondHeaderFilterStripBusinessObject.FilterConstants.NotSentForFilterDescription);
					return result;
				});
			}
		}

		public CodeDescriptionPairList InbondWPMessageStatusListForFilter
		{
			get
			{
				return Factory.GetCachedValue("InbondWPMessageStatusListForFilter", () =>
				{
					var result = CusInBondMoveHeaderLookups.GenerateInbondWPMessageStatusList();
					result.RemoveCode(MessageStatusListIT.Codes.NotSent);
					result.AddPair(CusInBondHeaderFilterStripBusinessObject.FilterConstants.NotSentForFilter, CusInBondHeaderFilterStripBusinessObject.FilterConstants.NotSentForFilterDescription);
					return result;
				});
			}
		}
	}
}
