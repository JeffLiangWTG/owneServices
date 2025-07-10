using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferOnPickCollection : ActiveBusinessObjectCollection<WhsTransfer>
	{
		public WhsTransferOnPickCollection(WhsPick pick)
			: base(GetFatoryWithNullCheck(pick), pick, new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer), WhsDocketSchema.WD_WP_ParentPickForTransfer)
		{
		}

		static BusinessObjectFactory GetFatoryWithNullCheck(WhsPick pick) => Argument.NotNull(pick, nameof(pick)).Factory;
	}
}

// Tested in Enterprise.Warehouse.Transactions.Business.Testing.WhsTransferOnPickCollectionTest
