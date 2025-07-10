using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentDataObjectWriter : TopLevelDataObjectWriter<DtbConsignment, UniversalShipment>
	{
		public DtbConsignmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public DtbConsignmentDataObjectWriter(IDataWritingManager manager, DataObjectList<Container> containers)
			: base(manager)
		{
			Containers = Argument.NotNull(containers, nameof(containers));
		}
		readonly DataObjectList<Container> Containers;
		Dictionary<ZGuid, ZInt> packageLinksDictionary;

		protected override void PopulateDataObject(DtbConsignment consignment, UniversalShipment dataObject)
		{
			PopulateConsignment(consignment, dataObject);
			PopulateBookingJob(consignment, dataObject);
			PopulateLocalClient(consignment, dataObject);
			PopulatePackageJobData(consignment, dataObject);
			PopulateConsignmentAddress(consignment, dataObject);
			PopulateOrgAddress(consignment, dataObject);
			PopulteAdditionalReference(consignment, dataObject);
			PopulteNote(consignment, dataObject);
		}

		void PopulateBookingJob(DtbConsignment consignment, UniversalShipment dataObject)
		{
			if (consignment.LTC_KM_Booking.IsEmpty)
			{
				return;
			}

			var transportBooking = consignment.Factory.Load<DtbBooking>(consignment.LTC_KM_Booking);
			dataObject.DataContext.AddDataSource(DataContextType.TransportBooking, transportBooking.KM_JobID);
		}

		void PopulateLocalClient(DtbConsignment consignment, UniversalShipment dataObject)
		{
			if (consignment.Job?.JH_OA_LocalChargesAddr.IsEmpty ?? true)
			{
				return;
			}

			var organizationDataObjectWriter = new OrganizationDataObjectWriter(writeManager, AddressTypes.SendersLocalClient);
			var localChargesAddr = consignment.Factory.Load<OrgAddress>(consignment.Job.JH_OA_LocalChargesAddr);

			dataObject.SetOrganizationAddressCollection(() =>
				dataObject.OrganizationAddressCollection.MergeCollection(new[] { organizationDataObjectWriter.GetDataObject(localChargesAddr) },
																		keepExistingData: true,
																		(OrganizationAddress x, OrganizationAddress y) => x != null && y != null && x.AddressType.GetValueOrDefault() == y.AddressType.GetValueOrDefault()));
		}

		void PopulateOrgAddress(DtbConsignment consignment, UniversalShipment dataObject)
		{
			if (!consignment.DocAddresses.Any())
			{
				return;
			}

			var jobDocAddressDataObjectWriter = new JobDocAddressDataObjectWriter(writeManager);
			dataObject.SetOrganizationAddressCollection(() =>
				dataObject.OrganizationAddressCollection.MergeCollection(ProcessCollection(consignment.DocAddresses, jobDocAddressDataObjectWriter, CollectionContent.Complete),
																		keepExistingData: true,
																		(OrganizationAddress x, OrganizationAddress y) => x != null && y != null && x.AddressType.GetValueOrDefault() == y.AddressType.GetValueOrDefault()));
		}

		void PopulteNote(DtbConsignment consignment, UniversalShipment dataObject)
		{
			var noteWriter = new NoteDataObjectWriter(writeManager);
			dataObject.SetNoteCollection(() => ProcessCollection(consignment.Notes.GetAllNotesVisibleToCurrentCompany(), noteWriter, CollectionContent.Complete));
		}

		void PopulteAdditionalReference(DtbConsignment consignment, UniversalShipment dataObject)
		{
			var additionalReferenceWriter = new AdditionalReferenceDataObjectWriter(writeManager);
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(consignment.AdditionalReferenceNumbers, additionalReferenceWriter, CollectionContent.Complete));
		}

		void PopulatePackageJobData(DtbConsignment consignment, UniversalShipment dataObject)
		{
			var packageJobWriter = new PkgPackageJobDataObjectWriter(writeManager);
			var packageJobDataObject = packageJobWriter.GetDataObject(consignment.PackageJob);
			dataObject.SetPackingLineCollection(() => packageJobDataObject.PackingLineCollection);

			if (Containers != null)
			{
				Containers.AddRange(packageJobDataObject.ContainerCollection);
			}
			else
			{
				dataObject.SetContainerCollection(() => packageJobDataObject.ContainerCollection);
			}
			packageLinksDictionary = packageJobWriter.LinksDictionary;
		}

		void PopulateConsignmentAddress(DtbConsignment consignment, UniversalShipment dataObject)
		{
			var addressWriter = new DtbConsignmentAddressDataObjectWriter(writeManager, packageLinksDictionary);
			dataObject.SetInstructionCollection(() => ProcessCollection(consignment.Addresses, addressWriter, CollectionContent.Complete));
		}

		void PopulateConsignment(DtbConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.ServiceLevel = new ServiceLevel() { Code = consignment.LTC_RS_NKServiceLevel };
			dataObject.ConsignmentNote = consignment.LTC_ConnoteNumber;
			dataObject.Direction = consignment.LTC_Direction;
			dataObject.GoodsValue = consignment.LTC_GoodsValue;
			dataObject.GoodsValueCurrency = new Currency() { Code = consignment.LTC_RX_NKGoodsValueCurrency };
			dataObject.InsuranceValue = consignment.LTC_InsuranceValue;
			dataObject.InsuranceValueCurrency = new Currency() { Code = consignment.LTC_RX_NKInsuranceValueCurrency };
			dataObject.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = consignment.LTC_JobType };
			dataObject.ShipmentIncoTerm = new IncoTerm() { Code = consignment.LTC_Incoterm };
			dataObject.AdditionalTerms = consignment.LTC_AdditionalTerms;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.LandTransportConsignment;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(DtbConsignment consignment)
		{
			var helper = ObjectFactory.Get<ICustomValuesHelper>();

			return helper.GetUserDefinedValues(consignment);
		}
	}
}


