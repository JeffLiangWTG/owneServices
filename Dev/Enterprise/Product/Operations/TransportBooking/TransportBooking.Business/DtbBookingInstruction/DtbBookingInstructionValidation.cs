using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionValidation : Common.DtbBookingInstructionValidation
	{
		public DtbBookingInstructionValidation(DtbBookingInstruction parent)
			: base(parent)
		{
		}

		public DtbBookingInstruction Instruction
		{
			get { return (DtbBookingInstruction)Parent; }
		}

		// calculated

		public void ValidateOrganisationType()
		{
			ValidateCalculatedProperty(Instruction.OrganisationTypeInfo);
		}

		void CheckOrganisationType()
		{
			MandatoryValidation.CheckEntered(Instruction.OrganisationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Instruction.OrganisationTypeInfo);
		}

		// persistent

		protected override void CheckKN_DropMode()
		{
			base.CheckKN_DropMode();
			ListValidation.ErrorIfInvalidCode(Parent.KN_DropModeInfo);

			if (Instruction.Booking?.IsSendingXUSToCTO == true)
			{
				if (string.IsNullOrEmpty(Instruction.KN_DropMode))
				{
					Instruction.KN_DropModeInfo.AddMessageError(Res.GetString("DtbBookingInstructionValidation|SendingXUSToCTO_KN_DropMode", "This Booking must have Drop Mode entered for all Instructions."));
				}
			}
		}

		protected override void CheckKN_InstructionType()
		{
			base.CheckKN_InstructionType();
			MandatoryValidation.CheckEntered(Parent.KN_InstructionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.KN_InstructionTypeInfo);
		}

		protected override void CheckKN_IsAuthorisedToLeave()
		{
			base.CheckKN_IsAuthorisedToLeave();
			if (Instruction.KN_IsAuthorisedToLeave)
			{
				var instruction = Instruction;

				if (instruction.HasAContainer)
				{
					instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("3E7EA972-58BA-4CBC-971C-B6B3583EC286", "Authority to Leave is not available to be given when delivering containers."));
				}

				if (!instruction.IsAuthorisedToLeaveAvailable)
				{
					instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("6A1CF60A-F13C-4E7E-85E8-274E97CF106D", "Authority to Leave is only available to be given for consignee deliveries."));
				}
				else
				{
					var consigneeOrgAddress = instruction.Address.Address;
					var consignorInstruction = instruction.FindRelatedConsignorInstruction();
					var consignorDocAddress = consignorInstruction != null ? consignorInstruction.Address : null;
					var clientOrgAddress = instruction.Booking != null ? instruction.Booking.BillingPartyOrLocalClientAddress : null;

					AddATLErrorIfAnyAddressIsNull(instruction.KN_IsAuthorisedToLeaveInfo, consigneeOrgAddress, consignorDocAddress, clientOrgAddress);
					if (consignorInstruction != null && consignorDocAddress != null)
					{
						var authorityToLeaveForConsignee = AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeOrgAddress, consignorDocAddress.Address);
						var authorityToLeaveForConsignor = AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeOrgAddress);
						var authorityToLeaveForClient = AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientOrgAddress);

						if (!authorityToLeaveForConsignee && consigneeOrgAddress != null)
						{
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("D6B3C813-46B6-447B-A518-43F348D0D422", "The consignee/delivery address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}

						if (!authorityToLeaveForConsignor && consignorDocAddress.Address != null)
						{
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("8A5E81E4-2A95-47D8-BD04-D0E744EFA7D2", "The consignor/pickup address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}

						if (!authorityToLeaveForClient && clientOrgAddress != null)
						{
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("ADF75A17-4A3F-4FAB-8A86-97DE6F6ABBE6", "The local client address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}
					}
				}
			}
		}

		static void AddATLErrorIfAnyAddressIsNull(ZPropertyInfo info, OrgAddress consigneeOrgAddress, JobDocAddress consignorDocAddress, OrgAddress clientOrgAddress)
		{
			if (consigneeOrgAddress == null)
			{
				info.AddError(ResString.GetMultilingualString("C77D1D84-2319-4A92-AD90-DDE78125534B", "There is no Consignee Address for this job, this means you cannot give the Authority to Leave."));
			}

			if (consignorDocAddress != null && consignorDocAddress.Address == null)
			{
				info.AddError(ResString.GetMultilingualString("9DF79CDD-EA76-4520-AC42-FAC2089891DF", "There is no Consignor Address for this job, this means you cannot give the Authority to Leave."));
			}

			if (clientOrgAddress == null)
			{
				info.AddError(ResString.GetMultilingualString("C0202068-3F77-4EBB-B594-3B29EDDF5EB8", "There is no Client/Billing Party Address for this job, this means you cannot give the Authority to Leave."));
			}
		}

		protected override void CheckKN_Sequence()
		{
			base.CheckKN_Sequence();
			if (Instruction.Booking != null)
			{
				CheckSequence(Instruction.Booking.Instructions);
			}
		}

		void CheckSequence(DtbBookingInstructionCollection instructions)
		{
			var instructionsArray = instructions.Where(i => !i.IsDeleted).ToArray();

			if (instructionsArray.Length > 0)
			{
				Array.Sort(instructionsArray, (t1, t2) => t1.KN_Sequence.CompareTo(t2.KN_Sequence));

				if (Parent == instructionsArray[0] && Parent.KN_Sequence != 1)
				{
					Parent.KN_SequenceInfo.AddError(Res.GetString("22d8f8f9-6b6a-402f-a848-48f60a5158d0", "The first Instruction should have a sequence number of 1."));
				}
				else
				{
					for (int index = 0; index < instructionsArray.Length; index++)
					{
						var currentInstruction = instructionsArray[index];
						if (Parent == currentInstruction && Parent.KN_Sequence != index + 1)
						{
							Parent.KN_SequenceInfo.AddError(Res.GetString("8b3e6dd9-4caf-438f-a116-de2f195ec083", "Sequence must be sequential integers, continuously increasing by one."));
							break;
						}
					}
				}
			}
		}

		protected override void CheckKN_IsLooseRateable()
		{
			base.CheckKN_IsLooseRateable();

			if (Instruction.KN_IsLooseRateable && Instruction.Booking != null)
			{
				var numberOfLooseRateable = Instruction.Booking.Instructions.Count(i => i.KN_IsLooseRateable);
				if (numberOfLooseRateable == 1)
				{
					Parent.KN_IsLooseRateableInfo.AddError(Res.GetString("DtbBookingInstructionValidation|KN_IsLooseRateable", "Rating Instruction Pairs can only consist of 2 or more Instructions."));
				}
			}
		}

		protected override void CheckKN_IsContainerRateable()
		{
			base.CheckKN_IsContainerRateable();

			if (Instruction.KN_IsContainerRateable && Instruction.Booking != null)
			{
				var numberOfContainerRateable = Instruction.Booking.Instructions.Count(i => i.KN_IsContainerRateable);
				if (numberOfContainerRateable == 1)
				{
					Parent.KN_IsContainerRateableInfo.AddError(Res.GetString("DtbBookingInstructionValidation|KN_IsContainerRateable", "Rating Instruction Pairs can only consist of 2 or more Instructions."));
				}
			}
		}

		public void ValidateActual()
		{
			ValidateCalculatedProperty(Instruction.ActualInfo);
		}

		void CheckActual()
		{
			CheckDate(Instruction.ActualInfo, (i) => i.Actual);
		}

		public void ValidateEstimated()
		{
			ValidateCalculatedProperty(Instruction.EstimatedInfo);
		}

		void CheckEstimated()
		{
			CheckDate(Instruction.EstimatedInfo, (i) => i.Estimated);
		}

		void CheckDate(ZPropertyInfo dateInfo, Func<DtbBookingInstruction, ZDateTime> getDate)
		{
			var sourceName = Res.GetString("DDD8968C-D175-4E78-8211-D6E95CC9A05C", "Instruction");
			CheckDateOnInstructionsForRelatedPackage(dateInfo, Instruction, sourceName, getDate);
		}

		public static void CheckDateOnInstructionsForRelatedPackage(ZPropertyInfo dateInfo, DtbBookingInstruction instruction, ZString sourceName, Func<DtbBookingInstruction, ZDateTime> getDate)
		{
			var date = (ZDateTime)dateInfo.Value;
			if (date.IsValid)
			{
				var relatedInstructions = FindRelatedInstructionsByPackage(instruction);

				foreach (var relatedInstruction in relatedInstructions)
				{
					var relatedDate = getDate(relatedInstruction);
					if (relatedDate.IsValid)
					{
						if (instruction.KN_Sequence > relatedInstruction.KN_Sequence && date < relatedDate)
						{
							var dateIsEarlierWarning = Res.GetString("445F54B8-383F-49B4-B183-29454211F0F7", "Date is earlier than a previous {0} for the same package.", sourceName);
							dateInfo.AddWarning(dateIsEarlierWarning);
						}
						else if (instruction.KN_Sequence < relatedInstruction.KN_Sequence && date > relatedDate)
						{
							var dateIsLaterWarning = Res.GetString("E19C7B11-DA32-46E3-8443-C8C83CFB818B", "Date is later than a following {0} for the same package.", sourceName);
							dateInfo.AddWarning(dateIsLaterWarning);
						}
					}
				}
			}
		}

		static IEnumerable<DtbBookingInstruction> FindRelatedInstructionsByPackage(DtbBookingInstruction instruction)
		{
			var relatedInstructions = Enumerable.Empty<DtbBookingInstruction>();

			if (instruction != null && instruction.Booking != null)
			{
				var instructionPackagePks = instruction.PackageDivots.Select(pd => pd.KD_KP_Package).Distinct().ToArray();
				var allOtherInstructions = instruction.Booking.Instructions.Where(i => i.PK != instruction.PK);

				relatedInstructions = from i in allOtherInstructions
									  from p in i.DivotsWithPackages.Typed
									  where instructionPackagePks.Contains(p.KD_KP_Package)
									  select i;
			}

			return relatedInstructions;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOrganisationType();
			ValidateEstimated();
			ValidateActual();
		}
	}
}
