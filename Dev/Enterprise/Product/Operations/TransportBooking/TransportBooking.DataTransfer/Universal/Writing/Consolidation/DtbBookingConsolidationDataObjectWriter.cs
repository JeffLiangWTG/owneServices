using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingConsolidationDataObjectWriter : TopLevelDataObjectWriter<DtbBookingConsolidation, UniversalShipment>
	{
		public DtbBookingConsolidationDataObjectWriter(IDataWritingManager manager, bool includeAllChildBookings)
			: base(manager)
		{
			this.IncludeAllChildBookings = includeAllChildBookings;
		}

		readonly bool IncludeAllChildBookings;

		protected override bool ShouldSendConsolCostsData(DtbBookingConsolidation sourceBO)
		{
			return true;
		}

		protected override void PopulateDataObject(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			Argument.NotNull(bookingConsolidationBO, "DtbBookingConsolidation bookingConsolidationBO");

			writeManager.AddPK(bookingConsolidationBO.PK);

			transportBookingDataObject.TransportBookingDirection = new TransportBookingDirection
			{
				Code = bookingConsolidationBO.KB_JobDirection,
				Description = DtbBookingDirectionDescription.GetDescription(bookingConsolidationBO.KB_JobDirection)
			};

			transportBookingDataObject.ShipmentType = new CodeDescriptionPair
			{
				Code = bookingConsolidationBO.KB_JobType,
				Description = (bookingConsolidationBO.KB_JobType == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation) ? TransportConsolidationJobTypes.Descriptions.BookingTransportConsolidation : TransportConsolidationJobTypes.Descriptions.Booking
			};

			if (!bookingConsolidationBO.KB_GoodsDescription.IsEmpty)
			{
				transportBookingDataObject.GoodsDescription = bookingConsolidationBO.KB_GoodsDescription;
			}

			PopulateRelatedEntities(bookingConsolidationBO, transportBookingDataObject);
			PopulateSchedules(bookingConsolidationBO, transportBookingDataObject);
		}

		void PopulateAddresses(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			transportBookingDataObject.SetOrganizationAddressCollection(() => ProcessCollection(bookingConsolidationBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)
			{
				PopulateGeoLocation = true,
				PopulateValidationStatus = true,
			}));
		}

		void PopulateRelatedEntities(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			PopulateAddresses(bookingConsolidationBO, transportBookingDataObject);

			if (bookingConsolidationBO.KB_JobType == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation)
			{
				PopulateChildBookingsForMultiJob(bookingConsolidationBO, transportBookingDataObject);
			}
			else
			{
				PopulatePackageJob(bookingConsolidationBO, transportBookingDataObject); // needs to populate before child bookings.
				PopulateChildBookings(bookingConsolidationBO, transportBookingDataObject);
				PopulateParentJob(bookingConsolidationBO, transportBookingDataObject);
			}
			PopulateAdditionalReferences(bookingConsolidationBO, transportBookingDataObject);
			transportBookingDataObject.SetNoteCollection(() =>
			{
				var notes = bookingConsolidationBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulatePackageJob(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			var packageJobBO = bookingConsolidationBO.PackageJob;
			if (packageJobBO != null)
			{
				var packageJobWriter = new PkgPackageJobDataObjectWriter(writeManager);
				var packageJobDataObject = packageJobWriter.GetDataObject(packageJobBO);
				transportBookingDataObject.SetPackingLineCollection(() => packageJobDataObject.PackingLineCollection);
				transportBookingDataObject.SetContainerCollection(() => packageJobDataObject.ContainerCollection);
				LinksDictionary = packageJobWriter.LinksDictionary;
			}
		}

		public Dictionary<ZGuid, ZInt> LinksDictionary
		{
			get;
			private set;
		}

		void PopulateChildBookings(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			if (IncludeAllChildBookings)
			{
				var data = ProcessCollection(bookingConsolidationBO.Bookings, new DtbBookingDataObjectWriter(LinksDictionary, writeManager, false));
				transportBookingDataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
			}
		}

		void PopulateChildBookingsForMultiJob(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			if (IncludeAllChildBookings)
			{
				transportBookingDataObject.SetSubShipmentCollection(() =>
			   {
				   var list = new DataObjectList<UniversalShipment>();
				   foreach (var booking in bookingConsolidationBO.Bookings)
				   {
					   list.Add(new DtbBookingDataObjectWriter(null, writeManager, true).GetDataObject(booking));
				   }
				   return list;
			   });
			}
		}

		void PopulateParentJob(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment transportBookingDataObject)
		{
			var consolidationParentInfo = bookingConsolidationBO.Parent;
			if (consolidationParentInfo != null)
			{
				var consolidationParent = consolidationParentInfo.ParentWithWorkflow;
				if (consolidationParent == null || writeManager.PKAlreadyExported(consolidationParent.PK))
				{
					return;
				}

				var parentManager = consolidationParent.GetUniversalDataContextManager();
				if (parentManager != null && parentManager.ManagesShipments())
				{
					if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
					{
						var iShipmentDataContextManager = parentManager as IShipmentDataContextManager;
						if (iShipmentDataContextManager != null)
						{
							var shipmentWriter = iShipmentDataContextManager.GetShipmentDataObjectWriter(writeManager);
							if (shipmentWriter != null)
							{
								var parentShipment = shipmentWriter.GetDataObject(consolidationParent) as UniversalShipment;
								if (parentShipment != null)
								{
									if (bookingConsolidationBO.KB_JobDirection == nameof(DtbBookingDirection.DLV))
									{
										transportBookingDataObject.SetPreCarriageShipmentCollection(() => transportBookingDataObject.PreCarriageShipmentCollection.AddSafe(parentShipment));
									}
									else
									{
										transportBookingDataObject.SetPostCarriageShipmentCollection(() => transportBookingDataObject.PostCarriageShipmentCollection.AddSafe(parentShipment));
									}
								}
							}
						}
					}
					else
					{
						MergeInAdditionalShipments(transportBookingDataObject, new[] { consolidationParent });
					}
				}
			}
		}

		void PopulateAdditionalReferences(DtbBookingConsolidation consolidation, UniversalShipment bookingDataObject)
		{
			bookingDataObject.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferenceCollection = ProcessCollection(consolidation.AdditionalReferenceNumbers,
					new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete)
					?? new DataObjectList<AdditionalReference>();

				PopulateWayBill(bookingDataObject, additionalReferenceCollection);

				if (additionalReferenceCollection.Any())
				{
					return additionalReferenceCollection;
				}
				return null;
			});
		}

		void PopulateWayBill(UniversalShipment consolidationDataObject, DataObjectList<AdditionalReference> additionalReferenceCollection)
		{
			var populatedHouseBill = TryPopulateWayBill(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, consolidationDataObject, additionalReferenceCollection, WayBillTypeList.Codes.House);

			if (!populatedHouseBill)
			{
				TryPopulateWayBill(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, consolidationDataObject, additionalReferenceCollection, WayBillTypeList.Codes.Master);
			}
		}

		bool TryPopulateWayBill(string additionalReferenceTypeCode, UniversalShipment cosnoldiationDataObject, DataObjectList<AdditionalReference> additionalReferenceCollection, string uxmlWayBillTypeCode)
		{
			var wayBill = additionalReferenceCollection.FirstOrDefault(r => r.Type != null && r.Type.GetCodeAsUpperCase() == additionalReferenceTypeCode);

			if (wayBill != null)
			{
				var wayBillNumber = wayBill.ReferenceNumber.GetValueOrDefault();
				if (!wayBillNumber.IsEmpty)
				{
					cosnoldiationDataObject.WayBillNumber = wayBillNumber;
					cosnoldiationDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(uxmlWayBillTypeCode, new WayBillTypeList());
				}
			}

			return !cosnoldiationDataObject.WayBillNumber.GetValueOrDefault().IsEmpty;
		}

		void PopulateSchedules(DtbBookingConsolidation bookingConsolidationBO, UniversalShipment bookingDataObject)
		{
			var writer = ObjectFactory.Get<ITransportLegDataObjectWriter>("ITransportLegDataObjectWriter", writeManager) as DataObjectWriter<BusinessObject, TransportLeg>;

			if (bookingConsolidationBO.IsParentSupportsRouting || bookingConsolidationBO.IsParentSupportsDirectSailing)
			{
				bookingDataObject.SetTransportLegCollection(() => bookingConsolidationBO.Parent.GetParentRoutingTransportLegs());
			}
			else if (bookingDataObject.TransportLegCollection.IsNullOrEmpty())
			{
				var collection = bookingConsolidationBO.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, bookingConsolidationBO.PK));
				bookingDataObject.SetTransportLegCollection(() => ProcessCollection(collection, writer, CollectionContent.Complete, true));

				if (bookingDataObject.TransportLegCollection != null && bookingDataObject.TransportLegCollection.Any())
				{
					var transportMode = bookingDataObject.TransportLegCollection.OrderBy(t => t.LegOrder).FirstOrDefault().TransportMode;
					if (transportMode != null)
					{
						bookingDataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportMode.Value.ToString().ToUpper(CultureInfo.InvariantCulture), new ZArchitecture.Core.CodeDescriptionPairList(Enterprise.ZArchitecture.Core.OLookUpEditType.LocalCartageTransportModes));
					}
				}
			}
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportBookingConsolidation;
		}
	}
}
