using System.Globalization;
using Enterprise.Edifact;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Messages.CODECO;
using Enterprise.Edifact.D95B.Segments;

namespace Enterprise.Freight.Agency.Business
{
	partial class CMMMessageDecoder
	{
		#region MessagingData

		partial class MessagingData : ICMMMessagingData
		{
			public MessagingData(CODECOMessage message)
			{
				if (message.UNH.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} UNH segments in group0", message.UNH.Count));
				}

				if (message.BGM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} BGM segments in group0", message.BGM.Count));
				}

				type = GetCODECOMessageType(message.BGM[0].DocumentMessageName.DocumentMessageNameCoded);
				if (type == CMMMessageType.Unknown)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised document type '{0}'", message.BGM[0].DocumentMessageName.DocumentMessageNameCoded));
				}

				if (message.Group1.Count > 0)
				{
					SegmentGroup1 group1 = message.Group1[0];
					if (group1.TDT.Count != 1)
					{
						throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} TDT segments in group 1", group1.TDT.Count));
					}

					lloydsNumber = group1.TDT[0].TransportIdentification.IdOfMeansOfTransportIdentification;
					voyageNumber = group1.TDT[0].ConveyanceReferenceNumber;
				}

				foreach (SegmentGroup2 group2 in message.Group2)
				{
					NADSegment nad = group2.NAD[0];
					if (nad.PartyQualifier == PartyQualifierList.DocumentMessageIssuerSender)
					{
						messageSender = new OrganisationData(nad);
					}
				}

				equipment = new ICMMEquipmentData[message.Group5.Count];
				for (int i = 0; i < message.Group5.Count; i++)
				{
					equipment[i] = new EquipmentData(message.Group5[i]);
				}
			}

			static CMMMessageType GetCODECOMessageType(DocumentMessageNameCodedList messageNameCoded)
			{
				switch (messageNameCoded.ToString())
				{
					case "34":
						return CMMMessageType.GateIn;
					case "36":
						return CMMMessageType.GateOut;
					default:
						return CMMMessageType.Unknown;
				}
			}
		}

		#endregion

		#region EquipmentData

		partial class EquipmentData : ICMMEquipmentData
		{
			public EquipmentData(SegmentGroup5 group5)
			{
				if (group5.EQD.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} EQD segments in group5", group5.EQD.Count));
				}

				if (group5.DTM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} DTM segments in group5", group5.DTM.Count));
				}

				containerNumber = group5.EQD[0].EquipmentIdentification.EquipmentIdentificationNumber;
				isoType = group5.EQD[0].EquipmentSizeAndType.EquipmentSizeAndTypeIdentification;
				isEmpty = group5.EQD[0].FullEmptyIndicatorCoded == FullEmptyIndicatorCodedList.Empty;
				equipmentSupplier = GetSupplier(group5.EQD[0].EquipmentSupplierCoded);
				positioningDateTime = EdifactDateParser.GetDate(group5.DTM[0]);
				sealNumbers = GetSealNumbers(group5.SEL);

				foreach (MEASegment mea in group5.MEA)
				{
					if (mea.MeasurementDetails.MeasurementDimensionCoded == MeasurementDimensionCodedList.GrossWeight)
					{
						grossWeightKG = WeightInKG(mea);
					}
				}

				foreach (RFFSegment rff in group5.RFF)
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







