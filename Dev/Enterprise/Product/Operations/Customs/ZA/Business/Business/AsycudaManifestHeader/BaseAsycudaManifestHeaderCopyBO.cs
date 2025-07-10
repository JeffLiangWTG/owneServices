using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using BaseAsycudaBill = Enterprise.Customs.ManifestBase.AsycudaBill;
using BaseAsycudaContainer = Enterprise.Customs.ManifestBase.AsycudaContainer;
using BaseAsycudaManifestHeader = Enterprise.Customs.ManifestBase.AsycudaManifestHeader;
using BaseAsycudaPack = Enterprise.Customs.ManifestBase.AsycudaPack;
using BaseAsycudaPackageContainerLink = Enterprise.Customs.ManifestBase.AsycudaContainerBillOrPackageLink;

namespace Enterprise.Customs.ZA.Business
{
	public class BaseAsycudaManifestHeaderCopyBO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BaseAsycudaManifestHeaderCopyBO(BaseAsycudaManifestHeader sourceHeader, AsycudaManifestHeader destinationHeader)
			: base(destinationHeader.Factory)
		{
			DestinationHeader = destinationHeader;
			SourceHeader = sourceHeader;
		}

		protected readonly AsycudaManifestHeader DestinationHeader;
		protected readonly BaseAsycudaManifestHeader SourceHeader;
		Dictionary<ZGuid, ZGuid> containerMap;

		[BusinessObjectTestExclude]
		public BaseAsycudaContainerCollection SourceContainers
		{
			get
			{
				if (sourceContainers == null)
				{
					sourceContainers = new BaseAsycudaContainerCollection(SourceHeader);
					sourceContainers.Load();
				}
				return sourceContainers;
			}
		}
		BaseAsycudaContainerCollection sourceContainers;

		public BaseAsycudaContainer SourceContainerToCopy { get; set; }

		public void CopyValuesFromGlobalManifest()
		{
			CopyManifestHeader();
			CopyContainer();
			CopyBills();
		}

		void CopyManifestHeader()
		{
			var copyHeaderArgs = new BusinessObjectCloneArgs();
			copyHeaderArgs.AddExcludedColumns(new[]
			{
				AsycudaManifestHeaderSchema.Constants.AMA_ApplicationCode,
				AsycudaManifestHeaderSchema.Constants.AMA_ClusterKey,
				AsycudaManifestHeaderSchema.Constants.AMA_JobReference,
				AsycudaManifestHeaderSchema.Constants.AMA_ParentId,
				AsycudaManifestHeaderSchema.Constants.AMA_ParentTableCode,
				AsycudaManifestHeaderSchema.Constants.AMA_ManifestType
			});

			DestinationHeader.CopyPersistentValuesFrom(SourceHeader, copyHeaderArgs);
		}

		void CopyBills()
		{
			var copyBillArgs = new BusinessObjectCloneArgs();
			copyBillArgs.AddExcludedColumns(new[]
			{
				AsycudaBillSchema.Constants.ABL_AMA,
				AsycudaBillSchema.Constants.ABL_BolType,
				AsycudaBillSchema.Constants.ABL_ClusterKey
			});

			CopyMasterBill(copyBillArgs);

			var otherBills = LoadBills(SourceHeader);
			foreach (var bill in otherBills)
			{
				var copiedBill = DestinationHeader.Bills.AddNew();
				copiedBill.CopyPersistentValuesFrom(bill, copyBillArgs);
				CopyBillPacks(bill, copiedBill);
			}
		}

		void CopyMasterBill(BusinessObjectCloneArgs copyBillArgs)
		{
			var masterBill = LoadMasterBill(SourceHeader);

			if (masterBill != null)
			{
				DestinationHeader.MasterBill.CopyPersistentValuesFrom(masterBill, copyBillArgs);
				DestinationHeader.AMA_IssueDateInfo.RefreshBinding();
				DestinationHeader.AMA_MasterBillInfo.RefreshBinding();
			}
		}

		void CopyBillPacks(BaseAsycudaBill sourceBill, AsycudaBill destinationBill)
		{
			var copyBillPackArgs = new BusinessObjectCloneArgs();
			copyBillPackArgs.AddExcludedColumns(new[]
			{
				AsycudaPackSchema.Constants.APA_ABL_Bill,
				AsycudaPackSchema.Constants.APA_ClusterKey
			});

			var packQuery = new ZDBOnlyQuery(typeof(AsycudaPack));
			packQuery.AddToFilter(AsycudaPackSchema.APA_ABL_Bill, sourceBill.PK);
			if (SourceContainerToCopy != null)
			{
				var containerLinkQuery = new ZDBOnlySubQuery(typeof(BaseAsycudaPackageContainerLink), AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack);
				containerLinkQuery.AddToFilter(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, SourceContainerToCopy.PK);
				packQuery.AddSubQuery(containerLinkQuery, JoinCondition.And);
			}

			var billPacks = Factory.Load<BaseAsycudaPack>(packQuery);
			destinationBill.Packs.RemoveAndDeleteAll();

			foreach (var billPack in billPacks)
			{
				var copiedBillPack = destinationBill.Packs.AddNew();
				copiedBillPack.CopyPersistentValuesFrom(billPack, copyBillPackArgs);
				if (SourceContainerToCopy != null)
				{
					copiedBillPack.ContainerPK = containerMap[SourceContainerToCopy.PK];
				}
			}
		}

		void CopyContainer()
		{
			if (SourceContainerToCopy != null)
			{
				var copyContainerArgs = new BusinessObjectCloneArgs();
				copyContainerArgs.AddExcludedColumns(new[]
				{
					AsycudaContainerSchema.Constants.ACN_AMA_Manifest,
					AsycudaContainerSchema.Constants.ACN_ClusterKey
				});

				var copiedContainer = DestinationHeader.Containers.AddNew();
				copiedContainer.CopyPersistentValuesFrom(SourceContainerToCopy, copyContainerArgs);
				if (containerMap == null)
				{
					containerMap = new Dictionary<ZGuid, ZGuid>();
				}

				containerMap.Add(SourceContainerToCopy.PK, copiedContainer.PK);
			}
		}

		BaseAsycudaBill LoadMasterBill(BaseAsycudaManifestHeader sourceHeader)
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, sourceHeader.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			query.OrderBy = AsycudaBill.Schema.ABL_SystemCreateTimeUtc;

			return Factory.LoadTop1<BaseAsycudaBill>(query);
		}

		BaseAsycudaBill[] LoadBills(BaseAsycudaManifestHeader sourceHeader)
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, sourceHeader.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);

			return Factory.Load<BaseAsycudaBill>(query);
		}

		public class BaseAsycudaContainerCollection : DependentBusinessObjectCollection<BaseAsycudaContainer, BaseAsycudaManifestHeader>
		{
			public BaseAsycudaContainerCollection(BaseAsycudaManifestHeader master)
				: base(master)
			{ }

			protected override string FkColumnName => AsycudaContainer.Schema.ACN_AMA_Manifest;
		}
	}
}
