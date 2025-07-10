using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class XsdDimensionValue
	{
		#region ImportWeight

		public static void ImportWeight(
			ZPropertyInfo weightInfo, ZPropertyInfo unitInfo,
			Xsd.DimensionValue value, int precision, int scale,
			IValueObjectImportContext context, string errorContext)
		{
			string unit = WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(value.DimensionType, errorContext, context);
			Import(weightInfo, unitInfo, new WeightConversionStrategy(), value.Value, unit, precision, scale, context, errorContext);
		}

		public static void ImportWeight(
			ZPropertyInfo weightInfo, ZPropertyInfo unitInfo,
			Xsd.DimensionValue value, SchemaDecimalColumn column,
			IValueObjectImportContext context, string errorContext)
		{
			string unit = WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(value.DimensionType, errorContext, context);
			Import(weightInfo, unitInfo, new WeightConversionStrategy(), value.Value, unit, column.Precision, column.Scale, context, errorContext);
		}

		#endregion

		#region ImportVolume

		public static void ImportVolume(
			ZPropertyInfo volumeInfo, ZPropertyInfo unitInfo,
			Xsd.DimensionValue value, int precision, int scale,
			IValueObjectImportContext context, string errorContext)
		{
			string unit = VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(value.DimensionType, errorContext, context);
			Import(volumeInfo, unitInfo, new VolumeConversionStrategy(), value.Value, unit, precision, scale, context, errorContext);
		}

		public static void ImportVolume(
			ZPropertyInfo volumeInfo, ZPropertyInfo unitInfo,
			Xsd.DimensionValue value, SchemaDecimalColumn column,
			IValueObjectImportContext context, string errorContext)
		{
			string unit = VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(value.DimensionType, errorContext, context);
			Import(volumeInfo, unitInfo, new VolumeConversionStrategy(), value.Value, unit, column.Precision, column.Scale, context, errorContext);
		}

		#endregion

		#region Import

		static void Import(
			ZPropertyInfo magnitudeInfo, ZPropertyInfo unitInfo,
			UnitConversionStrategy strategy,
			ZDecimal magnitude, ZString unit, int precision, int scale,
			IValueObjectImportContext context, string errorContext)
		{
			strategy.ReScale(ref magnitude, ref unit, precision, scale);

			if (magnitude.IsWithinSqlPrecisionAndScale(precision, scale))
			{
				magnitudeInfo.Value = magnitude;
				unitInfo.Value = unit;
			}
			else
			{
				string message = Res.GetString("233eea61-d48a-406e-8fc7-92be655d348d", "value = {0}{1}, max = {2}.{3}{1}",
					magnitude,
					unit,
					new string('9', precision - scale),
					new string('9', scale)
				);

				if (string.IsNullOrEmpty(errorContext))
				{
					context.Notify(new ErrorNotification(ErrorType.ValueOverflowError, message));
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.ValueOverflowError, errorContext + "; " + message));
				}
			}
		}

		#endregion
	}
}
