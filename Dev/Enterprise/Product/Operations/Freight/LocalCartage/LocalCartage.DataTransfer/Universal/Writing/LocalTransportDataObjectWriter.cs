using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class LocalTransportDataObjectWriter : TopLevelDataObjectWriter<CommonCartage, UniversalShipment>
	{
		internal LocalTransportDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.LocalTransport;
		}

		protected override void PopulateDataObject(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			PopulateJobDetails(sourceBO, dataObject);
			PopulateModes(sourceBO, dataObject);
			PopulateGoodsAndTotals(sourceBO, dataObject);
			PopulateContainers(sourceBO, dataObject);
			PopulatePackages(sourceBO, dataObject);
			PopulateReferences(sourceBO, dataObject);
			PopulateSchedule(sourceBO, dataObject);
			PopulateLegs(sourceBO, dataObject);
			PopulateInstructions(sourceBO, dataObject);
			PopulateNotes(sourceBO, dataObject);
			PopulateDates(sourceBO, dataObject);
		}

		void PopulateJobDetails(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			if (sourceBO.Branch != null)
			{
				dataObject.Branch = UniversalDataBuss.DataObjects.Branch.New(sourceBO.Branch);

				var client = sourceBO.LocalClientAddress;
				if (client != null)
				{
					dataObject.AddOrgAddress(writeManager, client, AddressTypes.SendersLocalClient);
				}
			}
		}

		void PopulateModes(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.JJ_ShippingTransportMode, sourceBO.BindToLists.ShippingTransportModeList); // Connecting Transport Mode
			dataObject.TransportBookingDirection = ListHelper.GetWithDescription<TransportBookingDirection>(sourceBO.JJ_Direction, sourceBO.BindToLists.Directions);
			dataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(sourceBO.JJ_ContainerMode, sourceBO.BindToLists.ContainerModes);
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(sourceBO.JJ_RS_NKServiceLevel, sourceBO.BindToLists.ServiceLevels);
			dataObject.LocalTransportJobType = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(sourceBO.JJ_E3_NKJobType, sourceBO.BindToLists.NewCartageJobTypes);
			dataObject.LocalTransportEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.JJ_DropMode, sourceBO.BindToLists.DropModes());
		}

		void PopulateGoodsAndTotals(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.GoodsDescription = sourceBO.JJ_GoodsDescription;

			dataObject.OuterPacks = sourceBO.JJ_OuterPacks;
			dataObject.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(sourceBO.JJ_F3_NKPackType, sourceBO.BindToLists.OuterPackTypes);

			dataObject.TotalVolume = sourceBO.JJ_Volume;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(sourceBO.JJ_VolumeUQ, sourceBO.BindToLists.VolumeUnits);

			dataObject.TotalWeight = sourceBO.JJ_Weight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.JJ_WeightUQ, sourceBO.BindToLists.WeightUnits);
		}

		void PopulateContainers(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetContainerCollection(() =>
				{
					var containers = new DataObjectList<Container>();
					containers.Content = CollectionContent.Complete;
					var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(sourceBO.Factory), writeManager);
					foreach (var containerSource in sourceBO.Containers)
					{
						var containerDataObject = writer.GetDataObject(containerSource);
						containers.Add(containerDataObject);

						var link = containers.Count;
						containerDataObject.Link = link;
						LinksDictionary.Add(containerSource.PK, link);
					}

					return containers.Count > 0 ? containers : null;
				});
		}

		void PopulatePackages(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() =>
			{
				var packages = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
				var writer = new BookedMovePackageDataObjectWriter(writeManager);
				foreach (var packageSource in sourceBO.LooseBookedMoves)
				{
					var packageDataObject = writer.GetDataObject(packageSource);
					packages.Add(packageDataObject);

					var link = packages.Count;
					packageDataObject.Link = link;
					LinksDictionary.Add(packageSource.PK, link);
				}

				return packages.Count > 0 ? packages : null;
			});
		}

		void PopulateReferences(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.Order = new Order();
			dataObject.Order.OrderNumber = sourceBO.JJ_OrderReferenceNumber;
			dataObject.QuoteNumber = sourceBO.JJ_QuoteNumber;
			dataObject.WayBillNumber = sourceBO.JJ_WaybillNumber;
			dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Waybill, new WayBillTypeList());

			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(sourceBO.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		void PopulateSchedule(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			if (sourceBO.SailingStandalone != null)
			{
				var writer = new ScheduleTransportLegDataObjectWriter(writeManager);
				var scheduleDataObject = writer.GetDataObject(sourceBO.SailingStandalone);

				dataObject.SetTransportLegCollection(() =>
				{
					var result = new DataObjectList<TransportLeg>() { Content = CollectionContent.Partial };
					result.Add(scheduleDataObject);
					return result;
				});
			}
		}

		void PopulateLegs(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			var localTransportLegs = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };
			var legWriter = new LocalTransportLegDataObjectWriter(writeManager);

			foreach (var move in sourceBO.BookedMovesCollection.OrderBy(m => m.EW_DisplayOrder))
			{
				foreach (var leg in move.CartageLegs.OrderBy(l => l.JU_DisplayOrder))
				{
					var legData = legWriter.GetDataObject(leg);
					localTransportLegs.Add(legData);

					var link = localTransportLegs.Count;
					legData.Link = link;
					LinksDictionary.Add(leg.PK, link);
				}
			}

			if (localTransportLegs.Count > 0)
			{
				if (dataObject.TransportLegCollection == null)
				{
					dataObject.SetTransportLegCollection(() => localTransportLegs);
				}
				else
				{
					dataObject.TransportLegCollection.AddRange(localTransportLegs);
				}
			}
		}

		/// <summary>
		/// Populates Instructions w/ their Packages w/ their Confirmations w/ Leg reference.
		/// Similar Instructions will be merged, but ensure Confirmations keep a reference their Legs.
		///	Shipment
		///		Containers
		///		PackageLines
		///		Legs
		///		Instructions
		///			ContainerLinks or PackageLinesLinks
		///				Confirmations
		///				LegLink
		///				LegSequence
		/// </summary>
		/// <param name="sourceBO"></param>
		/// <param name="dataObject"></param>
		void PopulateInstructions(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			var instructionChains = BuildInstructionChains(sourceBO);
			var entireInstructionChain = MergeInstructionChains(instructionChains);
			if (entireInstructionChain.Count > 0)
			{
				dataObject.SetInstructionCollection(() =>
				{
					var list = new DataObjectList<Instruction>();
					entireInstructionChain.ForEach(i => PopulateInstruction(i, list));
					return list;
				});
			}
		}

		void PopulateInstruction(InstructionBuilder instructionBuilder, DataObjectList<Instruction> instructions)
		{
			var instruction = new Instruction(writeManager.WriterStrategy);

			instruction.Type = instructionBuilder.InstructionType;
			instruction.Address = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(instructionBuilder.Address);
			instruction.DropMode = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.DropMode>(instructionBuilder.DropMode, BindToLists.GetCachedLists(instructionBuilder.Factory).DropModes());

			PopulatePackagesOrContainerLinks(instructionBuilder, instruction);

			instruction.Sequence = instructions.Count + 1;
			instructions.Add(instruction);
		}

		void PopulatePackagesOrContainerLinks(InstructionBuilder dataSource, Instruction instructionDataObject)
		{
			instructionDataObject.SetInstructionContainerLinkCollection(() =>
			{
				var containerLinks = new List<InstructionContainerLink>();
				foreach (var container in dataSource.Containers)
				{
					PopulateInstructionContainerLink(dataSource, containerLinks, container);
				}
				return containerLinks.Count > 0 ? containerLinks : null;
			});

			instructionDataObject.SetInstructionPackingLineLinkCollection(() =>
			{
				var packageLinks = new List<InstructionPackingLineLink>();
				foreach (var package in dataSource.Packages)
				{
					PopulateInstructionPackingLineLink(dataSource, packageLinks, package);
				}
				return packageLinks.Count > 0 ? packageLinks : null;
			});
		}

		void PopulateInstructionContainerLink(InstructionBuilder dataSource, List<InstructionContainerLink> containerLinks, CommonContainer container)
		{
			if (LinksDictionary.ContainsKey(container.PK))
			{
				var containerLinkDataObject = new InstructionContainerLink();
				containerLinkDataObject.ContainerLink = LinksDictionary[container.PK];
				containerLinkDataObject.Quantity = container.JC_ContainerCount;

				var confirmationCollection = new List<Confirmation>();
				PopulateConfirmations(dataSource, confirmationCollection, container);

				if (confirmationCollection.Count > 0)
				{
					containerLinkDataObject.ConfirmationCollection = confirmationCollection;
				}

				containerLinks.Add(containerLinkDataObject);
			}
		}

		void PopulateInstructionPackingLineLink(InstructionBuilder dataSource, List<InstructionPackingLineLink> packageLinks, CommonBookedCtgMove package)
		{
			if (LinksDictionary.ContainsKey(package.PK))
			{
				var packageLinkDataObject = new InstructionPackingLineLink();
				packageLinkDataObject.PackingLineLink = LinksDictionary[package.PK];
				packageLinkDataObject.Quantity = package.EW_BookedPackCount;

				var confirmationCollection = new List<Confirmation>();
				PopulateConfirmations(dataSource, confirmationCollection, package);

				if (confirmationCollection.Count > 0)
				{
					packageLinkDataObject.ConfirmationCollection = confirmationCollection;
				}

				packageLinks.Add(packageLinkDataObject);
			}
		}

		void PopulateConfirmations(InstructionBuilder dataSource, List<Confirmation> confirmations, BusinessObject package)
		{
			AddAndPopulateConfirmation(confirmations, dataSource.Confirmations.FirstOrDefault(c => c.IsDeliveringPackage(package)), package);
			AddAndPopulateConfirmation(confirmations, dataSource.Confirmations.FirstOrDefault(c => c.IsPickingUpPackage(package)), package);
		}

		void AddAndPopulateConfirmation(List<Confirmation> confirmations, ConfirmationBuilder confirmation, BusinessObject package)
		{
			if (confirmation != null)
			{
				var writer = new ConfirmationDataObjectWriter(writeManager, confirmation);
				var confirmationDataObject = writer.GetDataObject(package);
				confirmations.Add(confirmationDataObject);
			}
		}

		/// <summary>
		/// Create a list of Instructions per leg.
		/// </summary>
		/// <param name="sourceBO">
		/// Cartage w/ 2 Legs
		///		o	c1	A-B-C
		///		o	c2	A-D
		///	</param>
		/// <returns>
		///	2 Instruction Lists
		///		o	A - Pickup c1
		///			B - Wait Arrival c1, Wait Departure c1
		///			C - Delivery c1
		///		o	A - Pickup c2
		///			D - Delivery c2
		///</returns>
		List<List<InstructionBuilder>> BuildInstructionChains(CommonCartage sourceBO)
		{
			var instructionChains = new List<List<InstructionBuilder>>();

			foreach (var move in sourceBO.BookedMovesCollection.OrderBy(m => m.EW_DisplayOrder))
			{
				var instructionChain = new List<InstructionBuilder>();
				foreach (var leg in move.CartageLegs.OrderBy(l => l.JU_DisplayOrder))
				{
					var legBuilder = new LegBuilder(leg, LinksDictionary);
					AddToInstructionBuilderIfValid(instructionChain, legBuilder, AddressPoint.Pickup);
					AddToInstructionBuilderIfValid(instructionChain, legBuilder, AddressPoint.WaitPointArrival);
					AddToInstructionBuilderIfValid(instructionChain, legBuilder, AddressPoint.WaitPointDeparture);
					AddToInstructionBuilderIfValid(instructionChain, legBuilder, AddressPoint.Delivery);
				}

				if (instructionChain.Count > 0)
				{
					instructionChains.Add(instructionChain);
				}
			}

			return instructionChains;
		}

		/// <summary>
		/// Adds a new Instruction if different to the last, otherwise gets the last.
		/// Adds a confirmation (Pic, Dlv etc) to the instruction.
		/// </summary>
		/// <param name="instructions"></param>
		/// <param name="legBuilder"></param>
		/// <param name="addressPoint"></param>
		void AddToInstructionBuilderIfValid(List<InstructionBuilder> instructions, LegBuilder legBuilder, AddressPoint addressPoint)
		{
			var address = GetAddress(legBuilder.Leg, addressPoint);
			var instruction = CreateOrAddToInstructionBuilderIfValid(instructions, legBuilder.Leg, address);

			if (instruction != null)
			{
				var confirmation = new ConfirmationBuilder(addressPoint, legBuilder);
				instruction.Confirmations.Add(confirmation);
			}
		}

		/// <summary>
		/// Creates a new Instruction if different to the last, otherwise returns the last.
		/// </summary>
		/// <param name="instructions"></param>
		/// <param name="leg"></param>
		/// <param name="docAddress"></param>
		/// <returns></returns>
		InstructionBuilder CreateOrAddToInstructionBuilderIfValid(List<InstructionBuilder> instructions, CommonCartageLeg leg, JobDocAddress docAddress)
		{
			InstructionBuilder instruction = null;

			if (docAddress != null)
			{
				instruction = instructions.LastOrDefault();
				var dropMode = leg.BookedCtgMove.RequestedAddressType == docAddress.DocAddressType ? leg.BookedCtgMove.EW_DropMode : ZString.Empty;

				if (instruction == null || !instruction.IsMatch(docAddress, dropMode))
				{
					instruction = new InstructionBuilder(leg.Factory, docAddress, dropMode);
				}

				instructions.Add(instruction);
			}

			return instruction;
		}

		JobDocAddress GetAddress(CommonCartageLeg leg, AddressPoint addressPoint)
		{
			switch (addressPoint)
			{
				case AddressPoint.Pickup:
					return leg.PickupFromDocAddress;
				case AddressPoint.WaitPointArrival:
					return leg.WaitPointDocAddress;
				case AddressPoint.WaitPointDeparture:
					return leg.WaitPointDocAddress;
				case AddressPoint.Delivery:
					return leg.DeliverToDocAddress;
				default:
					return null;
			}
		}

		/// <summary>
		/// Takes a bunch of Instructions Lists and merges similar Instructions w/ their Packages w/ their Confirmations.
		/// </summary>
		/// <param name="instructionChains">
		/// 2 Instruction Lists of
		///		o	A - Pickup c1
		///			B - Wait Arrival c1, Wait Departure c1
		///			C - Delivery c1
		///		o	A - Pickup c2
		///			D - Delivery c2
		///	</param>
		/// <returns>
		///		o	A - Pickup c1 c2
		///			B - Wait Arrival c1, Wait Departure c2
		///			C - Delivery c1
		///			D - Delivery c2
		///</returns>
		List<InstructionBuilder> MergeInstructionChains(List<List<InstructionBuilder>> instructionChains)
		{
			var result = new List<InstructionBuilder>();
			foreach (var singleChain in instructionChains.OrderByDescending(c => c.Count))
			{
				int lastInstructionIndex = 0;
				foreach (var singleInstruction in singleChain)
				{
					lastInstructionIndex = result.FindIndex(lastInstructionIndex, i => i.IsMatch(singleInstruction.Address, singleInstruction.DropMode));
					if (lastInstructionIndex < 0)
					{
						result.Add(singleInstruction);
						lastInstructionIndex = result.Count - 1;
					}
					else
					{
						result[lastInstructionIndex].Confirmations.AddRange(singleInstruction.Confirmations);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Helper to wrap a leg and it's Confirmations.
		/// </summary>
		internal class LegBuilder
		{
			public LegBuilder(CommonCartageLeg leg, Dictionary<ZGuid, ZInt> linksDictionary)
			{
				this.leg = leg;
				this.linksDictionary = linksDictionary;
			}

			readonly CommonCartageLeg leg;
			readonly Dictionary<ZGuid, ZInt> linksDictionary;

			public CommonCartageLeg Leg
			{
				get { return leg; }
			}

			public ZInt LegLink
			{
				get { return linksDictionary[leg.PK]; }
			}

			public void AddConfirmation(ConfirmationBuilder confirmation)
			{
				Confirmations.Add(confirmation);
			}

			List<ConfirmationBuilder> Confirmations
			{
				get { return confirmations ?? (confirmations = new List<ConfirmationBuilder>()); }
			}

			List<ConfirmationBuilder> confirmations;
		}

		/// <summary>
		/// Wraps a portion of a leg to help with exporting Confirmations.
		/// </summary>
		internal class ConfirmationBuilder
		{
			public ConfirmationBuilder(AddressPoint addressPoint, LegBuilder legBuilder)
			{
				this.addressPoint = addressPoint;
				this.legBuilder = legBuilder;

				legBuilder.AddConfirmation(this);
			}

			readonly AddressPoint addressPoint;
			readonly LegBuilder legBuilder;

			public ZInt LegLink
			{
				get { return legBuilder.LegLink; }
			}

			public BusinessObject Package
			{
				get { return Leg.IsContainerised ? Leg.Container : Leg.BookedCtgMove; }
			}

			public ZBool IsPickingUpPackage(BusinessObject package)
			{
				return IsPickingUp && IsConfirmationForPackage(package);
			}

			public ZBool IsDeliveringPackage(BusinessObject package)
			{
				return IsDelivering && IsConfirmationForPackage(package);
			}

			public ZBool IsPickingUp
			{
				get { return addressPoint == AddressPoint.Pickup || addressPoint == AddressPoint.WaitPointDeparture; }
			}

			public ZBool IsDelivering
			{
				get { return addressPoint == AddressPoint.WaitPointArrival || addressPoint == AddressPoint.Delivery; }
			}

			public ZBool IsEmptyContainer
			{
				get { return Leg.JU_IsEmptyContainer; }
			}

			public ZDateTime EstimatedIn
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.JU_PlannedPickupTime;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
						case AddressPoint.Delivery:
							return Leg.JU_EstimatedDeliveryTime;
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime EstimatedOut
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.JU_PlannedPickupTimeEnd;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
						case AddressPoint.Delivery:
							return Leg.JU_EstimatedDeliveryTimeEnd;
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime ActualIn
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.JU_PickupTimeIn;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
							return Leg.JU_WaitPointTimeIn;
						case AddressPoint.Delivery:
							return Leg.JU_DeliverTimeIn;
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime ActualOut
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.JU_PickupTimeOut;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
							return Leg.JU_WaitPointTimeOut;
						case AddressPoint.Delivery:
							return Leg.JU_DeliverTimeOut;
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime RequiredFrom
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedPickupTimeStart);
						case AddressPoint.WaitPointArrival:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedDeliveryTimeStart);
						case AddressPoint.WaitPointDeparture:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedPickupTimeStart);
						case AddressPoint.Delivery:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedDeliveryTimeStart);
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime RequiredTo
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedPickupTimeEnd);
						case AddressPoint.WaitPointArrival:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedDeliveryTimeEnd);
						case AddressPoint.WaitPointDeparture:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedPickupTimeEnd);
						case AddressPoint.Delivery:
							return GetRequiredIfApplicable(Leg.BookedCtgMove.EW_RequestedDeliveryTimeEnd);
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZDateTime Demurrage
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.JU_CartagePickupDemurrage;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
							return Leg.JU_CartageWaitPointDemurrage;
						case AddressPoint.Delivery:
							return Leg.JU_CartageDeliveryDemurrage;
						default:
							return ZDateTime.Empty;
					}
				}
			}

			public ZString ConfirmationType
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
						case AddressPoint.WaitPointDeparture:
							return ConfirmationTypes.Codes.PickUp;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.Delivery:
							return ConfirmationTypes.Codes.Delivery;
						default:
							return ZString.Empty;
					}
				}
			}

			public ZString ReceivedBy
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.WaitPointArrival:
						case AddressPoint.Delivery:
							return Leg.JU_DeliverySignedFor;
						default:
							return ZString.Empty;
					}
				}
			}

			public ZString ServiceInstruction
			{
				get { return Leg.JU_LegNotes; }
			}

			public ZDecimal Distance
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.WaitPointArrival:
						case AddressPoint.Delivery:
							return Leg.JU_Distance;
						default:
							return ZDecimal.Zero;
					}
				}
			}

			public UnitOfLength DistanceUnit
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.WaitPointArrival:
						case AddressPoint.Delivery:
							return ListHelper.GetWithDescription<UnitOfLength>(Leg.JU_DistanceUnit, Leg.BindToLists.DimensionUnits);
						default:
							return null;
					}
				}
			}

			ZBool IsConfirmationForPackage(BusinessObject package)
			{
				return Package.PK == package.PK;
			}

			JobDocAddress Address
			{
				get
				{
					switch (addressPoint)
					{
						case AddressPoint.Pickup:
							return Leg.PickupFromDocAddress;
						case AddressPoint.WaitPointArrival:
						case AddressPoint.WaitPointDeparture:
							return Leg.WaitPointDocAddress;
						case AddressPoint.Delivery:
							return Leg.DeliverToDocAddress;
						default:
							return null;
					}
				}
			}

			ZDateTime GetRequiredIfApplicable(ZDateTime unfilteredRequiredTime)
			{
				return Leg.IsRequestedAddress(Address) ? unfilteredRequiredTime : ZDateTime.Empty;
			}

			CommonCartageLeg Leg
			{
				get { return legBuilder.Leg; }
			}
		}

		/// <summary>
		/// Wraps an Addres and Drop Mode, to help export a list of Instructions /w Packages /w Confirmations /w Leg Reference.
		/// </summary>
		class InstructionBuilder
		{
			public InstructionBuilder(BusinessObjectFactory factory, JobDocAddress docAddress, ZString dropMode)
			{
				this.address = docAddress;
				this.dropMode = dropMode;
				this.factory = factory;
			}

			public List<ConfirmationBuilder> Confirmations
			{
				get { return confirmations ?? (confirmations = new List<ConfirmationBuilder>()); }
			}

			List<ConfirmationBuilder> confirmations;

			public ZBool IsPickingUp
			{
				get { return Confirmations.Any(c => c.IsPickingUp); }
			}

			public ZBool IsDelivering
			{
				get { return Confirmations.Any(c => c.IsDelivering); }
			}

			public ZBool IsMulti
			{
				get { return IsDelivering && IsPickingUp; }
			}

			public CodeDescriptionPair InstructionType
			{
				get
				{
					var instructionType = ZString.Empty;

					if (IsMulti)
					{
						instructionType = InstructionTypes.Codes.Multi;
					}
					else if (IsPickingUp)
					{
						instructionType = InstructionTypes.Codes.PickUp;
					}
					else if (IsDelivering)
					{
						instructionType = InstructionTypes.Codes.Delivery;
					}

					return ListHelper.GetWithDescription<CodeDescriptionPair>(instructionType, new InstructionTypes().List);
				}
			}

			public JobDocAddress Address
			{
				get { return address; }
			}

			readonly JobDocAddress address;

			public ZString DropMode
			{
				get { return dropMode; }
			}

			readonly ZString dropMode;

			public CommonContainer[] Containers
			{
				get { return Confirmations.Where(c => c.Package != null && c.Package is CommonContainer).Select(c => (CommonContainer)c.Package).Distinct().ToArray(); }
			}

			public CommonBookedCtgMove[] Packages
			{
				get { return Confirmations.Where(c => c.Package != null && c.Package is CommonBookedCtgMove).Select(c => (CommonBookedCtgMove)c.Package).Distinct().ToArray(); }
			}

			public ZBool IsMatch(JobDocAddress docAddress, ZString mode)
			{
				return IsAddressMatch(docAddress) && IsDropModeMatch(mode);
			}

			ZBool IsAddressMatch(JobDocAddress docAddress)
			{
				return Address == docAddress;
			}

			ZBool IsDropModeMatch(ZString mode)
			{
				return DropMode == mode;
			}

			public BusinessObjectFactory Factory
			{
				get { return factory; }
			}

			readonly BusinessObjectFactory factory;
		}

		void PopulateNotes(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			var notes = sourceBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			dataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateDates(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() =>
			{
				var dateCollection = new List<Date>();
				dateCollection.Add(Date.New(DateType.LocalTransportPickup, true, sourceBO.JJ_EstimatedPickup));
				dateCollection.Add(Date.New(DateType.LocalTransportDelivery, true, sourceBO.JJ_EstimatedDelivery));
				dateCollection.Add(Date.New(DateType.LocalTransportCompleted, false, sourceBO.JJ_A_JCL));
				return dateCollection;
			});
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(CommonCartage cartageBO)
		{
			return cartageBO.GetUserDefinedValues();
		}

		public Dictionary<ZGuid, ZInt> LinksDictionary
		{
			get { return linksDictionary ?? (linksDictionary = new Dictionary<ZGuid, ZInt>()); }
		}

		Dictionary<ZGuid, ZInt> linksDictionary;
	}
}
