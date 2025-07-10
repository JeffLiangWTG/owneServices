using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class RecordFilter : IRecordFilter
	{
		IXmlFilter<measure> measureFilter;
		IXmlFilter<goodsNomenclature> goodsNomenclatureFilter;
		IXmlFilter<baseRegulation> baseRegulationFilter;
		IXmlFilter<modificationRegulation> modificationRegulationFilter;

		public RecordFilter SetMeasureFilter(IXmlFilter<measure> measureFilter)
		{
			this.measureFilter = measureFilter;
			return this;
		}

		public RecordFilter SetGoodsNomenclatureFilter(IXmlFilter<goodsNomenclature> goodsNomenclatureFilter)
		{
			this.goodsNomenclatureFilter = goodsNomenclatureFilter;
			return this;
		}

		public RecordFilter SetBaseRegulationFilter(IXmlFilter<baseRegulation> baseRegulationFilter)
		{
			this.baseRegulationFilter = baseRegulationFilter;
			return this;
		}

		public RecordFilter SetModificationRegulationFilter(IXmlFilter<modificationRegulation> modificationRegulationFilter)
		{
			this.modificationRegulationFilter = modificationRegulationFilter;
			return this;
		}

		public object GetValidValueFromRecord(record value)
		{
			if (value.Item is measure measure && measureFilter != null)
			{
				return measureFilter.GetValidValue(measure);
			}
			else if (value.Item is goodsNomenclature goodsNomenclature && goodsNomenclatureFilter != null)
			{
				return goodsNomenclatureFilter.GetValidValue(goodsNomenclature);
			}
			else if (value.Item is baseRegulation baseRegulation && baseRegulationFilter != null)
			{
				return baseRegulationFilter.GetValidValue(baseRegulation);
			}
			else if (value.Item is modificationRegulation modificationRegulation && modificationRegulationFilter != null)
			{
				return modificationRegulationFilter.GetValidValue(modificationRegulation);
			}
			return null;
		}

		public EUNCommonImportTariffPopulator.record GetValidValue(record value)
		{
			return value;
		}

		public bool IsValid(record value, DateTime publicationDate)
		{
			if (value.Item is measure measure && measureFilter != null)
			{
				return value.Item != null && measureFilter.IsValid(measure, publicationDate);
			}
			else if (value.Item is goodsNomenclature goodsNomenclature && goodsNomenclatureFilter != null)
			{
				return value.Item != null && goodsNomenclatureFilter.IsValid(goodsNomenclature, publicationDate);
			}
			else if (value.Item is baseRegulation baseRegulation && baseRegulationFilter != null)
			{
				return value.Item != null && baseRegulationFilter.IsValid(baseRegulation, publicationDate);
			}
			else if (value.Item is modificationRegulation modificationRegulation && modificationRegulationFilter != null)
			{
				return value.Item != null && modificationRegulationFilter.IsValid(modificationRegulation, publicationDate);
			}
			return false;
		}

		public bool IsMeasure(record value)
		{
			return value.Item is measure;
		}

		public bool IsGoodsNomenclature(record value)
		{
			return value.Item is goodsNomenclature;
		}

		public bool IsBaseRegulation(record value)
		{
			return value.Item is baseRegulation;
		}

		public bool IsModificationRegulation(record value)
		{
			return value.Item is modificationRegulation;
		}
	}
}
