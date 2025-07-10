using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class AsycudaBill
	{
		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string SG_PartyID = "SG_PartyID";
			public const string SG_PartyStatus = "SG_PartyStatus";
			public const string SG_PayeeIndicator = "SG_PayeeIndicator";
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
			public const string BatchDate = "BatchDate";
			public const string BatchNumber = "BatchNumber";
			public const string GSTNReferenceNo = "GSTNReferenceNo";
			public const int SG_PartyIDMaxLength = 20;
			public const int SG_PartyStatusMaxLength = 1;
			public const int SG_PayeeIndicatorMaxLength = 1;
			public const int CycleNumberMaxLength = 10;
			public const int BatchNumberMaxLength = 10;
			public const int GSTNReferenceNoMaxLength = 10;
		}

		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;

		#region CycleDate

		[ResourceStringData("SGAsycudaBill.CycleDate", Caption = "Cycle Date", ShortCaption = "Cycle Date")]
		[ReadOnlyMember(nameof(CycleFields_Readonly))]
		public ZDateTime CycleDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.CycleDate);
			set
			{
				var oldValue = CycleDate;
				this.SetSystemDefinedValue(Schema.CycleDate, value.Date.ToZDateTime());
				CycleDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateCycleDate();
				}
			}
		}

		public ZPropertyInfo CycleDateInfo => GetZPropertyInfo(nameof(CycleDate));

		#endregion

		#region CycleNumber

		[ResourceStringData("SGAsycudaBill.CycleNumber", Caption = "Cycle Number", ShortCaption = "Cycle No.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CycleNumbers))]
		[ReadOnlyMember(nameof(CycleFields_Readonly))]
		[MaxLength(Schema.CycleNumberMaxLength)]
		public ZString CycleNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CycleNumber);
			set
			{
				var oldValue = CycleNumber;
				CheckMaximumLength(CycleNumberInfo, value);
				this.SetSystemDefinedValue(Schema.CycleNumber, value);
				CycleNumberInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateCycleNumber();
				}
			}
		}

		public ZPropertyInfo CycleNumberInfo => GetZPropertyInfo(Schema.CycleNumber);

		#endregion

		public ZBool CycleFields_Readonly => RegistrationEntryNumber != null;

		#region BatchDate & BatchNumber

		public ZDateTime BatchDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.BatchDate);
			set
			{
				var oldValue = BatchDate;
				this.SetSystemDefinedValue(Schema.BatchDate, value);
				BatchDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BatchDateInfo => GetZPropertyInfo(Schema.BatchDate);

		[MaxLength(Schema.BatchNumberMaxLength)]
		public ZString BatchNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.BatchNumber);
			set
			{
				var oldValue = BatchNumber;
				CheckMaximumLength(BatchNumberInfo, value);
				this.SetSystemDefinedValue(Schema.BatchNumber, value);
				BatchNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BatchNumberInfo => GetZPropertyInfo(Schema.BatchNumber);

		#endregion

		public ZString ShortStatusDescription
		{
			get
			{
				var result = StatusDescription;

				if (ABL_MessageStatus == MessageStatusCodeList.Codes.Error)
				{
					result = GetShortErrorMessageStatusDescription(result);
				}
				return result;
			}
		}

		public ZPropertyInfo ShortStatusDescriptionInfo => GetZPropertyInfo(nameof(ShortStatusDescription));

		ZString GetShortErrorMessageStatusDescription(string codeAndDescription)
		{
			var result = "ERROR RECEIVED";
			if (codeAndDescription.StartsWith("ERR - ", StringComparison.CurrentCultureIgnoreCase))
			{
				var description = codeAndDescription.Substring(6);
				var splitDescriptions = description.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
				switch (splitDescriptions.Length)
				{
					case 0:
						result = description;
						break;
					case 1:
						result = splitDescriptions[0];
						break;
					default:
						result = splitDescriptions[0] + "...";
						break;
				}
			}
			return result;
		}

		public override ZString ABL_ShipmentType
		{
			get => base.ABL_ShipmentType;
			set
			{
				var oldValue = ABL_ShipmentType;
				base.ABL_ShipmentType = value;
				if (!IsCopying && oldValue != ABL_ShipmentType)
				{
					DefaultSGDataIfNeeded();
				}
			}
		}

		protected override bool ABL_ShipmentType_ReadOnly => true;

		[MaxLength(Schema.SG_PartyIDMaxLength)]
		[ReadOnlyMember(nameof(SG_PartyID_ReadOnly))]
		[ResourceStringData("SGAsycudaBill.SG_PartyID", Caption = "Party Identifier")]
		public ZString SG_PartyID
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SG_PartyID);
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AsycudaBill.Schema.SG_PartyID))
				{
					var oldValue = SG_PartyID;
					CheckMaximumLength(SG_PartyIDInfo, value);
					this.SetSystemDefinedValue(Schema.SG_PartyID, value);
					SG_PartyIDInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SG_PartyIDInfo => GetZPropertyInfo(Schema.SG_PartyID);

		protected virtual bool SG_PartyID_ReadOnly => !IsImport && !IsExport;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SGPartyStatusList))]
		[MaxLength(Schema.SG_PartyStatusMaxLength)]
		[ReadOnlyMember(nameof(SG_PartyStatus_ReadOnly))]
		[ResourceStringData("SGAsycudaBill.SG_PartyStatus", Caption = "Party Status")]
		public ZString SG_PartyStatus
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SG_PartyStatus);
			set
			{
				var oldValue = SG_PartyStatus;
				CheckMaximumLength(SG_PartyStatusInfo, value);
				this.SetSystemDefinedValue(Schema.SG_PartyStatus, value);
				SG_PartyStatusInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateSG_PartyStatus();
				}
				if (IsImport)
				{
					UpdateSGCountryPackGoodsType();
				}
			}
		}

		void UpdateSGCountryPackGoodsType()
		{
			foreach (AsycudaPack pack in Packs)
			{
				var packedItem = pack.PackedItem;
				if (packedItem != null)
				{
					packedItem.DefaultImportGoodsType();
				}
			}
		}

		public ZPropertyInfo SG_PartyStatusInfo => GetZPropertyInfo(Schema.SG_PartyStatus);

		protected virtual bool SG_PartyStatus_ReadOnly => !IsImport;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SGPayeeIndicatorList))]
		[MaxLength(Schema.SG_PayeeIndicatorMaxLength)]
		[ReadOnlyMember(nameof(SG_PayeeIndicator_ReadOnly))]
		[ResourceStringData("SGAsycudaBill.SG_PayeeIndicator", Caption = "Payee Indicator")]
		public ZString SG_PayeeIndicator
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SG_PayeeIndicator);
			set
			{
				var oldValue = SG_PayeeIndicator;
				CheckMaximumLength(SG_PayeeIndicatorInfo, value);
				this.SetSystemDefinedValue(Schema.SG_PayeeIndicator, value);
				SG_PayeeIndicatorInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateSG_PayeeIndicator();
				}
			}
		}

		public ZPropertyInfo SG_PayeeIndicatorInfo => GetZPropertyInfo(Schema.SG_PayeeIndicator);

		protected virtual bool SG_PayeeIndicator_ReadOnly => !IsImport;

		[MaxLength(Schema.GSTNReferenceNoMaxLength)]
		[ResourceStringData("SGAsycudaBill.GSTNReferenceNo", Caption = "GSTN Ref.", FullDescription = "GSTN Reference No.")]
		public ZString GSTNReferenceNo
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.GSTNReferenceNo); }
			set
			{
				var oldValue = GSTNReferenceNo;
				CheckMaximumLength(GSTNReferenceNoInfo, value);
				this.SetSystemDefinedValue(Schema.GSTNReferenceNo, value);
				GSTNReferenceNoInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateGSTNReferenceNo();
				}
			}
		}

		public ZPropertyInfo GSTNReferenceNoInfo => GetZPropertyInfo(Schema.GSTNReferenceNo);

		protected override bool DutyAmount_ReadOnly => true;
		protected override bool TaxAmount_ReadOnly => true;

		void DefaultSGDataIfNeeded()
		{
			if (IsImport)
			{
				var consignee = Consignee;
				DefaultSG_PartyIDFromOrganisationSG_UEN(consignee);
				DefaultSG_PartyStatus(consignee);
				DefaultSG_PayeeIndicatorFromOrganisationPaymentMethod(consignee);
			}
			else if (IsExport)
			{
				DefaultSG_PartyIDFromOrganisationSG_UEN(Shipper);
				if (!SG_PartyStatus.IsEmpty)
				{
					SG_PartyStatus = ZString.Empty;
				}

				if (!SG_PayeeIndicator.IsEmpty)
				{
					SG_PayeeIndicator = ZString.Empty;
				}

				GSTNReferenceNo = ZString.Empty;
			}
			else
			{
				if (!SG_PartyID.IsEmpty)
				{
					SG_PartyID = ZString.Empty;
				}

				if (!SG_PartyStatus.IsEmpty)
				{
					SG_PartyStatus = ZString.Empty;
				}

				if (!SG_PayeeIndicator.IsEmpty)
				{
					SG_PayeeIndicator = ZString.Empty;
				}

				GSTNReferenceNo = ZString.Empty;
			}
		}

		internal void DefaultSG_PartyIDFromOrganisationSG_UEN(OrgAddress address)
		{
			var uEN = address.GetSGUniqueEntityNumber();
			if (!uEN.IsEmpty && SG_PartyID != uEN)
			{
				SG_PartyID = uEN;
			}
		}

		protected override void DefaultOnCountryChanged()
		{
			DefaultSGDataIfNeeded();
			base.DefaultOnCountryChanged();
		}

		protected override void UpdateChildReadOnlyWhenRegistering(IBusiness child)
		{
			if (child.Identifier != CustomBusinessObject.PK)
			{
				base.UpdateChildReadOnlyWhenRegistering(child);
			}
		}

		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill)
		{
			var header = bill.Header;
			if (header != null)
			{
				ABL_ShipmentType = header.AMA_Nature;
			}
			else
			{
				base.CalculateShipmentTypeCore(bill);
			}
		}

		protected override IEnumerable<string> GetSupportedFields() => base.GetSupportedFields().Union(new[] { AsycudaBill.Schema.SG_PartyID });
	}
}
