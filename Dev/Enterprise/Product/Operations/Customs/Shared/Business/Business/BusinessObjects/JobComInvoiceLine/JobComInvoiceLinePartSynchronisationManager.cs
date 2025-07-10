using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public sealed class JobComInvoiceLinePartSynchronisationManager : IDataRefreshBusSubscriber
	{
		public JobComInvoiceLinePartSynchronisationManager(IInvoiceLinePartDetails invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(JobComInvoiceLinePartSynchronisationManager.invoiceLine));
			Enabled = true;
			UpdateInvoiceLineDetails = true;
		}
		readonly IInvoiceLinePartDetails invoiceLine;

		public OrgSupplierPart Part
		{
			get
			{
				if (!hasCalculatedPartBefore || ReloadPart)
				{
					if (!PartPK.IsEmpty)
					{
						fPart = (OrgSupplierPart)Factory.Load(invoiceLine.TypeOfPartUsed, PartPK);
					}
					else
					{
						fPart = null;
					}

					hasCalculatedPartBefore = true;
					ReloadPart = false;
				}
				return fPart;
			}
		}
		bool hasCalculatedPartBefore;

		public bool ReloadPart
		{
			get { return reloadPart; }
			set { reloadPart = value; }
		}
		bool reloadPart;

		public void Refresh()
		{
			if (Enabled)
			{
				hasCalculatedPartBefore = true;
				CalculatePart();
				if (fPart != null)
				{
					DataRefreshManager manager = new DataRefreshManager();
					manager.StartManaging(fPart);
				}
			}
		}

		public bool Enabled
		{
			get { return enabled && invoiceLine.Enabled; }
			set
			{
				var hasChanged = enabled != value;
				enabled = value;
				if (hasChanged || needRefreshEnabled)
				{
					needRefreshEnabled = false;
					var dictionary = GetPartSyncManagerActiveDeciderPKDictionary(Factory);
					var needToAddToDictionary = false;
					var partSyncManagerActiveDeciderPK = invoiceLine.PartSyncManagerActiveDeciderPK;
					if (!dictionary.TryGetValue(partSyncManagerActiveDeciderPK, out var hashSet))
					{
						hashSet = new HashSet<JobComInvoiceLinePartSynchronisationManager>();
						needToAddToDictionary = true;
					}

					if (enabled)
					{
						var currentPartSyncManagerActiveDeciderPK = GetCurrentPartSyncManagerActiveDeciderPK(Factory);
						if (!currentPartSyncManagerActiveDeciderPK.IsEmpty && currentPartSyncManagerActiveDeciderPK == partSyncManagerActiveDeciderPK)
						{
							GetSharedManager(Factory).StartManaging(this);
						}

						hashSet.Add(this);
						if (needToAddToDictionary)
						{
							dictionary.Add(partSyncManagerActiveDeciderPK, hashSet);
						}
					}
					else
					{
						GetSharedManager(Factory).StopManaging(this);
						if (!needToAddToDictionary)
						{
							hashSet.Remove(this);
							if (hashSet.Count == 0)
							{
								dictionary.Remove(partSyncManagerActiveDeciderPK);
							}
						}
					}
				}
			}
		}

		public void RefreshPartSyncManagerActiveDeciderPKDictionary(ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			var dictionary = GetPartSyncManagerActiveDeciderPKDictionary(Factory);
			var newPartSyncManagerActiveDeciderPK = invoiceLine.PartSyncManagerActiveDeciderPK;
			needRefreshEnabled = oldPartSyncManagerActiveDeciderPK != newPartSyncManagerActiveDeciderPK;
			if (needRefreshEnabled)
			{
				if (dictionary.TryGetValue(newPartSyncManagerActiveDeciderPK, out var hashSet))
				{
					if (!hashSet.Contains(this))
					{
						hashSet.Add(this);
					}
				}
				else
				{
					dictionary.Add(newPartSyncManagerActiveDeciderPK, new HashSet<JobComInvoiceLinePartSynchronisationManager>() { this });
				}

				if (dictionary.TryGetValue(oldPartSyncManagerActiveDeciderPK, out hashSet))
				{
					hashSet.Remove(this);
					if (hashSet.Count == 0)
					{
						dictionary.Remove(oldPartSyncManagerActiveDeciderPK);
					}
				}
			}
		}
		bool needRefreshEnabled;

		public bool UpdateInvoiceLineDetails;

		public static void SetCurrentPartSyncManagerActiveDeciderPK(BusinessObjectFactory factory, ZGuid activeDeciderPK)
		{
			if (factory != null)
			{
				var currentPartSyncManagerActiveDeciderPK = GetCurrentPartSyncManagerActiveDeciderPK(factory);
				if (currentPartSyncManagerActiveDeciderPK != activeDeciderPK)
				{
					var dictionary = GetPartSyncManagerActiveDeciderPKDictionary(factory);
					HashSet<JobComInvoiceLinePartSynchronisationManager> hashSet = null;
					if (currentPartSyncManagerActiveDeciderPK.IsValid && dictionary.TryGetValue(currentPartSyncManagerActiveDeciderPK, out hashSet))
					{
						var sharedManager = GetSharedManager(factory);
						hashSet.ForEach(x => sharedManager.StopManaging(x));
					}
					factory.ClearCachedValue<ZGuid>(CurrentPartSyncManagerActiveDeciderPKKey);
					if (activeDeciderPK.IsValid)
					{
						GetCurrentPartSyncManagerActiveDeciderPK(factory, activeDeciderPK);
						if (dictionary.TryGetValue(activeDeciderPK, out hashSet))
						{
							var sharedManager = GetSharedManager(factory);
							hashSet.ForEach(x => sharedManager.StartManaging(x));
						}
					}
				}
			}
		}

		public static void StopManagingWhenActiveDeciderPKWasDisposed(BusinessObjectFactory factory, ZGuid activeDeciderPK)
		{
			if (factory != null)
			{
				var dictionary = GetPartSyncManagerActiveDeciderPKDictionary(factory);
				HashSet<JobComInvoiceLinePartSynchronisationManager> hashSet = null;
				if (dictionary.TryGetValue(activeDeciderPK, out hashSet))
				{
					var sharedManager = GetSharedManager(factory);
					hashSet.ForEach(x => sharedManager.StopManaging(x));
					dictionary.Remove(activeDeciderPK);
				}
			}
		}

		static SharedPartSynchronisationManager GetSharedManager(BusinessObjectFactory factory)
		{
			var serviceContainer = factory.ServiceContainer;
			var sharedManager = serviceContainer.GetService<SharedPartSynchronisationManager>();
			if (sharedManager == null)
			{
				sharedManager = new SharedPartSynchronisationManager(factory);
				serviceContainer.AddService(sharedManager);
			}
			return sharedManager;
		}

		internal static Dictionary<ZGuid, HashSet<JobComInvoiceLinePartSynchronisationManager>> GetPartSyncManagerActiveDeciderPKDictionary(BusinessObjectFactory factory) => factory.GetCachedValue("PartSyncManagerActiveDeciderPKDictionary", () => new Dictionary<ZGuid, HashSet<JobComInvoiceLinePartSynchronisationManager>>());

		static ZGuid GetCurrentPartSyncManagerActiveDeciderPK(BusinessObjectFactory factory) => GetCurrentPartSyncManagerActiveDeciderPK(factory, ZGuid.Empty);

		static ZGuid GetCurrentPartSyncManagerActiveDeciderPK(BusinessObjectFactory factory, ZGuid activeDeciderPK) => factory.GetCachedValue(CurrentPartSyncManagerActiveDeciderPKKey, () => activeDeciderPK);
		const string CurrentPartSyncManagerActiveDeciderPKKey = "CurrentPartSyncManagerActiveDeciderPK";

