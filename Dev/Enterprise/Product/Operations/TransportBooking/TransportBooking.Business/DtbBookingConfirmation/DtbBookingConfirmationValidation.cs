//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTransportBookingConfirmationValidation
//
//    This class should be used for overriding validation in AutoTransportBookingConfirmationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingConfirmationValidation : Common.DtbBookingConfirmationValidation
	{
		public DtbBookingConfirmationValidation(DtbBookingConfirmation parent)
			: base(parent)
		{
		}

		DtbBookingConfirmation Confirmation
		{
			get { return (DtbBookingConfirmation)Parent; }
		}

		public void ValidateParentID_InstructionOrPackageDivot()
		{
			ValidateCalculatedProperty(Confirmation.ParentID_InstructionOrPackageDivotInfo);
		}

		void CheckParentID_InstructionOrPackageDivot()
		{
			MandatoryValidation.CheckEntered(Confirmation.ParentID_InstructionOrPackageDivotInfo);
			ListValidation.ErrorIfInvalidPK(Confirmation.ParentID_InstructionOrPackageDivotInfo);
		}

		public void ValidateConfirmationDescription()
		{
			ValidateCalculatedProperty(Confirmation.ConfirmationDescriptionInfo);
		}

		void CheckConfirmationDescription()
		{
			MandatoryValidation.CheckEntered(Confirmation.ConfirmationDescriptionInfo);
			ListValidation.ErrorIfInvalidCode(Confirmation.ConfirmationDescriptionInfo);
		}

		protected override void CheckKK_RequiredFrom()
		{
			base.CheckKK_RequiredFrom();

			if (!Parent.KK_RequiredFromInfo.HasErrors() && IsRequiredFromLaterThanRequiredTo)
			{
				Parent.KK_RequiredFromInfo.AddError(Res.GetString("DtbBookingConfirmationValidation|KK_RequiredFromEarlierThanKK_RequiredTo", "'Required From' needs to be earlier than 'Required To'."));
			}

			if (Confirmation.Instruction?.Booking != null && Confirmation.Instruction.Booking.IsSendingXUSToCTO && Confirmation.KK_RequiredFrom.IsEmpty &&
				(Confirmation.IsPickUp || Confirmation.IsDelivery))
			{
				Confirmation.KK_RequiredFromInfo.AddMessageError(Res.GetString("DtbBookingConfirmationValidation|SendingXUSToCTO_KK_RequiredFromTo", "This Booking must have Required From and Required To entered for all Confirmations."));
			}
		}

		protected override void CheckKK_RequiredTo()
		{
			base.CheckKK_RequiredTo();

			if (!Parent.KK_RequiredToInfo.HasErrors() && IsRequiredFromLaterThanRequiredTo)
			{
				Parent.KK_RequiredToInfo.AddError(Res.GetString("DtbBookingConfirmationValidation|KK_RequiredToLaterThanKK_RequiredFrom", "'Required To' needs to be later than 'Required From'."));
			}

			if (Confirmation.Instruction?.Booking != null && Confirmation.Instruction.Booking.IsSendingXUSToCTO && Confirmation.KK_RequiredTo.IsEmpty &&
				(Confirmation.IsPickUp || Confirmation.IsDelivery))
			{
				Confirmation.KK_RequiredToInfo.AddMessageError(Res.GetString("DtbBookingConfirmationValidation|SendingXUSToCTO_KK_RequiredFromTo", "This Booking must have Required From and Required To entered for all Confirmations."));
			}
		}

		bool IsRequiredFromLaterThanRequiredTo
		{
			get { return Parent.KK_RequiredFrom.IsValid && Parent.KK_RequiredTo.IsValid && Parent.KK_RequiredFrom > Parent.KK_RequiredTo; }
		}

		protected override void CheckKK_Estimated()
		{
			base.CheckKK_Estimated();

			if (Parent.KK_Estimated.IsValid)
			{
				CheckDate(Parent.KK_EstimatedInfo, (i) => i.Estimated);
			}
		}

		protected override void CheckKK_Actual()
		{
			base.CheckKK_Actual();

			if (Parent.KK_Actual.IsValid)
			{
				CheckDate(Parent.KK_ActualInfo, (i) => i.Actual);
			}
		}

		void CheckDate(ZPropertyInfo dateInfo, System.Func<DtbBookingInstruction, ZDateTime> getDate)
		{
			var bookingConfirmation = (DtbBookingConfirmation)Parent;
			var instruction = bookingConfirmation.Instruction;
			var sourceName = Res.GetString("DtbBookingConfirmation|Name", "Confirmation");

			DtbBookingInstructionValidation.CheckDateOnInstructionsForRelatedPackage(dateInfo, instruction, sourceName, getDate);
		}

		protected override void CheckKK_IsEmptyContainer()
		{
			base.CheckKK_IsEmptyContainer();

			if (Confirmation.HasPackages && Parent.KK_IsEmptyContainer && !Parent.KK_IsEmptyContainerInfo.HasErrors() && !Confirmation.IsContainerised)
			{
				Parent.KK_IsEmptyContainerInfo.AddError(Res.GetString("29523acd-374a-40e4-baa4-2d4c05d3bc3f", "Only Containers can be Empty."));
			}
		}

		protected override void CheckKK_ReferenceNum()
		{
			base.CheckKK_ReferenceNum();

			var firstInstruction = Confirmation.Booking?.Instructions.FirstOrDefault(i => i.KN_Sequence == 1);
			var needToCheck = InstructionApplicable(firstInstruction)
				&& InstructionApplicable(Confirmation.Instruction)
				&& Confirmation.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp
				&& Confirmation.Booking.IsSendingXUSToCTO;

			if (needToCheck && string.IsNullOrEmpty(Confirmation.KK_ReferenceNum))
			{
				Confirmation.KK_ReferenceNumInfo.AddMessageError(Res.GetString("DtbBookingConfirmation|KK_ReferenceNumber", "Confirmation must have Reference Number"));
			}

			bool InstructionApplicable(DtbBookingInstruction instruction)
			{
				var jobDirection = (string)instruction?.Booking?.ConsolidationSingleJob?.KB_JobDirection;
				return instruction != null
					&& instruction.KN_InstructionType == InstructionTypes.Codes.PickUp
					&& (
						(jobDirection == nameof(DtbBookingDirection.PIC) && instruction.OrganisationType == OrganisationTypesList.Codes.CYD) ||
						(jobDirection == nameof(DtbBookingDirection.DLV) && instruction.OrganisationType == OrganisationTypesList.Codes.CTO)
					);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateParentID_InstructionOrPackageDivot();
			ValidateConfirmationDescription();
		}
	}
}
