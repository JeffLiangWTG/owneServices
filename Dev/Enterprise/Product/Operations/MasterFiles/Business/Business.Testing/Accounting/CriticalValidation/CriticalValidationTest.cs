using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CriticalValidationTest<T> : TestCaseWithFactory where T : ISupportCriticalValidation, IFactoryProvider
	{
		public delegate T GetBusinessEntity(BusinessObjectFactory factory);

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public virtual void TestEachCase_OnSavingValidation()
		{
			CombineAssertions(() =>
			{
				foreach (TestCaseDefinitionWithDelegate_Obsolete testCase in GetTestCases())
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					T parent = testCase.GetTestDataDelegate(factory);

					if (CheckSuppliedFactoryUsed && parent is IBusiness)
					{
						AssertEquals(string.Format("[{0}] Should use the supplied factory", testCase.Description), factory, ((IBusiness)parent).Factory);
					}

					AssertOnSavingCheck(parent, testCase);
				}
			});
		}

		#region RegisterOnSavingCheck

		public void TestRegisterOnSavingCheck()
		{
			var testObject = new DummyCriticalValidationParent();
			AssertNull("Precondition: CriticalValidationOnSavingService should not be registered in validation parent factory.", testObject.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>());
			AssertNull("Precondition: CriticalValidationAfterSavingService should not be registered in validation parent factory.", testObject.Factory.ServiceContainer.GetAfterSaveInTransactionService<CriticalValidationService>());

			testObject.CriticalValidation.RegisterOnSavingCheck();
			var expectedOnSavingCriticalValidationService = testObject.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>();
			var expectedAfterSavingCriticalValidationService = testObject.Factory.ServiceContainer.GetAfterSaveInTransactionService<CriticalValidationService>();
			AssertNotNull("CriticalValidationOnSavingService should be registered in validation parent factory.", expectedOnSavingCriticalValidationService);
			AssertNotNull("CriticalValidationAfterSavingService should be registered in validation parent factory.", expectedAfterSavingCriticalValidationService);

			testObject.CriticalValidation.RegisterOnSavingCheck();
			AssertEquals("The same CriticalValidationOnSavingService should be in a factory after the second registration call.", expectedOnSavingCriticalValidationService, testObject.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>());
			AssertEquals("The same CriticalValidationAfterSavingService should be in a factory after the second registration call.", expectedAfterSavingCriticalValidationService, testObject.Factory.ServiceContainer.GetAfterSaveInTransactionService<CriticalValidationService>());
		}

		public void TestRegisterCrititalValidatonOnParentSaving()
		{
			var validationParent = GetCriticalValidationParent();
			var validation = (validationParent != null ? validationParent.CriticalValidation : null) as CriticalValidation<T>;
			if (validation != null)
			{
				AssertNull("Precondition: CriticalValidationService should not be registered in validation parent factory.", validation.Parent.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>());
				AssertNull("Precondition: CriticalValidationAfterSavingService should not be registered in validation parent factory.", validation.Parent.Factory.ServiceContainer.GetAfterSaveInTransactionService<CriticalValidationService>());

				(validation.Parent as BusinessObject).OnSaving();
				AssertNotNull("CriticalValidationService should be registered in validation parent factory.", validation.Parent.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>());
				AssertNotNull("CriticalValidationAfterSavingService should be registered in validation parent factory.", validation.Parent.Factory.ServiceContainer.GetAfterSaveInTransactionService<CriticalValidationService>());
			}
			else
			{
				Assert("The test is not suitable in this case", true);
			}
		}

		class DummyCriticalValidationParent : ISupportCriticalValidation, IFactoryProvider
		{
			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}

			#endregion

			#region IFactoryProvider Members

			public BusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factory;

			#endregion
		}

		class DummyCriticalValidation : CriticalValidation<DummyCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyCriticalValidationParent parent)
				: base(parent)
			{
			}
		}

		#endregion

		#region Implementation

		protected virtual ISupportCriticalValidation GetCriticalValidationParent()
		{
			if (typeof(T).IsSubclassOf(typeof(BusinessObject)))
			{
				return (ISupportCriticalValidation)Factory.New(typeof(T));
			}
			else
			{
				throw new NotImplementedException("You need to override this method for a class not subclassed");
			}
		}

		protected abstract List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases();

		protected virtual List<TestCaseDefinitionWithDelegate_Obsolete> GetDeletedObjectTestCases()
		{
			return new List<TestCaseDefinitionWithDelegate_Obsolete>();
		}

		protected virtual bool CheckSuppliedFactoryUsed { get { return false; } }

		#region AssertOnSavingCheck

		protected void AssertOnSavingCheck(T parent, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			AssertOnSavingCheck(parent, testCase, false);
		}

		protected void AssertDeletedObjectOnSavingCheck(T parent, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			AssertOnSavingCheck(parent, testCase, true);
		}

		protected void AssertOnSavingCheckForAnotherCriticalValidationType<BizoType>(BizoType parent, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			AssertOnSavingCheck((ISupportCriticalValidation)parent, testCase, false, parent.GetType());
		}

		protected void AssertDeletedObjectOnSavingCheckForAnotherCriticalValidationType<BizoType>(BizoType parent, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			AssertOnSavingCheck((ISupportCriticalValidation)parent, testCase, true, parent.GetType());
		}

		static void AssertOnSavingCheck<BizoType>(BizoType parent, TestCaseDefinition_ForSeparateTestsMethods testCase, bool isObjectDeleted, Type parentTypeForAnotherCriticalValidationType = null) where BizoType : ISupportCriticalValidation
		{
			try
			{
				if (isObjectDeleted)
				{
					var bizO = parent as BusinessObject;
					if (!bizO.IsDeleted)
					{
						bizO.Delete();
					}
					parent.CriticalValidation.RunDeletedObjectOnSavingCheck();
				}
				else
				{
					parent.CriticalValidation.RunOnSavingCheck();
				}

				if (testCase.CriticalCheckShouldFail)
				{
					Fail(string.Format("[{0}] Should found the {1} error with message '{2}', but no errors found", testCase.Description, testCase.ErrorType, testCase.GetExpectedErrorMessagePartsAsString()));
				}
				else
				{
					Assert("Everything is fine, no errors and it should not be an empty unit test", true);
				}
			}
			catch (OnSavingCriticalCheckException<BizoType> e) when (parentTypeForAnotherCriticalValidationType == null)
			{
				AssertException(e, testCase);
			}
			catch (OnSavingCriticalCheckException e) when (parentTypeForAnotherCriticalValidationType != null)
			{
				AssertException(e, testCase);
			}
		}

		#endregion

		#region AssertAfterSavingCheck

		protected void AssertAfterSavingCheck(T parent, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			try
			{
				parent.CriticalValidation.RunAfterSavingCheck();
				if (testCase.CriticalCheckShouldFail)
				{
					Fail(string.Format("[{0}] Should found the {1} error with message '{2}', but no errors found", testCase.Description, testCase.ErrorType, testCase.GetExpectedErrorMessagePartsAsString()));
				}
				else
				{
					Assert("Everything is fine, no errors and it should not be an empty unit test", true);
				}
			}
			catch (OnSavingCriticalCheckException<T> e)
			{
				AssertException(e, testCase);
			}
		}

		static void AssertException(OnSavingCriticalCheckException exception, TestCaseDefinition_ForSeparateTestsMethods testCase)
		{
			AssertEquals("No Developer Exception reported because it will be reported on Factory.Saved", 0, ExceptionReporterTestListener.Instance.Count);

			if (testCase.CriticalCheckShouldFail)
			{
				CriticalValidationResultTestHelper.AssertCriticalValidationResult(testCase, exception);
			}
			else
			{
				Fail(string.Format("[{0}] Expected no errors but the {1} error with '{2}' message was found", testCase.Description, exception.ErrorType, exception.Message));
			}
		}

		#endregion

		public class TestCaseDefinitionWithDelegate_Obsolete : TestCaseDefinition_ForSeparateTestsMethods
		{
			public TestCaseDefinitionWithDelegate_Obsolete(string description, GetBusinessEntity getObjectDelegate)
				: this(description, getObjectDelegate, false, CriticalValidationErrorType.NoError, string.Empty, Array.Empty<string>())
			{
			}

			public TestCaseDefinitionWithDelegate_Obsolete(string description, GetBusinessEntity getObjectDelegate, bool onSavingCheckShouldFail, CriticalValidationErrorType errorType, string userErrorMessage, params string[] expectedTechDetailsInThisOrderIntoErrorMessage)
				: base(description, onSavingCheckShouldFail, errorType, userErrorMessage, expectedTechDetailsInThisOrderIntoErrorMessage)
			{
				this.GetTestDataDelegate = getObjectDelegate;
			}

			public GetBusinessEntity GetTestDataDelegate { get; private set; }
		}

		#endregion
	}
}
