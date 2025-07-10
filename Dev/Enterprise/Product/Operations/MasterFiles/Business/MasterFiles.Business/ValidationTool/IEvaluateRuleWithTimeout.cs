using System;

namespace Enterprise.MasterFiles.Business;

public interface IEvaluateRuleWithTimeout
{
	(bool Passed, bool Timeout) EvaluateWithTimeout(Func<bool> evaluation);
}
