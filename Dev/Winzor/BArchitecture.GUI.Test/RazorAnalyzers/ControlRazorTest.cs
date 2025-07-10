#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace WinzorFramework.Test;

class ControlRazorTest
{
	// NOTE: Don't use TestCaseSource attribute for razor files because it makes test discovery unacceptable slow in the DAT environment,
	// as it causes network access to read source files
	[TestCase("stopPropagation", new[] { "@oncontextmenu", "@onclick", "@ondblclick", "@onmouseover", "@onmouseout", "@onwinzorfocusin", "@onsplittermoved", 
		"@ontextboxselectionchange", "@onmousedown", "@onmouseup", "@ondragstart", "@ondragenter", "@ondragleave", "@ondragover", "@ondrop", "@onwinzordragend" }, TestName = "{m}_stopPropogation")]
	[TestCase("preventDefault", new[] { "@oncontextmenu" }, TestName = "{m}_preventDefault")]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void RazorEventSufixAnalyser(string eventSuffix, string[] events)
	{
		Assert.Multiple(() =>
		{
			foreach (var path in RazorProjectSourcePath)
			{
				var fs = RazorProjectFileSystem.Create(path);
				var files = fs.EnumerateItems(path).ToArray();
				Assert.That(files, Is.Not.Empty);
				foreach (var file in files)
				{
					var syntaxNode = GetCSharpSyntaxTreeRootNode(fs, file);
					var invocations = syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
					var lastElement = string.Empty;

					if (IsAllowNoStopPropagation(invocations))
					{
						continue;
					}
					for (var i = 0; i < invocations.Length; i++)
					{
						var invocation = invocations[i];
						if (IsAddAttribute(invocation))
						{
							var attributeName = GetAddAttributeValue(invocation);
							if (!attributeName.Contains(":") && events.Contains(attributeName, StringComparer.OrdinalIgnoreCase))
							{
								var proceedingInvocationsForAttribute = invocations.Skip(i + 1).Where(e => e != null && IsBuilderExpression(e)).TakeWhile(e => IsAddAttribute(e) && GetAddAttributeValue(e).Contains(attributeName, StringComparison.OrdinalIgnoreCase));
								var expectedAttribute = $"{attributeName}:{eventSuffix}";
								Assert.That(proceedingInvocationsForAttribute.Select(e => GetAddAttributeValue(e)), Does.Contain(expectedAttribute), $"{file.FileName}:{Environment.NewLine}  Missing {expectedAttribute} for event {attributeName} on <{lastElement}> element, {attributeName} events on winzor controls should {eventSuffix} to match winforms behaviour");
							}
						}
						else if (invocation.Expression.TryGetInferredMemberName() == "OpenElement")
						{
							lastElement = GetArgumentStringLiteralValue(invocation, 1);
						}
					}
				}
			}
		});
	}

	[Test]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void RazorDataAttributeAnalyser()
	{
		Assert.Multiple(() =>
		{
			foreach (var path in RazorProjectSourcePath)
			{
				var fs = RazorProjectFileSystem.Create(path);
				var files = fs.EnumerateItems(path).ToArray();
				Assert.That(files, Is.Not.Empty);
				var compilation = CSharpCompilation.Create("RazorCompilation")
						.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
						.AddReferences(MetadataReference.CreateFromFile(typeof(Control).Assembly.Location));

				foreach (var file in files)
				{
					var syntaxNode = GetCSharpSyntaxTreeRootNode(fs, file);
					var classDeclaration = syntaxNode.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
					var semanticModel = compilation.AddSyntaxTrees(syntaxNode.SyntaxTree).GetSemanticModel(syntaxNode.SyntaxTree);
					var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
					var isControl = IsDerivedFrom(classSymbol!, "Control");
					if (!isControl || RazorAttributesIgnoreControlList.Contains(classSymbol!.Name))
					{
						continue;
					}
					var invocations = syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();

					var dataAttributes = invocations.FirstOrDefault(e => IsAddAttribute(e!) && GetAddAttributeValue(e!).StartsWith("@attributes", StringComparison.OrdinalIgnoreCase), null);
					Assert.That(dataAttributes, Is.Not.Null, $"{file.FileName}:{Environment.NewLine}  Missing data attributes for winzor controls based on System.Windows.Control");
				}
			}
		});
	}

