using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EthanolPermitNumberCusSupportingCollection))]
	sealed class EthanolPermitNumberCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<EthanolPermitNumberCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<EthanolPermitNumberCusSupporting> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			return new EthanolPermitNumberCusSupportingCollection(messageHeader);
		}

		[ExpectNoExceptions]
		public void TestAllowNewCore()
		{
			var testCollection = GetCusSupportingInfoCollection();
			for (var time = 0; time < 9; time++)
			{
				testCollection.AddNew();
				NUnit.Framework.Assert.That(testCollection.AllowNew, NUnit.Framework.Is.True);
			}

			testCollection.AddNew();
			NUnit.Framework.Assert.That(!testCollection.AllowNew, NUnit.Framework.Is.True);
		}
	}
}
