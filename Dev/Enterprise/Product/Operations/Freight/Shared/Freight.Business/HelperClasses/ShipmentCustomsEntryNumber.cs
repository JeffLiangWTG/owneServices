using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public abstract class ShipmentCustomsEntryNumber : NonPersistentBusinessObject
	{
		protected ShipmentCustomsEntryNumber(CommonShipment shipment)
			: base(shipment != null ? shipment.Factory : null)
		{
			Argument.NotNull(shipment, "shipment");
			Shipment = shipment;

			creationStackTrace = System.Environment.StackTrace;
		}

		public CommonShipment Shipment { get; private set; }

		#region Schema

		public static class Schema
		{
			public const string EntryType = "EntryType";
			public const string EntryNumber = "EntryNumber";
			public const string IssueDate = "IssueDate";
			public const string ExpiryDate = "ExpiryDate";
		}

		#endregion

		#region EntryType

		[BusinessObjectTestExclude]
		[ResourceStringData("EntryType", Caption = "Entry Type")]
		[List("EntryType_List")]
		[MaxLength("EntryTypeMaxLength")]
		public ZString EntryType
		{
			get { return GetEntryType(); }
			set { SetEntryType(value); }
		}

		protected abstract ZString GetEntryType();

		protected virtual void SetEntryType(ZString value)
		{
			ProcessCustomsEntryNumbersForSaving(value, EntryNumber, IssueDate, ExpiryDate);
		}

		public ZPropertyInfo EntryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.EntryType); }
		}

		protected virtual bool EntryType_ReadOnly
		{
			get { return false; }
		}

		public int EntryTypeMaxLength
		{
			get
			{
				return IsEntryNumberInAustralia
					? CMRExportExemptionCodes.Char4CodeMaxLength
					: CusEntryNumSchema.CE_EntryType.MaxLength;
			}
		}

		protected ZString customsEntryNumberType = ZString.Empty;

		#endregion

		#region EntryNumber

		[ResourceStringData("EntryNumber", Caption = "Entry Number")]
		[MaxLength("EntryNumberMaxLength")]
		public ZString EntryNumber
		{
			get { return GetEntryNumber(); }
			set { SetEntryNumber(value); }
		}

		protected abstract ZString GetEntryNumber();

		protected virtual void SetEntryNumber(ZString value)
		{
			ProcessCustomsEntryNumbersForSaving(EntryType, value, IssueDate, ExpiryDate);
		}

		public ZPropertyInfo EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EntryNumber); }
		}

		protected virtual bool EntryNumber_ReadOnly
		{
			get { return false; }
		}

		public int EntryNumberMaxLength
		{
			get { return CusEntryNumSchema.CE_EntryNum.MaxLength; }
		}

		public virtual bool IsEntryNumberForShipmentBinding
		{
			get;
			set;
		}

		public ZBool IsManyCustomsEntryNumber => (Shipment != null && Shipment.CusEntryNumbers.Count > 1);

		#endregion

		#region IssueDate

		[ResourceStringData("IssueDate", Caption = "Issue Date")]
		public ZDateTime IssueDate
		{
			get { return GetIssueDate(); }
			set { SetIssueDate(value); }
		}

		protected abstract ZDateTime GetIssueDate();

		protected virtual void SetIssueDate(ZDateTime value)
		{
			ProcessCustomsEntryNumbersForSaving(EntryType, EntryNumber, value, ExpiryDate);
		}

		public ZPropertyInfo IssueDateInfo
		{
			get { return GetZPropertyInfo(Schema.IssueDate); }
		}

		protected virtual bool IssueDate_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region ExpiryDate

		[ResourceStringData("ExpiryDate", Caption = "Expiry Date")]
		public ZDateTime ExpiryDate
		{
			get { return GetExpiryDate(); }
			set { SetExpiryDate(value); }
		}

		protected abstract ZDateTime GetExpiryDate();

		protected virtual void SetExpiryDate(ZDateTime value)
		{
			ProcessCustomsEntryNumbersForSaving(EntryType, EntryNumber, IssueDate, value);
		}

		public ZPropertyInfo ExpiryDateInfo
		{
			get { return GetZPropertyInfo(Schema.ExpiryDate); }
		}

		protected virtual bool ExpiryDate_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		public void Reset()
		{
			customsEntryNumberType = ZString.Empty;
		}

		protected abstract CusEntryNumber GetCusEntryNumber();

		readonly string creationStackTrace;

		public CodeDescriptionPairList EntryType_List
		{
			get
			{
				if (Factory == null)
				{
					var errorMessage = $"Shipment null: {Shipment == null}\r\nShipment deleted: {Shipment.IsDeleted}\r\nShipment in database: {Shipment.IsInDatabase}\r\nCreation Stack Trace: {creationStackTrace}";
					ErrorReporter.ReportOnce("Issue01473807Logging", errorMessage);

					return null;
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom)
				{
					return Factory.GetCachedValue(string.Format("ShipmentCustomsEntryNumber.EntryType_List|GB_EU|{0}", Shipment.UseImportEntryTypeList), () =>
					{
						var result = new UntranslatableCodeDescriptionPairList((NoResString)"Country-Specific Customs values");
						result.AddRange(CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Shipment.UseImportEntryTypeList));
						result.AddRangeOverwriteIfExists(CusEntryNumberTypes.EU.EUCustomsEntryTypeList);

						return result;
					});
				}

				return CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.UseImportEntryTypeList);
			}
		}

		void ProcessCustomsEntryNumbersForSaving(ZString entryTypeFromDisplay, ZString entryNumberFromDisplay, ZDateTime issueDateFromDisplay, ZDateTime expiryDateFromDisplay)
		{
			customsEntryNumberType = CusEntryNumber.GetEntryNumberTypeFromDisplay(entryTypeFromDisplay, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.IsImport());

			CusEntryNumber cusEntryNumber = GetCusEntryNumber();

			if (IsExemptionCode(entryTypeFromDisplay) ||
				// in some insanely inconsistent circumstances e.g. TNT client data export the exemption code may be supplied as a 3 digit code
				IsExemptionCode(CusEntryNumber.GetEntryNumberTypeForDisplay(entryTypeFromDisplay, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.IsImport())))
			{
				cusEntryNumber.CE_EntryType = customsEntryNumberType;
				cusEntryNumber.CE_EntryNum = ZString.Empty;
				cusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				cusEntryNumber.CE_ExpiryDate = ZDateTime.Empty;
			}
			else if (!IsNumberEmpty(customsEntryNumberType, entryNumberFromDisplay) || !HandleEmptyNumber(cusEntryNumber))
			{
				cusEntryNumber.CE_EntryType = customsEntryNumberType.IsEmpty ? GetCountrySpecificDefaultType() : customsEntryNumberType;
				cusEntryNumber.CE_EntryNum = entryNumberFromDisplay;
				cusEntryNumber.CE_IssueDate = issueDateFromDisplay;
				cusEntryNumber.CE_ExpiryDate = expiryDateFromDisplay;
			}

			EntryTypeInfo.RefreshBinding();
			EntryNumberInfo.RefreshBinding();
			IssueDateInfo.RefreshBinding();
			ExpiryDateInfo.RefreshBinding();

			if (!IsValidationSuspended)
			{
				Validation.ValidateEntryType();
				Validation.ValidateEntryNumber();
				Validation.ValidateIssueDate();
				Validation.ValidateExpiryDate();
			}

			Shipment.NotifyElementChanged();
		}

		public bool ShouldDeleteEmptyNumber(CusEntryNumber entryNumber)
		{
			return ShouldDeleteEmptyNumber(entryNumber.CE_EntryType, entryNumber.CE_EntryNum);
		}

		bool ShouldDeleteEmptyNumber(ZString entryType, ZString entryNumber)
		{
			return !IsExemptionCode(entryType) && IsNumberOrTypeEmpty(entryType, entryNumber);
		}

		protected bool IsNumberOrTypeEmpty(ZString entryType, ZString entryNumber)
		{
			bool isCountrySpecificDefaultType() => entryType == GetCountrySpecificDefaultType();

			return entryType.IsEmpty
				|| DeleteCustomsEntriesIfNumberIsEmpty && entryNumber.IsEmpty && (IsEntryNumberInAustralia || isCountrySpecificDefaultType());
		}

		public bool IsNumberEmpty(ZString entryType, ZString entryNumber)
		{
			bool isCountrySpecificDefaultType() => entryType == GetCountrySpecificDefaultType();

			return (entryNumber.IsEmpty &&
					(entryType.IsEmpty || DeleteCustomsEntriesIfNumberIsEmpty && (IsEntryNumberInAustralia || isCountrySpecificDefaultType())));
		}

		ZString GetCountrySpecificDefaultType()
		{
			return CusEntryNumberTypes.CountrySpecificDefaultEntryNumberType(
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
					Shipment.IsImport());
		}

		protected virtual bool HandleEmptyNumber(CusEntryNumber entryNumber)
		{
			bool handled = false;

			if (entryNumber != null)
			{
				entryNumber.Delete();
				handled = true;
			}

			return handled;
		}

		public bool IsExemptionCode(ZString entryNumberType)
		{
			return IsEntryNumberInAustralia && CusEntryNumberTypes.IsExemptionCode(entryNumberType);
		}

		protected virtual bool DeleteCustomsEntriesIfNumberIsEmpty
		{
			get { return true; }
		}

		protected virtual bool IsEntryNumberInAustralia
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia && !Shipment.IsImport(); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ShipmentCustomsEntryNumberValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ShipmentCustomsEntryNumberValidation GetNewValidation()
		{
			return new ShipmentCustomsEntryNumberValidation(this);
		}

		#endregion
	}
}
