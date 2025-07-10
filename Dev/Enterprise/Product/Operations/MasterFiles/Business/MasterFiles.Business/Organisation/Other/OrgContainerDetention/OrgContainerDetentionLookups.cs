using System.Linq;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContainerDetentionLookups : AutoOrgContainerDetentionLookups
	{
		public OrgContainerDetentionLookups(AutoOrgContainerDetention parent)
			: base(parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public new AutoOrgContainerDetention Parent { get; }

		public override OrgHeaderCollection Carriers
		{
			get { return new ShippingProviderCollection(Factory, true); }
		}

		public StorageCTOCollection StorageCTOs
		{
			get { return new StorageCTOCollection(Factory); }
		}

		public override OrgHeaderCollection Clients
		{
			get
			{
				var direction = Parent.PD_Direction;
				switch (direction)
				{
					case Constants.ContainerDetentionDirection.Import:
						return new ConsigneeCollection(Factory);

					case Constants.ContainerDetentionDirection.Export:
						return new ConsignorCollection(Factory);

					default:
						return new OrgHeaderCollection(Factory);
				}
			}
		}

		public CodeDescriptionPairList FreeDayTypes
		{
			get
			{
				switch (Parent.PD_Direction)
				{
					case Constants.ContainerDetentionDirection.Import:
						return FirstFreeDayTypes;

					default:
						return LastFreeDayTypes;
				}
			}
		}

		public CodeDescriptionPairList FirstFreeDayTypes
		{
			get
			{
				var penaltyType = Parent.PD_PenaltyType;

				switch (penaltyType)
				{
					case Constants.ContainerDetentionPenaltyType.MDD:
					case Constants.ContainerDetentionPenaltyType.DET:
						return Factory.GetCachedValue("ContainerDetentionFreeDayTypesForImport", () =>
							GetValidCodeDescriptionPairList(new[]
							{
								Constants.ContainerDetentionFreeDayType.CTOAvailable,
								Constants.ContainerDetentionFreeDayType.FCLUnload,
								Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload,
								Constants.ContainerDetentionFreeDayType.VesselArrival,
								Constants.ContainerDetentionFreeDayType.CTOGateOut
							}));
					default:
						return Factory.GetCachedValue("ContainerStorageFreeDayTypesForImport", () =>
							GetValidCodeDescriptionPairList(new[]
							{
								Constants.ContainerDetentionFreeDayType.CTOAvailable,
								Constants.ContainerDetentionFreeDayType.FCLUnload,
								Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload,
								Constants.ContainerDetentionFreeDayType.VesselArrival
							}));
				}
			}
		}

		public CodeDescriptionPairList LastFreeDayTypes
		{
			get
			{
				var penaltyType = Parent.PD_PenaltyType;

				switch (penaltyType)
				{
					case Constants.ContainerDetentionPenaltyType.MDD:
					case Constants.ContainerDetentionPenaltyType.STO:
						return Factory.GetCachedValue("ContainerStorageFreeDayTypesForExport", () =>
							GetValidCodeDescriptionPairList(new[]
							{
								Constants.ContainerDetentionFreeDayType.FCLLoad,
								Constants.ContainerDetentionFreeDayType.DayBeforeFCLLoad,
								Constants.ContainerDetentionFreeDayType.VesselDeparture,
								Constants.ContainerDetentionFreeDayType.DayBeforeVesselDeparture
							}));
					default:
						return Factory.GetCachedValue("ContainerDetentionFreeDayTypesForExport", () =>
							GetValidCodeDescriptionPairList(new[]
							{
								Constants.ContainerDetentionFreeDayType.WharfGateIn,
								Constants.ContainerDetentionFreeDayType.FCLLoad,
								Constants.ContainerDetentionFreeDayType.DayBeforeFCLLoad,
								Constants.ContainerDetentionFreeDayType.VesselDeparture,
								Constants.ContainerDetentionFreeDayType.DayBeforeVesselDeparture
							}));
				}
			}
		}

		CodeDescriptionPairList GetValidCodeDescriptionPairList(string[] validCodes)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.ContainerDetentionFreeDayTypes);
			foreach (var code in result.GetAllCodes())
			{
				if (!validCodes.Contains(code))
				{
					result.RemoveCode(code);
				}
			}
			return result;
		}

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		public CodeDescriptionPairList StorageClasses
		{
			get { return Factory.GetCachedValue("ContainerStorageClass", () => new CodeDescriptionPairList(OLookUpEditType.ContainerStorageClass)); }
		}

		public CodeDescriptionPairList CreditorTypes
		{
			get
			{
				return Factory.GetCachedValue("CTOStorageCreditorType", () =>
				{
					var creditorTypes = new CodeDescriptionPairList();
					creditorTypes.AddPair(Core.Constants.ContainerPenaltyCreditorType.Codes.CTO, Core.Constants.ContainerPenaltyCreditorType.Descriptions.CTO);
					creditorTypes.AddPair(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, Core.Constants.ContainerPenaltyCreditorType.Descriptions.Carrier);
					return creditorTypes;
				});
			}
		}

		public CodeDescriptionPairList Directions
		{
			get
			{
				return Factory.GetCachedValue("ContainerPenaltyDirection", () =>
				{
					var directions = new CodeDescriptionPairList();
					directions.AddPair(Core.Constants.ContainerDetentionDirection.Import, Core.Constants.ContainerDetentionDirectionDescription.Import);
					directions.AddPair(Core.Constants.ContainerDetentionDirection.Export, Core.Constants.ContainerDetentionDirectionDescription.Export);
					return directions;
				});
			}
		}

		public CodeDescriptionPairList PenaltyTypes
		{
			get
			{
				return Factory.GetCachedValue($"ContainerPenaltyType", () =>
				{
					var penaltyTypes = new CodeDescriptionPairList();
					penaltyTypes.AddPair(Core.Constants.ContainerDetentionPenaltyType.DET, Core.Constants.ContainerPenaltyPenaltyType.Descriptions.Detention);
					penaltyTypes.AddPair(Core.Constants.ContainerDetentionPenaltyType.STO, Core.Constants.ContainerPenaltyPenaltyType.Descriptions.Storage);
					penaltyTypes.AddPair(Core.Constants.ContainerDetentionPenaltyType.MDD, Core.Constants.ContainerPenaltyPenaltyType.Descriptions.MergedDemurrageAndDetention);

					return penaltyTypes;
				});
			}
		}
	}
}
