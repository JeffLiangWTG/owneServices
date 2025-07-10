using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceDocumentImageLookups : ZLookups
	{
		public ComplianceDocumentImageLookups(ComplianceDocumentImage parent)
			: base(parent)
		{
			this.Parent = parent;
			this.BizOFactory = new BusinessObjectFactory();
		}
		new readonly ComplianceDocumentImage Parent;

		readonly BusinessObjectFactory BizOFactory;

		public CodeDescriptionPairList ComplianceSubTypeList => AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(Parent.Country);

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
