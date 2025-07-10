using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentLookups : BaseJobShipmentLookups
	{
		public AgencyShipmentLookups(AgencyShipment parent)
			: base(parent)
		{
		}

		public new AgencyShipment Shipment
		{
			get { return Parent as AgencyShipment; }
		}

		public override OrganisationsFindBoxCollection Consignee_List
		{
			get { return AddToConsigneeOrgFilter(new ConsigneeOrForwarderCollection(Factory)); }
		}

		public override OrganisationsFindBoxCollection ConsigneeDefaultOnly_List
		{
			get { return AddToConsigneeOrgFilter(new ConsigneeOrForwarderCollection(Factory)); }
		}

		public override OrganisationsFindBoxCollection Consignor_List
		{
			get { return AddToConsignorOrgFilter(new ConsignorOrForwarderCollection(Factory)); }
		}

		public override OrganisationsFindBoxCollection ConsignorDefaultOnly_List
		{
			get { return AddToConsignorOrgFilter(new ConsignorOrForwarderCollection(Factory)); }
		}

		public override ReadOnlyCodeDescriptionPairList JS_ReleaseType_List
		{
			get { return AgencyRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList(); }
		}

		public override CodeDescriptionPairList JS_HBLAWBChargesDisplay_List
		{
			get { return new OBLChargesDisplayMode(); }
		}

		public override CodeDescriptionPairList JS_PackingMode_List
		{
			get { return Factory.GetCachedValue<AgencyCargoTypeCodeDescriptionPairList>(); }
		}

		public override CodeDescriptionPairList JS_INCO_List
		{
			get
			{
				if (js_INCO_List == null)
				{
					js_INCO_List = new CodeDescriptionPairList();
					js_INCO_List.AddPair(Core.Constants.DomesticPaymentTerms.Collect, ResString.GetMultilingualString("710b4419-f8a9-48a9-85a5-fa515bfee776", "Collect"));
					js_INCO_List.AddPair(Core.Constants.DomesticPaymentTerms.Prepaid, ResString.GetMultilingualString("e8d6ba63-8597-427e-86fe-7929a12c0226", "Prepaid"));
				}

				return js_INCO_List;
			}
		}
		CodeDescriptionPairList js_INCO_List;

		public ShipsAgencyPrincipalCollection Principal_List
		{
			get { return GetPrincipal_List(); }
		}

		ShipsAgencyPrincipalCollection GetPrincipal_List()
		{
			ShipsAgencyPrincipalCollection result;

			if (Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.WebBooking && Globals.IsWeb)
			{
				result = new ShipsAgencyPrincipalCollection(Factory);
				var principals = new List<ZGuid>();

				if (Shipment.Sailing != null && Shipment.Sailing.Voyage != null)
				{
					if (Shipment.Sailing.Voyage.TradeLanes.Count > 0)
					{
						foreach (JobTradeLaneVoyage lane in Shipment.Sailing.Voyage.TradeLanes)
						{
							principals.Add(lane.NB_OH);
						}
					}

					if (Shipment.Sailing.Voyage.Line != null && Shipment.Sailing.Voyage.Line.CompanyData.OB_CRIsShipsAgencyPrincipal)
					{
						principals.Add(Shipment.Sailing.Voyage.Line.PK);
					}

					if (Shipment.Principal != null && Shipment.Principal.CompanyData.OB_CRIsShipsAgencyPrincipal &&
						!principals.Contains(Shipment.Principal.PK))
					{
						principals.Add(Shipment.Principal.PK);
					}
				}

				var sailingRelated = principals.Count > 0 ? new ZQuery(OrgHeaderSchema.PK, principals.ToArray()) : ZQuery.NoResultQuery;
				result.LoadWithMoreFiltering(sailingRelated);
			}
			else
			{
				result = new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory);
			}

			return result;
		}

		#region CodeLists

		public CodeDescriptionPairList JS_ShipmentStatus_List
		{
			get
			{
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
				{
					var cachedShipmentStatusList = Factory.GetCachedValue<CodeDescriptionPairList>(string.Format(CultureInfo.InvariantCulture, "AgencyShipment.Lookups.JS_ShipmentStatus_List_{0}_{1}_{2}_{3}", Shipment.GetIsConfirmed(), Shipment.IsReceivedElectronicBooking(), Shipment.IsReceivedElectronicShippingInstruction(), Shipment.IsCancelled), // hardcoded list identifier
						() => new AgencyShipmentStatusList(Shipment.GetIsConfirmed(), Shipment.IsReceivedElectronicBooking(), Shipment.IsReceivedElectronicShippingInstruction(), Shipment.IsCancelled));

					if (!Shipment.GetIsConfirmed() && Shipment.IsInDatabase && !cachedShipmentStatusList.ContainsCode(Shipment.JS_ShipmentStatus))
					{
						var isAddWhenOriginalIsEBC = Shipment.JS_ShipmentStatusInfo.OriginalValue.ToString() == ShipmentStatusList.Codes.EBookingCancellationRequest
							&& (Shipment.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled || Shipment.JS_ShipmentStatus == Shipment.GetShipmentStatusBeforeLastestEBookingCancellationRequest());

						if (isAddWhenOriginalIsEBC || !Shipment.JS_ShipmentStatusInfo.HasChanges)
						{
							var result = new CodeDescriptionPairList(cachedShipmentStatusList);
							foreach (ICodeDescription bookingStatus in new AgencyShipmentStatusList(false))
							{
								if (bookingStatus.Code == Shipment.JS_ShipmentStatus)
								{
									result.AddPairIfNotExist(bookingStatus.Code, bookingStatus.Description);
									break;
								}
							}

							return result;
						}
					}

					return cachedShipmentStatusList;
				}
				else
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>(string.Format(CultureInfo.InvariantCulture, (NoResString)"AgencyShipment.Lookups.Original_JS_ShipmentStatus_List_{0}", Shipment.GetIsConfirmed()), // hardcoded list identifier
						() => new AgencyShipmentStatusList(Shipment.GetIsConfirmed()));
				}
			}
		}

		#endregion

		#region Binding Lists

		public BusinessObjectCollection ConsignorContacts_List
		{
			get
			{
				if (consignorContacts_List == null)
				{
					if (!Shipment.ConsignorPK.IsValid)
					{
						consignorContacts_List = new OrgContactCollection(Factory, ZQuery.NoResultQuery);
					}
					else
					{
						consignorContacts_List = new OrgContactDependentCollection(Shipment.Consignor, Factory);
						consignorContacts_List.Load();
					}
				}
				return consignorContacts_List;
			}
		}
		BusinessObjectCollection consignorContacts_List;

		internal void ReSetConsignorContacts_List()
		{
			consignorContacts_List = null;
		}

		public BusinessObjectCollection ConsigneeContacts_List
		{
			get
			{
				if (consigneeContacts_List == null)
				{
					if (!Shipment.ConsigneePK.IsValid)
					{
						consigneeContacts_List = new OrgContactCollection(Factory, ZQuery.NoResultQuery);
					}
					else
					{
						consigneeContacts_List = new OrgContactDependentCollection(Shipment.Consignee, Factory);
						consigneeContacts_List.Load();
					}
				}
				return consigneeContacts_List;
			}
		}
		BusinessObjectCollection consigneeContacts_List;

		internal void ReSetConsigneeContacts_List()
		{
			consigneeContacts_List = null;
		}

		#endregion
	}
}
