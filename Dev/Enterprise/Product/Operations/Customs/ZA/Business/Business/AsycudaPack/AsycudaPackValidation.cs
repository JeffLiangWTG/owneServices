using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaPackValidation : ManifestBase.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
			header = Parent.Header;
		}

		protected override void CheckAPA_PackQty()
		{
			base.CheckAPA_PackQty();

			if (HeaderNotNullWithType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackQtyInfo);
			}
		}

		protected override void CheckAPA_GoodsDescription()
		{
			base.CheckAPA_GoodsDescription();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_GoodsDescriptionInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_GoodsDescriptionInfo);
				}
			}
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.APA_PackUQInfo);
					MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackUQInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_PackUQInfo);
				}
			}
		}

		protected override void CheckAPA_Weight()
		{
			base.CheckAPA_Weight();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_WeightInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_WeightInfo);
				}
			}
		}

		protected override void CheckAPA_WeightUQ()
		{
			base.CheckAPA_WeightUQ();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.APA_WeightUQInfo);
					MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_WeightUQInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_WeightUQInfo);
				}
			}
		}

		protected override void CheckAPA_Volume()
		{
			base.CheckAPA_Volume();

			if (HeaderNotNullWithType)
			{
				if (header.IsBBB)
				{
					if (header.AMA_ContainerMode == Core.Constants.ContainerModes.Liquid)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_VolumeInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_VolumeInfo);
					}
				}
				else if (header.IsVOR || header.IsEOR || header.IsALD || header.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_VolumeInfo);
				}
			}
		}

		protected override void CheckAPA_VolumeUQ()
		{
			base.CheckAPA_VolumeUQ();

			if (HeaderNotNullWithType)
			{
				if (header.IsBBB)
				{
					if (header.AMA_ContainerMode == Core.Constants.ContainerModes.Liquid)
					{
						ListValidation.ErrorIfInvalidCode(Parent.APA_VolumeUQInfo);
						MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_VolumeUQInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_VolumeUQInfo);
					}
				}
				else if (header.IsVOR || header.IsEOR || header.IsALD || header.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_VolumeUQInfo);
				}
			}
		}

		protected override void CheckContainerPK()
		{
			base.CheckContainerPK();

			if (HeaderNotNullWithType)
			{
				if ((header.IsCOSTCO && !header.IsBBB && header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
					|| (header.IsGOVGIO && Parent.Outturn.C5_CargoType == CargoTypeList.Codes.Container)
					|| header.IsBGI)
				{
					ListValidation.ErrorIfInvalidPK(Parent.ContainerPKInfo);
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ContainerPKInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ContainerPKInfo);
				}
			}
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					if (Parent.APA_MarksAndNumbers.IsEmpty)
					{
						Parent.APA_MarksAndNumbersInfo.AddMessageError("Marks and numbers are required. Use NA if not available.");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.APA_MarksAndNumbersInfo);
				}
			}
		}

		bool HeaderNotNullWithType => header != null && (header.IsCOSTCO || header.IsGOVGIO);

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		readonly AsycudaManifestHeader header;
	}
}
