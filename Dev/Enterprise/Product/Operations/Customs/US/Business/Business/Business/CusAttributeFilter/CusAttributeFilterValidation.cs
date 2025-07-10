using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusAttributeFilterValidation : Customs.Business.CusAttributeFilterValidation
	{
		public CusAttributeFilterValidation(CusAttributeFilter parent)
			: base(parent)
		{
		}

		protected new CusAttributeFilter Parent
		{
			get { return (CusAttributeFilter)base.Parent; }
		}

		protected new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		protected override void CheckBG_AttributeValue1()
		{
			base.CheckBG_AttributeValue1();
			CheckDuplicateAttribute();
		}

		void CheckDuplicateAttribute()
		{
			var pivot = Pivot;
			if (pivot != null && pivot.IsImportClassification)
			{
				CusAttributeFilterCollection collection = null;
				var attributeType = ZString.Empty;
				if (Parent.IsAttribute2)
				{
					collection = pivot.Attributes2;
					attributeType = "Attribute 2";
				}
				else if (Parent.IsAttribute3)
				{
					collection = pivot.Attributes3;
					attributeType = "Attribute 3";
				}
				else if (Parent.IsAttribute1)
				{
					collection = pivot.Attributes1;
					attributeType = "Attribute 1";
				}
				if (collection != null && collection.HasSameValue1(Parent))
				{
					Parent.BG_AttributeValue1Info.AddError(ValidationConstants.Part.AttributeValueShouldBeUniqueFor(attributeType));
				}
			}
		}
	}
}
