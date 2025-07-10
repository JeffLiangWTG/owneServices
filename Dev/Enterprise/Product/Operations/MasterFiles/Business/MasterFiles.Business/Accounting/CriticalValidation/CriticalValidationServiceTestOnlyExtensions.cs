#if DEBUG

using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

//[Reminder] This file wasn't moved to test proejct, because it's shared by many non-Accounting test classes.
namespace Enterprise.MasterFiles.Business.Testing
{
	public static class CriticalValidationServiceTestOnlyExtensions
	{
		public static IDisposable TemporaryForceCriticalValidationErrorInAnyFactory_ForTestOnly(CriticalValidationErrorType errorType)
		{
			return new DisposableAction(
				() => CriticalValidationService.SetForcedErrorType_Static_ForTestOnly(errorType),
				() => CriticalValidationService.ResetForcedErrorType_Static_ForTestOnly());
		}

		public static IDisposable TemporaryForceExceptionInAnyFactory_ForTestOnly(Exception ex)
		{
			return new DisposableAction(
				() => CriticalValidationService.SetForcedExceptionType_Static_ForTestOnly(ex),
				() => CriticalValidationService.ResetForcedExceptionType_Static_ForTestOnly());
		}

		public static void ForceCriticalValidationErrorForTestOnly(this BusinessObjectFactory factory, CriticalValidationErrorType errorType)
		{
			var service = factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>();
			if (service == null)
			{
				service = new CriticalValidationService();
				factory.ServiceContainer.AddCriticalValidationService(service);
				factory.ServiceContainer.AddAfterSaveInTransactionService(service);
			}
			service.ForceCriticalValidationErrorForTestOnly(errorType);
		}

		public static void ResetCriticalValidationLastExceptionForTestOnly(this BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>();
			service.ResetLastExceptionForTestOnly();
		}

		internal static void ForceCriticalValidationErrorForTestOnly(this ICriticalValidationService service, CriticalValidationErrorType errorType)
		{
			if (service is CriticalValidationService validationService)
			{
				validationService.ForcedErrorTypeForTestOnly = errorType;
			}
		}

		internal static void ThrowCriticalValidationErrorIfRequiredForTestOnly(this ICriticalValidationService service)
		{
			if (service is CriticalValidationService validationService && validationService.ForcedErrorTypeForTestOnly != CriticalValidationErrorType.NoError &&
	Globals.IsTest)
			{
				var errorType = validationService.ForcedErrorTypeForTestOnly;
				validationService.ForcedErrorTypeForTestOnly = CriticalValidationErrorType.NoError;
				throw new OnSavingCriticalCheckException<CriticalValidationSupporterDummy>
					(new CriticalValidationSupporterDummy(), errorType, "Forced Critical Validation Error - For Test Only.", "Forced Critical Validation Error - For Test Only.");
			}
		}

		internal static void ThrowExceptionIfRequiredForTestOnly(this ICriticalValidationService service)
		{
			if (service is CriticalValidationService validationService && validationService.ForcedExceptionTypeForTestOnly != null && Globals.IsTest)
			{
				var exception = validationService.ForcedExceptionTypeForTestOnly;
				validationService.ForcedExceptionTypeForTestOnly = null;

				throw exception;
			}
		}

		class CriticalValidationSupporterDummy : NonPersistentBusinessObject, ISupportCriticalValidation
		{
			internal CriticalValidationSupporterDummy()
				: base()
			{ }

			ICriticalValidation ISupportCriticalValidation.CriticalValidation
			{
				get { throw new NotImplementedException(); }
			}

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SuspendCriticalValidationAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		public static bool IsActive;
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SuspendGLAccountAndChargeCodeCriticalValidationAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadSafe]
		public static bool IsActive;

		public static IDisposable ActivateTemporary()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Someone has already activated it. This action will cause unexpected deactivation.");
			}

			return new DisposableAction(() => IsActive = true, () => IsActive = false);
		}
	}

	public sealed class SuspendSumOfLinesEqualZeroCriticalValidationAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		static int isActive = 0;

		public static bool IsActive
		{
			get => isActive == 1;
			set => System.Threading.Interlocked.Exchange(ref isActive, value ? 1 : 0);
		}

		public static IDisposable ActivateTemporary()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Someone has already activated it. This action will cause unexpected deactivation.");
			}

			return new DisposableAction(() => IsActive = true, () => IsActive = false);
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SuspendINTransactionHeaderHasPostedLinesCriticalValidationAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadSafe]
		public static bool IsActive;

		public static IDisposable ActivateTemporary()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Someone has already activated it. This action will cause unexpected deactivation.");
			}

			return new DisposableAction(() => IsActive = true, () => IsActive = false);
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadSafe]
		public static bool IsActive;

		public static IDisposable ActivateTemporary()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Someone has already activated it. This action will cause unexpected deactivation.");
			}

			return new DisposableAction(() => IsActive = true, () => IsActive = false);
		}
	}
}

#endif
