using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationApportionmentEnabledTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestJobWithNoInvoicesDoesNotBlowUp()
		{
			BaseJobComInvHeaderCharge charge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = "AUD";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			BaseJobDeclaration testDecLoaded = anotherFactory.Load<BaseJobDeclaration>(testDec.PK);
			_ = testDecLoaded.JobComInvoiceGroupHeaders[0].Charges;
		}

		public void TestApportionmentDirtyChangedEventGetsCalled()
		{
			ObjectInterestedInApportionmentDirtyChangedEvent testClass = new ObjectInterestedInApportionmentDirtyChangedEvent(testDec);
			AssertEquals("TestObject not notified yet", false, testClass.JobDecApportionmentDirtyChangedCalled);
			AssertEquals("Currently Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testDec.ApportionmentDirty = true;
			AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);
			AssertEquals("TestObject gets notified", true, testClass.JobDecApportionmentDirtyChangedCalled);
		}

		public void TestApportionmentDirtyDisablesApportionment()
		{
			testDec.ApportionmentDirty = true;
			AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);
		}

		public void TestResumeApportionmentSetsDirtyToFalse()
		{
			testDec.ApportionmentDirty = true;
			AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);
			testDec.ResumeApportionment();
			AssertEquals("Apportionment is not dirty any more", false, testDec.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyDontSetDirtyWhenApportionmentDisabled()
		{
			using (testDec.SuspendMarkApportionmentDirty())
			{
				testDec.MarkApportionmentDirty();
				AssertEquals("Apportionment dirty suspended", false, testDec.ApportionmentDirty);
			}
		}

		public void TestOnApportionedCalled()
		{
			TestDeclaration testDec = Factory.New<TestDeclaration>();
			testDec.OnApportionedCalled = false;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned resumed and OnApportionedCalled", true, testDec.OnApportionedCalled);
		}

		BaseJobDeclaration testDec;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool OnApportionedCalled;
			protected override void OnApportioned()
			{
				base.OnApportioned();
				OnApportionedCalled = true;
			}
		}

		class ObjectInterestedInApportionmentDirtyChangedEvent
		{
			public ObjectInterestedInApportionmentDirtyChangedEvent(BaseJobDeclaration jobDeclaration)
			{
				jobDeclaration.OnApportionmentDirtyChanged += new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(JobDeclaration_OnApportionmentDirtyChanged);
			}

			public bool JobDecApportionmentDirtyChangedCalled;
			void JobDeclaration_OnApportionmentDirtyChanged()
			{
				JobDecApportionmentDirtyChangedCalled = true;
			}
		}
	}
}
