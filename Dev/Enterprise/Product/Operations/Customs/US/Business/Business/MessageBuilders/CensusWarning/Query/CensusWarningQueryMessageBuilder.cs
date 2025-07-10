using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class CensusWarningQueryMessageBuilder
	{
		public CensusWarningQueryMessageBuilder(IACECensusWarningQuery queryData)
		{
			this.queryData = queryData;
		}

		readonly IACECensusWarningQuery queryData;

		public MQEDIMessage PopulateMessage()
		{
			var processingPortCode = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(queryData.Branch);
			var block = new ACEInputBlockControlGenerator(queryData.CompanyPK, processingPortCode, queryData.ProcessingOfficeCode);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery;
			block.MessageBlocks.Add(CreateCJ1());

			MQEDIMessage message = block.CreateMessage<MQEDIMessage>(queryData.Factory);
			if (queryData.Messages != null)
			{
				queryData.Messages.Add(message);
			}
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CensusWarningQuery;
			return message;
		}

		MessageBlock CreateCJ1()
		{
			var result = new ACWQCJ1();
			if (queryData.EntryFilerCodesAndNumbers.Any())
			{
				var entryFilerCodesAndNumber = queryData.EntryFilerCodesAndNumbers.FirstOrDefault();
				result.EntryFilerCode = entryFilerCodesAndNumber.Code;
				if (!entryFilerCodesAndNumber.Number.IsEmpty)
				{
					result.EntryNumber1 = entryFilerCodesAndNumber.Number;
				}
			}

			if (!queryData.DateFrom.IsEmpty)
			{
				result.RequestedFromDate = queryData.DateFrom.Date;
				result.RequestedToDate = queryData.DateTo.Date;
			}

			if (!queryData.DistrictPortOfEntry.IsEmpty)
			{
				result.DistrictPortOfEntry = queryData.DistrictPortOfEntry;
			}

			return result;
		}
	}
}
