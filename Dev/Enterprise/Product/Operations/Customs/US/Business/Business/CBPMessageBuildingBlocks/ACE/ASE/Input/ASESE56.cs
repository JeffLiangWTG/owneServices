using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE56 : Abstract.ASESE56, IACEBIRDOrgCountryRecord
	{
		#region IACEBIRDOrgCountryRecord

		ZString IACEBIRDOrgCountryRecord.City
		{
			get { return CityName; }
		}

		ZString IACEBIRDOrgCountryRecord.Country
		{
			get { return CountryCode; }
		}

		ZString IACEBIRDOrgCountryRecord.ZipCode
		{
			get { return PostalCode; }
		}

		#endregion
	}
}
