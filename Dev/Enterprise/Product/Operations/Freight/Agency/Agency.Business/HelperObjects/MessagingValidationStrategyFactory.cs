using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Enterprise.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public static class MessagingValidationStrategyFactory
	{
		public static MessageValidationStrategy[] GetStrategies()
		{
			if (AllStrategies == null)
			{
				AllStrategies = new List<MessageValidationStrategy>();
				var xml = Env.Registry.GetFilterCriteria(MessagingValidationStrategyFactoryKey);

				if (!string.IsNullOrEmpty(xml))
				{
					var root = XElement.Parse(xml);
					var xmlStrategies = root.Elements(NodeName.Strategy);

					foreach (var strategyType in AllStrategyTypes)
					{
						var strategy = Activator.CreateInstance(strategyType) as MessageValidationStrategy;
						strategy.IsEnabled = GetIsEnabledFromElement(xmlStrategies, strategyType.FullName);

						AllStrategies.Add(strategy);
					}
				}
				else
				{
					foreach (var strategyType in AllStrategyTypes)
					{
						var strategy = Activator.CreateInstance(strategyType) as MessageValidationStrategy;
						AllStrategies.Add(strategy);
					}
				}
			}

			return AllStrategies.Where(x => x.IsApplicable).ToArray();
		}

		public static void SaveCustomiseStrategies()
		{
			var root = new XElement(NodeName.Strategies);
			var doc = new XDocument(root);
			doc.Declaration = new XDeclaration("1.0", "utf-16", null);

			foreach (var strategy in AllStrategies)
			{
				var item = new XElement(NodeName.Strategy);
				var menuNameNode = new XElement(NodeName.MessageValidationStrategyType, strategy.GetType().FullName);
				var isEnabledNode = new XElement(NodeName.IsEnabled, strategy.IsEnabled);

				item.Add(menuNameNode);
				item.Add(isEnabledNode);

				root.Add(item);
			}

			Env.Registry.SetFilterCriteria(MessagingValidationStrategyFactoryKey, doc.ToString());
		}

		static bool GetIsEnabledFromElement(IEnumerable<XElement> elements, string strategyType)
		{
			var element = elements.FirstOrDefault(x => x.Element(NodeName.MessageValidationStrategyType).Value == strategyType);

			return element == null || (bool)element.Element(NodeName.IsEnabled);
		}

		[ThreadStatic]
		static List<MessageValidationStrategy> AllStrategies;
		const string MessagingValidationStrategyFactoryKey = "Enterprise.Freight.Agency.Business.MessagingValidationStrategyFactory";

#if DEBUG

		public static void ResetAllStrategiesAndSetFilterCriteria(string xml)
		{
			AllStrategies = null;
			Env.Registry.SetFilterCriteria(MessagingValidationStrategyFactoryKey, xml);
		}

#endif

		static Type[] AllStrategyTypes => new[]
		{
			typeof(PortAuthorityBusinessObjectValidation),
			typeof(EIDOBusinessObjectValidation),
			typeof(NZPortMessageValidationStrategy),
			typeof(NZReleaseOrderMessageValidationStrategy),
			typeof(DangerousGoodsManifestMessageValidationStrategy)
		};
	}

	#region SuppressResourceStringsCheckRegion

	static class NodeName
	{
		public const string Strategies = "Strategies";
		public const string Strategy = "Strategy";
		public const string MessageValidationStrategyType = "MessageValidationStrategyType";
		public const string IsEnabled = "IsEnabled";
	}

	#endregion
}
