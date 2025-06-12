using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings;
using NUnit.Framework;
using System;
using System.IO;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	[TestFixture]
	public class ClientMappingsInsecureFTPTests
	{
		[Test]
		public void Constructor_ShouldThrowArgumentException_WhenCsvFileNameIsNullOrEmpty()
		{
			var ex1 = Assert.Throws<ArgumentException>(() => new ClientMappingsInsecureFTP(null));
			Assert.That(ex1.Message, Does.Contain("File name cannot be null or empty\r\nParameter name: csvFilePath"));
			var ex2 = Assert.Throws<ArgumentException>(() => new ClientMappingsInsecureFTP(string.Empty));
			Assert.That(ex2.Message, Does.Contain("File name cannot be null or empty\r\nParameter name: csvFilePath"));
		}

		[Test]
		public void Constructor_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
		{
			string nonExistentFile = "nonexistent.csv";

			var ex = Assert.Throws<FileNotFoundException>(() => new ClientMappingsInsecureFTP(nonExistentFile));
			Assert.That(ex.Message, Does.Contain("CSV file not found: nonexistent.csv"));
		}

		[Test]
		public void IsMatch_ShouldReturnTrue_WhenRuleMatches()
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile,
				"Sender,Recipient,Interface\n" +
				"\"AG1PQCSIN_DOR\",\"AG1PQCSIN\",\"Dole Order csv-file: Receive OrderManager Orders\"\n" +
				"\"ALUORDORD.*\",\"ALUORDORD_FTP\",\"ediEnterprise XML-Transfer to ALPI's ftp Server\"\n" +
				"\"DFDEWRPRD.*\",\"DFDEWRPRD_EXW\",\"ExWorks XML - Send Transport Booking\"\n" +
				"\".*\",\"KNASYDSYD_CSS\",\"CIEL Status xml - Send Shipment Status\"\n");

				var insecureFTP = new ClientMappingsInsecureFTP(tempFile);

				Assert.IsTrue(insecureFTP.IsMatch("AG1PQCSIN_DOR", "AG1PQCSIN", "Dole Order csv-file: Receive OrderManager Orders"));
				Assert.IsTrue(insecureFTP.IsMatch("ALUORDORD", "ALUORDORD_FTP", "ediEnterprise XML-Transfer to ALPI's FTP Server"));
				Assert.IsTrue(insecureFTP.IsMatch("DFDEWRPRD", "DFDEWRPRD_EXW", "ExWorks XML - Send Transport Booking"));
				Assert.IsTrue(insecureFTP.IsMatch("DFDEWRPRD_EPD", "DFDEWRPRD_EXW", "ExWorks XML - Send Transport Booking"));
				Assert.IsTrue(insecureFTP.IsMatch("KNASYDSYD", "KNASYDSYD_CSS", "CIEL Status xml - Send Shipment Status"));
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void IsMatch_ShouldReturnFalse_WhenNoRuleMatches()
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile,
				"Sender,Recipient,Interface\n" +
				"\"AG1PQCSIN_DOR\",\"AG1PQCSIN\",\"Dole Order csv-file: Receive OrderManager Orders\"\n" +
				"\"DFDEWRPRD.*\",\"DFDEWRPRD_EXW\",\"ExWorks XML - Send Transport Booking\"\n" +
				"\".*\",\"KNASYDSYD_CSS\",\"CIEL Status xml - Send Shipment Status\"\n");

				var insecureFTP = new ClientMappingsInsecureFTP(tempFile);

				Assert.IsFalse(insecureFTP.IsMatch("UNKNOWN", "AG1PQCSIN", "Dole Order csv-file: Receive OrderManager Orders"));
				Assert.IsFalse(insecureFTP.IsMatch("DFDEWRPRD", "UNKNOWN", "ExWorks XML - Send Transport Booking"));
				Assert.IsFalse(insecureFTP.IsMatch("KNASYDSYD", "KNASYDSYD_CSS", "UNKNOWN"));
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}
