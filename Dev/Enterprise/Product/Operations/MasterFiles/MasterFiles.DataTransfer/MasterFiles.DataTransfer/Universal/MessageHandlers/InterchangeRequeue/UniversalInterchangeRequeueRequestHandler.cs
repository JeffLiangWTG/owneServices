using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalInterchangeRequeueRequestHandler : UniversalXmlMessageHandler<InterchangeRequeueRequest>
	{
		public UniversalInterchangeRequeueRequestHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalInterchangeRequeueRequestHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalInterchangeRequeueRequest;

		protected sealed override IHttpXmlProcessingResult ProcessDataObject(BusinessObjectFactory factory,
			InterchangeRequeueRequest topLevelDataObject,
			IHttpXmlEDIMessage requestEdiMessage,
			ICodeMappingManager codeMapper = null,
			IHttpXmlMessageSaver messageSaver = null)
		{
			var listOfFilterProcessors = new List<IInterchangeRequeueRequestFilterProcessor>
				{
					new DateRangeInterchangeRequeueRequestFilterProcessor(),
					new NumberRangeInterchangeRequeueRequestFilterProcessor(),
					new OrInterchangeRequeueRequestFilterProcessor(EDIInterchangeSchema.EI_ApplicationCode, InterchangeRequeueRequestFilter.FilterTypeApplicationCode),
					new OrInterchangeRequeueRequestFilterProcessor(EDIInterchangeSchema.EI_From, InterchangeRequeueRequestFilter.FilterTypeSenderCode),
					new OrInterchangeRequeueRequestFilterProcessor(EDIInterchangeSchema.EI_To, InterchangeRequeueRequestFilter.FilterTypeRecipientCode),
					new OrInterchangeRequeueRequestFilterProcessor(typeof(IEDIInterchange), EDIInterchangeSchema.EI_GB, typeof(IGlbBranch), GlbBranchSchema.GB_Code, InterchangeRequeueRequestFilter.FilterTypeBranchCode),
				};

			ZQuery messageQuery = new ZQuery();

			if (topLevelDataObject.FilterCollection == null)
			{
				var errorMessage = FormattableString.Invariant($"Please specify FilterCollection.");
				xmlSessionTracker.LogBoth(LogType.Error, errorMessage);
				return new HttpXmlProcessingResult
				{
					Status = EDIInterchangeStatusList.Codes.Error,
				};
			}

			var filterWithInvalidType = topLevelDataObject.FilterCollection.FirstOrDefault(f => !InterchangeRequeueRequestFilter.AllSupportedFilterTypes.Contains(f.Type.ToString()));

			if (filterWithInvalidType != null)
			{
				var lastType = InterchangeRequeueRequestFilter.AllSupportedFilterTypes.Last();
				var allTypesExceptTheLast = InterchangeRequeueRequestFilter.AllSupportedFilterTypes.Take(InterchangeRequeueRequestFilter.AllSupportedFilterTypes.Count() - 1);
				var errorMessage = FormattableString.Invariant($"Invalid Filter Type [{filterWithInvalidType.Type}] - Valid types are {string.Join(", ", allTypesExceptTheLast)} and {lastType}.");
				xmlSessionTracker.LogBoth(LogType.Error, errorMessage);
				return new HttpXmlProcessingResult
				{
					Status = EDIInterchangeStatusList.Codes.Error,
				};
			}

			foreach (var processor in listOfFilterProcessors)
			{
				var query = processor.Process(topLevelDataObject.FilterCollection, out string errorMessage);

				if (query == null)
				{
					xmlSessionTracker.LogBoth(LogType.Error, errorMessage);
					return new HttpXmlProcessingResult
					{
						Status = EDIInterchangeStatusList.Codes.Error,
					};
				}

				messageQuery.AddToFilter(query);
			}

			messageQuery.AddToFilter(EDIInterchangeSchema.EI_Status, "SNT");

			var filterFromZQuery = messageQuery.GetAsWhereClause(true, FormattableString.Invariant($"AND lg.{StmALogSchema.SL_Reference.Name} LIKE '%{EDIInterchangeStatusList.Codes.eAdaptorQueued}%'"));

			filterFromZQuery = filterFromZQuery.Replace("#", "'");

			var logReferenceText = EventLogReferenceBuilder.GenerateEventReferenceToFitInReferenceMaxLength("",
				new Dictionary<string, string> {
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, EDIInterchangeStatusList.Codes.eAdaptorQueued },
					{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, EDIInterchangeStatusList.Codes.Sent },
				});

			var sql = FormattableString.Invariant(
				$@"
				BEGIN TRY
					BEGIN TRANSACTION

					-- need this as [dbo].[StmALog] has triggers so can't OUTPUT directly
					DECLARE @TableVar TABLE
					(
						tableName varchar(14),
						updatedpk uniqueidentifier,
						description varchar(24),
						eventtype varchar(3),
						eventtime datetime,
						usercode varchar(max),
						branch varchar(3),
						department varchar(3)
					);

					UPDATE 
						ei
					SET
						{EDIInterchangeSchema.EI_Status.Name} = '{EDIInterchangeStatusList.Codes.eAdaptorQueued}', {EDIInterchangeSchema.EI_TransportType.Name} = '{EDIInterchangeTransportTypeList.Codes.eAdaptor}'
					OUTPUT 
						'{EDIInterchangeSchema.Constants.TableName}', DELETED.EI_PK, '{logReferenceText}','{Enterprise.ZArchitecture.Business.Events.StatusUpdated.Code}', sysdatetime(), '{Env.CurrentUser.Initials}', '{Env.CurrentBranch.Code}', '{Env.CurrentDepartment.Code}'
					INTO 
						@TableVar
					FROM 
						[dbo].[{EDIInterchangeSchema.Constants.TableName}] ei
					INNER JOIN 
						[dbo].[{StmALogSchema.Constants.TableName}] lg
					ON
						ei.{EDIInterchangeSchema.PK.Name} = lg.{StmALogSchema.SL_Parent.Name}
					{filterFromZQuery}

					INSERT INTO [dbo].[{StmALogSchema.Constants.TableName}] ({StmALogSchema.SL_Table.Name}, {StmALogSchema.SL_Parent.Name}, {StmALogSchema.SL_Reference.Name}, {StmALogSchema.SL_SE_NKEvent.Name},  {StmALogSchema.SL_EventTime.Name}, {StmALogSchema.SL_GS_NKUser.Name}, {StmALogSchema.SL_GB_NKBranch.Name}, {StmALogSchema.SL_GE_NKDepartment.Name})
					SELECT * FROM @TableVar

					SELECT Count(1) FROM @TableVar

					COMMIT TRANSACTION
				END TRY
				BEGIN CATCH
					ROLLBACK TRANSACTION;
					THROW;
				END CATCH");

			var result = Db.Connection.ExecuteScalar<int>(sql);

			if (result > 0)
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(ServiceTaskCodes.EAM);
			}

			var response = GetSuccessResponse(factory, result);
			factory.Save();

			messageSaver?.Save(response);

			return response;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		IHttpXmlProcessingResult GetSuccessResponse(BusinessObjectFactory factory, int interchangeRequeueCount)
		{
			var result = new HttpXmlProcessingResult
			{
				Status = EDIMessageStatusList.Codes.ProcessedOK
			};

			result.ResponseMessageText = factory.SubscribeForDispose(new CargoWise.IO.Shim.SubStreamableStream());

			var evt = new Event
			{
				EventType = "EDT",
				EventTime = DateTime.UtcNow,
				ContextCollection = new List<Context>
				{
					new Context
					{
						Type = "InterchangeRequeueCount",
						Value = interchangeRequeueCount.ToString(CultureInfo.InvariantCulture),
					}
				}
			};

			new XmlWriter().WriteXML(evt, result.ResponseMessageText, false);

			return result;
		}
	}
}
