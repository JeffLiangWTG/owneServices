using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader, IControllerIDProvider
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void UnLockPacksIfNeed(ASYCUDA.Business.AsycudaBill bill)
		{
			base.UnLockPacksIfNeed(bill);
			bill.Packs.Cast<AsycudaPack>().Where(x => x.ReadOnly && x.PackedItem.API_MessageStatus == ASYCUDA.Business.MessageStatusCodeList.Codes.Error)
							.ForEach(x => x.SetReadOnlyIncludingChildren(false));
		}

		protected override void SetupAdditionalLockingIfNeeded(ASYCUDA.Business.AsycudaPackedItem packedItem,
			ASYCUDA.Business.AsycudaPack pack, ASYCUDA.Business.AsycudaBill bill, bool currentReadOnly,
			bool newReadOnly)
		{
			base.SetupAdditionalLockingIfNeeded(packedItem, pack, bill, currentReadOnly, newReadOnly);
			if (newReadOnly)
			{
				if (currentReadOnly != newReadOnly)
				{
					UnLockPacksIfNeed(bill);
				}
				else if (packedItem.API_MessageStatus == ASYCUDA.Business.MessageStatusCodeList.Codes.Error)
				{
					pack.SetReadOnlyIncludingChildren(false);
				}
			}
		}

		protected override bool IsAMA_NatureReadOnly => true;

		public override ZBool IsImport => AMA_ManifestType == Constants.ManifestType.Import;
		public override ZBool IsExport => AMA_ManifestType == Constants.ManifestType.Export;

		public ZBool IsOVRApplicable => IsImport && SGAccessRegistry.IsOVRLiveEffective;

		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (!IsCopying && oldValue != value)
				{
					if (IsExport)
					{
						AMA_Nature = ShipmentTypeList.Codes.Export22;
					}
					else if (IsImport)
					{
						AMA_Nature = ShipmentTypeList.Codes.Import23;
					}

					MarkBillsApportionmentDirty();
					foreach (AsycudaBill bill in Bills)
					{
						bill.ABL_ShipmentType = AMA_Nature;
					}
				}
			}
		}

		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				var oldValue = AMA_ApplicationCode;
				base.AMA_ApplicationCode = value;

				if (!IsCopying && oldValue != AMA_ApplicationCode)
				{
					foreach (AsycudaBill bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal void MarkBillsApportionmentDirty()
		{
			Bills.AsEnumerable().ForEach(c => c.ApportionmentDirty = true);
		}

		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		public new CusPersonCollection Persons => (CusPersonCollection)base.Persons;
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new CusPersonCollection(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Singapore;
		protected override bool SupportBillLock() => true;

		protected override ASYCUDA.Business.MessageChooser GetNewMessageChooserCore(IEnumerable<ISelectionItem> items, string messageType, bool showStatus) => new MessageChooser(this, items, showStatus);
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;
		protected override ZString GetCustomsLocalCode(ZString portCode, RefUNLOCO port)
		{
			var result = portCode;
			if (port != null)
			{
				result = GetCustomsLocalCodeList(port).FirstOrDefault()?.RY_LocalPortCode ?? result;
			}
			return result;
		}

		protected override bool ShouldAddUXMLMessagesToCollection => true;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = SGManifestTypes.Codes.MGI;
		}
#endif
	}
}

