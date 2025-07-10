using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	internal static class CompositeKeyGeneratorHelper
	{
		internal static string ExtractCompositeKeyOfSection(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.NotNullOrEmpty(node.Value, nameof(node.Value));
			return node.Value;
		}

		internal static string ExtractCompositeKeyOfChapter(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.NotNullOrEmpty(node.Value, nameof(node.Value));

			return $"{node.Parent.CompositeKey}.{node.Value}";
		}

		internal static string ExtractCompositeKeyOfSubChapter(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.NotNullOrEmpty(node.Value, nameof(node.Value));

			return node.Value == "00" ? $"{node.Parent.CompositeKey}." : $"{node.Parent.CompositeKey}.{node.Value}";
		}

		internal static string ExtractCompositeKeyOfHeading(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.NotNullOrEmpty(node.Value, nameof(node.Value));
			var heading = node.Value.Substring(2, 2);
			return $"{node.Parent.CompositeKey}.{heading}";
		}

		internal static string ExtractCompositeKeyOfSubHeading(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.NotNullOrEmpty(node.Value, nameof(node.Value));
			var subheading = node.Value.Substring(4);

			if (subheading.Length >= 2 && subheading.StartsWith("00", StringComparison.OrdinalIgnoreCase))
			{
				return node.CompositeKey;
			}

			if (subheading.Length == 1)
			{
				return $"{node.Parent.CompositeKey}.{subheading[0]}";
			}

			if (node.Parent.Value.Length == 5 && node.Value.Length == 6)
			{
				return $"{node.Parent.CompositeKey}.{subheading[1]}";
			}

			return subheading[1] == '0'
				? $"{node.Parent.CompositeKey}.{subheading[0]}"
				: $"{node.Parent.CompositeKey}.{subheading[0]}.{subheading[1]}";
		}
	}
}
