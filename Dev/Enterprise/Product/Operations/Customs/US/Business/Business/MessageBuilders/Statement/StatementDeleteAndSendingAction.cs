using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class StatementDeleteAndSendingAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string US_SendMessage = "US_SendMessage";
			public const string US_PaymentType = "US_PaymentType";
			public const string US_EntryNumber = "US_EntryNumber";
			public const string US_EntryProcessPort = "US_EntryProcessPort";
			public const string US_PreliminaryStatementPrintDate = "US_PreliminaryStatementPrintDate";
			public const string US_EntryFilerCode = "US_EntryFilerCode";
			public const string US_ClientBranchDesignation = "US_ClientBranchDesignation";
			public const string US_PeriodicStatementMonth = "US_PeriodicStatementMonth";
			public const string PortOfEntry = "PortOfEntry";
		}

		public StatementDeleteAndSendingAction(IStatementDeleteTransaction entity)
			: base(entity.Factory)
		{
			this.isRecon = entity is ReconDeclaration;
			this.entity = entity;
			this.fUS_PaymentType = entity.PaymentType;
			this.fUS_PreliminaryStatementPrintDate = ZDateTime.Empty;
			this.fUS_ClientBranchDesignation = entity.ClientBranchDesignation;

			if (PaymentTypeList.IsPeriodicPayment(US_PaymentType))
			{
				this.fUS_PeriodicStatementMonth = entity.PeriodicStatementMonth;
			}
			ValidatePortOfEntry();
		}

		public readonly IStatementDeleteTransaction entity;
		readonly bool isRecon;

		#region Bindable Properties

		public ZBool US_SendMessage
		{
			get { return fUS_SendMessage; }
			set
			{
				SetNonPersistentPropertyValue(US_SendMessageInfo, ref fUS_SendMessage, value);

				if (!IsValidationSuspended)
				{
					ValidateUS_SendMessage();
				}
			}
		}
		ZBool fUS_SendMessage;

		public ZPropertyInfo US_SendMessageInfo
		{
			get { return GetZPropertyInfo(Schema.US_SendMessage); }
		}

		public void ValidateUS_SendMessage()
		{
			US_SendMessageInfo.ClearAllNotifications();

			if (entity.IsStatementUpdateMessagePending)
			{
				US_SendMessageInfo.AddMessageError(ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
			}

			ValidateUS_PaymentType();
		}

		public bool IsSinglePayment
		{
			get { return US_PaymentType == Business.PaymentTypeList.Codes.IndividualBasis; }
		}

		[MaxLength(1 /*AddInfo.Schema.US_PaymentTypeMaxLength*/ )]
		public ZString US_PaymentType
		{
			get { return fUS_PaymentType; }
			set
			{
				SetNonPersistentPropertyValue(US_PaymentTypeInfo, ref fUS_PaymentType, value);

				if (IsSinglePayment)
				{
					US_PreliminaryStatementPrintDate = ZDateTime.Empty;
				}

				if (!Enterprise.Customs.US.Business.PaymentTypeList.IsPeriodicPayment(value))
				{
					US_PeriodicStatementMonth = "";
				}

				if (!IsValidationSuspended)
				{
					ValidateUS_PaymentType();
				}
			}
		}
		ZString fUS_PaymentType;

		public ZPropertyInfo US_PaymentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.US_PaymentType); }
		}

		public void ValidateUS_PaymentType()
		{
			US_PaymentTypeInfo.ClearAllNotifications();

			if (US_SendMessage)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(US_PaymentTypeInfo, PaymentTypeList);
				ValidateUS_PreliminaryStatementPrintDate();
				ValidateUS_PeriodicStatementMonth();

				if (IsSinglePayment && entity.IsACE && entity.Branch != null)
				{
					var loader = new CusStatementHeader.Loader(Factory);
					if (loader.Load(US_EntryFilerCode, US_EntryNumber, entity.Branch.GB_GC) == null //no active statement
						&& !loader.HasDeletedStatement(US_EntryFilerCode, US_EntryNumber, entity.Branch.GB_GC))//no deleted statement
					{
						US_PaymentTypeInfo.AddMessageError(ACEImportAddInfoJobDeclarationValidation.IndividualPaymentTypeNotAllowedUntilStatementIsIssued);
					}
				}
			}
		}

		public PaymentTypeList PaymentTypeList
		{
			get
			{
				if (isRecon)
				{
					return PaymentTypeList.GetCachedReconPaymentTypeList(Factory);
				}
				else
				{
					return Factory.GetCachedValue<PaymentTypeList>();
				}
			}
		}

		public ZDateTime US_PreliminaryStatementPrintDate
		{
			get { return fUS_PreliminaryStatementPrintDate; }
			set
			{
				SetNonPersistentPropertyValue(US_PreliminaryStatementPrintDateInfo, ref fUS_PreliminaryStatementPrintDate, value);

				if (!IsValidationSuspended)
				{
					ValidateUS_PreliminaryStatementPrintDate();
				}
			}
		}
		ZDateTime fUS_PreliminaryStatementPrintDate;

		public ZPropertyInfo US_PreliminaryStatementPrintDateInfo
		{
			get { return GetZPropertyInfo(Schema.US_PreliminaryStatementPrintDate); }
		}

		public void ValidateUS_PreliminaryStatementPrintDate()
		{
			US_PreliminaryStatementPrintDateInfo.ClearAllNotifications();

			if (US_SendMessage)
			{
				if (IsSinglePayment)
				{
					if (!US_PreliminaryStatementPrintDate.IsEmpty)
					{
						US_PreliminaryStatementPrintDateInfo.AddMessageError(ValidationConstants.Statement.PSDNotAllowedForPayType1);
					}
				}
				else
				{
					if (US_PreliminaryStatementPrintDate.IsEmpty)
					{
						US_PreliminaryStatementPrintDateInfo.AddMessageError(ValidationConstants.Statement.PSDRequiredForNonPayType1);
					}
					else if (US_PreliminaryStatementPrintDate.IsValid)
					{
						if (US_PreliminaryStatementPrintDate.Date <= ZDateTime.Today)
						{
							US_PreliminaryStatementPrintDateInfo.AddMessageError(ValidationConstants.Statement.PSDMustBeFutureDate);
						}

						var validator = new PrelimStatementPrintDateValidator();
						validator.ValidateWorkingDayWithDay11(US_PreliminaryStatementPrintDateInfo, US_PaymentType, US_PeriodicStatementMonth);
					}
					else
					{
						US_PreliminaryStatementPrintDateInfo.AddMessageError(ValidationConstants.Statement.PSDIsInvalid);
					}
				}

				new WeekendsAndHolidaysValidator().CheckWeekendsAndHolidays(US_PreliminaryStatementPrintDateInfo);
			}
		}

		public ZString US_EntryNumber
		{
			get { return entity.EntryNumber; }
		}

		public ZPropertyInfo US_EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_EntryNumber); }
		}

		public ZString PortOfEntry
		{
			get { return entity.PortOfEntry; }
		}

		public ZPropertyInfo PortOfEntryInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfEntry); }
		}

		void ValidatePortOfEntry()
		{
			MandatoryValidation.MessageErrorIfNotEntered(PortOfEntryInfo, "Port of Entry");
		}

		public ZString US_EntryFilerCode
		{
			get { return entity.EntryFilerCode; }
		}

		public ZPropertyInfo US_EntryFilerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_EntryFilerCode); }
		}

		[MaxLength(AddInfo.Schema.US_ClientBranchDesignationMaxLength)]
		public ZString US_ClientBranchDesignation
		{
			get { return fUS_ClientBranchDesignation; }
			set { SetNonPersistentPropertyValue(US_ClientBranchDesignationInfo, ref fUS_ClientBranchDesignation, value); }
		}
		ZString fUS_ClientBranchDesignation;

		public ZPropertyInfo US_ClientBranchDesignationInfo
		{
			get { return GetZPropertyInfo(Schema.US_ClientBranchDesignation); }
		}

		[MaxLength(AddInfo.Schema.US_PeriodicStatementMMMaxLength)]
		public ZString US_PeriodicStatementMonth
		{
			get { return fUS_PeriodicStatementMonth; }
			set
			{
				SetNonPersistentPropertyValue(US_PeriodicStatementMonthInfo, ref fUS_PeriodicStatementMonth, value);

				if (!IsValidationSuspended)
				{
					ValidateUS_PeriodicStatementMonth();
				}
			}
		}
		ZString fUS_PeriodicStatementMonth;

		public ZPropertyInfo US_PeriodicStatementMonthInfo
		{
			get { return GetZPropertyInfo(Schema.US_PeriodicStatementMonth); }
		}

		public CodeDescriptionPairList MonthList
		{
			get { return new MonthList(); }
		}

		public void ValidateUS_PeriodicStatementMonth()
		{
			US_PeriodicStatementMonthInfo.ClearAllNotifications();

			var validator = new PeriodicStatementMMValidator();

			validator.Validate(US_PeriodicStatementMonthInfo, US_PaymentType, MonthList);
			validator.ValidateAgainstCurrentDate(US_PeriodicStatementMonthInfo);

			if (!isRecon)
			{
				validator.ValidateAgainstReleaseDate(US_PeriodicStatementMonthInfo, entity.ReleaseDate);
			}
		}

		public void LinkMessages(MQEDIMessage message)
		{
			entity.AddMessages(message);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_SendMessage();
			ValidateUS_PaymentType();
			ValidateUS_PeriodicStatementMonth();
			ValidateUS_PreliminaryStatementPrintDate();
		}

		#endregion
	}
}
