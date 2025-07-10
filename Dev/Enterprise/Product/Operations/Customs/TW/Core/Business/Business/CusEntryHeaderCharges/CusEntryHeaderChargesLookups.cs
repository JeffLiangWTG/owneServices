using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges
		{
			get { return Parent; }
		}

		protected new CusEntryHeaderCharges Parent
		{
			get { return (CusEntryHeaderCharges)base.Parent; }
		}

		public ICodeDescriptionPairList ChargeTypeList => ChargeTypeHelper.GetEntryHeaderChargeTypes(Factory, Parent?.EntryHeader);

		public override CodeDescriptionPairList PaymentMethodsList => Factory.GetCachedValue<EntryChargePaymentMethod>();
	}
}
