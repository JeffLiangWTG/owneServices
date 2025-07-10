using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Freight.DangerousGoods;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public class SegregationRulesManager : ISegregationRulesManager
	{
		readonly IEnumerable<ISegregationRule> _incompatibilityRules;
		readonly IEnumerable<ISegregationRule> _exemptionRules;

		public SegregationRulesManager(IEnumerable<ISegregationRule> rules)
		{
			this._incompatibilityRules = rules.Where(r => !r.IsExemption).ToList();
			this._exemptionRules = rules.Where(r => r.IsExemption).ToList();
		}

		public IEnumerable<DGPairInfo<T>> Check<T>(IEnumerable<string> standardsToCheckAgainst, IEnumerable<T> items, Func<T, string, (UNDGClassificationData, UNDGSubstance, string)> fetchEntity)
		{
			var listOfItems = items.ToList();
			var listOfDgSubstancePairInfo = new List<DGPairInfo<T>>();

			foreach (var ruleStandard in standardsToCheckAgainst)
			{
				var rulesForStandard = _incompatibilityRules.Where(r => r.ApplicableStandard == ruleStandard).ToList();
				var exemptionForStandard = _exemptionRules.Where(r => r.ApplicableStandard == ruleStandard).ToList();

				for (var i = 0; i < listOfItems.Count; i++)
				{
					var (classification1, undgSubstance1) = GetQuantityClassificationAndSubstance(fetchEntity, ruleStandard, listOfItems[i]);

					for (var j = i + 1; j < listOfItems.Count; j++)
					{
						var (classification2, undgSubstance2) = GetQuantityClassificationAndSubstance(fetchEntity, ruleStandard, listOfItems[j]);

						if (undgSubstance1 == undgSubstance2)
						{
							continue;
						}

						var listOfMessagesForPair = new List<Message>();

						foreach (var rule in rulesForStandard)
						{
							var ruleMessages = rule.Check(undgSubstance1, undgSubstance2);
							if (!ruleMessages.IsNullOrEmpty())
							{
								listOfMessagesForPair.AddRange(ruleMessages.WhereNotNull());
							}
						}

						if (listOfMessagesForPair.Any(m => m.Type == MessageType.Error))
						{
							foreach (var rule in exemptionForStandard)
							{
								var segregationMessages = rule.Check(undgSubstance1, undgSubstance2);
								if (segregationMessages.IsNullOrEmpty())
								{
									listOfMessagesForPair.RemoveAll(m => m.Type == MessageType.Error);
									break;
								}
							}
						}
						var adjustedMessages = AdjustMessagesBasedOnClassifications(listOfMessagesForPair, classification1, classification2);

						listOfDgSubstancePairInfo.AddRange(adjustedMessages.Select(m => new DGPairInfo<T>(listOfItems[i], listOfItems[j], ruleStandard, m)));
					}
				}
			}

			return listOfDgSubstancePairInfo;
		}

		static List<Message> AdjustMessagesBasedOnClassifications(List<Message> messages, string classification1, string classification2)
		{
			if (string.IsNullOrEmpty(classification1) || string.IsNullOrEmpty(classification2))
			{
				return messages;
			}

			var adjustedMessages = new List<Message>();
			foreach (var message in messages)
			{
				if (message.Type == MessageType.Error)
				{
					var newMessageType = GetMessageType(classification1, classification2);
					if(newMessageType != MessageType.Error)
					{
						adjustedMessages.Add(new Message(newMessageType, message.Text));
						continue;
					}
				}
				adjustedMessages.Add(message);
			}
			return adjustedMessages;
		}

		static MessageType GetMessageType(string classification1, string classification2) =>
			(classification1, classification2) switch
			{
				(QuantityClassifications.Regulated, QuantityClassifications.Regulated) => MessageType.Error,
				(QuantityClassifications.Regulated, QuantityClassifications.Limited) => MessageType.Error,
				(QuantityClassifications.Regulated, QuantityClassifications.Excepted) => MessageType.Warning,
				(QuantityClassifications.Limited, QuantityClassifications.Regulated) => MessageType.Error,
				(QuantityClassifications.Limited, QuantityClassifications.Limited) => MessageType.Warning,
				(QuantityClassifications.Limited, QuantityClassifications.Excepted) => MessageType.Warning,
				(QuantityClassifications.Excepted, QuantityClassifications.Regulated) => MessageType.Warning,
				(QuantityClassifications.Excepted, QuantityClassifications.Limited) => MessageType.Warning,
				(QuantityClassifications.Excepted, QuantityClassifications.Excepted) => MessageType.Warning,
				_ => MessageType.Error
			};

		static (string, UNDGSubstance) GetQuantityClassificationAndSubstance<T>(Func<T, string, (UNDGClassificationData, UNDGSubstance, string)> fetchEntity, string ruleStandard, T item)
		{
			var (undgClassificationData, undgSubstance, errorMessage) = fetchEntity(item, ruleStandard);
			if (undgClassificationData == null || undgSubstance == null)
			{
				throw new DataNotFoundException(errorMessage);
			}

			return (undgClassificationData.QuantityClassification, undgSubstance);
		}
	}
}
