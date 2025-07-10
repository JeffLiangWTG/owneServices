using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyTopLevelPackValidation : AgencyShipmentContainerValidation
	{
		public AgencyTopLevelPackValidation(AgencyShipmentContainer container)
			: base(container)
		{
		}

		protected override void CheckJC_ContainerCount()
		{
			CompareValidation.CheckNumberNotNegative(Parent.JC_ContainerCountInfo);
			ValidateJC_ContainerNum();
		}

		protected override void CheckJC_ContainerNum()
		{
			// Ease common container validation
		}

		protected override void CheckJC_RC()
		{
			// Ease common container validation
		}

		protected override void CheckJC_F3_NKPackType()
		{
			base.CheckJC_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.JC_F3_NKPackTypeInfo);
		}

		protected override void CheckJC_RH_NKContainerCommodityCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JC_RH_NKContainerCommodityCodeInfo);
		}

		protected override void CheckJC_GrossWeight()
		{
			if (Parent.JC_GrossWeight == 0m)
			{
				Parent.JC_GrossWeightInfo.AddWarning(Res.GetString("5caed552-4f13-49a9-898c-d7402c59a66d", "You have not entered weight."));
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.JC_GrossWeightInfo);
			}
		}

		protected override void CheckJC_GrossWeightUQ()
		{
			MandatoryValidation.CheckUnitEntered(Parent.JC_GrossWeightUQInfo, Parent.JC_GrossWeightInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JC_GrossWeightUQInfo);
		}

		protected override void CheckJC_GrossVolume()
		{
			if (Parent.JC_GrossVolume == 0m)
			{
				Parent.JC_GrossVolumeInfo.AddWarning(Res.GetString("fc34b22f-fbb8-4d7f-a660-480df6868f11", "You have not entered volume."));
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanZero(Parent.JC_GrossVolumeInfo);
			}
		}

		protected override void CheckJC_GrossVolumeUQ()
		{
			MandatoryValidation.CheckUnitEntered(Parent.JC_GrossVolumeUQInfo, Parent.JC_GrossVolumeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JC_GrossVolumeUQInfo);
		}

		#region Dimensions

		protected override void CheckJC_TotalUnitOfMeasure()
		{
			base.CheckJC_TotalUnitOfMeasure();
			ListValidation.ErrorIfInvalidCode(Parent.JC_TotalUnitOfMeasureInfo);
		}

		protected override void CheckJC_TotalHeight()
		{
			base.CheckJC_TotalHeight();
			CompareValidation.CheckNumberNotNegative(Parent.JC_TotalHeightInfo);
		}

		protected override void CheckJC_TotalLength()
		{
			base.CheckJC_TotalHeight();
			CompareValidation.CheckNumberNotNegative(Parent.JC_TotalLengthInfo);
		}

		protected override void CheckJC_TotalWidth()
		{
			base.CheckJC_TotalHeight();
			CompareValidation.CheckNumberNotNegative(Parent.JC_TotalWidthInfo);
		}

		#endregion
	}
}


