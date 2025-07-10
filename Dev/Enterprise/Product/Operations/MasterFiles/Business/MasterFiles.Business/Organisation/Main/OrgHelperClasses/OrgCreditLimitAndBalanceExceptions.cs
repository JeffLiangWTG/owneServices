using System;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgCreditLimitAndBalanceExceptions
	{
		[Serializable]
		public abstract class ExceptionBase : InvalidOperationException
		{
			public ExceptionBase(string message)
				: base(message)
			{
			}

			public ExceptionBase(string message, Exception innerException)
				: base(message, innerException)
			{
			}

#if NETFRAMEWORK
			protected ExceptionBase(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		[Serializable]
		public class DbException : ExceptionBase
		{
			public DbException(ExceptionReason reason)
				: base(reason.ToString())
			{
				this.reason = reason;
			}

			public DbException(ExceptionReason reason, Exception innerException)
				: base(reason.ToString(), innerException)
			{
				this.reason = reason;
			}

#if NETFRAMEWORK
			protected DbException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
				Enum.TryParse(info.GetString(ReasonFiledName), out reason);
			}
#endif

			public ExceptionReason Reason
			{
				get { return reason; }
			}
			readonly ExceptionReason reason;

			public enum ExceptionReason
			{
				EmptyData,
				InvalidData,
				NoDataInCache
			}

#if NETFRAMEWORK
			public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			{
				base.GetObjectData(info, context);

				info.AddValue(ReasonFiledName, Reason.ToString());
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Serialization constant")]
			const string ReasonFiledName = "Reason";
#endif
		}

		[Serializable]
		public class WebServiceException : ExceptionBase
		{
			public WebServiceException(ExceptionReason reason)
				: base(reason.ToString())
			{
				this.reason = reason;
			}

			public WebServiceException(ExceptionReason reason, string message)
				: base(message)
			{
				this.reason = reason;
			}

			public WebServiceException(ExceptionReason reason, Exception innerException)
				: base(reason.ToString(), innerException)
			{
				this.reason = reason;
			}

#if NETFRAMEWORK
			protected WebServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
				Enum.TryParse(info.GetString(ReasonFiledName), out reason);
			}
#endif

			public ExceptionReason Reason
			{
				get { return reason; }
			}
			readonly ExceptionReason reason;

			public enum ExceptionReason
			{
				Configuration,
				Initialisation,
				ResponseFail,
				Exception,
				EmptyDataInResponse,
				InvalidDataInResponse,
				NoDataInCache,
				NullResponse
			}

#if NETFRAMEWORK
			public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			{
				base.GetObjectData(info, context);

				info.AddValue(ReasonFiledName, Reason.ToString());
			}

			const string ReasonFiledName = nameof(Reason);
#endif
		}
	}
}
