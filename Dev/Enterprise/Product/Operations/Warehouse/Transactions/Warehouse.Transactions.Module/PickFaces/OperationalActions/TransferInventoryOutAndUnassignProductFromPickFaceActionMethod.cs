using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferInventoryOutAndUnassignProductFromPickFaceActionMethod : OperationalActionMethod
	{
		public TransferInventoryOutAndUnassignProductFromPickFaceActionMethod() : base(new ZGuid("D6C8A310-E178-4A57-A7B3-75DAD88BE9B9"))
		{
		}

		public override string Name => Res.GetString("7C5D4B2B-E1DE-47EC-AAE4-4D8FB69E959F", "Transfer Inventory Out And Un-assign Product(s) From Pick Face");

		public override string Description => Name;

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator(factory);
		}
	}
}
