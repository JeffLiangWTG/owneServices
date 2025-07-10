using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RazorAnalysers
{
	[Test]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void TextFieldsHaveCorrectContextMenuBehavior()
	{
		Assert.Multiple(() =>
		{
			foreach (var path in RazorProjectSourcePath)
			{
				var fs = RazorProjectFileSystem.Create(path);
				var files = fs.EnumerateItems(path).ToArray();
				// Uncomment the following line once Enterprise.BufferManagement.NetworkVisualisation.GUI contains razor files
				//Assert.That(files, Is.Not.Empty);

				foreach (var file in files)
				{
					var syntaxNode = GetCSharpSyntaxTreeRootNode(fs, file);
					var elements = GroupAttributesByElement(syntaxNode);

					foreach (var textFieldAttributes in elements.Where(IsTextField))
					{
						var attributes = textFieldAttributes.Select(a => GetLiteralExpressionArgument(a, 1));
						var elementType = GetLiteralExpressionArgument(textFieldAttributes.Key, 1);

						Assert.That(attributes, Does.Contain("@oncontextmenu"), $"{file.FileName} contains a {elementType} element that does not have an @oncontextmenu action. Text fields should handle the context menu event and open the clipboard actions menu to match the WPF behavior.");
						Assert.That(attributes, Does.Contain("@oncontextmenu:stopPropagation"), $"{file.FileName} contains a {elementType} element without @oncontextmenu:stopPropagation");
						Assert.That(attributes, Does.Contain("@oncontextmenu:preventDefault"), $"{file.FileName} contains a {elementType} element without @oncontextmenu:preventDefault");
					}
				}
			}
		});
	}

	IEnumerable<IGrouping<InvocationExpressionSyntax, InvocationExpressionSyntax>> GroupAttributesByElement(SyntaxNode syntaxNode)
	{
		var invocations = syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();

		var attributes = new List<(InvocationExpressionSyntax element, InvocationExpressionSyntax attribute)>();
		var elements = new Stack<InvocationExpressionSyntax>();
		foreach (var invocation in invocations)
		{
			Action action = invocation.Expression.TryGetInferredMemberName() switch
			{
				"OpenElement" => () => elements.Push(invocation),
				"CloseElement" => () => elements.Pop(),
				"AddAttribute" => () => attributes.Add((elements.Peek(), invocation)),
				_ => () => { },
			};
			action();
		}
		return attributes.GroupBy(a => a.element, a => a.attribute);
	}

	bool IsTextField(IGrouping<InvocationExpressionSyntax, InvocationExpressionSyntax> element)
	{
		var elementType = GetLiteralExpressionArgument(element.Key, 1);

		if (elementType.Equals("textarea", StringComparison.InvariantCultureIgnoreCase))
		{
			return true;
		}
		else if (elementType.Equals("input", StringComparison.InvariantCultureIgnoreCase))
		{
			var typeAttribute = element.Where(a => GetLiteralExpressionArgument(a, 1) == "type").Select(a => GetLiteralExpressionArgument(a, 2)).SingleOrDefault();
			if (typeAttribute is null || typeAttribute == "text")
			{
				return true;
			}
		}
		return false;
	}

	string GetLiteralExpressionArgument(InvocationExpressionSyntax invocation, int arg) => invocation.ArgumentList.Arguments[arg].Expression is LiteralExpressionSyntax exp ? exp.Token.ValueText : throw new InvalidOperationException();

	SyntaxNode GetCSharpSyntaxTreeRootNode(RazorProjectFileSystem fs, RazorProjectItem item)
	{
		var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs);
		var razorDoc = engine.Process(item);
		var cSharpDoc = (RazorCSharpDocument)razorDoc.Items.Single(kp => kp.Value is RazorCSharpDocument).Value;
		var sourceText = SourceText.From(cSharpDoc.GeneratedCode, Encoding.UTF8);
		return CSharpSyntaxTree.ParseText(sourceText).GetRoot();
	}

	static IEnumerable<string> RazorProjectSourcePath => new[]
	{
		Path.Combine(BaseSourcePath, "Winzor", "NetworkVisualisation.GUI.Winzor"),
		Path.Combine(BaseSourcePath, "Enterprise", "Product", "Operations", "BufferManagement", "NetworkVisualisation", "NCN.GUI" )
	};

	static string BaseSourcePath
	{
		get
		{
			var baseSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
			if (string.IsNullOrEmpty(baseSourcePath))
			{
				baseSourcePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(RazorAnalysers).Assembly.Location)));
			}
			return baseSourcePath ?? string.Empty;
		}
	}
}
