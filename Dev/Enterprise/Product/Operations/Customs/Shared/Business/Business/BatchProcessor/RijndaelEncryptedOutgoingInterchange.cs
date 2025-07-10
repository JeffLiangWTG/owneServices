using System;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class RijndaelEncryptedOutgoingInterchange : OutgoingInterchange
	{
		public RijndaelEncryptedOutgoingInterchange(EDIInterchange interchange) : base(interchange)
		{
		}

		protected override string ContentToSend
		{
			get
			{
				Guid initVector = NewInitVector();
				string encryptedMessage = new TwoWayEncoder(initVector).Encrypt(ContentToEncryptThenSend);
				string result = Convert.ToBase64String(initVector.ToByteArray()) + encryptedMessage;
				return result;
			}
		}

		protected virtual string ContentToEncryptThenSend
		{
			get { return base.ContentToSend; }
		}

		protected virtual Guid NewInitVector()
		{
			return Guid.NewGuid();
		}
	}
}
