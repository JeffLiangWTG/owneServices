using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyLookups : AutoOrgRelatedPartyLookups
	{
		public OrgRelatedPartyLookups(AutoOrgRelatedParty parent)
			: base(parent)
		{
		}

		protected new OrgRelatedParty Parent
		{
			get { return (OrgRelatedParty)base.Parent; }
		}

		public OrgHeader ParentOrg
		{
			get { return Parent.ParentOrg; }
		}

		public override OrgHeaderCollection RelatedParties
		{
			get
			{
				if (Parent.PR_PartyType == RelatedPartyTypeList.Codes.CSAApprovedVendor)
				{
					return new ConsignorCollection(Factory);
				}

				if (Parent.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor && !Parent.PR_GC.IsEmpty) //for SPC, Only for COM level the related party should be payable
				{
					return new CreditorCollection(Factory);
				}

				if (Parent.PR_PartyType == RelatedPartyTypeList.Codes.SelfFilerForICS2)
				{
					return new SelfFilerCollection(Factory);
				}

				return new OrgHeaderCollection(Factory);
			}
		}

		public CompanyLevelList CompanyLevelList
		{
			get { return companyLevelList ?? (companyLevelList = new CompanyLevelList()); }
		}
		CompanyLevelList companyLevelList;

		public CodeDescriptionPairList ForwarderPartyTypeList
		{
			get
			{
				if (forwarderPartyTypeList == null)
				{
					forwarderPartyTypeList = new CodeDescriptionPairList();

					foreach (var forwarderPartyTypeCode in OrgRelatedPartyTypeHelper.GetForwarderPartyTypeCodes())
					{
						forwarderPartyTypeList.AddPair(forwarderPartyTypeCode, AllPartyTypeList.GetDescriptionFromCode(forwarderPartyTypeCode));
					}
				}

				return forwarderPartyTypeList;
			}
		}
		CodeDescriptionPairList forwarderPartyTypeList;

		public CodeDescriptionPairList ConsignorPartyTypeList
		{
			get
			{
				if (consignorPartyTypeList == null)
				{
					consignorPartyTypeList = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair pair in AllPartyTypeList)
					{
						var defaultDirection = OrgRelatedPartyTypeHelper.GetDefaultDirection(pair.Code);

						if (pair.Code == RelatedPartyTypeList.Codes.ServiceProviderCreditor)
						{
							continue;
						}

						if (pair.Code == RelatedPartyTypeList.Codes.WarehouseForwarder ||
							defaultDirection == RelatedPartyDirectionList.Codes.Pickup ||
							defaultDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery ||
							OrgRelatedPartyTypeHelper.ShouldHaveDirection(pair.Code) && !OrgRelatedPartyTypeHelper.ShouldCalculateDirection(pair.Code))
						{
							consignorPartyTypeList.Add(pair);
						}
					}
				}

				return consignorPartyTypeList;
			}
		}
		CodeDescriptionPairList consignorPartyTypeList;

		public CodeDescriptionPairList ConsigneePartyTypeList
		{
			get
			{
				if (consigneePartyTypeList == null)
				{
					consigneePartyTypeList = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair pair in AllPartyTypeList)
					{
						var defaultDirection = OrgRelatedPartyTypeHelper.GetDefaultDirection(pair.Code);

						if (pair.Code == RelatedPartyTypeList.Codes.ServiceProviderCreditor)
						{
							continue;
						}

						if (pair.Code == RelatedPartyTypeList.Codes.WarehouseForwarder ||
							defaultDirection == RelatedPartyDirectionList.Codes.Delivery ||
							defaultDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery ||
							OrgRelatedPartyTypeHelper.ShouldHaveDirection(pair.Code) && !OrgRelatedPartyTypeHelper.ShouldCalculateDirection(pair.Code))
						{
							consigneePartyTypeList.Add(pair);
						}
					}
				}

				return consigneePartyTypeList;
			}
		}
		CodeDescriptionPairList consigneePartyTypeList;

		public virtual PartyTypeDescriptionOnlyList PartyTypeList
		{
			get { return partyTypeList ?? (partyTypeList = new PartyTypeDescriptionOnlyList()); }
		}
		PartyTypeDescriptionOnlyList partyTypeList;

		public virtual PartyTypeDescriptionOnlyList ParentPartyTypeList
		{
			get { return parentPartyTypeList ?? (parentPartyTypeList = new PartyTypeDescriptionOnlyList()); }
		}
		PartyTypeDescriptionOnlyList parentPartyTypeList;

		#region ContainerModeList

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var key = Parent.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCoLoadWith ? "FCW" : (NoResString)"Common";
				return Factory.GetCachedValue("OrgRelatedParty.ContainerModeList_" + Parent.PR_FreightTransportMode + "_" + key,
					() =>
					{
						if (key == "FCW")
						{
							return ObjectFactory.Get<IFreightCodePairListProvider>().GetConsolModeList(Core.Constants.AgentType.CoLoad, Parent.PR_FreightTransportMode);
						}
						else
						{
							return ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(Parent.PR_FreightTransportMode);
						}
					});
			}
		}

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var key = "";
				switch (Parent.PR_PartyType)
				{
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
						key = "FCW";
						break;
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
						key = "ACR";
						break;
					default:
						key = (NoResString)"Common";
						break;
				}

				return Factory.GetCachedValue("OrgRelatedParty.TransportModeList_" + key, () =>
				{
					var result = new CodeDescriptionPairList();
					if (key == "ACR")
					{
						result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
					}
					else
					{
						result = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);

						if (key == "FCW")
						{
							result.RemoveCode(Core.Constants.TransportModes.Courier);
							result.RemoveCode(Core.Constants.TransportModes.Other);
						}
					}

					return result;
				});
			}
		}

		#endregion

		public CodeDescriptionPairList FreightDirectionList
		{
			get
			{
				var key = Parent.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCoLoadWith ? "FCW" : (NoResString)"Common";
				return Factory.GetCachedValue("OrgRelatedParty.FreightDirectionList_" + key, () =>
				{
					var freightDirectionList = new CodeDescriptionPairList();

					if (key == "FCW")
					{
						freightDirectionList.AddPair(RelatedPartyDirectionList.Codes.Pickup, RelatedPartyDirectionList.Descriptions.Pickup);
					}
					else
					{
						freightDirectionList.AddPair(RelatedPartyDirectionList.Codes.Delivery, RelatedPartyDirectionList.Descriptions.Delivery);
						freightDirectionList.AddPair(RelatedPartyDirectionList.Codes.Pickup, RelatedPartyDirectionList.Descriptions.Pickup);
						freightDirectionList.AddPair(RelatedPartyDirectionList.Codes.PickupAndDelivery, RelatedPartyDirectionList.Descriptions.PickupAndDelivery);
					}

					return freightDirectionList;
				});
			}
		}

		public virtual RelatedPartyTypeList AllPartyTypeList
		{
			get
			{
				return Factory.GetCachedValue("OrgRelatedParty.AllPartyTypeList", () => new RelatedPartyTypeList());
			}
		}

		public virtual RelatedPartyTypeList AllParentTypeList
		{
			get { return allParentTypeList ?? (allParentTypeList = new RelatedPartyTypeList()); }
		}
		RelatedPartyTypeList allParentTypeList;

		public new OrgAddressDependentCollection Addresses
		{
			get
			{
				return Parent.PR_PartyType == RelatedPartyTypeList.Codes.Warehouse || Parent.PR_PartyType == RelatedPartyTypeList.Codes.PickupFrom || Parent.PR_PartyType == RelatedPartyTypeList.Codes.DeliveryTo || Parent.PR_PartyType == RelatedPartyTypeList.Codes.NotifyParty || Parent.PR_PartyType == RelatedPartyTypeList.Codes.ProductRelationship
					? Parent.RelatedParty != null ? Parent.RelatedParty.Addresses : new OrgAddressDependentCollection(Factory)
					: ParentOrg.Addresses;
			}
		}

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}
	}
}
