using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StatusCalculator<TestHelperStatusNeedsRecalculationProvider>))]
	public class StatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeriveStatusCalledOnFactorySavingIfWeHaveChanges()
		{
			StatusCalculatorForTest calculator = (StatusCalculatorForTest)GetNewBusinessObject();
			AssertEquals(0, calculator.DeriveStatusCalledCount);
			BusinessObject.Messages.AddNew(typeof(TestHelperEDIMessage));
			BusinessObject.Messages[0].EM_MessageText = "MESSAGETEXT";
			Factory.Save();
			AssertEquals(1, calculator.DeriveStatusCalledCount);
			Factory.Save();
			AssertEquals(1, calculator.DeriveStatusCalledCount);
			BusinessObject.Messages.AddNew(typeof(TestHelperEDIMessage));
			BusinessObject.Messages[1].EM_MessageText = "MESSAGETEXT";
			Factory.Save();
			AssertEquals(2, calculator.DeriveStatusCalledCount);
		}

		public void TestAutoDeriveStatusOnFactorySaving()
		{
			StatusCalculatorForTest calculator = (StatusCalculatorForTest)GetNewBusinessObject();
			calculator.ExposedOverrideAutoDeriveStatusOnFactorySaving = true;
			AssertEquals(0, calculator.DeriveStatusCalledCount);
			BusinessObject.Messages.AddNew(typeof(TestHelperEDIMessage));
			BusinessObject.Messages[0].EM_MessageText = "MESSAGETEXT";

			calculator.ExposedAutoDeriveStatusOnFactorySaving = false;
			Factory.Save();
			AssertEquals(0, calculator.DeriveStatusCalledCount);

			calculator.ExposedAutoDeriveStatusOnFactorySaving = true;
			BusinessObject.Messages[0].EM_MessageText = "ANOTHER";
			Factory.Save();
			AssertEquals(1, calculator.DeriveStatusCalledCount);
		}

		public void TestDeriveNow()
		{
			StatusCalculatorForTest calculator = (StatusCalculatorForTest)GetNewBusinessObject();
			calculator.DeriveStatusNow();
			AssertEquals("DeriveStatusCalledCount", 1, calculator.DeriveStatusCalledCount);
		}

		public void TestDeriveIfRequired()
		{
			StatusCalculatorForTest calculator = (StatusCalculatorForTest)GetNewBusinessObject();
			calculator.DeriveStatusIfRequired();
			AssertEquals("DeriveStatusCalledCount", 0, calculator.DeriveStatusCalledCount);
			BusinessObject.Messages.AddNew().HasChanges = true;
			calculator.DeriveStatusIfRequired();
			AssertEquals("DeriveStatusCalledCount", 1, calculator.DeriveStatusCalledCount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StatusCalculatorForTest(BusinessObject);
		}

		TestHelperStatusNeedsRecalculationProvider businessObject;
		TestHelperStatusNeedsRecalculationProvider BusinessObject
		{
			get
			{
				if (businessObject == null)
				{
					businessObject = Factory.New<TestHelperStatusNeedsRecalculationProvider>();
				}
				return businessObject;
			}
		}

		#region TestHelpers

		class TestHelperEDIMessage : EDIMessage
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//do nothing
			}
		}

		public class TestHelperStatusNeedsRecalculationProvider : DummyEnterpriseBusinessObject, IStatusNeedsRecalculationProvider
		{
			public TestHelperStatusNeedsRecalculationProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool StatusNeedsRecalculation
			{
				get
				{
					return Messages.HasChanges;
				}
			}

			EDIMessageCollection messages;
			public EDIMessageCollection Messages
			{
				get
				{
					if (messages == null)
					{
						messages = new EDIMessageCollection(this, Factory);
						messages.Load();
					}
					return messages;
				}
			}
		}

		public class StatusCalculatorForTest : StatusCalculator<TestHelperStatusNeedsRecalculationProvider>
		{
			public StatusCalculatorForTest(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			protected override void DeriveStatus()
			{
				DeriveStatusCalledCount++;
			}

			public bool ExposedOverrideAutoDeriveStatusOnFactorySaving;
			public bool ExposedAutoDeriveStatusOnFactorySaving = true;

			protected override bool AutoDeriveStatusOnFactorySaving
			{
				get
				{
					if (ExposedOverrideAutoDeriveStatusOnFactorySaving)
					{
						return ExposedAutoDeriveStatusOnFactorySaving;
					}

					return base.AutoDeriveStatusOnFactorySaving;
				}
			}

			public int DeriveStatusCalledCount;
		}

		#endregion

		#endregion
	}
}
