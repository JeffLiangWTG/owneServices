using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	using Enterprise.ZArchitecture.Core;

	public class RefContainerLookups : AutoRefContainerLookups
	{
		public RefContainerLookups(AutoRefContainer parent) : base(parent)
		{
		}

		#region

		internal RefContainer ParentContainer => Parent as RefContainer;

		#endregion

		#region Storage Classes

		public CodeDescriptionPairList StorageClassList
		{
			get
			{
				if (ParentContainer.RC_ShippingMode == ShippingModes.Air)
				{
					return AirStorageClassesList;
				}

				return new CodeDescriptionPairList(OLookUpEditType.ContainerStorageClass);
			}
		}

		#region AirStorageClasses

		public CodeDescriptionPairList AirStorageClassesList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(AirStorageClasses.MainDeck, ResString.GetMultilingualString("20f9a188-66a4-40de-85a1-80f09b9f0f42", "Main Deck"));
				list.AddPair(AirStorageClasses.LowerDeck, ResString.GetMultilingualString("4ec63a5c-eba3-445d-a9be-fad4d4a036fa", "Lower Deck"));

				return list;
			}
		}

		public static class AirStorageClasses
		{
			public const string MainDeck = "MD";
			public const string LowerDeck = "LD";
		}

		#endregion

		#endregion

		#region Container Types

		public CodeDescriptionPairList ContainerTypes
		{
			get
			{
				if (ParentContainer != null && ParentContainer.IsAirContainer)
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Constants.ContainerTypes.Refrigerated, Constants.ContainerTypeDescriptions.Refrigerated);
					list.AddPair(Constants.ContainerTypes.DryStorage, Constants.ContainerTypeDescriptions.DryStorage);
					list.AddPair(Constants.ContainerTypes.Other, Constants.ContainerTypeDescriptions.Other);
					list.AddPair(Constants.ContainerTypes.AircraftPallet, Constants.ContainerTypeDescriptions.AircraftPallet);
					list.AddPair(Constants.ContainerTypes.AircraftContainer, Constants.ContainerTypeDescriptions.AircraftContainer);
					list.AddPair(Constants.ContainerTypes.AutomobileTransportEquipment, Constants.ContainerTypeDescriptions.AutomobileTransportEquipment);
					list.AddPair(Constants.ContainerTypes.AircraftEngineTransportEquipment, Constants.ContainerTypeDescriptions.AircraftEngineTransportEquipment);
					list.AddPair(Constants.ContainerTypes.FireResistantContainer, Constants.ContainerTypeDescriptions.FireResistantContainer);
					list.AddPair(Constants.ContainerTypes.CattleStalls, Constants.ContainerTypeDescriptions.CattleStalls);
					list.AddPair(Constants.ContainerTypes.HorseStalls, Constants.ContainerTypeDescriptions.HorseStalls);
					list.AddPair(Constants.ContainerTypes.ThermalAircraftContainer, Constants.ContainerTypeDescriptions.ThermalAircraftContainer);
					return list;
				}

				return new CodeDescriptionPairList(OLookUpEditType.ContainerType);
			}
		}

		#endregion

		#region Freight Classes

		public ContainerFreightRateClassList FreightRateClassList
		{
			get
			{
				return new ContainerFreightRateClassList();
			}
		}

		#endregion

		public RefContainerISOTypesCollection ISOTypes
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.MasterFiles.Business.RefContainer.ISOTypes", () =>
					{
						var fISOTypes = new RefContainerISOTypesCollection(Factory);
						foreach (var length in ContainerISOType.ValidISOLengthValues)
						{
							foreach (var width in ContainerISOType.ValidISOHeightAndWidthValues)
							{
								foreach (var group in ContainerISOType.ValidISOGroupValues)
								{
									fISOTypes.Add(new ContainerISOType
									{
										ISOCode = length + width + group
									});
								}
							}
						}
						return fISOTypes;
					});
			}
		}

		#region Handling Classes

		public ContainerHandlingRateClassList HandlingRateClassList
		{
			get
			{
				return new ContainerHandlingRateClassList();
			}
		}

		#endregion

		#region Shipping Modes

		public CodeDescriptionPairList ShippingModesList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(ShippingModes.Sea, Res.GetString("MasterFiles|ShippingModesList|Sea", "Sea FCL Container"));
				result.AddPair(ShippingModes.Air, Res.GetString("MasterFiles|ShippingModesList|Air", "Air ULD Container"));
				result.AddPair(ShippingModes.Road, Res.GetString("MasterFiles|ShippingModesList|Road", "Road/Truck Container"));
				return result;
			}
		}

		public static class ShippingModes
		{
			public const string Sea = "SEA";
			public const string Air = "AIR";
			public const string Road = "ROA";
		}

		#endregion

		#region IATA Rate Classes

		public CodeDescriptionPairList IATARateClasses
		{
			get
			{
				return fIATARateClasses ?? (fIATARateClasses = new ContainerIATARateClassList());
			}
		}
		CodeDescriptionPairList fIATARateClasses;

		#endregion

		#region Equipment Size Type

		public CodeDescriptionPairList EquipmentSizeTypeList
		{
			get
			{
				return Factory.GetCachedValue(
					"Enterprise.MasterFiles.Business.RefContainer.EquipmentSizeTypeList",
					() =>
					{
						var result = new UntranslatableCodeDescriptionPairList((NoResString)"UN/Edifact specific descriptions");
						result.AddRangeOverwriteIfExists(new ContainerSizeTypeList());
						return result;
					});
			}
		}

		#endregion
	}
}
