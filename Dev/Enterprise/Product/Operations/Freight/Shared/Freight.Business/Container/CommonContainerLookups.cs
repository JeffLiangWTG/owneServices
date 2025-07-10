using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business
{
	public class CommonContainerLookups : JobContainerLookups
	{
		public CommonContainerLookups(AutoJobContainer parent)
			: base(parent)
		{
		}

		public RefContainerCollection RefContainer_List
		{
			get { return refContainer_List ?? (refContainer_List = new RefContainerCollection(Factory)); }
		}
		RefContainerCollection refContainer_List;

		public FCLEquipmentNeededList DropMode_List
		{
			get { return Factory.GetCachedValue<FCLEquipmentNeededList>(); }
		}

		public ContainerYardCollection ContainerYard_List
		{
			get { return BindingLists.OrgContainerYard_List; }
		}

		protected new BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public static CodeDescriptionPairList GetContainerModesList(ZString transportMode)
		{
			if (containerModesListDictionary == null)
			{
				containerModesListDictionary = new Dictionary<ZString, CodeDescriptionPairList>();
			}

			if (!containerModesListDictionary.ContainsKey(transportMode))
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				if (transportMode == Constants.TransportModes.Air)
				{
					list.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
				}
				else if (transportMode == Constants.TransportModes.Road)
				{
					list.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					list.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					list.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					list.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					list.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					list.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					list.AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
				}
				else
				{
					list.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					list.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					list.AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
					list.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					list.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					list.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					list.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
				}

				containerModesListDictionary.Add(transportMode, list);
			}

			return containerModesListDictionary[transportMode];
		}

		[ThreadStatic]
		static Dictionary<ZString, CodeDescriptionPairList> containerModesListDictionary;

		public CodeDescriptionPairList SealParty_List => GetCachedValue_SealParty_List(Factory);

		public static CodeDescriptionPairList GetCachedValue_SealParty_List(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CommonContainer.Lookups.SealParty_List",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.ContainerSealParties.Codes.CarrierShippingLine, Constants.ContainerSealParties.Descriptions.CarrierShippingLine);
					result.AddPair(Constants.ContainerSealParties.Codes.ConsignorShipper, Constants.ContainerSealParties.Descriptions.ConsignorShipper);
					result.AddPair(Constants.ContainerSealParties.Codes.Customs, Constants.ContainerSealParties.Descriptions.Customs);
					result.AddPair(Constants.ContainerSealParties.Codes.Quarantine, Constants.ContainerSealParties.Descriptions.Quarantine);
					result.AddPair(Constants.ContainerSealParties.Codes.Terminal, Constants.ContainerSealParties.Descriptions.Terminal);
					return result;
				});
		}

		public CodeDescriptionPairList GrossWeightVerificationTypeList
		{
			get
			{
				if (RTLEnabled)
				{
					return Factory.GetCachedValue("CommonContainer.Lookups.GrossWeightVerificationTypeList_RTLEnabled",
						delegate
						{
							var result = new CodeDescriptionPairList
							{
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified),
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotRequired),
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container),
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages),
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.WeightAtTerminal),
								new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod,
									Constants.ContainerGrossWeightVerificationTypes.Descriptions.RationalMethod)
							};

							return result;
						});
				}

				return Factory.GetCachedValue("CommonContainer.Lookups.GrossWeightVerificationTypeList",
					delegate
					{
						var result = new CodeDescriptionPairList
						{
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotRequired),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.WeightAtTerminal)
						};

						return result;
					});
			}
		}

		public CodeDescriptionPairList VGMStatusList =>
				Factory.GetCachedValue("CommonContainer.Lookups.GrossWeightVerificationStatusList",
					() => new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotVerified),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotRequired),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotSent),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Sent),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.AmendedNotSent),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Acknowledged),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Rejected),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Accepted,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Accepted),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawSent),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawAcknowledged),
						new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected,
							Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawRejected)
					});

		bool RTLEnabled
		{
			get
			{
				var containerParent = Container.ContainerParent;
				var containerParentExists = containerParent != null;
				if (containerParentExists && containerParent.LoadPort != null
					&& containerParent.LoadPort.RL_RN_NKCountryCode == Constants.CountryCodes.UnitedStates)
				{
					return true;
				}

				var transports = containerParentExists ? Container.ContainerParent.Transports : Container.Booking?.Transports;
				return transports?.Cast<Transport>().Any(x => x.JW_RL_NKLoadPort.StartsWith(Constants.CountryCodes.UnitedStates,
						   StringComparison.OrdinalIgnoreCase)) ?? false;
			}
		}

		CommonContainer Container => (CommonContainer)Parent;

		public OrgHeaderCollection GrossWeightVerifiedByList
		{
			get
			{
				return grossWeightVerifiedByList ?? (grossWeightVerifiedByList = new OrgHeaderCollection(Factory));
			}
		}
		OrgHeaderCollection grossWeightVerifiedByList;

		public CodeDescriptionPairList SpotRateAutoratingModesList
		{
			get
			{
				return Factory.GetCachedValue("SpotRateAutoratingModesList",
					() => new CodeDescriptionPairList(OLookUpEditType.FreightRateAutoratingMode));
			}
		}

		public IBusinessObjectCollection SupplierBookingList
		{
			get
			{
				if (supplierBookingList == null)
				{
					supplierBookingList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IJobSupplierBookingCollection>(), Factory);
				}
				((IFilterBusinessObjectDefaultsProvider)supplierBookingList).FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Load Mode", "Property", (ZString)Constants.ContainerLoadListHeaderLoadMode.ContainerYard, false));
				return supplierBookingList;
			}
		}

		IBusinessObjectCollection supplierBookingList;

		public IBusinessObjectCollection RelatedContainerLoadListCollection
		{
			get
			{
				if (relatedContainerLoadListCollection == null)
				{
					var filter = new ZQuery(ContainerLoadListHeaderSchema.CLH_LoadMode, Constants.ContainerLoadListHeaderLoadMode.ContainerYard);
					relatedContainerLoadListCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IContainerLoadListHeaderCollection>(), new object[] { Factory, filter });
				}
				return relatedContainerLoadListCollection;
			}
		}

		IBusinessObjectCollection relatedContainerLoadListCollection;

		public IBusinessObjectCollection RelatedContainerLoadPlanCollection
		{
			get
			{
				if (relatedContainerLoadPlanCollection == null)
				{
					var filter = new ZQuery(ContainerLoadListHeaderSchema.CLH_LoadMode, Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation);
					relatedContainerLoadPlanCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IContainerLoadListHeaderCollection>(), new object[] { Factory, filter });
				}
				return relatedContainerLoadPlanCollection;
			}
		}

		IBusinessObjectCollection relatedContainerLoadPlanCollection;
	}
}
