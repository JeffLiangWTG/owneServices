using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class QueryMessageFilterStripBusinessObject : MQEDIMessageCommonFilterStripBusinessObject
	{
		protected override ZBool ShouldAddApplicationCodeFilter => false;

		protected override bool ShouldAddApplicationReferenceFilter => false;

		protected override ZBool ShouldAddDirectionFilter => false;

		protected override ZBool ShouldAddInterchageDetailFilters => false;

		protected override ZBool ShouldAddMessageTypeSubTypeFilter => false;

		protected override ZBool ShouldAddMessageTimeFilter => false;

		protected override ZBool ShouldAddEHubIDFilters => false;

		protected override ZBool ShouldAddReceiverFilters => false;

		protected override bool ShouldAddStatusFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var typeFilter = result.AddTextFilter("Message Type", EDIMessageSchema.EM_MessageType, GetQueryMessageTypeList());
			typeFilter.MaxLength = EDIMessageSchema.EM_MessageType.MaxLength;

			var filterPart = result.AddTextFilter(Constants.Status, GetMessageStatusQuery, EM_Status_List);
			filterPart.MultilingualDescription = ResString.GetMultilingualString("3D02E8C2-01AB-4664-8785-55F0949D46FA", "Status");
			filterPart.Category = FilterCategories.StatusAndFlags;
			filterPart.MaxLength = EDIMessageSchema.EM_Status.MaxLength;

			return result;
		}

		internal ZQuery GetMessageStatusQuery(ZString value)
		{
			var result = new ZQuery();

			if (value == Constants.AllExceptAcknowledgments)
			{
				result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Acknowledged);
			}
			else if (value == MQEDIMessage.Status.Received)
			{
				var messageResTypeList = GetResponseQueryMessageTypeList().ToArray().Select(x => x.Code).ToList();
				var messageTypeList = GetQueryMessageTypeList().ToArray().Select(x => x.Code).ToList();

				var queryResultText = string.Format(CultureInfo.InvariantCulture, @"
EM_PK IN 
(
	SELECT EM_PK
	FROM dbo.EDIMESSAGE AS M1
	JOIN (SELECT EM_MessageNum FROM dbo.EDIMESSAGE WHERE EM_Status = 'RCV' AND EM_MessageType IN ('{0}') AND EM_ReceiveTransmit = 'RCV') AS M2 ON M2.EM_MessageNum = M1.EM_MessageNum
	WHERE EM_Status = 'SNT'	AND EM_ReceiveTransmit = 'TRX' AND	EM_ApplicationCode = 'USI' AND EM_MessageType IN ('{1}') OR (EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'OR')
)", string.Join("','", messageResTypeList), string.Join("','", messageTypeList));

				result.AddFilterAndZSQLParameterCollection(queryResultText, new ZSqlParameterCollection());
			}
			else
			{
				result.AddToFilter(EDIMessageSchema.EM_Status, value);
			}

			return result;
		}

		internal CodeDescriptionPairList GetResponseQueryMessageTypeList()
		{
			var result = new CodeDescriptionPairList();
			var queryMessageTypes = ApplicationIdentifierCodeList.GetApplicationCodesForResponseQuery();

			foreach (var messageType in queryMessageTypes)
			{
				result.AddPair(messageType.Code, messageType.Description);
			}

			return result;
		}

		internal CodeDescriptionPairList GetQueryMessageTypeList()
		{
			var result = new CodeDescriptionPairList();
			var queryMessageTypes = ApplicationIdentifierCodeList.GetApplicationCodesForQuery();

			foreach (var messageType in queryMessageTypes)
			{
				result.AddPair(messageType.Code, messageType.Description);
			}

			return result;
		}
	}
}
