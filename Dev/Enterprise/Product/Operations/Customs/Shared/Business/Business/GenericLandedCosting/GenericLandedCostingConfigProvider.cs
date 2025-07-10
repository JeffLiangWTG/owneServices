using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	public sealed class GenericLandedCostingConfigProvider
	{
		const string GenericLandedCostingConfigLocation = "Enterprise.Customs.Business.GenericLandedCosting.GenericLandedCostingConfig.xml";

		public static GenericLandedCostingConfigProvider Instance
		{
			get { return instance ?? (instance = new GenericLandedCostingConfigProvider()); }
		}

		[ThreadSafe]
		static GenericLandedCostingConfigProvider instance;

		public IEnumerable<string> SupportedCountries
		{
			get { return GenericLandedCostingConfigDict.Keys.ToArray(); }
		}

		public GenericLandedCostingConfig GetGenericLandedCostingConfig(string countryCode)
		{
			GenericLandedCostingConfig config;

			if (!GenericLandedCostingConfigDict.TryGetValue(countryCode, out config))
			{
				ErrorReporter.ReportOnce("GenericLandedCostingConfig for " + countryCode + " cannot be found on " + GenericLandedCostingConfigLocation);
			}
			return config;
		}

		Dictionary<string, GenericLandedCostingConfig> GenericLandedCostingConfigDict
		{
			get
			{
				if (fGenericLandedCostingConfigDict == null)
				{
					lock (lockObject)
					{
						if (fGenericLandedCostingConfigDict == null)
						{
							LoadConfigFromConfiguration();
						}
					}
				}
				return fGenericLandedCostingConfigDict;
			}
		}
		Dictionary<string, GenericLandedCostingConfig> fGenericLandedCostingConfigDict;

		readonly object lockObject = new object();

		void LoadConfigFromConfiguration()
		{
			fGenericLandedCostingConfigDict = new Dictionary<string, GenericLandedCostingConfig>();

			XElement rootElement = null;
			Stream stream = null;
			try
			{
				stream = typeof(GenericLandedCostingConfig).Assembly.GetManifestResourceStream(GenericLandedCostingConfigLocation);
				using (var reader = XmlReader.Create(stream))
				{
					stream = null;
					rootElement = XElement.Load(reader);
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
			if (rootElement != null)
			{
				foreach (var element in rootElement.Elements())
				{
					var config = new GenericLandedCostingConfig(element);
					fGenericLandedCostingConfigDict.Add(config.CountryCode, config);
				}
			}
		}
	}
}
