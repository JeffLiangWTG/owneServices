//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsCartonSizeValidation
//
//    This class should be used for overriding validation in AutoWhsCartonSizeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class WhsCartonSizeValidation : AutoWhsCartonSizeValidation
	{
		public WhsCartonSizeValidation(AutoWhsCartonSize parent)
			: base(parent)
		{
		}

		#region CheckWCS_Code

		protected override void CheckWCS_Code()
		{
			base.CheckWCS_Code();
			MandatoryValidation.CheckEntered(Parent.WCS_CodeInfo);
			CheckCodeIsUnique();
		}

		void CheckCodeIsUnique()
		{
			if (!Parent.WCS_CodeInfo.HasErrors())
			{
				var query = new ZQuery(WhsCartonSizeSchema.WCS_Code, Parent.WCS_Code);
				query.AddToFilter(WhsCartonSizeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsCartonSize>(query) != null)
				{
					Parent.WCS_CodeInfo.AddError(Res.GetString("4e7a2b4f-d07c-453e-bd31-ca7e4f4a8f76", "Code must be unique."));
				}
			}
		}

		#endregion

		#region CheckWCS_Length

		protected override void CheckWCS_Length()
		{
			base.CheckWCS_Length();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_LengthInfo);
		}

		#endregion

		#region CheckWCS_Height

		protected override void CheckWCS_Height()
		{
			base.CheckWCS_Height();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_HeightInfo);
		}

		#endregion

		#region CheckWCS_Width

		protected override void CheckWCS_Width()
		{
			base.CheckWCS_Width();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_WidthInfo);
		}

		#endregion

		#region CheckWCS_DimensionUQ

		protected override void CheckWCS_DimensionUQ()
		{
			base.CheckWCS_DimensionUQ();
			MandatoryValidation.CheckEntered(Parent.WCS_DimensionUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WCS_DimensionUQInfo);
		}

		#endregion

		#region CheckWCS_EmptyWeight

		protected override void CheckWCS_EmptyWeight()
		{
			base.CheckWCS_EmptyWeight();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.WCS_EmptyWeightInfo, 0m);

			if (Parent.WCS_MaxWeight <= Parent.WCS_EmptyWeight)
			{
				Parent.WCS_EmptyWeightInfo.AddError(Res.GetString("e5d4e753-9711-43bd-a130-3255c26b4301", "Empty Weight must be less than Max Weight"));
			}
		}

		#endregion

		#region CheckWCS_MaxWeight

		protected override void CheckWCS_MaxWeight()
		{
			base.CheckWCS_MaxWeight();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_MaxWeightInfo);

			if (Parent.WCS_MaxWeight <= Parent.WCS_EmptyWeight)
			{
				Parent.WCS_MaxWeightInfo.AddError(Res.GetString("85944ec7-58d7-4363-a3ab-75d243e81f1c", "Max Weight must be greater than Empty Weight"));
			}
		}

		#endregion

		#region CheckWCS_WeightUQ

		protected override void CheckWCS_WeightUQ()
		{
			base.CheckWCS_WeightUQ();
			MandatoryValidation.CheckEntered(Parent.WCS_WeightUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WCS_WeightUQInfo);
		}

		#endregion

		#region CheckWCS_MaxUnits

		protected override void CheckWCS_MaxUnits()
		{
			base.CheckWCS_MaxUnits();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_MaxUnitsInfo);
		}

		#endregion

		#region CheckWCS_Volume

		protected override void CheckWCS_Volume()
		{
			base.CheckWCS_MaxUnits();

			var propertyInfo = Parent.WCS_VolumeInfo;
			CompareValidation.CheckNumberGreaterThanZero(propertyInfo);

			var cartonSize = (WhsCartonSize)Parent;
			if (!propertyInfo.HasErrors() && cartonSize.CanCalculateVolume)
			{
				CompareValidation.CheckLessThanOrEqualTo(propertyInfo, cartonSize.CalculatedVolume);
			}
		}

		#endregion

		#region CheckWCS_VolumeUQ

		protected override void CheckWCS_VolumeUQ()
		{
			base.CheckWCS_VolumeUQ();
			MandatoryValidation.CheckEntered(Parent.WCS_VolumeUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WCS_VolumeUQInfo);
		}

		#endregion

		#region CheckWCS_MaxFillPercent

		protected override void CheckWCS_MaxFillPercent()
		{
			base.CheckWCS_MaxFillPercent();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCS_MaxFillPercentInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.WCS_MaxFillPercentInfo, 100m);
		}

		#endregion

		#region CheckWCS_F3_NKPackType
		
		protected override void CheckWCS_F3_NKPackType()
		{
			base.CheckWCS_F3_NKPackType();
			MandatoryValidation.CheckEntered(Parent.WCS_F3_NKPackTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WCS_F3_NKPackTypeInfo);
		}

		#endregion
	}
}
