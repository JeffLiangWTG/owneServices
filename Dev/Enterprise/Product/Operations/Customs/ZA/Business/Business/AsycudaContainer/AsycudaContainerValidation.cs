using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaContainerValidation : ManifestBase.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
			header = Parent.Header;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGateInOutDate();
			ValidateContUnpackTime();
		}

		public void ValidateContUnpackTime() => ValidateCalculatedProperty(Parent.ContUnpackTimeInfo);

		public void ValidateGateInOutDate() => ValidateCalculatedProperty(Parent.GateInOutDateInfo);

		protected override void CheckACN_EmptyFullIndicator()
		{
			base.CheckACN_EmptyFullIndicator();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsTGO || header.IsTGI || header.IsDGI || header.IsDGO)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.ACN_EmptyFullIndicatorInfo);
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_EmptyFullIndicatorInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_EmptyFullIndicatorInfo);
				}
			}
		}

		protected override void CheckACN_ContainerNumber()
		{
			base.CheckACN_ContainerNumber();

			if (HeaderNotNullWithType)
			{
				if (header.IsTGO || header.IsTGI || header.IsDGI || header.IsDGO || header.IsDOR || header.IsDCI || header.IsBGI)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ContainerNumberInfo);
				}
				else if (header.IsBBB)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_ContainerNumberInfo);
				}
				else if (header.IsVOR)
				{
					if (header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ContainerNumberInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_ContainerNumberInfo);
					}
				}

				if (header.IsVOR && header.Containers.Count > 1 && Parent != header.Containers[0])
				{
					Parent.ACN_ContainerNumberInfo.AddMessageError("Only one container allowed for 'VOR' outturn.");
				}
			}
		}

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR)
				{
					if (Parent.ACN_RC_ContainerType == ZGuid.Empty)
					{
						MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.ACN_RC_ContainerTypeInfo);
					}
					else
					{
						ListValidation.ErrorIfInvalidPK(Parent.ACN_RC_ContainerTypeInfo);
					}
				}
				else if (!header.IsTGO && !header.IsTGI && !header.IsDGI && !header.IsDGO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_RC_ContainerTypeInfo);
				}
			}
		}

		protected override void CheckACN_SealingPartyType()
		{
			base.CheckACN_SealingPartyType();

			if (HeaderNotNullWithType)
			{
				if (header.IsBBB || header.IsAOR || header.IsALD || header.IsDCI || header.IsATI || header.IsADI || header.IsBGI)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_SealingPartyTypeInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.ACN_SealingPartyTypeInfo);
				}
			}
		}

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();

			if (HeaderNotNullWithType)
			{
				if (header.IsTGO || header.IsTGI || header.IsDGI || header.IsDGO || header.IsBGI)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_Seal1Info);
				}
				else if (header.IsDOR || header.IsVOR || header.IsALD)
				{
					if (header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_Seal1Info);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_Seal1Info);
					}
				}
				else if (!header.IsEOR)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ACN_Seal1Info);
				}
			}
		}

		protected void CheckGateInOutDate()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsGOVGIO)
				{
					if (Parent.GateInOutDate == ZDateTime.Empty)
					{
						MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.GateInOutDateInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.GateInOutDateInfo);
				}
			}
		}

		protected void CheckContUnpackTime()
		{
			if (HeaderNotNullWithType)
			{
				if ((header.IsDOR || header.IsVOR || header.IsEOR) && header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ContUnpackTimeInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ContUnpackTimeInfo);
				}
			}
		}

		bool HeaderNotNullWithType => header != null && (header.IsCOSTCO || header.IsGOVGIO);

		public new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		readonly AsycudaManifestHeader header;
	}
}
