using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.NCTS.Business;

[Immutable]
sealed class LocatorNCTS : LocatorBase
{
	public override ZString ApplicationCodes => ApplicationCodeList.Codes.PLCustomsNCTS;

	protected override BusinessObject FindBusinessObjectByLRN(BusinessObjectFactory factory, ZString lrn)
	{
		if (string.IsNullOrEmpty(lrn))
		{
			return null;
		}

		var query = new ZQuery { OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, lrn);
		return factory.LoadTop1<NctsCommonMovementHeader>(query) switch
		{
			NctsArrivalMovementHeader arrivalMovementHeader => arrivalMovementHeader.Header,
			NctsDepartureMovementHeader departureMovementHeader => departureMovementHeader,
			_ => null,
		};
	}

	protected override BusinessObject FindBusinessObjectByMRN(BusinessObjectFactory factory, ZString mrn)
	{
		var nctsHeader = base.FindBusinessObjectByMRN(factory, mrn) as NctsHeader;
		return (string)nctsHeader?.BH_HeaderType switch
		{
			NctsMovementType.Codes.Arrival => nctsHeader,
			NctsMovementType.Codes.Departure => nctsHeader.MovementHeader,
			_ => null,
		};
	}
}
