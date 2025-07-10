using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests;

sealed class CaptchaSolverTest
{
	const int ExpectedAccuracy = 50;

	[Test]
	public void TestSolveCaptchaAccuracy()
	{
		var captchaImgDir = @"Shared\INTestFiles\CaptchaImg";
		var captchaImgPaths = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, captchaImgDir));

		var correctCount = 0;
		foreach (var captchaImgPath in captchaImgPaths)
		{
			var expectedCaptchaText = Path.GetFileNameWithoutExtension(captchaImgPath);
			var actualCaptchaText = CaptchaSolver.SolveCaptcha(File.ReadAllBytes(captchaImgPath));
			if (expectedCaptchaText.Equals(actualCaptchaText, StringComparison.OrdinalIgnoreCase))
			{
				correctCount++;
			}
		}
		var accuracy = (double)correctCount / captchaImgPaths.Length * 100;
		Assert.That(accuracy >= ExpectedAccuracy, $"Captcha solver accuracy is {accuracy:F2}%, expected at least {ExpectedAccuracy}% correct answers.");
	}
}
