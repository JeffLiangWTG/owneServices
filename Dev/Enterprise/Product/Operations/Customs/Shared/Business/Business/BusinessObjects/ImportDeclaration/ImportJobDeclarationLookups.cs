using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class ImportJobDeclarationLookups : ZLookups
	{
		public ImportJobDeclarationLookups(ImportJobDeclaration parent) : base(parent)
		{
			this.importDeclaration = parent;
		}

		public BaseJobDeclarationCollection DeclarationList
		{
			get { return new BaseJobDeclarationCollection(Factory, importDeclaration.CountryCode); }
		}

		public RefCountryCollection CountryList
		{
			get
			{
				if (fCountryList == null)
				{
					fCountryList = new RefCountryCollection(Factory);
				}

				return fCountryList;
			}
		}
		RefCountryCollection fCountryList;

		readonly ImportJobDeclaration importDeclaration;
	}
}
