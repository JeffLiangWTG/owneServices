using System;
using System.Configuration;
using Enterprise.Rating.Web.Configuration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test
{
	public class RatesAPIsAppSettingsTest : TransactionedTestCase
	{
		public void TestServerName()
		{
			ConfigurationManager.AppSettings[ApplicationSettings.ServerName] = null;
			var configHelper = new RatesAPIsAppSettings();
			AssertEquals(string.Empty, configHelper.ServerName);

			ConfigurationManager.AppSettings[ApplicationSettings.ServerName] = string.Empty;
			configHelper = new RatesAPIsAppSettings();
			AssertEquals(string.Empty, configHelper.ServerName);

			ConfigurationManager.AppSettings[ApplicationSettings.ServerName] = "SomeTests";
			configHelper = new RatesAPIsAppSettings();
			AssertEquals("SomeTests", configHelper.ServerName);
		}

		public void TestDatabaseName()
		{
			ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName] = null;
			var configHelper = new RatesAPIsAppSettings();
			AssertEquals(string.Empty, configHelper.DatabaseName);

			ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName] = string.Empty;
			configHelper = new RatesAPIsAppSettings();
			AssertEquals(string.Empty, configHelper.DatabaseName);

			ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName] = "SomeTests";
			configHelper = new RatesAPIsAppSettings();
			AssertEquals("SomeTests", configHelper.DatabaseName);
		}

		public void TestTokenExpiryDurationInSeconds()
		{
			var configHelper = new RatesAPIsAppSettings();
			AssertEquals(30, configHelper.TokenExpiryDurationInSeconds);

			using (RatingDataRegistry.Instance.TokenExpiryDurationInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 78))
			{
				AssertEquals(78, configHelper.TokenExpiryDurationInSeconds);
			}
		}

		public void TestSupportJsonMediaType()
		{
			var configHelper = new RatesAPIsAppSettings();
			AssertEquals(true, configHelper.SupportJsonMediaType);

			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, configHelper.SupportJsonMediaType);
			}
		}

		public void TestShowExceptionMessagesInResponse()
		{
			var configHelper = new RatesAPIsAppSettings();
			AssertEquals(false, configHelper.ShowExceptionDetailsInResponse);

			using (RatingDataRegistry.Instance.ShowExceptionDetailsInResponse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, configHelper.ShowExceptionDetailsInResponse);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			ConfigurationManager.AppSettings[ApplicationSettings.ServerName] = null;
			ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName] = null;
		}
	}
}
