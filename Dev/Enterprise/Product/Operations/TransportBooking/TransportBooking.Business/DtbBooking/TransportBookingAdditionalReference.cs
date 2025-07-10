using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.TransportCommon;

namespace Enterprise.TransportBookings.Business
{
	public class TransportBookingAdditionalReference : NonPersistentBusinessObject
	{
		public TransportBookingAdditionalReference(DtbBooking booking, Customs.ICusEntryNumber additionalReference)
			: base()
		{
			this.booking = Argument.NotNull(booking, "booking");
			this.AdditionalReferenceWrappedObject = Argument.NotNull(additionalReference, "additionalReference");
		}

		void OnAdditionalReferenceDeletedByDataRefresh(object sender, EventArgs e)
		{
			if (!IsDeleted && sender is BusinessObject businessObject && businessObject.IsDeleted)
			{
				Delete();
				foreach (var parentCollection in ParentCollections.ToList())
				{
					if (parentCollection.Contains(this))
					{
						parentCollection.Remove(this);
					}
				}
			}
		}

		readonly DtbBooking booking;

		Customs.ICusEntryNumber additionalReference;

		Customs.ICusEntryNumber AdditionalReferenceWrappedObject
		{
			get { return additionalReference; }
			set
			{
				if (additionalReference is BusinessObject oldBusinessObject)
				{
					oldBusinessObject.DeletedByDataRefresh -= OnAdditionalReferenceDeletedByDataRefresh;
				}

				additionalReference = value;

				if (additionalReference is BusinessObject newBusinessObject)
				{
					newBusinessObject.DeletedByDataRefresh += OnAdditionalReferenceDeletedByDataRefresh;
				}
			}
		}

		public ZString AdditionalReferenceNumberTypeDescription
		{
			get { return AdditionalReferenceWrappedObject.AdditionalReferenceNumberTypeDescription; }
		}

