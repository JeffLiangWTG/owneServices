using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var ratingHeader = factory.Load(parentTablePrefix, parentID);

			Type result;

			return ratingHeader != null && TaskTypeDictionary.TryGetValue(ratingHeader.GetType(), out result)
				? result
				: null;
		}

		Dictionary<Type, Type> TaskTypeDictionary
		{
			get
			{
				return taskTypeDictionary ?? (taskTypeDictionary = new Dictionary<Type, Type>
					{
						{ typeof(Quote), typeof(QuotationProcessTask) },
						{ typeof(ClientRate), typeof(ClientRateProcessTask) },
						{ typeof(CompanyTariff), typeof(CompanyTariffProcessTask) }
					});
			}
		}
		Dictionary<Type, Type> taskTypeDictionary;

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			var workflowProviderType = workflowDescriptor.WorkflowProviderType;

			if (typeof(Quote).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);
			}
			else if (typeof(ClientRate).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
			}
			else if (typeof(CompanyTariff).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Tariff);
			}
		}
	}
}

