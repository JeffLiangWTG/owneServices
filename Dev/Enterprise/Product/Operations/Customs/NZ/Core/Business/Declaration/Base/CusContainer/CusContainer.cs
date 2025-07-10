using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainer : AutoNZCusContainer, Integration.Customs.NZ.ICusContainer
	{
		#region Schema
		public new class Schema : AutoNZCusContainer.Schema
		{
			public const string CO_OA_PackingLocation = "CO_OA_PackingLocation";
			public const string PackingLocationOrgPK = "PackingLocationOrgPK";
		}
		#endregion

		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation
		public new CusContainerValidation Validation
		{
			get { return GetNewValidation() as CusContainerValidation; }
		}

		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			return new CusContainerValidation(this);
		}
		#endregion

		#region Lookups
		public new CusContainerLookups Lookups
		{
			get { return (CusContainerLookups)base.Lookups; }
		}

		protected override Customs.Business.CusContainerLookups GetNewLookups()
		{
			return new CusContainerLookups(this);
		}
		#endregion

		#region Declaration
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
		#endregion

		public override ZGuid CO_RC
		{
			get { return base.CO_RC; }
			set
			{
				if (CO_RC != value)
				{
					base.CO_RC = value;
					var isoEquipmentSizeTypeCode = Container?.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
					if (!isoEquipmentSizeTypeCode.IsEmpty)
					{
						CO_MAF_ContainerType = isoEquipmentSizeTypeCode.SubstringSafe(0, NZCusContainerSchema.CO_MAF_ContainerType.MaxLength);
						CO_ContainerSize = isoEquipmentSizeTypeCode.Left(2);
					}
				}
			}
		}

		public override ZGuid CO_JE
		{
			get { return base.CO_JE; }
			set
			{
				base.CO_JE = value;
				if (Declaration != null && Declaration.DepotDocAddress != null)
				{
					if (CO_SealingParty.IsEmpty && Declaration.IsTSWDeclaration && Declaration.IsExport && Declaration.IsSea)
					{
						CO_SealingParty = Declaration.DefaultSealingParty;
					}
				}
			}
		}

		public override ZDecimal CO_Weight
		{
			get { return base.CO_Weight; }
			set
			{
				var oldValue = CO_Weight;
				base.CO_Weight = value;
				if (!IsCopying && oldValue != CO_Weight)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CO_FCL_LCL_AIR
		{
			get { return base.CO_FCL_LCL_AIR; }
			set
			{
				var oldValue = CO_FCL_LCL_AIR;
				if (value == BaseCusContainer.ContainerModes.FCX && IsBuyersConsolShipment)
				{
					value = ContainerModeList.Codes.FCL;
				}

				base.CO_FCL_LCL_AIR = value;
				if (!IsCopying && oldValue != CO_FCL_LCL_AIR)
				{
					JobContainer.JC_IsEmptyContainer = IsEmptyContainer;
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		bool IsBuyersConsolShipment
		{
			get
			{
				bool isBuyersConsolShipment = false;
				if (Declaration != null)
				{
					var relatedShipment = Declaration.Shipment;
					if (relatedShipment != null)
					{
						if (relatedShipment.JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead ||
							relatedShipment.JS_ShipmentType == Core.Constants.ShipmentTypes.StandardHouse && relatedShipment.JS_JS_ColoadMasterShipment.IsValid)
						{
							isBuyersConsolShipment = true;
						}
					}
				}

				return isBuyersConsolShipment;
			}
		}

		public override ZBool UseContainerSize => true;

		public override bool IsFullContainer
		{
			get { return base.IsFullContainer || CO_FCL_LCL_AIR == ContainerModeList.Codes.FCL; }
		}

		public bool IsEmptyContainer
		{
			get { return CO_FCL_LCL_AIR == ContainerModeList.Codes.Empty; }
		}

		#region ContainerNumberIsValidPalletNumber
		public bool ContainerNumberIsValidPalletNumber()
		{
			return CO_ContainerNumber.SubstringSafe(0, 1) == "P" && (
				(CO_ContainerNumber.Length == 2 && CO_ContainerNumber.SubstringSafe(1, 1).KeepChars("1234567890").Length == 1)
				||
				(CO_ContainerNumber.Length == 3 && CO_ContainerNumber.SubstringSafe(1, 2).KeepChars("1234567890").Length == 2)
				);
		}
		#endregion

		public ZDecimal ECI_ApportionedContainerisedGoodsValue
		{
			get { return Declaration != null ? Declaration.ECI_ApportionedContainerAndLoosePackageValues.GetValueForContainer(this) : ZDecimal.Zero; }
		}

		#region AddInfo Lookups
		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.MAFContainerTypeList))]
		public override ZString CO_MAF_ContainerType
		{
			get { return base.CO_MAF_ContainerType; }
			set { base.CO_MAF_ContainerType = value; }
		}
		#endregion

		[ChildEditable(true)]
		[UniversalCopySplitCollection("Packing Location", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.ContainerPacking + "'")]
		public override JobDocAddressDependentCollection DocAddresses => base.DocAddresses;

		#region PackingLocationDocumentaryAddress
		public JobDocAddress PackingLocationDocumentaryAddress
		{
			get
			{
				if (fPackingLocationDocumentaryAddress == null || fPackingLocationDocumentaryAddress.IsDeleted)
				{
					fPackingLocationDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ContainerPacking);
					OnPackingLocationDocumentaryAddressFoundOrCreated();
				}
				return fPackingLocationDocumentaryAddress;
			}
		}
		JobDocAddress fPackingLocationDocumentaryAddress;

		protected void OnPackingLocationDocumentaryAddressFoundOrCreated()
		{
			fPackingLocationDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(PackingLocationDocumentaryAddress_OnRelationshipFieldsChanged);
		}

		void PackingLocationDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		protected override DocAddressType[] GetSupportedAddressTypesCore()
		{
			return base.GetSupportedAddressTypesCore().Concat(new DocAddressType[] { DocAddressType.ContainerPacking }).ToArray();
		}

		[List(nameof(CO_OA_PackingLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid CO_OA_PackingLocation
		{
			get
			{
				return PackingLocationDocumentaryAddress.E2_OA_Address;
			}
			set
			{
				PackingLocationDocumentaryAddress.E2_OA_Address = value;

				Validation.ValidateCO_OA_PackingLocation();
				CO_OA_PackingLocationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CO_OA_PackingLocationInfo
		{
			get { return GetZPropertyInfo(Schema.CO_OA_PackingLocation); }
		}

		#endregion

		#region PackingLocationOrgPK
		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.PackingLocationList))]
		public ZGuid PackingLocationOrgPK
		{
			get { return CO_OA_PackingLocation_ZAddress.OrgPK; }
			set { CO_OA_PackingLocation_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo PackingLocationOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PackingLocationOrgPK, x => CO_OA_PackingLocation_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region CO_OA_PackingLocation_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress CO_OA_PackingLocation_ZAddress
		{
			get
			{
				if (fCO_OA_PackingLocation_ZAddress == null)
				{
					fCO_OA_PackingLocation_ZAddress = GetNewCO_OA_PackingLocation_ZAddress();
					fCO_OA_PackingLocation_ZAddress.IsOrgVisible = true;
				}
				return fCO_OA_PackingLocation_ZAddress;
			}
		}
		ZAddress fCO_OA_PackingLocation_ZAddress;

		protected virtual ZAddress GetNewCO_OA_PackingLocation_ZAddress()
		{
			ZAddress result = new ZAddress(CO_OA_PackingLocationInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		public OrgAddress StuffingEstablishmentAddress
		{
			get { return Factory.Load<OrgAddress>(CO_OA_PackingLocation); }
		}
	}
}
