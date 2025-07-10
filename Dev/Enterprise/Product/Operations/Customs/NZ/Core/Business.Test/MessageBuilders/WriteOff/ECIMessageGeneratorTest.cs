using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Express.Testing;
	using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Express;
	using Enterprise.Customs.NZ.Registry;
	class ECIMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestLocationOfOriginWithoutData()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var mawb = Factory.New<CusMAWB>();
			var mawbCreator = new TestCusMAWBCreator(mawb, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			var hawb1 = mawbCreator.AddHAWB(mawbCreator.Supplier2, mawbCreator.Importer1, "HOUSEBILL3", "", "NZAKL", "RATS TEETH", 13.4m, 2, 27.72m);//origin empty
			hawb1.CS_RL_NKOrigin = ZString.Empty;

			var messageBuilder = new MessageBuilderFromCusMAWB(mawb, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GenerateMessage();

			var message = messageBuilder.MessageBusinessObject;
			AssertNotNull("Precondition: message", message);

			AssertNotContains("LOC+4", message.EM_MessageText);
		}

		public void TestHousebillLength()
		{
			var mawb = Factory.New<CusMAWB>();
			var mawbCreator = new TestCusMAWBCreator(mawb, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2017, 09, 07), new ZDateTime(2017, 09, 07));
			mawbCreator.AddHAWB(mawbCreator.Supplier2, mawbCreator.Importer1, "HBTESTWITHLONGWAYBILLNUMBEROF35CHAR", "AUSYD", "NZAKL", "RATS TEETH", 13.4m, 2, 27.72m);
			var messageBuilder = new MessageBuilderFromCusMAWB(mawb, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			var message = messageBuilder.MessageBusinessObject;
			AssertNotNull("Precondition: message generated", message);
			AssertContains("RFF+HWB:HBTESTWITHLONGWAYBILLNUMBEROF35CHAR", message.EM_MessageText);
		}
	}
}
