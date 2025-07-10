using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	class ExBondJobDeclarationValidation : ImportCommonJobDeclarationValidation
	{
		public ExBondJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		#region Implementation

		#region VoyageFlightNo
		protected override void CheckJE_VoyageFlightNo()
		{
			if (!Parent.JE_VoyageFlightNo.IsEmpty && !Parent.IsRoad)
			{
				Parent.JE_VoyageFlightNoInfo.AddMessageError(VoyageFlightNoMustBeEmpty(Parent.JE_VoyageFlightNoInfo.Description));
			}
		}

		public static string VoyageFlightNoMustBeEmpty(ZString voyageFlightNoLabel)
		{
			return Res.GetString("d54782cd-6c8e-42d8-8f26-c3d71a46f760", "{0} must be blank.", voyageFlightNoLabel);
		}
		#endregion

		#region Vessel
		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (!Parent.JE_VesselName.IsEmpty)
			{
				Parent.JE_VesselNameInfo.AddMessageError(VesselMustBeBlank);
			}
		}

		public static string VesselMustBeBlank
		{
			get { return Res.GetString("8f609af6-3399-46a0-ba02-9d53168a3cd6", "Vessel must be blank."); }
		}
		#endregion

		protected override void CheckJE_RL_NKOrigin()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKOriginInfo, Parent.Lookups.Origins);
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (!Parent.FinalDestination?.Country?.IsBLNS ?? true)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_TransportModeInfo);
			}
		}

		#endregion

		#region AddInfo

		protected override void CheckJE_MasterBillIssuedDate()
		{
			base.CheckJE_MasterBillIssuedDate();
			if (!Parent.JE_MasterBillIssuedDate.IsEmpty)
			{
				if (Parent.JE_RemovalTransportCode != Core.Constants.TransportModes.Road)
				{
					Parent.JE_MasterBillIssuedDateInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedDateMustBeEmpty);
				}
				else if (Parent.JE_MasterBill.IsEmpty)
				{
					Parent.JE_MasterBillIssuedDateInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedDateMasterBillNotCaptured(Parent.JE_MasterBillInfo.HumanReadableName));
				}
			}

			if (Parent.FinalDestination?.Country?.IsBLNS ?? false)
			{
				if (Parent.JE_MasterBillIssuedDate.IsEmpty)
				{
					Parent.JE_MasterBillIssuedDateInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);
				}
			}
			else
			{
				if (!Parent.JE_MasterBillIssuedDate.IsEmpty)
				{
					Parent.JE_MasterBillIssuedDateInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedAtMustBeEmpty);
				}
			}
		}

		protected override void CheckJE_RL_NKMasterBillIssuedAt()
		{
			base.CheckJE_RL_NKMasterBillIssuedAt();
			if (Parent.FinalDestination?.Country?.IsBLNS ?? false)
			{
				if (Parent.JE_RL_NKMasterBillIssuedAt.IsEmpty)
				{
					Parent.JE_RL_NKMasterBillIssuedAtInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);
				}
			}
			else
			{
				if (!Parent.JE_RL_NKMasterBillIssuedAt.IsEmpty)
				{
					Parent.JE_RL_NKMasterBillIssuedAtInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedAtMustBeEmpty);
				}
			}
		}

		#endregion

	}
}
