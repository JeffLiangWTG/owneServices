using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLineCompleteCollectionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestInitialiseInvoiceLineDataIfNeeded_WhenInitialiseDataCreateNewLines()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			var collection = testDec.InvoiceLines;
			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();

			var invoiceLine = invoiceLineMock.Object;
			collection.Add(invoiceLine);

			BaseJobComInvoiceLine invoiceLine2 = null;
			var invoiceLineMockProtected = invoiceLineMock.Protected();
			invoiceLineMockProtected.Setup("InitialisePartSyncManager")
			.Callback(() =>
			{
				invoiceLine2 = testDec.InvoiceLines.AddNew();
				var hasInitialisedData2 = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(invoiceLine2);
				Assert("invoiceLine2 hasInitialisedData set in SetDefaultValues()", hasInitialisedData2);
				typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(invoiceLine2, false);
			});

			typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(invoiceLine, false);
			typeof(InvoiceLineCompleteCollection).GetField("hasInitialisedInvoiceLineData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(collection, false);

			CombineAssertions(() =>
			{
				typeof(InvoiceLineCompleteCollection).GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(collection, null);

				var hasInitialisedData = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(invoiceLine);
				Assert("invoiceLine hasInitialisedData", hasInitialisedData);

				var hasInitialisedData2 = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(invoiceLine2);
				Assert("invoiceLine2 was created durning InitialiseData(), do not call its InitialiseData() durning InitialiseInvoiceLineDataIfNeeded()", !hasInitialisedData2);
			});
		}

		public void TestSetDefaultFromPreviousLineForProcedureCode()
		{
			var declaration = Factory.NewMoq<BaseJobDeclaration>();
			declaration.Setup(d => d.ShouldCopyProcedureFromPreviousInvoiceLine).Returns(false);
			var invoiceLine1 = declaration.Object.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "10";
			var invoiceLine2 = declaration.Object.InvoiceLines.AddNew();
			AssertEquals("Procedure should not be copied if ShouldCopyProcedureFromPreviousInvoiceLine is false", string.Empty, invoiceLine2.JI_Procedure);

			var declaration2 = Factory.NewMoq<BaseJobDeclaration>();
			declaration2.Setup(d => d.ShouldCopyProcedureFromPreviousInvoiceLine).Returns(true);
			var invoiceLine3 = declaration2.Object.InvoiceLines.AddNew();
			invoiceLine3.JI_Procedure = "10";
			var invoiceLine4 = declaration2.Object.InvoiceLines.AddNew();
			AssertEquals("Procedure should be copied if ShouldCopyProcedureFromPreviousInvoiceLine is true", invoiceLine3.JI_Procedure, invoiceLine4.JI_Procedure);
		}

		public void TestCancelNewInvoiceLineDoesntModifyHasChanges()
		{
			var declaration = Factory.New<BaseJobDeclarationForTest>();
			var invoice = declaration.Invoices.AddNew();
			var package = declaration.Packages.AddNew();
			package.HasChanges = false;
			declaration.Packages.HasChanges = false;

			CombineAssertions(() =>
			{
				AssertEquals("No changes to collection initially", false, declaration.Packages.HasChanges);

				var line = (BaseJobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
				line.JI_JZ = invoice.PK;
				((ICancelAddNew)declaration.FilteredInvoiceLines).CancelNew(0);

				AssertEquals("No changes to collection after cancel edit", false, declaration.Packages.HasChanges);
				AssertEquals("No changes after cancel edit", false, declaration.Packages[0].HasChanges);
				AssertEquals("No changes to pivot collection after cancel edit", false, declaration.Packages[0].InvoiceLinePivotCollection.HasChanges);
			});
		}

		public void TestEndNewInvoiceLineModifyHasChanges()
		{
			var declaration = Factory.New<BaseJobDeclarationForTest>();
			var invoice = declaration.Invoices.AddNew();
			var package = (BasePackage)((IBindingList)declaration.Packages).AddNew();
			package.HasChanges = false;
			declaration.Packages.HasChanges = false;

			CombineAssertions(() =>
			{
				AssertEquals("No changes to pivot collection initially", false, declaration.Packages[0].InvoiceLinePivotCollection.HasChanges);

				var line = (BaseJobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
				line.JI_JZ = invoice.PK;
				line.JI_InvoiceQuantity = 10;
				((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(0);

				AssertEquals("Changes to pivot collection after cancel edit", true, declaration.Packages[0].InvoiceLinePivotCollection.HasChanges);
			});
		}

		class BaseJobDeclarationForTest : BaseJobDeclaration
		{
			public BaseJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;
		}
	}

	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : BusinessObjectCollectionTestCase
	{
		public virtual void TestSetDefaultValuesForTheFirstLineOnwardsWithOrderLineAttachedThatHasVeryLongOrderNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine's JI_JZ", invoiceHeader.PK, invoiceLine1.JI_JZ);
			invoiceLine1.JI_OrderNumber = "123-45674";

			var organization = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order = Factory.New<Order>();
			order.BuyerPK = organization.PK;
			order.SupplierPK = organization.PK;
			order.JD_OrderNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals("Pre-condition: order.JD_OrderNumber.Length", 26, order.JD_OrderNumber.Length);

			var orderLine = order.OrderLines.AddNew();
			invoiceLine1.JI_JO = orderLine.PK;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine2's JI_JZ copied from the first line", invoiceLine1.JI_JZ, invoiceLine2.JI_JZ);
			AssertEquals("invoiceLine2.JI_OrderNumber", order.JD_OrderNumberAndSplit.Right(invoiceLine2.JI_OrderNumberInfo.MaxLength), invoiceLine2.JI_OrderNumber);
		}

		protected virtual CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
		{
			return declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var declaration = Declaration;
			var isDeclarationWithEntryInstruction = !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction;

			Env.Registry.PackageVolumeUnit = "M3";
			Env.Registry.PackageWeightUnit = "KG";
			CusEntryInstruction testInstruction = null;
			if (isDeclarationWithEntryInstruction)
			{
				testInstruction = GetCusEntryInstruction(declaration);
				testInstruction.CEI_Style = "1";
			}

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals("JI_WeightUQ is defaulted", Core.Constants.Weight.Kilograms, invoiceLine.JI_WeightUQ);
			AssertEquals("KG", invoiceLine.JI_NetWeightUQ);
			AssertEquals("M3", invoiceLine.JI_VolumeUQ);
			AssertEquals(isDeclarationWithEntryInstruction ? testInstruction.PK : ZGuid.Empty, invoiceLine.JI_CEI);

			invoiceLine.JI_WeightUQ = "A";
			invoiceLine.JI_NetWeightUQ = "B";
			invoiceLine.JI_VolumeUQ = "C";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals(invoiceLine.JI_NetWeightUQ, invoiceLine2.JI_NetWeightUQ);
			AssertEquals(invoiceLine.JI_VolumeUQ, invoiceLine2.JI_VolumeUQ);
			AssertEquals(isDeclarationWithEntryInstruction ? testInstruction.PK : ZGuid.Empty, invoiceLine2.JI_CEI);

			invoiceLine2.JI_WeightUQ = "LB";
			invoiceLine2.JI_NetWeightUQ = "OZ";
			invoiceLine2.JI_VolumeUQ = "CF";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			AssertEquals("LB", invoiceLine3.JI_WeightUQ);
			AssertEquals("OZ", invoiceLine3.JI_NetWeightUQ);
			AssertEquals("CF", invoiceLine3.JI_VolumeUQ);
			AssertEquals(isDeclarationWithEntryInstruction ? testInstruction.PK : ZGuid.Empty, invoiceLine3.JI_CEI);

			if (isDeclarationWithEntryInstruction)
			{
				fDeclaration = null;
				declaration = Declaration;
				testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction.CEI_Style = "1";
				testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction.CEI_Style = "2";
				invoice = declaration.Invoices.AddNew();
				invoiceLine = declaration.InvoiceLines.AddNew();
				AssertEquals("Multiple Instruction, should not default JI_CEI for 1st Line", ZGuid.Empty, invoiceLine.JI_CEI);
			}
		}

		public virtual void TestSetDefaultValuesForTheFirstLineOnwards()
		{
			var declaration = Declaration;
			var isDeclarationWithEntryInstruction = !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction;

			var invoice1 = GetInvoiceHeaderFromDec(declaration);
			var invoice2 = GetInvoiceHeaderFromDec(declaration);
			CusEntryInstruction testInstruction1 = null;
			CusEntryInstruction testInstruction2 = null;
			if (isDeclarationWithEntryInstruction)
			{
				testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction1.CEI_Style = "1";
				testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction2.CEI_Style = "2";
			}
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine's JI_JZ", invoice1.PK, invoiceLine.JI_JZ);

			invoiceLine.JI_OrderNumber = "123-45674";
			if (isDeclarationWithEntryInstruction)
			{
				invoiceLine.JI_CEI = testInstruction2.PK;
			}

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine2's JI_JZ copied from the first line", invoiceLine.JI_JZ, invoiceLine2.JI_JZ);
			AssertEquals("Invoice line2's JI_OrderNumber copied from the first line", invoiceLine.JI_OrderNumber, invoiceLine2.JI_OrderNumber);
			AssertEquals("Invoice line2's JI_Calc_EntryInstruction copied from the first line", isDeclarationWithEntryInstruction ? invoiceLine.JI_CEI : ZGuid.Empty, invoiceLine2.JI_CEI);

			invoiceLine2.JI_JZ = invoice2.PK;
			if (isDeclarationWithEntryInstruction)
			{
				invoiceLine2.JI_CEI = testInstruction1.PK;
			}
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine3's JI_JZ copied", invoiceLine2.JI_JZ, invoiceLine3.JI_JZ);
			AssertEquals("Invoice line2's JI_Calc_EntryInstruction copied", isDeclarationWithEntryInstruction ? invoiceLine2.JI_CEI : ZGuid.Empty, invoiceLine3.JI_CEI);
		}

		public void TestDeletingInvoiceLineReapportionGroupCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var declaration = Declaration;
				var invoice = GetInvoiceHeaderFromDec(declaration);
				invoice.JZ_InvoiceAmount = 30000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var line1 = declaration.InvoiceLines.AddNew();
				line1.JI_LinePrice = 10000;

				var line2 = declaration.InvoiceLines.AddNew();
				line2.JI_LinePrice = 10000;

				var line3 = declaration.InvoiceLines.AddNew();
				line3.JI_LinePrice = 10000;

				BaseJobComInvHeaderCharge aDD = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, declaration.LocalCurrencyCode);
				PrepareCharge(aDD);
				declaration.ResumeApportionment();
				AssertEquals("PreCondition:Line1 has an apportioned ADD", 100m, line1.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("PreCondition:Line2 has an apportioned ADD", 100m, line2.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("PreCondition:Line3 has an apportioned ADD", 100m, line3.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);

				declaration.InvoiceLines.RemoveAndDelete(line3);
				declaration.ResumeApportionment();
				AssertEquals("Line1 has an apportioned ADD", 150m, line1.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("Line2 has an apportioned ADD", 150m, line2.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
			}
		}

		protected virtual void PrepareCharge(Common.JobComInvCharge charge)
		{
		}

		public void TestLoadRightLines()
		{
			var testDec = GetMeANewJobDeclaration();
			var invoice = GetInvoiceHeaderFromDec(testDec);
			var line1 = testDec.FilteredInvoiceLines.AddNew();
			var line2 = testDec.FilteredInvoiceLines.AddNew();
			var line3 = testDec.FilteredInvoiceLines.AddNew();

			var testDec2 = GetMeANewJobDeclaration();
			var invoice2 = GetInvoiceHeaderFromDec(testDec2);
			var line4 = testDec2.FilteredInvoiceLines.AddNew();

			AssertEquals("JI_JZ", invoice.PK, line1.JI_JZ);
			AssertEquals("JI_JZ", invoice.PK, line2.JI_JZ);
			AssertEquals("JI_JZ", invoice.PK, line3.JI_JZ);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<BaseJobDeclaration>(testDec.PK);
			AssertEquals("Three lines", 3, decLoaded.InvoiceLines.Count);
			AssertEquals("Three lines", 3, decLoaded.FilteredInvoiceLines.Count);
		}

		public void TestLoadWhenNoInvoicesAreUnderDeclaration()
		{
			var testDec = GetMeANewJobDeclaration();
			var invoice = GetInvoiceHeaderFromDec(testDec);
			var line = testDec.FilteredInvoiceLines.AddNew();

			AssertEquals("TestDec has one line", 1, testDec.InvoiceLines.Count);

			var otherDec = GetMeANewJobDeclaration();
			AssertEquals("OtherDec does not have any line", 0, otherDec.InvoiceLines.Count);
			AssertEquals("OtherDec does not have any line", 0, otherDec.FilteredInvoiceLines.Count);
		}

		public void TestOnLoaded()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			var collection = testDec.InvoiceLines;
			var line = collection.AddNew();
			typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(line, false);
			typeof(InvoiceLineCompleteCollection).GetField("hasInitialisedInvoiceLineData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(collection, false);

			CombineAssertions(() =>
			{
				collection.Load();
				var hasInitialisedInvoiceLineData = (bool)typeof(InvoiceLineCompleteCollection).GetField("hasInitialisedInvoiceLineData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(collection);
				Assert("InvoiceLineCompleteCollection Initialised InvoiceLineData", hasInitialisedInvoiceLineData);

				var hasInitialisedData = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(line);
				Assert("InvoiceLines hasInitialisedData", hasInitialisedData);

				typeof(InvoiceLineCompleteCollection).GetField("hasInitialisedInvoiceLineData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(collection, false);
				typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(line, false);
				using (testDec.SuspendInvoiceLineDataInitialization())
				{
					collection.Load();
					hasInitialisedInvoiceLineData = (bool)typeof(InvoiceLineCompleteCollection).GetField("hasInitialisedInvoiceLineData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(collection);
					Assert("InvoiceLineCompleteCollection Initialised InitialiseInvoiceLineData", !hasInitialisedInvoiceLineData);
					hasInitialisedData = (bool)typeof(BaseJobComInvoiceLine).GetField("hasInitialisedData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(line);
					Assert("JobDeclaration suspend hasInitialisedData", !hasInitialisedData);
				}
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected virtual BaseJobComInvoiceHeader GetInvoiceHeaderFromDec(BaseJobDeclaration dec) => dec.Invoices.AddNew();

		BaseJobDeclaration fDeclaration;
		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetMeANewJobDeclaration();
				}
				return fDeclaration;
			}
		}

		protected virtual BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
	}
}
