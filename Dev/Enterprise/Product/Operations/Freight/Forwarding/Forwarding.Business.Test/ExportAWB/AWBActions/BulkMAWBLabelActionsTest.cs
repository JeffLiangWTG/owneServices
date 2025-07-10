using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(BulkMAWBLabelActions))]
	sealed class BulkMAWBLabelActionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadAndSaveSettings()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();

			BulkMAWBLabelActions actions1 = new BulkMAWBLabelActions(Factory);
			actions1.PrintOptionalInformation = ZBool.True;
			actions1.FiveInchLabel = ZBool.False;
			actions1.SixInchLabel = ZBool.True;
			actions1.PackagesFromAWB = ZBool.False;
			actions1.PackagesFromConsol = ZBool.True;
			actions1.AWBLabelPrinter = printer.PK;
			actions1.AWBLabelUseEPrint = ZBool.False;
			actions1.NumberOfCopies = 3;

			actions1.SaveSettings();

			BulkMAWBLabelActions actions2 = new BulkMAWBLabelActions(Factory);
			actions2.LoadSettings();

			Assert(actions2.PrintOptionalInformation);
			Assert(!actions2.FiveInchLabel);
			Assert(actions2.SixInchLabel);
			Assert(!actions2.PackagesFromAWB);
			Assert(actions2.PackagesFromConsol);
			AssertEquals(printer.PK, actions2.AWBLabelPrinter);
			Assert(!actions2.AWBLabelUseEPrint);
			AssertEquals(3, actions2.NumberOfCopies);
		}

		public void TestValidateNumberOfCopies()
		{
			BulkMAWBLabelActions actions = new BulkMAWBLabelActions(Factory);
			actions.NumberOfCopies = 3;
			Assert(!actions.NumberOfCopiesInfo.HasErrors());

			actions.NumberOfCopies = 0;
			Assert(actions.NumberOfCopiesInfo.HasErrors());
		}

		public void TestValidateAWBLabelPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var actions = new BulkMAWBLabelActions(Factory);
			actions.AWBLabelPrinter = ZGuid.Empty;
			Assert(actions.AWBLabelPrinterInfo.HasErrors());

			actions.AWBLabelPrinter = printer.PK;
			Assert(!actions.AWBLabelPrinterInfo.HasErrors());

			actions.AWBLabelPrinter = ZGuid.Empty;
			Assert(actions.AWBLabelPrinterInfo.HasErrors());

			actions.AWBLabelUseEPrint = true;
			Assert(!actions.AWBLabelPrinterInfo.HasErrors());
		}

		public void TestAWBLabelPrinter_Readonlyness()
		{
			var actions = new BulkMAWBLabelActions(Factory);
			actions.AWBLabelUseEPrint = false;
			Assert(!actions.AWBLabelPrinterInfo.ReadOnly);
			actions.AWBLabelUseEPrint = true;
			Assert(actions.AWBLabelPrinterInfo.ReadOnly);
		}

		public void TestValidateAWBLabelUsePrint()
		{
			var actions = new BulkMAWBLabelActions(Factory);

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				actions.AWBLabelPrinter = ZGuid.NewZGuid();
				Assert(!actions.AWBLabelPrinter.IsEmpty);

				actions.AWBLabelUseEPrint = false;
				Assert(!actions.AWBLabelPrinter.IsEmpty);
				AssertNoErrors(actions.AWBLabelUseEPrintInfo);

				actions.AWBLabelUseEPrint = true;
				Assert(actions.AWBLabelPrinter.IsEmpty);
				AssertHasErrors(actions.AWBLabelUseEPrintInfo);
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				actions.AWBLabelPrinter = ZGuid.NewZGuid();
				Assert(!actions.AWBLabelPrinter.IsEmpty);

				actions.AWBLabelUseEPrint = false;
				Assert(!actions.AWBLabelPrinter.IsEmpty);
				AssertNoErrors(actions.AWBLabelUseEPrintInfo);

				actions.AWBLabelUseEPrint = true;
				Assert(actions.AWBLabelPrinter.IsEmpty);
				AssertNoErrors(actions.AWBLabelUseEPrintInfo);
			}
		}

		public void TestValidateFiveAndSixInchLabel()
		{
			BulkMAWBLabelActions actions = new BulkMAWBLabelActions(Factory);
			actions.FiveInchLabel = ZBool.False;
			actions.SixInchLabel = ZBool.False;
			actions.ValidateAll();
			Assert(actions.FiveInchLabelInfo.HasErrors());
			Assert(actions.SixInchLabelInfo.HasErrors());

			actions.FiveInchLabel = ZBool.True;
			Assert(!actions.FiveInchLabelInfo.HasErrors());
			Assert(!actions.SixInchLabelInfo.HasErrors());

			actions.FiveInchLabel = ZBool.False;
			actions.SixInchLabel = ZBool.True;
			Assert(!actions.FiveInchLabelInfo.HasErrors());
			Assert(!actions.SixInchLabelInfo.HasErrors());
		}

		public void TestValidatePackagesFromAWBAndConsol()
		{
			BulkMAWBLabelActions actions = new BulkMAWBLabelActions(Factory);
			actions.PackagesFromAWB = ZBool.False;
			actions.PackagesFromConsol = ZBool.False;
			actions.ValidateAll();
			Assert(actions.PackagesFromAWBInfo.HasErrors());
			Assert(actions.PackagesFromConsolInfo.HasErrors());

			actions.PackagesFromAWB = ZBool.True;
			Assert(!actions.PackagesFromAWBInfo.HasErrors());
			Assert(!actions.PackagesFromConsolInfo.HasErrors());

			actions.PackagesFromAWB = ZBool.False;
			actions.PackagesFromConsol = ZBool.True;
			Assert(!actions.PackagesFromAWBInfo.HasErrors());
			Assert(!actions.PackagesFromConsolInfo.HasErrors());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkMAWBLabelActions(Factory);
		}

		#endregion
	}
}
