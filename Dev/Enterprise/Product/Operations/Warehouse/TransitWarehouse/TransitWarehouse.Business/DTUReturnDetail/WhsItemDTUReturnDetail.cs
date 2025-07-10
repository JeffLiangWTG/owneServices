using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeAlive("This Business Object is used in Glow.")]
	public class WhsItemDTUReturnDetail : AutoWhsItemDTUReturnDetail
	{
		public WhsItemDTUReturnDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public WhsItemDispatchTransportationUnit DispatchTransportationUnit => Factory.Load<WhsItemDispatchTransportationUnit>(WDR_WDH_TransitDispatchTransportationUnit);

		public WhsLocation StagingLocation => Factory.Load<WhsLocation>(WDR_WL_StagingLocation);

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(DispatchTransportationUnit))]
		public override ZGuid WDR_WDH_TransitDispatchTransportationUnit { get => base.WDR_WDH_TransitDispatchTransportationUnit; }

		[RelatedBusinessObject(nameof(StagingLocation))]
		public override ZGuid WDR_WL_StagingLocation { get => base.WDR_WL_StagingLocation; }

		#endregion
	}
}
