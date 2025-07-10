using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace Enterprise.Rating.Business
{
	public class CostsComparePropertyDescriptor : KPropertyDescriptor
	{
		public CostsComparePropertyDescriptor(string name, Type propertyType)
			: base(null, name, new Attribute[] { new BrowsableAttribute(false) })
		{
			this.propertyType = propertyType;
			this.name = name;
		}

		protected override object GetValueCore(object component)
		{
			return ((CostsComparerEntry)component).SummaryColumn(Name, PropertyType);
		}

		public override Type PropertyType
		{
			get { return propertyType; }
		}
		readonly Type propertyType;

		public override string Name
		{
			get { return name; }
		}
		readonly string name;

		public override TypeConverter Converter
		{
			get
			{
				var customAttributes = PropertyType.GetCustomAttributes(typeof(TypeConverterAttribute), true);

				if (customAttributes != null && customAttributes.Length > 0)
				{
					return (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName));
				}

				return base.Converter;
			}
		}
	}
}
