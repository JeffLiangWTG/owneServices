using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public static class VoyageMessagingData
	{
		public static IPortAuthorityMessagingData New(PortAuthority message, BusinessObjectFactory factory = null)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			factory = factory ?? message.Factory;

			PortAuthorityBusinessObjectValidation.RegisterForFactory(factory);

			List<IPortAuthorityConsignmentData> consignments = new List<IPortAuthorityConsignmentData>();
			Dictionary<string, Equipment> equipmentLookup = new Dictionary<string, Equipment>();

			if (message.Voyage.Vessel != null && message.Voyage.Vessel.RV_LloydsNumber.IsEmpty)
			{
				Notify(message, message.Voyage.Vessel, Res.GetString("6d1b4aa3-1b1a-4333-9d3b-9489f23dbcd1", "The vessel '{0}' has no Lloyds number entered.", message.Voyage.Vessel.RV_Name), "");
			}

			foreach (BillOfLading shipment in message.GetRelatedShipments(factory).Where(shipment => shipment.JS_OH_DeliveryAgent == message.PrincipalPK))
			{
				shipment.MarkAsNeedingValidation();
				shipment.OuterPackLines.MarkAsNeedingValidation();
				shipment.RealContainers.MarkAsNeedingValidation();
				shipment.TopLevelPacks.MarkAsNeedingValidation();
				shipment.Transports.MarkAsNeedingValidation();
				shipment.RunPreSaveValidation();

				if (shipment.HasErrors)
				{
					Notify(message, shipment, Res.GetString("a6618613-40c8-4fe2-848d-ed92fd2c930d", "{0} has errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}
				else if (shipment.HasMessageErrors)
				{
					Notify(message, shipment, Res.GetString("37331d01-03e2-4c7d-80eb-b1a926509947", "{0} has message errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}
				else
				{
					consignments.Add(PopulateConsignmentData(equipmentLookup, message, shipment, consignments.Count + 1));
				}
			}

			IPortAuthorityEquipmentData[] equipment = new IPortAuthorityEquipmentData[equipmentLookup.Count];
			((System.Collections.ICollection)equipmentLookup.Values).CopyTo(equipment, 0);

			return new MessagingData(message, equipment, consignments.ToArray());
		}

		static IPortAuthorityConsignmentData PopulateConsignmentData(Dictionary<string, Equipment> equipmentLookup, PortAuthority message, BillOfLading shipment, int consignmentNumber)
		{
			List<IPortAuthorityGoodsData> goods = new List<IPortAuthorityGoodsData>();

			if (shipment.IsTopLevelPacksMode)
			{
				foreach (AgencyShipmentContainer topLevelPack in shipment.TopLevelPacks)
				{
					var packlineAdapter = AgencyShipmentPackLineAdapter.New(topLevelPack);
					goods.Add(new Goods(shipment, packlineAdapter, null, goods.Count + 1, message));
				}
			}
			else
			{
				foreach (BillOfLadingPackLine packline in shipment.OuterPackLines)
				{
					BillOfLadingContainer container = shipment.Factory.Load<BillOfLadingContainer>(packline.JL_JC);

					goods.Add(new Goods(shipment, packline, container, goods.Count + 1, message));
				}

				List<BillOfLadingContainer> emptyContainerNumbers = new List<BillOfLadingContainer>();

				foreach (BillOfLadingContainer container in shipment.RealContainers)
				{
					Equipment eq;

					if (equipmentLookup.TryGetValue(container.JC_ContainerNum, out eq))
					{
						eq.Populate(container, message.Direction);
					}
					else
					{
						equipmentLookup.Add(container.JC_ContainerNum, new Equipment(container, message.Direction));
					}

					if (container.PackLines.Count == 0)
					{
						emptyContainerNumbers.Add(container);
					}
				}

				if (emptyContainerNumbers.Count > 0)
				{
					goods.Add(new EmptyGoods(emptyContainerNumbers, goods.Count + 1));
				}
			}

			return new Consignment(shipment, message, goods.ToArray(), consignmentNumber);
		}

		static void Notify(IPortAuthorityIssueListener listener, BillOfLading shipment, ZString format, string details)
		{
			listener.Notify(shipment.PK, JobShipmentSchema.Constants.Prefix, format, details);
		}

		static void Notify(IPortAuthorityIssueListener listener, RefVessel vessel, ZString format, string details)
		{
			listener.Notify(vessel.PK, RefVesselSchema.Constants.Prefix, format, details);
		}

		#region MessagingData

		class MessagingData : IPortAuthorityMessagingData
		{
			public MessagingData(PortAuthority message,
				IPortAuthorityEquipmentData[] equipment,
				IPortAuthorityConsignmentData[] consignments)
			{
				this.messageFunction = message.GetMessageFunction();
				this.messagePrepared = ZDateTime.Now;
				this.vessel = message.Voyage.JV_RV_NKVessel;
				this.lloyds = message.Voyage.Vessel == null ? ZString.Empty : message.Voyage.Vessel.RV_LloydsNumber;
				this.voyage = message.Voyage.JV_VoyageFlight;
				this.load = (message.Direction == Constants.PortDirection.Load ? message.Port : ZString.Empty);
				this.discharge = (message.Direction == Constants.PortDirection.Discharge ? message.Port : ZString.Empty);
				this.equipment = equipment;
				this.consignments = consignments;
			}

			#region IPortAuthorityMessagingData Members

			public PortAuthorityMessageFunction MessageFunction
			{
				get { return messageFunction; }
			}

			public DateTime MessagePrepared
			{
				get { return messagePrepared.ToDateTime(); }
			}

			public string VesselName
			{
				get { return vessel; }
			}

			public string VesselLloyds
			{
				get { return lloyds; }
			}

			public string Voyage
			{
				get { return voyage; }
			}

			public string Load
			{
				get { return load; }
			}

			public string Discharge
			{
				get { return discharge; }
			}

			public IEnumerable<IPortAuthorityEquipmentData> Equipment
			{
				get { return equipment; }
			}

			public IEnumerable<IPortAuthorityConsignmentData> Consignments
			{
				get { return consignments; }
			}

			#endregion

			readonly PortAuthorityMessageFunction messageFunction;
			readonly ZDateTime messagePrepared;
			readonly ZString vessel;
			readonly ZString voyage;
			readonly ZString lloyds;
			readonly ZString load;
			readonly ZString discharge;
			readonly IPortAuthorityEquipmentData[] equipment;
			readonly IPortAuthorityConsignmentData[] consignments;
		}

		#endregion

		#region Consignment

		[System.Diagnostics.DebuggerDisplay("BOL = '{BillOfLading}'")]
		class Consignment : IPortAuthorityConsignmentData
		{
			public Consignment(BillOfLading shipment, PortAuthority message, IPortAuthorityGoodsData[] goods, int consignmentNumber)
			{
				this.consignmentNumber = consignmentNumber;
				this.isWaybill = (shipment.JS_ReleaseType == Constants.ShipmentReleaseTypes.SeaWaybill);
				this.billOfLading = shipment.JS_HouseBill;
				this.packingMode = shipment.JS_PackingMode;
				this.goods = goods;

				switch (message.Direction)
				{
					case Constants.PortDirection.Load:
						this.consignorNameAndAddress = NameAndAddress(shipment.ConsignorDocumentaryAddress);
						this.consigneeNameAndAddress = "";

						this.countryOfOrigin = "";
						this.portOfOrigin = shipment.JS_RL_NKOrigin;
						this.portOfLoading = "";
						this.portOfDischarge = shipment.Transports.OfType<Transport>().FirstOrDefault(transport => transport.JW_RL_NKLoadPort == message.Port)?.JW_RL_NKDiscPort ?? shipment.JS_NKDischargePort;
						this.portOfDestination = "";
						this.countryOfDestination = shipment.JS_RL_NKDestination;
						break;

					case Constants.PortDirection.Discharge:
						this.consignorNameAndAddress = "";
						this.consigneeNameAndAddress = NameAndAddress(shipment.ConsigneeDocumentaryAddress);

						this.countryOfOrigin = shipment.JS_RL_NKOrigin;
						this.portOfOrigin = "";
						this.portOfLoading = shipment.Transports.OfType<Transport>().FirstOrDefault(transport => transport.JW_RL_NKDiscPort == message.Port)?.JW_RL_NKLoadPort ?? shipment.JS_NKLoadPort;
						this.portOfDischarge = "";
						this.portOfDestination = shipment.JS_RL_NKDestination;
						this.countryOfDestination = "";
						break;

					default:
						throw new InvalidOperationException();
				}
			}

			#region IPortAuthorityConsignmentData Members

			public string BillOfLading
			{
				get { return billOfLading; }
			}

			public string PackingMode
			{
				get { return packingMode; }
			}

			public bool IsWaybill
			{
				get { return isWaybill; }
			}

			public int ConsignmentNumber
			{
				get { return consignmentNumber; }
			}

			public string ConsignorNameAndAddress
			{
				get { return consignorNameAndAddress; }
			}

			public string ConsigneeNameAndAddress
			{
				get { return consigneeNameAndAddress; }
			}

			public string PortOfOrigin
			{
				get { return portOfOrigin; }
			}

			public string PortOfLoading
			{
				get { return portOfLoading; }
			}

			public string CountryOfOrigin
			{
				get { return countryOfOrigin; }
			}

			public string PortOfDestination
			{
				get { return portOfDestination; }
			}

			public string PortOfDischarge
			{
				get { return portOfDischarge; }
			}

			public string CountryOfDestination
			{
				get { return countryOfDestination; }
			}

			public IEnumerable<IPortAuthorityGoodsData> Goods
			{
				get { return goods; }
			}

			#endregion

			static ZString NameAndAddress(JobDocAddress address)
			{
				return string.Format(CultureInfo.InvariantCulture,
					"{0}\n{1}\n{2}\n{3}\n{4} {5}",
					address.E2_CompanyNameTruncated,
					address.E2_Address1,
					address.E2_Address2,
					address.E2_City,
					address.E2_State,
					address.E2_Postcode);
			}

			readonly int consignmentNumber;
			readonly ZBool isWaybill;
			readonly ZString billOfLading;
			readonly ZString packingMode;
			readonly ZString portOfOrigin;
			readonly ZString portOfLoading;
			readonly ZString countryOfOrigin;
			readonly ZString portOfDestination;
			readonly ZString portOfDischarge;
			readonly ZString countryOfDestination;
			readonly ZString consignorNameAndAddress;
			readonly ZString consigneeNameAndAddress;
			readonly IPortAuthorityGoodsData[] goods;
		}

		#endregion

		#region Goods

		[System.Diagnostics.DebuggerDisplay("{ItemNumber} - {GoodsDescription} ({containerNumber})")]
		class Goods : IPortAuthorityGoodsData
		{
			public Goods(BillOfLading shipment, AgencyShipmentPackLine packline, BillOfLadingContainer container, int itemNumber, PortAuthority message)
			{
				this.itemNumber = itemNumber;
				this.packageCount = packline.JL_PackageCount;
				this.marksAndNumbers = packline.JL_MarksAndNumbers.Trim();
				this.containerNumber = shipment.IsTopLevelPacksMode ? packline.JL_RefNumber : (container == null ? ZString.Empty : container.JC_ContainerNum);
				this.cubicMeters = Constants.Volume.Convert(packline.JL_ActualVolume, packline.JL_ActualVolumeUQ, Constants.Volume.CubicMetres);
				this.killograms = Constants.Weight.Convert(packline.JL_ActualWeight, packline.JL_ActualWeightUQ, Constants.Weight.Kilograms);
				this.harmonisedCode = packline.JL_HarmonisedCode;

				if (packline.JL_HarmonisedCode.IsEmpty)
				{
					this.goodsDescription = packline.JL_DetailedDescription;
				}
				else
				{
					this.goodsDescription = packline.JL_HarmonisedCode + "\n" + packline.JL_DetailedDescription;
				}

				if (message.Port == "AUSYD")
				{
					var postalCode = ZString.Empty;
					switch (message.Direction)
					{
						case Constants.PortDirection.Discharge:
							var consigneePickupDeliveryAddress = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
							postalCode =
								consigneePickupDeliveryAddress != null && !consigneePickupDeliveryAddress.Postcode.IsEmpty ? consigneePickupDeliveryAddress.Postcode :
								shipment.ConsigneeDocumentaryAddress != null && !shipment.ConsigneeDocumentaryAddress.Postcode.IsEmpty ? shipment.ConsigneeDocumentaryAddress.Postcode :
								ZString.Empty;
							this.containerYardAddress = NameAndAddress(postalCode, container?.ArrivalContainerYardAddress);
							break;
						case Constants.PortDirection.Load:
							var consignorPickupDeliveryAddress = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
							postalCode =
								consignorPickupDeliveryAddress != null && !consignorPickupDeliveryAddress.Postcode.IsEmpty ? consignorPickupDeliveryAddress.Postcode :
								shipment.ConsignorDocumentaryAddress != null && !shipment.ConsignorDocumentaryAddress.Postcode.IsEmpty ? shipment.ConsignorDocumentaryAddress.Postcode :
								ZString.Empty;
							this.containerYardAddress = NameAndAddress(postalCode, container?.DepartureContainerYardAddress);
							break;
						default:
							throw new InvalidOperationException();
					}
				}
			}

			#region IPortAuthorityGoodsData Members

			public string ContainerNumber
			{
				get { return containerNumber; }
			}

			public IEnumerable<string> ContainerNumbers
			{
				get
				{
					if (!containerNumber.IsEmpty)
					{
						yield return containerNumber;
					}
				}
			}

			public decimal CubicMetres
			{
				get { return cubicMeters; }
			}

			public string GoodsDescription
			{
				get { return goodsDescription; }
			}

			public int ItemNumber
			{
				get { return itemNumber; }
			}

			public decimal Kilograms
			{
				get { return killograms; }
			}

			public string MarksAndNumbers
			{
				get { return marksAndNumbers; }
			}

			public string PackageCode
			{
				get { return "PK"; }
			}

			public string HarmonisedCode
			{
				get { return harmonisedCode; }
			}

			public int PackageCount
			{
				get { return packageCount; }
			}

			public string ContainerYardAddress
			{
				get { return containerYardAddress; }
			}

			#endregion

			static ZString NameAndAddress(ZString postalCode, OrgAddress address)
			{
				if (address != null)
				{
					return string.Format(CultureInfo.InvariantCulture,
					"POSTCODE-{0}\nECP-{1}\n{2}\n{3}\n{4} {5}",
					postalCode,
					address.CompanyName,
					address.Address1,
					address.Address2,
					address.City,
					address.Postcode);
				}

				return string.Format(CultureInfo.InvariantCulture, "POSTCODE-{0}", postalCode);
			}

			readonly ZInt itemNumber;
			readonly ZInt packageCount;
			readonly ZString goodsDescription;
			readonly ZString marksAndNumbers;
			readonly ZString containerNumber;
			readonly ZDecimal cubicMeters;
			readonly ZDecimal killograms;
			readonly ZString harmonisedCode;
			readonly ZString containerYardAddress;
		}

		#endregion

		#region EmptyGoods

		[System.Diagnostics.DebuggerDisplay("{ItemNumber}E - EMPTY CONTAINERS")]
		class EmptyGoods : IPortAuthorityGoodsData
		{
			public EmptyGoods(List<BillOfLadingContainer> containers, int itemNumber)
			{
				decimal totalTareWeight = 0m;
				List<string> containerNumbers = new List<string>();

				foreach (BillOfLadingContainer container in containers)
				{
					totalTareWeight += container.JC_TareWeight;
					containerNumbers.Add(container.JC_ContainerNum);
				}

				this.itemNumber = itemNumber;
				this.containerNumbers = containerNumbers.ToArray();
				this.totalTareWeight = totalTareWeight;
			}

			#region IPortAuthorityGoodsData Members

			public int ItemNumber
			{
				get { return itemNumber; }
			}

			public string ContainerNumber
			{
				get { return string.Empty; }
			}

			public IEnumerable<string> ContainerNumbers
			{
				get { return containerNumbers; }
			}

			public string GoodsDescription
			{
				get { return (NoResString)"EMPTY CONTAINER"; } // hard-coded constant
			}

			public string MarksAndNumbers
			{
				get { return string.Empty; }
			}

			public decimal Kilograms
			{
				get { return totalTareWeight; }
			}

			public decimal CubicMetres
			{
				get { return 0m; }
			}

			public int PackageCount
			{
				get { return containerNumbers.Length; }
			}

			public string PackageCode
			{
				get { return "BX"; }
			}

			public string HarmonisedCode
			{
				get { return string.Empty; }
			}

			public string ContainerYardAddress
			{
				get { return containerYardAddress; }
			}

			#endregion

			readonly int itemNumber;
			readonly string[] containerNumbers;
			readonly decimal totalTareWeight;
			readonly ZString containerYardAddress;
		}

		#endregion

		#region Equipment

		[System.Diagnostics.DebuggerDisplay("Container Number = '{ContainerNumber}'")]
		class Equipment : IPortAuthorityEquipmentData
		{
			[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
			public Equipment(BillOfLadingContainer container, ZString direction)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}

				if (container.Booking == null)
				{
					throw new ArgumentException("the container must be associated with a booking", "booking");
				}

				this.containerNumber = container.JC_ContainerNum;
				this.isoType = "";
				this.isEmpty = true;
				this.containerStatus = GetContainerStatus(container.Booking, direction);

				Populate(container, direction);
			}

			[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
			public void Populate(BillOfLadingContainer container, ZString direction)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}

				if (container.Booking == null)
				{
					throw new ArgumentException("the container must be associated with a booking", "booking");
				}

				if (isoType.IsEmpty && container.Container != null)
				{
					isoType = container.Container.RC_ISOType;
				}

				if (GetContainerStatus(container.Booking, direction) == PortAuthorityContainerStatus.Transhipment)
				{
					containerStatus = PortAuthorityContainerStatus.Transhipment;
				}

				this.isEmpty &= container.PackLines.Count == 0;
			}

			static PortAuthorityContainerStatus GetContainerStatus(AgencyShipment shipment, ZString direction)
			{
				switch (direction)
				{
					case Constants.PortDirection.Load:
						return IsTranship(shipment.JS_NKLoadPort, shipment.JS_RL_NKOrigin)
							? PortAuthorityContainerStatus.Transhipment
							: PortAuthorityContainerStatus.Export;

					case Constants.PortDirection.Discharge:
						return IsTranship(shipment.JS_NKDischargePort, shipment.JS_RL_NKDestination)
							? PortAuthorityContainerStatus.Transhipment
							: PortAuthorityContainerStatus.Import;

					default:
						return PortAuthorityContainerStatus.Transhipment;
				}
			}

			static bool IsTranship(ZString loadDischarge, ZString originDestination)
			{
				return !loadDischarge.IsEmpty && loadDischarge.SubstringSafe(0, 2) != originDestination.SubstringSafe(0, 2);
			}

			#region IPortAuthorityEquipmentData Members

			public string ContainerNumber
			{
				get { return containerNumber; }
			}

			public PortAuthorityContainerStatus ContainerStatus
			{
				get { return containerStatus; }
			}

			public string ContainerISOCode
			{
				get { return isoType; }
			}

			public bool IsEmpty
			{
				get { return isEmpty; }
			}

			#endregion

			readonly ZString containerNumber;
			PortAuthorityContainerStatus containerStatus;
			ZString isoType;
			ZBool isEmpty;
		}

		#endregion

		static string ExtractErrorDetail(BusinessObject obj)
		{
			var notifications = new ZNotificationCollector(obj, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			var errors = notifications.GetErrors();
			var messageErrors = notifications.GetMessageErrors();

			return errors.Concat(messageErrors).ToUniqueMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
		}
	}
}


