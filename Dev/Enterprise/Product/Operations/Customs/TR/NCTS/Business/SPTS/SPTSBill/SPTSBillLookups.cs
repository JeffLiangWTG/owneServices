using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSBillLookups : Customs.Business.CusInBondBillLookups
	{
		public SPTSBillLookups(SPTSBill parent)
			: base(parent)
		{
		}

		protected new SPTSBill Parent
		{
			get { return (SPTSBill)base.Parent; }
		}

		public CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<SPTSDeclarationTypeList>();

		public Universal.CodeDescriptionPairLists.YesNoList YesNoList
		{
			get { return Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>(); }
		}
	}
}
