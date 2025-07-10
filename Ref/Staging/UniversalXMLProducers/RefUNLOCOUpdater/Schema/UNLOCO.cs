using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema
{
	public class UNLOCO
	{
		public UNLOCO()
		{
			Function = string.Empty;
		}

		public string Country { get; set; }
		public string Location { get; set; }
		public string Name { get; set; }
		public string NameWoDiacritics { get; set; }
		public string Subdivision { get; set; }
		public string Status { get; set; }
		public string Function { get; set; }
		public string Date { get; set; }
		public string IATA { get; set; }
		public string Coordinates { get; set; }
		public string Remarks { get; set; }
		public string IATARegionCode { get; set; }

		public string UniqueIdentifier => Country + Location;
	}
	public static class UNLOCOExtension
	{
		public static RefUNLOCO ConvertToRefUNLOCO(this UNLOCO unloco, List<MappedCountry> mappedCountries)
		{
			Argument.NotNull(unloco, nameof(unloco));
			Argument.NotNull(mappedCountries, nameof(mappedCountries));

			var result = new RefUNLOCO()
			{
				RL_Code = unloco.UniqueIdentifier,
				RL_HasAirport = unloco.Function.HasAirport(),
				RL_CoOrdinates = unloco.Coordinates,
				RL_GeoLocation = new RefGeography(GetGeoLocationStringFromCoOrdinates(unloco.Coordinates)),
				RL_HasBorderCrossing = unloco.Function.HasBorderCrossing(),
				RL_HasPost = unloco.Function.HasPost(),
				RL_HasRail = unloco.Function.HasRail(),
				RL_HasRoad = unloco.Function.HasRoad(),
				RL_HasSeaport = unloco.Function.HasSeaport(),
				RL_NameWithDiacriticals = unloco.Name,
				RL_PortName = unloco.NameWoDiacritics,
				RL_RN_NKCountryCode = unloco.Country,
				RL_RW_RN_NKCountryCode = !string.IsNullOrEmpty(unloco.Subdivision) ? unloco.Country : string.Empty,
				RL_RW_NKCode = unloco.Subdivision,
				RL_IATA = unloco.IATA,
				RL_IATARegionCode = string.IsNullOrEmpty(unloco.IATARegionCode) ? unloco.IATA : unloco.IATARegionCode,
				RL_IsActive = true
			};

			if (string.IsNullOrEmpty(unloco.Subdivision))
			{
				return result;
			}

			var mappedCountry = mappedCountries.FirstOrDefault(o => o.Name == unloco.Country);
			if (mappedCountry == null)
			{
				return result;
			}

			var mappedSubdivision = mappedCountry.MappedSubdivisions.FirstOrDefault(o => o.OldSubdivisionCode == unloco.Subdivision);
			if (mappedSubdivision == null)
			{
				return result;
			}

			result.RL_RW_NKCode = mappedSubdivision.CurrentSubdivisionCode;
			return result;
		}

		public static Geometry GetGeoLocationStringFromCoOrdinates(string coOrdinates)
		{
			if (!string.IsNullOrEmpty(coOrdinates))
			{
				var coOrdinatesText = coOrdinates.Split(' ');
				if (coOrdinatesText.Length == 2)
				{
					double longitude = 0.0;
					double latitude = 0.0;
					if (coOrdinatesText[0].Length == 5)
					{
						_ = double.TryParse(coOrdinatesText[0].AsSpan(0, 2), out latitude);
						_ = int.TryParse(coOrdinatesText[0].AsSpan(2, 2), out int value);
						latitude += (value / 60.0);
						latitude *= (coOrdinatesText[0].Substring(4, 1) == "S" ? -1 : 1);
					}
					if (coOrdinatesText[1].Length == 6)
					{
						_ = double.TryParse(coOrdinatesText[1].AsSpan(0, 3), out longitude);
						_ = int.TryParse(coOrdinatesText[1].AsSpan(3, 2), out int value);
						longitude += Math.Round((value / 60.0), 3);
						longitude *= (coOrdinatesText[1].Substring(5, 1) == "W" ? -1 : 1);
					}

					return TypeExtension.ConvertToGeometry(4326,
						$"POINT ({Math.Round(longitude, 3)} {Math.Round(latitude, 3)})");
				}
			}

			return TypeExtension.ConvertToGeometry(4326, "POINT EMPTY");
		}
	}
}
