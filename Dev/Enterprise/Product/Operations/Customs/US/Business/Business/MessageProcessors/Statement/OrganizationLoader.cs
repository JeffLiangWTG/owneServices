using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrganizationLoader
	{
		public OrganizationLoader(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public IEnumerable<OrgHeader> LoadAllOrganisationByCusCode(ZString orgCusCode)
		{
			ZQuery query = GetQueryOnOrgCusCode(orgCusCode);
			OrgCusCode[] orgCodes = factory.Load<OrgCusCode>(query);

			foreach (OrgCusCode one in orgCodes)
			{
				yield return one.Header;
			}
		}

		public OrgHeader LoadTop1OrganisationByCusCode(ZString orgCusCode)
		{
			var query = GetQueryOnOrgCusCode(orgCusCode);
			return factory.LoadTop1<OrgCusCode>(query)?.Header;
		}

		ZQuery GetQueryOnOrgCusCode(ZString orgCusCode)
		{
			ZString codeType = ZString.Empty;

			if (CBPAssignedNumberValidator.IsValidCBPAssignedNumber(orgCusCode))
			{
				codeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			}
			else if (EmployerIdentificationNumberValidator.IsValidEIN(orgCusCode))
			{
				codeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			}
			else if (SocialSecurityNumberValidator.IsValidSSN(orgCusCode))
			{
				codeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			}

			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, orgCusCode);
			if (!codeType.IsEmpty)
			{
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			}

			return query;
		}
	}
}
