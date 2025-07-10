using System;

namespace Enterprise.eTail.Integration
{
	public interface IConvertToStandAloneDeclarationResponse
	{
		bool ConversionSucceeded { get; }
		string ConversionFailureReason { get; }
		Guid ConvertedStandAloneDeclaration { get; }
	}
}
