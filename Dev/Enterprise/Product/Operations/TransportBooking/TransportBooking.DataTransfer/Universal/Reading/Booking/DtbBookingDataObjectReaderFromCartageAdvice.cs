using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingDataObjectReaderFromCartageAdvice : DtbBookingDataObjectReader
	{
		public DtbBookingDataObjectReaderFromCartageAdvice(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsolidation consolidation, UniversalShipment sourceDO, UniversalShipment topLevelDO, DtbBooking existingBooking = null, Dictionary<ZInt, PkgPackage> packageContainerLinks = null, IList<ZGuid> assignedContainerPks = null, IEnumerable<PkgPackage> assignedPackages = null) // some override to create existing booking
			: base(shipment, logger, factory, consolidation, topLevelDO, sourceDO)
		{
			ExistingBooking = existingBooking;
			AssignedContainerPks = assignedContainerPks;
			PackageContainerLinks = packageContainerLinks;
			AssignedPackages = assignedPackages;
		}
		readonly DtbBooking ExistingBooking;
		readonly IList<ZGuid> AssignedContainerPks;
		readonly IDictionary<ZInt, PkgPackage> PackageContainerLinks;
		readonly IEnumerable<PkgPackage> AssignedPackages;
		DataObjectList<Container> Containers => dataObject.ContainerCollection;

		protected override DtbBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return ExistingBooking;
		}

		protected override void BeforePopulateBusinessObject(DtbBooking booking)
		{
			base.BeforePopulateBusinessObject(booking);
			PopulateLinks(booking);
			booking.KM_KB_Booking = Consolidation.PK;
			booking.SetContainerLinksAndNumbers(Containers);
		}

		protected override void PopulateRelatedEntitiesCore(DtbBooking booking)
		{
			PopulatePackages(booking);
			PopulateReferences(booking);
		}

		void PopulateLinks(DtbBooking booking)
		{
			booking.PackageContainerLinks = PackageContainerLinks;
		}

		void PopulatePackages(DtbBooking booking)
		{
			if (AssignedPackages != null && AssignedPackages.Any())
			{
				booking.DefaultPackages(AssignedPackages, null);
			}
			else
			{
				var hasPackedContainer = false;
				var containers = Containers;
				if ((PackageContainerLinks?.Any() ?? false) && containers != null)
				{
					var packedContainer = PackageContainerLinks.Where(l => Containers.Any(c => c.Link == l.Key || (!l.Value.KP_PackageID.IsEmpty && c.ContainerNumber.Equals(l.Value.KP_PackageID)))).Select(c => c.Value);
					var releaseNumbersByPackage = GetReleaseNumbersByPackage();
					booking.DefaultPackages(packedContainer, releaseNumbersByPackage);
					hasPackedContainer = packedContainer.Any();
				}
				else
				{
					// try to set with id (we should always have a link, but to keep backwards compatability, we will keep it for now and remove it in a Refactor - Feature Request)
					var containerIDs = GetContainerIDs(dataObject).ToArray();
					booking.DefaultPackages(containerIDs);
					hasPackedContainer = containerIDs.Any();
				}
				AssignEmptyContainers(booking, hasPackedContainer);
			}

			booking.KM_IsHazardous = booking.IsAnyPackageHazardous;
			booking.KM_RequiresRefrigeration = booking.IsAnyPackageRequiresRefridgeration;
		}

		Dictionary<PkgPackage, ZString> GetReleaseNumbersByPackage()
		{
			var releaseNumbersByPackage = new Dictionary<PkgPackage, ZString>();

			if (Containers != null)
			{
				var containerDOByLink = Containers.Where(c => c.Link.HasValue && c.Link != 0).ToDictionary(c => c.Link);

				var packagesWithReleaseNumbers = PackageContainerLinks
					.Select(x => new
					{
						package = x.Value,
						releaseNum = containerDOByLink.TryGetValue(x.Key, out var container) ? container.ReleaseNum.GetValueOrDefault() : ZString.Empty
					});

				releaseNumbersByPackage = packagesWithReleaseNumbers.Where(y => !y.releaseNum.IsEmpty).ToDictionary(x => x.package, x => x.releaseNum);
			}

			return releaseNumbersByPackage;
		}

		void AssignEmptyContainers(DtbBooking booking, bool hasPackedContainer)
		{
			if (!hasPackedContainer && Containers != null && !IsLooseTemplate(booking))
			{
				foreach (var container in Containers)
				{
					var packages = booking.ConsolidationSingleJob.PackageJob.Packages;
					var containerToAssign = packages.FirstOrDefault(p => p.IsContainer
						&& (!container.ContainerCount.HasValue || p.KP_PackageQty == container.ContainerCount.Value)
						&& !AssignedContainerPks.Contains(p.PK));

					if (containerToAssign != null)
					{
						foreach (var instruction in booking.Instructions)
						{
							instruction.DefaultPackages(new[] { containerToAssign });
						}
						AssignedContainerPks.Add(containerToAssign.PK);
					}
				}
			}
		}

		bool IsLooseTemplate(DtbBooking booking)
		{
			var template = booking.BookingTemplate;
			return template == null || template.Instructions.Any(i => i.K2_PackageType == PackageCategories.Codes.Loose);
		}

		void PopulateReferences(DtbBooking booking)
		{
			var bookingAdditionalReferenceCollection = booking.AdditionalReferenceNumbers;
			var consolidationAdditionalReferenceCollection = booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
			DeleteOrderNumbersFromBooking(booking);
			if (SourceOrder != null)
			{
				if (SourceOrder.ClientReference.HasValue && !SourceOrder.ClientReference.Value.IsEmpty)
				{
					CreateOrUpdateReference(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, SourceOrder.ClientReference.Value, bookingAdditionalReferenceCollection);
				}

				if (SourceOrder.OrderNumber.HasValue && !SourceOrder.OrderNumber.Value.IsEmpty)
				{
					CreateOrUpdateReference(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, SourceOrder.OrderNumber.Value, consolidationAdditionalReferenceCollection);
				}

				if (OrganisationAddresses != null)
				{
					var client = OrganisationAddresses.FirstOrDefault(a => a.AddressType.Equals(AddressTypes.SendersLocalClient));
					if (client != null && client.CompanyName.HasValue && IsWhsReceive)
					{
						CreateOrUpdateReference(TransportCommonAdditionalReferenceTypes.Codes.Client, client.CompanyName.Value, bookingAdditionalReferenceCollection);
					}
				}
			}

			PopulateOrderNumbersFromOwnerRef(consolidationAdditionalReferenceCollection);
			PopulateOrderNumbersFromOrderNumberCollection(consolidationAdditionalReferenceCollection);
		}

		void DeleteOrderNumbersFromBooking(DtbBooking booking)
		{
			DeleteOrderNumbersFromAdditionalReferences(booking.AdditionalReferenceNumbers);
			DeleteOrderNumbersFromAdditionalReferences(booking.ConsolidationSingleJob.AdditionalReferenceNumbers);
		}

		void DeleteOrderNumbersFromAdditionalReferences(ICusEntryNumAdditionalReferenceCollection additionalReferencesCollection)
		{
			if (additionalReferencesCollection != null)
			{
				foreach (var additionalReference in additionalReferencesCollection.Cast<Integration.Customs.ICusEntryNumber>().Where(r => r.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).ToArray())
				{
					additionalReferencesCollection.RemoveAndDelete(additionalReference);
				}
			}
		}

		void PopulateOrderNumbersFromOrderNumberCollection(ICusEntryNumAdditionalReferenceCollection additionalReferenceCollection)
		{
			var localProcessing = dataObject.LocalProcessing ?? SourceDO.LocalProcessing;
			if (localProcessing != null)
			{
				var orderNumbers = localProcessing.OrderNumberCollection;
				if (orderNumbers != null)
				{
					Array.ForEach(orderNumbers.Select(o => o.OrderReference.GetValueOrDefault()).Where(s => !s.IsEmpty).ToArray(),
						orderNum => additionalReferenceCollection.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, orderNum));
				}
			}
		}

		void PopulateOrderNumbersFromOwnerRef(ICusEntryNumAdditionalReferenceCollection additionalReferenceCollection)
		{
			var ownerRefValue = !string.IsNullOrEmpty(dataObject.OwnerRef.GetValueOrDefault()) ? dataObject.OwnerRef.GetValueOrDefault() : SourceDO.OwnerRef.GetValueOrDefault();
			if (!ownerRefValue.IsEmpty)
			{
				Array.ForEach(ownerRefValue.Split(',').Select(s => s.Trim()).Where(s => !s.IsEmpty).ToArray(),
					ownerRef => additionalReferenceCollection.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, ownerRef));
			}
		}

		void CreateOrUpdateReference(string referenceType, ZString number, ICusEntryNumAdditionalReferenceCollection additionalReferenceCollection)
		{
			var referenceNumber = additionalReferenceCollection.GetFirstReferenceNumberByType(referenceType);
			if (referenceNumber == null)
			{
				additionalReferenceCollection.AddNewIfNotExist(referenceType, number);
			}
			else
			{
				referenceNumber.CE_EntryNum = number;
			}
		}

		ZBool IsWhsReceive
		{
			get { return logger.TopLevelDataContext.GetMatchingDataSource(DataContextType.WarehouseReceive) != null; }
		}

		List<OrganizationAddress> OrganisationAddresses
		{
			get { return SourceDO != null ? SourceDO.OrganizationAddressCollection : null; }
		}

		Order SourceOrder
		{
			get { return SourceDO != null ? SourceDO.Order : null; }
		}
	}
}
