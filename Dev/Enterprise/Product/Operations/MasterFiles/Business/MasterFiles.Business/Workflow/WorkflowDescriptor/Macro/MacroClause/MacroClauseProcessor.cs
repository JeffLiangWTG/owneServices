using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	class MacroClauseProcessor : IMacroClauseProcessor
	{
		public MacroClauseProcessor(INotifications notifications, WorkflowMacroValidation validation)
		{
			this.notifications = notifications;
			this.validation = validation;
		}

		void IMacroClauseProcessor.ProcessPropertyAndValue(string valuePath, out string nextPropertyPath, out string expression)
		{
			Process(PropertyAndValueClauses, valuePath, out nextPropertyPath, out expression);
		}

		internal MacroClause ProcessPropertyAndValue(string valuePath, out string nextPropertyPath, out string expression)
		{
			return Process(PropertyAndValueClauses, valuePath, out nextPropertyPath, out expression);
		}

		internal MacroClause ProcessProperty(string valuePath, out string nextPropertyPath, out string expression)
		{
			return Process(PropertyClauses, valuePath, out nextPropertyPath, out expression);
		}

		internal MacroClause ProcessValue(string valuePath, out string nextValuePath, out string expression)
		{
			return Process(ValueClauses, valuePath, out nextValuePath, out expression);
		}

		MacroClause Process(List<MacroClause> macroClauses, string path, out string nextPath, out string expression)
		{
			expression = string.Empty;

			nextPath = path;

			var clause = GetClause(macroClauses, path);

			if (clause != null)
			{
				expression = clause.GetExpression(path, out nextPath);
				clause.Validate(expression, notifications, validation);
			}

			return clause;
		}

		public MacroClause GetClause(List<MacroClause> macroClauses, string propertyPath)
		{
			foreach (var macroClause in macroClauses)
			{
				if (macroClause.IsStartWithClause(propertyPath))
				{
					return macroClause;
				}
			}

			return null;
		}

		public bool IsValueClause(string path)
		{
			foreach (var macroClause in ValueClauses)
			{
				if (macroClause.IsContainClause(path))
				{
					return true;
				}
			}

			return false;
		}

		public List<MacroClause> PropertyClauses
		{
			get
			{
				if (propertyClauses == null)
				{
					propertyClauses = new List<MacroClause>
					{
						new WhereMacroClause()
					};
				}
				return propertyClauses;
			}
		}
		List<MacroClause> propertyClauses;

		public List<MacroClause> ValueClauses
		{
			get
			{
				if (valueClauses == null)
				{
					valueClauses = new List<MacroClause>
					{
						new FirstMacroClause(),
						new FirstOrDefaultMacroClause()
					};
				}
				return valueClauses;
			}
		}
		List<MacroClause> valueClauses;

		public List<MacroClause> PropertyAndValueClauses
		{
			get
			{
				if (propertyAndValueClauses == null)
				{
					propertyAndValueClauses = PropertyClauses.Union(ValueClauses).ToList();
				}
				return propertyAndValueClauses;
			}
		}
		List<MacroClause> propertyAndValueClauses;

		readonly INotifications notifications;
		readonly WorkflowMacroValidation validation;
	}
}
