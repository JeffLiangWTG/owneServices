using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChequeBookFilterBusinessObject))]
	sealed class AccChequeBookFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestAbsoluteFilter()
		{
			AccBankAccount bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>(TestBusinessObjectKind.MinimumRequiredToSave);
			bankAccount1.AB_GC = Env.CurrentCompany.PK;
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>(TestBusinessObjectKind.MinimumRequiredToSave);
			chequeBook1.AK_AB = bankAccount1.PK;

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			AccBankAccount bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>(TestBusinessObjectKind.MinimumRequiredToSave);
			bankAccount2.AB_GC = company.PK;
			AccChequeBook chequeBook2 = Factory.NewWithValidTestData<AccChequeBook>(TestBusinessObjectKind.MinimumRequiredToSave);
			chequeBook2.AK_AB = bankAccount2.PK;

			Factory.Save();

			AccChequeBookFilterBusinessObject absoluteFilter = new AccChequeBookFilterBusinessObject();
			AccChequeBookCollection collection = new AccChequeBookCollection(Factory);
			collection.Load(absoluteFilter.Filter);

			AssertCollectionContains(chequeBook1, collection);
			AssertCollectionNotContains(chequeBook2, collection);
		}

		public void TestAutoPrintFilter()
		{
			AccBankAccount bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>(TestBusinessObjectKind.MinimumRequiredToSave);
			bankAccount1.AB_GC = Env.CurrentCompany.PK;
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>(TestBusinessObjectKind.MinimumRequiredToSave);
			chequeBook1.AK_AB = bankAccount1.PK;
			chequeBook1.AK_AutoPrintCheque = ZBool.True;

			AccChequeBook chequeBook2 = Factory.NewWithValidTestData<AccChequeBook>(TestBusinessObjectKind.MinimumRequiredToSave);
			chequeBook2.AK_AB = bankAccount1.PK;
			chequeBook2.AK_AutoPrintCheque = ZBool.False;

			Factory.Save();

			AccChequeBookFilterBusinessObject otherFilter = new AccChequeBookFilterBusinessObject();
			AccChequeBookCollection collection = new AccChequeBookCollection(Factory);

			((ModuleTextFilter)otherFilter["Auto Print"]).Property = "AUT";
			((ModuleTextFilter)otherFilter["Auto Print"]).IsActive = true;

			collection.Load(otherFilter.Filter);

			AssertCollectionContains(chequeBook1, collection);
			AssertCollectionNotContains(chequeBook2, collection);

			((ModuleTextFilter)otherFilter["Auto Print"]).Property = "MAN";
			((ModuleTextFilter)otherFilter["Auto Print"]).IsActive = true;

			collection.Load(otherFilter.Filter);

			AssertCollectionNotContains(chequeBook1, collection);
			AssertCollectionContains(chequeBook2, collection);

			((ModuleTextFilter)otherFilter["Auto Print"]).Property = "ALL";
			((ModuleTextFilter)otherFilter["Auto Print"]).IsActive = true;

			collection.Load(otherFilter.Filter);

			AssertCollectionContains(chequeBook1, collection);
			AssertCollectionContains(chequeBook2, collection);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccChequeBookFilterBusinessObject();
		}

		#endregion
	}
}
