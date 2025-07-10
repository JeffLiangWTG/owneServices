using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageSendingObjectValidation : AutoInBondMenuItemMessageSendingObjectValidation
	{
		public InBondMenuItemMessageSendingObjectValidation(AutoInBondMenuItemMessageSendingObject autoSendArrivaOperationalActionBO)
			: base(autoSendArrivaOperationalActionBO)
		{
		}

		protected new InBondMenuItemMessageSendingObject Parent => (InBondMenuItemMessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateQPMessageStatus();
			ValidateWPMessageStatus();
			ValidateArrivalFirmsCode();
		}

		protected override void CheckInBondNumber()
		{
			base.CheckInBondNumber();

			var parent = Parent;
			var factory = parent.Factory;
			var inbondNumber = parent.InBondNumber;
			var inBondNumberInfo = parent.InBondNumberInfo;
			InBondNumberValidationHelper.ValidateInBondNumber(factory, inbondNumber, inBondNumberInfo, GlbBranch.CurrentBranch, true, false, AllocateInBondNumber.Schema.ConventionalInBondNumberMaxLength, AllocateInBondNumber.Schema.PostDepartureMessageInbondNumberMaxLength, true);

			if (parent.Parent.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().Count(x => x.InBondNumber == inbondNumber) > 1)
			{
				inBondNumberInfo.AddError(ZString.Format(DuplicateInBondNumberFound, inbondNumber));
			}

			if (parent.MovementHeader == null)
			{
				var inBondMenuItemMessageData = parent.Parent;
				if (inBondMenuItemMessageData.IsPedimento || inBondMenuItemMessageData.IsPrintDocument)
				{
					inBondNumberInfo.AddError(MatchingMovementHeaderNoFound);
				}
				else if (!Env.Security.USInBondNew.IsAllowed)
				{
					inBondNumberInfo.AddError(Env.Security.USInBondNew.ErrorMessageForNotAllowed);
				}
			}
		}
		internal const string DuplicateInBondNumberFound = "There are multiple movement headers found with same In-Bond number [{0}] in the list.";
		internal const string MatchingMovementHeaderNoFound = "The matching movement header could not be found.";

		protected override void CheckUSDestinationPortCode()
		{
			base.CheckUSDestinationPortCode();
			var parent = Parent;
			if (!parent.Parent.IsPedimento)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.USDestinationPortCodeInfo, parent.Lookups.RegionDistrictPorts);
			}
		}

		protected override void CheckForeignDestinationPortCode()
		{
			base.CheckForeignDestinationPortCode();
			var parent = Parent;
			if (!parent.Parent.IsPedimento)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.ForeignDestinationPortCodeInfo, parent.Lookups.ForeignPorts);
			}
		}

		protected override void CheckInBondCarrierCodeSCAC()
		{
			base.CheckInBondCarrierCodeSCAC();
			var parent = Parent;
			if (!parent.Parent.IsPedimento)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.InBondCarrierCodeSCACInfo, parent.Lookups.CarrierCollection);
			}
		}

		public void ValidateQPMessageStatus()
		{
			ValidateCalculatedProperty(Parent.QPMessageStatusInfo);
		}

		protected void CheckQPMessageStatus()
		{
			var parent = Parent;
			if (!parent.Parent.IsPedimento && parent.MovementHeader is CusInBondMoveHeader moveHeader && moveHeader.IsWaitingForResponse)
			{
				parent.QPMessageStatusInfo.AddMessageError(WaitForResponseMessageText);
			}
		}

		public void ValidateWPMessageStatus()
		{
			ValidateCalculatedProperty(Parent.WPMessageStatusInfo);
		}

		protected void CheckWPMessageStatus()
		{
			var parent = Parent;
			if (!parent.Parent.IsPedimento && parent.MovementHeader is CusInBondMoveHeader moveHeader && moveHeader.IsWaitingForResponse)
			{
				parent.WPMessageStatusInfo.AddMessageError(WaitForResponseMessageText);
			}
		}
		internal const string WaitForResponseMessageText = "This movement header is waiting for customs response.";

		protected override void CheckEntryType()
		{
			base.CheckEntryType();

			var parent = Parent;
			if (parent.Parent.IsExport &&
				parent.EntryType != InbondCommonTypeList.Codes._2TransportandExport &&
				parent.EntryType != InbondCommonTypeList.Codes._3ImmediateExport)
			{
				parent.EntryTypeInfo.AddMessageError($"Entry type should be {InbondCommonTypeList.Codes._2TransportandExport} or {InbondCommonTypeList.Codes._3ImmediateExport}.");
			}
		}

		protected override void CheckPedimentoNumber()
		{
			base.CheckPedimentoNumber();

			var parent = Parent;
			if (parent.Parent.IsPedimento)
			{
				MandatoryValidation.CheckEntered(parent.PedimentoNumberInfo);
			}
		}

		public void ValidateArrivalFirmsCode()
		{
			ValidateCalculatedProperty(Parent.ArrivalFirmsCodeInfo);
		}

		protected void CheckArrivalFirmsCode()
		{
			var parent = Parent;

			if (!parent.ArrivalFirmsCode.IsEmpty && parent.MovementHeader is CusInBondMoveHeader movementHeader && !movementHeader.IsAir)
			{
				var message = ResString.GetMultilingualString("F27CF194-6EAC-4FB7-9055-66129BA7C94A", "The code you have selected is not in the list.(In-Bond: {0})", parent.InBondNumber);
				ListValidation.MessageErrorIfInvalidCode(parent.ArrivalFirmsCodeInfo, movementHeader.Lookups.FIRMSCollection, message: message);
			}
		}
	}
}
