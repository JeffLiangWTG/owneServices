using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequestTypeLookups : AutoExternalRequestTypeLookups
	{
		public ExternalRequestTypeLookups(AutoExternalRequestType parent) : base(parent)
		{
		}

		public CodeDescriptionPairList JobTypeList => new ExternalRequestTypeJobTypes();

		public ExternalRequestInfoTemplateCollection FormTypeList
		{
			get
			{
				var mainQuery = new ZQuery();

				var externalRequestJobType = ((ExternalRequestType)Parent).RQT_JobType;
				if (externalRequestJobType != ExternalRequestTypeJobTypes.Codes.ALL)
				{
					var jobTypeQuery = new ZQuery();
					jobTypeQuery.AddToFilter(ExternalRequestInfoTemplateSchema.RIT_JobType, SQLComparisonOperator.Equal, externalRequestJobType);
					jobTypeQuery.AddToFilter(JoinCondition.Or, ExternalRequestInfoTemplateSchema.RIT_JobType, SQLComparisonOperator.Equal, ExternalRequestTypeJobTypes.Codes.ALL);

					mainQuery = new ZQuery(mainQuery, jobTypeQuery);
				}

				return new ExternalRequestInfoTemplateCollection(Factory, mainQuery);
			}
		}

		public CodeDescriptionPairList AssigneeAddressTypes
		{
			get
			{
				var externalRequestJobType = ((ExternalRequestType)Parent).RQT_JobType.ToString();
				if (externalRequestJobType == ExternalRequestTypeJobTypes.Codes.ALL)
				{
					return new CodeDescriptionPairList();
				}

				return GetRequestSupportedAddressTypesProvider(externalRequestJobType)?.SupportedAssigneeAddressTypes ?? new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList ReviewerAddressTypes
		{
			get
			{
				var externalRequestJobType = ((ExternalRequestType)Parent).RQT_JobType.ToString();
				if (externalRequestJobType == ExternalRequestTypeJobTypes.Codes.ALL)
				{
					return new CodeDescriptionPairList();
				}

				return GetRequestSupportedAddressTypesProvider(externalRequestJobType)?.SupportedReviewerAddressTypes ?? new CodeDescriptionPairList();
			}
		}

		IExternalRequestSupportedAddressTypesProvider GetRequestSupportedAddressTypesProvider(string externalRequestJobType)
		{
			if (ExternalRequestSupportedAddressTypesProviders.ContainsKey(externalRequestJobType) && ExternalRequestSupportedAddressTypesProviders[externalRequestJobType] is ObjectHandle handle && handle.GetObject() is IExternalRequestSupportedAddressTypesProvider provider)
			{
				return provider;
			}

			return null;
		}

		Hashtable ExternalRequestSupportedAddressTypesProviders
		{
			get
			{
				if (externalRequestSupportedAddressTypesProviders == null)
				{
					externalRequestSupportedAddressTypesProviders = (Hashtable)ObjectFactory.Get("ExternalRequestSupportedAddressTypesProviders");
				}

				return externalRequestSupportedAddressTypesProviders;
			}
		}
		Hashtable externalRequestSupportedAddressTypesProviders;
	}
}
