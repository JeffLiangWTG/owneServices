using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	partial class ContainerMovementTypes
	{
		public static string GetDetentionCalculation(string movementType)
		{
			switch (movementType)
			{
				case Codes.ReShipRequested:
				case Codes.YardGateIn:
				case Codes.ReturnToWharf:
					return DetentionInvoiceType.Codes.Import;

				case Codes.WharfGateIn:
				case Codes.ReturnedUnshipped:
					return DetentionInvoiceType.Codes.Export;

				default:
					return null;
			}
		}

		public static string[] GetMovementCodesForDetention(string detentionType)
		{
			switch (detentionType)
			{
				case DetentionInvoiceType.Codes.Export:
					return new string[]
					{
						Codes.WharfGateIn,
						Codes.ReturnedUnshipped,
					};

				case DetentionInvoiceType.Codes.Import:
					return new string[]
					{
						Codes.YardGateIn,
						Codes.ReShipRequested,
						Codes.ReturnToWharf,
					};

				default:
					return System.Array.Empty<string>();
			}
		}

		public static string[] GetMovementsCodesThatStartExportDetention()
		{
			return new string[]
			{
				Codes.YardGateOut,
				Codes.ReShipRequested,
			};
		}

		public static string[] MovementsCodesThatRestrictDetentionDaysDefaulting
		{
			get
			{
				return new[]
				{
					Codes.Discharge,
					Codes.Load,
					Codes.ReturnedUnshipped,
					Codes.ReturnToWharf,
					Codes.WharfGateIn,
					Codes.WharfGateOut,
					Codes.YardGateIn
				};
			}
		}

		public static string[] FCLReturnToWharfMovements
		{
			get
			{
				return new string[]
				{
					Codes.WharfGateIn,
					Codes.ReturnToWharf,
				};
			}
		}

		public static string[] EmptyReturnMovements
		{
			get
			{
				return new string[]
				{
					Codes.YardGateIn,
					Codes.ReturnToWharf,
				};
			}
		}

		public static bool RequiresClientAndPrincipal(string movementType)
		{
			switch (movementType)
			{
				case Codes.ReShipRequested:
				case Codes.ReturnedUnshipped:
				case Codes.YardGateOut:
				case Codes.YardGateIn:
				case Codes.WharfGateOut:
				case Codes.WharfGateIn:
				case Codes.ReturnToWharf:
					return true;

				default:
					return false;
			}
		}

		public static string GetDirection(string movementType)
		{
			switch (movementType)
			{
				case Codes.Discharge:
				case Codes.WharfGateOut:
				case Codes.YardGateIn:
				case Codes.ReturnToWharf:
					return MovementDirection.Codes.Arrival;

				case Codes.Load:
				case Codes.ReShipRequested:
				case Codes.WharfGateIn:
				case Codes.YardGateOut:
					return MovementDirection.Codes.Departure;

				default:
					return null;
			}
		}

		public static string GetToLocationCategory(string movementType)
		{
			switch (movementType)
			{
				case Codes.DepotGateIn:
					return ContainerLocationCategoryList.Codes.AtDepot;
				case Codes.DepotGateOut:
					return ContainerLocationCategoryList.Codes.OutOfGate;
				case Codes.Discharge:
					return ContainerLocationCategoryList.Codes.AtWharf;
				case Codes.Load:
					return ContainerLocationCategoryList.Codes.OnboardVessel;
				case Codes.OffHire:
					return ContainerLocationCategoryList.Codes.OutOfScope;
				case Codes.OnHire:
					return string.Empty;
				case Codes.RePositionIntoYard:
					return ContainerLocationCategoryList.Codes.AtYard;
				case Codes.RePositionOutOfYard:
					return ContainerLocationCategoryList.Codes.OutOfGate;
				case Codes.ReShipRequested:
					return ContainerLocationCategoryList.Codes.OutOfGate;
				case Codes.ReturnedUnshipped:
					return ContainerLocationCategoryList.Codes.AtYard;
				case Codes.ReturnToWharf:
					return ContainerLocationCategoryList.Codes.AtWharf;
				case Codes.WharfGateIn:
					return ContainerLocationCategoryList.Codes.AtWharf;
				case Codes.WharfGateOut:
					return ContainerLocationCategoryList.Codes.OutOfGate;
				case Codes.YardGateIn:
					return ContainerLocationCategoryList.Codes.AtYard;
				case Codes.YardGateOut:
					return ContainerLocationCategoryList.Codes.OutOfGate;
				default:
					return string.Empty;
			}
		}
		public static string[] GetMovementCodesLeadingToLocation(string locationCategory)
		{
			switch (locationCategory)
			{
				case ContainerLocationCategoryList.Codes.AtDepot:
					return new string[]
					{
						Codes.DepotGateIn,
					};

				case ContainerLocationCategoryList.Codes.AtWharf:
					return new string[]
					{
						Codes.WharfGateIn,
						Codes.Discharge,
						Codes.ReturnToWharf,
					};

				case ContainerLocationCategoryList.Codes.AtYard:
					return new string[]
					{
						Codes.YardGateIn,
						Codes.ReturnedUnshipped,
						Codes.RePositionIntoYard,
					};

				case ContainerLocationCategoryList.Codes.OnboardVessel:
					return new string[]
					{
						Codes.Load,
					};

				case ContainerLocationCategoryList.Codes.OutOfGate:
					return new string[]
					{
						Codes.DepotGateOut,
						Codes.RePositionOutOfYard,
						Codes.ReShipRequested,
						Codes.WharfGateOut,
						Codes.YardGateOut,
					};

				case ContainerLocationCategoryList.Codes.OutOfScope:
					return new string[]
					{
						Codes.OffHire,
					};

				default:
					return System.Array.Empty<string>();
			}
		}
		public static AddressType GetDefaultAddressType(string movementType)
		{
			switch (movementType)
			{
				case Codes.DepotGateIn:
				case Codes.RePositionIntoYard:
				case Codes.ReturnedUnshipped:
				case Codes.WharfGateIn:
				case Codes.YardGateIn:
				case Codes.Load:
				case Codes.ReturnToWharf:
					return AddressType.DLV;

				case Codes.DepotGateOut:
				case Codes.RePositionOutOfYard:
				case Codes.WharfGateOut:
				case Codes.YardGateOut:
				case Codes.Discharge:
					return AddressType.PIC;

				default:
					return AddressType.NoDefault;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module Filter Key")]
		public static OrganisationsFindBoxCollection GetDepotLookupCollection(BusinessObjectFactory factory, string movementType)
		{
			const string organizationTypes = "Organisation Types";
			const string services = "Property9";
			const string consignee = "Property2";
			const string forwarder = "Property5";

			const string secondaryType = "Secondary Type";
			const string property = "Property";
			const string servicesCTO = "Services - CTO";
			const string servicesDepot = "Services - Depot";
			const string servicesContainerYard = "Services - Container Yard";

			var result = new CTODepotContainerYardConsigneeForwarderCollection(factory);

			switch (movementType)
			{
				case Codes.Load:
				case Codes.Discharge:
				case Codes.WharfGateIn:
				case Codes.WharfGateOut:
				case Codes.ReturnToWharf:
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, services, ZBool.True));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(secondaryType, property, new ZString(servicesCTO)));
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsMiscFreightServices, ZBool.True);
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsSeaCTO, ZBool.True);
					break;

				case Codes.YardGateIn:
				case Codes.YardGateOut:
				case Codes.ReturnedUnshipped:
				case Codes.RePositionIntoYard:
				case Codes.RePositionOutOfYard:
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, services, ZBool.True));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(secondaryType, property, new ZString(servicesContainerYard)));
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsMiscFreightServices, ZBool.True);
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsContainerYard, ZBool.True);
					break;

				case Codes.DepotGateIn:
				case Codes.DepotGateOut:
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, services, ZBool.True));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(secondaryType, property, new ZString(servicesDepot)));
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsMiscFreightServices, ZBool.True);
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsPackDepot, ZBool.True);
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsUnpackDepot, ZBool.True);
					break;
				case Codes.ReShipRequested:
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, consignee, ZBool.True));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, forwarder, ZBool.True));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, "AndJoinCondition", ZBool.False));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(organizationTypes, "OrJoinCondition", ZBool.True));
					result.DefaultsForNewChild.Add(OrgHeaderSchema.Constants.OH_IsConsignee, ZBool.True);
					break;
			}

			return result;
		}
	}
}


