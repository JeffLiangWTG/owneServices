using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusPackableItemLookups : AutoCusPackableItemLookups
	{
		public CusPackableItemLookups(AutoCusPackableItem parent) : base(parent)
		{
		}

		protected new CusPackableItem Parent => (CusPackableItem)base.Parent;

		public CodeDescriptionPairList PackTypes => Parent.InvoiceLine?.GetPackableItemUQList() ?? RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
	}
}
