using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFLinePartSynchronisationManager : IDataRefreshBusSubscriber
	{
		public CusISFLinePartSynchronisationManager(CusISFLine line)
		{
			this.line = line;
			Enabled = true;
			updateLineDetails = true;
		}

		public US.Business.OrgSupplierPart Part
		{
			get
			{
				if (!hasCalculatedPartBefore || ReloadPart)
				{
					if (!PartPK.IsEmpty)
					{
						fPart = Factory.Load<US.Business.OrgSupplierPart>(PartPK);
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
			get;
			set;
		}

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
					var serviceContainer = line.Factory.ServiceContainer;
					SharedPartSynchronisationManager sharedManager = serviceContainer.GetService<SharedPartSynchronisationManager>();
					if (sharedManager == null)
					{
						sharedManager = new SharedPartSynchronisationManager(Factory);
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

		public void OnlySetBL_OP()
		{
			ProductLoadResult loadResult = LoadResults();
			fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
			US.Business.OrgSupplierPart newPart = (US.Business.OrgSupplierPart)loadResult.BestMatchingProduct;

			if (newPart != null && updateLineDetails && line != null && !line.IsDeleted)
			{
				line.BL_OP = newPart.PK;
			}
		}

		#region Are Classification Details Being Updated?

		public bool AreClassificationDetailsBeingUpdated
		{
			get { return classificationDetailsUpdateIndex > 0; }
		}
		int classificationDetailsUpdateIndex;

		IDisposable MarkThatClassificationsAreBeingUpdated()
		{
			return new ClassificationDetailsUpdaterNotifier(this);
		}

		class ClassificationDetailsUpdaterNotifier : IDisposable
		{
			public ClassificationDetailsUpdaterNotifier(CusISFLinePartSynchronisationManager manager)
			{
				this.manager = manager;
				manager.classificationDetailsUpdateIndex++;
			}

			readonly CusISFLinePartSynchronisationManager manager;

			public void Dispose()
			{
				manager.classificationDetailsUpdateIndex--;
			}
		}

		#endregion

		ZGuid previousPivotPK = ZGuid.Invalid;

		void UpdateDetailsOnPartChange()
		{
			if (!AreClassificationDetailsBeingUpdated && line.BL_TextProductCode_CanBeSetByCustomer)
			{
				using (MarkThatClassificationsAreBeingUpdated())
				{
					var product = line.USSupplierPart;

					if (product == null || product.IsDeleted)
					{
						line.DeleteProductLines();
						line.BL_OP = ZGuid.Empty;
						previousPivotPK = ZGuid.Invalid;
					}
					else
					{
						var importPivot = line.Pivot;
						var newPivotPK = importPivot != null ? importPivot.PK : ZGuid.Empty;
						if (product.JustUpdatedByDataRefresh || newPivotPK != previousPivotPK)
						{
							line.DeleteProductLines();
							line.BL_OP = product.PK;

							if (importPivot != null)
							{
								var childPivots = importPivot.Children.Cast<CusClassPartPivot>().OrderBy(x => x.CI_ChildListOrder).ToList();
								if (importPivot.TariffNumber.IsEmpty)
								{
									importPivot = childPivots.FirstOrDefault(x => !x.TariffNumber.IsEmpty);
								}

								if (importPivot != null)
								{
									UpdateDetailsFromPivot(importPivot, line);
									childPivots.Remove(importPivot);

									foreach (var pivot in childPivots)
									{
										CusISFLine newChild = null;

										if (pivot.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT)
										{
											newChild = line.AddChildLine();
											UpdateManufacturerDocAddressForChildLine(newChild);
										}
										else if (pivot.CI_ChildType == ClassificationChildTypeList.Codes.Related)
										{
											newChild = line.AddProductRelatedLine();
											UpdateManufacturerDocAddressForChildLine(newChild);
										}
										if (newChild != null)
										{
											UpdateDetailsFromPivot(pivot, newChild);
										}
									}
								}
							}
						}
						previousPivotPK = newPivotPK;
					}
					line.Validation.ValidateBL_TextProductCode();
				}
			}
		}

		void UpdateManufacturerDocAddressForChildLine(CusISFLine childLine)
		{
			var parentLine = childLine.ParentTariffLine;
			if (parentLine != null && parentLine.ManufacturerDocAddress != null)
			{
				childLine.BL_ManufacturerDocAddressPK = childLine.ParentTariffLine.ManufacturerDocAddress.PK;
			}
		}

		void UpdateDetailsFromPivot(CusClassPartPivot pivot, CusISFLine line)
		{
			line.BL_HarmonisedNum = pivot.TariffNumber.Left(CusISFLine.Schema.BL_HarmonisedNumMaxLength);

			if (pivot.CD_UC_NKCountryOfOrigin.IsEmpty)
			{
				line.SetCountryOfOriginFromManufacturer();
			}
			else
			{
				line.BL_RN_NKGoodsOrigin = pivot.CD_UC_NKCountryOfOrigin;
			}
		}

		readonly bool updateLineDetails;

		void CalculatePart()
		{
			CalculatePart(false);
		}

		void CalculatePart(bool justUpdatedByDataRefresh)
		{
			ProductLoadResult loadResult = LoadResults();
			fTotalNumberOfPartsCount = loadResult.TotalNumerOfPartsCount;
			US.Business.OrgSupplierPart newPart = (US.Business.OrgSupplierPart)loadResult.BestMatchingProduct;
			fTotalMatchCount = loadResult.TotalMatchCount;

			if (newPart != fPart)
			{
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh -= new Customs.Business.OrgSupplierPart.DeletedHandler(fPart_Deleted);
				}

				fPart = newPart;
				if (fPart != null)
				{
					fPart.DeletedByDataRefresh += new Customs.Business.OrgSupplierPart.DeletedHandler(fPart_Deleted);
				}
			}

			if (updateLineDetails && !line.IsDeleted)
			{
				try
				{
					if (fPart != null)
					{
						fPart.JustUpdatedByDataRefresh = justUpdatedByDataRefresh;
					}
					UpdateDetailsOnPartChange();
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

		ProductLoadResult LoadResults()
		{
			bool includeLocalPartInSearch = false;
			OrgHeader importer = Importer;
			ProductLoadResult result = new ProductLoadResult();
			var loader = new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(US.Business.OrgSupplierPart));
			if (line != null && !line.IsDeleted)
			{
				result = loader.LoadAndReturnMatchingCount(PartNo, importer, line.SellingParty, includeLocalPartInSearch);
			}
			return result;
		}

		void fPart_Deleted(Customs.Business.OrgSupplierPart part)
		{
			if (part == fPart && !line.IsDeleted)
			{
				CalculatePart();
				line.BL_OP = ZGuid.Empty;
			}
		}

		#region Implementation

		ZGuid PartPK
		{
			get { return !line.IsDeleted ? line.BL_OP : ZGuid.Empty; }
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
			get { return line.Factory; }
		}

		OrgHeader Importer
		{
			get { return (line.IsDeleted || line.Header == null || line.Header.IsDeleted) ? null : line.Header.Importer; }
		}

		ZString PartNo
		{
			get { return !line.IsDeleted ? line.BL_TextProductCode : ZString.Empty; }
		}

		US.Business.OrgSupplierPart fPart;

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

		readonly CusISFLine line;
		bool enabled;
	}
}
