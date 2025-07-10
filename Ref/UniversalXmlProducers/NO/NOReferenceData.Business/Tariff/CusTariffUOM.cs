using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	public static class CusTariffUOM
	{
		public static RefCusTariffUOM[] GetRefCusTariffUOM(VarenummerListe goodsNumberData, string tariffId)
		{
			var result = new List<RefCusTariffUOM>();

			foreach (var uom in from goodsNumber in goodsNumberData?.ItemNumber ?? Array.Empty<varenummer>()
								where goodsNumber.id == tariffId
								select new
								{
									goodsNumber.id,
									goodsNumber.Unit,
									goodsNumber.UnitDescription,
									goodsNumber.OtherUnit,
									goodsNumber.OtherUnitDescription
								})
			{
				var cusTariffUOM = ConvertRefCusTariffUOM(uom.id, uom.Unit, uom.UnitDescription, "1");
				if (cusTariffUOM != null)
				{
					result.Add(cusTariffUOM);
				}

				if (!string.IsNullOrEmpty(uom.OtherUnit))
				{
					cusTariffUOM = ConvertRefCusTariffUOM(uom.id, uom.OtherUnit, uom.OtherUnitDescription, "2");
					if (cusTariffUOM != null)
					{
						result.Add(cusTariffUOM);
					}
				}
			}

			return result.ToArray();
		}

		public static RefCusTariffUOM ConvertRefCusTariffUOM(string id, string unit, string unitdesc, string uomNumber)
		{
			var uomCode = ConvertUomCode(unit);
			if (!string.IsNullOrEmpty(uomCode))
			{
				return new RefCusTariffUOM
				{
					ZZ8_Type = string.Concat("CU", uomNumber),
					ZZ8_UOM = uomCode
				};
			}

			TariffParser.ErrorBuilder.AppendLine("Unable to parse Varenummer code due to empty or missing code.");
			TariffParser.ErrorBuilder.AppendLine("DETAILS:");
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Varenummer: {0}", id).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Unit: {0}", unit).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Unit description: {0}", unitdesc).AppendLine();
			return null;
		}

		static string ConvertUomCode(string input)
		{
			switch (input)
			{
				// Conversion from https://www.toll.no/Imagedocument/summary?imageBlockId=280&contentPageId=194#41%20Mengde%20i%20annen%20enhet
				case "C":
					return UomCodes.Carat;
				case "F":
					return UomCodes.Fat;
				case "G":
					return UomCodes.Gram;
				case "K":
					return UomCodes.Torrvekt;
				case "KG":
					return UomCodes.Kilogram;
				case "L":
					return UomCodes.Liter;
				case "M":
					return UomCodes.Meter;
				case "M2":
					return UomCodes.Kvadratmeter;
				case "M3":
					return UomCodes.Kubikkmeter;
				case "M3F":
					return UomCodes.Kubikkmeter;
				case "MWH":
					return UomCodes.Megawattime;
				case "PAR":
					return UomCodes.AntallPar;
				case "STK":
					return UomCodes.AntallEnheter;
			}

			return string.Empty;
		}
	}
}
