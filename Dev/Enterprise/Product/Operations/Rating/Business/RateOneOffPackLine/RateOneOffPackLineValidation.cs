
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateOneOffPackLineValidation : AutoRateOneOffPackLineValidation
	{
		public RateOneOffPackLineValidation(AutoRateOneOffPackLine parent) : base(parent)
		{
		}

		#region TPL_PackLineCount

		protected override void CheckTPL_PackLineCount()
		{
			base.CheckTPL_PackLineCount();
			MandatoryValidation.CheckEntered(Parent.TPL_PackLineCountInfo);
			MandatoryValidation.CheckNotNegative(Parent.TPL_PackLineCountInfo);
		}

		#endregion

		#region TPL_VehicleYear

		protected override void CheckTPL_VehicleYear()
		{
			base.CheckTPL_VehicleYear();
			MandatoryValidation.CheckNotNegative(Parent.TPL_VehicleYearInfo);
		}

		#endregion

		#region Dimension

		protected override void CheckTPL_DimensionUQ()
		{
			base.CheckTPL_DimensionUQ();

			if (Parent.TPL_Width > 0 || Parent.TPL_Height > 0 || Parent.TPL_Length > 0)
			{
				MandatoryValidation.CheckEntered(Parent.TPL_DimensionUQInfo);
			}

			if (!Parent.TPL_DimensionUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.TPL_DimensionUQInfo, Parent.Lookups.DimensionUnits);
			}
		}

		protected override void CheckTPL_Width()
		{
			base.CheckTPL_Width();
			MandatoryValidation.CheckNotNegative(Parent.TPL_WidthInfo);
		}

		protected override void CheckTPL_Height()
		{
			base.CheckTPL_Height();
			MandatoryValidation.CheckNotNegative(Parent.TPL_HeightInfo);
		}

		protected override void CheckTPL_Length()
		{
			base.CheckTPL_Length();
			MandatoryValidation.CheckNotNegative(Parent.TPL_LengthInfo);
		}

		#endregion

		#region TPL_VolumeUQ

		protected override void CheckTPL_VolumeUQ()
		{
			base.CheckTPL_VolumeUQ();
			MandatoryValidation.CheckUnitEntered(Parent.TPL_VolumeUQInfo, Parent.TPL_VolumeInfo);
			if (!Parent.TPL_VolumeUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.TPL_VolumeUQInfo, Parent.Lookups.VolumeUnits);
			}
		}

		protected override void CheckTPL_Volume()
		{
			base.CheckTPL_Volume();
			MandatoryValidation.CheckNotNegative(Parent.TPL_VolumeInfo);
		}

		#endregion

		#region Weight

		protected override void CheckTPL_WeightUQ()
		{
			base.CheckTPL_WeightUQ();
			MandatoryValidation.CheckUnitEntered(Parent.TPL_WeightUQInfo, Parent.TPL_WeightInfo);
			if (!Parent.TPL_WeightUQInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.TPL_WeightUQInfo, Parent.Lookups.WeightUnits);
			}
		}

		protected override void CheckTPL_Weight()
		{
			base.CheckTPL_Weight();
			MandatoryValidation.CheckNotNegative(Parent.TPL_WeightInfo);
		}

		#endregion

		#region TPL_F3_NKPackType

		protected override void CheckTPL_F3_NKPackType()
		{
			base.CheckTPL_F3_NKPackType();
			MandatoryValidation.CheckEntered(Parent.TPL_F3_NKPackTypeInfo);
			if (!Parent.TPL_F3_NKPackTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.TPL_F3_NKPackTypeInfo);
			}
		}

		#endregion
	}
}

