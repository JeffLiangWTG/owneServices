using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusAttributeFilterCollection : Customs.Business.CusAttributeFilterCollection<CusAttributeFilter>
	{
		public CusAttributeFilterCollection(CusClassPartPivot pivot, ZString attributeType)
			: base(pivot, attributeType)
		{
		}

		public bool HasSameValue1(CusAttributeFilter attributeToCheck)
		{
			bool result = false;
			if (!attributeToCheck.BG_AttributeValue1.IsEmpty)
			{
				foreach (CusAttributeFilter attribute in this)
				{
					if (attribute != attributeToCheck && attribute.BG_AttributeValue1 == attributeToCheck.BG_AttributeValue1)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}
}
