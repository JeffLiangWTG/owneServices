using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace eServices.BuildTools.Tests
{
	[SetUpFixture]
	public static class TestHelpers
	{
		public static string GetResourceAsString(this Type type, string resPath, [CallerMemberName] string callerMember = default)
			=> new StreamReader(GetResourceAsStreamPrivate(resPath, type, callerMember)).ReadToEnd();

		public static Stream GetResourceAsStream(this Type type, string resPath, [CallerMemberName] string callerMember = default)
			=> GetResourceAsStreamPrivate(resPath, type, callerMember);

		public static FileInfo GetResourceAsFile(this Type type, string resPath, [CallerMemberName] string callerMember = default)
		{
			var resFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, resPath);
			var resFile = new FileInfo(resFilePath);
			resFiles.Add(resFile);
			using (var resFileStream = resFile.OpenWrite())
			using (var resStream = GetResourceAsStreamPrivate(resPath, type, callerMember))
				resStream.CopyTo(resFileStream);
			return resFile;
		}

		private static Stream GetResourceAsStreamPrivate(string resPath, Type type = null, string callerMember = default)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var root = assembly.GetName().Name;
			var resPathParts = new List<string> { root, "Files" };
			if (type != null)
			{
				resPathParts.Add(type.FullName.Replace(root, "").Trim('.'));
			}
			if (callerMember != null)
			{
				resPathParts.Add(callerMember);
			}
			resPathParts.Add(resPath);
			var resFullPath = string.Join(".", resPathParts);
			return assembly.GetManifestResourceStream(resFullPath) ?? throw new InvalidOperationException($"Assembly resource not found: {resFullPath}");
		}

		private static ConcurrentBag<FileInfo> resFiles = new ConcurrentBag<FileInfo>();

		[OneTimeTearDown]
		public static void TearDown()
		{
			foreach (var file in resFiles)
			{
				if (file.Exists)
				{
					file.Delete();
				}
			}
		}
	}
}
