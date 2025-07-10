
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	[CodeProperty(JobConsolidatedTransportBookingSchema.Constants.D1_UniqueConsignRef), DescriptionProperty(JobConsolidatedTransportBookingSchema.Constants.D1_BookingReference)]
	public class CommonConsolidatedTransportBooking : AutoJobConsolidatedTransportBooking, IEDocsProvider
	{
		public CommonConsolidatedTransportBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			D1_BookingDate = ZDateTime.Now;
		}

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				D1_UniqueConsignRef = ZString.Empty;
			}
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			PopulateD1_UniqueConsignRefIfNeeded();
			base.OnSaving();
		}

		#endregion

		#region ToString()

		public override string ToString()
		{
			return D1_UniqueConsignRef;
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return D1_BookingReference.IsEmpty ?
					Res.GetString("fe49a3ac-a0e5-4860-9888-8f83edf59021", "Transport Booking") :
					Res.GetString("a88da2d4-a8be-44c7-a21d-7056c8507228", "Transport Booking {0}", D1_BookingReference);
			}
		}

		#endregion

		#endregion

		#region Property Overrides

		[List("TransportCoContacts")]
		public override ZString D1_TransportCoBookingContact
		{
			get { return base.D1_TransportCoBookingContact; }
			set { base.D1_TransportCoBookingContact = value; }
		}

		[List("BindToLists.Organisations")]
		public override ZGuid D1_OA_Customer
		{
			get { return base.D1_OA_Customer; }
			set { base.D1_OA_Customer = value; }
		}

		protected override ZAddress GetNewD1_OA_Customer_ZAddress()
		{
			ZAddress result = base.GetNewD1_OA_Customer_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		[List("BindToLists.TransportProviders")]
		public override ZGuid D1_OA_TransportCo
		{
			get { return base.D1_OA_TransportCo; }
			set
			{
				ZGuid oldValue = base.D1_OA_TransportCo;
				base.D1_OA_TransportCo = value;

				if (oldValue != D1_OA_TransportCo)
				{
					DefaultTransportCoOnConfirms(oldValue, D1_OA_TransportCo);
				}
			}
		}

		void DefaultTransportCoOnConfirms(ZGuid originalOA_TransportCo, ZGuid newOA_TransportCo)
		{
			foreach (CommonPickupDeliveryConfirm confirm in Confirms)
			{
				confirm.SetConsolidatedTransportBookingProvider();
			}
		}

		protected override ZAddress GetNewD1_OA_TransportCo_ZAddress()
		{
			ZAddress result = base.GetNewD1_OA_TransportCo_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#region Related Objects

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection Confirms
		{
			get
			{
				if (confirms == null)
				{
					confirms = new CommonPickupDeliveryConfirmCollection(this);
					RegisterEditableChildObject(confirms);
				}
				return confirms;
			}
		}
		CommonPickupDeliveryConfirmCollection confirms;

		#region FilteredCartageLegs

		public CommonPickupDeliveryConfirmCollection FilteredConfirms
		{
			get
			{
				if (filteredConfirms == null)
				{
					filteredConfirms = new CommonPickupDeliveryConfirmCollection(Factory);
					filteredConfirms.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Booking Allocated", "Property", new ZString(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Unallocated)));
					filteredConfirms.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Completed", "Property", new ZString(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Incomplete)));
					ZQuery query = new ZQuery(JobPickupDeliveryConfirmSchema.EU_D1, SQLComparisonOperator.Equal, null);
					query.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, SQLComparisonOperator.Equal, null);
					filteredConfirms.AdditionalFilter = query;
				}
				return filteredConfirms;
			}
		}
		CommonPickupDeliveryConfirmCollection filteredConfirms;

		#endregion
		#endregion

		#region Unique Index Failure Handler

		void PopulateD1_UniqueConsignRefIfNeeded()
		{
			if (D1_UniqueConsignRef.IsEmpty && !IsInDatabase)
			{
				D1_UniqueConsignRef = ConsolidatedTransportBookingNumberFountain.GetNextFormatted(Factory);
			}
		}

		INumberFountainProxy ConsolidatedTransportBookingNumberFountain
		{
			get { return Env.NumberFountains.JobConsolidatedTransportBookingNumber; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new ConsolidatedTransportBookingNumberFountainUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class ConsolidatedTransportBookingNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public ConsolidatedTransportBookingNumberFountainUniqueIndexFailureHandler(CommonConsolidatedTransportBooking consolidatedTransportBooking)
				: base(JobConsolidatedTransportBookingSchema.Constants.Indexes.NR_UX__D1_UniqueConsignRef, consolidatedTransportBooking)
			{
				ConsolidatedTransportBooking = consolidatedTransportBooking;
			}
			readonly CommonConsolidatedTransportBooking ConsolidatedTransportBooking;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return ConsolidatedTransportBooking.ConsolidatedTransportBookingNumberFountain; }
			}
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList TransportCoContacts
		{
			get
			{
				if (transportCoContacts == null || transportCoAddressForContacts != TransportCo)
				{
					transportCoAddressForContacts = TransportCo;
					transportCoContacts = new CodeDescriptionPairList();
					if (TransportCo != null)
					{
						foreach (OrgContact contact in TransportCo.Header.Contacts)
						{
							transportCoContacts.AddPair(contact.OC_ContactName.SubstringSafe(0, 20), contact.OC_JobCategory);
						}
					}
				}
				return transportCoContacts;
			}
		}
		CodeDescriptionPairList transportCoContacts;
		OrgAddress transportCoAddressForContacts;

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new CommonConsolidatedTransportBookingDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new ConsolidatedTransportBookingDocManagerInfo(this, Constants.DocManagerCodes.JobConsolidatedTransportBooking);
				}
				return docManagerInfo;
			}
		}
		ConsolidatedTransportBookingDocManagerInfo docManagerInfo;

		#endregion
	}
}
