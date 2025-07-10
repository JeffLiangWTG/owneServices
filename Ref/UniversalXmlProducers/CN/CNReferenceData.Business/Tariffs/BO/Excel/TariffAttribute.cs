using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class TariffAttribute
	{
		[Name("TariffCode")]
		public string TariffCode { get; set; }

		[Name("AttributeName")]
		public string AttributeName { get; set; }

		[Name("AttributeValue")]
		public string AttributeValue { get; set; }
	}

	public class TariffAttributeRepository
	{
		static TariffAttributeRepository instance;
		public static TariffAttributeRepository Instance => instance ?? (instance = new TariffAttributeRepository());

		public static List<TariffAttribute> GetAll()
		{
			// Important: Don't use excel to edit and save TariffCode.csv!
			// It will make the TarrifCode column change to number and delete the prefix 0 automatically that will cause wrong output with MisMatch problem.
			var assembly = typeof(TariffAttribute).Assembly;
			var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("TariffAttribute.csv", System.StringComparison.Ordinal));
			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			using (var streamReader = new StreamReader(resourceStream))
			using (var csv = new CsvReader(streamReader))
			{
				csv.Configuration.HasHeaderRecord = true;
				return csv.GetRecords<TariffAttribute>().ToList();
			}
		}

		public IEnumerable<TariffAttribute> GetByTariffCode(string tariffCode)
		{
			return AllAttributes.Where(a => tariffCode.StartsWith(a.TariffCode, System.StringComparison.Ordinal));
		}

		List<TariffAttribute> allAttributes;
		public List<TariffAttribute> AllAttributes
		{
			get
			{
				if (allAttributes == null)
				{
					allAttributes = GetAll();
				}
				return allAttributes;
			}
		}
	}
}
