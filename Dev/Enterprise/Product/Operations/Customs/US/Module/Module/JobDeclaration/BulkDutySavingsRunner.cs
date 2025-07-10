using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	sealed class BulkDutySavingsRunner
	{
		public void Run(JobDeclaration[] declarations)
		{
			using (var form = new ProgressForm())
			{
				form.ShowProgressBar = true;
				form.ShowCancelButton = false;
				form.Status = "Running Duty Savings Culculation...";
				form.Show();

				var declarationsNotProcessed = Caclulate(declarations, form);

				if (declarationsNotProcessed.Length > 0)
				{
					var information = @"
Duty Savings calculation finished running.

Calculation was not run on the following selected Customs Declarations.
Duty Savings is only applicable where Shipment Type is IMP(Import) and Enable ENS is ticked.

";

					information += new ZStringBuilder(declarationsNotProcessed).ToStringWithDelimiterBetweenAppends(", ");

					Globals.Message.ShowInformation(information);
				}
			}
		}

		string[] Caclulate(JobDeclaration[] declarations, ProgressForm form)
		{
			var result = new List<string>();
			var totalCount = declarations.Length;
			decimal count = 0;

			foreach (var declaration in declarations)
			{
				if (declaration.US_EnableENS && declaration.JE_MessageType == JobMessageTypeList.Codes.Import)
				{
					Calculate(declaration.PK);
				}
				else
				{
					result.Add(declaration.JE_DeclarationReference);
				}

				count++;
				form.PercentComplete = (int)((count / totalCount) * 100);
				form.Refresh();
			}

			return result.ToArray();
		}

		void Calculate(ZGuid decPK)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var declaration = factory.Load<JobDeclaration>(decPK);
			if (declaration != null)
			{
				declaration.AddMergeFetchHints();
				new EntryDutyCalculator(declaration).CalculateDutyReportingDuty();
				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
		}
	}
}
