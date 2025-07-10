using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI
{
	internal class WorkflowReadonlyCustomPropertyDescriptor : ZCustomPropertyDescriptor
	{
		public string CustomColumnDefinitionName { get; private set; }
		public WorkflowReadonlyCustomPropertyDescriptor(ICustomProperty property, string customColumnDefinitionName) : base(property.Identifier, property.Info.Type)
		{
			this.CustomColumnDefinitionName = customColumnDefinitionName;
		}

		protected override object GetValueCore(object component)
		{
			BusinessObject bizo = component as BusinessObject;
			if (bizo != null)
			{
				GenCustomAddOnValue customValue = bizo.Factory.LoadTop1<GenCustomAddOnValue>(WorkflowCustomFieldsGridEditableInitializer.GetCustomValueQuery(bizo, this.CustomColumnDefinitionName, AddOnColumnDataType.GetCodeFromType(this.PropertyType)));
				if (customValue != null)
				{
					try
					{
						return Converter.ConvertFrom(customValue.XV_Data);
					}
					catch (ZTypeValueException)
					{
						return null;
					}
					catch (FormatException)
					{
						return null;
					}
				}
			}
			return null;
		}

		protected override void SetValueCore(object component, object value)
		{
			throw new NotSupportedException(string.Format("CustomFieldReadonlyPropertyDescriptor for property '{0}' is read only", Name));
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}
	}
}
