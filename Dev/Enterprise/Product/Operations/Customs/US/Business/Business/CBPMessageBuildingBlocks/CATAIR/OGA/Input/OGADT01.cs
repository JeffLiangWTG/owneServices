using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGADT01 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGADT01, IBIRDOGALineRecord, IBIRDOGALineIDRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			DOT dotLine = (DOT)ogaLine;

			dotLine.US_DOTBoxNo = BoxNumber;
			dotLine.US_DOTClarCode = ClarificationCode;
			dotLine.US_DOTPassport = PassportNumber;
			dotLine.US_DOTCountryOfOrigin = CountryISO;
			dotLine.US_DOTBondSuretyCode = DOTBondSuretyCode;
			dotLine.US_DOTPriorApproval = NHTSAPermissionLetterOfficialOrdersCertification == "Y";
			dotLine.US_DOTImpSubstStatement = ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter == "Y";
			dotLine.US_DOTTireID = TireManufacturerIDCode;
			dotLine.US_DOTTireBrandName = TireManufacturerBrandName;
		}

		#endregion

		#region IBIRDOGALineIDRecord Members

		OGAType IBIRDOGALineIDRecord.OGAType
		{
			get { return OGAType.DOT; }
		}

		void IBIRDOGALineIDRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
		}

		#endregion
	}
}
