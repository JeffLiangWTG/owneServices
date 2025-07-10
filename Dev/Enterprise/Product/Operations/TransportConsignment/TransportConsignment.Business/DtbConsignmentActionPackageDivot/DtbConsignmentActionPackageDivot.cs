using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportConsignment.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used in future Work Items")]
	public class DtbConsignmentActionPackageDivot : AutoDtbConsignmentActionPackageDivot
	{
		public DtbConsignmentActionPackageDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public DtbConsignmentAction ConsignmentAction
		{
			get { return Factory.Load<DtbConsignmentAction>(LTP_LTA_ConsignmentAction); }
		}

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(LTP_KP_Package); }
		}

		#endregion
	}
}
