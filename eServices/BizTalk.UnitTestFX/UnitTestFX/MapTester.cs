using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.BizTalk.TestTools.Mapper;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.BizTalk.UnitTestFX
{
	/// <summary>
	/// Assert map output with expected output using embedded resources in the consuming test
	/// assembly or streams provided
	/// </summary>
	public class MapTester
	{
		#region Variables

		/// <summary>Embedded resource stream host assembly</summary>
		private Assembly _resourceHost;

		/// <summary>Comparer to use to evaluate equality between the map output and the expected output</summary>
		private ICompare _comparer;

		/// <summary>Extension objects that will superceded the defaults. Used to mock/stub external helpers</summary>
		private Dictionary<string, object> _replacementExtensionObjects;

		private bool deleteTempFiles = true;

		/// <summary>Unsuccessful mapping result error message format</summary>
		public const string MAP_ERROR =
			"Mapping source file did not result in expected output\r\n\r\n" +
			"Input resource: {0}\r\n" +
			"Expected output resource: {1}\r\n" +
			"Actual output: {2}\r\n\r\n" +
			"Differences or Updategram:\r\n{3}";

		/// <summary>Default resource name used in mapping success assertions</summary>
		public const string UNKNOWN_RESOURCE_NAME = "[Unknown]";

		/// <summary>Default namespace of Xslt extension objects</summary>
		public const string SCRIPT_OBJ_NS_FORMAT = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS{0}";

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a MapTester which uses the calling assembly to resolve resource references
		/// </summary>
		public MapTester()
			: this(Assembly.GetCallingAssembly())
		{
		}

		/// <summary>
		/// Used when passing embedded resource names for input and expected output reference.
		/// The embedded resource root name is resolved as the assembly name of the 
		/// <paramref name="resourceHost"/> so the embedded resources can be located using a
		/// shortened form of the resource name path
		/// </summary>
		/// <param name="resourceHost">Assembly that hosts the embedded resources used in the test</param>
		public MapTester(Assembly resourceHost)
			: this(resourceHost, new XmlDiffTool())
		{
		}

		/// <summary>
		/// Provision for a custom Xml comparer and using the calling assembly to resolve resources
		/// </summary>
		/// <param name="comparer">Custom Xml comparer</param>
		public MapTester(ICompare comparer)
			: this(Assembly.GetCallingAssembly(), comparer)
		{
		}

		/// <summary>
		/// Provision for a custom Xml comparer and embedded resource resolver
		/// </summary>
		/// <param name="resourceHost">Assembly that hosts the embedded resources used in the test</param>
		/// <param name="comparer">Custom Xml comparer</param>
		public MapTester(Assembly resourceHost, ICompare comparer)
			: this(resourceHost, comparer, new Dictionary<string,object>())
		{
		}

		/// <summary>
		/// Provision for a replacing map extension objects with mocked or stubbed objects
		/// </summary>
		/// <param name="resourceHost">Assembly that hosts the embedded resources used in the test</param>
		/// <param name="replacementExtensionObjects">Mock or stub helper object collection</param>
		public MapTester(Assembly resourceHost, Dictionary<string, object> replacementExtensionObjects)
			: this(resourceHost, new XmlDiffTool(), replacementExtensionObjects)
		{
		}

		/// <summary>
		/// Provision for both custom Xml comparer and replacing map extension objects with mocked or stubbed objects
		/// </summary>
		/// <param name="resourceHost">Assembly that hosts the embedded resources used in the test</param>
		/// <param name="comparer">Custom Xml comparer</param>
		/// <param name="replacementExtensionObjects">Mock or stub helper object collection</param>
		public MapTester(Assembly resourceHost, ICompare comparer, Dictionary<string, object> replacementExtensionObjects)
		{
			_resourceHost = resourceHost;
			_comparer = comparer;
			_replacementExtensionObjects = replacementExtensionObjects;
		}

		#endregion

		public bool DeleteTempFiles { set { deleteTempFiles = value; } }

		/// <summary>
		/// Asserts that the output of the map is the same as the expected output
		/// </summary>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="inputResourceExtension">Map input. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="expectedResourceExtension">Expected map output. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		public void Execute<T>(string inputResourceExtension, string expectedResourceExtension, bool ignoreComments = true) where T : TransformBase
		{
			if (_resourceHost == null)
			{
				throw new InvalidOperationException("Resource host not set");
			}

			using (Stream input = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
			{
				using (Stream expectedOutput = ResourceHelper.GetEmbeddedResource(_resourceHost, expectedResourceExtension))
				{
					MapResult result = Map<T>(input, expectedOutput, ignoreComments);
					AssertSuccess(result, inputResourceExtension, expectedResourceExtension);
				}
			}
		}

		/// <summary>
		/// Asserts that executing the map results in an exception containing the specified message
		/// </summary>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="inputResourceExtension">Map input. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="inputResourceExtension">Expected exception message.</param>
		public void ExecuteAssertException<T>(string inputResourceExtension, string exceptionMessage) where T : TransformBase
		{
			if (_resourceHost == null)
			{
				throw new InvalidOperationException("Resource host not set");
			}

			using (Stream input = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
			{
				// just use a dummy expectedOutput, as we actually expect an exception.
				using (Stream expectedOutput = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
				{
					try
					{
						MapResult result = Map<T>(input, expectedOutput);
						AssertSuccess(result, inputResourceExtension, inputResourceExtension);
					}
					catch (Exception ex)
					{
						if (ex.InnerException != null)
						{
							ex = ex.InnerException;
						}

						if (!exceptionMessage.Equals(ex.Message, StringComparison.InvariantCultureIgnoreCase))
						{
							throw new UnitTestFXAssertFailedException("The exception from the map was not the expected message.", ex);
						}
						return;
					}

					throw new UnitTestFXAssertFailedException("Expected exception to be thrown, but no exception occured.");
				}
			}
		}

		/// <summary>
		/// Asserts that the output of the map executed as XslCompiledTransform is the same as the expected output
		/// </summary>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="inputResourceExtension">Map input. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="expectedResourceExtension">Expected map output. Extension name of the embedded resource in the calling assembly</param>
		public void ExecuteCompiled<T>(string inputResourceExtension, string expectedResourceExtension) where T : TransformBase
		{
			if (_resourceHost == null)
			{
				throw new InvalidOperationException("Resource host not set");
			}

			using (Stream input = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
			{
				using (Stream expectedOutput = ResourceHelper.GetEmbeddedResource(_resourceHost, expectedResourceExtension))
				{
					MapResult result = MapCompiled<T>(input, expectedOutput);
					AssertSuccess(result, inputResourceExtension, expectedResourceExtension);
				}
			}
		}

#if DEBUG
		/// <summary>
		/// Asserts that the output of the map executed as XslCompiledTransform is the same as the expected output
		/// </summary>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="inputResourceExtension">Map input. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="expectedResourceExtension">Expected map output. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="xslFilePath">Your hard-coded xsl file path to help you debug the xsl file</param>
		/// <remarks>
		/// Please do NOT use this method in your automated unit test; please do NOT check in the source codes where this method is used.
		/// </remarks>
		public void ExecuteCompiledWithXslDebug<T>(string inputResourceExtension, string expectedResourceExtension, string xslFilePath) where T : TransformBase
		{
			if (_resourceHost == null)
			{
				throw new InvalidOperationException("Resource host not set");
			}

			using (Stream input = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
			{
				using (Stream expectedOutput = ResourceHelper.GetEmbeddedResource(_resourceHost, expectedResourceExtension))
				{
					MapResult result = MapCompiledWithXslDebug<T>(input, expectedOutput, xslFilePath);
					AssertSuccess(result, inputResourceExtension, expectedResourceExtension);
				}
			}
		}
#endif

		/// <summary>
		/// Asserts that the output of the map is the same as the expected output
		/// </summary>
		/// <remarks>Consumer must close and dispose stream parameters</remarks>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="inputResourceExtension">Map input. Extension name of the embedded resource in the calling assembly</param>
		/// <param name="expectedOutput">Expected map output</param>
		public void Execute<T>(string inputResourceExtension, Stream expectedOutput, bool ignoreComments = true) where T : TransformBase
		{
			if (_resourceHost == null)
			{
				throw new InvalidOperationException("Resource host not set");
			}

			using (Stream input = ResourceHelper.GetEmbeddedResource(_resourceHost, inputResourceExtension))
			{
				MapResult result = Map<T>(input, expectedOutput, ignoreComments);
				AssertSuccess(result, inputResourceExtension, null);
			}
		}

		/// <summary>
		/// Asserts that the output of the map is the same as the expected output
		/// </summary>
		/// <remarks>Consumer must close and dispose stream parameters</remarks>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="input">Map input</param>
		/// <param name="expectedOutput">Expected map output</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		public void Execute<T>(Stream input, Stream expectedOutput, bool ignoreComments = true) where T : TransformBase
		{
			MapResult result = Map<T>(input, expectedOutput, ignoreComments);
			AssertSuccess(result, null, null);
		}

		/// <summary>
		/// Execute the mapping procedure and determine if the output is as expected
		/// </summary>
		/// <remarks>
		/// If tester has provided an Xslt extension object collection then replace the map's
		/// default instances using the script namespace to resolve
		/// </remarks>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="input">Map source</param>
		/// <param name="expectedOutput">Expected map output</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		/// <returns>Success result with actual output on failure</returns>
		public MapResult Map<T>(Stream input, Stream expectedOutput, bool ignoreComments = true) where T : TransformBase
		{
			XPathDocument xpath = new XPathDocument(input);
			TransformBase mapInstance = Activator.CreateInstance<T>();
			XsltArgumentList transformArgs;

			// See if the tester wants to substitute the external assembly calls with their own implementation
			if (_replacementExtensionObjects.Count == 0)
			{
#pragma warning disable CS0612
				transformArgs = mapInstance.TransformArgs;
#pragma warning disable CS0612
			}
			else
			{
				ExtensionObjects realXtensions = XmlHelper.Deserialize<ExtensionObjects>(mapInstance.XsltArgumentListContent);
				transformArgs = XsltArgListBuilder.Replace(realXtensions, _replacementExtensionObjects);
			}

			using (MemoryStream mapResult = new MemoryStream())
			{
				mapInstance.Transform.Transform(xpath, transformArgs, mapResult, null);
				mapResult.Position = 0;
				return _comparer.Execute(mapResult, expectedOutput, ignoreComments);
			}
		}

		/// <summary>
		/// Execute the mapping procedure as XslCompiledTransform and determine if the output is as expected
		/// </summary>
		/// <remarks>
		/// If tester has provided an Xslt extension object collection then replace the map's
		/// default instances using the script namespace to resolve
		/// </remarks>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="input">Map source</param>
		/// <param name="expectedOutput">Expected map output</param>
		/// <returns>Success result with actual output on failure</returns>
		public MapResult MapCompiled<T>(Stream input, Stream expectedOutput) where T : TransformBase
		{
			XPathDocument xpath = new XPathDocument(input);
			TransformBase mapInstance = Activator.CreateInstance<T>();
			XsltArgumentList transformArgs;

			XslCompiledTransform compiledMap = new XslCompiledTransform();
			XsltSettings settings = new XsltSettings(true, true);

			using (StringReader stringReader = new StringReader(mapInstance.XmlContent))
			{
				XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
				compiledMap.Load(xmlTextReader, settings, new XmlUrlResolver());
			}

			// See if the tester wants to substitute the external assembly calls with their own implementation
			if (_replacementExtensionObjects.Count == 0)
			{
#pragma warning disable CS0612
				transformArgs = mapInstance.TransformArgs;
#pragma warning disable CS0612
			}
			else
			{
				ExtensionObjects realXtensions = XmlHelper.Deserialize<ExtensionObjects>(mapInstance.XsltArgumentListContent);
				transformArgs = XsltArgListBuilder.Replace(realXtensions, _replacementExtensionObjects);
			}

			using (MemoryStream mapResult = new MemoryStream())
			{
				using (XmlWriter mapResultWriter = XmlHelper.CreateFormattedXmlWriter(mapResult))
				{
					compiledMap.Transform(xpath, transformArgs, mapResultWriter);
					mapResult.Position = 0;
					return _comparer.Execute(mapResult, expectedOutput);
				}
			}
		}

#if DEBUG
		/// <summary>
		/// Execute the mapping procedure as XslCompiledTransform and determine if the output is as expected
		/// </summary>
		/// <remarks>
		/// If tester has provided an Xslt extension object collection then replace the map's
		/// default instances using the script namespace to resolve
		/// </remarks>
		/// <typeparam name="T">BizTalk map type</typeparam>
		/// <param name="input">Map source</param>
		/// <param name="expectedOutput">Expected map output</param>
		/// <param name="xslFilePath">Your mapping xsl file path</param>
		/// <returns>Success result with actual output on failure</returns>
		/// <remarks>
		/// Please do NOT use this method in your automated unit test; please do NOT check in the source codes where this method is used.
		/// </remarks>
		public MapResult MapCompiledWithXslDebug<T>(Stream input, Stream expectedOutput, string xslFilePath) where T : TransformBase
		{
			XPathDocument xpath = new XPathDocument(input);
			TransformBase mapInstance = Activator.CreateInstance<T>();
			XsltArgumentList transformArgs;

			XslCompiledTransform compiledMap = new XslCompiledTransform(Debugger.IsAttached);
			XsltSettings settings = new XsltSettings(true, true);

			if (Debugger.IsAttached)
			{
				var xdoc = XDocument.Parse(mapInstance.XmlContent);
				xdoc.Declaration = new XDeclaration("1.0", "utf-8", null);

				var xsltFile = xslFilePath;
				if (string.IsNullOrEmpty(xsltFile) || !File.Exists(xsltFile))
				{
					xsltFile = Path.GetTempFileName();
					xdoc.Save(xsltFile);
				}

				compiledMap.Load(xsltFile, settings, new XmlUrlResolver());
			}
			else
			{
				using (StringReader stringReader = new StringReader(mapInstance.XmlContent))
				{
					XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
					compiledMap.Load(xmlTextReader, settings, new XmlUrlResolver());
				}
			}

			// See if the tester wants to substitute the external assembly calls with their own implementation
			if (_replacementExtensionObjects.Count == 0)
			{
#pragma warning disable CS0612
				transformArgs = mapInstance.TransformArgs;
#pragma warning disable CS0612
			}
			else
			{
				ExtensionObjects realXtensions = XmlHelper.Deserialize<ExtensionObjects>(mapInstance.XsltArgumentListContent);
				transformArgs = XsltArgListBuilder.Replace(realXtensions, _replacementExtensionObjects);
			}

			using (MemoryStream mapResult = new MemoryStream())
			{
				using (XmlWriter mapResultWriter = XmlHelper.CreateFormattedXmlWriter(mapResult))
				{
					compiledMap.Transform(xpath, transformArgs, mapResultWriter);
					mapResult.Position = 0;
					return _comparer.Execute(mapResult, expectedOutput);
				}
			}
		}
#endif

		/// <summary>
		/// Calls TestableMapBase.TestMap, specifying XML input and XML output,
		/// and compares the result with the expected output, asserting success
		/// </summary>
		/// <typeparam name="TMap">The type of the Map to use</typeparam>
		/// <param name="inputResource">The name of the input resource</param>
		/// <param name="expectedOutputResource">The name of the expected output resource</param>
		public void TestXmlToXml<TMap>(string inputResource, string expectedOutputResource)
			where TMap : TestableMapBase
		{
			TestMap<TMap>(inputResource, InputInstanceType.Xml, expectedOutputResource, OutputInstanceType.XML);
		}

		/// <summary>
		/// Calls TestableMapBase.TestMap, specifying Native input and XML output,
		/// and compares the result with the expected output, asserting success
		/// </summary>
		/// <typeparam name="TMap">The type of the Map to use</typeparam>
		/// <param name="inputResource">The name of the input resource</param>
		/// <param name="expectedOutputResource">The name of the expected output resource</param>
		public void TestNativeToXml<TMap>(string inputResource, string expectedOutputResource)
			where TMap : TestableMapBase
		{
			TestMap<TMap>(inputResource, InputInstanceType.Native, expectedOutputResource, OutputInstanceType.XML);
		}

		/// <summary>
		/// Calls TestableMapBase.TestMap, specifying XML input and Native output,
		/// and compares the result with the expected output, asserting success
		/// </summary>
		/// <typeparam name="TMap">The type of the Map to use</typeparam>
		/// <param name="inputResource">The name of the input resource</param>
		/// <param name="expectedOutputResource">The name of the expected output resource</param>
		public void TestXmlToNative<TMap>(string inputResource, string expectedOutputResource)
			where TMap : TestableMapBase
		{
			TestMap<TMap>(inputResource, InputInstanceType.Xml, expectedOutputResource, OutputInstanceType.Native);
		}

		/// <summary>
		/// Calls TestableMapBase.TestMap and compares the result with the expected output, asserting success
		/// </summary>
		/// <typeparam name="TMap">The type of the Map to use</typeparam>
		/// <param name="inputResource">The name of the input resource</param>
		/// <param name="inputType">The type of the input resource</param>
		/// <param name="expectedOutputResource">The name of the expected output resource</param>
		/// <param name="outputType">The type of the expected output</param>
		public void TestMap<TMap>(string inputResource, InputInstanceType inputType, string expectedOutputResource, OutputInstanceType outputType)
			where TMap : TestableMapBase
		{
			using (var sourceFile = GetEmbeddedResourceFile(inputResource))
			using (var expectedFile = GetEmbeddedResourceFile(expectedOutputResource))
			using (var outputFile = GetTempFile())
			{
				var map = Activator.CreateInstance<TMap>();

				map.TestMap(sourceFile, inputType, outputFile, outputType);

				var comparer = _comparer;
				if (outputType != OutputInstanceType.XML && comparer is XmlDiffTool)
				{
					comparer = new TextComparer();
				}

				using (var expected = File.OpenRead(expectedFile))
				using (var actual = File.OpenRead(outputFile))
				{
					var result = comparer.Execute(actual, expected);
					AssertSuccess(result, inputResource, expectedOutputResource, outputFile);
				}
			}
		}

		DisposableFile GetEmbeddedResourceFile(string resourceName)
		{
			string tempFileName = Path.GetTempFileName();
			string fullResourceName = _resourceHost.GetName().Name + '.' + resourceName;

			using (Stream reader = _resourceHost.GetManifestResourceStream(fullResourceName))
			using (Stream writer = new FileStream(tempFileName, FileMode.Create))
			{
				reader.CopyTo(writer);
				writer.Flush();
			}

			return new DisposableFile(tempFileName, deleteTempFiles);
		}

		DisposableFile GetTempFile()
		{
			return new DisposableFile(Path.GetTempFileName(), deleteTempFiles);
		}

		/// <summary>
		/// Performs assertion on the <paramref name="result"/> Success property.  If false then an error
		/// is raised in the form of <see cref="MAP_ERROR"/>
		/// </summary>
		/// <remarks>
		/// <see cref="UNKNOWN_RESOURCE_NAME"/> is used in place of an empty or null 
		/// <paramref name="inputObjectName"/> or <see cref="expectedOutputObjectName"/> value
		/// </remarks>
		/// <param name="result">Result of the call to <see cref="Map&lt;T&gt;"/></param>
		/// <param name="inputObjectName">Name of the input used during test if known. Eg: resource name, file name</param>
		/// <param name="expectedOutputObjectName">Name of the input used during test if known. Eg: resource name, file name</param>
		public static void AssertSuccess(MapResult result, string inputObjectName, string expectedOutputObjectName, string outputFile = null)
		{
			if (result.Success)
			{
				return;
			}

			if (outputFile == null)
			{
				outputFile = Path.GetTempFileName();
				File.WriteAllLines(outputFile, new string[] { result.MapOutput });
			}

			var sourceName = inputObjectName ?? UNKNOWN_RESOURCE_NAME;
			var expectedName = expectedOutputObjectName ?? UNKNOWN_RESOURCE_NAME;
			var errorMessage = String.Format(MAP_ERROR, sourceName, expectedName, outputFile, FormatAssertionOutput(result.OutputUpdateGram));
			throw new UnitTestFXAssertFailedException(errorMessage);
		}

		static string FormatAssertionOutput(string value)
		{
			if (value.Length > 10000)
			{
				value = value.Substring(0, 10000) + "\r\n\r\nOutput truncated due to size.";
			}

			if (Environment.GetEnvironmentVariable("DAT_IS_TESTING") == "true")
			{
				value = value.Replace("&", "&amp;").Replace("<", "&lt;");
			}

			return value;
		}

		class DisposableFile : IDisposable
		{
			string fileName;
			bool deleteFileOnDispose;
			bool disposed;

			public DisposableFile(string fileName, bool deleteFileOnDispose)
			{
				this.fileName = fileName;
				this.deleteFileOnDispose = deleteFileOnDispose;
			}

			public static implicit operator string(DisposableFile file)
			{
				return file.fileName;
			}

			public void Dispose()
			{
				if (!disposed && deleteFileOnDispose)
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
					disposed = true;
				}
			}
		}
	}
}