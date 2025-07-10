using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentForMessaging : NonPersistentBusinessObject<CusUSLVConsignmentForMessagingValidation>
	{
		public CusUSLVConsignmentForMessaging(CusUSLVConsignment consignment)
			: base(consignment.Factory)
		{
			Consignment = consignment;
		}

		public CusUSLVConsignment Consignment { get; }

		public static class Schema
		{
			public const string HouseBill = nameof(HouseBill);
			public const string SendToCustoms = nameof(SendToCustoms);
			public const string ReasonCode = nameof(ReasonCode);
			public const string RequiredReference = nameof(RequiredReference);
			public const string ReferenceNumber = nameof(ReferenceNumber);
			public const string FilesSubmittedToDIS = nameof(FilesSubmittedToDIS);
			public const string DISReference = nameof(DISReference);
		}

		public ZString HouseBill => Consignment.ULB_HouseBill;

		public ZWrappedPropertyInfo HouseBillInfo => GetWrappedZPropertyInfo(nameof(HouseBill), _ => Consignment.ULB_HouseBillInfo);

		public ZBool SendToCustoms
		{
			get => sendToCustoms;
			set
			{
				SetNonPersistentPropertyValue(SendToCustomsInfo, ref sendToCustoms, value);
				Validation.ValidateSendToCustoms();
			}
		}
		ZBool sendToCustoms;

		public ZPropertyInfo SendToCustomsInfo => GetZPropertyInfo(Schema.SendToCustoms);

		#region ReasonCode

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentForMessagingLookups.ReasonCodeList))]
		[MaxLength(2)]
		public ZString ReasonCode
		{
			get => reasonCode;
			set
			{
				SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value);
				UpdateRequiredReferenceBasedOnReasonCode();
				ClearReferenceNumberBasedOnReasonCode();
			}
		}
		ZString reasonCode;

		public ZPropertyInfo ReasonCodeInfo => GetZPropertyInfo(Schema.ReasonCode);

		#endregion

		#region Required Reference

		[ReadOnly(true)]
		public ZString RequiredReference
		{
			get => requiredReference;
			set => SetNonPersistentPropertyValue(RequiredReferenceInfo, ref requiredReference, value);
		}
		ZString requiredReference;

		public ZPropertyInfo RequiredReferenceInfo => GetZPropertyInfo(Schema.RequiredReference);

		void UpdateRequiredReferenceBasedOnReasonCode()
		{
			switch (ReasonCode)
			{
				case ReasonCodeList.Codes.EntryReplacedBy7512:
					RequiredReference = ResString.GetMultilingualString("872603cd-2a38-44f6-9a4d-7d4968972298", "Replacement In-Bond Number");
					break;
				case ReasonCodeList.Codes.MerchandiseClearedByAnother:
					RequiredReference = ResString.GetMultilingualString("c06c7522-7291-43ea-adab-5b422cc715d6", "Replacement Entry Number");
					break;
				case ReasonCodeList.Codes.EntryReplacedByFTZ:
					RequiredReference = ResString.GetMultilingualString("2fcab806-d408-4a83-bd3f-69b8cb35ca65", "Replacement FTZ ADM. Num.");
					break;
				default:
					RequiredReference = string.Empty;
					break;
			}
		}

		#endregion

		#region ReferenceNumber

		[ReadOnlyMember(nameof(ReferenceNumberNotRequired))]
		public ZString ReferenceNumber
		{
			get => referenceNumber;
			set => SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value);
		}
		ZString referenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		bool ReferenceNumberNotRequired => !ReasonCodeList.IsReferenceNoRequired(ReasonCode);

		void ClearReferenceNumberBasedOnReasonCode()
		{
			if (ReferenceNumberNotRequired)
			{
				ReferenceNumber = string.Empty;
			}
		}

		#endregion

		#region FilesSubmittedToDIS

		public ZBool FilesSubmittedToDIS
		{
			get => filesSubmittedToDIS ?? Consignment.ULB_DISIndicator;
			set
			{
				SetNonPersistentPropertyValue(FilesSubmittedToDISInfo, ref filesSubmittedToDIS, value);
				ClearDISReferenceIfRequired();
			}
		}
		ZBool? filesSubmittedToDIS;

		public ZPropertyInfo FilesSubmittedToDISInfo => GetZPropertyInfo(Schema.FilesSubmittedToDIS);

		#endregion

		#region DISReference

		[ReadOnlyMember(nameof(FilesNotSubmittedToDIS))]
		[MaxLength(50)]
		public ZString DISReference
		{
			get => dISReference;
			set => SetNonPersistentPropertyValue(DISReferenceInfo, ref dISReference, value);
		}
		ZString dISReference;

		public ZPropertyInfo DISReferenceInfo => GetZPropertyInfo(Schema.DISReference);

		void ClearDISReferenceIfRequired()
		{
			if (FilesNotSubmittedToDIS)
			{
				DISReference = string.Empty;
			}
		}

		bool FilesNotSubmittedToDIS => !FilesSubmittedToDIS;

		#endregion

		#region Lookups

		public CusUSLVConsignmentForMessagingLookups Lookups => lookups ?? (lookups = new CusUSLVConsignmentForMessagingLookups(this));
		CusUSLVConsignmentForMessagingLookups lookups;

		#endregion

		#region Validation

		public override CusUSLVConsignmentForMessagingValidation GetNewValidation() => new CusUSLVConsignmentForMessagingValidation(this);

		#endregion
	}
}
