using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.InBond.Module
{
	public class USInBondMoveHeaderFilterLookups
	{
		public USInBondMoveHeaderFilterLookups(USInBondMoveHeaderFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		readonly USInBondMoveHeaderFilterStripBusinessObject filterBizObj;

		BusinessObjectFactory Factory => filterBizObj.Factory;

		public InbondCommonTypeList InbondCommonTypeList
		{
			get { return Factory.GetCachedValue<InbondCommonTypeList>(); }
		}

		public ConsigneeCollection ImporterList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ShippingProviderCollection ShippingProviders
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		public TransportModeCodes TransportModeList
		{
			get { return Factory.GetCachedValue<TransportModeCodes>(); }
		}

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(Factory); }
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
