
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class GenericLandedCostHistoryCollection : DependentBusinessObjectCollection<LandedCostHistory, BusinessObject>
	{
		public GenericLandedCostHistoryCollection(ILandedCostHistoryMaster master) : base(master as BusinessObject)
		{
			this.Master = master;
		}

		#region Implementation

		protected new readonly ILandedCostHistoryMaster Master;

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return Master.FKSchemaColumnInLandedCostHistory; }
		}

		#endregion
	}
}
