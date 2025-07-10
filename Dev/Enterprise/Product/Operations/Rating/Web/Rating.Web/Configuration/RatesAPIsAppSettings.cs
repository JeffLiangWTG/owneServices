
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Web.Configuration
{
	/// <summary>
	/// This class helps with reading RatesAPIs configurations from Application Settings of web.config (or app.config).
	/// </summary>
	public class RatesAPIsAppSettings : IRatesAPIsAppSettings
	{
		/// <summary>
		/// ServerName
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule")]
		public string ServerName
		{
			get
			{
				if (serverName == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ServerName];
					serverName = settingValue ?? string.Empty;
				}

				return serverName;
			}
		}
		string serverName;

		/// <summary>
		/// DatabaseName
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule")]
		public string DatabaseName
		{
			get
			{
				if (databaseName == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName];
					databaseName = settingValue ?? string.Empty;
				}

				return databaseName;
			}
		}
		string databaseName;

		/// <summary>
		/// TokenExpiryDurationInSeconds
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int TokenExpiryDurationInSeconds
		{
			get
			{
				try
				{
					return RatingDataRegistry.Instance.TokenExpiryDurationInSeconds.Value;
				}
				catch
				{
					return 30;
				}
			}
		}

		/// <summary>
		/// SupportJsonMediaType
		/// </summary>
		public bool SupportJsonMediaType
		{
			get
			{
				try
				{
					return RatingDataRegistry.Instance.SupportJsonMediaType.Value;
				}
				catch
				{
					return true;
				}
			}
		}

		/// <summary>
		/// ShowExceptionDetailsInResponse
		/// </summary>
		public bool ShowExceptionDetailsInResponse
		{
			get
			{
				try
				{
					return RatingDataRegistry.Instance.ShowExceptionDetailsInResponse.Value;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
