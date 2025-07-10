using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionLookups
	{
		#region New

		public static CommissionLookups New(BusinessObjectFactory factory)
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden(factory) : new CommissionLookups(factory);
		}

		protected delegate CommissionLookups NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Constructor

		protected CommissionLookups(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		#endregion

		#region Products

		public virtual ReadOnlyCodeDescriptionPairList GetProducts()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes());
			result.SortByDescription();

			return result;
		}

		public virtual ReadOnlyCodeDescriptionPairList AllProducts
		{
			get { return GetProducts(); }
		}

		#endregion

		#region Services

		public virtual ReadOnlyCodeDescriptionPairList GetServices(ZString product)
		{
			return new CodeDescriptionPairList();
		}

		public virtual ReadOnlyCodeDescriptionPairList AllServices
		{
			get { return GetServices(ZString.Empty); }
		}

		#endregion

		#region SubModules

		public virtual ReadOnlyCodeDescriptionPairList GetSubModules(ZString product, ZString service)
		{
			return new CodeDescriptionPairList();
		}

		public virtual ReadOnlyCodeDescriptionPairList AllSubModules
		{
			get { return GetSubModules(ZString.Empty, ZString.Empty); }
		}

		#endregion

		#region Modes

		public ReadOnlyCodeDescriptionPairList GetModes(ZString product)
		{
			CodeDescriptionPairList result = null;
			switch (product)
			{
				case JobInvoicingConsumerTypes.ShipmentCode:
					result = new CodeDescriptionPairList(OLookUpEditType.TransportType);
					result.AddPair(Core.Constants.TransportModes.All, Core.Constants.TransportModeDescriptions.All);
					result.RemoveCode(Constants.TransportModes.SeaAir);
					result.RemoveCode(Constants.TransportModes.AirSea);
					result.RemoveCode("MMD");
					break;
				case JobInvoicingConsumerTypes.BrokerageCode:
					result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.FreightShipmentDirection.Description.All);
					result.AddPair(Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.FreightShipmentDirection.Description.Export);
					result.AddPair(Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.FreightShipmentDirection.Description.Import);
					break;
				case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
				case JobInvoicingConsumerTypes.AgencyBookingCode:
					result = new CodeDescriptionPairList();
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					result.AddPair(OrgCommissionAgreementItem.AllItemCode);
					break;
				default:
					result = new CodeDescriptionPairList();
					result.AddPair(OrgCommissionAgreementItem.AllItemCode);
					break;
			}

			return result;
		}

		#endregion

		#region ShouldShowServicesAndSubModules

		public static bool ShouldShowServicesAndSubModules
		{
			get { return CommissionLookups.New(null).GetShouldShowServicesAndSubModules(); }
		}

		public virtual bool GetShouldShowServicesAndSubModules()
		{
			return false;
		}

		#endregion

		#region Commission Basis

		public ReadOnlyCodeDescriptionPairList CommissionBasisType
		{
			get { return new CommissionBasisType(); }
		}

		#endregion

		#region Commission Streams

		public ICodeDescriptionPairList CommissionStreams_All
		{
			get { return OrganisationRegistry.Instance.CommissionAgreementStreams.Value; }
		}

		public ICodeDescriptionPairList CommissionStreams_Active
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("CommissionLookups.CommissionStreams_Active", () => OrganisationRegistry.Instance.CommissionAgreementStreams.Value.GetActiveCodeDescriptionPairList()); }
		}

		#endregion

		#region Commission Types

		public ReadOnlyCodeDescriptionPairList CommissionTypes
		{
			get { return new CommissionTypes(); }
		}

		#endregion

		#region Commission Periods

		public ReadOnlyCodeDescriptionPairList CommissionPeriods
		{
			get { return OrganisationsDataRegistry.Instance.CommissionPeriodList.Value.GetCodeDescriptionPairList(); }
		}

		public ReadOnlyCodeDescriptionPairList EnabledCommissionPeriods
		{
			get { return OrganisationsDataRegistry.Instance.CommissionPeriodList.Value.GetEnabledCodeDescriptionPairList(); }
		}

		#endregion

		#region Commission Trigger Types

		public ReadOnlyCodeDescriptionPairList TriggerTypes
		{
			get { return GetNewTriggerTypes(); }
		}

		protected virtual CodeDescriptionPairList GetNewTriggerTypes()
		{
			return new CommissionTriggerTypes();
		}

		#endregion
	}
}
