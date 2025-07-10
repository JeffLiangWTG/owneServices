using System;
using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class DateTimeOffsetRelatedPortAttribute : Attribute, IFilteredAttributeForWrappingPropertyDescriptor
	{
		public DateTimeOffsetRelatedPortAttribute(string relatedPortProperty, bool isRelatedPortMandatory = false)
		{
			RelatedPortProperty = relatedPortProperty;
			IsRelatedPortMandatory = isRelatedPortMandatory;
		}

		public string RelatedPortProperty { get; private set; }
		public bool IsRelatedPortMandatory { get; private set; }

		#region IFilteredAttributeForWrappingPropertyDescriptor Members

		Attribute IFilteredAttributeForWrappingPropertyDescriptor.GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			return new DateTimeOffsetRelatedPortAttribute(wrappingProperty.Outer.Name + "+" + RelatedPortProperty);
		}

		#endregion
	}
}
