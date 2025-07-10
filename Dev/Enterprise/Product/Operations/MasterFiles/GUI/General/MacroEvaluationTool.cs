using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Macros.GUI;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Macro;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	sealed class MacroEvaluationTool : IMacroEvaluationTool
	{
		public string Name => (NoResString)"Macro Evaluator";
		public bool ShowAsButton => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public Form Run(IWin32Window parentForm)
		{
			if (parentForm == null)
			{
				Globals.Message.ShowInformation(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} tool cannot run because must be initialized with a parent form.", Name));
				return null;
			}

			var dataSource = (parentForm as IDataBoundControl)?.DataSource as IBusiness;

			const string environmentVariableName = "env";
			const string formVariableName = "form";

			var scope = new MacroScope(dataSource);
			scope.SetVariable(environmentVariableName, new Business.Macros.Environment());
			scope.SetVariable(formVariableName, parentForm);

			var context = new IMacroLibrary[]
			{
				new CargoWiseOneStandardLibrary()
			}.CreateContext();
			return Run(parentForm, input: null, context: context, scope: scope);
		}

		Form IMacroEvaluationTool.Run(IWin32Window parentForm, string defaultMacro, IAntlrMacroContextProvider provider)
		{
			var context = provider.GetSampleContext();
			return Run(parentForm, defaultMacro, context.Libraries.CreateContext(), context.Scope);
		}

		public MacroEvaluationForm Run(IWin32Window form, string input, IMacroEvaluationContext context, IMacroScope scope)
		{
			var macroEvaluationForm = new MacroEvaluationForm(context, input: input, scope: scope);
			macroEvaluationForm.Show(form);
			return macroEvaluationForm;
		}
	}
}
