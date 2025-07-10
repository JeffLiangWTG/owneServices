using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingBranchRegister : NonPersistentBusinessObject<EInvoicingBranchRegisterValidation>, ILogger
	{
		public EInvoicingBranchRegister(GlbBranch branch)
			: base(new BusinessObjectFactory())
		{
			Branch = Argument.NotNull(branch, nameof(branch));
		}
		public readonly GlbBranch Branch;

		[ResourceStringData("EDA4F0B8-729F-46C3-9647-4737530D62D9", Caption = "One Time Password")]
		public ZString OTP
		{
			get => otp;
			set
			{
				otp = value;
				Validation.ValidateOTP();
				OTPInfo.RefreshBinding();
			}
		}
		ZString otp;
		public ZPropertyInfo OTPInfo => GetZPropertyInfo(nameof(OTP));

		#region Debtor

		[RelatedBusinessObject("Debtor")]
		[List("Debtors")]
		public ZGuid DebtorPK
		{
			get { return debtorPk; }
			set
			{
				if (DebtorPK != value)
				{
					SetNonPersistentPropertyValue(DebtorPKInfo, ref debtorPk, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDebtor();
					}
				}
			}
		}
		ZGuid debtorPk;

		public ZPropertyInfo DebtorPKInfo => GetZPropertyInfo(nameof(DebtorPK));

		public OrgHeader Debtor => Factory.Load<OrgHeader>(DebtorPK);

		DebtorCollection debtorList;
		public DebtorCollection Debtors
		{
			get
			{
				if (debtorList == null)
				{
					debtorList = new DebtorCollection(Factory);
				}

				return debtorList;
			}
		}

		#endregion

		[ResourceStringData("370CFF93-2133-4A68-839C-C7896E67EC9E", Caption = "Progress log")]
		public ZString ProgressLog
		{
			get => progressLog;
			set { SetNonPersistentPropertyValue(ProgressLogInfo, ref progressLog, value); }
		}
		ZString progressLog;
		public ZPropertyInfo ProgressLogInfo => GetZPropertyInfo(nameof(ProgressLog));

		public IRegisterBranchForGlobalEInvoicing GetCountrySpecificRegistrationRequestor() => countrySpecificRegistrationRequestor ?? EInvoicingBranchRegisterCountryFactory.RegistrationRequestor(this, this);
		IRegisterBranchForGlobalEInvoicing countrySpecificRegistrationRequestor;

		public override EInvoicingBranchRegisterValidation GetNewValidation() => new EInvoicingBranchRegisterValidation(this);

		void ILogger.Log(LogType type, string message)
		{
			ProgressLog += message;
			RaiseNewItemAddedToLogEvent();
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			ProgressLog += message;
			RaiseNewItemAddedToLogEvent();
		}

		void RaiseNewItemAddedToLogEvent()
		{
			if (NewItemAddedToLog != null)
			{
				NewItemAddedToLog.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler NewItemAddedToLog;

		#region DEBUG

		public void SubstitueCountrySpecificRegistrationRequestor_TestOnly(IRegisterBranchForGlobalEInvoicing requestor)
		{
			countrySpecificRegistrationRequestor = requestor;
		}

		#endregion
	}
}
