using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.ZAManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ManifestBase.AutoAsycudaContainer.Schema
		{
			public const string LandedPurpose = "LandedPurpose";
			public const int LandedPurposeMaxLength = 3;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;
		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);
		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;
		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

		#region LandedPurpose

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.LandedPurposeList))]
		[MaxLength(Schema.LandedPurposeMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.Manifest.Business.AsycudaContainer.LandedPurpose", Caption = "Landed Purpose")]
		public ZString LandedPurpose
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.LandedPurpose);
			set
			{
				var oldValue = LandedPurpose;
				CheckMaximumLength(LandedPurposeInfo, value);
				var hasChanges = oldValue != value;
				if (hasChanges)
				{
					this.SetSystemDefinedValue(Schema.LandedPurpose, value);
					LandedPurposeInfo.RefreshBinding(oldValue);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLandedPurpose();
					}
				}
			}
		}

		public ZPropertyInfo LandedPurposeInfo => GetZPropertyInfo(nameof(LandedPurpose));

		#endregion
	}
}
