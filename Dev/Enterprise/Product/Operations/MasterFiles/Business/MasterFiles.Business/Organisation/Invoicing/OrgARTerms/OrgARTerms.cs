using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgARTerms : AutoOrgARTerms
	{
		public OrgARTerms(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		#region Overrides

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (PY_InvoiceClass.IsEmpty)
			{
				PY_InvoiceClass = "ALL";
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PY_JobType = JobTypeDirectionAndTransportInfoProvider.All;
			PY_Direction = JobTypeDirectionAndTransportInfoProvider.All;
			PY_TransportMode = JobTypeDirectionAndTransportInfoProvider.All;
			PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !IsThisLastTerms; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason = base.ReasonForNotAbleToDelete;
				if (IsThisLastTerms)
				{
					reason = DefaultTermMustExistMessage;
				}
				return reason;
			}
		}

		public override void Delete()
		{
			ARTermsCycles.DeleteAll();
			ARPaymentCycles.DeleteAll();
			ARTermsInstallments.DeleteAll();

			bool isInvoiceClassAll = PY_InvoiceClass == OrgARTermsLookups.InvoiceTypes.All.Code;
			OrgCompanyData companyData = CompanyData;

			base.Delete();

			if (companyData != null && isInvoiceClassAll)
			{
				companyData.ARTerms.MarkAsNeedingValidation();
			}
		}

		public static MultilingualString DefaultTermMustExistMessage
		{
			get { return ResString.GetMultilingualString("e6f75613-ebe5-4c87-8d43-45f866d6c394", "At least one term settings row with Job Type: ALL and Invoice Type: ALL must exist."); }
		}

		#endregion

		#region Properties

		[List("Lookups.InvoiceTypeList")]
		public override ZString PY_InvoiceClass
		{
			get { return base.PY_InvoiceClass; }
			set
			{
				base.PY_InvoiceClass = value;
			}
		}

		[List("Lookups.InvoiceTermList")]
		public override ZString PY_InvoiceTerm
		{
			get { return base.PY_InvoiceTerm; }
			set
			{
				bool changed = base.PY_InvoiceTerm != value;
				base.PY_InvoiceTerm = value;
				if (changed)
				{
					ResetPaymentTermDays(PY_InvoiceTerm, PY_InvoiceDaysInfo);
					SetARTermsCycleReadonly(true);
					ARTermsCycles.MarkAsNeedingValidationIncludingChildren();
					SetARPaymentCycleReadonly(true);
					ARPaymentCycles.MarkAsNeedingValidationIncludingChildren();
					SetARTermsInstallmentReadonly(true);
					ARTermsInstallments.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		[List("Lookups.AgreedPaymentMethodList")]
		public override ZString PY_AgreedPaymentMethod
		{
			get { return base.PY_AgreedPaymentMethod; }
			set { base.PY_AgreedPaymentMethod = value; }
		}

		[List("Lookups.JobTypeList")]
		public override ZString PY_JobType
		{
			get { return base.PY_JobType; }
			set
			{
				using (RowValidationSuspender.GetSuspender())
				{
					base.PY_JobType = value;
					PY_Direction = PY_Direction_ReadOnly ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : (PY_Direction.IsEmpty ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : PY_Direction);
					PY_TransportMode = PY_TransportMode_ReadOnly ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : (PY_TransportMode.IsEmpty ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : PY_TransportMode);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePY_InvoiceTerm();
					}
				}
			}
		}

		public JobInvoicingConsumerType JobType
		{
			get
			{
				return JobTypeDirectionAndTransportListProvider.GetJobTypeDetail();
			}
		}

		[List("Lookups.DirectionList")]
		public override ZString PY_Direction
		{
			get { return base.PY_Direction; }
			set
			{
				base.PY_Direction = value;
			}
		}

		public bool PY_Direction_ReadOnly
		{
			get { return JobTypeDirectionAndTransportListProvider.IsDirectionReadonly; }
		}

		[List("Lookups.TransportModeList")]
		public override ZString PY_TransportMode
		{
			get { return base.PY_TransportMode; }
			set
			{
				base.PY_TransportMode = value;
			}
		}

		public bool PY_TransportMode_ReadOnly
		{
			get { return JobTypeDirectionAndTransportListProvider.IsTransportModeReadonly; }
		}

		#region PY_InvoiceDays

		public override ZByte PY_InvoiceDays
		{
			get { return base.PY_InvoiceDays; }
			set
			{
				base.PY_InvoiceDays = value;
				ARTermsCycles.MarkAsNeedingValidationIncludingChildren();
			}
		}

		protected bool PY_InvoiceDays_ReadOnly => GetWhetherTermWithoutDays(PY_InvoiceTerm) || InvoiceTermIsMultipleInstallments;

		#endregion

		#endregion

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgARTermsCycleCollection ARTermsCycles
		{
			get
			{
				if (arTermsCycles_cached == null)
				{
					arTermsCycles_cached = new OrgARTermsCycleCollection(this);
					SetARTermsCycleReadonly();
					RegisterEditableChildObject(arTermsCycles_cached);
				}

				return arTermsCycles_cached;
			}
		}
		OrgARTermsCycleCollection arTermsCycles_cached;

		internal ZDateTime GetARTermsCycleDueDate(ZDateTime invoiceDate, int termMonths)
		{
			ZDateTime result = ZDateTime.Empty;
			ZByte dayOfMonth = (ZByte)invoiceDate.Day;

			var validARTermsCycles = ARTermsCycles.Where(cycle =>
				{
					return OrgARTermsCycleValidationHelper.IsValidCalendarDay(cycle.P5_ToDay) && OrgARTermsCycleValidationHelper.IsValidCalendarDay(cycle.P5_PaymentDay);
				});

			OrgARTermsCycle termsCycle = (
					 from cycle in validARTermsCycles
					 where cycle.P5_FromDayCalculated <= dayOfMonth && dayOfMonth <= cycle.P5_ToDay ||
									cycle.P5_FromDayCalculated > cycle.P5_ToDay &&
											((cycle.P5_FromDayCalculated <= dayOfMonth && dayOfMonth <= 31) ||
											(1 <= dayOfMonth && dayOfMonth <= cycle.P5_ToDay))
					 select cycle)
					 .FirstOrDefault();

			if (termsCycle != null)
			{
				int numberOfMonthsToAdd = termMonths;
				if (termsCycle.P5_FromDayCalculated > termsCycle.P5_ToDay && termsCycle.P5_FromDayCalculated <= dayOfMonth && dayOfMonth <= 31)
				{
					numberOfMonthsToAdd++;
				}
				if (termsCycle.P5_PaymentDay < termsCycle.P5_ToDay)
				{
					numberOfMonthsToAdd++;
				}

				ZDateTime resultMonth = new ZDateTime(invoiceDate.Year, invoiceDate.Month, 1).AddMonths(numberOfMonthsToAdd);
				int daysInPaymentMonth = DateTime.DaysInMonth(resultMonth.Year, resultMonth.Month);
				result = new ZDateTime(resultMonth.Year, resultMonth.Month, termsCycle.P5_PaymentDay < daysInPaymentMonth ? termsCycle.P5_PaymentDay : daysInPaymentMonth);
			}

			return result;
		}

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (CompanyData != null)
			{
				shouldBeReadOnly = CompanyData.CreditDetails_ReadOnly;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgARPaymentCycleCollection ARPaymentCycles
		{
			get
			{
				if (arPaymentCycles_cached == null)
				{
					arPaymentCycles_cached = new OrgARPaymentCycleCollection(this);
					SetARPaymentCycleReadonly(deleteExisting: false);
					RegisterEditableChildObject(arPaymentCycles_cached);
				}

				return arPaymentCycles_cached;
			}
		}
		OrgARPaymentCycleCollection arPaymentCycles_cached;

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgARTermsInstallmentCollection ARTermsInstallments
		{
			get
			{
				if (arTermsInstallments_cached == null)
				{
					arTermsInstallments_cached = new OrgARTermsInstallmentCollection(this);
					arTermsInstallments_cached.Load();
					SetARTermsInstallmentReadonly(deleteExisting: false);
					RegisterEditableChildObject(arTermsInstallments_cached);
				}

				return arTermsInstallments_cached;
			}
		}
		OrgARTermsInstallmentCollection arTermsInstallments_cached;

		internal ZDateTime GetARPaymentCycleDueDate(ZDateTime invoiceDate, int termDays)
		{
			ZDateTime result = ZDateTime.Empty;
			ZDateTime preliminaryDate = invoiceDate.AddDays(termDays);

			var validARPaymentCycles = ARPaymentCycles.Where(cycle =>
				{
					return OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(cycle.P5_ToDay) && OrgARTermsCycleValidationHelper.IsValidCalendarDay(cycle.P5_PaymentDay);
				});

			var dayOfMonth = preliminaryDate.Day;
			var month = preliminaryDate.Month;
			ZDateTime endOfPrevMonth = preliminaryDate.AddDays(-dayOfMonth);
			OrgARPaymentCycle paymentCycle = null;

			while (paymentCycle == null && validARPaymentCycles.Any())
			{
				paymentCycle = validARPaymentCycles.OrderBy(cycle => cycle.P5_PaymentDay).FirstOrDefault(cycle => cycle.P5_PaymentDay >= dayOfMonth && endOfPrevMonth.AddDays(cycle.P5_PaymentDay).Month == month);

				if (paymentCycle == null)
				{
					dayOfMonth = 1;
					try
					{
						endOfPrevMonth = endOfPrevMonth.AddDays(1).AddMonths(1).AddDays(-1);
					}
					catch (ArgumentOutOfRangeException)
					{
						var messageInner = (NoResString)"Calculating Due Date using the following info:\r\nInvoice Date: {0}\r\nTerm Days: {1}\r\n\r\nPayment Cycles Info:\r\n{2}\r\n";
						var paymentCyclesInfo = string.Join("\r\n", validARPaymentCycles.Select(cycle => string.Format(CultureInfo.InvariantCulture, (NoResString)"Payment Cycle: {0}, Payment Day: {1}", (byte)cycle.P5_ToDay, (byte)cycle.P5_PaymentDay)));
						var message = string.Format(CultureInfo.InvariantCulture, messageInner, invoiceDate, termDays, paymentCyclesInfo);
						ErrorReporter.ReportOnce("OrgARTerms_ARPaymentCycleDueDateIsAnUnrepresentableDateTime", message);
						break;
					}
					month = endOfPrevMonth.AddDays(1).Month;
				}
			}

			if (paymentCycle != null)
			{
				result = endOfPrevMonth.AddDays(paymentCycle.P5_PaymentDay);
			}

			return result;
		}

		internal JobTypeDirectionAndTransportInfoProvider JobTypeDirectionAndTransportListProvider
		{
			get
			{
				return jobTypeDirectionAndTransportListProvider ?? (jobTypeDirectionAndTransportListProvider = new JobTypeDirectionAndTransportInfoProvider(() => PY_JobTypeInfo, () => PY_DirectionInfo, () => PY_TransportModeInfo));
			}
		}
		JobTypeDirectionAndTransportInfoProvider jobTypeDirectionAndTransportListProvider;

		public FunctionalitySuspender RowValidationSuspender
		{
			get
			{
				if (rowValidationSuspender == null)
				{
					rowValidationSuspender = new FunctionalitySuspender(() => Validation.CheckIntegrity());
				}
				return rowValidationSuspender;
			}
		}
		FunctionalitySuspender rowValidationSuspender;

		public bool IsDisbursementTerm
		{
			get
			{
				return AccTransactionHeader.IsDisbursementInvoiceType(PY_InvoiceClass) || PY_InvoiceClass.Equals(OrgARTermsLookups.InvoiceTypes.DSB.Code);
			}
		}

		public bool IsDefaultTerm
		{
			get
			{
				return PY_JobType.Equals(JobTypeDirectionAndTransportInfoProvider.All)
								&& PY_Direction.Equals(JobTypeDirectionAndTransportInfoProvider.All)
								&& PY_TransportMode.Equals(JobTypeDirectionAndTransportInfoProvider.All)
								&& PY_GB_Branch.Equals(ZGuid.Empty)
								&& PY_GE_Department.Equals(ZGuid.Empty)
								&& PY_InvoiceClass.Equals(OrgARTermsLookups.InvoiceTypes.All.Code);
			}
		}

		public override string ToString()
		{
			return new ToStringConverter(this).ShortFullText;
		}

		public bool IsTermWithoutDays
		{
			get
			{
				return GetWhetherTermWithoutDays(PY_InvoiceTerm);
			}
		}

		#region Implementation

		bool IsThisLastTerms
		{
			get { return CompanyData.ARTerms.Count == 1; }
		}

		void ResetPaymentTermDays(ZString terms, ZPropertyInfo termDaysInfo)
		{
			if (GetWhetherTermWithoutDays(terms))
			{
				termDaysInfo.Value = ZByte.Zero;
			}
		}

		bool GetWhetherTermWithoutDays(ZString term)
		{
			return AccountingMasterFilesUtils.IsTermWithoutDays(term);
		}

		void SetARTermsCycleReadonly(bool deleteExisting = false)
		{
			ARTermsCycles.SetReadOnlyIncludingChildren(ARTermsCycle_Readonly);

			var query = new ZQuery();
			if (!InvoiceTermIsMonthsFromInvoiceCycleDate)
			{
				if (deleteExisting)
				{
					ARTermsCycles.DeleteAll();
				}

				query.IsNoResultQuery = true;
			}

			ARTermsCycles.AdditionalFilter = query;
		}

		bool ARTermsCycle_Readonly => !Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed || !InvoiceTermIsMonthsFromInvoiceCycleDate;

		bool InvoiceTermIsMonthsFromInvoiceCycleDate => PY_InvoiceTerm == InvoiceTermsList.MonthsFromInvoiceCycleDate.Code;

		void SetARPaymentCycleReadonly(bool deleteExisting = false)
		{
			ARPaymentCycles.SetReadOnlyIncludingChildren(ARPaymentCycle_Readonly);

			var query = new ZQuery();
			if (!InvoiceTermIsTermDaysAndDebtorPaymentCycle)
			{
				if (deleteExisting)
				{
					ARPaymentCycles.DeleteAll();
				}

				query.IsNoResultQuery = true;
			}

			ARPaymentCycles.AdditionalFilter = query;
		}

		bool ARPaymentCycle_Readonly => !Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed || !InvoiceTermIsTermDaysAndDebtorPaymentCycle;

		bool InvoiceTermIsTermDaysAndDebtorPaymentCycle => PY_InvoiceTerm == InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;

		void SetARTermsInstallmentReadonly(bool deleteExisting = false)
		{
			bool arTermsInstallment_Readonly = PY_InvoiceTerm != Constants.InvoiceTerms.FromInvoiceDate;

			ARTermsInstallments.SetReadOnlyIncludingChildren(arTermsInstallment_Readonly);

			if (arTermsInstallment_Readonly && deleteExisting)
			{
				ARTermsInstallments.DeleteAll();
			}
		}

		public bool InvoiceTermIsMultipleInstallments => PY_InvoiceTerm == Constants.InvoiceTerms.FromInvoiceDate && ARTermsInstallments.Count > 0;

		#endregion

		#region ToStringConverter

		public class ToStringConverter
		{
			public ToStringConverter(OrgARTerms arTerms)
					: this(arTerms.Lookups, arTerms.PY_JobType, arTerms.PY_Direction, arTerms.PY_TransportMode,
									arTerms.Branch, arTerms.Department,
									arTerms.PY_InvoiceClass, arTerms.PY_InvoiceTerm, arTerms.PY_InvoiceDays,
									arTerms.GetWhetherTermWithoutDays(arTerms.PY_InvoiceTerm), InvoiceTerm.GetIsTermWitMonths(arTerms.PY_InvoiceTerm))
			{
			}

			public ToStringConverter(OrgARTermsLookups lookups, ZString jobType, ZString direction, ZString transportMode, GlbBranch branch, GlbDepartment department, ZString invoiceType, ZString invoiceTerms, ZInt termDays, bool isTermWithoutDays, bool isTermWithMonths)
			{
				var getDescriptionSafely = new Func<CodeDescriptionPairList, ZString, ZString>((list, code) => (list != null && !string.IsNullOrEmpty(code) && list.ContainsCode(code)) ? list[code, StringComparison.OrdinalIgnoreCase].Description : string.Empty);

				this.jobType = new CodeDescriptionPair(jobType.ToString(), getDescriptionSafely(lookups.JobTypeList, jobType));
				this.direction = new CodeDescriptionPair(direction.ToString(), getDescriptionSafely(lookups.DirectionList, direction));
				this.transportMode = new CodeDescriptionPair(transportMode.ToString(), getDescriptionSafely(lookups.TransportModeList, transportMode));
				this.branchCode = new CodeDescriptionPair(branch != null ? branch.GB_Code.ToString() : string.Empty, branch != null ? branch.GB_BranchName.ToString() : string.Empty);
				this.departmentCode = new CodeDescriptionPair(department != null ? department.GE_Code.ToString() : string.Empty, department != null ? department.GE_DescMultilingual : string.Empty);
				this.invoiceType = new CodeDescriptionPair(invoiceType.ToString(), getDescriptionSafely(lookups.InvoiceTypeList, invoiceType));
				this.invoiceTerms = new CodeDescriptionPair(invoiceTerms.ToString(), getDescriptionSafely(lookups.InvoiceTermListWithoutDefaultValue, invoiceTerms));
				this.termDays = new CodeDescriptionPair(termDays.ToString(), termDays.ToString());
				this.isTermWithoutDays = isTermWithoutDays;
				this.isTermWithMonths = isTermWithMonths;
			}

			readonly CodeDescriptionPair jobType;
			readonly CodeDescriptionPair direction;
			readonly CodeDescriptionPair transportMode;
			readonly CodeDescriptionPair branchCode;
			readonly CodeDescriptionPair departmentCode;
			readonly CodeDescriptionPair invoiceType;
			readonly CodeDescriptionPair invoiceTerms;
			readonly CodeDescriptionPair termDays;
			readonly bool isTermWithoutDays;
			readonly bool isTermWithMonths;

			public string LongFullText
			{
				get
				{
					var startWith = LongConditionText.StartsWith(OrgARTermsLookups.InvoiceTypes.DSB.Description, StringComparison.CurrentCultureIgnoreCase) ? Res.GetString("5385f598-ef8a-4d09-8628-60e067ae7bac", "For ") : Res.GetString("f59f405d-028a-43ef-ac08-65e61ba89db4", "When");
					var connector = LongConditionText.StartsWith(OrgARTermsLookups.InvoiceTypes.DSB.Description, StringComparison.CurrentCultureIgnoreCase) ? "," : Res.GetString("f35a39cd-c4fb-4b63-b9b2-64baf77bcd54", "then");
					return Res.GetString("5a466db5-8983-4bbd-89b7-23b593f97fa6", "{0}{1}", !string.IsNullOrEmpty(LongConditionText) ? Res.GetString("6429a594-2a17-4e4d-901c-4ec105934cac", "{0} {1} {2} invoice term is ", startWith, LongConditionText, connector) : string.Empty, LongTermsText);
				}
			}

			public string ShortFullText
			{
				get
				{
					return Res.GetString("5a466db5-8983-4bbd-89b7-23b593f97fa6", "{0}{1}", !string.IsNullOrEmpty(ShortConditionText) ? Res.GetString("407fe792-a5d3-416d-bab0-20580c986d4a", "({0})->", ShortConditionText) : string.Empty, ShortTermsText);
				}
			}

			public string LongTermsText
			{
				get
				{
					string result = string.Empty;
					if (!isTermWithoutDays)
					{
						result = termDays.ToString() +
								(invoiceTerms.Code != Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle ? (isTermWithMonths ? (!invoiceTerms.MultilingualDescription.GetUnresolvedString().ToUpper(CultureInfo.CurrentCulture).StartsWith("MONTHS", StringComparison.CurrentCultureIgnoreCase) ? " " + Res.GetString("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", "MONTHS") + " " : " ") : " " + Res.GetString("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", "DAYS") + " ") : " ");
					}
					result += invoiceTerms.Description.Trim().ToUpper(CultureInfo.CurrentCulture);
					return result;
				}
			}

			public string ShortTermsText
			{
				get
				{
					return Res.GetString("5a466db5-8983-4bbd-89b7-23b593f97fa6", "{0}{1}", !isTermWithoutDays ? Res.GetString("0a724bfa-ab67-41ec-8836-8edf6ce4260a", "{0}/", termDays) : string.Empty, invoiceTerms);
				}
			}

			public string LongConditionText
			{
				get { return ConditionsText(true); }
			}

			public string ShortConditionText
			{
				get { return ConditionsText(false); }
			}

			string ConditionsText(bool longVersion)
			{
				var qualifierText = string.Empty;
				var getString = new Func<CodeDescriptionPair, string, string>((x, y) => !new string[] { JobTypeDirectionAndTransportInfoProvider.All, string.Empty }.Contains(x.Code) ? (longVersion ? (y + x.Description) : x.Code) : string.Empty);
				var qualifiers = new string[]
										{
																getString(jobType, Res.GetString("a255a2e8-fa24-440f-b9e0-06174f45c05b", "Job Type: ")),
																getString(branchCode, Res.GetString("80eecd01-f071-4b26-ab2b-6fc5852b5ecd","Branch: ")),
																getString(departmentCode, Res.GetString("3aab2baa-136e-4ab2-8c60-9cc98d7a8e2e","Department: ")),
																getString(direction, Res.GetString("f4c18bff-cd1f-4a23-8ca3-c32ebb5dad93","Direction: ")),
																getString(transportMode, Res.GetString("72C28078-1072-433F-88C7-E16222130752","Transport Mode: ")),
																getString(invoiceType, (invoiceType.Code != OrgARTermsLookups.InvoiceTypes.DSB.Code ? Res.GetString("5935eb59-04bb-459e-b1d7-67a65b41b18e","Invoice Type: ") : string.Empty))
										};

				qualifierText = string.Join(longVersion ? ", " : "-", qualifiers.Where(x => !string.IsNullOrWhiteSpace(x)));
				return qualifierText;
			}
		}

		#endregion
	}
}
