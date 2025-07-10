namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AABIX0Test : NUnit.Framework.TestCase
	{
		public void TestBlockData()
		{
			var block = "X0 BLOCK       1 REF ID:      286    AE YASYUSPRD_70018                         ";
			var x0 = new AABIX0();
			x0.Deserialise(block);
			var x01 = new AABIX01();
			x01.Deserialise(AABIX01.Get80ByteStringWithMandatoryCharacter(x0));
			AssertEquals("Filer code", "286", x01.FilerCode);
			AssertEquals("App ID", "AE", x01.ApplicationIdentifierCode);
			AssertEquals("User data", "YASYUSPRD_70018", x01.UserData);
		}
	}
}
