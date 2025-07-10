using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System.ComponentModel;
	using NUnit.Framework;

	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestResetHadErrorInLastResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(declaration);
			JobComInvoiceLine invoiceLine1 = collection.AddNew();
			invoiceLine1.JI_HadErrorInLastResponse = true;
			JobComInvoiceLine invoiceLine2 = collection.AddNew();
			invoiceLine2.JI_HadErrorInLastResponse = false;
			JobComInvoiceLine invoiceLine3 = collection.AddNew();
			invoiceLine3.JI_HadErrorInLastResponse = true;

			collection.ResetHadErrorInLastResponse();
			AssertEquals("invoiceLine1.JI_HadErrorInLastResponse", false, invoiceLine1.JI_HadErrorInLastResponse);
			AssertEquals("invoiceLine2.JI_HadErrorInLastResponse", false, invoiceLine2.JI_HadErrorInLastResponse);
			AssertEquals("invoiceLine3.JI_HadErrorInLastResponse", false, invoiceLine3.JI_HadErrorInLastResponse);
		}

		#region TestRefreshBindingGetsCalledWhenCollectionCountChangesOnECIWriteOff
		public void TestRefreshBindingGetsCalledWhenCollectionCountChangesOnECIWriteOff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			IBindingList declarationAsIBindingList = declaration;
			declarationAsIBindingList.ListChanged += new ListChangedEventHandler(DeclarationAsIBindingList_ListChanged);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declarationRefreshBindingCalled = false;
			declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Declaration.RefreshBinding has been called", false, declarationRefreshBindingCalled);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declarationRefreshBindingCalled = false;
			declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Declaration.RefreshBinding has been called", true, declarationRefreshBindingCalled);
		}
		bool declarationRefreshBindingCalled;
		void DeclarationAsIBindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			declarationRefreshBindingCalled = true;
		}
		#endregion

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new InvoiceLineCompleteCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobComInvoiceLine>();
		}
		#endregion
	}
}
