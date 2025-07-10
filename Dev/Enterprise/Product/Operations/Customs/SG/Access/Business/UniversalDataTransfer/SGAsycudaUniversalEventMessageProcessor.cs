using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using MessageProcessorConstants = Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.AsycudaUniversalEventMessageProcessor.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	class SGAsycudaUniversalEventMessageProcessor
	{
		public static class Constants
		{
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
			public const string BatchDate = "BatchDate";
			public const string BatchNumber = "BatchNumber";
		}

		public SGAsycudaUniversalEventMessageProcessor(UniversalEvent universalEvent, AsycudaManifestHeader manifestHeader)
		{
			this.universalEvent = Argument.NotNull(universalEvent, "universalEvent");
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
		}

		protected readonly UniversalEvent universalEvent;
		protected readonly AsycudaManifestHeader manifestHeader;

		public void Process()
		{
			if (universalEvent.IsSGAccessMessageEvent())
			{
				ProcessCore();
			}
		}

		protected virtual void ProcessCore()
		{
			if (universalEvent.EventType.GetValueOrDefault() == AutoEvents.MessageReceivedCode)
			{
				SetCycleOrBatchToBillCountries(manifestHeader);
			}
		}

		void SetCycleOrBatchToBillCountries(AsycudaManifestHeader header)
		{
			var manifestHeaderIsImport = header.IsImport;

			var dateToSetStr = GetSpecifiedContextValue(manifestHeaderIsImport ? Constants.CycleDate : Constants.BatchDate);
			var numberToSet = GetSpecifiedContextValue(manifestHeaderIsImport ? Constants.CycleNumber : Constants.BatchNumber);

			if (!dateToSetStr.IsEmpty || !numberToSet.IsEmpty)
			{
				var eventMasterBillContext = GetSpecifiedContext(MessageProcessorConstants.MasterBill);
				var eventHouseBillContexts = GetSpecifiedSubContexts(MessageProcessorConstants.HouseBill, eventMasterBillContext);
				var eventBillNumbers = eventHouseBillContexts.Select(houseBill => houseBill.Value).Distinct().ToDictionary(number => number, number => number);

				var manifestBills = header.Bills.Cast<AsycudaBill>()
					.Where(bill => eventBillNumbers.ContainsKey(bill.ABL_BillNumber));
#if NETFRAMEWORK
				var distinctOnes = manifestBills.DistinctBy(bill => bill.ABL_BillNumber);
#else
				var distinctOnes = Enumerable.DistinctBy(manifestBills, bill => bill.ABL_BillNumber);
#endif
				var manifestSgCountries = distinctOnes.ToArray();

				SetCycleOrBatch(manifestSgCountries, dateToSetStr, numberToSet, manifestHeaderIsImport);
			}
		}

		void SetCycleOrBatch(IEnumerable<AsycudaBill> bills, ZString dateToSetStr, ZString numberToSet, ZBool manifestHeaderIsImport)
		{
			foreach (var bill in bills)
			{
				if ((ZDateTime.TryParseISO8601Date(dateToSetStr, out var dateToSet) || ZDateTime.TryParseExact(dateToSetStr, out dateToSet, "yyyyMMdd")) && !dateToSet.IsEmpty)
				{
					if (manifestHeaderIsImport)
					{
						bill.CycleDate = dateToSet;
					}
					else
					{
						bill.BatchDate = dateToSet;
					}
				}

				if (!numberToSet.IsEmpty)
				{
					if (manifestHeaderIsImport)
					{
						bill.CycleNumber = numberToSet.TrimStart('0');
					}
					else
					{
						bill.BatchNumber = numberToSet;
					}
				}
			}
		}

		protected Context GetSpecifiedContext(ZString type)
		{
			return universalEvent.ContextCollection.FirstOrDefault(x => IsContext(x, type));
		}

		protected ZString GetSpecifiedContextValue(ZString type)
		{
			return GetSpecifiedContext(type)?.Value.GetValueOrDefault() ?? ZString.Empty;
		}

		protected Context GetSpecifiedSubContext(ZString type, Context context)
		{
			return context?.SubContextCollection.FirstOrDefault(subContext => type.Equals(subContext.Type?.Type));
		}

		protected IEnumerable<Context> GetSpecifiedSubContexts(ZString type, Context context)
		{
			return context.SubContextCollection.Where(subContext => type.Equals(subContext.Type?.Type));
		}

		protected IEnumerable<Context> GetSpecifiedSubContexts(ZString type, IEnumerable<Context> contexts)
		{
			return contexts.SelectMany(context => GetSpecifiedSubContexts(type, context));
		}

		protected bool IsContext(Context context, ZString type, bool ensureNonEmptyValueOnly = true)
		{
			return type.EqualsIgnoringCase(context?.Type?.Type.GetValueOrDefault()) && (!ensureNonEmptyValueOnly || !(context?.Value).GetValueOrDefault().IsEmpty);
		}
	}
}
