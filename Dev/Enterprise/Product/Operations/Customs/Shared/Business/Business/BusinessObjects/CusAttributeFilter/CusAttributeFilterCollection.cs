using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusAttributeFilterCollection<T> : CusAttributeFilterCollection
		where T : CusAttributeFilter
	{
		public CusAttributeFilterCollection(BaseCusClassPartPivot pivot, ZString attributeName)
			: base(pivot, attributeName)
		{
		}

		public new T this[int i]
		{
			get { return (T)base[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public new T[] ToArray()
		{
			return (T[])base.ToArray();
		}
	}

	public class CusAttributeFilterCollection : ActiveBusinessObjectCollection<CusAttributeFilter>
	{
		public CusAttributeFilterCollection(BaseCusClassPartPivot pivot, ZString attributeName)
			: base(pivot, new ZQuery(CusAttributeFilterSchema.BG_AttributeName, attributeName))
		{
			this.attributeName = attributeName;
		}

		public bool HasValue1(ZString value1)
		{
			bool result = false;
			foreach (CusAttributeFilter attribute in this)
			{
				if (attribute.BG_AttributeValue1.EqualsIgnoringCase(value1))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool HasSameValue1(CusAttributeFilter attributeToCheck)
		{
			bool result = false;
			if (!attributeToCheck.BG_AttributeValue1.IsEmpty)
			{
				foreach (CusAttributeFilter attribute in this)
				{
					if (attribute != attributeToCheck && attribute.BG_AttributeValue1.EqualsIgnoringCase(attributeToCheck.BG_AttributeValue1))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusAttributeFilter newElement, bool throwIfRelationshipNotSupported)
		{
			newElement.BG_AttributeName = attributeName;
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
		}

		readonly ZString attributeName;
	}
}
