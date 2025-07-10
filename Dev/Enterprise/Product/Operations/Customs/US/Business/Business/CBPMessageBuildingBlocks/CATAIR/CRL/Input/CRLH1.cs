using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class CRLH1 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.CRLH1, IBIRDHeaderRecord, IBIRDHeaderIDRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_SchDEntry = DistrictPortOfEntry;
			declaration.US_EntryFilerCode = EntryFilerCode;
			declaration.ImportEntryNumber = EntryNumber;

			declaration.IOROrgPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, ImporterNumber, "importer of record", notifications)?.PK ?? ZGuid.Empty;

			BIRDTransportMode.SetTransportMode(declaration, ModeOfTransportationMOTCode, notifications);

			declaration.US_EntryDate = EstimatedDateOfArrival;

			declaration.US_BondType = BondTypeCode != BondTypeList.Codes.NoBondRequired ? BondTypeCode : ZString.Empty;

			declaration.US_CertifyCargoRelease = ReleaseCertificationCode == 1;

			declaration.US_PresentationDate = PresentationDate;

			declaration.US_UI_NKCarrierSCAC = CarrierCode;

			declaration.US_SchDArrival = DistrictPortOfUnlading;

			declaration.US_EntryType = EntryType;

			declaration.US_SuretyCode = SuretyCode;

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsCargoRelease)
				{
					entry.US_UseConsigneeNameAddress = ConsigneeNameAndAddress == "1";
					break;
				}
			}
		}

		#endregion

		#region IBIRDHeaderIDRecord Members

		ZString IBIRDHeaderIDRecord.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IBIRDHeaderIDRecord.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		#endregion
	}
}
