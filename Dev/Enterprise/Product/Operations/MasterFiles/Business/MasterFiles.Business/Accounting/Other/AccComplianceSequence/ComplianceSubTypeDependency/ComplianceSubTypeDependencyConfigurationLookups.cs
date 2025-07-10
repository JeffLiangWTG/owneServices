using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeDependencyConfigurationLookups : ZLookups
	{
		public ComplianceSubTypeDependencyConfigurationLookups(ComplianceSubTypeDependencyConfiguration parent)
			: base(parent)
		{
			this.Parent = parent;
			this.BizOFactory = new BusinessObjectFactory();
		}
		new readonly ComplianceSubTypeDependencyConfiguration Parent;

		readonly BusinessObjectFactory BizOFactory;

		public CodeDescriptionPairList SubTypeList
		{
			get
			{
				return AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(Parent.Country);
			}
		}

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(BizOFactory);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;
	}
}
