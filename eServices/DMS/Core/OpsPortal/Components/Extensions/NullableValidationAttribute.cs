using System.ComponentModel.DataAnnotations;

namespace eServices.Dms.Core.OpsPortal.Components.Extensions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
	AllowMultiple = false)]
public sealed class NullableValidationAttribute<T>() : DataTypeAttribute("Nullable") where T : DataTypeAttribute
{
	private readonly T validationAttribute = (T)Activator.CreateInstance(typeof(T))!;

	new public string? ErrorMessage
	{
		get => validationAttribute.ErrorMessage;
		set => validationAttribute.ErrorMessage = value;
	}

	public override bool IsValid(object? value)
	{
		if (value is null || value is string s && string.IsNullOrEmpty(s))
			return true;
		return validationAttribute.IsValid(value);
	}

	public override string FormatErrorMessage(string name)
		=> validationAttribute.FormatErrorMessage(name);
}
