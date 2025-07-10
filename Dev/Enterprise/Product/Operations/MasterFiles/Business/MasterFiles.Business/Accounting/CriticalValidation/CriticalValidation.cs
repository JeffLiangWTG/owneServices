using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CriticalValidation<T> : ICriticalValidation where T : ISupportCriticalValidation, IFactoryProvider
	{
		#region Construction

		protected CriticalValidation(T parent)
		{
			this.Parent = parent;
		}

		public T Parent { get; private set; }

		#endregion

		#region Public Methods

		public void RegisterOnSavingCheck()
		{
			var criticalValidationService = Parent.Factory.ServiceContainer.GetCriticalValidationService<CriticalValidationService>();
			if (criticalValidationService == null)
			{
				criticalValidationService = new CriticalValidationService();
				Parent.Factory.ServiceContainer.AddCriticalValidationService(criticalValidationService);
				Parent.Factory.ServiceContainer.AddAfterSaveInTransactionService(criticalValidationService);
			}
		}

		protected virtual string CreateCompleteUserMessage(CriticalValidationResult result)
		{
			StringBuilder userMessageBuilder = new StringBuilder();
			if (Globals.IsUserInteractive)
			{
				userMessageBuilder.AppendLine(Res.GetString("ce6d0dbf-4fae-4dcb-afb4-ac7e600062d5", @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: {0}",
					result.UserFriendlyErrorMessage));
			}
			else
			{
				userMessageBuilder.AppendLine(Res.GetString("bef9e469-c75d-47c8-8726-b66d90744ed5", @"An error has occurred. Service task failed to process completely.
Please wait for the next cycle of service task.
Error Message: {0}",
					result.UserFriendlyErrorMessage));
			}

			if (!result.UserFriendlyErrorMessage.Equals(result.EnglishUserFriendlyErrorMessage))
			{
				userMessageBuilder.AppendLine("(" + result.EnglishUserFriendlyErrorMessage + ")");
			}

			return userMessageBuilder.ToString().Trim();
		}

		string CreateDevMessage(CriticalValidationResult result, string heading)
		{
			string devMessage = (NoResString)"Developer Details (" + heading + (NoResString)"): "
								+ System.Environment.NewLine
								+ System.Environment.NewLine
								+ result.EnglishUserFriendlyErrorMessage
								+ System.Environment.NewLine
								+ System.Environment.NewLine
								+ result.DeveloperErrorMessage;

			var constructorStackTraceInfo = Parent as IHaveConstructorStackTrace;
			if (constructorStackTraceInfo != null && constructorStackTraceInfo.ConstructorStackTrace != null)
			{
				devMessage += System.Environment.NewLine + Res.GetString("ca85bdae-136f-435b-93bc-33b5cda0d089", "The {0} was constructed at the following trace:\r\n{1}", Parent.GetType(), constructorStackTraceInfo.ConstructorStackTrace);
			}
			return devMessage;
		}

		public void RunOnSavingCheck()
		{
#if DEBUG
			if (Globals.IsTest && SuspendCriticalValidationAttribute.IsActive)
			{
				return;
			}
#endif

			RunCriticalCheck(OnSavingOnlyCriticalChecks, (NoResString)"Critical Validation Failure");
		}

		public void RunDeletedObjectOnSavingCheck()
		{
			RunCriticalCheck(DeletedObjectOnSavingCriticalChecks, (NoResString)"Deleted Object Critical Validation Failure");
		}

		public void RunAfterSavingCheck()
		{
			RunCriticalCheck(AfterSavingCriticalChecks, (NoResString)"After Saving Critical Validation Failure");
		}

		void RunCriticalCheck(Func<IEnumerable<CriticalValidationResult>> criticalChecks, string heading)
		{
			foreach (var result in criticalChecks())
			{
				if (result != null && result.IsError)
				{
					string devMessage = CreateDevMessage(result, heading);
					string userMessage = CreateCompleteUserMessage(result);
					throw new OnSavingCriticalCheckException<T>(Parent, result.ErrorType, userMessage, devMessage);
				}
			}
		}

		#endregion

		#region Check Grouping Methods

		protected virtual IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			yield return new CriticalValidationResult();
		}

		protected virtual IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
		{
			yield return new CriticalValidationResult();
		}

		protected virtual IEnumerable<CriticalValidationResult> AfterSavingCriticalChecks()
		{
			yield return new CriticalValidationResult();
		}

		#endregion

		protected static bool AreDateTimesEqualSafe(ZPropertyInfo dateInfo1, ZPropertyInfo dateInfo2)
		{
			return CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dateInfo1, dateInfo2);
		}
	}

	public class CriticalValidationResult
	{
		public CriticalValidationResult()
		{
			isError = false;
			errorType = CriticalValidationErrorType.NoError;
			developerErrorMessages = null;
		}

		public CriticalValidationResult(CriticalValidationErrorType errorType, ResourceString userFriendlyErrorMessage, params string[] developerMessage)
		{
			isError = true;
			this.errorType = errorType;
			developerErrorMessages = developerMessage != null ? developerMessage.Where(x => !string.IsNullOrEmpty(x)).ToArray() : null;
			this.userFriendlyErrorMessage = userFriendlyErrorMessage;
			this.englishUserFriendlyErrorMessage = userFriendlyErrorMessage.GetUnresolvedString();
		}

		public bool IsError
		{
			get { return isError; }
		}
		readonly bool isError;

		public CriticalValidationErrorType ErrorType
		{
			get { return errorType; }
		}
		readonly CriticalValidationErrorType errorType;

		public ResourceString UserFriendlyErrorMessage
		{
			get { return userFriendlyErrorMessage; }
		}
		readonly ResourceString userFriendlyErrorMessage;

		public string EnglishUserFriendlyErrorMessage
		{
			get { return englishUserFriendlyErrorMessage; }
		}
		readonly string englishUserFriendlyErrorMessage;

		public string DeveloperErrorMessage
		{
			get
			{
				var result = string.Empty;
				if (developerErrorMessages != null && developerErrorMessages.Any())
				{
					result = string.Join(System.Environment.NewLine, developerErrorMessages);
				}
				return result;
			}
		}
		readonly string[] developerErrorMessages;
	}
}
