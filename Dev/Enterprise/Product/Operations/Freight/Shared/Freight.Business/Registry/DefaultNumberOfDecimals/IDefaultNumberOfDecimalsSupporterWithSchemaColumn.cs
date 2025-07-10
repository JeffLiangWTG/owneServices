using System.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IDefaultNumberOfDecimalsSupporterWithSchemaColumn : IDefaultNumberOfDecimalsSupporter
	{
		ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value);
	}
}
