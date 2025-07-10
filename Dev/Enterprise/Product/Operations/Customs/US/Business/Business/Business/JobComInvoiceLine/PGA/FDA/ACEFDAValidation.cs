using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public ACEFDAValidation(ACEFDA fda)
			: base(fda)
		{
		}

		new ACEFDA Parent
		{
			get { return (ACEFDA)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateUS_CanDim1_16th();
			ValidateUS_CanDim2_16th();
			ValidateUS_CanDim3_16th();
			ValidateUS_CanDim1Inch();
			ValidateUS_CanDim2Inch();
			ValidateUS_CanDim3Inch();
		}

		#region Validate Container Dimensions

		public void ValidateUS_CanDim1Inch()
		{
			ValidateCalculatedProperty(Parent.US_CanDim1InchInfo);
		}

		protected void CheckUS_CanDim1Inch()
		{
			CheckCanDimension(Parent.US_CanDim1InchInfo);
		}

		public void ValidateUS_CanDim2Inch()
		{
			ValidateCalculatedProperty(Parent.US_CanDim2InchInfo);
		}

		protected void CheckUS_CanDim2Inch()
		{
			CheckCanDimension(Parent.US_CanDim2InchInfo);
		}

		public void ValidateUS_CanDim3Inch()
		{
			ValidateCalculatedProperty(Parent.US_CanDim3InchInfo);
		}

		protected void CheckUS_CanDim3Inch()
		{
			CheckCanDimension(Parent.US_CanDim3InchInfo);
		}

		public void ValidateUS_CanDim1_16th()
		{
			ValidateCalculatedProperty(Parent.US_CanDim1_16thInfo);
		}

		protected void CheckUS_CanDim1_16th()
		{
			CheckCanDimension_16th(Parent.US_CanDim1_16thInfo);
		}

		public void ValidateUS_CanDim2_16th()
		{
			ValidateCalculatedProperty(Parent.US_CanDim2_16thInfo);
		}

		protected void CheckUS_CanDim2_16th()
		{
			CheckCanDimension_16th(Parent.US_CanDim2_16thInfo);
		}

		public void ValidateUS_CanDim3_16th()
		{
			ValidateCalculatedProperty(Parent.US_CanDim3_16thInfo);
		}

		protected void CheckUS_CanDim3_16th()
		{
			CheckCanDimension_16th(Parent.US_CanDim3_16thInfo);
		}

		void CheckCanDimension_16th(ZPropertyInfo propertyInfo)
		{
			var valueDim = (ZInt)propertyInfo.Value;

			if (valueDim < 0 || valueDim > 15)
			{
				propertyInfo.AddMessageError(Dimensions16th);
			}
		}
		internal const string Dimensions16th = "The '16th' field should be between 0 and 15.";

		void CheckCanDimension(ZPropertyInfo propertyInfo)
		{
			var value = (ZInt)propertyInfo.Value;

			if (value < 0 || value > 99)
			{
				propertyInfo.AddMessageError(DimensionsInch);
			}
		}
		internal const string DimensionsInch = "The Inch field should be between 0 and 99.";

		#endregion
	}
}
