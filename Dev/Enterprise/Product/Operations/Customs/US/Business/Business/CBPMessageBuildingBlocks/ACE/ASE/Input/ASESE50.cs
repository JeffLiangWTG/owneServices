using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE50 : Abstract.ASESE50, IACEBIRDOrgCompanyRecord
	{
		#region IACEBIRDOrgCompanyRecord Members

		ZString IACEBIRDOrgCompanyRecord.OrganizationType
		{
			get { return EntityCode; }
		}

		ZString IACEBIRDOrgCompanyRecord.CustomsNoType
		{
			get { return BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier(EntityIdentifierQualifier); }
		}

		ZString IACEBIRDOrgCompanyRecord.CustomsNumber
		{
			get { return EntityIdentifier; }
		}

		ZString IACEBIRDOrgCompanyRecord.CompanyName
		{
			get { return EntityName; }
		}

		#endregion
	}
}
