
namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ShipmentPortMessagingForDakosyValidation : JobShipmentPortMessagingValidation
	{
		public ShipmentPortMessagingForDakosyValidation(ShipmentPortMessaging parent)
			: base(parent)
		{
		}

		protected override void CheckJSM_EntryType()
		{
			base.CheckJSM_EntryType();
			PortMessagingValidationHelper.CheckEntryType(Parent.JSM_EntryTypeInfo);
		}

		protected override void CheckJSM_MovementReferenceNumber()
		{
			base.CheckJSM_MovementReferenceNumber();
			PortMessagingValidationHelper.CheckMovementReferenceNumber(Parent.JSM_MovementReferenceNumberInfo);
		}

		protected override void CheckJSM_MovementReferenceNumberComplete()
		{
			base.CheckJSM_MovementReferenceNumberComplete();
			PortMessagingValidationHelper.CheckMovementReferenceNumberComplete(Parent.JSM_MovementReferenceNumberCompleteInfo);
		}

		protected override void CheckJSM_ATBNumber()
		{
			base.CheckJSM_ATBNumber();
			PortMessagingValidationHelper.CheckATBNumber(Parent.JSM_ATBNumberInfo);
		}

		protected override void CheckJSM_ExemptionReason()
		{
			base.CheckJSM_ExemptionReason();
			PortMessagingValidationHelper.CheckExemptionReason(Parent.JSM_ExemptionReasonInfo);
		}

		protected override void CheckJSM_Annex30AType()
		{
			base.CheckJSM_Annex30AType();
			PortMessagingValidationHelper.CheckAnnex30AType(Parent.JSM_Annex30ATypeInfo);
		}

		protected override void CheckJSM_Annex30AFailureProcess()
		{
			base.CheckJSM_Annex30AFailureProcess();
			PortMessagingValidationHelper.CheckAnnex30AFailureProcess(Parent.JSM_Annex30AFailureProcessInfo);
		}

		protected override void CheckJSM_ExportDeclarationReference()
		{
			base.CheckJSM_ExportDeclarationReference();
			PortMessagingValidationHelper.CheckExportDeclarationReference(Parent.JSM_ExportDeclarationReferenceInfo);
		}

		protected override void CheckJSM_CustomsReleaseDate()
		{
			base.CheckJSM_CustomsReleaseDate();
			PortMessagingValidationHelper.CheckCustomsReleaseDate(Parent.JSM_CustomsReleaseDateInfo);
		}

		protected override void CheckJSM_LocalReferenceNumber()
		{
			base.CheckJSM_LocalReferenceNumber();
			PortMessagingValidationHelper.CheckLocalReferenceNumber(Parent.JSM_LocalReferenceNumberInfo);
		}

		protected override void CheckJSM_LocalReferenceNumberComplete()
		{
			base.CheckJSM_LocalReferenceNumberComplete();
			PortMessagingValidationHelper.CheckLocalReferenceNumberComplete(Parent.JSM_LocalReferenceNumberCompleteInfo);
		}
	}
}
