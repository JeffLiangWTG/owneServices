using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public class CustomContentTypeProvider : IContentTypeProvider
	{
		readonly IContentTypeProvider defaultProvider = new FileExtensionContentTypeProvider();

		static readonly ImmutableDictionary<string, string> FileExtensionAndContentTypeDictionary =
			ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, new Dictionary<string, string>
		{
			{ ".appinstaller", "application/appinstaller" },
			{ ".msix", "application/msix" },
		});

		public CustomContentTypeProvider()
		{
		}

		public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
		{
			var extension = Path.GetExtension(subpath);
			if (FileExtensionAndContentTypeDictionary.TryGetValue(extension, out contentType))
			{
				return true;
			}
			return defaultProvider.TryGetContentType(subpath, out contentType);
		}
	}
}
