using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonBookedCtgMoveCollection : ActiveBusinessObjectCollection<CommonBookedCtgMove>
	{
		public CommonBookedCtgMoveCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommonBookedCtgMoveCollection(CommonCartage cartage)
			: base(cartage.Factory, cartage, null, JobBookedCtgMoveSchema.EW_JJ)
		{
			allowNew = !cartage.HasParent;
		}

		public CommonBookedCtgMoveCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public CommonBookedCtgMoveCollection(BusinessObjectFactory factory, ICollectionRelationship relationship, bool allowNew)
			: base(factory, relationship)
		{
			this.allowNew = allowNew;
		}

		protected override bool AllowNew
		{
			get { return allowNew; }
		}
		readonly bool allowNew = true;
	}
}
