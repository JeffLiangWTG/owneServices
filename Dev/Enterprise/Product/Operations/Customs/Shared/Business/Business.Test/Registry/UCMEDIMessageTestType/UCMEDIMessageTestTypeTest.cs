using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(UCMEDIMessageTestType))]
	sealed class UCMEDIMessageTestTypeTest : Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase
	{
		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.ApplicationCodeInfo, UCMEDIMessageTestType.Schema.ApplicationCode, EDIMessage.Schema.EM_ApplicationCodeMaxLength);
			TestZPropertyInfo(BizObj.UCKDelayTimeInMillisecondsInfo, UCMEDIMessageTestType.Schema.UCKDelayTimeInMilliseconds);
			TestZPropertyInfo(BizObj.UCQDelayTimeInMillisecondsInfo, UCMEDIMessageTestType.Schema.UCQDelayTimeInMilliseconds);
			TestZPropertyInfo(BizObj.UCUDelayTimeInMillisecondsInfo, UCMEDIMessageTestType.Schema.UCUDelayTimeInMilliseconds);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName, int expectedMaxLength)
		{
			TestZPropertyInfo(propertyInfo, expectedName);
			AssertEquals("PropertyInfo.MaxLength", expectedMaxLength, propertyInfo.MaxLength);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var bizObj = BizObj;
			bizObj.ApplicationCode = "_T1";
			bizObj.UCKDelayTimeInMilliseconds = 10;
			bizObj.UCQDelayTimeInMilliseconds = 15;
			bizObj.UCUDelayTimeInMilliseconds = 26;
			return bizObj;
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => false;
		new UCMEDIMessageTestType BizObj => (UCMEDIMessageTestType)base.BizObj;
	}
}
