using System.Linq;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISSourceCollection : DependentCusAddInfoCollection<APHISSource, APHISHeader>
	{
		public APHISSourceCollection(APHISHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISSource)
		{
		}

		public void AddCountryOfSpeciesOriginElementIfRequired()
		{
			if (Master.IsAACProgramType)
			{
				var invoiceLine = Master.Parent;
				if (invoiceLine == null || !invoiceLine.IsDataImportInProgress)
				{
					if (this.Count == 0 || this.Cast<APHISSource>().All(x => x.US_SourceTypeCode != SourceTypeCodesList.Codes.CountryOfSpeciesOrigin))
					{
						var newLine = this.AddNew();
						newLine.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
						if (invoiceLine != null)
						{
							newLine.US_CountryCode = invoiceLine.US_UC_NKCountryOfOrigin;
						}
					}
				}
			}
		}
		public bool HasSourceType39or267 => this.Cast<APHISSource>().Any(x => x.US_SourceTypeCode == SourceTypeCodesList.Codes.CountryOfSpeciesOrigin || x.US_SourceTypeCode == SourceTypeCodesList.Codes.CountryOfProduction);
		public bool HasSourceType262HRV267 => this.Cast<APHISSource>().Any(x => x.US_SourceTypeCode == SourceTypeCodesList.Codes.PlaceOfGrowth || x.US_SourceTypeCode == SourceTypeCodesList.Codes.Harvested || x.US_SourceTypeCode == SourceTypeCodesList.Codes.CountryOfSpeciesOrigin);
		public bool HasSourceCountryCA => this.Cast<APHISSource>().Any(x => x.US_CountryCode == Core.Constants.CountryCodes.Canada);
	}
}
