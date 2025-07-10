using System;
using Enterprise.Integration.LandTransport;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Business.LandTransport;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	public class ConsignmentFact : InputFactWithUserDefinedProperties, IConsignmentFact
	{
		public ConsignmentFact(IDtbConsignment consignment, IBranchFact branchFact, IDepartmentFact departmentFact, string companyCountry, IOrganisationWithMainAddressFact localClient, ILandTransportJobWarehouseFact parentWarehouseOrder, ILandTransportJobShipmentFact parentForwardingShipment)
		{
			Argument.NotNull(consignment, nameof(consignment));
			Argument.NotNull(branchFact, nameof(branchFact));
			Argument.NotNull(departmentFact, nameof(departmentFact));
			Argument.NotNull(companyCountry, nameof(companyCountry));

			PK = consignment.PK.ToGuid();
			LocalClient = new FactLeftJoin<IOrganisationWithMainAddressFact>(localClient);
			ParentWarehouseOrder = new FactLeftJoin<ILandTransportJobWarehouseFact>(parentWarehouseOrder);
			ParentForwardingShipment = new FactLeftJoin<ILandTransportJobShipmentFact>(parentForwardingShipment);

			CurrentBranch = new FactJoin<IBranchFact>(branchFact);
			CurrentDepartment = new FactJoin<IDepartmentFact>(departmentFact);
			CurrentCompanyCountry = companyCountry;
		}

		public Guid PK { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> LocalClient { get; }

		public FactLeftJoin<ILandTransportJobWarehouseFact> ParentWarehouseOrder { get; }

		public FactLeftJoin<ILandTransportJobShipmentFact> ParentForwardingShipment { get; }

		public FactJoin<IBranchFact> CurrentBranch { get; }

		public FactJoin<IDepartmentFact> CurrentDepartment { get; }

		public string CurrentCompanyCountry { get; }
	}
}
