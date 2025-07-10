using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IPrelimStatementDetailsDefault : Customs.Business.IRegistryAccessingSupporter
	{
		//Information needed for decision making
		bool IsManualPayment { get; }
		bool FixPSD { get; }
		bool RegistryAllowsDefaulting { get; }

		ZDate BaseDateToCalculateOn { get; }
		OrgHeader IOR { get; }
		GlbBranch Branch { get; }
		ZString US_PaymentType { get; }

		bool ShouldValidatePastDate { get; }
		string DatePrecedenceMessage { get; }
		int DaysToAddToStatementDate { get; }

		//Fields to default
		ZDateTime US_PreliminaryStatementPrintDate { get; set; }
		ZString US_PeriodicStatementMM { get; set; }
	}

	class PrelimStatementDetailsDefaulter
	{
		public void Default(IPrelimStatementDetailsDefault declaration)
		{
			DefaultPeriodicStatementMonth(declaration);
			DefaultPreliminaryStatementDate(declaration);
		}

		void DefaultPreliminaryStatementDate(IPrelimStatementDetailsDefault declaration)
		{
			if (!declaration.FixPSD)
			{
				if (declaration.IsManualPayment)
				{
					declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
				}
				else
				{
					if (declaration.RegistryAllowsDefaulting)
					{
						ZDateTime calculatedPSPDate = new AddInfoJobDeclarationWorkingDate().GeneratePrelimStmtDate(declaration);

						if (!calculatedPSPDate.IsEmpty)
						{
							declaration.US_PreliminaryStatementPrintDate = calculatedPSPDate;
						}
					}
				}
			}
		}

		protected virtual void DefaultPeriodicStatementMonth(IPrelimStatementDetailsDefault declaration)
		{
		}
	}
}
