using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.Business
{
	[ProvideMetaDataProperty("ShouldSynchroniserReadOnly", MetaDataTypes.ReadOnly)]
	public class Package : BasePackage, Integration.Customs.TW.IPackage, IPackingInformation, ISynchroniserReadOnlyMembersProvider
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusDecHouseContainerPackLookups GetNewLookups() => new PackageLookups(this);

		public new PackageLookups Lookups => (PackageLookups)base.Lookups;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);

		public new PackageValidation Validation => (PackageValidation)base.Validation;

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_NetWeight", Caption = "Net Weight")]
		public override ZDecimal CW_NetWeight
		{
			get => base.CW_NetWeight;
			set
			{
				bool hasChanged = base.CW_NetWeight != value;
				base.CW_NetWeight = value;
				if (hasChanged && !IsCopying)
				{
					Declaration?.TotalDeclarationPackingNetWeightInKilogramsInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_NetWeightUQ", Caption = "Net Weight UQ")]
		public override ZString CW_NetWeightUQ { get => base.CW_NetWeightUQ; set => base.CW_NetWeightUQ = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_GrossWeight", Caption = "Gross Weight")]
		public override ZDecimal CW_GrossWeight
		{
			get => base.CW_GrossWeight;
			set
			{
				bool hasChanged = base.CW_GrossWeight != value;
				base.CW_GrossWeight = value;
				if (hasChanged && !IsCopying)
				{
					Declaration?.TotalDeclarationPackingGrossWeightInKilogramsInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_GrossWeightUQ", Caption = "Gross Weight UQ")]
		public override ZString CW_GrossWeightUQ { get => base.CW_GrossWeightUQ; set => base.CW_GrossWeightUQ = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_Volume", Caption = "Volume")]
		public override ZDecimal CW_Volume { get => base.CW_Volume; set => base.CW_Volume = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_VolumeUQ", Caption = "Volume UQ")]
		public override ZString CW_VolumeUQ { get => base.CW_VolumeUQ; set => base.CW_VolumeUQ = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_Length", Caption = "Length")]
		public override ZDecimal CW_Length { get => base.CW_Length; set => base.CW_Length = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_Height", Caption = "Height")]
		public override ZDecimal CW_Height { get => base.CW_Height; set => base.CW_Height = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_Width", Caption = "Width")]
		public override ZDecimal CW_Width { get => base.CW_Width; set => base.CW_Width = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.Package|CW_DimensionUQ", Caption = "Dimension UQ")]
		public override ZString CW_DimensionUQ { get => base.CW_DimensionUQ; set => base.CW_DimensionUQ = value; }

		public override ZInt CW_PackQty
		{
			get { return base.CW_PackQty; }
			set
			{
				base.CW_PackQty = value;
				SetCW_HouseBillDefaultValue();
			}
		}

		public void SetCW_HouseBillDefaultValue()
		{
			var lastPackage = Declaration?.Packages?.Cast<Package>()?.Where(x => x != this && !x.IsDeleted).LastOrDefault();
			if (lastPackage != null && !lastPackage.CW_HouseBill.IsEmpty && CW_CR_HouseContainer.IsEmpty)
			{
				CW_HouseBill = lastPackage.CW_HouseBill;
			}
		}

		public ZDecimal NetWeightInKilograms => new ZWeight(CW_NetWeight, CW_NetWeightUQ).InKilogramsSafe;

		public ZDecimal GrossWeightInKilograms => new ZWeight(CW_GrossWeight, CW_GrossWeightUQ).InKilogramsSafe;

		bool IPackingInformation.SupportMarksAndNumbers
		{
			get { return false; }
		}

		bool IsDataSyncFromShipment => Declaration?.IsDataSyncFromShipment ?? false;

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string> { CW_GrossWeightInfo.Name, CW_GrossWeightUQInfo.Name, CW_VolumeInfo.Name, CW_VolumeUQInfo.Name, CW_LengthInfo.Name, CW_HeightInfo.Name, CW_WidthInfo.Name, CW_DimensionUQInfo.Name, CW_PackQtyInfo.Name, CW_HouseBillInfo.Name, CW_PackTypeInfo.Name });

		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldSynchroniserReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || (IsDataSyncFromShipment && SynchroniserReadOnlyMembers.Contains(property.Name));
		}
	}
}
