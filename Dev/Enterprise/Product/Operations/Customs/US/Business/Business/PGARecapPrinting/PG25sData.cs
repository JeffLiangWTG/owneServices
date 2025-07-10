using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG25sData : IDataSerialiser
	{
		internal PG25sData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public IEnumerable<AEPAPG25> PG25s
		{
			get { return pg25s; }
		}

		public bool HasData
		{
			get { return pg25s != null && pg25s.Count > 0; }
		}

		public void Clear()
		{
			if (pg25s != null)
			{
				pg25s.Clear();
			}
		}

		List<AEPAPG25> pg25s;

		public void Add(AEPAPG25 pg25)
		{
			if (pg25 != null)
			{
				(pg25s = pg25s ?? new List<AEPAPG25>()).Add(pg25);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			if (HasData)
			{
				foreach (var pg25 in PG25s)
				{
					string temperature;
					switch (pg25.LocationOfTemperatureRecording)
					{
						case "A":
							temperature = "Product Temperature: ";
							break;
						case "B":
							temperature = "Container Temperature: ";
							break;
						case "C":
							temperature = "Conveyance Temperature: ";
							break;
						default:
							temperature = "Temperature: ";
							break;
					}
					var temperatureData = new ZStringBuilder();
					temperatureData.AppendIfNotEmpty(Serialiser.PrependDescription(pg25.TemperatureQualifier, TemperatureQualifierList, Serialiser.DelimitedType.Parenthesis));
					if (!pg25.DegreeType.IsEmpty)
					{
						temperatureData.Append((pg25.NegativeNumber == "X" ? "-" : "") + pg25.ActualTemperature.ToString());
						temperatureData.AppendIfNotEmpty(DegreeTypeList.GetDescriptionFromCode(pg25.DegreeType) ?? pg25.DegreeType);
					}
					string lot;
					switch (pg25.LotNumberQualifier)
					{
						case "1":
							lot = "Manufacturer Lot Number";
							break;
						case "2":
							lot = "Seller Lot Number";
							break;
						case "3":
							lot = "Grower Lot Number";
							break;
						case "4":
							lot = "Producer Lot Number";
							break;
						default:
							lot = "Lot Number";
							break;
					}
					var productionDate = new ZStringBuilder();
					productionDate.AppendIfNotEmpty("Start - ", pg25.ProductionStartDateOfTheLot.IsEmpty ? "" : pg25.ProductionStartDateOfTheLot.ToShortDateString());
					productionDate.AppendIfNotEmpty("End - ", pg25.ProductionEndDateOfTheLot.IsEmpty ? "" : pg25.ProductionEndDateOfTheLot.ToShortDateString());
					yield return Serialiser.CreateLine(false, Serialiser.CreateValue(temperature, temperatureData.ToStringWithDelimiterBetweenAppends(" ")), Serialiser.CreateValue(lot, pg25.LotNumber), Serialiser.CreateValue("Production Date: ", productionDate.ToStringWithDelimiterBetweenAppends(" ")), Serialiser.CreateValue("Line Value: ", !pg25.PGALineValue.IsEmpty || !pg25.PGAUnitValue.IsEmpty ? (pg25.PGALineValue.ToString() + " " + pg25.PGAUnitValue).Trim() : ""));
				}
			}
		}

		TemperatureQualifierList TemperatureQualifierList
		{
			get { return factory.GetCachedValue<TemperatureQualifierList>(); }
		}

		DegreeTypeList DegreeTypeList
		{
			get { return factory.GetCachedValue<DegreeTypeList>(); }
		}

		readonly BusinessObjectFactory factory;
	}
}
