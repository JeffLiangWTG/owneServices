using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.Testing
{
	class MessageTypeListTest : TestCaseWithFactory
	{
		public void TestIsTSWCode()
		{
			Assert(MessageTypeList.IsTSWCode(Factory, MessageTypeList.Codes.ANA));
			Assert(!MessageTypeList.IsTSWCode(Factory, "AAA"));
			Assert(MessageTypeList.IsTSWCode(Factory, MessageTypeList.Codes.OCR));
			Assert(!MessageTypeList.IsTSWCode(Factory, "ORM"));
			Assert(MessageTypeList.IsTSWCode(Factory, "I10"));
			Assert(MessageTypeList.IsTSWCode(Factory, "E40"));
			Assert(MessageTypeList.IsTSWCode(Factory, "CRE"));
			Assert(MessageTypeList.IsTSWCode(Factory, "ICR"));
			Assert(!MessageTypeList.IsTSWCode(Factory, "ECI"));
		}
	}
}
