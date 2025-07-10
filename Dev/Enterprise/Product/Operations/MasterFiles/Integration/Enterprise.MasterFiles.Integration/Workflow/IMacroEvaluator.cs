using CargoWise.Macros;

namespace Enterprise.MasterFiles.Integration
{
	public interface IMacroEvaluator
	{
		object EvaluateMacroValue(IMacroScope scope, IMacroExpression expression);
	}
}
