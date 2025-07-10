using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class SmsSender
	{
		#region New

		public static SmsSender New()
		{
			SmsSender result = null;

			if (NewSmsSenderDelegate != null)
			{
				result = NewSmsSenderDelegate();
			}

			return result;
		}

		protected SmsSender()
		{
		}

		protected delegate SmsSender GetNewSmsSender();
		[ThreadStatic]
		protected static GetNewSmsSender NewSmsSenderDelegate;

		#endregion

		#region IsSmsSupported

		public static bool IsSmsSupported
		{
			get
			{
				if (!isSmsSupported.HasValue)
				{
					isSmsSupported = (New() != null);
				}

				return isSmsSupported.Value;
			}
		}
		[ThreadStatic]
		static bool? isSmsSupported;

		#endregion

		#region UserName / Password

		public ZString UserName
		{
			get { return SmsConfigRegistryItem.Value.UserName; }
		}

		public ZString Password
		{
			get { return SmsConfigRegistryItem.Value.Password; }
		}

		protected virtual ServerUsernamePasswordConfigurationRegistryItem SmsConfigRegistryItem
		{
			get { return PhysicalServerDataRegistry.Instance.SmsConfiguration; }
		}

		#endregion

		#region Send

		/// <summary>
		/// Only use this method if you do NOT want to use the batch processor to send. Sending is immediate.
		/// </summary>
		public SmsSendResult SendImmediately(Sms sms)
		{
			return Send(sms, true);
		}

		/// <summary>
		/// This is the normal way to send SMS messages. The message will be queued on the batch processor for sending.
		/// </summary>
		public SmsSendResult Send(Sms sms)
		{
			return Send(sms, false);
		}

		/// <summary>
		/// Use this when you want to save the factory yourself (to prevent multiple DB hits when sending multiple messages).
		/// Upon saving the factory, the message will be queued on the batch processor for sending.
		/// </summary>
		public void SendOnFactorySave(Sms sms, BusinessObjectFactory factory)
		{
			CreateSmsInFactoryForSending(sms, factory);
		}

		protected abstract SmsSendResult SendCore(Sms sms);

		#endregion

		#region Send (Implementation)

		SmsSendResult Send(Sms sms, bool sendImmediately)
		{
			SmsSendResult result;
			SmsSendResult validationResult = ValidateSmsForSend(sms);

			if (!validationResult.Success)
			{
				result = validationResult;
			}
			else if (sendImmediately)
			{
				result = SendCore(sms);
			}
			else
			{
				SaveSmsInFactoryForSending(sms, new BusinessObjectFactory());
				result = new SmsSendResult(true, Res.GetString("239312dc-0d66-4bce-b455-cda07eb60011", "SMS was sent to the batch processor."));
			}

			return result;
		}

		SmsSendResult ValidateSmsForSend(Sms sms)
		{
			SmsSendResult result = new SmsSendResult();
			ZString userNameErrorMsg = "";
			ZString passwordErrorMsg = "";

			sms.Validation.ValidateAll();
			result.Success = !sms.HasErrors;

			if (UserName.IsEmpty)
			{
				result.Success = false;
				userNameErrorMsg = "\r\n" + Res.GetString("181fd6bb-6195-418d-8940-2d7c1effab31", "Error - No User Name is set in the SMS Configuration registry.");
			}
			if (Password.IsEmpty)
			{
				result.Success = false;
				passwordErrorMsg = "\r\n" + Res.GetString("1f4469ad-7157-45f9-87e9-fa6072409941", "Error - No Password is set in the SMS Configuration registry.");
			}

			if (!result.Success)
			{
				result.Message =
					Res.GetString("45f5d880-6332-4996-8763-8747dfcc4181", "Unable to send SMS, the following errors were found:") + "\r\n\r\n" +
					sms.Notifications.GetErrors().ToUniqueMessageListString().Replace("\n", "\r\n") +
					userNameErrorMsg +
					passwordErrorMsg;
			}

			return result;
		}

		#endregion

		#region Creating / Saving StmPrintJob

		void SaveSmsInFactoryForSending(Sms sms, BusinessObjectFactory factory)
		{
			CreateSmsInFactoryForSending(sms, factory);
			factory.Save();

#if DEBUG
			LastSavedFactoryForTesting = factory;
#endif
		}

#if DEBUG
		public BusinessObjectFactory LastSavedFactoryForTesting;
#endif

		void CreateSmsInFactoryForSending(Sms sms, BusinessObjectFactory factory)
		{
			BusinessObject deliveryGroup = (BusinessObject)factory.New<Enterprise.Integration.DocumentEngine.IStmDeliveryGroup>();
			deliveryGroup[StmDeliveryGroupSchema.SB_IsProcessed] = true;

			foreach (ZString phoneNumber in sms.PhoneNumbers)
			{
				BusinessObject job = (BusinessObject)factory.New<Enterprise.Integration.DocumentEngine.IStmPrintJob>();

				job[StmPrintJobSchema.SP_JobType] = "SMS";
				job[StmPrintJobSchema.SP_FaxDestination] = phoneNumber;
				job[StmPrintJobSchema.SP_CustomProperties] = ZBlob.FromAscii(sms.Message);
				job[StmPrintJobSchema.SP_RunDateTime] = ZDateTime.UtcNow;
				job[StmPrintJobSchema.SP_SB_DeliveryGroup] = deliveryGroup.PK;
			}
		}

		#endregion
	}
}
