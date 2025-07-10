using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Environment;
using ICompany = Enterprise.DocumentVisualizer.DocDataObjects.ICompany;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Company : ICompany
	{
		public Company(GlbCompany company)
		{
			this.company = company;
		}

		readonly GlbCompany company;

		public ZString Code => company?.GC_Code ?? ZString.Empty;

		public ZString Name => company?.GC_Name ?? ZString.Empty;

		public ZGuid PK => company?.PK ?? ZGuid.Empty;

		public DocumentVisualizer.DocDataObjects.ICountry Country => country ?? (country = new Country(company?.Country));
		Country country;

		public ZString LicenceCode => company?.GetLicenceCode() ?? ZString.Empty;

		public ZBool IsReciprocal => company?.GC_IsReciprocal ?? ZBool.False;

		public IOrganization Organization => organization ?? (organization = new Organization(company?.OrgProxy));
		Organization organization;
	}
}
