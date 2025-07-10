using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrganisationViewStmNums : ViewStmNums
	{
		public OrganisationViewStmNums(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : ViewStmNums.Schema
		{
			public const string SN_NamePrefix = "OrgOwned_";

			public const string SN_ZoneIDPrefix = "SN_ZoneIDPrefix";
			public const string SN_ClientPrefix = "SN_ClientPrefix";

			public const long DefaultFTZMaximumValue_NonWarehouse = 99999;
			public const long DefaultFTZMaximumValue_Warehouse = 99999999;

			public const int OldSN_ZoneIDPrefixMaxLength = 7;
			public const int SN_ZoneIDPrefixMaxLength = 9;
			public const int SN_ClientPrefixMaxLength = 3;
			public const int FTZFormatDigits = 5;
			public const int FTZControlNumberLength = FTZFormatDigits + SN_ClientPrefixMaxLength;

			public const int FTZMaximumValue_NonWarehouseDigits = 5;
			public const int FTZMaximumValue_WarehouseDigits = 8;

			public new const int SN_PrefixMaxLength = 22;
			public new const int SN_TypeMaxLength = 3;
		}

		#endregion

		#region Overrides

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var header = Header;
			bool result =
					header != null &&
					!header.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		protected override ZString NamePrefix => OrganisationViewStmNums.Schema.SN_NamePrefix;
		protected override int SN_Prefix_MaxLength => OrganisationViewStmNums.Schema.SN_PrefixMaxLength;
		protected override int SN_Type_MaxLength => OrganisationViewStmNums.Schema.SN_TypeMaxLength;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SN_Name = Schema.SN_NamePrefix;
			SN_Value = SN_MinimumValue;
		}

		protected override void InitializeTypeAndPrefixCore()
		{
			if (IsFTZNumberType)
			{
				if (IsFTZWarehouseType)
				{
					SN_ZoneIDPrefix = SN_Prefix.Left(SN_ZoneIDPrefixInfo.MaxLength);
				}
				else
				{
					var prefixLength = SN_Prefix.Length;
					var useOldFormat = (Schema.OldSN_ZoneIDPrefixMaxLength + Schema.SN_ClientPrefixMaxLength) == prefixLength;
					var zoneIDPrefixMaxLength = useOldFormat ? Schema.OldSN_ZoneIDPrefixMaxLength : Schema.SN_ZoneIDPrefixMaxLength;
					SN_ZoneIDPrefix = SN_Prefix.Left(zoneIDPrefixMaxLength);
					if (prefixLength > zoneIDPrefixMaxLength)
					{
						SN_ClientPrefix = SN_Prefix.SubstringSafe(zoneIDPrefixMaxLength, Schema.SN_ClientPrefixMaxLength);
					}
				}
			}
		}

		public new OrganisationViewStmNumsValidation Validation => (OrganisationViewStmNumsValidation)base.Validation;

		protected override ViewStmNumsValidation GetNewValidation()
		{
			return new OrganisationViewStmNumsValidation(this);
		}

		public new OrganisationViewStmNumsLookups Lookups => (OrganisationViewStmNumsLookups)base.Lookups;

		protected override ViewStmNumsLookups GetNewLookups()
		{
			return new OrganisationViewStmNumsLookups(this);
		}

		#endregion

		#region Properties

		bool IsSSCCNumberType => SN_Type == OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
		public bool IsFTZNumberType => IsFTZWarehouseType || IsFTZNonWarehouseType;
		public bool IsFTZWarehouseType => SN_Type == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
		public bool IsFTZNonWarehouseType => SN_Type == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;

		protected override ZLong DefaultTypeRangeMaxCore
		{
			get
			{
				return IsFTZNumberType
			? GetMaximumValueForFtz()
			: (IsSSCCNumberType ? CalculatedMaximumValueForSSCCNumbers : Schema.DefaultFountainMaximumValue);
			}
		}

		long CalculatedMaximumValueForSSCCNumbers
		{
			get
			{
				var maxLengthOfValue = SSCCBarCodeChecker.GetFormatDigitsFromPrefix(SN_Prefix);
				return (long)Math.Pow(10, maxLengthOfValue) - 1;
			}
		}

		public override ZString SN_Type
		{
			get { return base.SN_Type; }
			set
			{
				var oldValue = SN_Type;
				base.SN_Type = value;
				if (!IsCopying && oldValue != SN_Type)
				{
					UpdatePrefixFromType();
					UpdateCanRolloverFromType();
				}
			}
		}

		void UpdatePrefixFromType()
		{
			if (!IsFTZNumberType)
			{
				SN_ZoneIDPrefix = string.Empty;
				SN_ClientPrefix = string.Empty;

				if (SN_Type == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers)
				{
					SN_Prefix = string.Empty;
				}
			}
		}

		protected override bool SN_Prefix_ReadOnly
		{
			get { return IsInDatabase || SN_Type == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers; }
		}

		void UpdatePrefixFromZoneIDAndClientPrefix()
		{
			if (!IsSetNameAndPrefixInProgress)
			{
				SN_Prefix = SN_ZoneIDPrefix + SN_ClientPrefix;
			}
		}

		void UpdateCanRolloverFromType()
		{
			SN_CanRollover = IsSSCCNumberType;
			SN_CanRolloverInfo.RefreshBinding();
		}

		[MaxLength(Schema.SN_ZoneIDPrefixMaxLength)]
		[ResourceStringData("OrganisationViewStmNums|SN_ZoneIDPrefix", Caption = "Zone ID. Prefix")]
		public ZString SN_ZoneIDPrefix
		{
			get { return zoneIDPrefix; }
			set
			{
				if (SN_ZoneIDPrefix != value)
				{
					SetNonPersistentPropertyValue(SN_ZoneIDPrefixInfo, ref zoneIDPrefix, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSN_ZoneIDPrefix();
					}
					UpdatePrefixFromZoneIDAndClientPrefix();
				}
			}
		}
		ZString zoneIDPrefix;

		public ZPropertyInfo SN_ZoneIDPrefixInfo
		{
			get { return GetZPropertyInfo(Schema.SN_ZoneIDPrefix); }
		}

		protected bool SN_ZoneIDPrefix_ReadOnly
		{
			get { return IsInDatabase; }
		}

		[MaxLength(Schema.SN_ClientPrefixMaxLength)]
		[ResourceStringData("OrganisationViewStmNums|SN_ClientPrefix", Caption = "Client Prefix")]
		public ZString SN_ClientPrefix
		{
			get { return clientPrefix; }
			set
			{
				if (SN_ClientPrefix != value)
				{
					SetNonPersistentPropertyValue(SN_ClientPrefixInfo, ref clientPrefix, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSN_ClientPrefix();
					}
					UpdatePrefixFromZoneIDAndClientPrefix();
				}
			}
		}
		ZString clientPrefix;

		public ZPropertyInfo SN_ClientPrefixInfo
		{
			get { return GetZPropertyInfo(Schema.SN_ClientPrefix); }
		}

		protected bool SN_ClientPrefix_ReadOnly
		{
			get { return IsInDatabase; }
		}

		public override ZString SN_Prefix
		{
			get => base.SN_Prefix;
			set
			{
				var oldValue = SN_Prefix;
				base.SN_Prefix = value;
				if (!IsCopying && oldValue != SN_Prefix)
				{
					if (IsSSCCNumberType)
					{
						UpdateMaxiumValueIfNeeded();
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		public override BusinessObject Owner
		{
			get
			{
				return Header;
			}
		}

		public OrgHeader Header
		{
			get { return Factory.Load<OrgHeader>(SN_Owner); }
		}

		#endregion

		public override bool CanDelete
		{
			get { return base.CanDelete && !HaveStmNumberRangeMatchingDetails(); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("OrganisationViewStmNums|PrefixHasBeenUsedInMatchingDetails", "Range with prefix '{0}' has Matching Details setup against it.", SN_Prefix); }
		}

		bool HaveStmNumberRangeMatchingDetails()
		{
			return NumberRangeDetail != null;
		}

		#region HasStmNumberRangeMatchingDetails

		public StmNumberRangeMatchingDetail NumberRangeDetail
		{
			get { return GetNumberRangeDetails().FirstOrDefault(); }
		}

		public StmNumberRangeMatchingDetail[] GetNumberRangeDetails()
		{
			if (numberRangeDetailsCached == null)
			{
				numberRangeDetailsCached = new CachedProperty<StmNumberRangeMatchingDetail[]>(Factory, () =>
				{
					var query = new ZQuery(StmNumberRangeMatchingDetailSchema.NRM_OwnerId, SN_Owner);
					query.AddToFilter(StmNumberRangeMatchingDetailSchema.NRM_Prefix, SN_Prefix);
					query.AddToFilter(StmNumberRangeMatchingDetailSchema.NRM_RangeType, SN_Type);
					var result = Factory.Load<StmNumberRangeMatchingDetail>(query).OrderBy(x => x.PK).ToArray();
					return result ?? Array.Empty<StmNumberRangeMatchingDetail>();
				});
			}
			return numberRangeDetailsCached.Value;
		}
		CachedProperty<StmNumberRangeMatchingDetail[]> numberRangeDetailsCached;

		#endregion

		#region Number Fountain
		long GetMaximumValueForFtz()
		{
			return IsFTZWarehouseType
				? Schema.DefaultFTZMaximumValue_Warehouse
				: Schema.DefaultFTZMaximumValue_NonWarehouse;
		}

		protected override FormattedNumberFountainFactory GetNumberFountainFactory(BusinessObject ownerBizObj)
		{
			FormattedNumberFountainFactory fountainFactory = null;
			switch (SN_Type)
			{
				case OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers:
					{
						if (!SN_Prefix.IsEmpty && Enterprise.NumberFountain.SSCCBarCodeChecker.IsSSCCBarCodePrefix(SN_Prefix))
						{
							fountainFactory = new SSCCBarCodeNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, (long)SN_MinimumValue, (long)SN_MaximumValue);
						}
						break;
					}

				case OrgConstants.NumberFountains.Code.ForwardAirBillNumbers:
					{
						fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, (long)SN_MinimumValue, (long)SN_MaximumValue);
						break;
					}

				case OrgConstants.NumberFountains.Code.TransportReferenceNumbers:
					{
						fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, (long)SN_MinimumValue, (long)SN_MaximumValue, CalculateRequiredDigit((long)SN_MaximumValue));
						break;
					}

				case OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber:
					{
						if (!SN_Prefix.IsEmpty)
						{
							fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, Schema.MinimumValue, GetMaximumValueForFtz(), Schema.FTZFormatDigits); // 5 digits
						}
						break;
					}
				case OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse:
					{
						if (!SN_Prefix.IsEmpty)
						{
							fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, Schema.MinimumValue, GetMaximumValueForFtz(), Schema.FTZControlNumberLength); // all 8 digits
						}
						break;
					}
			}

			return fountainFactory;
		}

		#region CalculateRequiredDigit

		int CalculateRequiredDigit(long value)
		{
			return value.ToString(Culture.Invariant).Length;
		}

		#endregion

		#endregion
	}
}
