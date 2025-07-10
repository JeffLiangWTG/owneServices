using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class LocatorPLX : LocatorBase
{
	public override ZString ApplicationCodes => ApplicationCodeList.Codes.PLCustomsExitControl;

	protected override BusinessObject FindBusinessObjectByLRN(BusinessObjectFactory factory, ZString lrn) =>
		FindExitReportByReferenceNumber(factory, CusExitConsignmentSchema.CXC_LocalReference, lrn);

	protected override BusinessObject FindBusinessObjectByMRN(BusinessObjectFactory factory, ZString mrn) =>
		FindExitReportByReferenceNumber(factory, CusExitConsignmentSchema.CXC_MovementReference, mrn);

	static BusinessObject FindExitReportByReferenceNumber(BusinessObjectFactory factory, SchemaStringColumn referenceNumberColumn, ZString referenceNumber)
	{
		if (referenceNumber.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = CusExitConsignmentSchema.CXC_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(referenceNumberColumn, referenceNumber);
		var exitConsignment = factory.LoadTop1<CusExitConsignment>(query);

		var exitReport = exitConsignment?.Header.CusExitReports.SingleOrDefault(r => r.CER_CXC_Consignment == exitConsignment.PK);
		return exitReport;
	}
}
