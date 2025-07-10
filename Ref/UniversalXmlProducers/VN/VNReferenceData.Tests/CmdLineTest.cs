using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenCvSharp;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	/// <summary>
	/// As this is a cmd line exe app, we also need these test cases to verify some basic aspects of the cmd line app
	/// </summary>
	[TestFixture]
	public class CmdLineTest
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "You need to specify which function to run");
		}

		static readonly object[] ChinaEasternSampleCaptcha = Directory.GetFiles("./CaptchaImages", "*.png").ToArray();
		[Test, TestCaseSource(nameof(ChinaEasternSampleCaptcha))]
		public void ProcessCaptchaImageSampleImagesSuccess(string filename)
		{
			using (var fileStream = File.OpenRead(filename))
			{
				var solvedCaptcha = CaptchaSolver.SolveCaptcha(fileStream);
				var imageName = Path.GetFileNameWithoutExtension(filename);
				Assert.That(solvedCaptcha, Is.EqualTo(imageName));
			}
		}

		static readonly object[] ChinaEasternSampleCaptchaFail = Directory.GetFiles("./CaptchaImages/Fail", "*.png").ToArray();
		[Test, TestCaseSource(nameof(ChinaEasternSampleCaptchaFail))]
		public void ProcessCaptchaImageSampleImagesFailure(string filename)
		{
			using (var fileStream = File.OpenRead(filename))
			{
				var solvedCaptcha = CaptchaSolver.SolveCaptcha(fileStream);
				var imageName = Path.GetFileNameWithoutExtension(filename);
				Assert.That(solvedCaptcha, Is.Not.EqualTo(imageName));
			}
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("NoFUNCTION", "Unknown function: NOFUNCTION");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.VNReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			StringAssert.Contains(expectedErrorMessage, message);
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
