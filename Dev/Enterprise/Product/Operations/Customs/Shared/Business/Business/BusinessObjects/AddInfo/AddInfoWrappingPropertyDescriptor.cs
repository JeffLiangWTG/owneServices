using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class AddInfoWrappingPropertyDescriptor : KPropertyDescriptor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AddInfoWrappingPropertyDescriptor(BusinessObjectPropertyDescriptorCollection collection, PropertyDescriptor inner, PropertyDescriptor addInfoInner, string lookupsStringToReplaceWith)
			: base(collection, inner)
		{
			innerReflectPropertyDescriptor = inner as KReflectPropertyDescriptor;
			isPropertyTypePersistentBusinessObject = typeof(BusinessObject).IsAssignableFrom(PropertyType) && !typeof(NonPersistentBusinessObject).IsAssignableFrom(PropertyType);
			this.addInfoInner = addInfoInner;
			this.lookupsStringToReplaceWith = Argument.NotNullOrEmpty(lookupsStringToReplaceWith, nameof(lookupsStringToReplaceWith));
		}
		readonly string lookupsStringToReplaceWith;
		readonly PropertyDescriptor addInfoInner;

		public override AttributeCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					var list = new List<Attribute>();
					list.AddRange(Inner.Attributes.Cast<Attribute>());
					if (addInfoInner != null)
					{
						var inner = Inner;
						if (inner != null)
						{
							var innerAttributes = inner.Attributes.OfType<ListAttribute>().Cast<Attribute>().Union(inner.Attributes.OfType<MaxLengthAttribute>()).ToList();
							if (innerAttributes.Any())
							{
								var innerPropertyName = $"{Inner.ComponentType.FullName}.{Inner.Name}";
								var addInfoPropertyName = $"{addInfoInner.ComponentType.FullName}.{addInfoInner.Name}";
								var attribute = innerAttributes.First().GetType().Name;
								ErrorReporter.ReportOnce($"Attributes ({attribute}) should not be added directly to the {innerPropertyName} property. Please add the Attribute to the {addInfoPropertyName} property instead.");
							}
						}

						foreach (Attribute attribute in addInfoInner.Attributes)
						{
							if (attribute is ListAttribute listAttriute)
							{
								var newAttribute = new ListAttribute(listAttriute.ListDataSourceMember.Replace("Lookups.", lookupsStringToReplaceWith), listAttriute.ValueMember, listAttriute.DisplayMember);
								list.Add(newAttribute);
							}
							else if (attribute is MaxLengthAttribute)
							{
								list.Add(attribute);
							}
						}
					}
					list.Add(BrowsableAttribute.No);
					attributes = new AttributeCollection(list.ToArray());
				}
				return attributes;
			}
		}
		AttributeCollection attributes;

		protected override object GetValueCore(object component)
		{
			object value = innerReflectPropertyDescriptor != null ? innerReflectPropertyDescriptor.GetValue(component) : base.GetValueCore(component);
			if (value == null && isPropertyTypePersistentBusinessObject)
			{
				var business = component as IBusiness;
				if (business != null && business.Factory != null)
				{
					value = business.Factory.GetNull(PropertyType);
				}
			}
			return value;
		}

		protected override void SetValueCore(object component, object value)
		{
			if (!HasSetter())
			{
				throw new InvalidOperationException(GetType().Name + " is read-only <" + Name + ">.");
			}
			else
			{
				base.SetValueCore(component, value is DBNull ? null : value);
			}
		}

		public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
		{
			return ZCustomTypeDescriptor.GetProperties(PropertyType);
		}

		#region Implementation

		readonly KReflectPropertyDescriptor innerReflectPropertyDescriptor;
		readonly bool isPropertyTypePersistentBusinessObject;

		#endregion
	}
}
