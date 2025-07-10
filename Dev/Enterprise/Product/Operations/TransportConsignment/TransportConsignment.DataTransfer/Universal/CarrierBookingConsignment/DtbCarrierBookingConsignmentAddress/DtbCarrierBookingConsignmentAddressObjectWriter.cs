using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer
{
	class DtbCarrierBookingConsignmentAddressDataObjectWriter : DataObjectWriter<DtbConsignmentAddress, OrganizationAddress>
	{
		static readonly Dictionary<string, DocAddressType> OrganizationTypesMapping = new Dictionary<string, DocAddressType>()
		{
			["PIC"] = DocAddressType.PickUpAddress,
			["DLV"] = DocAddressType.DropOffAddress,
			["RTS"] = DocAddressType.ReturnAddress
		};

		public DtbCarrierBookingConsignmentAddressDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override OrganizationAddress PopulateDataObject(DtbConsignmentAddress conAddress)
		{
			JobDocAddress docAddressBO = conAddress.Address;
			if (docAddressBO == null || docAddressBO.IsEmpty)
			{
				return null;
			}
			DocAddressType docAddressType = DocAddressType.None;
			string jobdocType = conAddress.LTS_InstructionType.ToString().ToUpper();
			if (OrganizationTypesMapping.ContainsKey(jobdocType))
			{
				docAddressType = OrganizationTypesMapping[jobdocType];
			}
			OrganizationAddress organizationAddress = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(conAddress.Address);
			organizationAddress.AddressType = docAddressType.ToString();
			return organizationAddress;
		}
	}
}
