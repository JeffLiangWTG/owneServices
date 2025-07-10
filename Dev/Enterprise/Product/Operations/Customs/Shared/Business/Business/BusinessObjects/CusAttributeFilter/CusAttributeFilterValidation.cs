//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusAttributeFilterValidation
//
//    This class should be used for overriding validation in AutoCusAttributeFilterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using CargoWise.EntityFramework;

	public class CusAttributeFilterValidation : AutoCusAttributeFilterValidation
	{
		public CusAttributeFilterValidation(AutoCusAttributeFilter parent)
			: base(parent)
		{
		}

		protected new CusAttributeFilter Parent => (CusAttributeFilter)base.Parent;

		protected BaseCusClassPartPivot Pivot => Parent.Pivot;

		protected override void CheckBG_AttributeValue1()
		{
			base.CheckBG_AttributeValue1();
			MandatoryValidation.CheckEntered(Parent.BG_AttributeValue1Info, Res.GetString("404de03e-824c-4cf1-b229-c51b3c52ea64", "Attribute Value"));
		}

		public static class AttributeName
		{
			public static string Attribute1 => Res.GetString("55e8e48a-526b-452f-a321-2b95dc0629ff", "Attribute 1");

			public static string Attribute2 => Res.GetString("feac2e65-bba0-4181-bfe7-f16fa204d8f9", "Attribute 2");

			public static string Attribute3 => Res.GetString("cb0272b9-6024-45b8-b7f0-2d8dfc6decd1", "Attribute 3");
		}
	}
}
