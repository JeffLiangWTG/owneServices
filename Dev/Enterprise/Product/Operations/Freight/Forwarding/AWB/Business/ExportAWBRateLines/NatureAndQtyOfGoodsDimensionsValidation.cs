using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsDimensionsValidation : NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsDimensionsValidation(NatureAndQtyOfGoodsDimensions parent)
			: base(parent)
		{
		}

		protected new NatureAndQtyOfGoodsDimensions Parent
		{
			get { return (NatureAndQtyOfGoodsDimensions)base.Parent; }
		}

		protected override bool IsValidationApplicable
		{
			get { return Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateLength();
			ValidateHeight();
			ValidateWidth();
			ValidateUnit();
			ValidateCount();
		}

		protected override void CheckText()
		{
		}

		public void ValidateLength()
		{
			ValidateCalculatedProperty(Parent.LengthInfo);
		}

		protected virtual void CheckLength()
		{
			if (IsValidationApplicable)
			{
				CheckDimensionWithinRange(Parent.LengthInfo, 1, 99999);
			}
		}

		public void ValidateWidth()
		{
			ValidateCalculatedProperty(Parent.WidthInfo);
		}

		protected virtual void CheckWidth()
		{
			if (IsValidationApplicable)
			{
				CheckDimensionWithinRange(Parent.WidthInfo, 1, 99999);
			}
		}

		public void ValidateHeight()
		{
			ValidateCalculatedProperty(Parent.HeightInfo);
		}

		protected virtual void CheckHeight()
		{
			if (IsValidationApplicable)
			{
				CheckDimensionWithinRange(Parent.HeightInfo, 1, 99999);
			}
		}

		public void ValidateUnit()
		{
			ValidateCalculatedProperty(Parent.UnitInfo);
		}

		protected virtual void CheckUnit()
		{
			if (IsValidationApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.UnitInfo);
				ListValidation.ErrorIfInvalidCode(Parent.UnitInfo, Parent.UnitList);
			}
		}

		public void ValidateCount()
		{
			ValidateCalculatedProperty(Parent.CountInfo);
		}

		protected virtual void CheckCount()
		{
			if (IsValidationApplicable)
			{
				CheckDimensionWithinRange(Parent.CountInfo, 1, 9999,
					Res.GetString("463f8d9a-e689-4ebf-b382-791793e1285a", "AWB Dimension 'Package Count' must be within the range 1 to 9999."));
			}
		}

		void CheckDimensionWithinRange(ZPropertyInfo propertyInfo, ZInt minimumAcceptableValue, ZInt maximumAcceptableValue, string overridingMessage = default)
		{
			var dimension = new ZInt(propertyInfo.Value);
			if (dimension < minimumAcceptableValue || dimension > maximumAcceptableValue)
			{
				var message = overridingMessage
					?? Res.GetString("0ef7cd14-55b3-426d-b620-a80f2b5bbb1d", "AWB Dimension '{0}' must be within the range {1} to {2}.", propertyInfo.HumanReadableName, minimumAcceptableValue.ToString(), maximumAcceptableValue.ToString());
				propertyInfo.AddError(message);
			}
		}
	}
}
