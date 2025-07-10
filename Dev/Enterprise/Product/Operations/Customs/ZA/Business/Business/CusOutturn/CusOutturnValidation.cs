using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusOutturnValidation : Customs.Business.CusOutturnValidation
	{
		public CusOutturnValidation(CusOutturn parent)
			: base(parent)
		{
			header = Parent.Header;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePackCondDesc();
			ValidateExcessShortInd();
			ValidateContShouldBe();
		}

		public void ValidatePackCondDesc() => ValidateCalculatedProperty(Parent.PackCondDescInfo);

		public void ValidateExcessShortInd() => ValidateCalculatedProperty(Parent.ExcessShortIndInfo);

		public void ValidateContShouldBe() => ValidateCalculatedProperty(Parent.ContShouldBeInfo);

		protected void CheckExcessShortInd()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ExcessShortIndInfo);

					if (Parent.ExcessShortInd == ExcessShortIndicatorList.Codes.None
						&& (Parent.ContShouldBe != Parent.C5_GoodsDescription
							|| Parent.Pack.APA_Weight != Parent.C5_WeightOutturned
							|| Parent.Pack.APA_WeightUQ != Parent.C5_WeightOutturnedUQ
							|| Parent.Pack.APA_Volume != Parent.C5_VolumeOutturned
							|| Parent.Pack.APA_VolumeUQ != Parent.C5_VolumeOutturnedUQ
							|| Parent.Pack.APA_PackQty != Parent.C5_PackagesOutturned))
					{
						Parent.ExcessShortIndInfo.AddMessageError("Discrepancies exist but Short Excess Indicator 3 indicates that no discrepancies were found");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ExcessShortIndInfo);
				}
			}
		}

		protected void CheckPackCondDesc()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO && Parent.C5_PackageCondition == PackConditionList.Codes.Other)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.PackCondDescInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.PackCondDescInfo);
				}
			}
		}

		protected void CheckContShouldBe()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					if ((header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD) && Parent.ExcessShortInd != ExcessShortIndicatorList.Codes.None)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.ContShouldBeInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ContShouldBeInfo);
				}
			}
		}

		protected override void CheckC5_PackagesOutturned()
		{
			base.CheckC5_PackagesOutturned();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_PackagesOutturnedInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_PackagesOutturnedInfo);
				}
			}
		}

		protected override void CheckC5_WeightOutturned()
		{
			base.CheckC5_WeightOutturned();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_WeightOutturnedInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_WeightOutturnedInfo);
				}
			}
		}

		protected override void CheckC5_WeightOutturnedUQ()
		{
			base.CheckC5_WeightOutturnedUQ();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.C5_WeightOutturnedUQInfo);
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_WeightOutturnedUQInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_WeightOutturnedUQInfo);
				}
			}
		}

		protected override void CheckC5_VolumeOutturned()
		{
			base.CheckC5_VolumeOutturned();

			if (HeaderNotNullWithType)
			{
				if ((header.IsDOR || header.IsBBB || header.IsVOR || header.IsAOR) && header.AMA_ContainerMode == Core.Constants.ContainerModes.Liquid)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_VolumeOutturnedInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_VolumeOutturnedInfo);
				}
			}
		}

		protected override void CheckC5_VolumeOutturnedUQ()
		{
			base.CheckC5_VolumeOutturnedUQ();

			if (HeaderNotNullWithType)
			{
				if ((header.IsDOR || header.IsBBB || header.IsVOR || header.IsAOR) && header.AMA_ContainerMode == Core.Constants.ContainerModes.Liquid)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.C5_VolumeOutturnedUQInfo);
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.C5_VolumeOutturnedUQInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_VolumeOutturnedUQInfo);
				}
			}
		}

		protected override void CheckC5_PackageCondition()
		{
			base.CheckC5_PackageCondition();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.C5_PackageConditionInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_PackageConditionInfo);
				}
			}
		}

		protected override void CheckC5_CargoType()
		{
			base.CheckC5_CargoType();

			if (HeaderNotNullWithType)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.C5_CargoTypeInfo);
			}
		}

		protected override void CheckC5_GoodsDescription()
		{
			base.CheckC5_GoodsDescription();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					if ((header.IsDOR || header.IsBBB || header.IsAOR) && !Parent.ContShouldBe.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_GoodsDescriptionInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.C5_GoodsDescriptionInfo);
				}
			}
		}

		bool HeaderNotNullWithType => header != null && (header.IsCOSTCO || header.IsGOVGIO);

		public new CusOutturn Parent => (CusOutturn)base.Parent;

		readonly AsycudaManifestHeader header;
	}
}
