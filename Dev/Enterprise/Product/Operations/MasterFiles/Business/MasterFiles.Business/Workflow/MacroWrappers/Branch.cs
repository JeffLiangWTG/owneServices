using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Branch : IBranch
	{
		public Branch(GlbBranch branch, GlbCompany company)
		{
			this.branch = branch;
			this.company = company;
		}

		readonly GlbBranch branch;
		readonly GlbCompany company;

		public ZString Name => branch?.GB_BranchName ?? ZString.Empty;

		public ZString City => branch?.GB_City ?? ZString.Empty;

		public ZString Code => branch?.GB_Code ?? ZString.Empty;

		public ZGuid PK => branch?.PK ?? ZGuid.Empty;

		public IUnloco HomePort => homePort ?? (homePort = new Unloco(branch?.HomePort));
		Unloco homePort;

		public ICountry Country => country ?? (country = new Country(branch?.Country));
		Country country;

		public IOrganization Organization
		{
			get
			{
				if (organization == null)
				{
					if (branch != null && !branch.IsNull && !branch.GB_OH_OrgProxy.IsEmpty)
					{
						organization = new Organization(branch?.OrgProxy);
					}
					else
					{
						organization = new Organization(company?.OrgProxy);
					}
				}
				return organization;
			}
		}
		Organization organization;

		public string PortCode => HomePort.Code;

		public string PortName => HomePort.Name;

		public string PortCountry
		{
			get
			{
				var result = "";
				var orgProxy = branch?.OrgProxy;
				if (orgProxy != null)
				{
					result = orgProxy.UNLOCO != null
						? (orgProxy.UNLOCO.Country != null ? orgProxy.UNLOCO.Country.RN_DescMultilingual : "") : "";
				}

				return result;
			}
		}
	}
}
