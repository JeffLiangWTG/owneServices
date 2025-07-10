using System;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.RefDbRepo.Client.Common;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class ErrorReportingClientWrapperTest : TestCase
	{
		public void TestShouldReport()
		{
			var wrapper = new ErrorReportingClientWrapper(false);
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "JNBCO-WLML-1"));
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "UnknownName"));
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "SHACO-LVLO-1"));
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "ORDWT-SPRC-5"));
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));

			var exception = new Exception("One or more errors occurred.", new HttpRequestException());
			AssertEquals(false, wrapper.ShouldReport(exception, "SYDSP-SWEB-1"));
			exception = new Exception("One or more errors occurred.", new NotSupportedException());
			AssertEquals(true, wrapper.ShouldReport(exception, "SYDSP-SWEB-1"));
		}

		[UseSnapshotProtection]
		public void TestShouldReportWhenREFServiceUriChanges()
		{
			var wrapper = new ErrorReportingClientWrapper(false);
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));

			RefDataRepoRegistry.Instance.RefDbRepoServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www-refdbrepo.sand.wtg.zone/");
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));
		}

		[UseSnapshotProtection]
		public void TestShouldReportWhenEnableIssueReportIsOn()
		{
			var wrapper = new ErrorReportingClientWrapper(false);
			var wrapper1 = new ErrorReportingClientWrapper(true);
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "JNBCO-WLML-1"));
			AssertEquals(false, wrapper1.ShouldReport(new Exception(), "JNBCO-WLML-1"));

			RemoteDatabaseRegistry.Instance.EnableIssueReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "JNBCO-WLML-1"));
			AssertEquals(true, wrapper1.ShouldReport(new Exception(), "JNBCO-WLML-1"));
		}

		[UseSnapshotProtection]
		public void TestShouldReportWhenRDUServiceUriChangesORSRDbNameChanges()
		{
			var wrapper = new ErrorReportingClientWrapper(true);
			AssertEquals(true, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));

			RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CW-RefDatabase1");
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));

			RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www-refdbrepo.sand.wtg.zone/");
			AssertEquals(false, wrapper.ShouldReport(new Exception(), "SYDSP-SWEB-1"));
		}

		public void TestIsReporting()
		{
			var validMachineNameForSendingReports = "ORDWT-SPRC-5";
			var message1 = "Testing 1";
			var message2 = "Testing 2";
			var wrapper = new ErrorReportingClientWrapper(false);
			try
			{
				ThrownExceptionWithMessage(message1);
			}
			catch (Exception ex)
			{
				wrapper.PostCrashReport(ex, validMachineNameForSendingReports);
			}

			AssertGreaterThan(ErrorReporter.ExceptionsThrown.Count, 0);
			AssertEquals(message1, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			try
			{
				ThrownExceptionWithMessage(message2);
			}
			catch (Exception ex)
			{
				wrapper.PostCrashReport(ex, validMachineNameForSendingReports);
			}

			AssertGreaterThan(ErrorReporter.ExceptionsThrown.Count, 0);
			AssertEquals(message2, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestIsAvoidingSomeExceptions()
		{
			var validMachineNameForSendingReports = "ORDWT-SPRC-5";
			foreach (var message in ErrorReportingKnownExceptionsMapping.IgnoredExceptionMessages)
			{
				var wrapper = new ErrorReportingClientWrapper(false);
				try
				{
					ThrownExceptionWithMessage(message);
				}
				catch (Exception ex)
				{
					wrapper.PostCrashReport(ex, validMachineNameForSendingReports);
				}

				AssertEquals(ErrorReporter.ExceptionsThrown.Count, 0);
			}
		}

		public void TestShouldReportIssue_RefApplicationException()
		{
			var wrapper = new ErrorReportingClientWrapper(false);
			var exception = new RefApplicationException { ReportIssue = true };
			AssertEquals(true, wrapper.ShouldReport(exception, "SYDSP-SWEB-1"));
			exception = new RefApplicationException { ReportIssue = false };
			AssertEquals(false, wrapper.ShouldReport(exception, "SYDSP-SWEB-1"));
		}

		public void ThrownExceptionWithMessage(string message)
		{
			throw new Exception(message);
		}
	}
}
