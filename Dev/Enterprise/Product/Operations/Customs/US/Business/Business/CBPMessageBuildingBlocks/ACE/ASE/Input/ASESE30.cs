using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE30 : Abstract.ASESE30, IACEBIRDOrgCompanyRecord
	{
		#region IACEBIRDHeaderCompanyRecord Members

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
