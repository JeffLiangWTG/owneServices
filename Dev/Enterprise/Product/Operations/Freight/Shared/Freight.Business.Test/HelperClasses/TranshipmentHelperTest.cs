using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TranshipmentHelperTest : BaseFreightTest
	{
		public void Test1TranshipmentWithNoDates()
		{
			Consol1.JK_RL_NKLoadPort = "NZAKL";
			Consol1.JK_RL_NKDischargePort = "AUSYD";
			Transport1.JW_ATD = ZDateTime.Empty;
			Transport1.JW_ATA = ZDateTime.Empty;
			Consol2.JK_RL_NKLoadPort = "AUSYD";
			Consol2.JK_RL_NKDischargePort = "MYPKG";
			Transport2.JW_ATD = ZDateTime.Empty;
			Transport2.JW_ATA = ZDateTime.Empty;
			Consol3.JK_RL_NKLoadPort = "MYPKG";
			Consol3.JK_RL_NKDischargePort = "AUMEL";
			Transport3.JW_ATD = ZDateTime.Empty;
			Transport3.JW_ATA = ZDateTime.Empty;
			Consol4.Delete();

			CommonConsol[] consolsInOrder = Helper.ConsolsInShippingOrder;
			AssertEquals("1", consolsInOrder[0].JK_MasterBillNum);
			AssertEquals("2", consolsInOrder[1].JK_MasterBillNum);
			AssertEquals("3", consolsInOrder[2].JK_MasterBillNum);
		}

		public void TestConsolsInShippingOrder()
		{
			CommonConsol[] consolsInOrder = Helper.ConsolsInShippingOrder;
			AssertEquals("1", consolsInOrder[0].JK_MasterBillNum);
			AssertEquals("2", consolsInOrder[1].JK_MasterBillNum);
			AssertEquals("3", consolsInOrder[2].JK_MasterBillNum);
			AssertEquals("4", consolsInOrder[3].JK_MasterBillNum);
		}

		public void TestGetConsolsInShippingOrder_WhenOnly1Consol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "x";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "MYPKG";

			AssertEquals("Should only be 1 consol", 1, new TestTranshipmentHelper(shipment).ConsolsInShippingOrder.Length);
			AssertEquals("Correct consol", "x", new TestTranshipmentHelper(shipment).ConsolsInShippingOrder[0].JK_MasterBillNum);
		}

		public void TestTranshipmentPorts()
		{
			AssertEquals("Should be 3 points of transhipment", 6, Helper.TranshipmentPorts.Length);
			AssertEquals("Transhipment point 1", "AUSYD", Helper.TranshipmentPorts[0]);
			AssertEquals("Transhipment point 2", "AUADL", Helper.TranshipmentPorts[1]);
			AssertEquals("Transhipment point 3", "MYPKG", Helper.TranshipmentPorts[2]);
			AssertEquals("Transhipment point 4", "MYAOR", Helper.TranshipmentPorts[3]);
			AssertEquals("Transhipment point 5", "AUMEL", Helper.TranshipmentPorts[4]);
			AssertEquals("Transhipment point 6", "AUPER", Helper.TranshipmentPorts[5]);
		}

		public void TestTranshipmentPorts_WhenOnly1Consol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "MYPKG";
			AssertEquals("When only 1 consol, there are no transhipment points", 0, new TranshipmentHelper(shipment).TranshipmentPorts.Length);
		}

		#region Validation

		public void TestValidateShipment_ForValidShipment()
		{
			string[] errors = Helper.ValidateShipment();
			AssertEquals("Valid CommonShipment should have no errors", 0, errors.Length);
		}

		public void TestValidateShipment_ForSingleNonTranshippedConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			CommonConsol importConsol = shipment.Consols.AddNew();
			importConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			importConsol.JK_RL_NKLoadPort = "SGSIN";
			importConsol.JK_RL_NKDischargePort = "MYPKG";

			CommonConsol exportConsol = shipment.Consols.AddNew();
			exportConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			exportConsol.JK_RL_NKLoadPort = "MYPKG";
			exportConsol.JK_RL_NKDischargePort = "AUMEL";

			Helper = new TestTranshipmentHelper(shipment);
			string[] errors = Helper.ValidateShipment();
			AssertEquals("Valid shipment should have no errors", 0, errors.Length);
		}

		public void TestValidateShipment_ForShipmentWithNoConsolsInCurrentCountry()
		{
			string oldHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "SMSAI"; // some random unused country
				GlbCompany.CurrentCompany.SetCountry("SM");

				Shipment.JS_RL_NKOrigin = "AUSYD";
				Shipment.JS_RL_NKDestination = "MYPKG";

				Consol1.JK_RL_NKLoadPort = "AUSYD";
				Consol1.JK_RL_NKDischargePort = "NZAKL";
				Consol2.JK_RL_NKLoadPort = "NZAKL";
				Consol2.JK_RL_NKDischargePort = "SGSIN";
				Consol3.JK_RL_NKLoadPort = "SGSIN";
				Consol3.JK_RL_NKDischargePort = "MYBAG";
				Consol4.JK_RL_NKLoadPort = "MYBAG";
				Consol4.JK_RL_NKDischargePort = "NLAMS";

				string[] errors = Helper.ValidateShipment();
				AssertEquals("Shipment should have 2 errors", 2, errors.Length);
				Assert("Should validate the no consols are neither import nor export", errors[0].IndexOf("no import consols") != -1);
				Assert("Should validate the no consols are neither import nor export", errors[1].IndexOf("no export consols") != -1);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = oldHomePort;
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestValidateShipment_NoStrayConsols()
		{
			Consol4.JK_RL_NKLoadPort = "SGSIN";
			Transport4.JW_ATD = ZDateTime.Empty;
			Transport4.JW_ATA = ZDateTime.Empty;

			Shipment.JS_RL_NKOrigin = "SMSAI";
			Shipment.JS_RL_NKDestination = "SGSIN";

			string[] errors = Helper.ValidateShipment();
			AssertEquals("Shipment should have 1 error", 1, errors.Length);
			Assert(
				"Should validate the all consols must link by it's port of loading/discharge pairs",
				errors[0].IndexOf("does not link to any other consol") != -1);
		}

		public void TestValidateShipment_NoOverlappingConsolDates()
		{
			Transport1.JW_ATD = new ZDateTime(2004, 1, 2);
			Transport1.JW_ATA = new ZDateTime(2004, 1, 5);
			Transport2.JW_ATD = new ZDateTime(2004, 1, 1);
			Transport2.JW_ATA = new ZDateTime(2004, 1, 4);

			string[] errors = Helper.ValidateShipment();
			AssertEquals("Shipment should have 1 error", 1, errors.Length);
			Assert(
				"Should validate that all consols cannot have overlapping consol dates",
				errors[0].IndexOf("overlaps") != -1);
		}

		public void TestValidateShipment_NoOverlappingConsolDates2()
		{
			Transport4.JW_ATD = new ZDateTime(2004, 1, 1);
			Transport4.JW_ATA = new ZDateTime(2004, 1, 4);
			Transport1.JW_ATD = new ZDateTime(2004, 1, 2);
			Transport1.JW_ATA = new ZDateTime(2004, 1, 5);

			string[] errors = Helper.ValidateShipment();
			AssertEquals("Shipment should have 1 error", 1, errors.Length);
			Assert(
				"Should validate that all consols cannot have overlapping consol dates",
				errors[0].IndexOf("overlaps") != -1);
		}

		#endregion

		#region Implementation

		string OldHomePort;
		string OldCountry;
		CommonShipment Shipment;
		CommonConsol Consol1;
		CommonConsol Consol2;
		CommonConsol Consol3;
		CommonConsol Consol4;
		Transport Transport1;
		Transport Transport2;
		Transport Transport3;
		Transport Transport4;

		TestTranshipmentHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			OldHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "MYPKG";
			OldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("MY");

			Shipment = CommonShipment.New(Factory);
			Consol3 = Shipment.Consols.AddNew();
			Consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Consol1 = Shipment.Consols.AddNew();
			Consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Consol2 = Shipment.Consols.AddNew();
			Consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Consol4 = Shipment.Consols.AddNew();
			Consol4.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport1 = Consol1.Transports[0];
			Transport2 = Consol2.Transports[0];
			Transport3 = Consol3.Transports[0];
			Transport4 = Consol4.Transports[0];

			Consol1.JK_MasterBillNum = "1";
			Consol1.JK_RL_NKLoadPort = "NZAKL";
			Consol1.JK_RL_NKDischargePort = "AUSYD";
			Transport1.JW_ATD = new ZDateTime(2004, 1, 1);
			Transport1.JW_ATA = new ZDateTime(2004, 1, 2);

			Consol2.JK_MasterBillNum = "2";
			Consol2.JK_RL_NKLoadPort = "AUADL";
			Consol2.JK_RL_NKDischargePort = "MYPKG";
			Transport2.JW_ATD = new ZDateTime(2004, 2, 1);
			Transport2.JW_ATA = new ZDateTime(2004, 2, 2);

			Consol3.JK_MasterBillNum = "3";
			Consol3.JK_RL_NKLoadPort = "MYAOR";
			Consol3.JK_RL_NKDischargePort = "AUMEL";
			Transport3.JW_ATD = new ZDateTime(2004, 3, 1);
			Transport3.JW_ATA = new ZDateTime(2004, 3, 2);

			Consol4.JK_MasterBillNum = "4";
			Consol4.JK_RL_NKLoadPort = "AUPER";
			Consol4.JK_RL_NKDischargePort = "SGSIN";
			Transport4.JW_ATD = new ZDateTime(2004, 4, 1);
			Transport4.JW_ATA = new ZDateTime(2004, 4, 2);

			Helper = new TestTranshipmentHelper(Shipment);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = OldHomePort;
			GlbCompany.CurrentCompany.SetCountry(OldCountry);
		}

		class TestTranshipmentHelper : TranshipmentHelper
		{
			public TestTranshipmentHelper(CommonShipment shipment)
				: base(shipment)
			{
			}

			public new CommonConsol[] ConsolsInShippingOrder
			{
				get { return base.ConsolsInShippingOrder; }
			}
		}

		#endregion
	}
}
