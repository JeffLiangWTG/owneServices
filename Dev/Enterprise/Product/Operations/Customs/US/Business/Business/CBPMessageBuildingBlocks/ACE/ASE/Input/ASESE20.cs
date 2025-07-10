using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE20 : Abstract.ASESE20, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			switch (ReferenceIdentifierQualifier)
			{
				case ReferenceIdentifierCodeList.Codes.ExpressConsignmentShipment:
					declaration.US_ExpConsign = YesNoDefaultList.Codes.Yes;
					break;
				case ReferenceIdentifierCodeList.Codes.SplitShipmentReleaseElectionCode:
					declaration.US_SESplitRel = ReferenceIdentifier;
					break;
				case ReferenceIdentifierCodeList.Codes.ElectedExamSite:
					declaration.US_US_NKCentralizedExamSite = ReferenceIdentifier;
					break;
				case ReferenceIdentifierCodeList.Codes.NonAMSBillOfLading:
					declaration.US_NonAMS = true;
					break;
				case ReferenceIdentifierCodeList.Codes.SuretyCode:
					declaration.US_SuretyCode = ReferenceIdentifier;
					break;
				case ReferenceIdentifierCodeList.Codes.GeneralOrderNumber:
					declaration.US_GeneralOrderNo = ReferenceIdentifier;
					break;
				case ReferenceIdentifierCodeList.Codes.BondAmount:
					declaration.US_BondAmount = ZDecimal.ParseSafe(ReferenceIdentifier, 0m);
					break;
				case ReferenceIdentifierCodeList.Codes.KnownImporterIndicator:
					var iorWrapper = declaration.IORWrapper;
					if (iorWrapper != null)
					{
						if (!iorWrapper.ZO_KnwImpInd.EqualsIgnoringCase(YesNoDefaultList.Codes.Yes))
						{
							notifications.AddWarning("The value for Importer of Record > Known Importer in this BIRD transaction is 'Y' and it is different to a value set in the organization. Please check and set it manually if required.");
						}
					}
					break;
			}
		}

		#endregion
	}
}
