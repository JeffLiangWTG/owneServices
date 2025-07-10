using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class AsycudaBillValidationForRegularBill
	{
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateShortStatusDescription();
			ValidateSG_PartyStatus();
			ValidateSG_PayeeIndicator();
			ValidateCycleDate();
			ValidateCycleNumber();
			ValidateCustomsJobNumber();
			ValidateGSTNReferenceNo();
		}

		public void ValidateCycleDate()
		{
			ValidateCalculatedProperty(Parent.CycleDateInfo);
		}

		protected void CheckCycleDate()
		{
			if (Parent.IsImport)
			{
				var cycleDate = Parent.CycleDate.Date;
				if (!cycleDate.IsEmpty)
				{
					TypeValidation.CheckValidSmallDateTime(Parent.CycleDateInfo);
					new DateRangeValidation().ValidateDateValueHasChanged(Parent.CycleDateInfo);
				}
			}
		}

		public void ValidateCycleNumber()
		{
			ValidateCalculatedProperty(Parent.CycleNumberInfo);
		}

		protected void CheckCycleNumber()
		{
			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CycleNumberInfo);
			}
		}

		public void ValidateSG_PartyStatus()
		{
			ValidateCalculatedProperty(Parent.SG_PartyStatusInfo);
		}

		protected void CheckSG_PartyStatus()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_PartyStatusInfo);
		}

		public void ValidateSG_PayeeIndicator()
		{
			ValidateCalculatedProperty(Parent.SG_PayeeIndicatorInfo);
		}

		protected void CheckSG_PayeeIndicator()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_PayeeIndicatorInfo);
		}

		public void ValidateShortStatusDescription()
		{
			ValidateCalculatedProperty(Parent.ShortStatusDescriptionInfo);
		}

		protected void CheckShortStatusDescription()
		{
			if (Parent.ABL_MessageStatus == MessageStatusCodeList.Codes.Error && Parent.ShortStatusDescription.EndsWith("...", StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.ShortStatusDescriptionInfo.AddWarning(Parent.StatusDescription);
			}
		}

		public void ValidateCustomsJobNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsJobNumberInfo);
		}

		protected void CheckCustomsJobNumber()
		{
			var parent = Parent;

			if (parent.IsIBGAccountLinked && parent.Packs.Cast<AsycudaPack>().Any(c => !c.HasValidTradeNetPermitNumber))
			{
				parent.CustomsJobNumberInfo.AddMessageError(ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			}
		}

		public void ValidateGSTNReferenceNo()
		{
			ValidateCalculatedProperty(Parent.GSTNReferenceNoInfo);
		}

		protected void CheckGSTNReferenceNo()
		{
			var parent = Parent;

			if (parent.GSTNReferenceNo.IsEmpty && (parent.Header?.IsOVRApplicable ?? false) && parent.Packs.Cast<AsycudaPack>().Any(c => !c.PackedItem.GSTPaid.IsEmpty))
			{
				parent.GSTNReferenceNoInfo.AddMessageError(ValidationConstants.Bill.GSTRegistrationNoMustNotBeBlank);
			}
		}
	}
}

