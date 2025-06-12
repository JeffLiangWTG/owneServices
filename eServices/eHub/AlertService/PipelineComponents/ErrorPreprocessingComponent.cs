using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Common.Logging;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.AlertService.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("115D9707-4BCE-49C4-B3AC-502494472B24")]
	public class ErrorPreprocessingComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Preprocess Error Message Before AlertService Orchestration"; }
		}

		public string Name
		{
			get { return "Error Preprocessing"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message);
			LoggerHelpers.LogComponentStart(logger, this);

            if (ShouldBeIgnored(message, logger)) return null;

			int retryCount = 0;
			var retry = true;
			var skipSendAlert = false;
			while (retryCount <= ExceptionRetryCount && retry)
			{
				try
				{
					logger.Debug("Resolving Routing Rules.");
					skipSendAlert = SkipSendAlert(message, logger);
					retry = false;
				}
				catch (SqlException e)
				{
					logger.Debug("Retry on SqlException: " + e);
					retryCount++;
					Thread.Sleep(ExceptionRetryIntervalSenconds);
				}
				catch (Exception ex)
				{
					logger.Debug("Caught Exception: " + ex);
				}
			}
			message.Context.WriteProperty<CargoWise.eHub.AlertService.PropertySchemas.SkipSendAlert>(skipSendAlert.ToString());
			LoggerHelpers.LogComponentEnd(logger, this);

			return message;
		}

		#endregion

		#region Implementation

		bool SkipSendAlert(IBaseMessage message, ILog logger)
		{

			var dbContext = GetDBContext();
			var client = dbContext.eHubClients.Include("eHubRoutingRule").Where(c => c.CC_ID == "SENDALERT").FirstOrDefault();
			if (client == null)
			{
				return false;
			}
			var results = GetRoutingRuleEvaluate(dbContext, logger, message, client);
			var result = results.Count == 1 && results.First().Value == "SKIP";

			return result;
		}

        bool ShouldBeIgnored(IBaseMessage message, ILog logger)
        {
            var isAs2Payload = message.Context.ReadPropertyString<EdiIntAS.IsAS2PayloadMessage>();
            if (isAs2Payload != null && !Convert.ToBoolean(isAs2Payload))
            {
                logger.InfoFormat("AS2 MDN messages will not be processed. MessageID: {0}.", message.MessageID);
                return true;
            }

            var isAck = message.Context.ReadPropertyString<EDI.IsSystemGeneratedAck>();
            if (isAck != null && Convert.ToBoolean(isAck))
            {
                logger.InfoFormat("System-generated Ack messages will not be processed. MessageID: {0}.", message.MessageID);
                return true;
            }

            return false;
        }

		public virtual eHubTransactionsContext GetDBContext()
		{
			Database.SetInitializer<eHubTransactionsContext>(null);
			return new eHubTransactionsContext();
		}

		public virtual Collection<Result> GetRoutingRuleEvaluate(eHubTransactionsContext context, ILog logger, IBaseMessage message, eHubClient client)
		{
			var ruleFactory = new RoutingRuleFactory(context, logger);
			var routingRule = ruleFactory.GetForReading(client);
			var factResolver = new RoutingRuleMessageFactResolver(message);
			var sqlResolver = new RoutingRuleSqlFactResolver(context);
			return routingRule.Evaluate(context, new IFactResolver[] { factResolver, sqlResolver }, logger);
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("115D9707-4BCE-49C4-B3AC-502494472B24");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("ExceptionRetryCount", out var, errorLog); }
			catch { }
			if (var != null) ExceptionRetryCount = (int)var;

			var = null;
			try { propertyBag.Read("ExceptionRetryInterval", out var, errorLog); }
			catch { }
			if (var != null) ExceptionRetryIntervalSenconds = (int)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = ExceptionRetryCount;
			propertyBag.Write("ExceptionRetryCount", ref val);

			val = ExceptionRetryIntervalSenconds;
			propertyBag.Write("ExceptionRetryIntervalSenconds", ref val);
		}

		#endregion

		#region Properties

		public int ExceptionRetryCount { get; set; }

		public int ExceptionRetryIntervalSenconds { get; set; }

		#endregion
	}
}
