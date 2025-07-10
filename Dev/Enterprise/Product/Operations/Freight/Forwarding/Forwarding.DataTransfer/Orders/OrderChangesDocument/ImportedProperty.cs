using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public enum ImportedPropertyState
	{
		New,
		Unchanged,
		Modified,
	}

	public class ImportedProperty : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ImportedProperty(ZPropertyInfo property, ZString caption, PropertyDisplayValueDelegate displayValueDelegate)
			: this(property, caption)
		{
			this.displayValueDelegate = displayValueDelegate;
		}

		public ImportedProperty(ZPropertyInfo property, ZString caption, PropertyDisplayValueDelegate displayValueDelegate, ImportedPropertyState? forcedState)
			: this(property, property.Name, caption, forcedState)
		{
			this.displayValueDelegate = displayValueDelegate;
		}

		public ImportedProperty(ZPropertyInfo property, ZString caption)
			: this(property, property.Name, caption, null)
		{
		}

		public ImportedProperty(ZPropertyInfo property, ZString name, ZString caption)
			: this(property, name, caption, null)
		{
		}

		public ImportedProperty(ZPropertyInfo property, ZString caption, ImportedPropertyState? forcedState)
			: this(property, property.Name, caption, forcedState)
		{
		}

		public ImportedProperty(ZPropertyInfo property, ZString name, ZString caption, ImportedPropertyState? forcedState)
			: base()
		{
			Init(property, name, caption, forcedState);
		}

		public delegate string PropertyDisplayValueDelegate();

		void Init(ZPropertyInfo property, ZString nameString, ZString captionString, ImportedPropertyState? forcedState)
		{
			if (!forcedState.HasValue && !property.IsPersistent)
			{
				throw new ArgumentException("You must specify the ImportedPropertyState manually if the property is not persistent.");
			}
			this.caption = captionString;
			this.name = nameString;
			this.value = property.Value;
			this.state = GetState(property, forcedState);
		}

		public IZType Value
		{
			get { return value; }
		}

		public ZString DisplayValue
		{
			get
			{
				ZString result;
				if (Value is ZDateTime)
				{
					result = ((ZDateTime)Value).ToLongTimeString();
				}
				else
				{
					result = displayValueDelegate != null ? displayValueDelegate() : Value.ToString();
				}
				return result;
			}
		}

		public ZString Caption
		{
			get { return caption; }
		}

		public ZString Name
		{
			get { return name; }
		}

		public ImportedPropertyState State
		{
			get { return state; }
		}

		public override string ToString()
		{
			return DisplayValue;
		}

		ImportedPropertyState GetState(ZPropertyInfo property, ImportedPropertyState? forcedState)
		{
			ImportedPropertyState result;
			if (forcedState.HasValue)
			{
				result = forcedState.Value;
			}
			else if (!property.BizObj.IsInDatabase)
			{
				result = ImportedPropertyState.New;
			}
			else if (property.HasChanges)
			{
				result = ImportedPropertyState.Modified;
			}
			else
			{
				result = ImportedPropertyState.Unchanged;
			}
			return result;
		}

		ZString caption;
		ZString name;
		IZType value;
		ImportedPropertyState state;
		readonly PropertyDisplayValueDelegate displayValueDelegate;
	}
}

