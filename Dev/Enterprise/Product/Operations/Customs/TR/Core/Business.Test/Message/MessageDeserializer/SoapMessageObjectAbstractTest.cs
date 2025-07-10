using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestsSubclassesOf(typeof(SoapMessageObject))]
	abstract class SoapMessageObjectAbstractTest<TMessageObject> : TestCaseWithFactory
		where TMessageObject : SoapMessageObject
	{
		public void TestEmptyText()
		{
			var logger = new LoggingInformation();
			var testObject = (SoapMessageObject)Activator.CreateInstance(typeof(TMessageObject), string.Empty, Factory.New<TRBaseMessage>(), logger);
			_ = testObject.InnerMessageObjects.FirstOrDefault();
			AssertEquals("Should be able to run and write a log.", true, logger.Logs.Any(log => log.Type == Integration.LogType.Error && log.Message.Contains("Unrecognized message text,")));
		}

		public void TestInvalidXml()
		{
			var invalidXml = @"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<?xml version=""1.0"" encoding=""utf-8"" ?>
Unrecodnized
<Unrecodnized>
	<content>
	</content>
</UnrecodnizedUnrecodnized>";
			var logger = new LoggingInformation();
			var testObject = (SoapMessageObject)Activator.CreateInstance(typeof(TMessageObject), invalidXml, Factory.New<TRBaseMessage>(), logger);
			_ = testObject.InnerMessageObjects.FirstOrDefault();
			AssertEquals("Should be able to run and write a log.", true, logger.Logs.Any(log => log.Type == Integration.LogType.Error && log.Message.Contains("Unrecognized message text,")));
		}

		public void TestUnrecodnizedXml()
		{
			var unrecognizedXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Unrecodnized>
	<content>
	</content>
</Unrecodnized>";
			var logger = new LoggingInformation();
			var testObject = (SoapMessageObject)Activator.CreateInstance(typeof(TMessageObject), unrecognizedXml, Factory.New<TRBaseMessage>(), logger);
			_ = testObject.InnerMessageObjects.FirstOrDefault();
			AssertEquals("Should be able to run and write a log.", true, logger.Logs.Any(log => log.Type == Integration.LogType.Error && log.Message.Contains("Unrecognized message text,")));
		}
	}
}
