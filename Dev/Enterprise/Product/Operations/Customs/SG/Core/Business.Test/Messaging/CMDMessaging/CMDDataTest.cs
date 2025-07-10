using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	public class CMDDataTest : TestCaseWithFactory
	{
		public void TestImportConsol()
		{
			var shipment = CreateShipment();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			CMDData data = new CMDData(shipmentWrapper);
			AssertNull("Shipment is not a transhipment", data.ImportConsol);
			ZString orgHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString orgCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "MYPKG";
				GlbCompany.CurrentCompany.SetCountry("MY");
				var consol1 = shipment.Consols.AddNew();
				consol1.JK_MasterBillNum = "1";
				consol1.JK_RL_NKLoadPort = "MYAOR";
				consol1.JK_RL_NKDischargePort = "AUSYD";
				var transport1 = consol1.Transports[0];
				transport1.JW_ATD = new ZDateTime(2004, 1, 1);
				transport1.JW_ATA = new ZDateTime(2004, 1, 2);
				var consol3 = shipment.Consols.AddNew();
				consol3.JK_MasterBillNum = "3";
				consol3.JK_RL_NKLoadPort = "MYAOR";
				consol3.JK_RL_NKDischargePort = "AUSYD";
				var transport3 = consol3.Transports[0];
				transport3.JW_ATD = new ZDateTime(2004, 1, 1);
				transport3.JW_ATA = new ZDateTime(2004, 1, 2);
				var helper = new TranshipmentHelper(shipment);
				AssertEquals("Sanity check to make sure that this shipment is transhipment", true, helper.TranshipmentPorts.Length > 0);
				data = new CMDData(shipmentWrapper);
				AssertNull(data.ImportConsol);
				var consol2 = shipment.Consols.AddNew();
				consol2.JK_MasterBillNum = "2";
				consol2.JK_RL_NKLoadPort = "AUSYD";
				consol2.JK_RL_NKDischargePort = "MYAOR";
				var transport2 = consol2.Transports[0];
				transport2.JW_ATD = new ZDateTime(2004, 2, 1);
				transport2.JW_ATA = new ZDateTime(2004, 2, 2);
				helper = new TranshipmentHelper(shipment);
				AssertEquals("Sanity check to make sure that this shipment is transhipment", true, helper.TranshipmentPorts.Length > 0);
				data = new CMDData(shipmentWrapper);
				AssertEquals("Import Consol should be Consol1", consol2.JK_MasterBillNum, data.ImportConsol.JK_MasterBillNum);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = orgHomePort;
				GlbCompany.CurrentCompany.SetCountry(orgCountry);
			}
		}

		public void TestHAWBSerialNo()
		{
			var shipment = CreateShipment();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			shipment.JS_HouseBill = "JS101";
			CMDData data = new CMDData(shipmentWrapper);
			AssertEquals("JS101", data.HAWBSerialNo);
			shipment.JS_HouseBill = "JS202";
			data = new CMDData(shipmentWrapper);
			AssertEquals("JS202", data.HAWBSerialNo);
		}

		public void TestHAWBNoOfPieces()
		{
			var shipment = CreateShipment();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			shipment.JS_TotalPackageCount = 0;
			shipment.JS_OuterPacks = 15;
			CMDData data = new CMDData(shipmentWrapper);
			AssertEquals(15, data.HAWBNoOfPieces);
			shipment.JS_TotalPackageCount = 5;
			data = new CMDData(shipmentWrapper);
			AssertEquals(5, data.HAWBNoOfPieces);
		}

		public void TestHAWBWeightCode()
		{
			ZString orgWeightUnit = Env.Registry.FreightWeightUnit;
			try
			{
				Env.Registry.FreightWeightUnit = Core.Constants.Weight.Pounds;
				ForwardingShipment shipment = CreateShipment();
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				CMDData data = new CMDData(shipmentWrapper);
				AssertEquals("L", data.HAWBWeightCode);
				Env.Registry.FreightWeightUnit = Core.Constants.Weight.Kilograms;
				data = new CMDData(shipmentWrapper);
				AssertEquals("K", data.HAWBWeightCode);
			}
			finally
			{
				Env.Registry.FreightWeightUnit = orgWeightUnit;
			}
		}

		public void TestHAWBGrossWeight()
		{
			var shipment = CreateShipment();
			shipment.JS_ActualWeight = 1;
			shipment.JS_DocumentedWeight = 2;
			shipment.JS_ManifestedWeight = 3;
			ZDecimal expectedWeight = shipment.GetWeightForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay);
			CMDData data = new CMDData(new CMDShipmentWrapper(shipment));
			AssertEquals(expectedWeight, data.HAWBGrossWeight);
		}

		#region TestHAWBNatureOfGoods
		public void TestHAWBNatureOfGoods()
		{
			var shipment = CreateShipment();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			AddPackLinesForTest(shipment);
			CMDData data = new CMDData(shipmentWrapper);
			AssertHAWBNatureOfGoodsFromPackLines(data);
			var declaration = CreateDeclaration(shipment, "I");
			ArrayList invoiceLines = CreateInvoiceLines(declaration, 5);
			Factory.Save();
			data = new CMDData(shipmentWrapper);
			AssertEquals("Should get the data from pack lines.", 3, data.HAWBNatureOfGoods.Length);
			declaration.JE_JS = shipment.PK;
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TEST";
			data = new CMDData(shipmentWrapper);
			AssertHAWBNatureOfGoodsFromPackLines(data);
		}

		public void TestHAWBNatureOfGoodsWhenTariffIsNull()
		{
			ForwardingShipment shipment = CreateShipment();
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			BaseJobDeclaration declaration = CreateDeclaration(shipment, "I");
			CreateInvoiceLines(declaration, 1, false, true);
			declaration.JE_JS = shipment.PK;
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TEST";
			CMDData data = new CMDData(shipmentWrapper);
			AssertEquals(0, data.HAWBNatureOfGoods.Length);
		}

		#region Assertion
		void AssertHAWBNatureOfGoodsFromPackLines(CMDData data)
		{
			AssertEquals(3, data.HAWBNatureOfGoods.Length);
			Array.Sort(data.HAWBNatureOfGoods, GoodsComparer.Instance);
			AssertEquals("Pack line 1", data.HAWBNatureOfGoods[0].ManifestDescription);
			AssertEquals("h0001", data.HAWBNatureOfGoods[0].HarmonisedCode);
			AssertEquals("Pack line 2", data.HAWBNatureOfGoods[1].ManifestDescription);
			AssertEquals("h0002", data.HAWBNatureOfGoods[1].HarmonisedCode);
			AssertEquals("Pack line 3", data.HAWBNatureOfGoods[2].ManifestDescription);
			AssertEquals("h0003", data.HAWBNatureOfGoods[2].HarmonisedCode);
		}

		#endregion
		#region Populate Test Data
		void AddPackLinesForTest(ForwardingShipment shipment)
		{
			for (int i = 0; i < 3; i++)
			{
				PackLine line = shipment.OuterPackLines.AddNew();
				line.JL_Description = string.Concat("Pack line ", i + 1);
				line.JL_HarmonisedCode = string.Format("h{0:0000}", i + 1);
			}
		}

		BaseJobDeclaration CreateDeclaration(CommonShipment shipment, ZString reference)
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = reference;
			return declaration;
		}

		ArrayList CreateInvoiceLines(BaseJobDeclaration declaration, int count)
		{
			return CreateInvoiceLines(declaration, count, true, true);
		}

		ArrayList CreateInvoiceLines(BaseJobDeclaration declaration, int count, bool populateTariff, bool populateDescription)
		{
			ArrayList invoiceLines = new ArrayList(count);
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JZ_AddInfo = "Testing 1	2 3";
			header.JZ_JE = declaration.PK;
			for (int i = 0; i < count; i++)
			{
				BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				line.JI_JZ = header.PK;
				if (populateDescription)
				{
					line.JI_Description = string.Concat("Invoice ", declaration.JE_DeclarationReference, i + 1);
				}

				if (populateTariff)
				{
					line.JI_Tariff = string.Concat("HA", declaration.JE_DeclarationReference, i + 1);
				}

				invoiceLines.Add(line);
			}

			return invoiceLines;
		}

		#endregion
		#endregion
		public void TestGoodsDelivered()
		{
			var shipment = CreateShipment();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			CMDData data = new CMDData(shipmentWrapper);
			AssertEquals(ZBool.False, data.GoodsDelivered);
			var cartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			data = new CMDData(shipmentWrapper);
			AssertEquals(ZBool.False, data.GoodsDelivered);
			cartage.JP_DeliveryCartageCompleted = ZDateTime.MaxSmallDateTime;
			data = new CMDData(shipmentWrapper);
			AssertEquals(ZBool.False, data.GoodsDelivered);
			cartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 1, 1);
			data = new CMDData(shipmentWrapper);
			AssertEquals(ZBool.True, data.GoodsDelivered);
		}

		public void TestExemptionCodeAndRemarks()
		{
			var shipment = CreateShipmentForCMDDataTests();
			CMDData data = new CMDData(new CMDShipmentWrapper(shipment));
			AssertEquals(CustomsEntryTypeList.Singapore.SGExemption.Codes.MF, data.ExemptionCode);
			AssertEquals("MF101", data.ExemptionRemarks);
		}

		public void TestTDBPermitNos()
		{
			var shipment = CreateShipmentForCMDDataTests();
			shipment.CusEntryNumbers.Load();
			CMDData data = new CMDData(new CMDShipmentWrapper(shipment));
			AssertEquals(7, data.TDBPermitNos.Length);
			Array.Sort(data.TDBPermitNos);
			AssertEquals("IES101", data.TDBPermitNos[0]);
			AssertEquals("IES102", data.TDBPermitNos[1]);
			AssertEquals("IES103", data.TDBPermitNos[2]);
			AssertEquals("IES104", data.TDBPermitNos[3]);
			AssertEquals("IES105", data.TDBPermitNos[4]);
			AssertEquals("IES106", data.TDBPermitNos[5]);
			AssertEquals("IES107", data.TDBPermitNos[6]);
		}

		#region Implementation
		void InsertPermit(ForwardingShipment shipment, string permit)
		{
			InsertCMDData(shipment, CustomsEntryTypeList.Singapore.Permit, permit);
		}

		void InsertCertificate(ForwardingShipment shipment, string certificate)
		{
			InsertCMDData(shipment, CustomsEntryTypeList.Singapore.Certificate, certificate);
		}

		void InsertExemption(ForwardingShipment shipment, string exemptionType, string remarks)
		{
			InsertCMDData(shipment, exemptionType, remarks);
		}

		void InsertCMDData(ForwardingShipment shipment, string code, string value)
		{
			var permit = Factory.New<CMDPermitNumber>();
			permit.CY_ParentID = shipment.PK;
			permit.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			permit.CY_Code = code;
			permit.CY_Data = value;
		}

		ForwardingShipment CreateShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		ForwardingShipment CreateShipmentForCMDDataTests()
		{
			var shipment = CreateShipment();
			InsertPermit(shipment, "IES101");
			InsertPermit(shipment, "IES102");
			InsertPermit(shipment, "IES103");
			InsertPermit(shipment, "IES104");
			InsertPermit(shipment, "IES105");
			InsertPermit(shipment, "IES106");
			InsertPermit(shipment, "IES107");
			InsertCertificate(shipment, "CER101");
			InsertExemption(shipment, CustomsEntryTypeList.Singapore.SGExemption.Codes.MF, "MF101");
			//Factory.Save();
			return shipment;
		}

		#region GoodsComparer
		protected class GoodsComparer : IComparer
		{
			public static GoodsComparer Instance = new GoodsComparer();
			protected GoodsComparer()
			{
			}

			#region IComparer Members
			public int Compare(object x, object y)
			{
				string code1 = ((Goods)x).HarmonisedCode;
				string code2 = ((Goods)y).HarmonisedCode;
				return code1.CompareTo(code2);
			}
			#endregion
		}
		#endregion
		#endregion
	}
}