	[Test]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void ControlProxyComponentRequiresKeyAttribute()
	{
		Assert.Multiple(() =>
		{
			foreach (var path in RazorProjectSourcePath)
			{
				var fs = RazorProjectFileSystem.Create(path);
				var files = fs.EnumerateItems(path).ToArray();
				Assert.That(files, Is.Not.Empty);
				foreach (var file in files)
				{
					var syntaxNode = GetCSharpSyntaxTreeRootNode(fs, file);
					var parsedElements = ParseElementAndAttributes(syntaxNode);
					var controlProxyComponents = parsedElements.Where(e => e.ElementTag.Contains(nameof(ControlProxyComponent)));
					foreach (var controlProxyComponent in controlProxyComponents)
					{
						if (!controlProxyComponent.Attributes.TryGetValue(nameof(ControlProxyComponent.Control), out var control))
						{
							Assert.Fail($"{file.FileName}\r\n\tControlProxyComponent elements must specify the Control attribute");
							continue;
						}

						var expectedKeyValue = $"{control}.{nameof(Control.WinzorControlGuid)}";

						if (!controlProxyComponent.Attributes.TryGetValue("@key", out var keyValue))
						{
							Assert.Fail($"{file.FileName}\r\n\tControlProxyComponent elements must specify the @key attribute with the Control's WinzorControlGuid. Add the following attribute to the ControlProxyComponent: @key={expectedKeyValue}");
							continue;
						}

						if (keyValue != expectedKeyValue)
						{
							Assert.Fail($"{file.FileName}\r\n\tControlProxyComponent has the incorrect @key attribute set. Use the following attribute instead: @key={expectedKeyValue}");
							continue;
						}
					}
				}
			}
		});
	}

	bool IsBuilderExpression(InvocationExpressionSyntax invocation) => invocation.ToString().StartsWith("__builder", StringComparison.OrdinalIgnoreCase);

	bool IsAddAttribute(InvocationExpressionSyntax invocation) => invocation.Expression.TryGetInferredMemberName() == "AddAttribute";

	bool IsAllowNoStopPropagation(InvocationExpressionSyntax[] invocations) => invocations.Any(x => x.ArgumentList.Arguments.Any(y => y.ToString().Trim('"') == "allowNoStopPropagation"));

	static bool IsDerivedFrom(INamedTypeSymbol classSymbol, string baseControlName)
	{
		var baseType = classSymbol.BaseType;
		while (baseType != null)
		{
			if (baseType.Name == baseControlName)
			{
				return true;
			}
			baseType = baseType.BaseType;
		}

		return false;
	}

	string GetAddAttributeValue(InvocationExpressionSyntax invocation) => GetArgumentStringLiteralValue(invocation, 1)!;

	string? GetArgumentStringLiteralValue(InvocationExpressionSyntax invocation, int i)
	{
		var args = invocation.ArgumentList.Arguments;
		if (i > args.Count)
		{
			return null;
		}
		return args[i].ToString().Trim('"');
	}

	IEnumerable<Element> ParseElementAndAttributes(SyntaxNode syntaxNode)
	{
		var invocations = syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
		var elements = new Stack<Element>();
		foreach (var invocation in invocations)
		{
			switch (invocation.Expression.TryGetInferredMemberName())
			{
				case "OpenElement":
					{
						elements.Push(new Element(GetArgumentStringLiteralValue(invocation, 1)!, new Dictionary<string, string?>()));
						break;
					}
				case "CloseElement":
					{
						yield return elements.Pop();
						break;
					}
				case "AddAttribute":
					{
						elements.Peek().Attributes.Add(GetArgumentStringLiteralValue(invocation, 1)!, GetArgumentStringLiteralValue(invocation, 2));
						break;
					}
				default:
					{
						break;
					}
			}
		}
	}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
	record Element(string ElementTag, Dictionary<string, string?> Attributes);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

	SyntaxNode GetCSharpSyntaxTreeRootNode(RazorProjectFileSystem fs, RazorProjectItem item)
	{
		var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs);
		var razorDoc = engine.Process(item);
		var cSharpDoc = (RazorCSharpDocument)razorDoc.Items.Single(kp => kp.Value is RazorCSharpDocument).Value;
		var sourceText = SourceText.From(cSharpDoc.GeneratedCode, Encoding.UTF8);
		return CSharpSyntaxTree.ParseText(sourceText).GetRoot();
	}

	static IEnumerable<string> RazorProjectSourcePath => new[] { Path.Combine(BaseSourcePath, "Winzor"), Path.Combine(BaseSourcePath, @"Enterprise\Architecture\GUI\Enterprise.ZArchitecture.GUI\Winzor") };

	static string BaseSourcePath
	{
		get
		{
			var baseSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
			if (string.IsNullOrEmpty(baseSourcePath))
			{
				baseSourcePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ControlRazorTest).Assembly.Location)));
			}
			return baseSourcePath!;
		}
	}

	// the following controls are not standard winforms controls so ignore checking attributes for them
	static string[] RazorAttributesIgnoreControlList => new[]
	{
		"SplashScreen",
		"NextHomeUserControl",
		"NotificationIcon",
		"DataGridAddNewRow",
		"DataGridRelationshipRow",
		"ElementHost",
		"ErrorProvider",
		"OpenFileDialog",
		"SaveFileDialog",
		"ToolStripGrip",
		"ToolStripSeparator",
		"ToolStripSplitButton",
		"DiagramAreaUserControl",
		"NetworkUserControl",
		"BalloonWindow",
		"ZDropForm",
		"ZDynamicMultilineTextBoxForm",
		"RecentItemsControl",
		"TileBarControl"
	};
}
