using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEEntrySummaryQueryMessageBuilder
	{
		public ACEEntrySummaryQueryMessageBuilder(IEntrySummaryQueryMessageAttachee queryData)
		{
			this.queryData = queryData;
		}
		readonly IEntrySummaryQueryMessageAttachee queryData;

		public MQEDIMessage PopulateMessage()
		{
			var isNewEntrySummaryQueryEffective = ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective;
			var processingPort = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(queryData.Branch);
			var generator = new ACEInputBlockControlGenerator(queryData.CompanyPK, processingPort, queryData.ProcessingOfficeCode);
			generator.B.ApplicationIdentifierCode = isNewEntrySummaryQueryEffective ?
				ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery : ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery;

			if (queryData.EntryFilerCodesAndNumbers.Any(x => !x.Number.IsEmpty))
			{
				var aenqj1List = new List<AENQJ1>();
				AENQJ1 aenqj1 = null;
				foreach (var entry in queryData.EntryFilerCodesAndNumbers)
				{
					if (aenqj1 == null || !aenqj1.EntryNumber5.IsEmpty)
					{
						aenqj1 = new AENQJ1();
						aenqj1List.Add(aenqj1);
					}

					if (aenqj1.EntryNumber1.IsEmpty)
					{
						aenqj1.EntryFilerCode1 = entry.Code;
						aenqj1.EntryNumber1 = entry.Number.SubstringSafe(0, 8);
					}
					else if (aenqj1.EntryNumber2.IsEmpty)
					{
						aenqj1.EntryFilerCode2 = entry.Code;
						aenqj1.EntryNumber2 = entry.Number.SubstringSafe(0, 8);
					}
					else if (aenqj1.EntryNumber3.IsEmpty)
					{
						aenqj1.EntryFilerCode3 = entry.Code;
						aenqj1.EntryNumber3 = entry.Number.SubstringSafe(0, 8);
					}
					else if (aenqj1.EntryNumber4.IsEmpty)
					{
						aenqj1.EntryFilerCode4 = entry.Code;
						aenqj1.EntryNumber4 = entry.Number.SubstringSafe(0, 8);
					}
					else
					{
						aenqj1.EntryFilerCode5 = entry.Code;
						aenqj1.EntryNumber5 = entry.Number.SubstringSafe(0, 8);
					}
				}

				generator.MessageBlocks.AddRange(aenqj1List);
			}
			else if (!queryData.CriteriaCode.IsEmpty)
			{
				var aenqj2 = new AENQJ2();
				aenqj2.CriteriaQueryTypeCode = queryData.CriteriaCode;
				aenqj2.RequestedFromDateTime = GetDateTimeStringFormat(queryData.DateFrom);
				aenqj2.RequestedToDateTime = GetDateTimeStringFormat(queryData.DateTo);
				aenqj2.ConsumptionEntrySummariesFlag = queryData.ConsumptionEntrySummaries ? "Y" : "";
				aenqj2.FTAReconSummariesFlag = queryData.FTAReconSummaries ? "Y" : "";
				aenqj2.OtherReconSummariesFlag = queryData.OtherReconSummaries ? "Y" : "";
				aenqj2.DrawbackSummariesFlag = queryData.DrawbackSummaries ? "Y" : "";
				aenqj2.NAFTADutyDeferralSummariesFlag = queryData.NAFTADutyDeferralSummaries ? "Y" : "";
				if (!queryData.CollectionBillInformationCode.IsEmpty)
				{
					aenqj2.CollectionBillInformationCode = queryData.CollectionBillInformationCode;
				}
				generator.MessageBlocks.Add(aenqj2);
			}

			var message = generator.CreateMessage<MQEDIMessage>(queryData.Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
			if (queryData.Messages != null)
			{
				queryData.Messages.Add(message);
			}

			if (isNewEntrySummaryQueryEffective)
			{
				message.EM_MessageOwner = Constants.ACE;
			}

			return message;
		}

		ZString GetDateTimeStringFormat(ZDateTime dateTime)
		{
			return dateTime.IsValid ? (ZString)dateTime.ToString("MMddyyhhmmsstt", CultureInfo.CurrentCulture) : ZString.Empty;
		}
	}
}
