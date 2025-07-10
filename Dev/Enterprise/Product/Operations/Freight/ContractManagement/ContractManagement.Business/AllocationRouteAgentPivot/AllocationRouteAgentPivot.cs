using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.ContractManagement.Business
{
	public class AllocationRouteAgentPivot : AutoAllocationRouteAgentPivot, IAllocationRouteAgentPivot, IPivotBusinessObject
	{
		public AllocationRouteAgentPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(AllocationRoute))]
		public override ZGuid ARA_RCA_AllocationLine
		{
			get => base.ARA_RCA_AllocationLine;
			set => base.ARA_RCA_AllocationLine = value;
		}

		public RatingContractAllocationLine AllocationRoute => Factory.Load<RatingContractAllocationLine>(ARA_RCA_AllocationLine);

		public OrgHeader Organisation => Factory.Load<OrgHeader>(ARA_OH_Agent);

		#region IPivotBusinessObject Members

		public ZGuid Relation1ID { get => ARA_RCA_AllocationLine; set => ARA_RCA_AllocationLine = value; }

		public BusinessObject Relation1Object => AllocationRoute;

		public ZGuid Relation2ID { get => ARA_OH_Agent; set => ARA_OH_Agent = value; }

		public BusinessObject Relation2Object => Organisation;

		#endregion
	}
}
