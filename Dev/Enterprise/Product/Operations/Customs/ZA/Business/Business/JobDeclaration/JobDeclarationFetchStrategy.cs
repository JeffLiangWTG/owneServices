using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.FetchStrategy
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration => BusinessObject as JobDeclaration;

		protected override bool IsCusEntryInstructionRelatedColumn(string columnName)
			=> columnName == JobDeclaration.Schema.CaseNumbers || columnName == JobDeclaration.Schema.SupportingDocumentStatus;

		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			base.FetchForViewDeclaration(columns);

			if (columns.Any(x => IsCusEntryInstructionRelatedColumn(x.ColumnName)))
			{
				var entryInstructions = Factory.Load<CusEntryInstruction>(new ZQuery(CusEntryInstructionSchema.CEI_ClusterKey, Declaration.JE_ClusterKey));
				foreach (var entryInstruction in entryInstructions)
				{
					Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, entryInstruction.PK);
					foreach (var caseNum in entryInstruction.CaseNumbers)
					{
						Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, caseNum.PK);
					}
				}
			}

			foreach (TableColumn tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case JobDeclaration.Schema.CombinedUCREntryNumbers:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, Declaration.PK);
						Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, Declaration.PK);
						break;

					case JobDeclaration.Schema.CombinedReleasePrintIndicator:
						Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, Declaration.PK);
						break;

					default:
						break;
				}
			}
		}

		#region FetchForMerge

		protected override void AddMergeFetchHintsFor(Customs.Business.CusEntryHeader entry)
		{
			base.AddMergeFetchHintsFor(entry);
			Factory.AddFetchHint(CusEntryPayInfoSchema.C9_CH, entry.PK);

			var query = new ZQuery();
			query.AddToFilter(CusCodeDataSchema.CY_ParentID, entry.PK);
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.VOCValueBefore);
			Factory.AddFetchHint(CusCodeDataSchema.Instance, query);
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.CusEntryLine entryLine)
		{
			base.AddMergeFetchHintsFor(entryLine);
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, entryLine.PK);
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.CusEntryInstruction instruction)
		{
			base.AddMergeFetchHintsFor(instruction);
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, instruction.PK);
		}

		protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			AddFetchHintForRefCusProcedure(invoiceLine);
		}

		void AddFetchHintForRefCusProcedure(BaseJobComInvoiceLine invoiceLine)
		{
			var procedure = invoiceLine.ProcedureCode;

			if (!procedure.IsEmpty)
			{
				var customsCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				var concession = invoiceLine.JI_Calc_Concession;
				var prevProcedure = invoiceLine.JI_Calc_PreviousProcedure;

				var query = Universal.RefCusProcedure.Loader.GetFilter(customsCountryCode, ZDateTime.Today);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedure);

				if (!prevProcedure.IsEmpty && !concession.IsEmpty)
				{
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, prevProcedure);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession);
				}
				else
				{
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, prevProcedure);
				}

				Factory.AddFetchHint(RefCusProcedureSchema.Instance, query);
			}
		}

		#endregion

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
			{
				var query = Universal.RefCusProcedure.Loader.GetFilter(invoiceLine.CountryCode, ZDateTime.Today);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, invoiceLine.ProcedureCode);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.NotEqual, string.Empty);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, invoiceLine.Declaration?.JE_MessageType ?? ZString.Empty);
				Factory.AddFetchHint(RefCusProcedureSchema.Instance, query);
			}

			foreach (CusEntryInstruction instruction in Declaration.CustomsEntryInstructions)
			{
				AddFetchHintForRefCusProcedure(instruction);
			}

			if (Declaration.IsImport)
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					var query = ProvisionalPaymentEntryPayInfoCollection.GetFetchHintQueryForProvisionalPaymentPayInfos(Factory, entry);
					Factory.AddFetchHint(CusEntryPayInfoSchema.Instance, query);
				}
			}
		}

		void AddFetchHintForRefCusProcedure(CusEntryInstruction instruction)
		{
			var query = Universal.RefCusProcedure.Loader.GetFilter(instruction.CountryCode, ZDateTime.Today);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, instruction.CEI_Style);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
			Factory.AddFetchHint(RefCusProcedureSchema.Instance, query);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();

			foreach (CusEntryInstruction instruction in Declaration.CustomsEntryInstructions)
			{
				if (instruction.IsInDatabase)
				{
					Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, instruction.PK);
				}
			}

			foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
			{
				if (invoiceLine.IsInDatabase)
				{
					AddFetchHintForRefCusProcedure(invoiceLine);
				}
			}
		}
	}
}
