using System.Text;

namespace CargoWiseNext.Blazor.Components;

public class CssBuilder
{
	readonly HashSet<string> classes = new();

	readonly Func<string, bool> DefaultCondition = (string s) => !string.IsNullOrEmpty(s);

	public CssBuilder AddClass(string? value, bool condition = true)
	{
		if (value == null || !condition)
		{
			return this;
		}

		foreach (var @class in value.Split(' ').Where(DefaultCondition))
		{
			classes.Add(@class);
		}
		return this;
	}

	public string Build()
	{
		return new StringBuilder().AppendJoin(" ", classes).ToString();
	}
}
