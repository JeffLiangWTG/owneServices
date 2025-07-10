using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class JobHeaderDataCollectionExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant format string")]
		const string DateTimeWithMilliseconds = "yyyy-MM-dd HH:mm:ss.fff";

		internal static void ReportInvalidJobCreationAsDeveloperException(this JobHeader job)
		{
			if (GetConfigurationsFormRegistry().Any(config => ShouldReport(job, config)))
			{
				ReportDeveloperException(job);
			}
		}

		public static string GetJobCreationWithEmptyBranchMessage(this JobHeader job)
		{
			var msg = string.Empty;

			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(job.Factory);

			if (job.Branch == null)
			{
				msg = FormattableString.Invariant($@"Argument 'Branch (PK: {job.JH_GB})' cannot be null.

{job.GetJobInfo()}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GBChanged)}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GBChangedFromValidToEmpty)}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GBChangedFromValidToEmptyForSavedJob)}

{job.ConstructorStackTrace}
");
			}

			return msg;
		}

		public static string GetJobCreationWithEmptyDepartmentMessage(this JobHeader job)
		{
			var msg = string.Empty;

			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(job.Factory);

			if (job.Department == null)
			{
				msg = FormattableString.Invariant($@"Argument 'Department (PK: {job.JH_GE})' cannot be null.

{job.GetJobInfo()}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GEChanged)}

{job.ConstructorStackTrace}
");
			}

			return msg;
		}

		internal static void RecordJobConstructorStackTrace(this JobHeader job) =>
			CriticalValidationInfoCollectorService
				.GetOrCreateService(job.Factory).
				AddInfoWhenAllowed(job.PK,
					CriticalValidationInfoCollectorServiceKeyType.JobConstructorStackTrace,
					() => $"Job Created Time: {ZDateTime.UtcNow.ToString(DateTimeWithMilliseconds, CultureInfo.InvariantCulture)}\r\n{System.Environment.StackTrace}", CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE, useNeverClearedInfo: true);

		internal static void RecordLastJH_GCChange(this JobHeader job) =>
			CriticalValidationInfoCollectorService
				.GetOrCreateService(job.Factory)
				.AddLastInfoWhenAllowed(job.PK,
									CriticalValidationInfoCollectorServiceKeyType.JH_GCChanged,
									() => IsInfoCollectionAllowed(job) ? FormattableString.Invariant($"Company: {job.Company?.GC_Code ?? "<NULL>"}\r\nCompany Changed Time: {ZDateTime.UtcNow.ToString(DateTimeWithMilliseconds, CultureInfo.InvariantCulture)}\r\nJH_GC change StackTrace ->\r\n{new StackTrace(true)}") : string.Empty,
									CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

		internal static void RecordLastJH_GBChange(this JobHeader job) =>
			CriticalValidationInfoCollectorService
				.GetOrCreateService(job.Factory)
				.AddLastInfoWhenAllowed(job.PK,
										CriticalValidationInfoCollectorServiceKeyType.JH_GBChanged,
										() => IsInfoCollectionAllowed(job) ? FormattableString.Invariant($"Branch: {job.Branch?.GB_Code ?? "<NULL>"}\r\nBranch Changed Time: {ZDateTime.UtcNow.ToString(DateTimeWithMilliseconds, CultureInfo.InvariantCulture)}\r\nJH_GB change StackTrace ->\r\n{new StackTrace(true)}") : string.Empty,
									CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

		internal static void RecordLastJH_GBChangeFromValidToEmpty(this JobHeader job, ZGuid previousValue)
		{
			if (job.JH_GB == ZGuid.Empty && previousValue.IsValid)
			{
				string getMessage() => FormattableString.Invariant($"Branch: {job.Branch?.GB_Code ?? "<NULL>"}\r\nPrevious Branch: (Code: {job.Factory.Load<GlbBranch>(previousValue)?.GB_Code ?? (NoResString)"<Branch No longer exists in database>"})\r\nPrevious Branch Changed Time: {ZDateTime.UtcNow.ToString(DateTimeWithMilliseconds, CultureInfo.InvariantCulture)}\r\nJH_GB change from valid to empty StackTrace ->\r\n{new StackTrace(true)}");
				if (job.IsInDatabase)
				{
					CriticalValidationInfoCollectorService
						.GetOrCreateService(job.Factory)
						.AddLastInfoWhenAllowed(job.PK,
							CriticalValidationInfoCollectorServiceKeyType.JH_GBChangedFromValidToEmptyForSavedJob,
							getMessage);
				}
				else
				{
					CriticalValidationInfoCollectorService
						.GetOrCreateService(job.Factory)
						.AddLastInfoWhenAllowed(job.PK,
							CriticalValidationInfoCollectorServiceKeyType.JH_GBChangedFromValidToEmpty,
							() => IsInfoCollectionAllowed(job) ? getMessage() : string.Empty,
							CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}
		}

		internal static void RecordLastJH_GEChange(this JobHeader job) =>
			CriticalValidationInfoCollectorService
				.GetOrCreateService(job.Factory)
				.AddLastInfoWhenAllowed(job.PK,
										CriticalValidationInfoCollectorServiceKeyType.JH_GEChanged,
										() => IsInfoCollectionAllowed(job) ? FormattableString.Invariant($"Department: {job.Department?.GE_Code ?? "<NULL>"}\r\nDepartment Changed Time: {ZDateTime.UtcNow.ToString(DateTimeWithMilliseconds, CultureInfo.InvariantCulture)}\r\nJH_GE change StackTrace ->\r\n{new StackTrace(true)}") : string.Empty,
									CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

		static bool IsInfoCollectionAllowed(JobHeader job)
		{
			var result = !job.IsInDatabase &&
							((AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value == AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.Value) ||
							!string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.Value.Trim()));
			return result;
		}

		static bool IsEqual(string text, string compareWith, bool considerEmptyAsValid = true)
		{
			return considerEmptyAsValid
					? (string.IsNullOrEmpty(compareWith) || text.Equals(compareWith, StringComparison.OrdinalIgnoreCase))
					: (!string.IsNullOrEmpty(compareWith) && text.Equals(compareWith, StringComparison.OrdinalIgnoreCase));
		}

		static void ReportDeveloperException(JobHeader job)
		{
			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(job.Factory);

			var developerExceptionMsg = FormattableString.Invariant($@"{BuildMessage(job)}

Job Information ->
{job.GetJobInfo()}

{job.ConstructorStackTrace}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GCChanged)}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GBChanged)}

{infoCollector.GetInfo(job.PK, CriticalValidationInfoCollectorServiceKeyType.JH_GEChanged)}");

			ErrorReporter.ReportOnce("InvalidJobCreation_1", developerExceptionMsg);
		}

		static string BuildMessage(JobHeader job)
		{
			var msgBuilder = new ZStringBuilder();

			//Check logged in user is creating the job
			if (job.JH_SystemCreateUser != Env.CurrentUser.Initials)
			{
				msgBuilder.Append(FormattableString.Invariant($"Logged in user is not creating the Job. Job is created by : {job.JH_SystemCreateUser}"));
			}
			else
			{
				msgBuilder.Append((NoResString)"Logged in user is creating the Job.");
			}

			//Check user is creating job in a company/branch where user does not have any login permission
			var creatingUser = job.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, job.JH_SystemCreateUser));
			var userContext = new UserContext(creatingUser.PK.ToGuid(), job.JH_GB.ToGuid(), job.JH_GE.ToGuid());
			if (!userContext.LoginSecurityCheckpoint.IsAllowed)
			{
				msgBuilder.Append(FormattableString.Invariant($"Current user does not have login permission to {job.Company.GC_Code}-{job.Branch.GB_Code}-{job.Department.GE_Code}"));
			}
			else
			{
				msgBuilder.Append(FormattableString.Invariant($"Current user has login permission to {job.Company.GC_Code}-{job.Branch.GB_Code}-{job.Department.GE_Code}"));
			}

			//Check user can create job from Billing tab
			using (Env.SetTemporaryUserContext(userContext))
			{
				var supporter = (job.Parent as IJobInvoicingPlugIn)?.InvoicingSupporter;
				if (supporter != null)
				{
					if (!supporter.JobInvoicingSecurity.IsAllowed)
					{
						msgBuilder.Append(FormattableString.Invariant($@"This billing job has {job.Branch.GB_Code} as branch and {job.Department.GE_Code} as department.
{supporter.JobInvoicingSecurity.ErrorMessageForNotAllowed}"));
					}
					else
					{
						msgBuilder.Append(FormattableString.Invariant($@"This billing job has {job.Branch.GB_Code} as branch and {job.Department.GE_Code} as department. Allowed to create job from Billing tab"));
					}
				}
			}

			var context = FormattableString.Invariant($@"User: {Env.CurrentUser.Initials} - Company: {Env.CurrentCompany.Code} - Branch: {Env.CurrentBranch.Code} - Dept: {Env.CurrentDepartment.Code} - UTC Time: {Env.Time.CurrentUtcDateTime}
Is initiated by ProcessController: {(!string.IsNullOrEmpty(Env.Instance.ServiceTaskCode)).ToYesNoString()} - Service Task Code: {Env.Instance.ServiceTaskCode}

{msgBuilder.ToStringWithNewLineBetweenAppends()}

NOTE for the Developer: Please consult with product team to make sure that 'Record invalid job creation by user(CargoWise Support Only)' has been set correctly. If not, this may indicate a false positive. 
Current value of this registry is {AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.Value}");

			return context;
		}

		static bool ShouldReport(JobHeader job, (string userCode, string companyCode, string branchCode, string departmentCode) segment)
		{
			return IsEqual(job.JH_SystemCreateUser, segment.userCode, false) &&
				   IsEqual(job.Company?.GC_Code, segment.companyCode) &&
				   IsEqual(job.Branch?.GB_Code, segment.branchCode) &&
				   IsEqual(job.Department?.GE_Code, segment.departmentCode);
		}

		static List<(string, string, string, string)> GetConfigurationsFormRegistry()
		{
			var list = new List<(string, string, string, string)>();
			var regVal = AccountingMasterFilesRegistry.Instance.RecordInvalidJobCreationByUser.Value.Trim();
			var segments = regVal.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var segment in segments)
			{
				list.Add(ParseLine(segment));
			}

			return list;
		}

		static (string UserCode, string CompanyCode, string BranchCode, string DepartmentCode) ParseLine(string lineValue)
		{
			var userCode = string.Empty;
			var companyCode = string.Empty;
			var branchCode = string.Empty;
			var departmentCode = string.Empty;

			var regVal = lineValue.Trim();
			var regCharVal = regVal.ToCharArray();

			if (regCharVal.Length > 2 && regCharVal[0].Equals('[') && regCharVal[regCharVal.Length - 1].Equals(']'))
			{
				regVal = regVal.Substring(1);
				regVal = regVal.Substring(0, regVal.Length - 1);
				if (!string.IsNullOrEmpty(regVal))
				{
					var segments = regVal.Split(new string[] { "]-[" }, StringSplitOptions.RemoveEmptyEntries);
					userCode = segments.Length > 0 ? segments[0] : string.Empty;
					companyCode = segments.Length > 1 ? segments[1] : string.Empty;
					branchCode = segments.Length > 2 ? segments[2] : string.Empty;
					departmentCode = segments.Length > 3 ? segments[3] : string.Empty;
				}
			}
			return (userCode, companyCode, branchCode, departmentCode);
		}
	}
}
