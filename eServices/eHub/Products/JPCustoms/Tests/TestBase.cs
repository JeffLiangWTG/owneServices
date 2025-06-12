using System;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;
using CargoWise.eHub.Shared.IssueManager;
using CargoWise.eHub.Shared.IssueManagerTests;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestBase
	{
		public TestBase()
		{
			logger = new TestLogger();
		}

		protected DirectoryInfo AuditFolder
		{
			get { return auditFolder ?? (auditFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "JPCustoms", Guid.NewGuid().ToString().Replace("-", "")))); }
		}

		DirectoryInfo auditFolder;
		readonly TestLogger logger;

		protected string FileNamePattern
		{
			get
			{
				return "JPCustoms_AuditLog_{0}.txt";
			}
		}

		protected ILog Logger
		{
			get
			{
				return logger;
			}
		}

		protected IssueManagerForTesting TestIssueManager
		{
			get { return new IssueManagerForTesting();}
		}

		protected IDateTimeProvider DateTimeProvider
		{
			get
			{
				return new TestDateTimeProvider { DateTimeNow = new DateTime(2013, 09, 26, 22, 45, 11, 555) };
			}
		}

		protected IPullRunnerConfiguration PullRunnerConfiguration
		{
			get
			{
				return new TestPullRunnerConfiguration(1);
			}
		}

		protected ISmtpMailClientConfiguration SmtpMailClientConfiguration
		{
			get
			{
				return new TestSmtpMailClientConfiguration(Logger);
			}
		}

		protected IPop3MailClientConfiguration Pop3MailClientConfiguration
		{
			get
			{
				return new TestPop3MailClientConfiguration(Logger);
			}
		}

		public IAuditLoggerConfiguration AuditLoggerConfiguration
		{
			get
			{
				return new TestAuditLoggerConfiguration(DateTimeProvider, AuditFolder, FileNamePattern);
			}
		}

		protected string ReadFileAsString(string filePath)
		{
			return new StreamReader(filePath).ReadToEnd();
		}

		protected static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		protected static string GetEmbeddedResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		protected static byte[] GetEmbeddedResourceAsByteArray(string resourceName)
		{
			byte[] buffer = new byte[16 * 1024];

			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var memoryStream = new MemoryStream())
				{
					int read;
					while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						memoryStream.Write(buffer, 0, read);
					}
					return memoryStream.ToArray();
				}
			}
		}

		protected static bool CompareStreams(Stream expectedStream, MemoryStream actualStream)
		{
			expectedStream.Position = 0;
			actualStream.Position = 0;

			if (expectedStream.Length != actualStream.Length)
			{
				return false;
			}

			while (true)
			{
				int expectedByte = expectedStream.ReadByte();
				int actualByte = actualStream.ReadByte();

				if (expectedByte != actualByte)
				{
					return false;
				}

				if (expectedByte == -1 && actualByte == -1)
				{
					return true;
				}
			}
		}

	}
}
