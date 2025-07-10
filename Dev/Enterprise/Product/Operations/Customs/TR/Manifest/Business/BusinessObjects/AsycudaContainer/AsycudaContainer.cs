using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.TRManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ManifestBase.AutoAsycudaContainer.Schema
		{
			public const string Relation = "Relation";
			public const int RelationMaxLength = 7;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Relation = RelationList.Codes.Foreign;
		}

		#endregion

		#region Properties

		#region Relation

		[ResourceStringData("AsycudaContainer.Relation", Caption = "Foreign/Local")]
		[MaxLength(Schema.RelationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.RelationList))]
		public ZString Relation
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.Relation);
			set
			{
				var oldValue = Relation;
				CheckMaximumLength(RelationInfo, value);
				this.SetSystemDefinedValue(Schema.Relation, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRelation();
				}
				RelationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo RelationInfo => GetZPropertyInfo(Schema.Relation);

		#endregion

		#endregion
	}
}
