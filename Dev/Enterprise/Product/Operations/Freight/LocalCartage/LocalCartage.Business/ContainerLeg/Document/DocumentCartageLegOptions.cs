using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentCartageLegOptions : NonPersistentBusinessObject
	{
		public DocumentCartageLegOptions(DocumentCartageLegCollection cartageLegs)
			: base(cartageLegs.Factory)
		{
			this.cartageLegs = cartageLegs;
		}

		public DocumentCartageLegCollection CartageLegs
		{
			get { return cartageLegs; }
		}
		readonly DocumentCartageLegCollection cartageLegs;
	}
}
