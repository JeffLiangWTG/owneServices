using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	[UserDefinedValues, CodeProperty(Schema.BJ_RegistrationNumber), DescriptionProperty(Schema.BJ_EquipmentDescription)]
	public class Equipment : AutoCusInBondEquipment, ICusCodeDataTypeSupporter
	{
		public Equipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCusInBondEquipment.Schema
		{
			public const string BJ_EmptyIITsCoveredByCarrier = "BJ_EmptyIITsCoveredByCarrier";
			public const string BJ_EmptyIITsCoveredByImporter = "BJ_EmptyIITsCoveredByImporter";
			public const string BJ_EquipmentDescription = "BJ_EquipmentDescription";
			public const string BJ_MerchandiseAndIITsCoveredByCarrier = "BJ_MerchandiseAndIITsCoveredByCarrier";
			public const string BJ_MerchandiseAndIITsCoveredByImporter = "BJ_MerchandiseAndIITsCoveredByImporter";
			public const string BJ_SealNumbers = "BJ_SealNumbers";
		}

		#endregion

		#region Load

		public static Equipment LoadOrCreateConveyance(Trip master)
		{
			var query = new ZQuery(CusInBondEquipmentSchema.BJ_BH_Header, master.PK);
			query.AddToFilter(CusInBondEquipmentSchema.BJ_IsConveyance, true);
			query.FetchOnlyFromLocalCache = !master.IsInDatabase;

			var conveyance = master.Factory.LoadTop1<Equipment>(query);
			if (conveyance == null)
			{
				conveyance = master.Factory.New<Equipment>();
				using (conveyance.SuspendSettingHasChanges())
				using (conveyance.GetValidationSuspender())
				{
					conveyance.BJ_BH_Header = master.PK;
					conveyance.BJ_IsConveyance = true;
				}
			}
			return conveyance;
		}

		#endregion

		#region Properties

		#region BJ_BH_Header

		[RelatedBusinessObject("Trip")]
		public override ZGuid BJ_BH_Header
		{
			get { return base.BJ_BH_Header; }
			set { base.BJ_BH_Header = value; }
		}

		public Trip Trip
		{
			get { return Factory.Load<Trip>(BJ_BH_Header); }
		}

		#endregion

		#region BJ_EmptyIITsCoveredByCarrier

		[ResourceStringData("Enterprise.Customs.US.eManifest.Business.Equipment|BJ_EmptyIITsCoveredByCarrier", Caption = "EC - Shipment consists of empty IIT's covered by Carrier's bond", ShortCaption = "EC", MediumCaption = "EC - Empty IIT's covered by Carrier's bond", FullDescription = "Shipment consists of empty IIT's covered by Carrier's bond.")]
		public ZBool BJ_EmptyIITsCoveredByCarrier
		{
			get { return BJ_IITEntityIndicators.Contains(IITEntityIndicatorCodes.Codes.EC); }
			set
			{
				UpdateIITEntityIndicators(IITEntityIndicatorCodes.Codes.EC, value);
				BJ_EmptyIITsCoveredByCarrierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BJ_EmptyIITsCoveredByCarrierInfo
		{
			get { return GetZPropertyInfo(Schema.BJ_EmptyIITsCoveredByCarrier); }
		}

		void UpdateIITEntityIndicators(string indicator, ZBool value)
		{
			var contains = BJ_IITEntityIndicators.Contains(indicator);
			if (value && !contains)
			{
				BJ_IITEntityIndicators += indicator;
			}
			else if (!value && contains)
			{
				BJ_IITEntityIndicators = BJ_IITEntityIndicators.Replace(indicator, "");
			}
		}

		#endregion

		#region BJ_EmptyIITsCoveredByImporter

		[ResourceStringData("Enterprise.Customs.US.eManifest.Business.Equipment|BJ_EmptyIITsCoveredByImporter", Caption = "EI - Shipment consists of empty IIT's covered by Importer's bond", ShortCaption = "EI", MediumCaption = "EI - Empty IIT's covered by Importer's bond", FullDescription = "Shipment consists of empty IIT's covered by Importer's bond.")]
		public ZBool BJ_EmptyIITsCoveredByImporter
		{
			get { return BJ_IITEntityIndicators.Contains(IITEntityIndicatorCodes.Codes.EI); }
			set
			{
				UpdateIITEntityIndicators(IITEntityIndicatorCodes.Codes.EI, value);
				BJ_EmptyIITsCoveredByImporterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BJ_EmptyIITsCoveredByImporterInfo
		{
			get { return GetZPropertyInfo(Schema.BJ_EmptyIITsCoveredByImporter); }
		}

		#endregion

		#region BJ_EquipmentDescription

		public ZString BJ_EquipmentDescription
		{
			get { return RefEquipment?.RQ_DescriptionMultilingual ?? ZString.Empty; }
		}

		#endregion

		bool InsuranceReadOnly
		{
			get { return !BJ_IsConveyance; }
		}

		[ReadOnlyMember(nameof(InsuranceReadOnly))]
		public override ZDecimal BJ_InsuranceAmount
		{
			get { return base.BJ_InsuranceAmount; }
			set { base.BJ_InsuranceAmount = value; }
		}

		[ReadOnlyMember(nameof(InsuranceReadOnly))]
		public override ZString BJ_InsuranceName
		{
			get { return base.BJ_InsuranceName; }
			set { base.BJ_InsuranceName = value; }
		}

		[ReadOnlyMember(nameof(InsuranceReadOnly))]
		public override ZString BJ_InsurancePolicyNumber
		{
			get { return base.BJ_InsurancePolicyNumber; }
			set { base.BJ_InsurancePolicyNumber = value; }
		}

		[ReadOnlyMember(nameof(InsuranceReadOnly))]
		public override ZInt BJ_InsuranceYearPolicyIssue
		{
			get { return base.BJ_InsuranceYearPolicyIssue; }
			set { base.BJ_InsuranceYearPolicyIssue = value; }
		}

		#region EQ_InsuranceCurrency

		[List(nameof(Lookups) + "." + nameof(EquipmentLookups.Currencies))]
		public ZString EQ_InsuranceCurrency
		{
			get { return Core.Constants.CurrencyCodes.UnitedStates; }
		}

		#endregion

		#region BJ_MerchandiseAndIITsCoveredByCarrier

		[ResourceStringData("Enterprise.Customs.US.eManifest.Business.Equipment|BJ_MerchandiseAndIITsCoveredByCarrier", Caption = "MC - Shipment consists of merchandise and IIT's. IIT's covered by Carrier's bond", ShortCaption = "MC", MediumCaption = "MC - Merchandise and IIT's. IIT's covered by Carrier's bond", FullDescription = "Shipment consists of merchandise and IIT's. IIT's covered by Carrier's bond.")]
		public ZBool BJ_MerchandiseAndIITsCoveredByCarrier
		{
			get { return BJ_IITEntityIndicators.Contains(IITEntityIndicatorCodes.Codes.MC); }
			set
			{
				UpdateIITEntityIndicators(IITEntityIndicatorCodes.Codes.MC, value);
				BJ_MerchandiseAndIITsCoveredByCarrierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BJ_MerchandiseAndIITsCoveredByCarrierInfo
		{
			get { return GetZPropertyInfo(Schema.BJ_MerchandiseAndIITsCoveredByCarrier); }
		}

		#endregion

		#region BJ_MerchandiseAndIITsCoveredByImporter

		[ResourceStringData("Enterprise.Customs.US.eManifest.Business.Equipment|BJ_MerchandiseAndIITsCoveredByImporter", Caption = "MI - Shipment consists of merchandise and IIT's. IIT's covered by Importer's bond", ShortCaption = "MI", MediumCaption = "MI - Merchandise and IIT's. IIT's covered by Importer's bond", FullDescription = "Shipment consists of merchandise and IIT's. IIT's covered by Importer's bond.")]
		public ZBool BJ_MerchandiseAndIITsCoveredByImporter
		{
			get { return BJ_IITEntityIndicators.Contains(IITEntityIndicatorCodes.Codes.MI); }
			set
			{
				UpdateIITEntityIndicators(IITEntityIndicatorCodes.Codes.MI, value);
				BJ_MerchandiseAndIITsCoveredByImporterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BJ_MerchandiseAndIITsCoveredByImporterInfo
		{
			get { return GetZPropertyInfo(Schema.BJ_MerchandiseAndIITsCoveredByImporter); }
		}

		#endregion

		#region BJ_RQ_Equipment

		[List(nameof(Lookups) + "." + nameof(EquipmentLookups.Equipment))]
		[RelatedBusinessObject("RefEquipment")]
		public override ZGuid BJ_RQ_Equipment
		{
			get { return base.BJ_RQ_Equipment; }
			set
			{
				var hasCHanged = base.BJ_RQ_Equipment != value;
				base.BJ_RQ_Equipment = value;
				if (!IsCopying && hasCHanged && !value.IsEmpty)
				{
					var equipment = RefEquipment;
					if (equipment != null)
					{
						// Copy properties from dbo.RefEquipment onto (CusInBond)Equipment
						BJ_RegistrationNumber = equipment.RQ_Registration.Left(Schema.BJ_RegistrationNumberMaxLength);
						BJ_ACEID = GetReferenceNumberCore(equipment, ConveyanceReferences.Codes.ACEId).Left(Schema.BJ_ACEIDMaxLength);
						BJ_ContainerType = equipment.RQ_EquipmentType.Left(Schema.BJ_ContainerTypeMaxLength);
						BJ_RN_NKRegistrationCountry = equipment.RQ_RN_NKRegistrationCountry.Left(Schema.BJ_RN_NKRegistrationCountryMaxLength);
						BJ_RW_NKRegistrationState = equipment.RQ_RegState.Left(Schema.BJ_RW_NKRegistrationStateMaxLength);
						BJ_RC_RoadContainerType = equipment.RQ_RC_RoadContainerType;
						BJ_VIN = equipment.RQ_VIN.Left(Schema.BJ_VINMaxLength);
					}
				}
			}
		}

		public override ZPropertyInfo BJ_RQ_EquipmentInfo
		{
			get
			{
				var result = base.BJ_RQ_EquipmentInfo;
				result.HumanReadableName = HumanReadableName;
				return result;
			}
		}

		public RefEquipment RefEquipment
		{
			get { return Equipment; }
		}

		#endregion

		#region BJ_SealNumbers

		[BusinessObjectMaxLengthTestExclude]
		public ZString BJ_SealNumbers
		{
			get { return SealNumbers.DataAsString(); }
			set
			{
				SealNumbers.PopulateDataFromString(value);
				BJ_SealNumbersInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BJ_SealNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.BJ_SealNumbers); }
		}

		#endregion

		bool IsRqEquipmentSet
		{
			get { return !BJ_RQ_Equipment.IsEmpty; }
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZString BJ_RegistrationNumber
		{
			get { return base.BJ_RegistrationNumber; }
			set { base.BJ_RegistrationNumber = value; }
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZString BJ_ACEID
		{
			get { return base.BJ_ACEID; }
			set { base.BJ_ACEID = value; }
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZString BJ_ContainerType
		{
			get { return base.BJ_ContainerType; }
			set { base.BJ_ContainerType = value; }
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZGuid BJ_RC_RoadContainerType
		{
			get => base.BJ_RC_RoadContainerType;
			set => base.BJ_RC_RoadContainerType = value;
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		[ResourceStringData("Enterprise.Customs.US.eManifest.Business.Equipment|BJ_RN_NKRegistrationCountry", ShortCaption = "Reg. Ctry/Rgn.", Caption = "Reg. Country/Region", FullDescription = "Registration Country/Region")]
		public override ZString BJ_RN_NKRegistrationCountry
		{
			get => base.BJ_RN_NKRegistrationCountry;
			set => base.BJ_RN_NKRegistrationCountry = value;
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZString BJ_RW_NKRegistrationState
		{
			get => base.BJ_RW_NKRegistrationState;
			set => base.BJ_RW_NKRegistrationState = value;
		}

		[ReadOnlyMember(nameof(IsRqEquipmentSet))]
		public override ZString BJ_VIN
		{
			get => base.BJ_VIN;
			set => base.BJ_VIN = value;
		}

		public override ZBool BJ_IsConveyance
		{
			get => base.BJ_IsConveyance;
			set
			{
				base.BJ_IsConveyance = value;
				if (value && Trip?.AllEquipmentIncludingMainConveyance.Count(x => x.BJ_IsConveyance) == 1)
				{
					Trip.PopulateCommodityWithEquipment(this.PK);
				}
			}
		}

		#endregion

		#region Collections

		#region Seal Numbers

		[ChildEditable(true)]
		public SealNumberCollection SealNumbers
		{
			get
			{
				if (sealNumbers == null)
				{
					sealNumbers = new SealNumberCollection(this);
					sealNumbers.Load();
					RegisterEditableChildObject(sealNumbers);
				}
				return sealNumbers;
			}
		}

		SealNumberCollection sealNumbers;

		#endregion

		#endregion

		#region New Methods

		public ZString GetReferenceNumber(string type)
		{
			return GetReferenceNumberCore(RefEquipment, type);
		}

		ZString GetReferenceNumberCore(RefEquipment equipment, string type)
		{
			return equipment?.Certificates.FirstOrDefault(cert => cert.XZ_Type == type)?.XZ_RefNumber ?? ZString.Empty;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.SealNumber, typeof(SealNumber));
			return result;
		}

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsEmpty)
			{
				Delete();
			}
		}

		public bool IsEmpty
		{
			get { return BJ_RQ_Equipment.IsEmpty && BJ_ACEID.IsEmpty && BJ_ContainerType.IsEmpty && BJ_RegistrationNumber.IsEmpty && BJ_IITEntityIndicators.IsEmpty && BJ_InsuranceAmount.IsEmpty && BJ_InsuranceName.IsEmpty && BJ_InsurancePolicyNumber.IsEmpty && BJ_InsuranceYearPolicyIssue.IsEmpty && !SealNumbers.Any(); }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				SealNumbers.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return BJ_IsConveyance ? "Conveyance" : "Equipment"; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region Validation

		public new EquipmentValidation Validation
		{
			get { return (EquipmentValidation)base.Validation; }
		}

		protected override CusInBondEquipmentValidation GetNewValidation()
		{
			return new EquipmentValidation(this);
		}

		#endregion

		#region Lookups

		public new EquipmentLookups Lookups
		{
			get { return (EquipmentLookups)base.Lookups; }
		}

		protected override CusInBondEquipmentLookups GetNewLookups()
		{
			return new EquipmentLookups(this);
		}

		#endregion

		#endregion
	}
}
