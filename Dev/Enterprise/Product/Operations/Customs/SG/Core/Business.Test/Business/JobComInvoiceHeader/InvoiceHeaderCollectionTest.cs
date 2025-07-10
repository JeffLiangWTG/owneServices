using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class InvoiceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderActiveCollection>
	{
		public void TestJE_Calc_InvoicesCountInfoRefreshBindingCalledWhenItIsNeeded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			int index = 0;
			int currentIndex = index;
			declaration.JE_Calc_InvoicesCountInfo.ValueChanged += new EventHandler(delegate
			{
				index++;
			}

			);
			JobComInvoiceHeader invoice1 = (JobComInvoiceHeader)((IBindingList)declaration.Invoices).AddNew();
			AssertEquals("should have refreshed when an uncommitted row is added and relationship is set", true, index > currentIndex);
			((ICancelAddNew)declaration.Invoices).EndNew(0);
			currentIndex = index;
			JobComInvoiceHeader standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_JE = declaration.PK;
			AssertEquals("should have refreshed when a relationship is set if an invoice is attached", true, index > currentIndex);
			currentIndex = index;
			invoice1.Delete();
			AssertEquals("should have refreshed when an invoice is deleted", true, index > currentIndex);
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return Declaration.Invoices;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceHeader result = Factory.New<JobComInvoiceHeader>();
			result.JZ_JE = Declaration.PK;
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration fDeclaration;
	}
}
