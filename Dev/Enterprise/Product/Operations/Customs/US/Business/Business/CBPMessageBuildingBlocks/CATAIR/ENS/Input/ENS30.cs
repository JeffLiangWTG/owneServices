using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS30 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS30, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, CargoWise.ComponentModel.INotifications notifications)
		{
			declaration.US_WHSEntryFilerCode = EntryFilerCodeOfWarehouseEntry;
			declaration.US_WHSEntryNumber = WarehouseEntryNumber;
			declaration.US_WHSDistrictPortCode = DistrictPortCodeOfWarehouseEntry;
			declaration.US_IsFinalWHS = FinalWarehouseIndicator == "1";

			declaration.US_CertifyCargoRelease = ReleaseCertificationCode == 1;

			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalIndicator;
			declaration.US_SchDExam = DesignatedExamPort;

			declaration.US_PeriodicStatementMM = PeriodicStatementMonth;
			declaration.US_PaymentType = PaymentTypeIndicator;
			declaration.US_PreliminaryStatementPrintDate = PreliminaryStatementPrintDate;
			declaration.US_UI_NKCarrierSCAC = CarrierCode;
			declaration.US_TeamNo = TeamNumber;
		}

		#endregion
	}
}