		public ZWrappedPropertyInfo AdditionalReferenceNumberTypeDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AdditionalReferenceNumberTypeDescription), x => AdditionalReferenceWrappedObject.AdditionalReferenceNumberTypeDescriptionInfo); }
		}

		[ResourceStringData("TransportBookingAdditionalReference|EntryLineReference", Caption = "Additional Information", ShortCaption = "Information",
			MediumCaption = "Information", FullDescription = "Enter any additional information related to this Reference Number. Example: Enter the location it was issued.")]
		public ZString EntryLineReference
		{
			get { return AdditionalReferenceWrappedObject.CE_EntryLineReference; }
			set { AdditionalReferenceWrappedObject.CE_EntryLineReference = value; }
		}

		public ZWrappedPropertyInfo EntryLineReferenceInfo { get { return GetWrappedZPropertyInfo(nameof(EntryLineReference), x => AdditionalReferenceWrappedObject.CE_EntryLineReferenceInfo); } }

		[ResourceStringData("TransportBookingAdditionalReference|EntryNum", Caption = "Reference Number", ShortCaption = "Number", FullDescription = "Number")]
		public ZString EntryNum
		{
			get { return AdditionalReferenceWrappedObject.CE_EntryNum; }
			set { AdditionalReferenceWrappedObject.CE_EntryNum = value; }
		}

		public ZWrappedPropertyInfo EntryNumInfo { get { return GetWrappedZPropertyInfo(nameof(EntryNum), x => AdditionalReferenceWrappedObject.CE_EntryNumInfo); } }

		[ResourceStringData("TransportBookingAdditionalReference|IssueDate", Caption = "Number Issue Date", ShortCaption = "Issued", MediumCaption = "Issue Date", FullDescription = "Enter the Date the Reference Number was issued.")]
		public ZDateTime IssueDate
		{
			get { return AdditionalReferenceWrappedObject.CE_IssueDate; }
			set { AdditionalReferenceWrappedObject.CE_IssueDate = value; }
		}

		public ZWrappedPropertyInfo IssueDateInfo { get { return GetWrappedZPropertyInfo(nameof(IssueDate), x => AdditionalReferenceWrappedObject.CE_IssueDateInfo); } }

		[ResourceStringData("TransportBookingAdditionalReference|EntryType", Caption = "Number Type", ShortCaption = "Type", FullDescription = "Indicates the Type of Reference Number.")]
		[List("Lookups.AdditionalReferenceNumberTypes")]
		public ZString EntryType
		{
			get { return AdditionalReferenceWrappedObject.CE_EntryType; }
			set { AdditionalReferenceWrappedObject.CE_EntryType = value; }
		}

		public ZWrappedPropertyInfo EntryTypeInfo { get { return GetWrappedZPropertyInfo(nameof(EntryType), x => AdditionalReferenceWrappedObject.CE_EntryTypeInfo); } }

		[ResourceStringData("TransportBookingAdditionalReference|RN_NKCountryCode", Caption = "Country/Region Of Issue", ShortCaption = "Country/Region")]
		[List("Lookups.Countries")]
		public ZString RN_NKCountryCode
		{
			get { return AdditionalReferenceWrappedObject.CE_RN_NKCountryCode; }
			set { AdditionalReferenceWrappedObject.CE_RN_NKCountryCode = value; }
		}

		public ZWrappedPropertyInfo RN_NKCountryCodeInfo { get { return GetWrappedZPropertyInfo(nameof(RN_NKCountryCode), x => AdditionalReferenceWrappedObject.CE_RN_NKCountryCodeInfo); } } // Proxy property to CE_RN_NKCountryCodeInfo

		public Customs.ICusEntryNumLookups Lookups
		{
			get { return AdditionalReferenceWrappedObject.Lookups; }
		}

		[ResourceStringData("TransportBookingAdditionalReference|BookingOnly", Caption = "This TB Only", ShortCaption = "TB Only")]
		public ZBool BookingOnly
		{
			get { return booking.AdditionalReferenceNumbers.Contains(AdditionalReferenceWrappedObject); }
			set
			{
				if (value && !booking.AdditionalReferenceNumbers.Contains(AdditionalReferenceWrappedObject) && booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Contains(AdditionalReferenceWrappedObject))
				{
					SwapAdditionalReference(booking.ConsolidationSingleJob, booking);
				}
				else if (!value && booking.AdditionalReferenceNumbers.Contains(AdditionalReferenceWrappedObject) && !booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Contains(AdditionalReferenceWrappedObject))
				{
					SwapAdditionalReference(booking, booking.ConsolidationSingleJob);
				}
				BookingOnlyInfo.RefreshBinding();
			}
		}

		void SwapAdditionalReference(ITransportAdditionalReferenceNumbers transportParentToRemoveAdditionalReference, ITransportAdditionalReferenceNumbers transportParentToAddAdditionalReference)
		{
			var newAdditionalReference = transportParentToAddAdditionalReference.AdditionalReferenceNumbers.AddNew();
			var oldAdditionalReference = AdditionalReferenceWrappedObject;
			CopyAdditionalReferenceData(newAdditionalReference, oldAdditionalReference);
			AdditionalReferenceWrappedObject = newAdditionalReference;
			transportParentToRemoveAdditionalReference.AdditionalReferenceNumbers.RemoveAndDelete(oldAdditionalReference);
		}

		void CopyAdditionalReferenceData(Customs.ICusEntryNumber newAdditionalReference, Customs.ICusEntryNumber additionalReferenceToCopyValueFrom)
		{
			newAdditionalReference.CE_Category = additionalReferenceToCopyValueFrom.CE_Category;
			newAdditionalReference.CE_EntryLineReference = additionalReferenceToCopyValueFrom.CE_EntryLineReference;
			newAdditionalReference.CE_EntryNum = additionalReferenceToCopyValueFrom.CE_EntryNum;
			newAdditionalReference.CE_EntryType = additionalReferenceToCopyValueFrom.CE_EntryType;
			newAdditionalReference.CE_IssueDate = additionalReferenceToCopyValueFrom.CE_IssueDate;
			newAdditionalReference.CE_RN_NKCountryCode = additionalReferenceToCopyValueFrom.CE_RN_NKCountryCode;
		}

		public ZPropertyInfo BookingOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(BookingOnly)); }
		}

		public override void Delete()
		{
			AdditionalReferenceWrappedObject.Delete();
			base.Delete();
		}
	}
}
