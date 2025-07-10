using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IDefaultNumberOfDecimalsSupporter
	{
		ZString TransportMode { get; }
		ZString GetUnitOfMeasure(PropertyDescriptor property);
		int GetDefaultNumberOfDecimals(PropertyDescriptor property);
		ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value);
		void RoundMeasurePropertiesOnTransportModeChanged();
	}
}
