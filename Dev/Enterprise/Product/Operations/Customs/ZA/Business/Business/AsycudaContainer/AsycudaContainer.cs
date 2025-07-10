using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(Schema.ACN_ContainerNumber), DescriptionProperty("Description")]
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.ZAOutTurn)]
	public class AsycudaContainer : ManifestBase.AsycudaContainer
		, Integration.Customs.ZA.IAsycudaContainer
		, IPurgeValueParent
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ManifestBase.AsycudaContainer.Schema
		{
			public const string ContUnpackTime = "ContUnpackTime";
			public const string GateInOutDate = "GateInOutDate";
		}

		public ZString Description => ZString.Format("{0} {1}pk {2}", ACN_ContainerNumber, ACN_NumberOfPackages, ACN_CommodityCode);

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.EmptyFullList))]
		public override ZString ACN_EmptyFullIndicator
		{
			get => base.ACN_EmptyFullIndicator;
			set => base.ACN_EmptyFullIndicator = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealTypeList))]
		public override ZString ACN_SealingPartyType
		{
			get => base.ACN_SealingPartyType;
			set => base.ACN_SealingPartyType = value;
		}

		#region ContUnpackTime

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaContainer.ContUnpackTime", Caption = "Packed/Unpacked DateTime")]
		public ZDateTime ContUnpackTime
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.ContUnpackTime);
			set
			{
				var oldValue = ContUnpackTime;
				this.SetSystemDefinedValue(Schema.ContUnpackTime, value);
				ContUnpackTimeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContUnpackTime();
				}
			}
		}

		public ZPropertyInfo ContUnpackTimeInfo => GetZPropertyInfo(nameof(Schema.ContUnpackTime));

		#endregion

		#region GateInOutDate

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.GateInOutDate", Caption = "Gate In/Out DateTime")]
		[PurgeValueExcept("UseGateInOutDatePerContainer")]
		public ZDateTime GateInOutDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.GateInOutDate);
			set
			{
				var oldValue = GateInOutDate;
				var hasChanges = oldValue != value;
				this.SetSystemDefinedValue(Schema.GateInOutDate, value);
				GateInOutDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGateInOutDate();
				}
			}
		}

		public ZPropertyInfo GateInOutDateInfo => GetZPropertyInfo(nameof(GateInOutDate));

		public bool UseGateInOutDatePerContainer => Header.UseGateInOutDatePerContainer;

		#endregion

		#region IPurgeValueParent Members

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeHelper ?? (purgeHelper = new PurgeValueHelper<AsycudaContainer>(this)); }
		}

		IPurgeValueHelper purgeHelper;

		#endregion

		public override void Delete()
		{
			this.DeleteChildren<AsycudaContainerBillOrPackageLink>(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container);
			base.Delete();
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);
	}
}
