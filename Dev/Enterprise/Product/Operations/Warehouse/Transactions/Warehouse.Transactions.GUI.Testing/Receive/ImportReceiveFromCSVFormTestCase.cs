using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(ImportReceiveFromCSVForm))]
	internal class ImportReceiveFromCSVFormTestCase : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportReceiveFromCSVForm();
		}

		public void TestFormHeading()
		{
			using (var testForm = new ImportReceiveFromCSVForm())
			{
				AssertEquals("Form text", "Import Warehouse Receive Data", Res.GetString("ImportReceiveFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Warehouse Receive Data"));
			}
		}

		void PopulateTestFile(TempFile tempFile)
		{
			var helper = new Business.Testing.WhsTestHelperFunctions(Factory);

			var client = helper.CreateClient("CLIENT", "CLIENT");
			helper.CreateWarehouse("WHS");
			helper.CreateProduct(client, "P01");

			using (var sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("CustomsEntryNo,ClientCode,Warehouse,Reference,ArrivalDate,ProductCode,Quantity,QuantityUQ,Pallets,Location,Attribute1,Attribute2,Attribute3,ExpiryDate,PackingDate,CustomsEntryLineNo,CustomsEntryDate,CustomsAddInfo,CustomsQty,CustomsUQ,CtryOfOrigin,ValueForDuty,BondedWhsQty,BondedWhsUQ,TILV,CustomsSecondQuantity,CustomsSecondUnitQty,Tariff,PrimaryPreference,CustomsThirdQuantity,CustomsThirdUnitQty,ManufacturerCode,ZoneStatus,IsFromOtherFTZWarehouse,OutwardType,SerialNumber,ReceiveCategory");
				sw.WriteLine(",CLIENT,WHS,REF1234,,P01,10,,,,,,,,,1,,,20,,200,AU,,,,,,,,,,,,,,");
				sw.Flush();
			}
		}

		void PopulateTestFile_ValueTooLong(TempFile tempFile)
		{
			var helper = new Business.Testing.WhsTestHelperFunctions(Factory);

			var client = helper.CreateClient("CLIENT", "CLIENT");
			helper.CreateWarehouse("WHS");
			helper.CreateProduct(client, "P01");
			Factory.Save();

			using (var sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("CustomsEntryNo,ClientCode,Warehouse,Reference,ArrivalDate,ProductCode,Quantity,QuantityUQ,Pallets,Location,Attribute1,Attribute2,Attribute3,ExpiryDate,PackingDate,CustomsEntryLineNo,CustomsEntryDate,CustomsAddInfo,CustomsQty,CustomsUQ,CtryOfOrigin,ValueForDuty,BondedWhsQty,BondedWhsUQ,TILV,CustomsSecondQuantity,CustomsSecondUnitQty,Tariff,PrimaryPreference,CustomsThirdQuantity,CustomsThirdUnitQty,ManufacturerCode,ZoneStatus,IsFromOtherFTZWarehouse,OutwardType,SerialNumber,ReceiveCategory");
				sw.WriteLine(",CLIENT,WHS,REF1234,,P01,10,,,,1234567890123456789012345678901,,,,,1,,,20,,200,AU,,,,,,,,,,,,,,");
				sw.Flush();
			}
		}

		public void TestConfirmOKLoadData()
		{
			using (var tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (var testForm = new ImportReceiveFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBoxForTest.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButtonForTest.PerformClick();

					Assert(testForm.CopyLogToClipboardButtonForTest.Enabled);
					Assert(testForm.CopyLogToClipboardButtonForTest.Visible);
					Assert(testForm.CloseButtonForTest.Enabled);
					Assert(testForm.CloseButtonForTest.Visible);
					Assert(testForm.OutputListBoxForTest.Items.Count > 0);

					AssertEquals("Progress Bar total has not been set", 2, testForm.ProgressBarForTest.Maximum);
					AssertEquals("Progress Bar has not been updated", 2, testForm.ProgressBarForTest.Value);

					var logData = testForm.GetLogForTest();
					Assert("Log Data not as expected", logData.StartsWith("Inventories to Import = 1", StringComparison.Ordinal));
					var logDataOutputFileName = testForm.CreateLogInDataDirectoryForTest(logData);
					try
					{
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						File.Delete(logDataOutputFileName);
					}
				}
			}
		}

		public void TestLoadData_ValueTooLong()
		{
			using (var tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile_ValueTooLong(tempFile);
				using (var testForm = new ImportReceiveFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBoxForTest.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButtonForTest.PerformClick();
					var logData = testForm.GetLogForTest();
					AssertStartsWith("Log Data not as expected", "Inventories to Import = 1", logData);
					AssertContains("The maximum length for Attribute 1", logData);
					AssertContains("has been exceeded", logData);
				}
			}
			ErrorReporter.Clear();
		}
	}
}
