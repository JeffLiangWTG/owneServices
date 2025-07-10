using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.UY.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader, IMessageAttachee
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Uruguay;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		#region IMessageAttachee

		ZGuid IMessageAttachee.GlobalBranchPK => AMA_GB;
		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
		ZString IMessageAttachee.JobReference => AMA_JobReference;
		ZGuid IMessageAttachee.PK => PK;
		ZString IMessageAttachee.TableName => AsycudaManifestHeaderSchema.Constants.TableName;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = UYManifestTypes.Codes.MAN;
		}
#endif
	}
}
