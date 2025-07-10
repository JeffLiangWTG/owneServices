using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLinePackagePivotUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestGetPackageNumber_NoNullReferenceException()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 1";
			var invoiceLine = Factory.New<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "B1";
			var packingGroup = bill.PackingGroups[0];
			var package1 = packingGroup.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			package1.CW_MarksAndNos = "QWE";
			var package2 = packingGroup.Packages.AddNew();
			package2.CW_PackQty = 1;
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			package2.CW_MarksAndNos = "QWE";
			var package3 = packingGroup.Packages.AddNew();
			package3.CW_PackQty = 1;
			package3.CW_HouseBill = bill.CU_BillUniqueCode;
			package3.CW_MarksAndNos = "QWE";
			Factory.Save();

			var npbo = invoiceLine.PackagesForInvoiceLinesForBindingOnly[2];
			npbo.IsLinked = true;
			npbo.PackQty = 1;
			npbo.Quantity = 2;

			anotherFactory.Load<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>(declaration.PK);
			anotherFactory.Load<BaseJobComInvoiceHeader>(invoice.PK);
			var invoiceLineInAnotherFactory = anotherFactory.Load<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>(invoiceLine.PK);
			anotherFactory.Load<Bill>(bill.PK);
			anotherFactory.Load<BasePackingGroup>(packingGroup.PK);
			anotherFactory.Load<BasePackage>(package1.PK);
			anotherFactory.Load<BasePackage>(package2.PK);
			anotherFactory.Load<BasePackage>(package3.PK);
			var npboInAnotherFactory = invoiceLineInAnotherFactory.PackagesForInvoiceLinesForBindingOnly[2];
			npboInAnotherFactory.IsLinked = true;
			npboInAnotherFactory.PackQty = 3;
			npboInAnotherFactory.Quantity = 4;

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			AssertNoExceptionThrown(() =>
			{
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			});
		}

		public void TestConflictResolution()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 1";
			var invoiceLine = Factory.New<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "B1";
			var packingGroup = bill.PackingGroups[0];
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_MarksAndNos = "QWE";
			Factory.Save();

			var npbo = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			npbo.IsLinked = true;
			npbo.PackQty = 1;
			npbo.Quantity = 2;
			var pivot = npbo.Pivot;

			anotherFactory.Load<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>(declaration.PK);
			anotherFactory.Load<BaseJobComInvoiceHeader>(invoice.PK);
			var invoiceLineInAnotherFactory = anotherFactory.Load<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>(invoiceLine.PK);
			anotherFactory.Load<Bill>(bill.PK);
			anotherFactory.Load<BasePackingGroup>(packingGroup.PK);
			anotherFactory.Load<BasePackage>(package.PK);
			var npboInAnotherFactory = invoiceLineInAnotherFactory.PackagesForInvoiceLinesForBindingOnly[0];
			npboInAnotherFactory.IsLinked = true;
			npboInAnotherFactory.PackQty = 3;
			npboInAnotherFactory.Quantity = 4;
			var pivotInAnotherFactory = npboInAnotherFactory.Pivot;

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"Package (1 marked 'QWE' for HB:B1) has already been linked to invoice line (invoice INV 1 line 1) by another user ({declaration.JE_SystemLastEditUser} @ {declaration.JE_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Your changes have been merged, please review your changes and save again.
CHC_NumberOfPacks
CHC_Quantity
CHC_SystemCreateTimeUtc
CHC_SystemLastEditTimeUtc", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				AssertEquals("Should be using the existing Pivot now", ((InvoiceLinePackagePivot)pivot).PK, ((InvoiceLinePackagePivot)invoiceLineInAnotherFactory.PackagesForInvoiceLinesForBindingOnly[0].Pivot).PK);
			});
		}

		public void TestConflictResolution_WithContainer()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Another" };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 1";
			var invoiceLine = Factory.New<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CN 1";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "B1";
			var packingGroup = bill.PackingGroups[0];
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_MarksAndNos = "QWE";
			Factory.Save();

			var npbo = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			npbo.IsLinked = true;
			npbo.PackQty = 1;
			npbo.Quantity = 2;
			var pivot = npbo.Pivot;

			var declarationInAnotherFactory = anotherFactory.Load<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>(declaration.PK);
			var invoiceLineInAnotherFactory = declarationInAnotherFactory.Invoices[0].InvoiceLines[0];
			_ = declarationInAnotherFactory.Bills[0].PackingGroups[0].Packages[0];
			var npboInAnotherFactory = invoiceLineInAnotherFactory.PackagesForInvoiceLinesForBindingOnly[0];
			npboInAnotherFactory.IsLinked = true;
			npboInAnotherFactory.PackQty = 3;
			npboInAnotherFactory.Quantity = 4;
			var pivotInAnotherFactory = npboInAnotherFactory.Pivot;

			Factory.Save();

			//CachedSortedTables is not shared between two CW1 instances
			typeof(ZSaver).GetField("cachedSortedTables", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;
			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"Package (1 in CN 1 marked 'QWE' for HB:B1) has already been linked to invoice line (invoice INV 1 line 1) by another user ({declaration.JE_SystemLastEditUser} @ {declaration.JE_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Your changes have been merged, please review your changes and save again.
CHC_NumberOfPacks
CHC_Quantity
CHC_SystemCreateTimeUtc
CHC_SystemLastEditTimeUtc", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);

				AssertNoExceptionThrown("No unique index NR_UX__C2_CO_C2_JI issue", () =>
				{
					anotherFactory.Save();
				});

				AssertEquals("Should be using the existing Pivot now", ((InvoiceLinePackagePivot)pivot).PK, ((InvoiceLinePackagePivot)invoiceLineInAnotherFactory.PackagesForInvoiceLinesForBindingOnly[0].Pivot).PK);
			});
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLinePackagePivotUniqueIndexFailureHandler(null));
		}
	}
}
