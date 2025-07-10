using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class UXMLParser
	{
		readonly IXmlWriter _unlocoXmlWriter;
		readonly IXmlWriter _unlocoXmlWithCoordinates;
		readonly IXmlWriter _unlocoXmlWithoutIATAWriter;
		readonly List<UNLOCO> _unlocoes;
		readonly List<IATA> _iatas;
		readonly List<MappedCountry> _mappedCountries;

		public UXMLParser(List<UNLOCO> unlocoes, List<IATA> iatas, IXmlWriter unlocoXmlWriter, IXmlWriter unlocoXmlWithoutIATAWriter, IXmlWriter unlocoXmlWithoutCoordinates)
		{
			Argument.NotNull(unlocoes, nameof(unlocoes));
			Argument.NotNull(iatas, nameof(iatas));
			Argument.NotNull(unlocoXmlWriter, nameof(unlocoXmlWriter));
			Argument.NotNull(unlocoXmlWithoutIATAWriter, nameof(unlocoXmlWithoutIATAWriter));
			Argument.NotNull(unlocoXmlWithoutCoordinates, nameof(unlocoXmlWithoutCoordinates));

			_unlocoes = unlocoes;
			_iatas = iatas;
			_unlocoXmlWriter = unlocoXmlWriter;
			_unlocoXmlWithoutIATAWriter = unlocoXmlWithoutIATAWriter;
			_unlocoXmlWithCoordinates = unlocoXmlWithoutCoordinates;
			_mappedCountries = MappingSubdivisionHelper.GenerateMappedSubdivisions();
		}

		public void Parse()
		{
			GenerateNoAirportUNLOCO();
			GenerateAirportUNLOCO();
		}

		void GenerateNoAirportUNLOCO()
		{
			var unlocoNoAirport = _unlocoes.Where(o => !o.Function.HasAirport());
			var cityCodeIATAs = _iatas.Where(o => o.FunctionType == "C");
			if (unlocoNoAirport.Any())
			{
				foreach (var unloco in unlocoNoAirport)
				{
					var cityCodeIATA = cityCodeIATAs.FirstOrDefault(o => o.CountryName == unloco.Country && (o.IATACode == unloco.Location || o.IATACityCode == unloco.Location));
					if (cityCodeIATA != null)
					{
						unloco.IATA = cityCodeIATA.IATACode;
						unloco.IATARegionCode = cityCodeIATA.IATACityCode;
					}

					_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));
					PopulateUNLOCOWithCoordinates(unloco);
				}
			}
		}

		void PopulateUNLOCOWithCoordinates(UNLOCO uNLOCO)
		{
			if (!string.IsNullOrEmpty(uNLOCO.Coordinates))
			{
				_unlocoXmlWithCoordinates.PopulateData(uNLOCO.ConvertToRefUNLOCO(_mappedCountries));
			}
		}

		void GenerateAirportUNLOCO()
		{
			var unlocoAirports = _unlocoes.Where(o => o.Function.HasAirport()).ToList();
			if (unlocoAirports.Any())
			{
				foreach (var iata in _iatas.ToList())
				{
					var unlocoRelatedList = unlocoAirports.Where(o => o.Country == iata.CountryName && o.IATA == iata.IATACode);
					if (unlocoRelatedList.Any())
					{
						foreach (var unloco in unlocoRelatedList.ToList())
						{
							unloco.IATARegionCode = iata.IATACityCode;
							_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));
							PopulateUNLOCOWithCoordinates(unloco);

							unlocoAirports.Remove(unloco);
						}
					}
				}

				foreach (var unloco in unlocoAirports.ToList())
				{
					PopulateUNLOCOWithCoordinates(unloco);
					if (!FoundByCountryAndLocation(_iatas, unlocoAirports, unloco))
					{
						if (!FoundByCountryAndCityName(_iatas, unlocoAirports, unloco))
						{
							_unlocoXmlWithoutIATAWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));
						}
					}
				}
			}
		}

		bool FoundByCountryAndLocation(List<IATA> iatas, List<UNLOCO> unlocoAirports, UNLOCO unloco)
		{
			Argument.NotNull(iatas, nameof(iatas));
			Argument.NotNull(unlocoAirports, nameof(unlocoAirports));
			Argument.NotNull(unloco, nameof(unloco));

			var iataByCountryAndLocation = _iatas.Where(o => o.CountryName == unloco.Country && o.IATACityCode == unloco.Location);
			if (iataByCountryAndLocation.Count() == 1)
			{
				unloco.IATA = iataByCountryAndLocation.First()?.IATACode;
				unloco.IATARegionCode = iataByCountryAndLocation.First()?.IATACityCode;
				_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));

				unlocoAirports.Remove(unloco);
				return true;
			}
			else if (iataByCountryAndLocation.Count() > 1)
			{
				unloco.IATA = iataByCountryAndLocation.First()?.IATACityCode;
				unloco.IATARegionCode = iataByCountryAndLocation.First()?.IATACityCode;
				_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));

				unlocoAirports.Remove(unloco);
				return true;
			}

			return false;
		}

		bool FoundByCountryAndCityName(List<IATA> iatas, List<UNLOCO> unlocoAirports, UNLOCO unloco)
		{
			Argument.NotNull(iatas, nameof(iatas));
			Argument.NotNull(unlocoAirports, nameof(unlocoAirports));
			Argument.NotNull(unloco, nameof(unloco));

			var iataByCountryAndCityName = iatas.Where(o => o.CountryName == unloco.Country && (o.CityName.Contains(unloco.NameWoDiacritics) || unloco.NameWoDiacritics.Contains(o.CityName)) && unloco.Subdivision.StartsWith(o.StateName, StringComparison.InvariantCultureIgnoreCase));
			if (iataByCountryAndCityName.Count() == 1)
			{
				unloco.IATA = iataByCountryAndCityName.First()?.IATACode;
				unloco.IATARegionCode = iataByCountryAndCityName.First()?.IATACityCode;
				_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));

				unlocoAirports.Remove(unloco);
				return true;
			}
			else if (iataByCountryAndCityName.Any())
			{
				unloco.IATA = iataByCountryAndCityName.First()?.IATACityCode;
				unloco.IATARegionCode = iataByCountryAndCityName.First()?.IATACityCode;
				_unlocoXmlWriter.PopulateData(unloco.ConvertToRefUNLOCO(_mappedCountries));

				unlocoAirports.Remove(unloco);
				return true;
			}

			return false;
		}
	}
}
