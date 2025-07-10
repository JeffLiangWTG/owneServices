using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackageValidation : CusDecHouseContainerPackValidation
	{
		public PackageValidation(Package package)
			: base(package)
		{
		}

		public new Package Parent => (Package)base.Parent;

		protected override void CheckCW_NetWeight()
		{
			var netWeight = Parent.CW_NetWeight;
			if (!netWeight.IsDefault && netWeight >= Parent.CW_GrossWeight)
			{
				Parent.CW_NetWeightInfo.AddMessageError(ValidationConstants.Package.NetWeightLessThanGrossWeight);
			}

			if (netWeight < 0)
			{
				Parent.CW_NetWeightInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckCW_NetWeightUQ()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CW_NetWeightUQInfo, Parent.Lookups.WeightUQList);

			var netWeightUQ = Parent.CW_NetWeightUQ;
			if (!netWeightUQ.IsEmpty && netWeightUQ != Parent.CW_GrossWeightUQ)
			{
				Parent.CW_NetWeightUQInfo.AddMessageError(ValidationConstants.Package.SameWeightUnit);
			}

			if (Parent.CW_NetWeight > 0 && netWeightUQ.IsEmpty)
			{
				Parent.CW_NetWeightUQInfo.AddMessageError(ValidationConstants.Package.NetWeightUQIsRequiredWhenNetWeightIsGreaterThanZero);
			}
		}

		protected override void CheckCW_GrossWeight()
		{
			var grossWeight = Parent.CW_GrossWeight;
			if (!grossWeight.IsDefault && Parent.CW_NetWeight >= grossWeight)
			{
				Parent.CW_GrossWeightInfo.AddMessageError(ValidationConstants.Package.NetWeightLessThanGrossWeight);
			}

			if (grossWeight < 0)
			{
				Parent.CW_GrossWeightInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckCW_GrossWeightUQ()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CW_GrossWeightUQInfo, Parent.Lookups.WeightUQList);

			var grossWeightUQ = Parent.CW_GrossWeightUQ;
			if (!grossWeightUQ.IsEmpty && grossWeightUQ != Parent.CW_NetWeightUQ)
			{
				Parent.CW_GrossWeightUQInfo.AddMessageError(ValidationConstants.Package.SameWeightUnit);
			}

			if (Parent.CW_GrossWeight > 0 && grossWeightUQ.IsEmpty)
			{
				Parent.CW_GrossWeightUQInfo.AddMessageError(ValidationConstants.Package.GrossWeightUQIsRequiredWhenGrossWeightIsGreaterThanZero);
			}
		}

		protected override void CheckCW_DimensionUQ()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CW_DimensionUQInfo, Parent.Lookups.DimensionUQList);

			if ((Parent.CW_Length > 0 || Parent.CW_Height > 0 || Parent.CW_Width > 0) && Parent.CW_DimensionUQ.IsEmpty)
			{
				Parent.CW_DimensionUQInfo.AddMessageError(ValidationConstants.Package.DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero);
			}
		}

		protected override void CheckCW_VolumeUQ()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CW_VolumeUQInfo, Parent.Lookups.VolumeUQList);

			if (Parent.CW_Volume > 0 && Parent.CW_VolumeUQ.IsEmpty)
			{
				Parent.CW_VolumeUQInfo.AddMessageError(ValidationConstants.Package.VolumeUQIsRequiredWhenVolumeIsGreaterThanZero);
			}
		}

		protected override void CheckCW_Volume()
		{
			base.CheckCW_Volume();

			if (Parent.CW_Volume < 0)
			{
				Parent.CW_VolumeInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckCW_Length()
		{
			base.CheckCW_Length();

			if (Parent.CW_Length < 0)
			{
				Parent.CW_LengthInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckCW_Height()
		{
			base.CheckCW_Height();

			if (Parent.CW_Height < 0)
			{
				Parent.CW_HeightInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckCW_Width()
		{
			base.CheckCW_Width();

			if (Parent.CW_Width < 0)
			{
				Parent.CW_WidthInfo.AddMessageError(ValidationConstants.Package.ValueShouldNotBeNegative);
			}
		}

		protected override void CheckTotalNumberOfPacksExceedsCW_PackQty()
		{
			//Remove the validation
		}

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();
			MandatoryValidation.CheckNotZero(Parent.CW_PackQtyInfo);
		}

		protected override void CheckCW_PackType()
		{
			base.CheckCW_PackType();
			MandatoryValidation.CheckEntered(Parent.CW_PackTypeInfo);
		}
	}
}
