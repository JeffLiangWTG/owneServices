using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business
{
	public class HeaderOtherInfoCollection : OtherInfoCollection
	{
		public HeaderOtherInfoCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoPropertyInfo) : base(factory, addInfoPropertyInfo) { }

		public new HeaderOtherInfo this[int index]
		{
			get { return (HeaderOtherInfo)base[index]; }
		}

		public new HeaderOtherInfo AddNew()
		{
			return (HeaderOtherInfo)base.AddNew();
		}

		protected override CodeDataPair CreateNewCodeInfo()
		{
			return new HeaderOtherInfo(Factory, this);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			HeaderOtherInfo info = bizO as HeaderOtherInfo;
			if (info != null && !IsCodesChangedRelatedActionsSuspended)
			{
				info.RefreshMCDDetailsIfApplicable();
			}
			base.OnRemoved(bizO);
		}
	}
}
