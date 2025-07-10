using System.Text;

namespace CargoWiseNext.Blazor.Components;

public class StyleBuilder
{
	readonly StringBuilder styles = new();

	public StyleBuilder AddStyle(string prop, string? value, bool condition = true)
	{
		if (!condition || string.IsNullOrEmpty(prop) || string.IsNullOrEmpty(value))
		{
			return this;
		}

		styles.Append($"{prop}:{value};");
		return this;
	}

	public StyleBuilder AddStyle(string? value, bool condition = true)
	{
		if (!condition || string.IsNullOrWhiteSpace(value))
		{
			return this;
		}

		styles.AppendJoin(" ", value.Trim());
		return this;
	}

	public string? Build() => styles.Length > 0 ? styles.ToString() : null;
}
