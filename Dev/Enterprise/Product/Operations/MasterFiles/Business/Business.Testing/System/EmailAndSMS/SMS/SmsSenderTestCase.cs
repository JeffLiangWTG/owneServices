using System.Reflection;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class SmsSenderTestCase<T> : TransactionedTestCase where T : SmsSender, new()
	{
		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			ResetIsSmsSupported();
		}

		protected void ResetIsSmsSupported()
		{
			FieldInfo isSmsSupportedField = typeof(SmsSender).GetField("isSmsSupported", BindingFlags.NonPublic | BindingFlags.Static);
			isSmsSupportedField.SetValue(null, null); // reset the static member
		}

		protected T Sender
		{
			get
			{
				if (fSender == null)
				{
					fSender = new T();
				}
				return fSender;
			}
		}

		protected Sms SMS
		{
			get
			{
				if (fSMS == null)
				{
					fSMS = new Sms();
				}
				return fSMS;
			}
		}

		T fSender;
		Sms fSMS;

		#endregion
	}
}
