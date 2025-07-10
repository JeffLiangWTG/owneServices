using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.InBond.Business
{
	public sealed class CusInBondCargoDescPartSynchronisationManager : IDataRefreshBusSubscriber
	{
		public CusInBondCargoDescPartSynchronisationManager(CusInBondCargoDesc commodity)
		{
			this.commodity = Argument.NotNull(commodity, "commodity");
			Enabled = true;
		}
		readonly CusInBondCargoDesc commodity;

		public OrgSupplierPart Part
		{
			get
			{
				if (!hasCalculatedPartBefore || ReloadPart)
				{
					if (!PartPK.IsEmpty)
					{
						fPart = Factory.Load<OrgSupplierPart>(PartPK);
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
			get { return enabled; }
			set
			{
				bool hasChanged = enabled != value;
				enabled = value;
				if (hasChanged)
				{
					var serviceContainer = commodity.Factory.ServiceContainer;
					var sharedManager = serviceContainer.GetService<Customs.Business.SharedPartSynchronisationManager>();
					if (sharedManager == null)
					{
						sharedManager = new Customs.Business.SharedPartSynchronisationManager(Factory);
						serviceContainer.AddService(sharedManager);
					}
					if (enabled)
					{
						sharedManager.StartManaging(this);
					}
					else
					{
						sharedManager.StopManaging(this);
					}
				}
			}
		}

		internal void CalculatePart()
		{
			CalculatePart(false);
		}

		void CalculatePart(bool justUpdatedByDataRefresh)
		{
			ProductLoadResult loadResult = LoadResults();
			fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
			OrgSupplierPart newPart = (OrgSupplierPart)loadResult.BestMatchingProduct;
			fTotalMatchCount = loadResult.TotalMatchCount;

			if (newPart != fPart)
			{
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh -= fPart_Deleted;
				}

				fPart = newPart;
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh += fPart_Deleted;
				}
			}

			if (!commodity.IsDeleted)
			{
				try
				{
					if (fPart != null)
					{
						fPart.JustUpdatedByDataRefresh = justUpdatedByDataRefresh;
					}

					commodity.UpdateDetailsOnPartChange();
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

		public void OnlySetBY_OP_Part()
		{
			var loadResult = LoadResults();
			fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
			var newPart = (OrgSupplierPart)loadResult.BestMatchingProduct;

			if (newPart != null && !commodity.IsDeleted)
			{
				commodity.BY_OP_Part = newPart.PK;
			}
		}

		ProductLoadResult LoadResults()
		{
			OrgHeader importer = commodity.Importer;
			OrgHeader supplier = commodity.Supplier;
			return new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).LoadAndReturnMatchingCount(PartNo, importer, supplier, false);
		}

		void fPart_Deleted(Customs.Business.OrgSupplierPart part)
		{
			if (part == fPart && !commodity.IsDeleted)
			{
				CalculatePart();
				commodity.BY_OP_Part = ZGuid.Empty;
			}
		}

		#region Implementation

		ZGuid PartPK
		{
			get { return !commodity.IsDeleted ? commodity.BY_OP_Part : ZGuid.Empty; }
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
			get { return commodity.Factory; }
		}

		ZString PartNo
		{
			get { return !commodity.IsDeleted ? commodity.BY_PartNumber : ZString.Empty; }
		}

		internal int TotalMatchCount
		{
			get { return fTotalMatchCount; }
		}
		int fTotalMatchCount;

		internal int TotalNumberOfPartsCount
		{
			get { return fTotalNumberOfPartsCount; }
		}
		int fTotalNumberOfPartsCount;

		OrgSupplierPart fPart;
		bool enabled;
	}
}
