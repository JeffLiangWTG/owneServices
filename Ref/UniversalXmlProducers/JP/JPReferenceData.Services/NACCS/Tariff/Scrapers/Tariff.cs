using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class Tariff : Scraper
	{
		public Tariff(IWebSourceProvider sourceProvider, string url) : base(sourceProvider, url)
		{
		}

		public int Section { get; set; }

		TariffCode TariffCode { get; set; }

		List<Scraper> OtherChildScrapers { get; } = new List<Scraper>();

		public override void Load()
		{
			base.Load();

			TariffCode = new TariffCode(SourceProvider, Url) { Document = Document, ChapterCompositeKeys = ChapterCompositeKeys };

			OtherChildScrapers.Clear();

			if (IsImport)
			{
				OtherChildScrapers.Add(new TariffRate(Url) { Document = Document });
				OtherChildScrapers.Add(new TariffRateEpa(Url) { Document = Document });
				OtherChildScrapers.Add(new TariffUnit(Url) { Document = Document });
			}
		}

		public static DynamicCsvRecord Header { get => DynamicCsvRecord.Combine(TariffCode.Header, TariffRate.Header, TariffRateEpa.Header, TariffUnit.Header); }

		public Dictionary<string, string> ChapterCompositeKeys { get; set; }

		void Combine()
		{
			var count = TariffCode.CompositeKeys.Count;
			var index = 0;

			foreach (var record in TariffCode.Records)
			{
				if (index < count)
				{
					if (index == 0 && TariffCode.CompositeKeys[0][0].Length == 2)
					{
						GlobalSections.TryAdd(Section, DynamicCsvRecord.Combine(record));
					}
					else
					{
						Records.Add(record);
					}
				}
				else
				{
					var childRecords = new List<DynamicCsvRecord> { record };
					childRecords.AddRange(OtherChildScrapers.Select(c => c.Record));

					Records.Add(DynamicCsvRecord.Combine(childRecords.ToArray()));
				}

				index++;
			}
		}

		public override void Translate()
		{
			try
			{
				Load();

				TariffCode.Translate();
				OtherChildScrapers.ForEach(x => x.Translate());

				Combine();
				Release();
			}
			catch (Exception e)
			{
				throw new UnhandledApplicationException($"Parsing Url [{Url}] is not expected.", e);
			}
		}
	}
}
