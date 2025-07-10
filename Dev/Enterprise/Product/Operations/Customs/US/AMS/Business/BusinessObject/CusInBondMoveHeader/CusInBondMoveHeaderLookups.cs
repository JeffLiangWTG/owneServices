using CargoWise.Integration;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveHeaderLookups : US.Business.CusInBondMoveHeaderLookups
	{
		public CusInBondMoveHeaderLookups(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		public SubApplicationCodeList SubApplicationCodeList
		{
			get { return Factory.GetCachedValue<SubApplicationCodeList>(); }
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}

		public ActiveOrgCusCodeCollection ImporterCodes
		{
			get { return new ActiveOrgCusCodeCollection(Factory); }
		}

		public ICodeDescriptionPairList InBondStatusList
		{
			get { return AMSBillMessageStatusList.GetInBondCodeList(Factory); }
		}

		public override ICodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<AMSBillMessageStatusList>(); }
		}
	}
}
