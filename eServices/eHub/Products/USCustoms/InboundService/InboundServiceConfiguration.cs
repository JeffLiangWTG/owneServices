using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public interface IInboundServiceConfiguration
	{
		int PullIntervalInSecond { get; }
		string MessageType { get; }
		bool IsProduction { get; }
		bool SendCopiesToTest { get; }
		string TestBrokerInstanceID { get; }
		string EHubConnectionString { get; }
		string[] PoisonMessageMQErrorCode { get; }
	}

	public class InboundServiceConfiguration : IInboundServiceConfiguration
	{
		public int PullIntervalInSecond
		{
			get
			{
				if (pullIntervalInSecond == 0)
				{
					var pullIntervalInSecondString = ConfigurationManager.AppSettings["PullIntervalInSecond"];

					if (!int.TryParse(pullIntervalInSecondString, out pullIntervalInSecond))
					{
						throw new ArgumentException("PullInterval is not integer");
					}
				}

				return pullIntervalInSecond;
			}
		}
		int pullIntervalInSecond;

		public string MessageType
		{
			get
			{
				if (string.IsNullOrEmpty(messageTypeCached))
				{
					messageTypeCached = ConfigurationManager.AppSettings.Get("MessageType");
					if (string.IsNullOrEmpty(messageTypeCached))
						throw new ApplicationException("Application configuration missing or key 'MessageType' could not be found.");
				}
				return messageTypeCached;
			}
		}
		string messageTypeCached;

		public bool IsProduction
		{
			get
			{
				return Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsProduction"));
			}
		}

		public bool SendCopiesToTest
		{
			get
			{
				return Convert.ToBoolean(ConfigurationManager.AppSettings.Get("SendCopiesToTest"));
			}
		}

		public string TestBrokerInstanceID
		{
			get
			{
				return ConfigurationManager.AppSettings.Get("TestBrokerInstanceID");
			}
		}

		public string EHubConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(eHubConnectionString))
				{
					eHubConnectionString = ConfigurationManager.ConnectionStrings["USCustoms"].ConnectionString;
					if (string.IsNullOrEmpty(eHubConnectionString))
						throw new ApplicationException(
							"Application configuration missing or connection string with key 'USCustoms' could not be found.");
				}
				return eHubConnectionString;
			}
		}
		string eHubConnectionString;

		public string[] PoisonMessageMQErrorCode
		{
			get
			{
				if (poisonMessageMQErrorCode == null)
				{
					poisonMessageMQErrorCode = ConvertToArray((NameValueCollection)ConfigurationManager.GetSection("PoisonMessageMQErrorCodes"));
				}
				return poisonMessageMQErrorCode;
			}
		}
		string[] poisonMessageMQErrorCode;

		string[] ConvertToArray(NameValueCollection source)
		{
			var values = new List<string>();
			for (int i = 0; i < source.Count; i++)
			{
				values.Add(source[i]);
			}

			return values.ToArray();
		}
	}
}
