using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVOriginLoadListConsolAllocator : IHVLVOriginLoadListConsolAllocator
	{
		public HVLVOriginLoadListConsolAllocator(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		HVLVOriginLoadListHelper ConsolAllocatorLegacy => consolAllocatorLegacy ??= new HVLVOriginLoadListHelper(logger);
		HVLVOriginLoadListHelper consolAllocatorLegacy;

		HVLVOriginLoadListConsolXUSAllocator ConsolAllocator => consolAllocator ??= new HVLVOriginLoadListConsolXUSAllocator(logger);
		HVLVOriginLoadListConsolXUSAllocator consolAllocator;

		public IForwardingConsol AllocatedConsol { get; private set; }

		public bool TryAttachToConsol(IForwardingConsol consol, IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage)
		{
			var succeed = false;
			Exception exceptionOnXUSProcessing = null;
			errorMessage = string.Empty;

			if (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.Value)
			{
				try
				{
					succeed = ConsolAllocator.TryAttachToConsol(consol, loadLists, out errorMessage);
				}
				catch (Exception ex)
				{
					exceptionOnXUSProcessing = ex;
					(loadLists.First().Factory as IBusinessObjectFactoryInternals).Rollback();

					if (consol is BusinessObject existingConsol)
					{
						(existingConsol.Factory as IBusinessObjectFactoryInternals).Rollback(); 
					}
				}
			}

			if (!succeed)
			{
				succeed = ConsolAllocatorLegacy.TryAttachToConsol(consol, loadLists, out errorMessage);
			}

			if (succeed && exceptionOnXUSProcessing != null)
			{
				ErrorReporter.ReportOnce("Error detected while trying to use Universal XML for load list processing, completed using direct data access instead.", exceptionOnXUSProcessing);
			}

			return succeed;
		}

		public bool TryCreateConsolAndAttachLoadLists(IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage)
		{
			var succeed = false;
			Exception exceptionOnXUSProcessing = null;
			errorMessage = string.Empty;

			if (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.Value)
			{
				try
				{
					succeed = ConsolAllocator.TryCreateConsolAndAttachLoadLists(loadLists, out errorMessage);
					AllocatedConsol = ConsolAllocator.AllocatedConsol;
				}
				catch (Exception ex)
				{
					exceptionOnXUSProcessing = ex;
					(loadLists.First().Factory as IBusinessObjectFactoryInternals).Rollback();
				}
			}

			if (!succeed)
			{
				AllocatedConsol = ConsolAllocatorLegacy.CreateConsol(loadLists.FirstOrDefault());
				succeed = consolAllocatorLegacy.TryAttachToConsol(AllocatedConsol, loadLists, out errorMessage);
			}

			if (succeed && exceptionOnXUSProcessing != null)
			{
				ErrorReporter.ReportOnce("Error detected while trying to use Universal XML for load list processing, completed using direct data access instead.", exceptionOnXUSProcessing);
			}

			return succeed;
		}

		#region Application Locks

		public bool TryAcquireApplicationLocks(IList<ZGuid> pks, Action<IEnumerable<ZGuid>> actionForLockedRows, out string errorMessage)
		{
			return pks.TryAcquireApplicationLocks<HVLVOriginLoadList>("HVLVOriginLoadListProcessingQueue", actionForLockedRows, out errorMessage);
		}

		#endregion
	}
}
