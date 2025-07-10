using System;
using System.Collections.Generic;
using Res = CryptoUtilities.Res;

namespace Enterprise.CryptoUtilities
{
	public class CertificateState
	{
		public CertificateState(ICryptoContainer certificate, DateTime timeToVerifyAgainst)
		{
			this.certificate = certificate;
			this.timeToVerifyAgainst = timeToVerifyAgainst;
		}

		public string[] Errors
		{
			get
			{
				if (fErrors == null)
				{
					fErrors = GetErrors().ToArray();
				}

				return fErrors;
			}
		}

		public string[] Warnings
		{
			get
			{
				if (fWarnings == null)
				{
					var warningList = new List<string>();
					var validToDate = certificate.ValidToDate;
					if (timeToVerifyAgainst <= validToDate && timeToVerifyAgainst.AddDays(7) > validToDate)
					{
						warningList.Add(Res.GetString("ff684f16-496f-4bae-90c4-49116f642ee0", "The certificate will shortly expire.  Please replace this certificate by {0}.", validToDate));
					}

					fWarnings = warningList.ToArray();
				}

				return fWarnings;
			}
		}

		#region Implementation

		protected virtual List<string> GetErrors()
		{
			var errorList = new List<string>();
			if (timeToVerifyAgainst > certificate.ValidToDate)
			{
				errorList.Add(Res.GetString("0cac288d-6d5f-4043-9638-1698f6f1ae3f", "The certificate has expired as of {0}.", certificate.ValidToDate));
			}
			if (timeToVerifyAgainst < certificate.ValidFromDate)
			{
				errorList.Add(Res.GetString("6365ee1b-e363-4477-b277-af8ed3cd2ef8", "The certificate is not yet valid.  The certificate will become valid at {0}.", certificate.ValidFromDate));
			}
			return errorList;
		}

		protected string[] fErrors;
		protected string[] fWarnings;
		protected DateTime timeToVerifyAgainst;
		protected ICryptoContainer certificate;

		#endregion
	}
}
