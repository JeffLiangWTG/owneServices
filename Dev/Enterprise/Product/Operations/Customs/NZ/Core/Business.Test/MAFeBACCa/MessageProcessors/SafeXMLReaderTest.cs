namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
	class SafeXmlReaderTest : TestCaseWithFactory
	{
		public void TestCanReadWellFormedXML()
		{
			SafeXmlReader<EBACCANotificationTypeType> reader = new SafeXmlReader<EBACCANotificationTypeType>();
			AssertEquals("reader.TryReadFromXML(ResponseMessages.RequestMoreInfoXML)", true, reader.TryReadFromXML(ResponseMessages.RequestMoreInfoXML));
			AssertEquals("reader.ErrorText", "", reader.ErrorText);
			AssertNotNull("reader.Result", reader.Result);
		}

		public void TestReturnsErrorOnBadlyFormedXML()
		{
			SafeXmlReader<EBACCANotificationTypeType> reader = new SafeXmlReader<EBACCANotificationTypeType>();
			AssertEquals("reader.TryReadFromXML(ResponseMessages.RequestMoreInfoXML)", false, reader.TryReadFromXML("Nothing useful here"));
			AssertEquals("reader.ErrorText", "There is an error in XML document (1, 1). Please contact the ECN support desk to solve this problem.", reader.ErrorText);
			AssertNull("reader.Result", reader.Result);
		}
	}
}
