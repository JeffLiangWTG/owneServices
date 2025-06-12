using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.BizTalk.UnitTestFX;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	public class AsycudaTestCase<T> where T : Microsoft.XLANGs.BaseTypes.TransformBase
	{
		// Note - rather than having many source files to tweak each time CW1 sends different data, we'll have ONE source file. 
		// The main thing we need to change for the different countries is the ActionPurpose code, ZFJ, ZSB, etc. 
		// There will be a few little tweaks, such as sending no customs office code for Sri Lanka, but they can be done on much smaller input files. 

		protected void RunTest(string expectedOutputFileName, string countryCode)
		{
			RunTest("TestFiles.GenericSource.xml", expectedOutputFileName, countryCode);
		}

		protected void RunTest(string inputFileName, string expectedOutputFileName, string countryCode, string transportMode = "", string nature = "")
		{
			var assembly = Assembly.GetExecutingAssembly();
			var mapTester = new MapTester(assembly);

			using (var embeddedStreamExpectedSource = ResourceHelper.GetEmbeddedResource(assembly, inputFileName))
			{
				using (var expectedSourceReader = new StreamReader(embeddedStreamExpectedSource))
				{
					var genericInputText = expectedSourceReader.ReadToEnd();
					genericInputText = genericInputText.Replace("{{COUNTRY_CODE}}", countryCode);
					if (!string.IsNullOrEmpty(transportMode))
					{
						genericInputText = genericInputText.Replace("{{TRANSPORT_MODE}}", transportMode);
					}
					if (!string.IsNullOrEmpty(nature))
					{
						genericInputText = genericInputText.Replace("{{NATURE}}", nature);
					}

					var inputBytes = Encoding.ASCII.GetBytes(genericInputText);
					using (var inputMemStream = new MemoryStream(inputBytes))
					{
						mapTester.Execute<T>(inputMemStream, ResourceHelper.GetEmbeddedResource(assembly, "TestFiles." + expectedOutputFileName), false);
					}
				}
			}
		}
	}
}
