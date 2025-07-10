using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(BulkConsolAWBActions))]
	sealed class BulkConsolAWBActionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadAndSaveSettings()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SouthAfrica))
			{
				BulkConsolAWBActions actions1 = new BulkConsolAWBActions(Factory);
				actions1.PrintMasterAirWaybill = ZBool.True;
				actions1.MAWBPrinter = printer.PK;
				actions1.MAWBUseEPrint = ZBool.False;
				actions1.NeutralAWB = ZBool.False;
				actions1.CarrierAWB = ZBool.True;
				actions1.LaserAWB = ZBool.False;
				actions1.SendFWB = ZBool.True;
				actions1.SendFHL = ZBool.False;
				actions1.PrintConsignmentSecurityDeclaration = ZBool.True;

				actions1.SaveSettings();

				BulkConsolAWBActions actions2 = new BulkConsolAWBActions(Factory);
				actions2.LoadSettings();

				Assert(actions2.PrintMasterAirWaybill);
				AssertEquals(printer.PK, actions2.MAWBPrinter);
				Assert(!actions2.MAWBUseEPrint);
				Assert(!actions2.NeutralAWB);
				Assert(actions2.CarrierAWB);
				Assert(!actions2.LaserAWB);
				Assert(actions2.SendFWB);
				Assert(!actions2.SendFHL);
				Assert(!actions2.PrintConsignmentSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				BulkConsolAWBActions actions1 = new BulkConsolAWBActions(Factory);
				actions1.PrintConsignmentSecurityDeclaration = ZBool.True;

				actions1.SaveSettings();

				BulkConsolAWBActions actions2 = new BulkConsolAWBActions(Factory);
				actions2.LoadSettings();

				Assert(!actions2.PrintConsignmentSecurityDeclaration);
			}
		}

		public void TestDisabledPropertiesWhenNotPrintingMasterAirWaybill()
		{
			BulkConsolAWBActions actions = new BulkConsolAWBActions(Factory);
			StmPrintQueue printer = Factory.New<StmPrintQueue>();

			actions.PrintMasterAirWaybill = ZBool.True;
			actions.AllowPrintWithMessageErrors = ZBool.True;
			actions.MAWBPrinter = printer.PK;

			Assert(actions.AllowPrintWithMessageErrors);
			AssertEquals(printer.PK, actions.MAWBPrinter);

			actions.PrintMasterAirWaybill = ZBool.False;

			Assert(!actions.AllowPrintWithMessageErrors);
			AssertEquals(ZGuid.Empty, actions.MAWBPrinter);
		}

		public void TestValidateMAWBPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var actions = new BulkConsolAWBActions(Factory);
			actions.PrintMasterAirWaybill = true;
			actions.MAWBPrinter = ZGuid.Empty;
			Assert(actions.MAWBPrinterInfo.HasErrors());

			actions.MAWBPrinter = printer.PK;
			Assert(!actions.MAWBPrinterInfo.HasErrors());

			actions.MAWBPrinter = ZGuid.Empty;
			Assert(actions.MAWBPrinterInfo.HasErrors());

			actions.MAWBUseEPrint = true;
			Assert(!actions.MAWBPrinterInfo.HasErrors());
		}

		public void TestMAWBPrinter_Readonlyness()
		{
			var actions = new BulkConsolAWBActions(Factory);
			actions.PrintMasterAirWaybill = false;
			actions.MAWBUseEPrint = false;
			Assert(actions.MAWBPrinterInfo.ReadOnly);

			actions.PrintMasterAirWaybill = true;
			Assert(!actions.MAWBPrinterInfo.ReadOnly);

			actions.MAWBUseEPrint = true;
			Assert(actions.MAWBPrinterInfo.ReadOnly);
		}

		public void TestValidateMAWBUseEPrint()
		{
			var actions = new BulkConsolAWBActions(Factory);
			actions.PrintMasterAirWaybill = true;

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				actions.MAWBPrinter = ZGuid.NewZGuid();
				Assert(!actions.MAWBPrinter.IsEmpty);

				actions.MAWBUseEPrint = false;
				Assert(!actions.MAWBPrinter.IsEmpty);
				AssertNoErrors(actions.MAWBUseEPrintInfo);

				actions.MAWBUseEPrint = true;
				Assert(actions.MAWBPrinter.IsEmpty);
				AssertHasErrors(actions.MAWBUseEPrintInfo);
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				actions.MAWBPrinter = ZGuid.NewZGuid();
				Assert(!actions.MAWBPrinter.IsEmpty);

				actions.MAWBUseEPrint = false;
				Assert(!actions.MAWBPrinter.IsEmpty);
				AssertNoErrors(actions.MAWBUseEPrintInfo);

				actions.MAWBUseEPrint = true;
				Assert(actions.MAWBPrinter.IsEmpty);
				AssertNoErrors(actions.MAWBUseEPrintInfo);
			}
		}

		public void TestMAWBUseEPrint_Readonlyness()
		{
			var actions = new BulkConsolAWBActions(Factory);
			actions.PrintMasterAirWaybill = false;
			Assert(actions.MAWBUseEPrintInfo.ReadOnly);

			actions.PrintMasterAirWaybill = true;
			Assert(!actions.MAWBUseEPrintInfo.ReadOnly);
		}

		public void TestValidateSendFHL()
		{
			BulkConsolAWBActions actions = new BulkConsolAWBActions(Factory);
			actions.SendFWB = ZBool.False;
			actions.SendFHL = ZBool.True;
			AssertHasError(actions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutFWB);

			actions.SendFWB = ZBool.True;
			AssertNoError(actions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutFWB);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkConsolAWBActions(Factory);
		}

		#endregion
	}
}
