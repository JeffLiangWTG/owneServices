using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class MovementHeaderWrapperValidation : ZValidation
	{
		public MovementHeaderWrapperValidation(BusinessObject parent) : base(parent)
		{
			this.parent = parent as MovementHeaderWrapper;
			this.zValidationInternals = this;
		}

		readonly MovementHeaderWrapper parent;
		readonly IValidationInternals zValidationInternals;

		public override Type AutoValidationType => typeof(MovementHeaderWrapperValidation);

		public override void ValidateAll()
		{
			ValidateEntryType();
			ValidateCarrierOrgPK();
			ValidateInBondCarrierAddress();
			ValidateDestination();
		}

		public void ValidateEntryType()
		{
			zValidationInternals.Validate(parent.EntryTypeInfo, new RunValidationInvoker(CheckEntryType));
		}

		protected void CheckEntryType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.EntryTypeInfo);
		}

		public void ValidateCarrierOrgPK()
		{
			zValidationInternals.Validate(parent.CarrierOrgPKInfo, new RunValidationInvoker(CheckCarrierOrgPKIsValidZGuid));
		}

		protected void CheckCarrierOrgPKIsValidZGuid()
		{
			TypeValidation.CheckValidGuid(parent.CarrierOrgPKInfo);
		}

		public void ValidateInBondCarrierAddress()
		{
			zValidationInternals.Validate(parent.InBondCarrierAddressInfo, new RunValidationInvoker(CheckInBondCarrierAddress));
		}

		protected void CheckInBondCarrierAddress()
		{
			TypeValidation.CheckValidGuid(parent.InBondCarrierAddressInfo);
		}

		public void ValidateDestination()
		{
			zValidationInternals.Validate(parent.DestinationInfo, new RunValidationInvoker(CheckDestination));
		}

		protected void CheckDestination()
		{
			ListValidation.MessageErrorIfInvalidCode(parent.DestinationInfo);
		}
	}
}
