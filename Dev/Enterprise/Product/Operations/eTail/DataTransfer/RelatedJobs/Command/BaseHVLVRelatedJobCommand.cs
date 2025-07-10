using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class BaseHVLVRelatedJobCommand
	{
		public static bool IsNeeded(Type commandType, ForwardingShipment shipment)
		{
			return IsNeededByAttribute<ApplicableLoginCountryAttribute>(commandType, (attribute) =>
			{
				var result = false;
				result = attribute.CountryCodes.Contains<string>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (!result)
				{
					if (!string.IsNullOrEmpty(attribute.AllowLoginToDifferentCountryRegistry)
					&& HVLVDataRegistry.Instance.FindByName(attribute.AllowLoginToDifferentCountryRegistry) is BooleanRegistryItem registry)
					{
						result = registry.Value;
					}
				}

				return result;
			})
			&& IsNeededByAttribute<ShipmentDestinationCountryAttribute>(commandType, (attribute) =>
			{
				var countryCode = shipment.Destination?.Country?.Code;
				return countryCode.HasValue && CountryCodes.GetCustomsCountryOfJurisdictionOrEU(countryCode) == attribute.CountryCode;
			})
			&& IsNeededByAttribute<ShipmentTransportModeAttribute>(commandType, (attribute) => attribute.TransportModes.Contains<string>(shipment.TransportMode))
			&& IsNeededByAttribute<ShipmentDirectionAttribute>(commandType, (attribute) => attribute.Directions.Contains(shipment.JobDirection))
			&& IsNeededByAttribute<RequiredRegistryItemAttribute>(commandType, (attribute) =>
			{
				if (!string.IsNullOrEmpty(attribute.RegistryItemSetName) && !string.IsNullOrEmpty(attribute.RequiredRegistryItemName))
				{
					var locator = new RegistryItemSetLocator();
					var registryItemSet = locator.GetRegistryItemSet(attribute.RegistryItemSetName) as RegistryItemSet;
					var registryItem = registryItemSet?.FindByName(attribute.RequiredRegistryItemName) as BooleanRegistryItem;
					return registryItem?.Value ?? false;
				}
				return false;
			})
			&& IsNeededByAttribute<RequiredFeatureControlCodeAttribute>(commandType, (attribute) =>
			{
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var currentCompany = GlbCompany.CurrentCompany.GC_Code;
				var isEU = ObjectFactory
					.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>()
					.IsMemberOfEU(CountryCodes.GetCustomsCountryOfJurisdiction(currentCountry));
				var isGB = currentCountry == CountryCodes.UnitedKingdom;

				return (isEU || isGB) && ObjectFactory.Get<Enterprise.Integration.Customs.EUH7.IH7FeatureControlProvider>().IsAuthorized(currentCountry, currentCompany);
			});
		}

		static bool IsNeededByAttribute<TAttribute>(Type commandType, Func<TAttribute, bool> predicate) where TAttribute : Attribute
		{
			var attribute = (TAttribute)Attribute.GetCustomAttribute(commandType, typeof(TAttribute));
			return attribute == null || predicate(attribute);
		}

		public BaseHVLVRelatedJobCommand(ForwardingShipment shipment)
		{
			Shipment = shipment;
		}

		public bool CheckDataIsReadyForConvertToCustomsJob(out string errorMessage)
		{
			return CheckHasAnyActiveConsignment(Header.ConsignmentsForBinding, out errorMessage)
				&& CheckShipmentHasAtLeastOneConsol(Shipment, out errorMessage)
				&& (!ShouldValidateItemContainer || CheckItemContainer(Header.Shipment.HVLVItems.Cast<HVLVItem>(), out errorMessage));
		}

		bool CheckHasAnyActiveConsignment(IEnumerable<HVLVConsignment> consignments, out string errorMessage)
		{
			var result = true;
			errorMessage = null;
			if (!consignments.Any(x => x.HVC_IsActive))
			{
				result = false;
				errorMessage = Res.GetString("22c00904-ade8-4dd8-8ca0-22d20f7c8254", "Please make sure there's at least one active consignment on this shipment.");
			}

			return result;
		}

		bool CheckShipmentHasAtLeastOneConsol(ForwardingShipment shipment, out string errorMessage)
		{
			var result = true;
			errorMessage = null;
			if (!shipment.Consols.Any())
			{
				result = false;
				errorMessage = Res.GetString("905acde4-dede-4887-b6f2-e86a1006788f", "Please make sure there is a consolidation attached to this shipment.");
			}
			return result;
		}

		bool CheckItemContainer(IEnumerable<HVLVItem> items, out string errorMessage)
		{
			var result = true;
			var hasContainerErrors = false;
			errorMessage = null;

			foreach (var item in items)
			{
				using (item.MarkValidatingForSeaCargoReport())
				{
					item.Validation.ValidateHVI_ContainerNumber();

					if (!hasContainerErrors)
					{
						hasContainerErrors = item.HVI_ContainerNumberInfo.HasErrors();
					}
				}
			}

			if (hasContainerErrors)
			{
				errorMessage = Res.GetString("9223f13d-825a-44d9-98ba-e305a0bd808c", "Please enter container number for Item(s).");
				result = false;
			}

			return result;
		}

		public readonly ForwardingShipment Shipment;

		public HVLVConsignmentHeader Header => Shipment.GetHVLVConsignmentHeader();

		public abstract MultilingualString RelatedJobName { get; }

		public bool CanCreate => !HasActiveRelatedCustomsJobs;
		public bool CanOpen => AllowOpen && HasActiveRelatedCustomsJobs;
		public bool CanSync => AllowSync && HasActiveRelatedCustomsJobs;

		public virtual bool ShouldValidateWaybill => false;
		public virtual bool ShouldValidateItemContainer => false;
		public virtual bool NeedPreScreening => false;

		public IEnumerable<BusinessObject> ActiveRelatedCustomsJobs => GetRelatedCustomsJobsCore().Where(job => !(job is ICancellable cancellable && cancellable.IsCancelled));

		public abstract CustomsRelatedBusinessObjectConverter Converter { get; }

		protected virtual bool AllowOpen => true;
		protected virtual bool AllowSync => true;

		protected abstract Type RelatedCustomsJobType { get; }

		protected IEnumerable<BusinessObject> GetRelatedCustomsJobsCore()
		{
			return Header.GenPivotCollection.CustomsJobs.Where(job => RelatedCustomsJobType.IsAssignableFrom(job.GetType()));
		}

		bool HasActiveRelatedCustomsJobs => Header != null && ActiveRelatedCustomsJobs.Any();

		public abstract string UsageCode { get; }
	}
}
