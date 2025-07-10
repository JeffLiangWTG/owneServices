using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.US.Business.MessageProcessors.Statement
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class StatementProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IABIControlMessageBlockA, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var branchPK = message.EM_GB;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;
			var branch = message.Branch ?? message.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			if (branch != null)
			{
				branchPK = branch.PK;
				if (message.MessageBlock.B is IABIControlMessageBlockB bBlock)
				{
					var entryFilerCode = bBlock.EntryFilerCode;
					jobNumber = bBlock.StatementNumber;
					var companyPK = branch.GB_GC;
					if (new CusStatementHeader.Loader(message.Factory).LoadWithStatementNumber(entryFilerCode, jobNumber, companyPK) is CusStatementHeader statementHeader)
					{
						linkUniqueID = statementHeader.PK;
						linkTableName = statementHeader.TableName;
					}
				}
			}
			return new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPK, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var result = new HashSet<string>();
			var branch = message.Branch ?? message.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			if (branch != null)
			{
				if (message.MessageBlock.B is IABIControlMessageBlockB bBlock)
				{
					var entryFilerCode = bBlock.EntryFilerCode;
					var jobNumber = bBlock.StatementNumber;
					var companyPK = branch.GB_GC;
					result.Add(USIUniversalCustomsMessageProcessor.GetStatementJobNumberKey(entryFilerCode, jobNumber, companyPK));
				}
			}
			return result;
		}

		#endregion

		protected abstract string StatementReportSubject { get; }
		protected abstract string StatementReportDescription { get; }
		protected abstract string StatementReportBodyDetails { get; }
		protected abstract string StatementReportFooterDetails { get; }

		protected void FillCusStatementHeaderPropertiesFromAOrBBlock(CusStatementHeader cusStatementHeader)
		{
			cusStatementHeader.ServiceTaskLogger = Logger;
			cusStatementHeader.B2_StatementNumber = B.StatementNumber;
			cusStatementHeader.B2_PrintDate = B.PreliminaryStatementPrintDate;

			if (cusStatementHeader.B2_Status != StatementHeaderStatusList.Codes.Deleted)
			{
				if (B.StatementStatus == "F")
				{
					cusStatementHeader.B2_Status = StatementHeaderStatusList.Codes.Final;
					if (cusStatementHeader.IsMonthlyStatement)
					{
						foreach (CusStatementHeader daily in cusStatementHeader.DailyStatements)
						{
							daily.B2_Status = StatementHeaderStatusList.Codes.Final;
						}
					}
				}
				else if (cusStatementHeader.B2_Status.IsEmpty)
				{
					cusStatementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
					cusStatementHeader.B2_PreparerDistrictPort = B.PreparerDistrictPort;
				}
			}

			cusStatementHeader.B2_EntryFilerCode = B.EntryFilerCode;
			cusStatementHeader.B2_ProcessPort = B.ProcessingDistrictPortCode;
			cusStatementHeader.B2_BranchDesignation = B.ClientBranchDesignation;
			cusStatementHeader.B2_PaymentType = B.PaymentTypeIndicator.ToString();
			cusStatementHeader.B2_ProcessDate = GetProcessDate();
		}

		protected virtual ZDateTime GetProcessDate()
		{
			return A?.CurrentDate ?? ZDateTime.Now;
		}

		public void SetImporterCustomsIDAndImporterOfRecord(CusStatementHeader cusStatementHeader)
		{
			if (PaymentTypeList.IsPaidByImporter(B.PaymentTypeIndicator.ToString()))
			{
				cusStatementHeader.B2_ImporterCustomsID = B.ImporterOfRecordNumber;
				cusStatementHeader.B2_OH_Importer = GetImporterPK(B.ImporterOfRecordNumber, cusStatementHeader);
			}
		}

		ZGuid GetImporterPK(ZString orgCusCode, CusStatementHeader statementHeader)
		{
			var result = ZGuid.Empty;
			if (statementHeader.IsDailyStatement)
			{
				result = GetIORFromStatementLine(statementHeader.ActiveLines);
			}
			else
			{
				foreach (var statement in statementHeader.DailyStatements)
				{
					result = GetIORFromStatementLine(statement.ActiveLines);
					if (!result.IsEmpty)
					{
						break;
					}
				}
			}

			if (result.IsEmpty && !orgCusCode.IsEmpty)
			{
				result = new OrganizationLoader(Factory).LoadTop1OrganisationByCusCode(orgCusCode)?.PK ?? ZGuid.Empty;
			}

			return result;
		}

		ZGuid GetIORFromStatementLine(CusStatementLineStatusCollection activeLines)
		{
			return activeLines.Cast<CusStatementLine>().FirstOrDefault(x => x.Declaration?.IOR != null)?.Declaration.IOR.PK ?? ZGuid.Empty;
		}

		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementMessage;
		}
	}
}
