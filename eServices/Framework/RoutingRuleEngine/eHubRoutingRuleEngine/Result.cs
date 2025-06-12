using System;
using System.Collections.Generic;
using System.Linq;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using System.Runtime.Serialization;

namespace eServices.eHubRoutingRuleEngine
{
	[DataContract]
	public class Result
	{
		public Result() { }

		public Result(eHubClient recipient, string errorCode, string errorDescription, string value)
		{
			Recipient = recipient;
			ErrorCode = errorCode;
			ErrorDescription = errorDescription;
			Value = value;
		}

		[IgnoreDataMember]
		public eHubClient Recipient { get; set; }
		[DataMember]
		public string ErrorCode { get; set; }
		[DataMember]
		public string ErrorDescription { get; set; }
		[DataMember]
		public string Value { get; set; }

		[DataMember]
		public string RecipientId
        {
            get { return Recipient != null ? Recipient.CC_ID : null;  }
            private set { Recipient = !string.IsNullOrWhiteSpace(value) ? new eHubClient {CC_ID = value} : null; }
        }

		[IgnoreDataMember]
		public bool Nil
		{
			get
			{
				return this.Recipient == null && string.IsNullOrWhiteSpace(this.ErrorCode) && string.IsNullOrWhiteSpace(this.ErrorDescription) && Value == null;
			}
		}

		public override string ToString()
		{
			return Nil ? "nil" : "{ " +
				String.Join(", ", new List<string> { 
					this.Recipient != null ? "Recipient: '" + this.Recipient.CC_ID + "'" : null,
					this.ErrorCode != null ? "ErrorCode: '" + this.ErrorCode + "'" : null,
					this.ErrorDescription != null ? "ErrorDescription: '" + this.ErrorDescription + "'" : null,
					this.Value != null ? "Value: '" + this.Value + "'" : null}.Where(s => s != null))
				+ " }";
		}
	}
}
