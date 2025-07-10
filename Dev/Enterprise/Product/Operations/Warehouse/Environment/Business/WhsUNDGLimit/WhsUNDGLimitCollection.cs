using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsUNDGLimitCollection : ActiveBusinessObjectCollection<WhsUNDGLimit>
	{
		public WhsUNDGLimitCollection(WhsWarehouse warehouse)
			: base(warehouse.Factory, warehouse, null, WhsUNDGLimitSchema.WWD_WW_Warehouse)
		{
		}

		public WhsWarehouse Warehouse
		{
			get
			{
				var matchingParentLine = MatchingParentLine;
				return matchingParentLine == null
					? (WhsWarehouse)Relationship.Master
					: matchingParentLine.Warehouse;
			}
		}
		WhsUNDGLimit MatchingParentLine
		{
			get { return Relationship.Master as WhsUNDGLimit; }
		}
	}
}
