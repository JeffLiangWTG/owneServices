using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestQueryBizObj : NonPersistentBusinessObject
		, IObsoleteValidation
		, ICargoManifestQuerySendingObject
	{
		public CargoManifestQueryBizObj(CargoManifestQueryHeader header)
			: base(new BusinessObjectFactory())
		{
			this.header = header;
			OutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
		}

		readonly CargoManifestQueryHeader header;

		public static class Schema
		{
			public const string Issuer = "Issuer";
			public const int IssuerMaxLength = 4;
			public const string MasterBillNumber = "MasterBillNumber";
			public const string HouseBillNumber = "HouseBillNumber";
			public const int HouseBillNumberMaxLength = 12;
			public const string InBondNumber = "InBondNumber";
			public const int InBondNumberMaxLength = 12;
			public const string RequestForRelatedBills = "RequestForRelatedBills";
			public const string OutputOption = "OutputOption";
		}

		#region Properties

		[MaxLength(Schema.InBondNumberMaxLength)]
		public ZString InBondNumber
		{
			get { return inBondNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(InBondNumberInfo, ref inBondNumber, value, false))
				{
					ValidateInBondNumber();
				}
			}
		}
		ZString inBondNumber;

		public ZPropertyInfo InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InBondNumber); }
		}

		public void ValidateInBondNumber()
		{
			if (!IsValidationSuspended)
			{
				InBondNumberInfo.ClearAllNotifications();

				if (header.IsInBondNumberVisible)
				{
					MandatoryValidation.MessageErrorIfNotEntered(InBondNumberInfo, "In-Bond Number");
				}
			}
		}

		[MaxLength(Schema.IssuerMaxLength)]
		public ZString Issuer
		{
			get { return issuer; }
			set
			{
				if (SetNonPersistentPropertyValue(IssuerInfo, ref issuer, value, false))
				{
					ValidateIssuer();
				}
			}
		}
		ZString issuer;

		public ZPropertyInfo IssuerInfo
		{
			get { return GetZPropertyInfo(Schema.Issuer); }
		}

		public void ValidateIssuer()
		{
			if (!IsValidationSuspended)
			{
				IssuerInfo.ClearAllNotifications();

				if (header.IsIssuerVisible)
				{
					MandatoryValidation.MessageErrorIfNotEntered(IssuerInfo, "Issuer SCAC Code");
				}
			}
		}

		[MaxLength(nameof(MasterBillNumber_MaxLength))]
		public ZString MasterBillNumber
		{
			get { return masterBillNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillNumberInfo, ref masterBillNumber, value, false))
				{
					ValidateMasterBillNumber();
				}
			}
		}
		ZString masterBillNumber;

		public int MasterBillNumber_MaxLength
		{
			get { return header.ActionCode == CargoManifestStatusQueryActionList.Codes.AIR ? 11 : 12; }
		}

		public ZPropertyInfo MasterBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillNumber); }
		}

		public void ValidateMasterBillNumber()
		{
			if (!IsValidationSuspended)
			{
				MasterBillNumberInfo.ClearAllNotifications();

				if (header.ActionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill)
				{
					MandatoryValidation.MessageErrorIfNotEntered(MasterBillNumberInfo, "Bill Number");
				}
				else if (header.ActionCode == CargoManifestStatusQueryActionList.Codes.AIR)
				{
					MandatoryValidation.MessageErrorIfNotEntered(MasterBillNumberInfo, "Master Bill Number");
				}
			}
		}

		[MaxLength(Schema.HouseBillNumberMaxLength)]
		public ZString HouseBillNumber
		{
			get { return houseBillNumber; }
			set
			{
				SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value);
			}
		}
		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBillNumber); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.CargoManifestQueryBizObj|RequestForRelatedBills", Caption = "Request For Related Bills")]
		public ZBool RequestForRelatedBills
		{
			get { return requestForRelatedBills; }
			set { SetNonPersistentPropertyValue(RequestForRelatedBillsInfo, ref requestForRelatedBills, value); }
		}
		ZBool requestForRelatedBills;

		public ZPropertyInfo RequestForRelatedBillsInfo
		{
			get { return GetZPropertyInfo(Schema.RequestForRelatedBills); }
		}

		#region Limit Output

		[MaxLength(21)]
		[List(nameof(OutputCodes))]
		[ResourceStringData("Enterprise.Customs.US.Business.CargoManifestQueryBizObj|OutputOption", Caption = "Output Option")]
		public ZString OutputOption
		{
			get { return outputOption; }
			set
			{
				if (SetNonPersistentPropertyValue(OutputOptionInfo, ref outputOption, value, false))
				{
					ValidateLimitOutputOption();
				}
			}
		}
		ZString outputOption;

		public ZPropertyInfo OutputOptionInfo
		{
			get { return GetZPropertyInfo(Schema.OutputOption); }
		}

		public CodeDescriptionPairList OutputCodes
		{
			get { return header.Factory.GetCachedValue<LimitOutputCodeList>(); }
		}

		public void ValidateLimitOutputOption()
		{
			if (!IsValidationSuspended)
			{
				OutputOptionInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(OutputOptionInfo, OutputCodes);
			}
		}

		#endregion

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateInBondNumber();
			ValidateIssuer();
			ValidateMasterBillNumber();
			ValidateLimitOutputOption();
		}

		#endregion

		#region ICargoManifestQuerySendingObject Members

		ZString ICargoManifestQuerySendingObject.ActionCode
		{
			get { return header.ActionCode; }
		}

		BusinessObjectFactory ICargoManifestQuerySendingObject.Factory
		{
			get { return Factory; }
		}

		ZString ICargoManifestQuerySendingObject.MasterBillNumber
		{
			get { return MasterBillNumber; }
		}

		ZString ICargoManifestQuerySendingObject.HouseBillNumber
		{
			get { return HouseBillNumber; }
		}

		void ICargoManifestQuerySendingObject.LinkToMessage(EDIMessage message)
		{
			//this is non-persistent and do not link at all
		}

		ZString ICargoManifestQuerySendingObject.BillIssuerCode
		{
			get { return Issuer; }
		}

		ZString ICargoManifestQuerySendingObject.EntryOrInBondNumber
		{
			get { return InBondNumber; }
		}

		ZBool ICargoManifestQuerySendingObject.UpdateEntryWithResults
		{
			get { return false; }
		}

		ZBool ICargoManifestQuerySendingObject.RequestForRelatedBOL => RequestForRelatedBills;

		ZString ICargoManifestQuerySendingObject.LimitOutputOption => OutputOption;

		#endregion
	}
}
