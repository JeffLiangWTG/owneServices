using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FCC))]
	public class FCCTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FCC>
	{
		public void TestIFCC()
		{
			FCC.US_FCCImpCondNo = "01";
			AssertEquals(FCC.US_FCCImpCondNo, iFCC.ImportConditionNumber);

			iFCC.FCCLineNumber = 3;
			AssertEquals(3, iFCC.FCCLineNumber);

			FCC.US_FCCImpCondNoQtyAppr = true;
			AssertEquals(FCC.US_FCCImpCondNoQtyAppr, iFCC.ImportConditionNumberQuantityApproval);

			FCC.US_FCCID = "ID";
			AssertEquals(FCC.US_FCCID, iFCC.FCCIdentifier);

			FCC.US_FCCTradeName = "TRADE NAME";
			AssertEquals(FCC.US_FCCTradeName, iFCC.TradeName);

			FCC.US_FCCModel = "MODEL";
			AssertEquals(FCC.US_FCCModel, iFCC.ModelTypeNumber);

			FCC.US_FCCQty = 99;
			AssertEquals(FCC.US_FCCQty, iFCC.FCCQuantity);

			FCC.US_FCCWithhold = true;
			AssertEquals(FCC.US_FCCWithhold, iFCC.WithholdFromPublicInspectionRequested);

			FCC.US_FCCCommercialDesc = "COMM DESC";
			AssertEquals(FCC.US_FCCCommercialDesc, iFCC.CommercialDescription);
		}

		public void TestClone()
		{
			FCC.US_FCCID = "ID";
			var newFCC = (FCC)FCC.Clone();
			AssertEquals(FCC.US_FCCID, newFCC.US_FCCID);
			newFCC.B7_ParentTableCode = FCC.B7_ParentTableCode;
			newFCC.B7_ParentID = FCC.B7_ParentID;

			InvoiceLine.JI_Description = "JI_DESC";

			var fcc = InvoiceLine.FCCs.AddNew();
			fcc.US_FCCCommercialDesc = "COMMDESC";
			fcc.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._03;
			fcc.US_FCCImpCondNoQtyAppr = ZBool.True;
			fcc.US_FCCModel = "2014";
			fcc.US_FCCQty = 20m;
			fcc.US_FCCTradeName = "TRADE";
			fcc.US_FCCWithhold = ZBool.True;
			fcc.US_FCCID = "ID15";

			InvoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var fccNew = InvoiceLine.FCCs.AddNew();

			AssertEquals("FCCCommercialDesc must be the same", InvoiceLine.JI_Description, fccNew.US_FCCCommercialDesc);
			AssertEquals("FCCImpCondNo must be the same", fcc.US_FCCImpCondNo, fccNew.US_FCCImpCondNo);
			AssertEquals("FCCImpCondNoQtyAppr must be the same", fcc.US_FCCImpCondNoQtyAppr, fccNew.US_FCCImpCondNoQtyAppr);
			AssertEquals("FCCModel must be the same", fcc.US_FCCModel, fccNew.US_FCCModel);
			AssertEquals("FCCQty must be the same", fcc.US_FCCQty, fccNew.US_FCCQty);
			AssertEquals("FCCTradeName must be the same", fcc.US_FCCTradeName, fccNew.US_FCCTradeName);
			AssertEquals("FCCWithhold must be the same", fcc.US_FCCWithhold, fccNew.US_FCCWithhold);
			AssertEquals("FCCID must be the same", fcc.US_FCCID, fccNew.US_FCCID);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return FCC;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			return dec.Invoices.AddNew().InvoiceLines.AddNew().FCCs.AddNew();
		}

		protected override IEnumerable<FCC> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return dec.Invoices.AddNew().InvoiceLines.AddNew().FCCs.AddNew();
		}

		#endregion

		#region Implementation

		IFCC iFCC
		{
			get { return FCC; }
		}

		FCC FCC
		{
			get { return fcc ?? (fcc = InvoiceLine.FCCs.AddNew()); }
		}
		FCC fcc;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoiceHeader;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
