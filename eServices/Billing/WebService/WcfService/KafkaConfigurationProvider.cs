using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CargoWise.Billing.Kafka.API;
using Confluent.Kafka;

namespace CargoWise.eServices.Billing.WcfService
{
	public class KafkaConfigurationProvider
	{
		public  static  T GetKafkaConfig<T>() where T : ClientConfig, new()
		{
			var baseClientConfig = GetSettings("billingKafkaClientSettings");
			if (typeof(T) == typeof(ProducerConfig))
			{
				var producerSettings = GetSettings("billingKafkaProducerSettings");
				return BillingKafkaClient.GetKafkaConfig<ProducerConfig>(baseClientConfig, producerSettings) as T;
			}
			if (typeof(T) == typeof(ConsumerConfig))
			{
				var consumerSettings = GetSettings("billingKafkaConsumerSettings");
				return BillingKafkaClient.GetKafkaConfig<ConsumerConfig>(baseClientConfig, null, consumerSettings)  as T;
			}
			if (typeof(T) == typeof(AdminClientConfig))
			{
				return BillingKafkaClient.GetKafkaConfig<AdminClientConfig>(baseClientConfig) as T;
			}
			return BillingKafkaClient.GetKafkaConfig<ClientConfig>(baseClientConfig) as T;
		}

		private static Dictionary<string, string> GetSettings(string sectionName)
		{
			var settings = (Hashtable)ConfigurationManager.GetSection(sectionName);
			var settingsDic = settings.Cast<DictionaryEntry>().ToDictionary(x => x.Key.ToString(), y => y.Value.ToString());
			return settingsDic;
		}
	}
}
