using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCACCase)]
	public class USCACCaseCollection : ActiveBusinessObjectCollection<USCACCase>
	{
		public USCACCaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public USCACCaseCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		public void DefaultFilters(ZString countryOfOrigin, ZString caseNumber, ZString casePrifix, params ZString[] tariffNumbers)
		{
			foreach (var filterBODefault in AddFilterBusinessObjectDefaultForADD_CVD.DefaultFilters(countryOfOrigin, caseNumber, casePrifix, tariffNumbers))
			{
				FilterBusinessObjectDefaults.Add(filterBODefault);
			}
		}
	}
}
