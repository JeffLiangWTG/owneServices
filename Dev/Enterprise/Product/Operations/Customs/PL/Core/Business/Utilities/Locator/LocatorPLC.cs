using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

[Immutable]
sealed class LocatorPLC : LocatorBase
{
	public override ZString ApplicationCodes => $"{ApplicationCodeList.Codes.PLCustoms}";

	protected override BusinessObject FindBusinessObjectByLRN(BusinessObjectFactory factory, ZString lrn)
	{
		if (lrn.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = CusEntryHeaderSchema.CH_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, lrn)
			.AddToFilter(CusEntryHeaderSchema.CH_DataModel, Core.Constants.CountryCodes.Poland);
		return factory.LoadTop1<CusEntryHeader>(query);
	}
}
