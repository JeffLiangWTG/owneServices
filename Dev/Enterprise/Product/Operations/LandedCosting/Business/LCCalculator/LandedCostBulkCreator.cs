using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.LandedCosting.Business
{
	class LandedCostBulkCreator : ILandedCostBulkCreator
	{
		/// <summary>
		/// Run Landed Costing on the passed ILandedCostHeader
		/// </summary>
		/// <returns>the list of job numbers of Hosts for which Landed Costing could not be run</returns>
		public IEnumerable<string> CreateAndRunLC(IEnumerable<ILandedCostHeader> hosts)
		{
			List<string> result = new List<string>();
			List<ZGlobalMutex> mutexList = new List<ZGlobalMutex>();

			BusinessObjectFactory factory = new BusinessObjectFactory();

			int totalCount = hosts.Count();
			decimal count = 0;

			foreach (ILandedCostHeader host in hosts)
			{
				LandedCostHeader lcHeader = null;

				if (host.IsLCSupported)
				{
					lcHeader = factory.LoadTop1<LandedCostHeader>(new LandedCostHeaderFilter(host));

					if (lcHeader == null)
					{
						ZGlobalMutex mutex = host.GetLandedCostMutex();

						if (!mutex.IsLocked && mutex.Lock())
						{
							lcHeader = factory.New<LandedCostHeader>();
							lcHeader.SynchroniseAll(host);

							mutexList.Add(mutex);
						}
					}

					if (lcHeader != null)
					{
						lcHeader.RunPreSaveValidation();

						if (!lcHeader.HasErrors)
						{
							new LCDistributionManager(lcHeader).RunLandedCosting();
						}
					}
				}

				if (lcHeader == null || lcHeader.Histories.Count == 0)
				{
					result.Add(host.UniqueReferenceNumber);
				}

				if (OnLCProgressChanged != null)
				{
					count++;

					int percentage = (int)((count / totalCount) * 100);
					OnLCProgressChanged(percentage);
				}
			}

			try
			{
				factory.Save();

				mutexList.ForEach(x => x.Unlock());
			}
			catch (ZSaveException e)
			{
				result.Clear();

				ZExceptionReporting.HandleSaveException(e);
			}

			return result;
		}

		public event LCProgressEventHandler OnLCProgressChanged;
	}
}
