using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	internal class eManifestDocumentWrapper : NonPersistentBusinessObject, IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		public eManifestDocumentWrapper(Trip trip)
		{
			this.trip = trip;
		}

		public ZString IITDescription
		{
			get
			{
				var builder = new ZStringBuilder();
				var conveynance = trip.Conveyance;
				if (conveynance.BJ_EmptyIITsCoveredByCarrier)
				{
					builder.Append('*' + IITEntityIndicatorCodes.Descriptions.EC);
				}
				if (conveynance.BJ_EmptyIITsCoveredByImporter)
				{
					builder.Append('*' + IITEntityIndicatorCodes.Descriptions.EI);
				}
				if (conveynance.BJ_MerchandiseAndIITsCoveredByCarrier)
				{
					builder.Append('*' + IITEntityIndicatorCodes.Descriptions.MC);
				}
				if (conveynance.BJ_MerchandiseAndIITsCoveredByImporter)
				{
					builder.Append('*' + IITEntityIndicatorCodes.Descriptions.MI);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString ConveyanceLicensePlate
		{
			get { return trip.Conveyance.BJ_RegistrationNumber; }
		}

		public ZString TripReference
		{
			get { return trip.BH_CarrierSCAC + trip.BH_VoyageNumber; }
		}

		public ZString QRCodeText
		{
			get
			{
				var conveyance = trip.Conveyance;
				var equipment = trip.Equipment.FirstOrDefault();
				return ZString.Format("V01 {0,-25} {1:yyyyMMdd} {2,-2}{3,3}{4,-7} {5,-2}{6,3}{7,-7}"
					, TripReference
					, EstimatedDateOfArrival
					, conveyance.BJ_RN_NKRegistrationCountry
					, conveyance.BJ_RW_NKRegistrationState
					, conveyance.BJ_RegistrationNumber
					, equipment?.BJ_RN_NKRegistrationCountry
					, equipment?.BJ_RW_NKRegistrationState
					, equipment?.BJ_RegistrationNumber);
			}
		}

		public ZString ConveyanceLicensePlateAndDescription
		{
			get
			{
				var builder = new ZStringBuilder();
				var refEquipment = trip.Conveyance.RefEquipment;
				if (refEquipment != null)
				{
					builder.AppendLine(refEquipment.RQ_DescriptionMultilingual);
					builder.AppendIfNotEmpty(refEquipment.RQ_RN_NKRegistrationCountry + " ");
					builder.Append(refEquipment.RQ_Registration);
				}
				return builder.ToString();
			}
		}

		public ZString TrailerPlates
		{
			get
			{
				var plates = from equipment in trip.Equipment
							 let refEquipment = equipment.RefEquipment
							 where refEquipment != null && refEquipment.RQ_IsVehicle
							 select refEquipment.RQ_Registration;
				return new ZStringBuilder(plates).ToStringWithDelimiterBetweenAppends(CommaDelimiter);
			}
		}

		public OrgAddress Importer
		{
			get { return trip.Importer; }
		}

		public ZString OtherEquipmentIds
		{
			get
			{
				var ids = from equipment in trip.Equipment
						  let refEquipment = equipment.RefEquipment
						  where refEquipment != null && !refEquipment.RQ_IsVehicle
						  select refEquipment.RQ_Registration;
				return new ZStringBuilder(ids).ToStringWithDelimiterBetweenAppends(CommaDelimiter);
			}
		}

		public ZDateTime ManifestSubmitted
		{
			get
			{
				return (from StmALog log in trip.Logs.GetAllLogs()
						where log.SL_SE_NKEvent == Events.CustomsManifestStatusCode
						orderby log.SL_EventTime descending
						select log.SL_EventTime).FirstOrDefault();
			}
		}

		public ZString HazardousMaterials
		{
			get { return trip.HasHazmatShipments ? CrewMember.Yes : CrewMember.No; }
		}

		public ZString ForeignPortOfLanding
		{
			get
			{
				var first = trip.Shipments.Select(s => s.B0_PortOfLadingKCode).FirstOrDefault();
				var portCode = trip.Shipments.All(s => s.B0_PortOfLadingKCode == first) ? first : (ZString)Various;
				var foreignPort = trip.Lookups.ScheduleKPortCodes.Where(x => x.ZZD_Code == portCode).FirstOrDefault();
				if (foreignPort == null)
				{
					return portCode;
				}
				var builder = new ZStringBuilder();
				if (!string.IsNullOrEmpty(portCode))
				{
					builder.Append(portCode);
					builder.AppendIfNotEmpty(foreignPort.ZZD_Description);
				}
				return builder.ToStringWithDelimiterBetweenAppends(" - ");
			}
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get { return trip.BH_ETA; }
		}

		public ZString FirstExpectedPortOfArrival
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(trip.Factory, trip.BH_PortUnladingDCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return port == null ? ZString.Empty : port.ZZD_Code + " - " + port.ZZD_Description;
			}
		}

		public CrewWrapper PersonInCharge
		{
			get
			{
				return (from crewMember in trip.CrewMembers
						where crewMember.CP_Type == CrewTypes.Codes.ResponsibleParty
						select new CrewWrapper(crewMember)).FirstOrDefault();
			}
		}

		public BusinessObjectCollectionWrapper<CrewWrapper> CrewMembers
		{
			get
			{
				var collection = from crewMember in trip.CrewMembers
								 where crewMember.CP_Type == CrewTypes.Codes.CrewMember
								 select new CrewWrapper(crewMember);
				return new BusinessObjectCollectionWrapper<CrewWrapper>(collection);
			}
		}

		public BusinessObjectCollectionWrapper<ShipmentWrapper> Shipments
		{
			get
			{
				IEnumerable<ShipmentWrapper> shipments;
				if (trip.IsFromHVLV)
				{
					shipments = new List<ShipmentWrapper>();
				}
				else
				{
					shipments = from shipment in trip.Shipments
								where !trip.IsFinalized || shipment.IsLinked
								select new ShipmentWrapper(shipment, ForeignPortOfLanding == Various);
				}
				return new BusinessObjectCollectionWrapper<ShipmentWrapper>(shipments);
			}
		}

		public ZString ControlNumber => RefSysConfigLoader.GetStringValue(CBP7533CN);

		public ZString ExpirationDate => RefSysConfigLoader.GetStringValue(CBP7533ED);

		public ZString RevisionDate => RefSysConfigLoader.GetStringValue(CBP7533RD);

		RefSysConfig.Loader RefSysConfigLoader
		{
			get { return refSysConfigLoader ?? (refSysConfigLoader = new RefSysConfig.Loader(trip.Factory)); }
		}
		RefSysConfig.Loader refSysConfigLoader;

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => trip.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusInBondHeaderSchema.Constants.Prefix;

		#endregion

		#region Crew Wrapper

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class CrewWrapper : NonPersistentBusinessObject
		{
			public CrewWrapper(CrewMember crew)
			{
				this.crew = crew;
			}

			public ZString FullName
			{
				get { return crew.CP_FullName; }
			}

			public ZDate DateOfBirth
			{
				get { return crew.CP_DateOfBirth.Date; }
			}

			readonly CrewMember crew;
		}

		#endregion

		#region ShipmentWrapper

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class ShipmentWrapper : NonPersistentBusinessObject
		{
			public ShipmentWrapper(Shipment shipment, bool printForeignPortOfLanding)
			{
				this.printForeignPortOfLanding = printForeignPortOfLanding;
				this.shipment = FallbackToOriginalShipmentIfSplit(shipment);
				inBond = shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond ? shipment.InBond : null;
			}

			Shipment FallbackToOriginalShipmentIfSplit(Shipment splitShipment)
			{
				if (splitShipment.B0_ShipmentType == ShipmentTypes.Codes.SplitShipment)
				{
					var query = new ZQuery(CusInBondBillSchema.B0_MasterBillNumber, splitShipment.B0_MasterBillNumber);
					query.AddToFilter(CusInBondBillSchema.B0_ShipmentType, SQLComparisonOperator.NotEqual, splitShipment.B0_ShipmentType);
					query.AddToFilter(CusInBondBillSchema.B0_BoardedQuantity, SQLComparisonOperator.NotEqual, ZInt.Zero);
					splitShipment = splitShipment.Factory.LoadTop1<Shipment>(query);
				}
				return splitShipment;
			}

			public ZString ShipmentType
			{
				get
				{
					return inBond != null
							? shipment.InBond.BM_InBondEntryType.ToCodeDescription<InbondTypes>()
							: (ZString)new ShipmentTypes().GetDescriptionFromCode(shipment.B0_ShipmentType);
				}
			}

			public ZString ShipmentControlNumber
			{
				get { return shipment.ShipmentControlNumber; }
			}

			public ZString Packages
			{
				get
				{
					var builder = new ZStringBuilder();
					if (!shipment.B0_ManifestQty.IsEmpty)
					{
						builder.Append(shipment.B0_ManifestQty.ToString());
						builder.AppendIfNotEmpty(shipment.B0_ManifestUQ);
					}
					return builder.ToStringWithDelimiterBetweenAppends(SpaceDelimiter);
				}
			}

			public ZString CargoGrossWeight
			{
				get
				{
					var builder = new ZStringBuilder();
					if (!shipment.B0_Weight.IsEmpty)
					{
						builder.Append(WeightUnits.ConvertWeightToKilogramsIfRequired(shipment.B0_Weight, shipment.B0_WeightUQ).ToStringTrimZeros());
						builder.AppendIfNotEmpty(WeightUnits.GetWeightUOM(shipment.B0_WeightUQ));
					}
					return builder.ToStringWithDelimiterBetweenAppends(SpaceDelimiter);
				}
			}

			public ZString DescriptionOfCargo
			{
				get
				{
					var result = shipment.B0_DescriptionOfCargo;
					if (result.IsEmpty)
					{
						var descriptions = shipment.Commodities.Select(c => c.BY_Description).ToArray();
						var first = descriptions.FirstOrDefault();
						result = descriptions.All(d => d == first) ? first : ZString.Empty;
					}
					return result;
				}
			}

			public ZString InbondNumber
			{
				get { return inBond != null ? inBond.InBondNumber : ZString.Empty; }
			}

			public ZString Equipment
			{
				get
				{
					var equipmentIds = (from commodity in shipment.Commodities
										where commodity.BY_BJ_Equipment.IsValid && commodity.Equipment != null
										select commodity.Equipment.BJ_RegistrationNumber).Distinct();
					return new ZStringBuilder(equipmentIds).ToStringWithDelimiterBetweenAppends(CommaDelimiter);
				}
			}

			public ZString Consignee
			{
				get { return shipment.Consignee.E2_CompanyNameTruncated; }
			}

			public ZString ForeignPortOfLanding
			{
				get
				{
					if (!printForeignPortOfLanding)
					{
						return ZString.Empty;
					}
					else
					{
						var foreignPort = shipment.Lookups.ScheduleKPortCodes.Where(x => x.ZZD_Code == shipment.B0_PortOfLadingKCode).FirstOrDefault();
						if (foreignPort == null)
						{
							return ZString.Empty;
						}
						var builder = new ZStringBuilder();
						if (!string.IsNullOrEmpty(shipment.B0_PortOfLadingKCode))
						{
							builder.Append(shipment.B0_PortOfLadingKCode);
							builder.AppendIfNotEmpty(foreignPort.ZZD_Description);
						}
						return builder.ToStringWithDelimiterBetweenAppends(" - ");
					}
				}
			}

			readonly Shipment shipment;
			readonly InBond inBond;
			readonly bool printForeignPortOfLanding;
		}

		#endregion

		#region BusinessObjectCollectionWrapper
		internal sealed class BusinessObjectCollectionWrapper<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
		{
			public BusinessObjectCollectionWrapper(IEnumerable<T> collection)
			{
				foreach (var obj in collection)
				{
					Add(obj);
				}
			}

			#region Overrides of NonPersistentBusinessObjectCollection

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotSupportedException();
			}

			protected override bool AllowNewCore
			{
				get { return false; }
			}

			#endregion
		}

		#endregion

		readonly Trip trip;
		const string CommaDelimiter = ", ";
		const string SpaceDelimiter = " ";
		const string Various = "VARIOUS";
		const string CBP7533CN = "CBP7533CN";
		const string CBP7533ED = "CBP7533ED";
		const string CBP7533RD = "CBP7533RD";

		#region ISourceIdentifierProvider members

		ZGuid ISourceIdentifierProvider.SourceIdentifier => trip.PK;

		#endregion
	}
}
