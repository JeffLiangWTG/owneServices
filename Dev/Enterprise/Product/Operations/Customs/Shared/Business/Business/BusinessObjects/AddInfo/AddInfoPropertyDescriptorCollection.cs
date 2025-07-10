using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class AddInfoPropertyDescriptorCollection : BusinessObjectPropertyDescriptorCollection
	{
		protected AddInfoPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
		}

		public new static AddInfoPropertyDescriptorCollection FromType(Type componentType) => FromType(componentType, false);

		public new static AddInfoPropertyDescriptorCollection FromType(Type componentType, bool includePrivate) => factory.FromType(componentType, includePrivate);

		[ThreadStatic]
		static PropertyDescriptorCollectionFactory<AddInfoPropertyDescriptorCollection> ffactory;

		static PropertyDescriptorCollectionFactory<AddInfoPropertyDescriptorCollection> factory
		{
			get { return ffactory ?? (ffactory = new PropertyDescriptorCollectionFactory<AddInfoPropertyDescriptorCollection>(componentType => new AddInfoPropertyDescriptorCollection(componentType, false))); }
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new AddInfoPropertyDescriptorCollection(componentType, includePrivate);
		}

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override void PopulatePropertyDescriptorsCore()
		{
			base.PopulatePropertyDescriptorsCore();
			ReplaceWithAddInfoWrappingIfNeeded("AddInfoSchema", "AddInfoType", "AddInfoLookups.");
			ReplaceWithAddInfoWrappingIfNeeded("AddInfoChildSchema", "AddInfoChildType", "AddInfoChildLookups.");
			ReplaceWithOldPrefixAddInfoWrappingIfNeeded("AddInfoSchema", "AddInfoLookups.");
		}

		void ReplaceWithAddInfoWrappingIfNeeded(string addInfoSchemaPropertyName, string addInfoTypePropertyName, string addInfoLookupsToReplaceWith)
		{
			var addInfoSchema = (CargoWise.Schema.ITableSchema)ComponentType.GetProperty(addInfoSchemaPropertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
			var addInfoType = (Type)ComponentType.GetProperty(addInfoTypePropertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
			if (addInfoSchema != null && addInfoType != null)
			{
				var addInfoProperties = BusinessObjectPropertyDescriptorCollection.FromType(addInfoType, IncludePrivate).Cast<PropertyDescriptor>();
				var addInfoSchemaProperties = addInfoProperties.Where(x => addInfoSchema.All.Any(y => y.Name.Equals(x.Name)));

				foreach (var addInfoProperty in addInfoSchemaProperties)
				{
					var outerProperty = Find(addInfoProperty.Name, true);
					if (outerProperty != null)
					{
						Remove(outerProperty);
						AddIfNotExists(new AddInfoWrappingPropertyDescriptor(this, outerProperty, addInfoProperty, addInfoLookupsToReplaceWith));
					}
				}
			}
		}

		void ReplaceWithOldPrefixAddInfoWrappingIfNeeded(string addInfoSchemaPropertyName, string addInfoLookupsToReplaceWith)
		{
			var addInfoSchema = (CargoWise.Schema.ITableSchema)ComponentType.GetProperty(addInfoSchemaPropertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
			var oldTablePrefix = (string)ComponentType.GetProperty("OldTablePrefix", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
			if (addInfoSchema != null && !string.IsNullOrEmpty(oldTablePrefix))
			{
				var addInfoProperties = BusinessObjectPropertyDescriptorCollection.FromType(ComponentType, IncludePrivate).Cast<PropertyDescriptor>();
				var addInfoSchemaProperties = addInfoProperties.Where(x => addInfoSchema.All.Any(y => y.Name.Equals(x.Name)));

				foreach (var addInfoProperty in addInfoSchemaProperties)
				{
					if (!addInfoProperty.Name.StartsWith(oldTablePrefix))
					{
						var outerProperty = Find(oldTablePrefix + addInfoProperty.Name.Substring(addInfoProperty.Name.IndexOf("_")), true);
						if (outerProperty != null)
						{
							Remove(outerProperty);
							AddIfNotExists(new AddInfoWrappingPropertyDescriptor(this, outerProperty, addInfoProperty, addInfoLookupsToReplaceWith));
						}
					}
				}
			}
		}
	}
}
