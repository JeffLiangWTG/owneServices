namespace Enterprise.MasterFiles.Business.Testing;

public class DummyValidationToolSettings(DummyWorkflowDescriptor owner) : ValidationToolSettings(owner)
{
	protected override bool SupportsValidationRulesCore() => ValidationRulesSupported;

	public bool ValidationRulesSupported;

	protected override bool IsValidationRulesAvailableForGlobalTemplatesCore() => IsValidationRulesAvailableForGlobalTemplatesForTest;

	public bool IsValidationRulesAvailableForGlobalTemplatesForTest;
}
