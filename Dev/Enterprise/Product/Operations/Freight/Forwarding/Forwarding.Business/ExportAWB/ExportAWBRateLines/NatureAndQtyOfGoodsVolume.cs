using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class NatureAndQtyOfGoodsVolume : Forwarding.AWB.Business.NatureAndQtyOfGoodsVolume,
		IDefaultNumberOfDecimalsSupporter
	{
		public NatureAndQtyOfGoodsVolume(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		protected override decimal GetRoundedVolume(decimal value)
		{
			if (Unit != ZString.Empty && FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value)
			{
				return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, VolumeInfo, value);
			}
			return base.GetRoundedVolume(value);
		}

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			return Unit;
		}

		int IDefaultNumberOfDecimalsSupporter.GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetNumberOfDecimalsCore(property);
		}

		protected override int GetNumberOfDecimalsCore(PropertyDescriptor property)
		{
			if (Unit != ZString.Empty && FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value)
			{
				return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
			}
			return -1;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedVolume(value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(VolumeInfo);
		}

		#endregion
	}
}
