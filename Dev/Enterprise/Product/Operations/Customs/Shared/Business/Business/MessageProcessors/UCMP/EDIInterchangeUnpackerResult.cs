using System;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public class EDIInterchangeUnpackerResult : Messaging.Business.MessageProcessor.IUniversalCustomsInterchangeUnpackerResult
	{
		public EDIInterchangeUnpackerResult(ICollection<EDIMessage> ediMessages)
		{
			if (ediMessages == null)
			{
				throw new ArgumentNullException(nameof(ediMessages));
			}

			_ediMessages = ediMessages;
			_errorReason = null;
		}

		public EDIInterchangeUnpackerResult(string errorReason)
		{
			if (string.IsNullOrWhiteSpace(errorReason))
			{
				throw new ArgumentNullException(nameof(errorReason));
			}
			_errorReason = errorReason;
			_ediMessages = null;
		}

		public bool IsSuccess => _ediMessages != null && _errorReason == null;

		public ICollection<EDIMessage> EdiMessages
		{
			get
			{
				if (!IsSuccess)
				{
					throw new InvalidOperationException("EdiMessages cannot be accessed when IsSuccess is false.");
				}
				return _ediMessages ?? throw new InvalidOperationException("EdiMessages is not set.");
			}
			private set => _ediMessages = value;
		}
		ICollection<EDIMessage> _ediMessages;

		public string ErrorReason
		{
			get
			{
				if (IsSuccess)
				{
					throw new InvalidOperationException("ErrorReason cannot be accessed when IsSuccess is true.");
				}
				return _errorReason ?? throw new InvalidOperationException("ErrorReason is not set.");
			}
			private set => _errorReason = value;
		}
		string _errorReason;
	}
}
