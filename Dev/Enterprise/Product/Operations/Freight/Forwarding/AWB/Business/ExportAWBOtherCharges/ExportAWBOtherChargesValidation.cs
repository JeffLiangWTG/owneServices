using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBOtherChargesValidation : AutoExportAWBOtherChargesValidation
	{
		public ExportAWBOtherChargesValidation(AutoExportAWBOtherCharges parent)
			: base(parent)
		{
		}

		public new ExportAWBOtherCharges Parent
		{
			get { return (ExportAWBOtherCharges)base.Parent; }
		}

		protected override void CheckEO_ChargeCode()
		{
			base.CheckEO_ChargeCode();
			if (ShouldValidateMandatoryChargeCode)
			{
				CheckValueRequiredInElectronicTransmissionIsNotEmpty(Parent.EO_ChargeCodeInfo);
			}

			CheckValueRequiredInElectronicTransmissionIsValid(Parent.EO_ChargeCodeInfo);
		}

		protected virtual bool ShouldValidateMandatoryChargeCode
		{
			get { return true; }
		}

		protected override void CheckEO_EntitlementCode()
		{
			base.CheckEO_EntitlementCode();

			CheckValueRequiredInElectronicTransmissionIsNotEmpty(Parent.EO_EntitlementCodeInfo);
			CheckValueRequiredInElectronicTransmissionIsValid(Parent.EO_EntitlementCodeInfo);
		}

		protected override void CheckEO_Amount()
		{
			base.CheckEO_Amount();
			MandatoryValidation.WarnIfNotEntered(Parent.EO_AmountInfo);
		}

		#region Implementation

		protected virtual void CheckValueRequiredInElectronicTransmissionIsNotEmpty(ZPropertyInfo propertyInfo)
		{
			if (Parent.Master != null)
			{
				Parent.Master.Validation.ValidateFieldAddingMessageForElectronicTransmission(propertyInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(propertyInfo);
			}
		}

		protected virtual void CheckValueRequiredInElectronicTransmissionIsValid(ZPropertyInfo propertyInfo)
		{
			if (Parent.Master != null && Parent.Master.Validation.NotificationLevelForFieldsRequiredForElectronicTransmission == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(propertyInfo);
			}
		}

		#endregion
	}
}
