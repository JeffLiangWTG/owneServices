using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class EDIMessageDeliveryContextEvaluator : IEDIMessageDeliveryContextEvaluator
	{
		public IEnumerable<IEDIMessageDeliveryContextResult> GetValues(BusinessObjectFactory factory, IEDIMessageDeliveryContextProvider provider, INotifications notifications)
		{
			var selector = factory.Load<EDIMessageDeliveryContextSelector>(provider.EDIMessageDeliveryContextSelectorPK);
			var roots = provider.GetRoots();

			if (selector != null && roots.Length != 0)
			{
				var evaluator = new WorkflowMacroEvaluator();
				foreach (var line in selector.Lines)
				{
					ZString result;
					try
					{
						var (value, errors) = evaluator.EvaluateMacros(roots, line.ECL_Value);
						AddWarningIfRequired(line.ECL_Value, errors, notifications);
						result = value.ToString();
					}
					catch (WorkflowMacroEvaluationException e)
					{
						notifications?.AddWarning(GetLogMessage(line.ECL_Value, e.MultilingualMessage));
						result = ZString.Empty;
					}
					line.ContextValue = result;
					yield return line;
				}
			}
		}

		void AddWarningIfRequired(string macro, IEnumerable<IReportError> errors, INotifications notifications)
		{
			if (notifications != null && errors.Any())
			{
				notifications.AddWarning(GetLogMessage(macro, errors.First().Message));
			}
		}

		string GetLogMessage(string macro, string errorMessage) => Res.GetString("6EA61792-BDB6-4CC8-A230-DC88CF8C5E14", "Error evaluating Additional Context macro: {0}\r\n{1}", macro, errorMessage);
	}
}
