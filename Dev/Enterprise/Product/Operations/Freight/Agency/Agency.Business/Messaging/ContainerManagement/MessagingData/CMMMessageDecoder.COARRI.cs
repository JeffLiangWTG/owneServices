using System.Globalization;
using Enterprise.Edifact;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Messages.COARRI;
using Enterprise.Edifact.D95B.Segments;

namespace Enterprise.Freight.Agency.Business
{
	partial class CMMMessageDecoder
	{
		#region MessagingData

		partial class MessagingData : ICMMMessagingData
		{
			public MessagingData(COARRIMessage message)
			{
				if (message.UNH.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} UNH segments in group0", message.UNH.Count));
				}

				if (message.BGM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} BGM segments in group0", message.BGM.Count));
				}

				type = GetCOARRIMessageType(message.BGM[0].DocumentMessageName.DocumentMessageNameCoded);
				if (type == CMMMessageType.Unknown)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised document type '{0}'", message.BGM[0].DocumentMessageName.DocumentMessageNameCoded));
				}

				if (message.Group1.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} Group 1's in group0", message.Group1.Count));
				}

				SegmentGroup1 group1 = message.Group1[0];
				if (group1.TDT.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} TDT segments in group 1", group1.TDT.Count));
				}

				lloydsNumber = group1.TDT[0].TransportIdentification.IdOfMeansOfTransportIdentification;
				voyageNumber = group1.TDT[0].ConveyanceReferenceNumber;
				transportMode = string.Empty;

				foreach (SegmentGroup2 group2 in message.Group2)
				{
					NADSegment nad = group2.NAD[0];
					if (nad.PartyQualifier == PartyQualifierList.DocumentMessageIssuerSender)
					{
						messageSender = new OrganisationData(nad);
					}
				}

				equipment = new ICMMEquipmentData[message.Group3.Count];
				for (int i = 0; i < message.Group3.Count; i++)
				{
					equipment[i] = new EquipmentData(message.Group3[i]);
				}
			}

			static CMMMessageType GetCOARRIMessageType(DocumentMessageNameCodedList messageNameCoded)
			{
				switch (messageNameCoded.ToString())
				{
					case "46":
					case "270":
						return CMMMessageType.Load;

					case "44":
					case "98":
						return CMMMessageType.Discharge;
					default:
						return CMMMessageType.Unknown;
				}
			}
		}

		#endregion

		#region EquipmentData

		partial class EquipmentData : ICMMEquipmentData
		{
			public EquipmentData(SegmentGroup3 group3)
			{
				if (group3.EQD.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} EQD segments in group3", group3.EQD.Count));
				}

				if (group3.DTM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} DTM segments in group3", group3.DTM.Count));
				}

				containerNumber = group3.EQD[0].EquipmentIdentification.EquipmentIdentificationNumber;
				isoType = group3.EQD[0].EquipmentSizeAndType.EquipmentSizeAndTypeIdentification;
				isEmpty = group3.EQD[0].FullEmptyIndicatorCoded == FullEmptyIndicatorCodedList.Empty;
				equipmentSupplier = GetSupplier(group3.EQD[0].EquipmentSupplierCoded);
				positioningDateTime = EdifactDateParser.GetDate(group3.DTM[0]);
				sealNumbers = GetSealNumbers(group3.SEL);

				foreach (MEASegment mea in group3.MEA)
				{
					if (mea.MeasurementDetails.MeasurementDimensionCoded == MeasurementDimensionCodedList.GrossWeight)
					{
						grossWeightKG = WeightInKG(mea);
					}
				}

				foreach (RFFSegment rff in group3.RFF)
				{
					if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.BookingReferenceNumber)
					{
						bookingReference = rff.Reference.ReferenceNumber;
					}
					else if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.BillOfLadingNumber)
					{
						billOfLading = rff.Reference.ReferenceNumber;
					}
					else if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.GoodsDeclarationNumber)
					{
						goodsDeclarationNumber = rff.Reference.ReferenceNumber;
					}
				}

				if (bookingReference == null)
				{
					bookingReference = string.Empty;
				}

				if (billOfLading == null)
				{
					billOfLading = string.Empty;
				}

				if (goodsDeclarationNumber == null)
				{
					goodsDeclarationNumber = string.Empty;
				}
			}
		}

		#endregion
	}
}





