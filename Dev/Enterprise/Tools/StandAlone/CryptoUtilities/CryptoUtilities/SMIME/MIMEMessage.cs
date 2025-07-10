using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Enterprise.CryptoUtilities.SMIME
{
	public abstract class MIMEMessage
	{
		protected MIMEMessage()
		{
			SetTag("Mime-Version", "1.0");
		}

		public string RawMIMEString
		{
			get
			{
				if (fOutputString == null)
				{
					fOutputString = Header + "\r\n" + EncodedContents;
				}
				return fOutputString;
			}
		}

		public void SetTag(String tagName, String tagValue)
		{
			if (fEncodedContents != null)
			{
				throw new Exception("You can't set another header tag because you have previously retrieved the raw MIME string.");
			}
			MIMETags.Add(tagName, tagValue);
		}

		#region Implementation
		protected abstract string EncodedContents
		{
			get;
		}

		protected string Header
		{
			get
			{
				StringBuilder ResultsBuilder = new StringBuilder();
				IDictionaryEnumerator TagEnumeration = MIMETags.GetEnumerator();
				for (int i = 0; i < MIMETags.Count; i++)
				{
					TagEnumeration.MoveNext();
					ResultsBuilder.Append(TagEnumeration.Key);
					ResultsBuilder.Append(": ");
					ResultsBuilder.Append(TagEnumeration.Value);
					ResultsBuilder.Append("\r\n");
				}
				return ResultsBuilder.ToString();
			}
		}

		protected string fEncodedContents;
		protected string AttachmentName;
		protected string fOutputString;
		protected ListDictionary MIMETags = new ListDictionary();
		#endregion
	}
}
