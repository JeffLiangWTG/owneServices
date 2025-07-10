using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class AEPAPG20 : Abstract.AEPAPG20, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord
	{
		#region IACEBIRDOrgAddress2Record Members

		ZString IACEBIRDOrgAddress2Record.Address2
		{
			get { return EntityAddress2; }
		}

		#endregion

		#region IACEBIRDOrgCountryRecord Members

		ZString IACEBIRDOrgCountryRecord.City
		{
			get { return EntityCity; }
		}

		ZString IACEBIRDOrgCountryRecord.Country
		{
			get { return EntityCountry; }
		}

		ZString IACEBIRDOrgCountryRecord.ZipCode
		{
			get { return EntityZipPostalCode; }
		}

		#endregion
	}
}
