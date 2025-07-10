using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class UniversalXmlContentFilterDataObjectWriterFactoryTest : TestCaseWithFactory
	{
		public void TestForShipment()
		{
			var purpose = Factory.NewWithValidTestData<EDIMessagePurpose>();
			purpose.EMP_Code = "BOG";
			var filter = Factory.NewWithValidTestData<EDIMessageContentFilter>();
			purpose.EMP_ECF_Filter = filter.PK;

			var r = new UniversalXmlContentFilterDataObjectWriterFactory().Load(Factory, "BOG", Integration.EDIMessageContentFilterSchemaType.Shipment);

			AssertEquals(true, r is UniversalXmlContentFilterDataObjectWriter);
		}

		public void TestForEvent()
		{
			var purpose = Factory.NewWithValidTestData<EDIMessagePurpose>();
			purpose.EMP_Code = "BOG";
			var filter = Factory.NewWithValidTestData<EDIMessageContentFilter>();
			purpose.EMP_ECF_Filter = filter.PK;

			var r = new UniversalXmlContentFilterDataObjectWriterFactory().Load(Factory, "BOG", Integration.EDIMessageContentFilterSchemaType.Event);

			AssertEquals(true, r is UniversalXmlContentFilterDataObjectWriter);
		}

		public void TestNoPurpose()
		{
			var r = new UniversalXmlContentFilterDataObjectWriterFactory().Load(Factory, "BOG", Integration.EDIMessageContentFilterSchemaType.Event);

			AssertEquals(false, r is UniversalXmlContentFilterDataObjectWriter);
		}
	}
}
