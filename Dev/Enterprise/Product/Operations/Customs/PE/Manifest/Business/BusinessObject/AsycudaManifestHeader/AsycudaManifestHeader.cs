using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Peru;

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source)
			=> new AsycudaManifestHeaderSynchroniser(this, source);

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public override bool ShowPackedItems => false;

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (!IsCopying && oldValue != AMA_TransportMode)
				{
					Bills?.Cast<AsycudaBill>().ForEach(x => x.CargoNatureInfo.ClearValue());
					Bills?.Cast<AsycudaBill>().ForEach(x => x.CargoConditionInfo.ClearValue());
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = PEManifestTypes.Codes.MAN;
		}
#endif
	}
}
