using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class HVLVTransportBookingLoader : IHVLVTransportBookingLoader
	{
		#region Interface

		public IEnumerable<IDataObject> GetPackingLines(ZGuid packingParentPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks)
		{
			if (!packingParentPK.IsEmpty)
			{
				var bookingQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
				bookingQuery.AddToFilter(DtbBookingSchema.KM_KB_Booking, packingParentPK);

				var lineQuery = new ZDBOnlyQuery(typeof(SupplierBookingLine));
				lineQuery.AddSubQuery(SupplierBookingLineSchema.DL_KM_LastMileTransportBooking, bookingQuery, JoinCondition.And);
				lineQuery.OrderBy = SupplierBookingLineSchema.DL_ConsigneeReference.Name;

				var lines = factory.Load<SupplierBookingLine>(lineQuery);

				if (lines.Length > 0)
				{
					for (int index = 0; index < lines.Length; index++)
					{
						yield return GeneratePackingLine(lines[index], index + 1, packingLineLinks);
					}
				}
			}
		}

		public IEnumerable<IDataObject> GetInstructions(ZGuid bookingPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks, IDataWritingManager outboundSessionTracker)
		{
			if (!bookingPK.IsEmpty)
			{
				var lineQuery = new ZQuery(SupplierBookingLineSchema.DL_KM_LastMileTransportBooking, bookingPK);
				lineQuery.OrderBy = SupplierBookingLineSchema.DL_Index.Name;

				var lines = factory.Load<SupplierBookingLine>(lineQuery);

				if (lines.Length > 0)
				{
					int sequence = 1;

					var pickupInstructionQuery = new ZQuery(DtbBookingInstructionSchema.KN_KM_BookingMovement, bookingPK);
					pickupInstructionQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, InstructionTypes.Codes.PickUp);
					var pickupInstructions = factory.Load<IDtbBookingInstruction>(pickupInstructionQuery);
					if (pickupInstructions.Length != 1)
					{
						throw new InvalidOperationException(string.Format("Must always be one and only one Pickup Instruction on an HVLV Transport Booking. Found {0} pickup instructions for Booking PK {1}.", pickupInstructions.Length, bookingPK));
					}

					var addressWriter = new JobDocAddressDataObjectWriter(outboundSessionTracker);

					var destinationDepot = pickupInstructions[0].Address as JobDocAddress;
					if (destinationDepot != null)
					{
						yield return GeneratePickupInstruction(destinationDepot, sequence++, packingLineLinks, lines, addressWriter, outboundSessionTracker);
					}

					for (int index = 0; index < lines.Length; index++)
					{
						yield return GenerateDeliveryInstruction(lines[index], sequence++, packingLineLinks, outboundSessionTracker);
					}
				}
			}
		}

		public IEnumerable<IDataObject> GetShipments(ZGuid packingParentPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks, IDataWritingManager outboundSessionTracker)
		{
			if (!packingParentPK.IsEmpty)
			{
				var bookingQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
				bookingQuery.AddToFilter(DtbBookingSchema.KM_KB_Booking, packingParentPK);

				var lineQuery = new ZDBOnlyQuery(typeof(SupplierBookingLine));
				lineQuery.AddSubQuery(SupplierBookingLineSchema.DL_KM_LastMileTransportBooking, bookingQuery, JoinCondition.And);
				lineQuery.OrderBy = SupplierBookingLineSchema.DL_ConsigneeReference.Name;

				var lines = factory.Load<SupplierBookingLine>(lineQuery);

				if (lines.Length > 0)
				{
					var bookingLineWriter = new SupplierBookingLineWriter(outboundSessionTracker);

					for (int index = 0; index < lines.Length; index++)
					{
						yield return GenerateShipment(lines[index], bookingLineWriter);
					}
				}
			}
		}

		public BusinessObject[] GetEventParents(IXmlEventValueObject eventValueObject, ZGuid bookingPK, BusinessObjectFactory factory, IXmlImportLogger inboundSessionTracker)
		{
			var packageId = eventValueObject.Context.TransportBookingPackageID;

			if (!packageId.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(SupplierBookingLine));
				query.AddToFilter(SupplierBookingLineSchema.DL_ConsigneeReference, packageId);
				query.AddToFilter(SupplierBookingLineSchema.DL_KM_LastMileTransportBooking, bookingPK);

				return factory.Load(typeof(SupplierBookingLine), query);
			}

			return null;
		}

		#endregion

		#region Mapping Implementation

		PackingLine GeneratePackingLine(SupplierBookingLine bookingLine, ZInt linkID, Dictionary<ZGuid, ZInt> packingLineLinks)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.Instance)
			{
				Link = linkID,
				ReferenceNumber = bookingLine.DL_ConsigneeReference,
				Weight = bookingLine.DL_GrossWeight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(bookingLine.DL_GrossWeightUQ, bookingLine.Lookups.DL_GrossWeightUQ_List),
				Volume = bookingLine.DL_Cubic,
				VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(bookingLine.DL_CubicUQ, bookingLine.Lookups.DL_CubicUQ_List),
				PackQty = new ZLong(bookingLine.DL_PiecesManifested),
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Package, Description = Core.Constants.PkgUnit.GetDescription(Core.Constants.PkgUnit.Package) },
				GoodsDescription = bookingLine.DL_GoodsDescription,
				MarksAndNos = bookingLine.DL_MarksAndNumbers,
				TransportReference = bookingLine.DL_DeliveryBarcode,
				OrderReference = bookingLine.DL_OrderTrackingNumber,
				LinePrice = bookingLine.DL_GoodsValue,
				LinePriceCurrency = ListHelper.GetWithDescription<Currency>(bookingLine.DL_RX_NKGoodsValueCurrency, bookingLine.Lookups.GoodsValueCurrencies),
				IsSignatureRequired = bookingLine.DL_SignatureRequired
			};

			packingLineLinks.Add(bookingLine.PK, linkID);

			return packingLine;
		}

		UniversalShipment GenerateShipment(SupplierBookingLine bookingLine, SupplierBookingLineWriter bookingLineWriter)
		{
			return bookingLineWriter.GetDataObject(bookingLine);
		}

		Instruction GeneratePickupInstruction(JobDocAddress desinationDepot, ZInt sequence, Dictionary<ZGuid, ZInt> packingLineLinks, IEnumerable<SupplierBookingLine> packingLines, JobDocAddressDataObjectWriter addressWriter, IDataWritingManager dataWritingManager)
		{
			var instruction = new Instruction(dataWritingManager.WriterStrategy)
			{
				Sequence = sequence,
				Type = new CodeDescriptionPair() { Code = InstructionTypes.Codes.PickUp, Description = InstructionTypes.Descriptions.PickUp },
				Status = new CodeDescriptionPair() { Code = TransportStatuses.Codes.Available, Description = TransportStatuses.Descriptions.Available },
				Address = GetAddress(desinationDepot, addressWriter, DocAddressType.LocalCartageCFS),
			};
			instruction.SetInstructionPackingLineLinkCollection(() => packingLines.Select(line => new InstructionPackingLineLink()
			{
				PackingLineLink = packingLineLinks[line.PK],
				Quantity = line.DL_PiecesManifested,
			}).ToList());

			return instruction;
		}

		Instruction GenerateDeliveryInstruction(SupplierBookingLine bookingLine, ZInt sequence, Dictionary<ZGuid, ZInt> packingLineLinks, IDataWritingManager dataWritingManager)
		{
			var instruction = new Instruction(dataWritingManager.WriterStrategy)
			{
				Sequence = sequence,
				Type = new CodeDescriptionPair() { Code = InstructionTypes.Codes.Delivery, Description = InstructionTypes.Descriptions.Delivery },
				Status = new CodeDescriptionPair() { Code = TransportStatuses.Codes.Available, Description = TransportStatuses.Descriptions.Available },
				Address = new SupplierBookingLineConsigneeAddressWriter(dataWritingManager).GetDataObject(bookingLine)
			};
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>(new[] { new InstructionPackingLineLink()
			{
				PackingLineLink = packingLineLinks[bookingLine.PK],
				Quantity = bookingLine.DL_PiecesManifested,
			} }));

			if (instruction.Address != null)
			{
				instruction.Address.AddressType = nameof(DocAddressType.LocalCartageImporter);
			}

			return instruction;
		}

		OrganizationAddress GetAddress(JobDocAddress address, JobDocAddressDataObjectWriter addressWriter, DocAddressType addressType)
		{
			if (address == null || address.IsEmpty)
			{
				return null;
			}

			var addressDO = addressWriter.GetDataObject(address);
			addressDO.AddressType = addressType.ToString();
			return addressDO;
		}

		#endregion
	}
}
