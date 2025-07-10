using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.TWManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(17)]
		public override ZString ACN_ContainerNumber { get => base.ACN_ContainerNumber; set => base.ACN_ContainerNumber = value; }

		[MaxLength(17)]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaContainer|ACN_Seal1", Caption = "Seal 1")]
		public override ZString ACN_Seal1 { get => base.ACN_Seal1; set => base.ACN_Seal1 = value; }

		[MaxLength(17)]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaContainer|ACN_Seal2", Caption = "Seal 2")]
		public override ZString ACN_Seal2 { get => base.ACN_Seal2; set => base.ACN_Seal2 = value; }

		[MaxLength(17)]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaContainer|ACN_Seal3", Caption = "Seal 3")]
		public override ZString ACN_Seal3 { get => base.ACN_Seal3; set => base.ACN_Seal3 = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaContainer|ACN_EmptyFullIndicator", Caption = "Container Mode", ShortCaption = "Cont. Mode")]
		public override ZString ACN_EmptyFullIndicator { get => base.ACN_EmptyFullIndicator; set => base.ACN_EmptyFullIndicator = value; }

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);
	}
}
