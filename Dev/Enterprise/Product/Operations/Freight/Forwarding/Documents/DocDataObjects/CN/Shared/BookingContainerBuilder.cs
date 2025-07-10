using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using ICodeDescription = Enterprise.DocumentVisualizer.DocDataObjects.ICodeDescription;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class BookingContainerBuilder
	{
		public BookingContainerBuilder(IContext context, Container container, ZString bookingNumber, object containerID)
		{
			this.context = Argument.NotNull(context, nameof(context));
			this.container = Argument.NotNull(container, nameof(container));
			this.bookingNumber = bookingNumber;
			this.containerID = containerID;
		}

		readonly IContext context;
		readonly Container container;
		readonly ZString bookingNumber;
		readonly object containerID;

		public BookingContainer Build()
		{
			var bookingcontainer = new BookingContainer(containerID);

			bookingcontainer.PackingLines = container.PackingLines.Where(p => p.ExportReferenceNumber == bookingNumber).ToArray();
			PopulateGeneralInfo(bookingcontainer);
			PopulateRefrigeration(bookingcontainer);
			PopulateSeals(bookingcontainer);
			PopulateExportInfo(bookingcontainer);
			PopulateWeightAndVolume(bookingcontainer);
			PopulateMeasures(bookingcontainer);
			PopulateContainerVerification(bookingcontainer);
			PopulateCollections(bookingcontainer);

			return bookingcontainer;
		}

		#region PopulateGeneralInfo

		void PopulateGeneralInfo(BookingContainer bookingcontainer)
		{
			bookingcontainer.Number = container.Number;

			var containerType = container.Type;
			if (containerType != null)
			{
				bookingcontainer.Type = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
				{
					Code = containerType.Code,
					ISOCode = containerType.ISOCode,
					Type = CreateCodeDescription(container.Type?.Type)
				};
			}
			bookingcontainer.IsNonOperativeReefer = container.IsNonOperativeReefer;

			bookingcontainer.ContainerMode = CreateCodeDescription(container.ContainerMode);
			bookingcontainer.ContainerQuality = CreateCodeDescription(container.ContainerQuality);
			bookingcontainer.ContainerStatus = CreateCodeDescription(container.ContainerStatus);
			bookingcontainer.Commodity = CreateCodeDescription(container.Commodity);

			bookingcontainer.GoodsValue = new Measurement
			{
				Value = container.GoodsValue?.Value ?? 0,
				Unit = CreateCodeDescription(container.GoodsValue?.Unit)
			};

			bookingcontainer.ContainerCount = container.ContainerCount;
			bookingcontainer.IsEmpty = container.IsEmpty;
			bookingcontainer.IsPartOf = container.IsPartOf;
			bookingcontainer.IsShipperOwned = container.IsShipperOwned;
			bookingcontainer.IsSealOk = container.IsSealOk;
		}

		#endregion

		#region PopulateRefrigeration

		void PopulateRefrigeration(BookingContainer bookingcontainer)
		{
			bookingcontainer.HasControlledAtmosphere = container.HasControlledAtmosphere;
			bookingcontainer.TemperatureRecorderSerialNumber = container.TemperatureRecorderSerialNumber;
			bookingcontainer.RefrigGeneratorID = container.RefrigGeneratorID;

			bookingcontainer.SetTemperature = new Measurement
			{
				Value = container.SetTemperature?.Value ?? 0,
				Unit = CreateCodeDescription(container.SetTemperature?.Unit, context.TemperatureUnits)
			};

			bookingcontainer.Humidity = new Measurement
			{
				Value = (byte)(container.Humidity?.Value ?? 0),
				Unit = CreateCodeDescription(container.Humidity?.Unit, context.Humidity)
			};

			bookingcontainer.AirVentFlow = new Measurement
			{
				Value = container.AirVentFlow?.Value ?? 0,
				Unit = CreateCodeDescription(container.AirVentFlow?.Unit, context.AirVentFlow)
			};

			bookingcontainer.Genset = container.HasControlledAtmosphere && container.Genset;
		}

		#endregion

		#region PopulateSeals

		void PopulateSeals(BookingContainer bookingcontainer)
		{
			bookingcontainer.Seal = container.Seal;
			bookingcontainer.SealPartyType = CreateCodeDescription(container.SealPartyType);

			bookingcontainer.SecondSeal = container.SecondSeal;
			bookingcontainer.SecondSealPartyType = CreateCodeDescription(container.SecondSealPartyType);

			bookingcontainer.ThirdSeal = container.ThirdSeal;
			bookingcontainer.ThirdSealPartyType = CreateCodeDescription(container.ThirdSealPartyType);
		}

		#endregion

		#region PopulateExportInfo

		void PopulateExportInfo(BookingContainer bookingcontainer)
		{
			bookingcontainer.EmptyRequired = container.EmptyRequired;
			bookingcontainer.ReleaseNumber = container.ReleaseNumber;
			bookingcontainer.ContainerParkEmptyPickupGateOut = container.ContainerParkEmptyPickupGateOut;

			bookingcontainer.DepartureCartageAdvised = container.DepartureCartageAdvised;
			bookingcontainer.DepartureCartageReference = container.DepartureCartageReference;
			bookingcontainer.DepartureCartageComplete = container.DepartureCartageComplete;
			bookingcontainer.DepartureTruckWaitCost = container.DepartureTruckWaitCost;
			bookingcontainer.DepartureTruckWaitTime = container.DepartureTruckWaitTime;
			bookingcontainer.DepartureDeliveryByRail = container.DepartureDeliveryByRail;
			bookingcontainer.DepartureSlotDateTime = container.DepartureSlotDateTime;
			bookingcontainer.DepartureSlotReference = container.DepartureSlotReference;
			bookingcontainer.DepartureEstimatedPickup = container.DepartureEstimatedPickup;
			bookingcontainer.DepartureContainerYard = AddressBuilder.Create(context, container.DepartureContainerYard);
			bookingcontainer.ExportDepotCustomsReference = container.ExportDepotCustomsReference;

			bookingcontainer.FCLWharfGateIn = container.FCLWharfGateIn;
			bookingcontainer.FCLOnBoardVessel = container.FCLOnBoardVessel;
		}

		#endregion

		#region PopulateWeightAndVolume

		void PopulateWeightAndVolume(BookingContainer bookingcontainer)
		{
			var packlines = bookingcontainer.PackingLines;

			bookingcontainer.GoodsWeight = new Measurement
			{
				Value = packlines
						.Where(p => p.Weight != null && p.Weight.Unit != null)
						.Sum(p => Core.Constants.Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, Core.Constants.Weight.Kilograms)),

				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = container.GoodsWeight?.Unit.Code ?? ZString.Empty
				}
			};

			bookingcontainer.TareWeight = CreateMeasure(container.TareWeight, context.WeightUnits);
			bookingcontainer.Dunnage = CreateMeasure(container.Dunnage, context.WeightUnits);
			bookingcontainer.NetWeight = CreateMeasure(container.NetWeight, context.WeightUnits);
			bookingcontainer.GrossWeight = CreateMeasure(container.GrossWeight, context.WeightUnits);

			bookingcontainer.Volume = new Measurement
			{
				Value = packlines
						.Where(p => p.Volume != null && p.Volume.Unit != null)
						.Sum(p => Core.Constants.Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, Core.Constants.Volume.CubicMetres)),
				Unit = CreateCodeDescription(container.Volume?.Unit)
			};

			bookingcontainer.WeightCapacity = CreateMeasure(container.WeightCapacity);
			bookingcontainer.VolumeCapacity = CreateMeasure(container.VolumeCapacity);
		}

		#endregion

		#region PopulateMeasures

		void PopulateMeasures(BookingContainer bookingcontainer)
		{
			bookingcontainer.OverhangFront = CreateMeasure(container.OverhangFront);
			bookingcontainer.OverhangBack = CreateMeasure(container.OverhangBack);
			bookingcontainer.OverhangLeft = CreateMeasure(container.OverhangLeft);
			bookingcontainer.OverhangRight = CreateMeasure(container.OverhangRight);
			bookingcontainer.OverhangHeight = CreateMeasure(container.OverhangHeight);

			bookingcontainer.TotalHeight = CreateMeasure(container.TotalHeight);
			bookingcontainer.TotalWidth = CreateMeasure(container.TotalWidth);
			bookingcontainer.TotalLength = CreateMeasure(container.TotalLength);
		}

		Measurement CreateMeasure(IMeasurement containerMeasure, object lookups = null)
		{
			return new Measurement
			{
				Value = containerMeasure?.Value ?? 0,
				Unit = CreateCodeDescription(containerMeasure?.Unit, lookups)
			};
		}

		#endregion

		#region PopulateContainerVerification

		void PopulateContainerVerification(BookingContainer bookingcontainer)
		{
			bookingcontainer.VerifiedMethod = CreateCodeDescription(container.VerifiedMethod);
			bookingcontainer.VerifiedStatus = CreateCodeDescription(container.VerifiedStatus);
			bookingcontainer.VerifiedByAddress = AddressBuilder.Create(context, container.VerifiedByAddress);
			bookingcontainer.VerifiedDate = container.VerifiedDate;
		}

		#endregion

		#region PopulateCollections

		void PopulateCollections(BookingContainer bookingcontainer)
		{
			bookingcontainer.Numbers = container.Numbers?.Select(AsReferenceNumber).ToArray();
			bookingcontainer.AdditionalServices = container.AdditionalServices?.Select(AsAdditionalService).ToArray();
			bookingcontainer.Milestones = container.Milestones?.Select(AsMilestone).ToArray();
		}

		ReferenceNumber AsReferenceNumber(IReferenceNumber referenceNumber)
		{
			return new ReferenceNumber
			{
				Value = referenceNumber.Value,
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = referenceNumber.CountryOfIssue?.Code ?? ZString.Empty
				},
				Type = CreateCodeDescription(referenceNumber.Type)
			};
		}

		AdditionalService AsAdditionalService(IAdditionalService additionalService)
		{
			return new AdditionalService
			{
				Contractor = AddressBuilder.Create(context, additionalService.Contractor),
				Location = AddressBuilder.Create(context, additionalService.Location),
				ServiceCode = CreateCodeDescription(additionalService.ServiceCode),
				Booked = additionalService.Booked,
				Completed = additionalService.Completed,
				Duration = additionalService.Duration,
				ServiceCount = additionalService.ServiceCount,
				References = additionalService.References,
				ServiceNote = additionalService.ServiceNote
			};
		}

		Milestone AsMilestone(IMilestone milestone)
		{
			return new Milestone
			{
				ActualDate = milestone.ActualDate,
				EstimatedDate = milestone.EstimatedDate,
				Sequence = milestone.Sequence,
				ConditionReference = milestone.ConditionReference,
				ConditionType = milestone.ConditionType,
				Description = milestone.Description,
				EventCode = milestone.EventCode
			};
		}

		#endregion

		#region CreateCodeDescription

		CodeDescription CreateCodeDescription(ICodeDescription containerCodeDescription, object lookups = null)
		{
			return new CodeDescription((lookups ?? containerCodeDescription?.Codes) as ICodeDescriptionPairList ?? new CodeDescriptionPairList())
			{
				Code = containerCodeDescription?.Code ?? ZString.Empty
			};
		}

		#endregion
	}
}