#if DEBUG
		internal
#endif
		void CalculatePart()
		{
			CalculatePart(invoiceLine.JustUpdatedByDataRefresh);
		}

		void CalculatePart(bool justUpdatedByDataRefresh)
		{
			ProductLoadResult loadResult = LoadResults();
			fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
			OrgSupplierPart newPart = (OrgSupplierPart)loadResult.BestMatchingProduct;
			fTotalMatchCount = loadResult.TotalMatchCount;
			var hasPartChanged = newPart != fPart;
			var wasPartPreviouslySet = fPart != null;

			if (hasPartChanged)
			{
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh -= new OrgSupplierPart.DeletedHandler(fPart_Deleted);
				}

				fPart = newPart;
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh += new OrgSupplierPart.DeletedHandler(fPart_Deleted);
				}
			}

			if ((ShouldUpdatePartDetails(invoiceLine) || (hasPartChanged && (wasPartPreviouslySet || !((invoiceLine as BaseJobComInvoiceLine)?.IsInDatabase ?? false)))) && UpdateInvoiceLineDetails && !invoiceLine.IsDeleted)
			{
				var country = invoiceLine.InvoiceCountry;
				if (country != null)
				{
					try
					{
						if (fPart != null)
						{
							fPart.JustUpdatedByDataRefresh = justUpdatedByDataRefresh;
						}

						invoiceLine.UpdateDetailsOnPartChange();
					}
					finally
					{
						if (fPart != null)
						{
							fPart.JustUpdatedByDataRefresh = false;
						}
					}
				}
			}
		}

		bool ShouldUpdatePartDetails(IInvoiceLinePartDetails invoiceLine) => (invoiceLine.Header?.JobDeclaration?.ForcePartRefreshFromUI ?? true) || CustomsDataRegistry.Instance.EnableAutoRefreshProductData.Value;

		public void OnlySetPartPK()
		{
			if (Enabled)
			{
				ProductLoadResult loadResult = LoadResults();
				fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
				OrgSupplierPart newPart = (OrgSupplierPart)loadResult.BestMatchingProduct;

				if (newPart != null && UpdateInvoiceLineDetails && !invoiceLine.IsDeleted)
				{
					invoiceLine.PartPK = newPart.PK;
				}
			}
		}

		public void ClearPart()
		{
			fPart = null;
		}

		ProductLoadResult LoadResults()
		{
			return LoadResults(Factory, invoiceLine.TypeOfPartUsed, PartNo, invoiceLine.Importer, invoiceLine.Supplier, invoiceLine.IsDrawback, invoiceLine.IsForExportSectionOfDrawback, invoiceLine.IsForImportSectionOfDrawback, invoiceLine.Header?.IsExport ?? ZBool.False);
		}

		public static ProductLoadResult LoadResults(BusinessObjectFactory factory, Type typeOfPartUsed, ZString partNo, OrgHeader importer, OrgHeader supplier, bool isDrawback, bool isForExportSectionOfDrawback, bool isForImportSectionOfDrawback, bool isExportJob)
		{
			bool includeLocalPartInSearch = false;
			if (isDrawback)
			{
				if (isForExportSectionOfDrawback && isForImportSectionOfDrawback)
				{
					return LoadResultsForDuelImportExportDrawbackLine(factory, typeOfPartUsed, partNo, importer, isExportJob);
				}
				else if (isForImportSectionOfDrawback)
				{
					supplier = null;
				}
				else if (isForExportSectionOfDrawback)
				{
					supplier = importer;
					importer = null;
				}
				else
				{
					includeLocalPartInSearch = true;
				}
			}
			return new OrgSupplierPart.Loader(factory, typeOfPartUsed).LoadAndReturnMatchingCount(partNo, importer, supplier, includeLocalPartInSearch, isExportJob: isExportJob);
		}

		static ProductLoadResult LoadResultsForDuelImportExportDrawbackLine(BusinessObjectFactory factory, Type typeOfPartUsed, ZString partNo, OrgHeader importer, bool isExportJob)
		{
			var result = new OrgSupplierPart.Loader(factory, typeOfPartUsed).LoadAndReturnMatchingCount(partNo, importer, null, true, isExportJob: isExportJob);
			var newPart = (OrgSupplierPart)result.BestMatchingProduct;
			if (newPart == null || newPart.OP_PartNum == partNo)
			{
				var newResult = new OrgSupplierPart.Loader(factory, typeOfPartUsed).LoadAndReturnMatchingCount(partNo, null, importer, false, isExportJob: isExportJob);
				if (newResult.BestMatchingProduct != null)
				{
					result = newResult;
				}

				if (result.BestMatchingProduct != null)
				{
					((OrgSupplierPart)result.BestMatchingProduct).LocalPart = null;
				}
			}
			else
			{
				var newResult = new OrgSupplierPart.Loader(factory, typeOfPartUsed).LoadAndReturnMatchingCount(partNo, null, importer, false, isExportJob: isExportJob);
				if (result.BestMatchingProduct != null)
				{
					((OrgSupplierPart)result.BestMatchingProduct).LocalPart = (OrgSupplierPart)newResult.BestMatchingProduct;
				}
			}
			return result;
		}

		void fPart_Deleted(OrgSupplierPart part)
		{
			if (part == fPart && !invoiceLine.IsDeleted)
			{
				CalculatePart();
				invoiceLine.PartPK = ZGuid.Empty;
			}
		}

		#region Implementation

#if DEBUG
		internal
#endif
		ZGuid PartPK
		{
			get { return !invoiceLine.IsDeleted ? invoiceLine.PartPK : ZGuid.Empty; }
		}

		BusinessObjectFactory IDataRefreshBusSubscriber.Factory
		{
			get { return Factory; }
		}

		void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			foreach (IPartProvider publishedObject in publishedObjects)
			{
				var part = publishedObject.Part;
				if (part.PK == PartPK || part.OP_PartNum == PartNo)
				{
					CalculatePart(true);
				}
			}
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		#endregion

		BusinessObjectFactory Factory
		{
			get { return invoiceLine.Factory; }
		}

		ZString PartNo
		{
			get { return !invoiceLine.IsDeleted ? invoiceLine.PartNo : ZString.Empty; }
		}

		public int TotalMatchCount
		{
			get { return fTotalMatchCount; }
		}
		int fTotalMatchCount;

		public int TotalNumberOfPartsCount
		{
			get { return fTotalNumberOfPartsCount; }
		}
		int fTotalNumberOfPartsCount;

		OrgSupplierPart fPart;
		bool enabled;
	}
}
