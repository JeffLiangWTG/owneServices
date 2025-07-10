using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	public abstract class AWBActionsTest : NonPersistentBusinessObjectTestCase, ISerializeAWBTest
	{
		public abstract void TestLoadSettings();
		public abstract void TestSaveSettings();

		#region TestDoPrintFiveInchBarcodeLabel

		public virtual void TestDoPrintFiveInchBarcodeLabel()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.LabelPrinter = printer.PK;
			AWBActions.PrintBarcodeLabel = ZBool.True;

			AWBActions.PrintAWBBarcodeLabel();
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);
			AssertEquals(nameof(PrintType.PRN), printJobs[0].SP_JobType);
			AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals(printer.PK, printJobs[0].SP_SQ);
			AssertEquals(string.Empty, printJobs[0].SP_FaxDestination);
		}

		public virtual void TestDoPrintFiveInchBarcodeLabel_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				AWBActions.PrintBarcodeLabel = true;
				AWBActions.LabelUseEPrint = true;

				AWBActions.PrintAWBBarcodeLabel();
				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(1, printJobs.Length);
				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);
			}
		}

		#endregion

		#region TestDoPrintSixInchBarcodeLabel

		public virtual void TestDoPrintSixInchBarcodeLabel()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.LabelPrinter = printer.PK;
			AWBActions.PrintBarcodeLabel = ZBool.True;

			AWBActions.PrintAWBBarcodeLabel();
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			BusinessObject[] printJobs = Factory.Load(typeof(StmPrintJob), filter);
			AssertEquals(1, printJobs.Length);
		}

		#endregion

		#region TestPrintBarcodeLabel

		public void TestPrintBarcodeLabel()
		{
			AWBActions.PrintBarcodeLabel = ZBool.False;
			Assert(AWBActions.LabelPrinterInfo.ReadOnly);

			AWBActions.PrintBarcodeLabel = ZBool.True;
			Assert(!AWBActions.LabelPrinterInfo.ReadOnly);
		}

		#endregion

		#region TestFiveInchLabel

		public void TestFiveInchLabel()
		{
			AWBActions.FiveInchLabel = true;
			AssertEquals(true, AWBActions.FiveInchLabel);
			AWBActions.FiveInchLabel = false;
			AssertEquals(false, AWBActions.FiveInchLabel);
		}

		#endregion

		#region TestSixInchLabel

		public void TestSixInchLabel()
		{
			AWBActions.SixInchLabel = true;
			AssertEquals(true, AWBActions.SixInchLabel);

			AWBActions.SixInchLabel = false;
			AssertEquals(false, AWBActions.SixInchLabel);
		}

		#endregion

		#region TestLabelRangeFrom

		public void TestLabelRangeFrom()
		{
			AWBActions.LabelRangeFrom = 3;
			AssertEquals(3, AWBActions.LabelRangeFrom);
			AWBActions.LabelRangeFrom = 0;
			AssertEquals(true, AWBActions.LabelRangeFromInfo.HasErrors());

			AWBActions.TotalPacks = 7;
			AWBActions.LabelRangeFrom = 8;
			AssertEquals(true, AWBActions.LabelRangeFromInfo.HasErrors());

			AWBActions.LabelRangeFrom = 5;
			AssertEquals(false, AWBActions.LabelRangeFromInfo.HasErrors());
		}

		#endregion

		#region TestLabelRangeTo

		public void TestLabelRangeTo()
		{
			AWBActions.LabelRangeTo = 3;
			AssertEquals(3, AWBActions.LabelRangeTo);

			AWBActions.LabelRangeTo = 0;
			AssertEquals(true, AWBActions.LabelRangeToInfo.HasErrors());

			AWBActions.TotalPacks = 7;
			AWBActions.LabelRangeTo = 8;
			AssertEquals(true, AWBActions.LabelRangeToInfo.HasErrors());
			AWBActions.LabelRangeTo = 5;
			AssertEquals(false, AWBActions.LabelRangeToInfo.HasErrors());

			AWBActions.LabelRangeFrom = 6;
			AssertEquals(true, AWBActions.LabelRangeToInfo.HasErrors());

			AWBActions.LabelRangeFrom = 1;
			AssertEquals(false, AWBActions.LabelRangeToInfo.HasErrors());
			AWBActions.LabelRangeTo = 1;
			AssertEquals(false, AWBActions.LabelRangeToInfo.HasErrors());
		}

		#endregion

		#region TestTotalPacks

		public void TestTotalPacks()
		{
			AWB.AWBRateLines[0].ER_NoOfPiecesOrRCP = "10";
			AWB.AWBRateLines[1].ER_NoOfPiecesOrRCP = "10";
			AWBActions.AWBPackagesLabel = true;
			AssertEquals(20, AWBActions.TotalPacks);

			AWBActions.TotalPacks = 21;
			AssertEquals(true, AWBActions.TotalPacksInfo.HasErrors());

			AWBActions.TotalPacks = 0;
			AssertEquals(true, AWBActions.TotalPacksInfo.HasErrors());

			AWBActions.ParentPackagesLabel = true;
			AssertEquals(7, AWBActions.TotalPacks);

			AWBActions.TotalPacks = 9;
			AssertEquals(true, AWBActions.TotalPacksInfo.HasErrors());

			AWBActions.TotalPacks = 7;
			AssertEquals(false, AWBActions.TotalPacksInfo.HasErrors());
		}

		#endregion

		#region TestPackagesLabels

		public void TestAWBPackagesLabel()
		{
			AWB.AWBRateLines[0].ER_NoOfPiecesOrRCP = "10";
			AWB.AWBRateLines[1].ER_NoOfPiecesOrRCP = "10";

			AWBActions.AWBPackagesLabel = true;
			AssertEquals(20, AWBActions.TotalPacks);
			AssertEquals(20, AWBActions.LabelRangeTo);
		}

		public void TestParentPackagesLabel()
		{
			AWBActions.ParentPackagesLabel = true;
			AssertEquals(7, AWBActions.TotalPacks);
			AssertEquals(7, AWBActions.LabelRangeTo);
		}

		#endregion

		#region TestLabelPrinter

		public void TestLabelPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.PrintBarcodeLabel = true;
			AWBActions.LabelPrinter = ZGuid.Empty;
			Assert(AWBActions.LabelPrinterInfo.HasErrors());

			AWBActions.LabelPrinter = printer.PK;
			Assert(!AWBActions.LabelPrinterInfo.HasErrors());

			AWBActions.LabelPrinter = ZGuid.Empty;
			Assert(AWBActions.LabelPrinterInfo.HasErrors());

			AWBActions.LabelUseEPrint = true;
			Assert(!AWBActions.LabelPrinterInfo.HasErrors());
		}

		public void TestLabelPrinter_Readonlyness()
		{
			AWBActions.PrintBarcodeLabel = false;
			AWBActions.LabelUseEPrint = false;
			Assert(AWBActions.LabelPrinterInfo.ReadOnly);

			AWBActions.PrintBarcodeLabel = true;
			Assert(!AWBActions.LabelPrinterInfo.ReadOnly);

			AWBActions.LabelUseEPrint = true;
			Assert(AWBActions.LabelPrinterInfo.ReadOnly);
		}

		public void TestLabelUseEPrint()
		{
			AWBActions.PrintBarcodeLabel = true;

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AWBActions.LabelPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.LabelPrinter.IsEmpty);

				AWBActions.LabelUseEPrint = false;
				Assert(!AWBActions.LabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.LabelUseEPrintInfo);

				AWBActions.LabelUseEPrint = true;
				Assert(AWBActions.LabelPrinter.IsEmpty);
				AssertHasErrors(AWBActions.LabelUseEPrintInfo);
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				AWBActions.LabelPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.LabelPrinter.IsEmpty);

				AWBActions.LabelUseEPrint = false;
				Assert(!AWBActions.LabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.LabelUseEPrintInfo);

				AWBActions.LabelUseEPrint = true;
				Assert(AWBActions.LabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.LabelUseEPrintInfo);
			}
		}

		public void TestLabelUseEPrint_Readonlyness()
		{
			AWBActions.PrintBarcodeLabel = false;
			Assert(AWBActions.LabelUseEPrintInfo.ReadOnly);

			AWBActions.PrintBarcodeLabel = true;
			Assert(!AWBActions.LabelUseEPrintInfo.ReadOnly);
		}

		#endregion

		#region TestPrintOptionalInformation

		public void TestPrintOptionalInformation()
		{
			AssertEquals("Default true", true, AWBActions.PrintOptionalInformation);
		}

		#endregion

		#region TestPrinterNames

		public void TestPrinterNames()
		{
			AssertNotNull(AWBActions.PrinterNames);
		}

		#endregion

		#region TestDocumentSettings

		public virtual void TestDocumentSettings()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.LabelPrinter = printer.PK;

			AWBActions.FiveInchLabel = false;
			AWBActions.LabelRangeFrom = 3;
			AWBActions.LabelRangeTo = 7;
			AWBActions.TotalPacks = 11;
			AWBActions.PrintOptionalInformation = true;

			AWBActions.PrintAWBBarcodeLabel();

			AssertEquals("6 Inch", AWB.DocumentSize);
			AssertEquals(3, AWB.LabelStartRange);
			AssertEquals(7, AWB.LabelEndRange);
			AssertEquals(7, AWB.LabelTotalPacks);
			AssertEquals(3, AWB.MAWBLabelStartRange);
			AssertEquals(11, AWB.MAWBLabelTotalPacks);
			AssertEquals(true, AWB.PrintOptionalInformation);
			AssertEquals(true, AWB.DocumentSettingsPopulated);
		}

		#endregion

		#region Implementation

		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		protected override void SetUp()
		{
			CurrentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CurrentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CurrentCompanyCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchPort;
			base.TearDown();
		}

		protected StmPrintQueue CreatePrinter(Guid pk)
		{
			var printer = Factory.NewWithPrimaryKey<StmPrintQueue>(pk);
			printer.SQ_DisplayName = Guid.NewGuid().ToString();
			printer.SQ_QueueName = Guid.NewGuid().ToString();

			Factory.Save();

			return printer;
		}

		protected AWBActions AWBActions { get { return fAWBActions; } }
		protected AWBActions fAWBActions;
		protected ExportAWBHeader AWB;

		#endregion
	}
}
