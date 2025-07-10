using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class BCR01 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.BCR01, IBIRDHeaderRecord, IBIRDHeaderIDRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_SchDEntry = DistrictPortOfEntry;

			declaration.US_EntryFilerCode = FilerCode;

			declaration.ImportEntryNumber = EntryNumber;

			BIRDTransportMode.SetTransportMode(declaration, ModeOfTransportationMOTCode, notifications);

			declaration.IOROrgPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, ImporterOfRecord, "importer of record", notifications)?.PK ?? ZGuid.Empty;

			string bondType = BondType.ToString();
			declaration.US_BondType = bondType != BondTypeList.Codes.NoBondRequired ? bondType : string.Empty;

			declaration.US_SuretyCode = SuretyCode;

			declaration.JE_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, UltimateConsignee, "ultimate consignee", notifications);

			declaration.US_EntryDate = DateOfArrival;

			declaration.US_EntryType = EntryType;

			declaration.US_UI_NKCarrierSCAC = CarrierCode;

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsBorderCargoRelease)
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
			get { return FilerCode; }
		}

		#endregion
	}
}
