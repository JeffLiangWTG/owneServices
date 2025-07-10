using System;
using System.ComponentModel;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class PropertyDescriptorForTest : PropertyDescriptor
	{
		public PropertyDescriptorForTest(string name, Attribute[] attrs) : base(name, attrs)
		{
		}

		public override Type ComponentType => throw new NotImplementedException();

		public override bool IsReadOnly => throw new NotImplementedException();

		public override Type PropertyType => throw new NotImplementedException();

		public override bool CanResetValue(object component)
		{
			throw new NotImplementedException();
		}

		public override object GetValue(object component)
		{
			throw new NotImplementedException();
		}

		public override void ResetValue(object component)
		{
			throw new NotImplementedException();
		}

		public override void SetValue(object component, object value)
		{
			throw new NotImplementedException();
		}

		public override bool ShouldSerializeValue(object component)
		{
			throw new NotImplementedException();
		}
	}
}
