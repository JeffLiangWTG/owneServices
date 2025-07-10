using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models
{
	internal class TariffKey
	{
		public Guid PK { get; set; }
		public string CheckDigit { get; set; } = "";
		public List<string> RelationshipTariffCodes { get; set; } = new List<string>();
		public short UniqueId { get; set; }
	}
}
