using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

 namespace Enterprise.TransportConsignment.DataTransfer
{
	class DtbCarrierBookingConsignmentAddressInstructionDataObjectWriter : DataObjectWriter<DtbConsignmentAddress, Note>
	{
		static readonly Dictionary<string,string> OrganizationTypesMapping = new Dictionary<string, string>()
		{
			["PIC"] = (NoResString)"Pickup instructions",
			["DLV"] = (NoResString)"Delivery instructions"
		};

		public DtbCarrierBookingConsignmentAddressInstructionDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Note PopulateDataObject(DtbConsignmentAddress conAddress)
		{
			JobDocAddress docAddressBO = conAddress.Address;
			if (docAddressBO == null || docAddressBO.IsEmpty)
			{
				return null;
			}
			//might need to set this up properly
			string docAddressType = "";
			string jobdocType = conAddress.LTS_InstructionType.ToString().ToUpper();
			if (OrganizationTypesMapping.ContainsKey(jobdocType))
			{
				docAddressType = OrganizationTypesMapping[jobdocType];
			}
			Note note = new Note();
			note.NoteText = conAddress.LTS_Notes.ToString();
			note.Description = docAddressType;
			return note;
		}
	}
}
