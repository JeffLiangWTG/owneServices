using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace CargoWise.RefDataRepo.Ent.Client.NudgeServiceTask
{
	public class RENInboundInterchangeProcessor : BaseInboundInterchangeProcessor
	{
		readonly INudgeUpdaterManagerWrapper dataSetUpdaterWrapper;
		public RENInboundInterchangeProcessor(INudgeUpdaterManagerWrapper dataSetUpdaterWrapper, LoggingInformation logger)
			: base(logger)
		{
			this.dataSetUpdaterWrapper = dataSetUpdaterWrapper;
		}
		protected override string[] ApplicationCodes => new[] { EDIInterchangeTypeList.Codes.GenericMessageDelivery };

		protected override bool IsNoBranchFilter => true;
		protected override bool SupportEnvironmentSwitch => true;

		protected override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, "REN");
			base.AddTypeFilter(query);
		}

		IEnumerable<RefDataRepoNudgeDataSet> RunUpdaterAndReturnFailedRecords(IEnumerable<RefDataRepoNudgeDataSet> dataSetsToNudge)
		{
			var result = new List<RefDataRepoNudgeDataSet>();
			foreach (var dataSet in dataSetsToNudge)
			{
				if (dataSet.DataSetName == null || dataSet.TableName == null)
				{
					result.Add(dataSet);
					continue;
				}
				var updateResult = dataSetUpdaterWrapper.Update(dataSet.TableName, dataSet.DataSetName);
				if (!updateResult)
				{
					result.Add(dataSet);
				}
			}
			return result;
		}

		IEnumerable<RefDataRepoNudgeDataSet> GetDataSetsFromInterchangeBody(string interchangeBodyText)
		{
			var xDoc = XDocument.Parse(interchangeBodyText);
			var root = xDoc?.Root;
			if (root != null)
			{
				var dataSetsElement = root.Elements().FirstOrDefault(x => x.Name.LocalName == ApplicationConfiguration.InterchangeBodyMessage.DataSetsElement);
				if (dataSetsElement != null && dataSetsElement.HasElements)
				{
					var dsElements = dataSetsElement.Elements().Where(x => x.Name.LocalName == ApplicationConfiguration.InterchangeBodyMessage.DataSetElement);
					foreach (var dsRecord in dsElements)
					{
						var dataSetId = dsRecord.Elements().FirstOrDefault(x => x.Name.LocalName == ApplicationConfiguration.InterchangeBodyMessage.DataSetIdElement)?.Value;
						var tableName = dsRecord.Elements().FirstOrDefault(x => x.Name.LocalName == ApplicationConfiguration.InterchangeBodyMessage.TableNameElement)?.Value;
						var timeStamp = dsRecord.Elements().FirstOrDefault(x => x.Name.LocalName == ApplicationConfiguration.InterchangeBodyMessage.TimeStampElement)?.Value;
						yield return new RefDataRepoNudgeDataSet(dataSetId, tableName, Convert.ToDateTime(timeStamp));
					}
				}
			}
		}

		protected override bool ProcessInterchange(EDIInterchange interchange)
		{
			var dataSetsToNudge = GetDataSetsFromInterchangeBody(interchange.EI_BodyText);

			var updateCount = 0;
			var failedDatasets = RunUpdaterAndReturnFailedRecords(dataSetsToNudge);
			while (updateCount < 3 && failedDatasets.Any())
			{
				failedDatasets = RunUpdaterAndReturnFailedRecords(failedDatasets);
				updateCount++;
			}
			return !failedDatasets.Any();
		}
	}
}
