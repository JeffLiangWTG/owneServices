using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class CombineBookings : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CombineBookings(QuotedBooking masterQuotedBooking)
			: base(masterQuotedBooking.Factory)
		{
			this.masterQuotedBooking = masterQuotedBooking;
		}

		readonly QuotedBooking masterQuotedBooking;

		public QuotedBooking MasterQuotedBooking
		{
			get { return masterQuotedBooking; }
		}

		public ViewQuotedBookingForCombineCollection OtherViewQuotedBookings
		{
			get
			{
				if (otherViewQuotedBookings == null)
				{
					otherViewQuotedBookings = new ViewQuotedBookingForCombineCollection(MasterQuotedBooking);
				}
				return otherViewQuotedBookings;
			}
		}
		ViewQuotedBookingForCombineCollection otherViewQuotedBookings;

		public ViewQuotedBookingForCombineCollection OtherViewQuotedBookingsLookups
		{
			get
			{
				if (otherViewQuotedBookingsLookups == null)
				{
					otherViewQuotedBookingsLookups = new ViewQuotedBookingForCombineCollection(MasterQuotedBooking);
					var filterProvider = GetDefaultFilterProvider(MasterQuotedBooking);
					filterProvider.SetDefaultFilters(otherViewQuotedBookingsLookups);
				}

				return otherViewQuotedBookingsLookups;
			}
		}
		ViewQuotedBookingForCombineCollection otherViewQuotedBookingsLookups;

		public void Combine(BusinessObjectFactory factory)
		{
			var masterQuotedBookingInNewFactory = factory?.Load<QuotedBooking>(MasterQuotedBooking.PK);
			if (masterQuotedBookingInNewFactory == null)
			{
				return;
			}

			var otherViewQuotedBookingsInNewFactory = factory.Load<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.PK, OtherViewQuotedBookings.Select(x => x.PK)));
			foreach (var otherBooking in otherViewQuotedBookingsInNewFactory)
			{
				Combine(masterQuotedBookingInNewFactory, otherBooking.QuotedBooking);
			}

			masterQuotedBookingInNewFactory.HasChanges = true;
		}

		#region Implementation

		void Combine(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var convertedWeight = Constants.Weight.Convert(otherBooking.Booking.JS_ActualWeight, otherBooking.Booking.JS_UnitOfWeight, masterBooking.Booking.JS_UnitOfWeight);
			var convertedVolume = Constants.Volume.Convert(otherBooking.Booking.JS_ActualVolume, otherBooking.Booking.JS_UnitOfVolume, masterBooking.Booking.JS_UnitOfVolume);

			var newWeight = convertedWeight + masterBooking.Booking.JS_ActualWeight;
			var newVolume = convertedVolume + masterBooking.Booking.JS_ActualVolume;

			CombineAttachedOrders(masterBooking, otherBooking);
			CombineOrderItems(masterBooking, otherBooking);
			CombineContainersAndPacklines(masterBooking, otherBooking);
			CombineNotes(masterBooking, otherBooking);
			CombineServices(masterBooking, otherBooking);
			CombineReferenceNumbers(masterBooking, otherBooking);
			CombineEDocs(masterBooking, otherBooking);

			masterBooking.Booking.JS_ActualWeight = newWeight;
			masterBooking.Booking.JS_ActualVolume = newVolume;

			otherBooking.Booking.JS_IsCancelled = true;
		}

		void CombineContainersAndPacklines(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var copyContainerArgs = new BusinessObjectCloneArgs();
			copyContainerArgs.AddExcludedColumns(new string[]
			{
				JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink,
			});

			var copyPacklineArgs = new BusinessObjectCloneArgs();
			copyPacklineArgs.AddExcludedColumns(new string[]
			{
				JobPackLinesSchema.Constants.JL_JS,
				ForwardingPackLine.Schema.JL_JC,
			});

			var containersNeedClone = otherBooking.QuotedBookingContainers.Cast<ForwardingContainer>()
				.Where(x => x.JC_ContainerNum.IsEmpty || !masterBooking.QuotedBookingContainers.Cast<ForwardingContainer>().Any(y => y.JC_ContainerNum == x.JC_ContainerNum));

			foreach (var container in containersNeedClone)
			{
				var copiedContainer = (CommonContainer)container.Clone(copyContainerArgs);
				masterBooking.QuotedBookingContainers.Add(copiedContainer);
			}

			foreach (var packline in otherBooking.Booking.OuterPackLines)
			{
				var copiedPackLine = (ForwardingPackLine)packline.Clone(copyPacklineArgs);
				masterBooking.Booking.OuterPackLines.Add(copiedPackLine);
			}
		}

		void CombineNotes(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var copyNoteArgs = new BusinessObjectCloneArgs();
			copyNoteArgs.AddExcludedColumns(new string[]
			{
				StmNoteSchema.Constants.ST_ParentID,
				StmNoteSchema.Constants.ST_Table
			});

			foreach (QuotedBookingStmNote note in otherBooking.Notes.GetAllNotes())
			{
				if (masterBooking.Notes.FindByDescription(note.ST_Description).Length == 0 || !IsUniqueNoteForDescription(note))
				{
					var noteCopy = (QuotedBookingStmNote)note.Clone(copyNoteArgs);
					noteCopy.QuotedBooking = masterBooking;

					if (IsSerializableNote(note))
					{
						noteCopy.ST_NoteText = note.GetUnSerializeNoteText();
					}

					masterBooking.Notes.Add(noteCopy);
				}
			}
		}

		void CombineServices(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var copyServiceArgs = new BusinessObjectCloneArgs();
			copyServiceArgs.AddExcludedColumns(new string[]
			{
				JobServiceSchema.Constants.ES_ParentID,
				JobServiceSchema.Constants.ES_ParentTableCode
			});

			foreach (JobService service in otherBooking.Services)
			{
				if (masterBooking.Services.Cast<JobService>().All(x => x.ES_ServiceCode != service.ES_ServiceCode))
				{
					var serviceCopy = (JobService)service.Clone(copyServiceArgs);

					masterBooking.Services.Add(serviceCopy);
				}
			}
		}

		void CombineReferenceNumbers(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var copyCusEntryNumberArgs = new BusinessObjectCloneArgs();
			copyCusEntryNumberArgs.AddExcludedColumns(new string[]
			{
				CusEntryNumSchema.Constants.CE_ParentID,
			});

			foreach (CusEntryNumber number in otherBooking.Booking.Numbers)
			{
				if (CanAddCusEntryNumber(masterBooking.Booking.Numbers, number))
				{
					var numberCopy = (CusEntryNumber)number.Clone(copyCusEntryNumberArgs);

					masterBooking.Booking.Numbers.Add(numberCopy);
				}
			}
		}

		void CombineEDocs(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var masterBookingDocManagerInfo = ((IDocManagerSupport)masterBooking.Booking).DocManagerInfo;
			var otherBookingDocManagerInfo = ((IDocManagerSupport)otherBooking.Booking).DocManagerInfo;
			var masterBookingDocuments = masterBookingDocManagerInfo.Documents as BusinessObjectCollection;
			var masterBookingFiles = masterBookingDocManagerInfo.Files as BusinessObjectCollection;
			var otherBookingDocuments = otherBookingDocManagerInfo.Documents as BusinessObjectCollection;
			var otherBookingFiles = otherBookingDocManagerInfo.Files as BusinessObjectCollection;

			var copyEdocArgs = new List<string>
			{
				StorageDocsSchema.Constants.SC_SM
			};

			if (otherBookingDocuments != null && masterBookingDocuments != null)
			{
				foreach (var document in otherBookingDocuments)
				{
					var copiedDocument = masterBookingDocuments.AddNew();
					copiedDocument.CopyPersistentValuesFrom(document, new BusinessObjectCloneArgs(copyEdocArgs));
				}
			}

			if (otherBookingFiles != null && masterBookingFiles != null)
			{
				foreach (var file in otherBookingFiles)
				{
					var copiedFile = masterBookingFiles.AddNew();
					copiedFile.CopyPersistentValuesFrom(file, new BusinessObjectCloneArgs(copyEdocArgs));
				}
			}
		}

		void CombineAttachedOrders(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			var attachedOrdersCount = otherBooking.Booking.AttachedOrders.Count;
			for (var i = attachedOrdersCount - 1; i >= 0; i--)
			{
				var attachedOrder = otherBooking.Booking.AttachedOrders[i];
				attachedOrder.JD_JS = masterBooking.Booking.PK;
			}

			if (otherBooking.Booking.AttachedWarehouseOrders.Count > 0)
			{
				var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_DocketType, "ORD"); // WHS Docket Type
				pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, otherBooking.Booking.PK);
				pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, otherBooking.Booking.TablePrefix);

				var pivots = otherBooking.Factory.Load<IWhsDocketJobPivot>(pivotQuery);
				if (pivots != null)
				{
					foreach (var pivot in pivots.Cast<BusinessObject>())
					{
						pivot[WhsDocketJobPivotSchema.WV_ParentId] = masterBooking.Booking.PK;
					}
				}
			}
		}

		void CombineOrderItems(QuotedBooking masterBooking, QuotedBooking otherBooking)
		{
			if (masterBooking.Booking.DocsAndCartage != null
				&& otherBooking.Booking.DocsAndCartage != null
				&& !otherBooking.Booking.DocsAndCartage.JP_OrderItemsAsString.IsEmpty)
			{
				var masterBookingReferences = masterBooking.Booking.DocsAndCartage.JP_OrderItemsAsString.Split(',');
				var otherBookingReferences = otherBooking.Booking.DocsAndCartage.JP_OrderItemsAsString.Split(',');
				var combineReferences = masterBookingReferences.Concat(otherBookingReferences);
				masterBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = string.Join(",", combineReferences.Distinct());

				otherBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = ZString.Empty;
			}
		}

		ZBool CanAddCusEntryNumber(CusEntryNumAdditionalReferenceCollection numbers, CusEntryNumber numberToAdd)
		{
			var matchedRefNumberType = numberToAdd.Lookups.AdditionalReferenceNumberTypes
															.OfType<ICustomsNumberTypeCodeDescription>()
															.FirstOrDefault(numType => numType.Code == numberToAdd.CE_EntryType);

			if (matchedRefNumberType != null)
			{
				if (matchedRefNumberType.IsUnique)
				{
					return !numbers.Cast<CusEntryNumber>().Any(c => c.CE_EntryType == numberToAdd.CE_EntryType
																&& c.CE_RN_NKCountryCode == numberToAdd.CE_RN_NKCountryCode);
				}
				else
				{
					return !numbers.Cast<CusEntryNumber>().Any(c => c.CE_EntryType == numberToAdd.CE_EntryType
																&& c.CE_EntryNum == numberToAdd.CE_EntryNum
																&& c.CE_RN_NKCountryCode == numberToAdd.CE_RN_NKCountryCode);
				}
			}

			return false;
		}

		ZBool IsUniqueNoteForDescription(QuotedBookingStmNote note)
		{
			var desc = note.ST_DescriptionInDatabase;
			var noteType = note.ST_Description_List
				.Cast<PredefinedNoteType>()
				.FirstOrDefault(x => x.Code.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase) || x.Description.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase))
					?? PredefinedNoteTypes.Instance.NoteTypeByDescription(desc);

			return noteType?.IsOnlyOneAllowed ?? false;
		}

		bool IsSerializableNote(QuotedBookingStmNote note)
		{
			var desc = note.ST_Description;
			var noteType = note.ST_Description_List
				.Cast<PredefinedNoteType>()
				.FirstOrDefault(x => x.Code.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase) || x.Description.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase));

			return noteType != null && noteType.SerializableNoteType != null;
		}

		ViewQuotedBookingDefalutFilterProvider GetDefaultFilterProvider(QuotedBooking quotedBooking)
		{
			var filterProvider = new ViewQuotedBookingDefalutFilterProvider();

			if (quotedBooking.ClientPK.IsValid)
			{
				filterProvider.Client = quotedBooking.ClientPK;
			}

			if (quotedBooking.Booking.BookingParty != null)
			{
				filterProvider.BookingParty = quotedBooking.Booking.BookingParty.PK;
			}

			if (!quotedBooking.TransportMode.IsEmpty)
			{
				filterProvider.TransportMode = quotedBooking.TransportMode;
			}

			if (!quotedBooking.ContainerMode.IsEmpty)
			{
				filterProvider.ContainerMode = quotedBooking.ContainerMode;
			}

			if (!quotedBooking.Origin.IsEmpty)
			{
				filterProvider.OriginPort = quotedBooking.Origin;
			}

			if (!quotedBooking.Destination.IsEmpty)
			{
				filterProvider.DestinationPort = quotedBooking.Destination;
			}

			if (!quotedBooking.LoadPort.IsEmpty)
			{
				filterProvider.LoadPort = quotedBooking.LoadPort;
			}

			if (!quotedBooking.DischargePort.IsEmpty)
			{
				filterProvider.DischargePort = quotedBooking.DischargePort;
			}

			if (quotedBooking.OH_Carrier.IsValid)
			{
				filterProvider.Carrier = quotedBooking.OH_Carrier;
			}

			if (quotedBooking.Booking.ConsignorPK.IsValid)
			{
				filterProvider.Consignor = quotedBooking.Booking.ConsignorPK;
			}

			if (quotedBooking.Booking.ConsigneePK.IsValid)
			{
				filterProvider.Consignee = quotedBooking.Booking.ConsigneePK;
			}

			if (quotedBooking.Booking.BookingPartyDocumentaryAddress.OrganisationPK.IsValid)
			{
				filterProvider.BookingParty = quotedBooking.Booking.BookingPartyDocumentaryAddress.OrganisationPK;
			}

			if (quotedBooking.ScheduleChooser.Sailing != null)
			{
				if (!quotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight.IsEmpty)
				{
					filterProvider.Voyage = quotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight;
				}

				if (!quotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel.IsEmpty)
				{
					filterProvider.Vessel = quotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel;
				}
			}

			return filterProvider;
		}

		#endregion
	}
}
