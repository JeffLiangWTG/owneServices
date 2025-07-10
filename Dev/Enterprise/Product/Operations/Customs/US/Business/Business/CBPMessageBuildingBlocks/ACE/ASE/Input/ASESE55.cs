using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE55 : Abstract.ASESE55, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record
	{
		#region IACEBIRDOrgAddressRecord Members

		ZString IACEBIRDOrgAddressRecord.Address1
		{
			get { return AddressInformation; }
		}

		#endregion

		#region IACEBIRDOrgAddress2Record Members

		ZString IACEBIRDOrgAddress2Record.Address2
		{
			get { return AddressInformation1; }
		}

		#endregion
	}
}
