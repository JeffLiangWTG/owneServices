using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionValidation : DtbBookingInstructionValidation
	{
		protected DtbTransportInstructionValidation(DtbTransportInstruction parent)
			: base(parent)
		{
		}

		#region Instruction

		public DtbTransportInstruction Instruction
		{
			get { return (DtbTransportInstruction)Parent; }
		}

		#endregion

		// calculated

		#region ValidateOrganisationType

		public void ValidateOrganisationType()
		{
			ValidateCalculatedProperty(Instruction.OrganisationTypeInfo);
		}

		protected void CheckOrganisationType()
		{
			MandatoryValidation.CheckEntered(Instruction.OrganisationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Instruction.OrganisationTypeInfo);
		}

		#endregion

		// persistent

		#region ValidateKN_DropMode

		protected override void CheckKN_DropMode()
		{
			base.CheckKN_DropMode();
			ListValidation.ErrorIfInvalidCode(Parent.KN_DropModeInfo);
		}

		#endregion

		#region ValidateKN_InstructionType

		protected override void CheckKN_InstructionType()
		{
			base.CheckKN_InstructionType();
			MandatoryValidation.CheckEntered(Parent.KN_InstructionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.KN_InstructionTypeInfo);
		}

		#endregion

		#region ValidateKN_IsAuthorisedToLeave

		protected override void CheckKN_IsAuthorisedToLeave()
		{
			base.CheckKN_IsAuthorisedToLeave();
			if (Instruction.KN_IsAuthorisedToLeave)
			{
				var instruction = Instruction;

				if (instruction.HasAContainer)
				{
					instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("55A13F67-ADA2-46D8-A1F3-907C94CCFE2F", "Authority to Leave is not available to be given when delivering containers."));
				}

				if (!instruction.IsAuthorisedToLeaveAvailable)
				{
					instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("E76E4285-4203-4EE2-8AAF-48646853EDD9", "Authority to Leave is only available to be given for consignee deliveries."));
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
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("4774E5A3-567D-44DD-B7DE-C9A8E7DCA38D", "The consignee/delivery address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}

						if (!authorityToLeaveForConsignor && consignorDocAddress.Address != null)
						{
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("87a95003-da24-4b0a-888c-2bfd7eff3059", "The consignor/pickup address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}

						if (!authorityToLeaveForClient && clientOrgAddress != null)
						{
							instruction.KN_IsAuthorisedToLeaveInfo.AddError(ResString.GetMultilingualString("ED34BFCE-A524-4030-8E61-01A32794D900", "The local client address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking."));
						}
					}
				}
			}
		}

		#endregion

		static void AddATLErrorIfAnyAddressIsNull(ZPropertyInfo info, OrgAddress consigneeOrgAddress, JobDocAddress consignorDocAddress, OrgAddress clientOrgAddress)
		{
			if (consigneeOrgAddress == null)
			{
				info.AddError(ResString.GetMultilingualString("A1F5393B-0C17-44D1-AECF-1718F7AC20CB", "There is no Consignee Address for this job, this means you cannot give the Authority to Leave."));
			}

			if (consignorDocAddress != null && consignorDocAddress.Address == null)
			{
				info.AddError(ResString.GetMultilingualString("6227BBF8-38BE-4F5A-A463-7D243533E00F", "There is no Consignor Address for this job, this means you cannot give the Authority to Leave."));
			}

			if (clientOrgAddress == null)
			{
				info.AddError(ResString.GetMultilingualString("42B615D2-F73A-462A-80A6-ABD9F4D8FEF8", "There is no Client/Billing Party Address for this job, this means you cannot give the Authority to Leave."));
			}
		}

		// validate all

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOrganisationType();
		}

		#endregion
	}
}
