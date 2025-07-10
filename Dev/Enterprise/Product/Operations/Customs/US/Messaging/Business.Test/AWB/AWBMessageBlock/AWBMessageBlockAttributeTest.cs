using CargoWise.Types;
using NUnit.Framework;
using ValueType = Enterprise.Messaging.Business.AWB.ValueType;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.Testing
{
	[TestsSubclassesOf(typeof(AWBMessageBlockAttribute))]
	abstract class AWBMessageBlockAttributeTest<T> : TestCase where T : AWBMessageBlockAttribute
	{
		public void TestGetElement()
		{
			var attribute = CreateAttribute();
			var invalidValue = attribute.InvalidValue;
			var element = attribute.GetElement(InvalidValue);
			AssertEquals("element.ValueType", ValueType.Value, element.ValueType);
			AssertEquals("element.Status", attribute.Status, element.Status);
			AssertEquals("element.Value", invalidValue, element.Value);
			AssertEquals("element.ToString()", invalidValue, element.ToString());
			var validValue = ValidValue;
			element = attribute.GetElement(attribute.DeSerialise(validValue));
			AssertEquals("element.ValueType", ValueType.Value, element.ValueType);
			AssertEquals("element.Status", attribute.Status, element.Status);
			AssertEquals("element.Value", validValue, element.Value);
			AssertEquals("element.ToString()", validValue, element.ToString());
		}

		public abstract void TestSerialise();

		public abstract void TestDeSerialise();

		protected abstract ZString ValidValue { get; }

		protected abstract IZType InvalidValue { get; }

		protected abstract T CreateAttribute();
	}
}
