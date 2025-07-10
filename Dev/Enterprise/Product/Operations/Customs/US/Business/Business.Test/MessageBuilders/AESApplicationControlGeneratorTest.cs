using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class AESApplicationControlGeneratorTest : TestCaseWithFactory
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
			var applicationControlGenerator = ApplicationControlGenerator.New(EDIMessage.ApplicationCodes.USCustomsExport, ApplicationIdentifierCodeList.AES.CommodityShipment, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AESApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AESCommShipAXP), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AESCommShipZXP), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("Z    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.Z.Serialise());

			applicationControlGenerator = ApplicationControlGenerator.New(EDIMessage.ApplicationCodes.USCustomsExport, "XN", GlbBranch.CurrentBranch);
			AssertEquals(typeof(AESApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AESCommShipAXP), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AESCommShipZXP), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("Z    364331434      SXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 364331434                                 ", applicationControlGenerator.Z.Serialise());

			filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "36-123456700";
			filer.EntryFilerIDType = "E";

			using (USCustomsDataRegistry.Instance.ExportEntryFilerID.DataType.SuspendValidation())
			{
				USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			}

			applicationControlGenerator = ApplicationControlGenerator.New(EDIMessage.ApplicationCodes.USCustomsExport, ApplicationIdentifierCodeList.AES.CommodityShipment, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AESApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AESCommShipAXP), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AESCommShipZXP), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A    361234567      EXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 361234567                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("Z    361234567      EXP19710918" + AESTIRMessageNumberEncoder.Encode(320989).PadRight(6) + " 361234567                                 ", applicationControlGenerator.Z.Serialise());
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}
	}
}
