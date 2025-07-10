using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Edifact;
using Enterprise.Edifact.D95B;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Segments;

namespace Enterprise.Freight.Agency.Business
{
	public static partial class CMMMessageDecoder
	{
		/// <summary>
		/// Decode D95B CODECO/COARRI message text into a ICMMMessagingData object.
		/// </summary>
		/// <param name="messageText">The text to decode.</param>
		/// <returns>The decoded content of the message.</returns>
		public static ICMMMessagingData Parse(string messageText)
		{
			object segment = NewMessageFactory().GetMessage(CharacterSet, messageText);
			{
				var message = segment as Edifact.D95B.Messages.COARRI.COARRIMessage;
				if (message != null)
				{
					return new MessagingData(message);
				}
			}

			{
				var message = segment as Edifact.D95B.Messages.CODECO.CODECOMessage;
				if (message != null)
				{
					return new MessagingData(message);
				}
			}

			throw new InvalidFormatException("Corrupt or Malformed D95B CODECO or COARRI message. Cannot Process.");
		}

		static MessageFactory NewMessageFactory()
		{
			return new EdifactD95BMessageFactory();
		}

		#region CharacterSet

		static UNCharacterSet CharacterSet
		{
			get { return new UNOACharacterSet(); }
		}

		#endregion

		#region MessagingData

		partial class MessagingData : ICMMMessagingData
		{
			#region ICMMMessagingData Members

			CMMMessageType ICMMMessagingData.Type
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return type; }
			}
			string ICMMMessagingData.LloydsNumber
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return lloydsNumber; }
			}
			string ICMMMessagingData.VoyageNumber
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return voyageNumber; }
			}
			string ICMMMessagingData.TransportMode
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return transportMode; }
			}
			ICMMOrganisationData ICMMMessagingData.MessageSender
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return messageSender; }
			}
			IEnumerable<ICMMEquipmentData> ICMMMessagingData.Equipment
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return equipment; }
			}

			#endregion

			readonly CMMMessageType type;
			readonly string lloydsNumber;
			readonly string voyageNumber;
			readonly string transportMode;
			readonly ICMMOrganisationData messageSender;
			readonly ICMMEquipmentData[] equipment;
		}

		#endregion

		#region EquipmentData

		partial class EquipmentData : ICMMEquipmentData
		{
			decimal WeightInKG(MEASegment segment)
			{
				if (segment.ValueRange.MeasureUnitQualifier == "KGM")
				{
					decimal result;
					if (!decimal.TryParse(segment.ValueRange.MeasurementValue, out result))
					{
						throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Invalid magnitude ({0})", segment.ValueRange.MeasurementValue));
					}

					return result;
				}
				else
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised weight unit ({0})", segment.ValueRange.MeasureUnitQualifier));
				}
			}

			CMMEquipmentSupplier GetSupplier(EquipmentSupplierCodedList equipmentSupplierCode)
			{
				if (equipmentSupplierCode == EquipmentSupplierCodedList.CarrierSupplied)
				{
					return CMMEquipmentSupplier.Carrier;
				}
				else if (equipmentSupplierCode == EquipmentSupplierCodedList.ShipperSupplied)
				{
					return CMMEquipmentSupplier.Shipper;
				}
				else
				{
					return CMMEquipmentSupplier.Unknown;
				}
			}

			static string[] GetSealNumbers(SELSegmentMessageSection selSection)
			{
				List<string> seals = null;

				foreach (SELSegment sel in selSection)
				{
					if (seals == null)
					{
						seals = new List<string>();
					}

					seals.Add(sel.SealNumber);
				}

				return seals == null ? Array.Empty<string>() : seals.ToArray();
			}

			#region ICMMEquipmentData Members

			string ICMMEquipmentData.ContainerNumber
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return containerNumber; }
			}
			string ICMMEquipmentData.ISOType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return isoType; }
			}
			string ICMMEquipmentData.BookingReference
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return bookingReference; }
			}
			string ICMMEquipmentData.BillOfLading
			{
				get { return billOfLading; }
			}
			string ICMMEquipmentData.GoodsDeclarationNumber
			{
				get { return goodsDeclarationNumber; }
			}
			bool ICMMEquipmentData.IsEmpty
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return isEmpty; }
			}
			CMMEquipmentSupplier ICMMEquipmentData.EquipmentSupplier
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return equipmentSupplier; }
			}
			DateTime ICMMEquipmentData.PositioningDateTime
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return positioningDateTime; }
			}
			decimal? ICMMEquipmentData.GrossWeightKG
			{
				get { return grossWeightKG; }
			}
			IEnumerable<string> ICMMEquipmentData.SealNumbers
			{
				get { return sealNumbers; }
			}

			#endregion

			readonly string containerNumber;
			readonly string isoType;
			readonly bool isEmpty;
			readonly string bookingReference;
			readonly string billOfLading;
			readonly string goodsDeclarationNumber;
			readonly CMMEquipmentSupplier equipmentSupplier;
			readonly DateTime positioningDateTime;
			readonly decimal? grossWeightKG;
			readonly string[] sealNumbers;
		}

		#endregion

		#region OrganisationData

		class OrganisationData : ICMMOrganisationData
		{
			public OrganisationData(NADSegment nad)
			{
				if (nad == null)
				{
					throw new ArgumentNullException(nameof(nad));
				}

				code = nad.PartyIdentificationDetails.PartyIdIdentification;
				codeType = GetCodeType(nad.PartyIdentificationDetails.CodeListResponsibleAgencyCoded);
			}

			CMMOrganisationType GetCodeType(CodeListResponsibleAgencyCodedList list)
			{
				if (list == CodeListResponsibleAgencyCodedList.AuAcosAustralianChamberOfShipping)
				{
					return CMMOrganisationType.OneStop;
				}
				else if (list == CodeListResponsibleAgencyCodedList.MutuallyDefined)
				{
					return CMMOrganisationType.MutuallyDefined;
				}
				else
				{
					return CMMOrganisationType.Unknown;
				}
			}

			#region ICMMOrganisationData Members

			string ICMMOrganisationData.Code
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return code; }
			}
			CMMOrganisationType ICMMOrganisationData.CodeType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return codeType; }
			}

			#endregion

			readonly string code;
			readonly CMMOrganisationType codeType;
		}

		#endregion
	}
}
