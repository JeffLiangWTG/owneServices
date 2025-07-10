using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class FTZAddInfoJobComInvoiceHeaderValidation : CommonImportAddInfoJobComHeaderValidation
	{
		public FTZAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateInvoiceLineForTemporaryDeposit();
		}

		void ValidateInvoiceLineForTemporaryDeposit()
		{
			if (IsFTZAdmissionValidationMode && Parent.JobDeclaration.IsTemporaryDeposit)
			{
				Parent.AddRowWarning(InvoiceLineIsNotRequiredForTemporaryDeposit);
			}
		}
		internal const string InvoiceLineIsNotRequiredForTemporaryDeposit = "Please be advised that Invoice lines are not required for Temporary Deposit and will not be sent in the message.";

		protected override void CheckUS_SplitShipmentDetail()
		{
			base.CheckUS_SplitShipmentDetail();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SplitShipmentDetailInfo, Parent.AddInfoLookups.SplitShipmentDetailsList);

			if (Parent.Bill is Bill bill && bill.US_SESplitShip)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SplitShipmentDetailInfo);
			}
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return Parent.JobDeclaration.IsFTZAdmissionValidationMode; }
		}
	}
}
