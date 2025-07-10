using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class AEPAPG19 : Abstract.AEPAPG19, IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord
	{
		#region IACEBIRDOrgCompanyRecord Members

		ZString IACEBIRDOrgCompanyRecord.CompanyName
		{
			get { return EntityName; }
		}

		ZString IACEBIRDOrgCompanyRecord.CustomsNoType
		{
			get { return EntityIdentificationCode; }
		}

		ZString IACEBIRDOrgCompanyRecord.CustomsNumber
		{
			get { return EntityNumber; }
		}

		public ZString OrganizationType
		{
			get { return EntityRoleCode; }
		}

		#endregion

		#region IACEBIRDOrgAddressRecord Members

		ZString IACEBIRDOrgAddressRecord.Address1
		{
			get { return EntityAddress1; }
		}

		#endregion
	}
}
