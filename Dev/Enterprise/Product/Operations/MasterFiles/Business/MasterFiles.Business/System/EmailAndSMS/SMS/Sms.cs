using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class Sms : NonPersistentBusinessObject
	{
		public Sms()
		{
			PhoneNumbersList = new List<ZString>();
		}

		public Sms(ZString phoneNumber, ZString message)
			: this()
		{
			AddRecipient(phoneNumber);
			Message = message;
		}

		#region Phone Numbers

		public void AddRecipient(ZString phoneNumber)
		{
			if (!phoneNumber.IsEmpty && !PhoneNumbersList.Contains(phoneNumber))
			{
				PhoneNumbersList.Add(phoneNumber);
			}
		}

		public void RemoveRecipient(ZString phoneNumber)
		{
			PhoneNumbersList.Remove(phoneNumber);
		}

		public ZString[] PhoneNumbers
		{
			get { return PhoneNumbersList.ToArray(); }
		}

		readonly List<ZString> PhoneNumbersList;

		public ZInt PhoneNumberCount
		{
			get { return PhoneNumbersList.Count; }
		}

		public ZPropertyInfo PhoneNumberCountInfo
		{
			get { return GetZPropertyInfo(nameof(PhoneNumberCount)); }
		}

		#endregion

		#region Message

		[BusinessObjectTestExclude] // don't want the max length exception, this is handled by validation instead
		[CargoWise.ComponentModel.MaxLength(160)]
		public ZString Message
		{
			get { return fMessage; }
			set
			{
				if (fMessage != value)
				{
					fMessage = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateMessage();
					}
					MessageInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MessageInfo
		{
			get { return GetZPropertyInfo(nameof(Message)); }
		}

		ZString fMessage;

		#endregion

		#region Validation

		public SmsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual SmsValidation GetNewValidation()
		{
			return new SmsValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region FillWithValidTestData
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			ServerUsernamePasswordConfiguration smsConfig = new ServerUsernamePasswordConfiguration();
			smsConfig.UserName = "Geoff";
			smsConfig.Password = "pass";
			smsConfig.ConfirmPassword = "pass";
			PhysicalServerDataRegistry.Instance.SmsConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, smsConfig);

			AddRecipient("1234567");
			Message = "message";
		}

#endif
		#endregion
	}

	#region class SmsValidation

	public class SmsValidation : ZValidation
	{
		public SmsValidation(Sms parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidatePhoneNumberCount

		public void ValidatePhoneNumberCount()
		{
			ValidateCalculatedProperty(Parent.PhoneNumberCountInfo);
		}

		protected virtual void CheckPhoneNumberCount()
		{
			// TODO : Add Phone Number Format Validation (PhoneNumberFormatAndValidation in).
			if (Parent.PhoneNumberCount <= 0)
			{
				Parent.PhoneNumberCountInfo.AddError(Res.GetString("83d1f6a1-6963-4788-9ab4-45b6015c0d98", "SMS requires at least one phone number."));
			}
		}

		#endregion

		#region ValidateMessage

		public void ValidateMessage()
		{
			ValidateCalculatedProperty(Parent.MessageInfo);
		}

		protected virtual void CheckMessage()
		{
			// TODO : Add 7-Bit character validation
			MandatoryValidation.CheckEntered(Parent.MessageInfo);

			if (Parent.Message.Length > Parent.MessageInfo.MaxLength)
			{
				Parent.MessageInfo.AddError(Res.GetString("7893bf9b-fe21-4899-b2d1-3419a3491a64", "Your Message Text is {0} characters. The maximum allowed is {1} characters.",
					Parent.Message.Length, Parent.MessageInfo.MaxLength));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidatePhoneNumberCount();
			ValidateMessage();
		}

		public override Type AutoValidationType
		{
			get { return typeof(SmsValidation); }
		}

		protected readonly Sms Parent;
	}

	#endregion
}
