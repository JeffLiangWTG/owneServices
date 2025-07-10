using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class CRLH2 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.CRLH2, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_US_NKLocationOfGoods = LocationOfGoods;

			declaration.JE_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, UltimateConsigneeNumber, "ultimate consignee", notifications);

			if (declaration.JE_OH_Importer.IsEmpty)
			{
				declaration.JE_OH_Importer = declaration.ConsigneeAddressOrgPK;
			}

			declaration.US_EntryDateElectionCode = EntryDateElectionCode;

			declaration.JE_VoyageFlightNo = VoyageFlightTripManifestNumber;

			if (VesselNameForeignTradeZoneNumber.StartsWith("FTZ", StringComparison.OrdinalIgnoreCase))
			{
				declaration.JE_MasterBill = VesselNameForeignTradeZoneNumber;
			}
			else
			{
				declaration.JE_VesselName = VesselNameForeignTradeZoneNumber;
			}
		}

		#endregion
	}
}
