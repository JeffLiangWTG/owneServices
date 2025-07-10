using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business;

public class PLRefCusProcedureCollection : ActiveBusinessObjectCollection<RefCusProcedure>
{
	public PLRefCusProcedureCollection(BusinessObjectFactory factory)
		: base(factory)
	{ }

	PLRefCusProcedureCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
	{ }

	public static PLRefCusProcedureCollection LoadCustomsProcedureCodesForPoland(BusinessObjectFactory factory, string dataGroupingCode, string shipmentType, ZDateTime date)
	{
		var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
		query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, shipmentType);
		return new PLRefCusProcedureCollection(factory, query);
	}
}
