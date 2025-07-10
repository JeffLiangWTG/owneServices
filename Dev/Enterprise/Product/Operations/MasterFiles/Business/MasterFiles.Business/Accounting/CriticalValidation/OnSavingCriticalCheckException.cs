using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public abstract class OnSavingCriticalCheckException : ExceptionHandledWithPopup, IHasErrorReportID
	{
		protected OnSavingCriticalCheckException(ISupportCriticalValidation businessEntity, CriticalValidationErrorType errorType, string message, string developerErrorMessage)
			: base(message)
		{
			this.businessEntity = businessEntity;
			this.errorType = errorType.ToString();
			this.developerErrorMessage = developerErrorMessage;
		}

#if NETFRAMEWORK
		protected OnSavingCriticalCheckException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ISupportCriticalValidation BusinessEntity
		{
			get { return businessEntity; }
		}
		readonly ISupportCriticalValidation businessEntity;

		public string ErrorType
		{
			get { return errorType; }
		}
		readonly string errorType;

		public string DeveloperErrorMessage
		{
			get
			{
				return developerErrorMessage;
			}
		}
		readonly string developerErrorMessage;

		string IHasErrorReportID.ErrorReportID
		{
			get
			{
				return errorReportID;
			}
			set
			{
				errorReportID = value;
			}
		}
		string errorReportID;
	}

	[Serializable]
	public class OnSavingCriticalCheckException<T> : OnSavingCriticalCheckException where T : ISupportCriticalValidation
	{
		public OnSavingCriticalCheckException(T businessEntity, CriticalValidationErrorType errorType, string userFreindlyErrorMessage, string developerMessage)
			: base(businessEntity, errorType, userFreindlyErrorMessage, developerMessage)
		{
		}

#if NETFRAMEWORK
		protected OnSavingCriticalCheckException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public new T BusinessEntity
		{
			get { return (T)base.BusinessEntity; }
		}
	}

	[Serializable]
	public class CannotSaveAfterCriticalErrorException : OnSavingCriticalCheckException
	{
		public CannotSaveAfterCriticalErrorException(string message)
			: base(null, CriticalValidationErrorType.CannotSaveAfterError, message, string.Empty)
		{ }

#if NETFRAMEWORK
		protected CannotSaveAfterCriticalErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
