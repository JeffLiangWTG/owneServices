using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class MacroEvaluator : IMacroEvaluator
	{
		public object EvaluateMacroValue(IMacroScope scope, IMacroExpression expression)
		{
			try
			{
				return expression.Evaluate(scope);
			}
			catch (OperationOnInvalidZDateTimeException)
			{
				return null;
			}
		}
	}
}
