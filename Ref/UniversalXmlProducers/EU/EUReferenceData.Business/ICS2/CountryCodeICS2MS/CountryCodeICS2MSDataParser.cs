using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Business
{
	public class CountryCodeICS2MSDataParser : CommonDataParser
	{
		public CountryCodeICS2MSDataParser() :
			base(Constants.CountryCodeICS2MS.RDEntityAttributeValue, Constants.CountryCodeICS2MS.CodeAttributeValue)
		{
		}
	}
}
