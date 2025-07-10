using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TaxDateDefaultingOptionLookups : JobConfigurationSelectorLookups
	{
		public TaxDateDefaultingOptionLookups(TaxDateDefaultingOption parent)
			: base(parent)
		{
			Parent = parent;
		}
		new readonly TaxDateDefaultingOption Parent;

		#region LedgerList

		public CodeDescriptionPairList LedgerList => ledgerList ?? (ledgerList = GetLedgerList());
		CodeDescriptionPairList ledgerList;

		public static CodeDescriptionPairList GetLedgerList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(LedgerTypeAdditionalCodes.All, Res.GetString("dc2c7fce-8a8a-47d8-a9aa-87b8a38f3299", "All"));
			result.AddPair(LedgerTypes.AccountsPayable, Res.GetString("3ac661a9-b39b-4479-a945-c6ab97ff5a3b", "Accounts Payable"));
			result.AddPair(LedgerTypes.AccountsReceivable, Res.GetString("5508f4d5-9be5-488e-b203-e88d15c88cc8", "Accounts Receivable"));
			return result;
		}

		public static class LedgerTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region Tax Date Option List

		public CodeDescriptionPairList TaxDateOptionList
		{
			get
			{
				if (TaxDateOptionLookupListBasedOnJobType.TryGetValue(Parent.JobType, out var result))
				{
					return result;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		Dictionary<ZString, CodeDescriptionPairList> TaxDateOptionLookupListBasedOnJobType
		{
			get
			{
				if (taxDateOptionLookupListBasedOnJobType == null)
				{
					taxDateOptionLookupListBasedOnJobType = new Dictionary<ZString, CodeDescriptionPairList>();
					foreach (CodeDescriptionPair jobType in GetJobTypeList())
					{
						var jobTypeCode = jobType.Code;
						switch (jobTypeCode)
						{
							case JobInvoicingConsumerTypes.ShipmentCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForShipmentsList);
								break;
							case JobInvoicingConsumerTypes.ForwardingConsolCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForForwardingConsolList);
								break;
							case JobInvoicingConsumerTypes.GatewayConsolCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForGateWayConsolList);
								break;
							case JobInvoicingConsumerTypes.BrokerageCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForDeclarationList);
								break;
							case JobInvoicingConsumerTypes.TransportBookingCode:
							case JobInvoicingConsumerTypes.TransportBookingConsignmentCode:
							case JobInvoicingConsumerTypes.TransportConsignmentCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForTransportList);
								break;
							case JobInvoicingConsumerTypes.AgencyBookingCode:
							case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForAgencyList);
								break;
							case JobInvoicingConsumerTypes.LocalCartageCode:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForCartageList);
								break;
							default:
								taxDateOptionLookupListBasedOnJobType.Add(jobTypeCode, TaxDateOptionForOtherList);
								break;
						}
					}
				}
				return taxDateOptionLookupListBasedOnJobType;
			}
		}
		Dictionary<ZString, CodeDescriptionPairList> taxDateOptionLookupListBasedOnJobType;

		CodeDescriptionPairList TaxDateOptionForShipmentsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				result.AddPair(TaxDateDefaultingOption.Code.ArrivalDate, TaxDateDefaultingOption.Description.ArrivalDate);
				result.AddPair(TaxDateDefaultingOption.Code.DepartureDate, TaxDateDefaultingOption.Description.DepartureDate);
				result.AddPair(TaxDateDefaultingOption.Code.EstimatedArrivalDate, TaxDateDefaultingOption.Description.EstimatedArrivalDate);
				result.AddPair(TaxDateDefaultingOption.Code.EstimatedDepartureDate, TaxDateDefaultingOption.Description.EstimatedDepartureDate);
				result.AddPair(TaxDateDefaultingOption.Code.PickupDate, TaxDateDefaultingOption.Description.ActualEstimatePickupDate);
				result.AddPair(TaxDateDefaultingOption.Code.DeliveryDate, TaxDateDefaultingOption.Description.ActualEstimateDeliveryDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForGateWayConsolList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				result.AddPair(TaxDateDefaultingOption.Code.ArrivalDate, TaxDateDefaultingOption.Description.ArrivalDate);
				result.AddPair(TaxDateDefaultingOption.Code.DepartureDate, TaxDateDefaultingOption.Description.DepartureDate);
				result.AddPair(TaxDateDefaultingOption.Code.EstimatedArrivalDate, TaxDateDefaultingOption.Description.EstimatedArrivalDate);
				result.AddPair(TaxDateDefaultingOption.Code.EstimatedDepartureDate, TaxDateDefaultingOption.Description.EstimatedDepartureDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForForwardingConsolList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForDeclarationList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				result.AddPair(TaxDateDefaultingOption.Code.CustomClearanceDate, TaxDateDefaultingOption.Description.CustomClearanceDate);
				result.AddPair(TaxDateDefaultingOption.Code.PickupDate, TaxDateDefaultingOption.Description.ActualEstimatePickupDate);
				result.AddPair(TaxDateDefaultingOption.Code.DeliveryDate, TaxDateDefaultingOption.Description.ActualEstimateDeliveryDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForTransportList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForAgencyList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				result.AddPair(TaxDateDefaultingOption.Code.VesselArrivalDate, TaxDateDefaultingOption.Description.VesselArrivalDate);
				result.AddPair(TaxDateDefaultingOption.Code.VesselDepartureDate, TaxDateDefaultingOption.Description.VesselDepartureDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForCartageList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				result.AddPair(TaxDateDefaultingOption.Code.PickupDate, TaxDateDefaultingOption.Description.EstimatePickupDate);
				result.AddPair(TaxDateDefaultingOption.Code.DeliveryDate, TaxDateDefaultingOption.Description.EstimateDeliveryDate);
				return result;
			}
		}

		CodeDescriptionPairList TaxDateOptionForOtherList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxDateDefaultingOption.Code.Today, TaxDateDefaultingOption.Description.Today);
				result.AddPair(TaxDateDefaultingOption.Code.InvoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
				return result;
			}
		}
		#endregion

	}
}
