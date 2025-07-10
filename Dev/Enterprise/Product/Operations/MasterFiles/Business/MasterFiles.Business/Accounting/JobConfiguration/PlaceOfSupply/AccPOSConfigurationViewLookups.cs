//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPOSConfigurationViewLookups
//
//    This class should be used for overriding collections in AutoAccPOSConfigurationViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSConfigurationViewLookups : AutoAccPOSConfigurationViewLookups
	{
		public AccPOSConfigurationViewLookups(AutoAccPOSConfigurationView parent) : base(parent)
		{
		}

		public IJobConfiguration ParentJobConfiguration => (IJobConfiguration)base.Parent;
		public new AccPOSConfiguration Parent => (AccPOSConfiguration)base.Parent;

		#region ChargeTypeList

		public CodeDescriptionPairList ChargeTypeList => new AccPOSChargeTypeList();

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				var supportedList = new[] { "ALL", JobInvoicingConsumerTypes.BrokerageCode,
					JobInvoicingConsumerTypes.ForwardingConsolCode,
					JobInvoicingConsumerTypes.QuotedBookingCode,
					JobInvoicingConsumerTypes.ShipmentCode,
					JobInvoicingConsumerTypes.WorkItemCode
				};

				var jobTypeList = Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;
				var notSupported = jobTypeList.Cast<CodeDescriptionPair>().Where(y => !supportedList.Contains(y.Code)).Select(y => y.Code).ToArray();
				foreach (var item in notSupported)
				{
					jobTypeList.RemoveCode(item);
				}

				return jobTypeList;
			}
		}

		#endregion

		#region ServiceDirectionList

		public CodeDescriptionPairList ServiceDirectionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);
				result.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
				result.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
				result.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
				result.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
				return result;
			}
		}

		#endregion

		#region IncoTermList

		public CodeDescriptionPairList IncoTermList =>
			Parent?.PSC_JobType.ToString() == JobInvoicingConsumerTypes.ShipmentCode ||
			Parent?.PSC_JobType.ToString() == JobInvoicingConsumerTypes.BrokerageCode ||
			Parent?.PSC_JobType.ToString() == JobInvoicingConsumerTypes.QuotedBookingCode ||
			Parent?.PSC_JobType.ToString() == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All
			? new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)
			: new CodeDescriptionPairList();

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (Parent?.PSC_JobType.ToString() == JobInvoicingConsumerTypes.BrokerageCode
						&& Activator.CreateInstance(ObjectFactory.GetType<IDeclarationTransportModeCodeDescriptionPairProvider>()) is ICodeDescriptionPairListProvider provider)
				{
					var result = new CodeDescriptionPairList(provider.CodeDescriptionPairList);
					result.Insert(0, new CodeDescriptionPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All));
					return result;
				}
				var list = new CodeDescriptionPairList(Parent.JobTypeDirectionAndTransportListProvider.TransportModeList);

				if (Parent?.PSC_JobType.ToString() == JobInvoicingConsumerTypes.QuotedBookingCode)
				{
					list.RemoveCode(Constants.TransportModes.FixedTransportInstallations);
					list.RemoveCode(Constants.TransportModes.InlandWaterwayTransport);
					list.RemoveCode(Constants.TransportModes.OwnPropulsion);
					list.RemoveCode(Constants.TransportModes.Mail);
					list.RemoveCode(Constants.TransportModes.SeaAir);
					list.RemoveCode(Constants.TransportModes.AirSea);
				}

				return list;
			}
		}

		#endregion

		#region TaxRegistrationTypeList

		public CodeDescriptionPairList TaxRegistrationTypeList => new AccPOSTaxRegistrationList();

		#endregion

		#region BranchList

		public CodeDescriptionPairList BranchList
			=> Factory.GetCachedValue(nameof(AccPOSConfigurationViewLookups) + "." + nameof(BranchList), () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(string.Empty, Res.GetString("b14f08e6-d64a-4260-a96a-21fe8a8d3d54", "All Branches"));

					var branches = new GlbBranchCollection(Factory);
					var companyFilter = new ZQuery(GlbBranchSchema.GB_GC, Parent.PSC_GC);
					companyFilter.OrderBy = GlbBranchSchema.Constants.GB_Code;
					branches.Load(companyFilter);
					foreach (GlbBranch branch in branches)
					{
						result.AddPair(branch.GB_Code, branch.GB_BranchName);
					}

					return result;
				});

		#endregion

		#region SupplyTypeList

		public CodeDescriptionPairList SupplyTypeList => AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.GetActiveCodeDescriptionPairList();

		#endregion

		#region PlaceOfSupplyRuleList

		public CodeDescriptionPairList PlaceOfSupplyRuleList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AccPOSRuleList.Codes.BillToPartyLocation, AccPOSRuleList.Descriptions.BillToPartyLocation);
				result.AddPair(AccPOSRuleList.Codes.SupplierLocation, AccPOSRuleList.Descriptions.SupplierLocation);

				var enabledPlaceOfSupplyTypes = PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(Parent.Company ?? GlbCompany.CurrentCompany);
				if (enabledPlaceOfSupplyTypes.ContainsCode(PlaceOfSupplyTypes.PredefinedRule))
				{
					result.AddPair(AccPOSRuleList.Codes.OtherTerritories, AccPOSRuleList.Descriptions.OtherTerritories);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.ShipmentCode)
				{
					result.AddPair(AccPOSRuleList.Codes.PickupCFS, AccPOSRuleList.Descriptions.PickupCFS);
					result.AddPair(AccPOSRuleList.Codes.DeliveryCFS, AccPOSRuleList.Descriptions.DeliveryCFS);
					result.AddPair(AccPOSRuleList.Codes.PickupLocation, AccPOSRuleList.Descriptions.PickupLocation);
					result.AddPair(AccPOSRuleList.Codes.DeliveryLocation, AccPOSRuleList.Descriptions.DeliveryLocation);
					result.AddPair(AccPOSRuleList.Codes.PickupAgent, AccPOSRuleList.Descriptions.PickupAgent);
					result.AddPair(AccPOSRuleList.Codes.DeliveryAgent, AccPOSRuleList.Descriptions.DeliveryAgent);
					result.AddPair(AccPOSRuleList.Codes.Origin, AccPOSRuleList.Descriptions.Origin);
					result.AddPair(AccPOSRuleList.Codes.Destination, AccPOSRuleList.Descriptions.Destination);
					result.AddPair(AccPOSRuleList.Codes.PickupTransitWarehouse, AccPOSRuleList.Descriptions.PickupTransitWarehouse);
					result.AddPair(AccPOSRuleList.Codes.DeliveryTransitWarehouse, AccPOSRuleList.Descriptions.DeliveryTransitWarehouse);
					result.AddPair(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry, AccPOSRuleList.Descriptions.FirstPortOfLoadingInCompanyCountry);
					result.AddPair(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry, AccPOSRuleList.Descriptions.LastPortOfDischargeInCompanyCountry);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.ForwardingConsolCode
					|| Parent.PSC_JobType == JobInvoicingConsumerTypes.ShipmentCode)
				{
					result.AddPair(AccPOSRuleList.Codes.ConsolPortOfLoading, AccPOSRuleList.Descriptions.ConsolPortOfLoading);
					result.AddPair(AccPOSRuleList.Codes.ConsolPortOfDischarge, AccPOSRuleList.Descriptions.ConsolPortOfDischarge);
					result.AddPair(AccPOSRuleList.Codes.ConsolSendingAgent, AccPOSRuleList.Descriptions.ConsolSendingAgent);
					result.AddPair(AccPOSRuleList.Codes.ConsolReceivingAgent, AccPOSRuleList.Descriptions.ConsolReceivingAgent);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.ForwardingConsolCode)
				{
					result.AddPair(AccPOSRuleList.Codes.DepartureCFS, AccPOSRuleList.Descriptions.DepartureCFS);
					result.AddPair(AccPOSRuleList.Codes.ArrivalCFS, AccPOSRuleList.Descriptions.ArrivalCFS);
					result.AddPair(AccPOSRuleList.Codes.DepartureCTO, AccPOSRuleList.Descriptions.DepartureCTO);
					result.AddPair(AccPOSRuleList.Codes.ArrivalCTO, AccPOSRuleList.Descriptions.ArrivalCTO);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.BrokerageCode)
				{
					result.AddPair(AccPOSRuleList.Codes.PortOfLoading, AccPOSRuleList.Descriptions.PortOfLoading);
					result.AddPair(AccPOSRuleList.Codes.PortOfDischarge, AccPOSRuleList.Descriptions.PortOfDischarge);
					result.AddPair(AccPOSRuleList.Codes.PortOfFirstArrival, AccPOSRuleList.Descriptions.PortOfFirstArrival);
					result.AddPair(AccPOSRuleList.Codes.PortOfOrigin, AccPOSRuleList.Descriptions.PortOfOrigin);
					result.AddPair(AccPOSRuleList.Codes.FinalDestination, AccPOSRuleList.Descriptions.FinalDestination);
					result.AddPair(AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry, AccPOSRuleList.Descriptions.FirstPortOfLoadingInCompanyCountry);
					result.AddPair(AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry, AccPOSRuleList.Descriptions.LastPortOfDischargeInCompanyCountry);
					result.AddPair(AccPOSRuleList.Codes.PickupLocation, AccPOSRuleList.Descriptions.PickupLocation);
					result.AddPair(AccPOSRuleList.Codes.DeliveryLocation, AccPOSRuleList.Descriptions.DeliveryLocation);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.QuotedBookingCode)
				{
					result.AddPair(AccPOSRuleList.Codes.Origin, AccPOSRuleList.Descriptions.Origin);
					result.AddPair(AccPOSRuleList.Codes.Destination, AccPOSRuleList.Descriptions.Destination);
					result.AddPair(AccPOSRuleList.Codes.PickupLocation, AccPOSRuleList.Descriptions.PickupAddress);
					result.AddPair(AccPOSRuleList.Codes.PickupCFS, AccPOSRuleList.Descriptions.PickupCFS);
					result.AddPair(AccPOSRuleList.Codes.DeliveryCFS, AccPOSRuleList.Descriptions.DeliveryCFS);
					result.AddPair(AccPOSRuleList.Codes.Load, AccPOSRuleList.Descriptions.Load);
					result.AddPair(AccPOSRuleList.Codes.Discharge, AccPOSRuleList.Descriptions.Discharge);
					result.AddPair(AccPOSRuleList.Codes.PickupCTO, AccPOSRuleList.Descriptions.PickupCTO);
					result.AddPair(AccPOSRuleList.Codes.DeliveryCTO, AccPOSRuleList.Descriptions.DeliveryCTO);
				}

				if (Parent.PSC_JobType == JobInvoicingConsumerTypes.WorkItemCode)
				{
					result.AddPair(AccPOSRuleList.Codes.CountryRegionPort, AccPOSRuleList.Descriptions.CountryRegionPort);
				}

				return result;
			}
		}

		#endregion
	}
}
