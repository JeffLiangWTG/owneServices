using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var timeReceivedDateFilter = new CustomsResponseReceivedDateFilter(Constants.TimeReceived);
			timeReceivedDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			timeReceivedDateFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|timeReceived", Constants.TimeReceived);
			result.AddFilter(timeReceivedDateFilter);

			var lrnNumberFilter = new CustomsResponseMessageTextFilter(Constants.LRNNumber, EDIMessageSchema.EM_MessageText, new ZString[] { "BGM+962+", "BGM+963+" });
			lrnNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lrnNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|lrnNumber", Constants.LRNNumber);
			result.AddFilter(lrnNumberFilter);

			var mrnNumberFilter = new CustomsResponseMessageTextFilter(Constants.MRNNumber, EDIMessageSchema.EM_MessageText, new ZString[] { "RFF+ABT:" });
			mrnNumberFilter.Category = FilterCategories.NumbersAndReferences;
			mrnNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|mrnNumber", Constants.MRNNumber);
			result.AddFilter(mrnNumberFilter);

			var agentCodeFilter = new CustomsResponseMessageTextFilter(Constants.AgentCode, EDIMessageSchema.EM_MessageText, new ZString[] { "NAD+AG+" });
			agentCodeFilter.Category = FilterCategories.TextSearch;
			agentCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|agentCode", Constants.AgentCode);
			result.AddFilter(agentCodeFilter);

			var transportDocumentFilter = new CustomsResponseMessageTextFilter(Constants.MasterTransportDocument, EDIMessageSchema.EM_MessageText, new ZString[] { "RFF+AAS:" });
			transportDocumentFilter.Category = FilterCategories.TextSearch;
			transportDocumentFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|masterTransportDocument", Constants.MasterTransportDocument);
			result.AddFilter(transportDocumentFilter);

			var customsOfficeFilter = new CustomsResponseMessageTextFilter(Constants.CustomsOffice, EDIMessageSchema.EM_MessageText, new ZString[] { "LOC+22+" }, ZARefCusCodeListTypes.GetCustomsOfficeList(Factory));
			customsOfficeFilter.Category = FilterCategories.TextSearch;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|customsOffice", Constants.CustomsOffice);
			result.AddFilter(customsOfficeFilter);

			var containerFilter = new CustomsResponseMessageTextFilter(Constants.Container, EDIMessageSchema.EM_MessageText, new ZString[] { "EQD+CN+" });
			containerFilter.Category = FilterCategories.TextSearch;
			containerFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|container", Constants.Container);
			result.AddFilter(containerFilter);

			var receivingProfileFilter = new CustomsResponseMessageTextFilter(Constants.ReceivingProfile, EDIInterchangeSchema.EI_HeaderText, new ZString[] { "UNB+UNOB:4+SARSDECT+", "UNB+UNOB:4+SARSCART+" }, 2, 0);
			receivingProfileFilter.Category = FilterCategories.TextSearch;
			receivingProfileFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|receivingProfile", Constants.ReceivingProfile);
			result.AddFilter(receivingProfileFilter);

			var entryStatusFilter = result.AddTextFilter(Constants.EntryStatus, GetEntryStatusQuery, ZARefCusCodeListTypes.GetCustomsStatusList(Factory));
			entryStatusFilter.Category = FilterCategories.StatusAndFlags;
			entryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|entryStatus", Constants.EntryStatus);

			var agentFilter = result.AddGuidFilter(Constants.Agent, ModuleIDs.Organisation, GetAgentQuery, Organisations);
			agentFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|agent", Constants.Agent);

			var linkedToAJobFilter = result.AddFlagsFilter(Constants.LinkedToAJob, new string[] { Constants.LinkedToAJob }, new GetFlagsQuery[] { GetLinkedToAJobQuery });
			linkedToAJobFilter.Visibility = FilterVisibility.AlwaysVisible;
			linkedToAJobFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|linkedToAJob", Constants.LinkedToAJob);

			var messageTypeFilter = new ModuleTextFilter(Constants.MessageType, EDIMessageSchema.EM_MessageType, NonGenralMessageTypes);
			messageTypeFilter.Category = FilterCategories.StatusAndFlags;
			messageTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			messageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterCollection|messageType", Constants.MessageType);
			result.AddFilter(messageTypeFilter);

			return result;
		}

		#region GetQuery

		ZQuery GetEntryStatusQuery(ZString value)
		{
			return new ZQuery(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, ZString.Format("GIS+{0}:120", value));
		}

		ZQuery GetLinkedToAJobQuery(ZBool value)
		{
			var result = new ZQuery();
			if (value)
			{
				result.AddToFilter(EDIMessageSchema.EM_LinkTable, CusEntryHeaderSchema.Constants.TableName);
				var entryQuery = new ZDBOnlyQuery(typeof(EDIMessage));
				entryQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(CusEntryHeader), EDIMessageSchema.EM_LinkUniqueID), JoinCondition.And);
				result.AddToFilter(entryQuery);
			}
			else
			{
				result.AddToFilter(EDIMessageSchema.EM_LinkTable, SQLComparisonOperator.NotEqual, CusEntryHeaderSchema.Constants.TableName);
				result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, null);
			}
			return result;
		}

		ZQuery GetAgentQuery(ZGuid value)
		{
			var sql = ZString.Format("{0} LIKE (SELECT TOP 1 '%NAD+AG+' + {1} + '[:+'']%' FROM {2} WHERE {3}='{4}' AND {5} = '{6}')",
				EDIMessageSchema.Constants.EM_MessageText, OrgCusCodeSchema.Constants.OK_CustomsRegNo,
				OrgCusCodeSchema.Constants.TableName, OrgCusCodeSchema.Constants.OK_CodeType, OrgCusCode.CodeTypes.AgentCode,
				OrgCusCodeSchema.Constants.OK_OH, value);
			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			result.IncludeBlob(EDIMessageSchema.EM_MessageText);
			result.AddFilterAndZSQLParameterCollection(sql, null);
			return result;
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public const string TimeReceived = "Time Received";
			public const string LRNNumber = "LRN";
			public const string MRNNumber = "MRN";
			public const string ReceivingProfile = "Receiving Profile";
			public const string Agent = "Agent";
			public const string AgentCode = "Agent Code";
			public const string MasterTransportDocument = "Master Transport Document";
			public const string CustomsOffice = "Customs Office";
			public const string Container = "Container";
			public const string EntryStatus = "Entry Status";
			public const string LinkedToAJob = "Linked to a Job";
			public const string MessageType = "Message Type";
		}

		#endregion

		#region Lookups

		OrgHeaderCollection Organisations
		{
			get { return fOrganisations ?? (fOrganisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection fOrganisations;

		protected CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<CustomsResponseMessageTypeList>();

		protected CodeDescriptionPairList NonGenralMessageTypes
		{
			get
			{
				var messageTypeList = new CodeDescriptionPairList(MessageTypes);
				messageTypeList.RemoveCode(CustomsResponseMessageTypeList.Codes.GEN);
				return messageTypeList;
			}
		}

		#endregion
	}
}
