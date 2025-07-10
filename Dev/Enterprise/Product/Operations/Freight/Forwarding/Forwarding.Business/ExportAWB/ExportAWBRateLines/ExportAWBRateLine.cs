using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ConsolExportAWBHeader), "AWBRateLines")]
	public class ExportAWBRateLine : Forwarding.AWB.Business.ExportAWBRateLine,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		public ExportAWBRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			originalER_EH = ER_EH;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				switch (FactorySaveMode)
				{
					case ExportAWBHeader.SaveMode.Normal:
						return base.IsSavedByFactory;
					case ExportAWBHeader.SaveMode.Forced:
						return IsInDatabase || (!IsDeleted && !IsEmpty);
					default:
						return false;
				}
			}
		}

		ExportAWBHeader.SaveMode FactorySaveMode
		{
			get
			{
				var result = ExportAWBHeader.SaveMode.Normal;
				var originalMaster = OriginalMaster;

				if (originalMaster != null)
				{
					if (!(originalMaster.Parent?.IsAWBHeaderAccessible ?? false))
					{
						result = ExportAWBHeader.SaveMode.Never;
					}
					else if (originalMaster.EH_AreRateLinesOverridden)
					{
						result = ExportAWBHeader.SaveMode.Forced;
					}
					else
					{
						return originalMaster.FactorySaveMode;
					}
				}

				return result;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("475fe46e-2c75-4140-9988-a5184628459c", "Air Waybill Freight Breakdown");

				if (Master is ShipmentExportAWBHeader)
				{
					result = Res.GetString("45e41bc8-0e00-4932-9cce-2b49f10f8932", "House {0}", result);
				}
				else if (Master is ConsolExportAWBHeader)
				{
					result = Res.GetString("123debad-2eb2-4007-a0d7-ccedddedb2c8", "Master {0}", result);
				}

				return result;
			}
		}

		public new ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(ER_EH); }
		}

		ExportAWBHeader OriginalMaster
		{
			get { return Factory.Load<ExportAWBHeader>(IsDeleted || ER_EH.IsEmpty ? originalER_EH : ER_EH); }
		}

		public override ZGuid ER_EH
		{
			get { return base.ER_EH; }
			set
			{
				base.ER_EH = value;
				if (!value.IsEmpty)
				{
					originalER_EH = value;
				}
			}
		}
		ZGuid originalER_EH;

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsVolume GetNatureAndQtyOfGoodsVolume()
		{
			return new NatureAndQtyOfGoodsVolume(this);
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoods GetNewNatureAndQtyOfGoodsText()
		{
			return new NatureAndQtyOfGoods(this);
		}

		#region IDefaultNumberOfDecimalsSupporter Members

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedGrossWeight(column, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedGrossWeight(null, value);
		}

		public override ZDecimal GetRoundedGrossWeight(SchemaColumn column, ZDecimal value)
		{
			if (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value && DefaultNumberOfDecimalsSupporterHelperForFreight.IsRegistryDecimalValueOverridden(this, ER_GrossWeightInfo))
			{
				return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, ER_GrossWeightInfo, value);
			}
			return base.GetRoundedGrossWeight(column, value);
		}

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			switch (base.ER_WeightInLBsOrKGs)
			{
				case Core.Constants.AWB.RateLineUQ.Kilos:
					return Core.Constants.Weight.Kilograms;
				case Core.Constants.AWB.RateLineUQ.Pounds:
					return Core.Constants.Weight.Pounds;
				default:
					return ZString.Empty;
			}
		}

		int IDefaultNumberOfDecimalsSupporter.GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetNumberOfDecimalsCore(property);
		}

		protected override int GetNumberOfDecimalsCore(PropertyDescriptor property)
		{
			if (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value && property.Name == Schema.ER_GrossWeight)
			{
				if (DefaultNumberOfDecimalsSupporterHelperForFreight.IsRegistryDecimalValueOverridden(this, property))
				{
					return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
				}
				else if (ER_WeightInLBsOrKGs.IsEmpty && ER_LineCount > 1)
				{
					int rateLineIndexWithLastSetDecimalPlace = GetIndexOfLastRateLineWithDecimalPlace();
					if (rateLineIndexWithLastSetDecimalPlace >= 0)
					{
						return Master.AWBRateLines[rateLineIndexWithLastSetDecimalPlace].GetNumberOfDecimalsCore(property);
					}
				}
			}
			return base.GetNumberOfDecimalsCore(property);
		}

		int GetIndexOfLastRateLineWithDecimalPlace()
		{
			int lastIndex = -1;

			if (Master == null)
			{
				return lastIndex;
			}

			for (int index = ER_LineCount - 1; index >= 0 && index < Master.AWBRateLines.Count; index--)
			{
				if (!Master.AWBRateLines[index].ER_WeightInLBsOrKGs.IsEmpty)
				{
					lastIndex = index;
					break;
				}
			}
			return lastIndex;
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(ExportAWBRateLineSchema.ER_GrossWeight, ER_GrossWeightInfo);
		}

		#endregion
	}
}
