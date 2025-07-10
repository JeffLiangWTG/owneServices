using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class MultipleNonPersistentItemManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MultipleNonPersistentItemManager(IBusinessObjectCollection collection, string propertyName)
			=> (Collection, PropertyName) = (collection, propertyName);

		IBusinessObjectCollection Collection { get; }
		string PropertyName { get; }

		public ZString Value
		{
			get
			{
				switch (Collection.Count)
				{
					case 0:
						return (ZString)ZDataType.ZTypeToEmptyValue(typeof(ZString));

					case 1:
						return GetItemValue((BusinessObject)Collection[0]);

					default:
						return Res.GetString("852ca680-10e7-478a-a207-e830cffe7abb", "Many");
				}
			}
		}

		string GetItemValue(BusinessObject item)
		{
			if (item.GetType().GetProperty(PropertyName) == null)
			{
				return string.Empty;
			}

			if (item[PropertyName] is IZType property)
			{
				return property.ToString();
			}

			return string.Empty;
		}
	}
}
