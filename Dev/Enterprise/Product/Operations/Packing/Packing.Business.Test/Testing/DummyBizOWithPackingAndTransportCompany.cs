using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.Business.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyBizOWithPackingAndTransportCompany : DummyWithPacking, IPackingParentWithTransportCompanyAndBookingParty
	{
		public DummyBizOWithPackingAndTransportCompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GetTransportCompany

		public OrgAddress GetTransportCompany(string packageId, string partyType) => TransportCompanyForTest;

		public OrgAddress GetBookingParty(string packageId, string partyType) => BookingPartyForTest;

		public OrgAddress TransportCompanyForTest { private get; set; }
		public OrgAddress BookingPartyForTest { private get; set; }

		#endregion

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;
	}
}
