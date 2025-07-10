using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Macros;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	class LogMacroExecutor
	{
		readonly string formBuilderMacro;
		internal IMacroExpression GetCompiledLogMacro() => formBuilderMacro.With<StandardLibrary>().CreateExpression();

		public LogMacroExecutor(string logMacro)
		{
			formBuilderMacro = string.Concat("\"", logMacro, "\"");
		}

		public Tuple<string, IEnumerable<ErrorMessage>> Execute(object data)
		{
			using (var scope = new MacroScope(data))
			{
				var variables = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetBaseVariables();
				foreach (var variable in variables)
				{
					scope.SetVariable(variable.Key, variable.Value.Object);
				}

				var macro = GetCompiledLogMacro();
				var result = macro.Evaluate(scope);

				return Tuple.Create(Convert.ToString(result, CultureInfo.InvariantCulture), macro.Errors);
			}
		}
	}
}
