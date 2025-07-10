using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ExportApplicationControlGeneratorCreatorTest : TestCaseWithFactory
	{
		[TestDate(1971, 9, 18)]
		public void TestNew()
		{
			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "364-33-1434";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = "320989";
			var creator = new ExportApplicationControlGeneratorCreator();
			var applicationControlGenerator = creator.New(ApplicationIdentifierCodeList.AES.CommodityShipment, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AESApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AESCommShipAXP), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AESCommShipZXP), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("Z    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.Z.Serialise());

			var applicationControlGenerator2 = creator.New("XN", GlbBranch.CurrentBranch);
			AssertNotEquals(applicationControlGenerator2, applicationControlGenerator);
			AssertEquals(typeof(AESApplicationControlGenerator), applicationControlGenerator2.GetType());
			AssertEquals(typeof(AESCommShipAXP), applicationControlGenerator2.A.GetType());
			AssertEquals(typeof(AESCommShipZXP), applicationControlGenerator2.Z.GetType());
			applicationControlGenerator2.AddMessage(message);
			AssertEquals("A    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator2.A.Serialise());
			AssertEquals("Z    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator2.Z.Serialise());
		}
	}
}
