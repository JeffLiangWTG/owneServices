using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSDECEDIMessage))]
	sealed class CUSDECEDIMessageTest : SARSEDIMessageAbstractTest
	{
		public void TestDeclarationType()
		{
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_MessageText = ZAMessageTest.CUSDECTestMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "66";

			AssertEquals(ZString.Empty, cusdecMessage.DeclarationType);

			cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_MessageText = ZAMessageTest.CUSDECTestMessage.Replace("\r\n", "").Replace("BGM+929+01020304JSA20160708000064::00002+9'", "BGM+929:::RCD+01020304JSA20160708000064::00002+9'");
			cusdecMessage.EM_MessageNum = "66";

			AssertEquals("RCD", cusdecMessage.DeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_MessageText = ZAMessageTest.CUSDECTestMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "66";

			AssertEquals("01020304JSA20160708000064", cusdecMessage.LocalReferenceNumber);
		}

		public void TestParentMessageNumber()
		{
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_MessageText = ZAMessageTest.CUSDECTestMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "66";

			AssertEquals("66", cusdecMessage.ParentMessageNumber);
		}

		public void TestDefaultValues()
		{
			var testMessage = Factory.New<CUSDECEDIMessage>();
			AssertEquals("DEC", testMessage.EM_MessageType);
		}

		public void TestGetVPBAmount()
		{
			#region prep

			var newFactory = new BusinessObjectFactory();
			var newMessage = newFactory.NewWithValidTestData<ZAMessageForTest>();
			newMessage.EM_MessageType = "DEC";

			var vpb1 = newFactory.New<VPBAmountCodeData>();
			vpb1.CY_Code = "Line1";
			vpb1.CY_Data = "100";
			vpb1.CY_ParentTableCode = "EM";
			vpb1.CY_ParentID = newMessage.PK;

			var vpb2 = newFactory.New<VPBAmountCodeData>();
			vpb2.CY_Code = "Line2";
			vpb2.CY_Data = "200";
			vpb2.CY_ParentTableCode = "EM";
			vpb2.CY_ParentID = newMessage.PK;

			var vpbx = newFactory.New<VPBAmountCodeData>();
			vpbx.CY_Code = "Linex";
			vpbx.CY_Data = "300";
			vpbx.CY_ParentTableCode = "EM";
			vpbx.CY_ParentID = newMessage.PK;

			var vpbinvalid = newFactory.New<VPBAmountCodeData>();
			vpbinvalid.CY_Code = "whatever";
			vpbinvalid.CY_Data = "400";
			vpbinvalid.CY_ParentTableCode = "EM";
			vpbinvalid.CY_ParentID = newMessage.PK;

			var nonvpb = newFactory.New<CaseNumber>();
			nonvpb.CY_Type = "OTH";
			nonvpb.CY_Code = "Line5";
			nonvpb.CY_Data = "500";
			nonvpb.CY_ParentTableCode = "EM";
			nonvpb.CY_ParentID = newMessage.PK;

			var vpb6 = newFactory.New<CaseNumber>();
			vpb6.CY_Type = "VPB";
			vpb6.CY_Code = "Line6";
			vpb6.CY_Data = "600";
			vpb6.CY_ParentTableCode = "EM";
			vpb6.CY_ParentID = newMessage.PK;

			var vpb7 = newFactory.New<CaseNumber>();
			vpb7.CY_Type = "VPB";
			vpb7.CY_Code = "line7";
			vpb7.CY_Data = "700";
			vpb7.CY_ParentTableCode = "EM";
			vpb7.CY_ParentID = newMessage.PK;

			newFactory.Save();

			#endregion

			var testMessage = Factory.Load<CUSDECEDIMessage>(newMessage.PK);
			CombineAssertions(() =>
			{
				AssertEquals(6, testMessage.VPBAmounts.Count);
				AssertEquals(100m, testMessage.GetVPBAmountForLine("1"));
				AssertEquals(200m, testMessage.GetVPBAmountForLine("2"));
				AssertEquals(300m, testMessage.GetVPBAmountForLine("x"));
				AssertEquals(0m, testMessage.GetVPBAmountForLine("4"));
				AssertEquals(0m, testMessage.GetVPBAmountForLine("5"));
				AssertEquals(600m, testMessage.GetVPBAmountForLine("6"));
				AssertEquals(0m, testMessage.GetVPBAmountForLine("7"));
			});
		}

		public void TestSetVPBAmount()
		{
			var newFactory = new BusinessObjectFactory();
			var testHeader = newFactory.NewWithValidTestData<CusEntryHeader>();
			var testLine1 = testHeader.MergedLines.AddNew();
			testLine1.CL_LineNumber = 1;
			testLine1.CL_VPBAmount = 100m;
			var testLine2 = testHeader.MergedLines.AddNew();
			testLine2.CL_LineNumber = 2;
			var testLine3 = testHeader.MergedLines.AddNew();
			testLine3.CL_LineNumber = 3;
			testLine3.CL_VPBAmount = 301m;
			var testLine4 = testHeader.MergedLines.AddNew();
			testLine4.CL_LineNumber = 4;
			testLine4.CL_VPBAmount = 0m;

			var newMessage = newFactory.NewWithValidTestData<ZAMessageForTest>();
			newMessage.EM_MessageType = "DEC";
			testHeader.Messages.Add(newMessage);
			newFactory.Save();

			var testMessage = Factory.Load<CUSDECEDIMessage>(newMessage.PK);
			AssertEquals(0, testMessage.VPBAmounts.Count);
			testMessage.CopyVPBValues();
			AssertEquals(2, testMessage.VPBAmounts.Count);
			AssertEquals(100m, testMessage.GetVPBAmountForLine("1"));
			AssertEquals(0m, testMessage.GetVPBAmountForLine("2"));
			AssertEquals(301m, testMessage.GetVPBAmountForLine("3"));
			AssertEquals(0m, testMessage.GetVPBAmountForLine("4"));
		}
	}

	sealed class CUSDECEDIMessageForTest : CUSDECEDIMessage
	{
		internal string MessageNumForTesting { get; set; }

		public CUSDECEDIMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row) { }

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumForTesting ?? "0001";
		}
	}
}
