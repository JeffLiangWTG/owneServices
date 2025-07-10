using System;
using System.Management.Automation;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	class PowerShellScriptErrorBuilderTest
	{
		[TestCase("Could not detect any platforms from XX in SkiaSharp 2.88.6, please tell the package authors", "at <ScriptBlock>, Deploy.ps1: line 12", "")]
		[TestCase("normal error message", "at <ScriptBlock>, Deploy.ps1: line 43", "normal error message\r\nat <ScriptBlock>, Deploy.ps1: line 43\r\n")]
		[TestCase(default(string), "at <ScriptBlock>, Deploy.ps1: line 210", "\r\nat <ScriptBlock>, Deploy.ps1: line 210\r\n")]
		public void TestErrorBuilderHandleErrorMessage(string errorRecordToString, string errorRecordScriptStackTrace, string expectedBuilderResult)
		{
			var errorBuilder = new PowerShellScriptErrorBuilder();
			var errorRecordMock = new Mock<ErrorRecord>(new Mock<Exception>().Object, "1", ErrorCategory.NotSpecified, new Mock<object>().Object);
			errorRecordMock.Setup(o => o.ToString()).Returns(errorRecordToString);
			var errorRecord = errorRecordMock.Object;
			var scriptStackTraceField = typeof(ErrorRecord).GetField("_scriptStackTrace", BindingFlags.NonPublic | BindingFlags.Instance);
			scriptStackTraceField?.SetValue(errorRecord, errorRecordScriptStackTrace);
			errorBuilder.Append(errorRecord);
			var errorMessage = errorBuilder.Build();
			Assert.That(errorMessage, Is.EqualTo(expectedBuilderResult));
		}
	}
}
