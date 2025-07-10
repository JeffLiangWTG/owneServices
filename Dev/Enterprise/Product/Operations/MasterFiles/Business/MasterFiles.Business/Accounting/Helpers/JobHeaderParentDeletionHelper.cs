using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class JobHeaderParentDeletionHelper
	{
		public static string CheckIfCanDeleteJobHeaderParent(ZGuid parentPK, ZString businessObjectReadableName)
		{
			return GetTableNamesWhichBlockJobHeaderParentDeletionErrorMessage(parentPK, businessObjectReadableName, true);
		}

		public static string CheckIfCanCancelJobHeaderParent(ZGuid parentPK, ZString businessObjectReadableName)
		{
			return GetTableNamesWhichBlockJobHeaderParentDeletionErrorMessage(parentPK, businessObjectReadableName, false);
		}

		public static string GetJobNoDeletionErrorMessage(string tablename, string businessObjectReadableName)
		{
			return string.Format(CultureInfo.InvariantCulture, @"{0}
{1}", GetJobNoDeletionErrorMessageHeading(businessObjectReadableName, true), GetJobNoDeletionErrorMessageReason(new List<string> { tablename }, string.Empty, string.Empty));
		}

		public static string GetJobNoDeactiveErrorMessage(string tablename, string businessObjectReadableName)
		{
			return string.Format(CultureInfo.InvariantCulture, @"{0}
{1}", GetJobNoDeletionErrorMessageHeading(businessObjectReadableName, false), GetJobNoDeletionErrorMessageReason(new List<string> { tablename }, string.Empty, string.Empty));
		}

		public static string GetJobNoDeactiveErrorMessage(CannotDeactiveJobReason reason, string businessObjectReadableName, Guid jobCompanyPK)
		{
			return string.Format(CultureInfo.InvariantCulture, @"{0}
{1}", GetJobNoDeletionErrorMessageHeading(businessObjectReadableName, false), GetJobCannotDeactiveErrorMessage(reason, string.Empty, jobCompanyPK, null));
		}

		static string GetTableNamesWhichBlockJobHeaderParentDeletionErrorMessage(ZGuid parentPK, ZString businessObjectReadableName, bool isDeletion)
		{
			var result = new StringBuilder();
			var blockingDetailCollection = GetBlockingDetailResult(parentPK);

			if (blockingDetailCollection.Any())
			{
				result.Append(GetJobNoDeletionErrorMessageHeading(businessObjectReadableName, isDeletion));

				var jobDeactivationRegistryItem = (CodePairRegistryItem)ObjectFactory.Get<IAccounting>().Registry.JobDeactivationConfiguration;
				var isForNPL = jobDeactivationRegistryItem.Value == AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code;

				if (isForNPL)
				{
					blockingDetailCollection.ForEach(blockingDetail => blockingDetail.cannotDeactiveJobReason = MapBlockingDetailToDeactiveReason(blockingDetail));

					var blockingDetailsGroupByJobNumAndCompanyCode = blockingDetailCollection
						.GroupBy(detail => new { detail.JobNum, detail.CompanyCode })
						.Select(group => group
							.OrderBy(detail => GetDeactiveJobReasonPriority(detail.cannotDeactiveJobReason))
							.First())
						.ToList();

					foreach (var blockingDetails in blockingDetailsGroupByJobNumAndCompanyCode)
					{
						result.AppendLine();
						result.Append(GetJobCannotDeactiveErrorMessage(blockingDetails.cannotDeactiveJobReason, blockingDetails.JobNum, blockingDetails.CompanyPK.ToGuid(), blockingDetails.CompanyCode));
					}
				}
				else
				{
					var blockingDetailsGroupByJobNumAndCompanyCode = blockingDetailCollection
						.GroupBy(detail => new { detail.JobNum, detail.CompanyCode })
						.ToList();

					foreach (var blockingDetails in blockingDetailsGroupByJobNumAndCompanyCode)
					{
						result.AppendLine();
						result.Append(GetJobNoDeletionErrorMessageReason(blockingDetails.Select(detail => detail.BlockingTableName).ToList(), blockingDetails.Key.JobNum, blockingDetails.Key.CompanyCode));
					}
				}
			}

			return result.ToString();
		}

		static int GetDeactiveJobReasonPriority(CannotDeactiveJobReason reason)
		{
			switch (reason)
			{
				case CannotDeactiveJobReason.HasPostedTransactions:
					return 1;
				case CannotDeactiveJobReason.HasSavedNonZeroJobCharges:
					return 2;
				case CannotDeactiveJobReason.HasNotReversedWIPACR:
					return 3;
				case CannotDeactiveJobReason.HasSavedHotCheques:
					return 4;
				case CannotDeactiveJobReason.HasSavedJobHeaders:
					return 5;
				default:
					return 6;
			}
		}

		static CannotDeactiveJobReason MapBlockingDetailToDeactiveReason(BlockingDetailResult blockingDetail)
		{
			if (Enum.TryParse<CannotDeactiveJobReason>(blockingDetail.BlockingReason, out var result))
			{
				return result;
			}
			return CannotDeactiveJobReason.EmptyReason;
		}

		static string GetJobNoDeletionErrorMessageHeading(string header, bool isDeletion)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}",
					header,
					isDeletion ? Res.GetString("f343252f-3a62-49c2-949a-752ac1c2c70d", "cannot be deleted.") : Res.GetString("d889d1d1-7b4c-4fc4-af01-bc8b8c2835e6", "cannot be deactivated."));
		}

		static string GetJobNoDeletionErrorMessageReason(List<string> tablenames, ZString jobNumber, ZString companyCode)
		{
			var result = new StringBuilder();
			var reasonHeading = string.Empty;

			if (tablenames.Contains(JobChargeSchema.Constants.TableName))
			{
				reasonHeading = Res.GetString("6dae5687-df75-46ba-9a81-ffeb365387b6", "Job Invoicing Charge(s)");
			}
			else if (tablenames.Contains(AccTransactionLinesSchema.Constants.TableName))
			{
				reasonHeading = Res.GetString("f2a29e39-e291-4ba8-a924-e67b3a2f3cf9", "Accounting Transaction Line(s)");
			}
			else if (tablenames.Contains(AccTransactionHeaderSchema.Constants.TableName))
			{
				reasonHeading = Res.GetString("1cdb728d-65fd-4dac-a458-d9720defda5d", "Accounting Transaction(s)");
			}
			else if (tablenames.Contains(AccHotChequeSchema.Constants.TableName))
			{
				reasonHeading = Res.GetString("b4fe12bb-260a-40b6-a8fb-eccc6bad0d75", "Hot Cheque(s)");
			}
			else if (tablenames.Contains(JobHeaderSchema.Constants.TableName))
			{
				reasonHeading = Res.GetString("01a889ef-4253-4e52-97db-f4d331ba1722", "Child Invoicing Job(s)");
			}

			result.Append(Res.GetString("6f8b2a8f-6b41-4033-b41c-3d8933c7a0e0", "{0} have been saved against this Invoicing Job Header", reasonHeading));

			AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
			return result.ToString();
		}

		static string GetJobCannotDeactiveErrorMessage(CannotDeactiveJobReason reason, ZString jobNumber, Guid jobCompanyPK, ZString companyCode)
		{
			var result = new StringBuilder();
			switch (reason)
			{
				case CannotDeactiveJobReason.HasPostedTransactions:
					result.Append(Res.GetString("a90d6908-263f-47fd-8e1e-f907aeaefe66", "Invoice, Credit Note and/or Job Revenue Journal, CFX Journal have been posted against this Invoicing Job"));
					AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
					break;
				case CannotDeactiveJobReason.HasSavedNonZeroJobCharges:
				case CannotDeactiveJobReason.HasNotReversedWIPACR:
					result.Append(GetReasonForHasChargesOrNotReversedWipAcr(reason, jobNumber, jobCompanyPK, companyCode));
					break;
				case CannotDeactiveJobReason.HasSavedHotCheques:
					result.Append(Res.GetString("3e153afb-40d4-406e-9fca-3740ee153618", "Hot Cheque(s) have been saved against this Invoicing Job Header"));
					AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
					break;
				case CannotDeactiveJobReason.HasSavedJobHeaders:
					result.Append(Res.GetString("01c6a93a-15c8-4b38-b175-10fd0cd29b95", "Child Invoicing Job(s) have been saved against this Invoicing Job Header"));
					AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
					break;
			}

			return result.ToString();
		}

		static string GetReasonForHasChargesOrNotReversedWipAcr(CannotDeactiveJobReason reason, ZString jobNumber, Guid jobCompanyPK, ZString companyCode)
		{
			var result = new StringBuilder();
			var enableElectronicProcessingCharge = (ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality as BooleanRegistryItem).GetFallBackValueAtAllLevels(jobCompanyPK, Guid.Empty, Guid.Empty);
			if (enableElectronicProcessingCharge)
			{
				var globalChargeCodeGuid = ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeCode;
				var chargeCode = new BusinessObjectFactory().Load<AccChargeCode>(globalChargeCodeGuid)?.AC_Code ?? ZString.Empty;
				result.Append(Res.GetString("ce7b51b9-83bc-49b7-b599-bcef9315d288", @"Please delete all charges excluding system generated JRJ posted against the Electronic Processing Charge ({0}) from this Invoicing Job", chargeCode));
				AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
				result.AppendLine();
				result.Append(Res.GetString("1725f519-039d-445b-9c64-6b2388ec5e5b", @"Please reverse the WIP related to the Electronic Processing Charge ({0})", chargeCode));
				AppendJobNumberAndCompanyCode(result, string.Empty, companyCode);
			}
			else
			{
				if (reason == CannotDeactiveJobReason.HasSavedNonZeroJobCharges)
				{
					result.Append(Res.GetString("9016e5f5-2895-436d-a020-002fa6b29cca", "Please delete all charges from this Invoicing Job"));
					AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
				}
				else if (reason == CannotDeactiveJobReason.HasNotReversedWIPACR)
				{
					result.Append(Res.GetString("1dfaa3a1-6edd-492e-b784-2da02728808c", "Please reverse all WIPs and ACRs before deactivating the job"));
					AppendJobNumberAndCompanyCode(result, jobNumber, companyCode);
				}
			}

			return result.ToString();
		}

		static void AppendJobNumberAndCompanyCode(StringBuilder result, ZString jobNumber, ZString companyCode)
		{
			if (!jobNumber.IsEmpty)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, " ({0})", jobNumber);
			}
			if (!companyCode.IsEmpty)
			{
				var formattedCompanyCode = Res.GetString("043bc01f-85c0-4d02-b2c4-bd27d6a3c396", "in the company {0}", companyCode);
				result.AppendFormat(CultureInfo.InvariantCulture, " {0}", formattedCompanyCode);
			}
			result.Append(".");
		}

		static List<BlockingDetailResult> GetBlockingDetailResult(ZGuid parentPK)
		{
			var result = new List<BlockingDetailResult>();

			var queryResults = GetTableNamesWhichBlockJobHeaderParentDeletion(parentPK);
			foreach (var queryResult in queryResults)
			{
				var jobNum = (ZString)queryResult[JobHeaderSchema.Constants.JH_JobNum];
				var companyCode = (ZString)queryResult[GlbCompanySchema.Constants.GC_Code];
				var companyPK = (ZGuid)queryResult[JobHeaderSchema.Constants.JH_GC];
				var blockingTableName = (ZString)queryResult["TableName"];
				var blockingReason = (ZString)queryResult["BlockingReason"];

				result.Add(new BlockingDetailResult(jobNum, companyCode, companyPK, blockingTableName, blockingReason));
			}

			return result;
		}

		static DynamicBusinessObjectCollection GetTableNamesWhichBlockJobHeaderParentDeletion(ZGuid parentPK)
		{
			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var jobDeactivationRegistryItem = (CodePairRegistryItem)ObjectFactory.Get<IAccounting>().Registry.JobDeactivationConfiguration;

			var isForNPL = jobDeactivationRegistryItem.Value == AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code;
			var chargeCodeForNPL = isForNPL ? accountingRegistryProvider.ElectronicProcessingChargeCode : ZGuid.Empty;

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ParentPK", parentPK, JobHeaderSchema.JH_ParentID);
			parameters.Add("@IsForNPL", isForNPL, Schema.GenericBitSchemaColumn);

			var codeParameter = string.Empty;

			if (isForNPL && chargeCodeForNPL.IsValid)
			{
				var factory = new BusinessObjectFactory();
				codeParameter = factory.Load<AccChargeCode>(chargeCodeForNPL)?.AC_Code ?? string.Empty;
			}
			parameters.Add("@ElectronicProcessingChargeCode", codeParameter, AccChargeCodeSchema.AC_Code);

			var sqlQuery = $"SELECT JH_JobNum, JH_GC, GC_Code, TableName, BlockingReason FROM GetTableNamesWhichBlockJobHeaderParentDeletion(@ParentPK, @ElectronicProcessingChargeCode, @IsForNPL)";
			var queryResults = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			queryResults.Load(sqlQuery, parameters);

			return queryResults;
		}

		class BlockingDetailResult
		{
			public string JobNum { get; }
			public string CompanyCode { get; }
			public ZGuid CompanyPK { get; }
			public string BlockingTableName { get; }
			public string BlockingReason { get; }
			public CannotDeactiveJobReason cannotDeactiveJobReason { get; set; }

			public BlockingDetailResult(string jobNum, string companyCode, ZGuid companyPK, string blockingTableName, string blockingReason)
			{
				JobNum = jobNum;
				CompanyCode = companyCode;
				CompanyPK = companyPK;
				BlockingTableName = blockingTableName;
				BlockingReason = blockingReason;
			}
		}

		public enum CannotDeactiveJobReason
		{
			EmptyReason,
			HasPostedTransactions,
			HasSavedNonZeroJobCharges,
			HasNotReversedWIPACR,
			HasSavedHotCheques,
			HasSavedJobHeaders
		}
	}
}
