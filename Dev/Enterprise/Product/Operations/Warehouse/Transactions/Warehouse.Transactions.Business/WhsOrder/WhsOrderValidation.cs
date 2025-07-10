using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderValidation : WhsPickableDocketValidation
	{
		public WhsOrderValidation(WhsOrder parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsOrder Parent
		{
			get { return (WhsOrder)base.Parent; }
		}

		#endregion

		#region CheckWD_ExternalReferenceForDuplicates

		protected override bool ShouldCheckForDuplicateExternalReference() => !Parent.BOM.IsAutoCreatingWorkOrders;

		#endregion

		#region CheckWD_WhsOrderFulfillmentRule

		protected override void CheckWD_WhsOrderFulfillmentRule()
		{
			base.CheckWD_WhsOrderFulfillmentRule();

			ListValidation.ErrorIfInvalidCode(Parent.WD_WhsOrderFulfillmentRuleInfo);

			if (!Parent.WD_WhsOrderFulfillmentRuleInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.WD_WhsOrderFulfillmentRuleInfo);
			}
			if (!Parent.IsFulfillmentRuleMet)
			{
				Parent.WD_WhsOrderFulfillmentRuleInfo.AddWarning(Res.GetString("fddb0eae-bd44-4030-8f69-614334f3cabe",
@"The Fulfillment Rule has not been met.
Pick documentation cannot be printed until the Fulfillment Rule has been satisfied or manually overridden."));
			}
		}
		#endregion

		#region CheckWD_OH_Client

		protected override void CheckWD_OH_Client()
		{
			base.CheckWD_OH_Client();

			var parent = Parent;
			if (parent.Client != null)
			{
				if (parent.IsCustomsTransaction && !parent.Warehouse.IsFTZBondedEnabledAndUSJurisdiction() && !DoesClientHaveCPCode())
				{
					parent.WD_OH_ClientInfo.AddWarning(Res.GetString("8158796a-e98b-4753-aa19-5de380f2d0d3", "This client does not have a Customs CP Permit Code"));
				}
			}
		}

		bool DoesClientHaveCPCode()
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CustomsCPPermitCode);
			query.AddToFilter(OrgCusCodeSchema.OK_OH, Parent.WD_OH_Client);
			return Parent.Factory.LoadTop1<OrgCusCode>(query) != null;
		}

		#endregion

		#region CheckWD_WW_Whs

		protected override void CheckWD_WW_Whs()
		{
			base.CheckWD_WW_Whs();

			var parent = Parent;
			if (!parent.WD_WW_WhsInfo.HasErrors() && parent.IsInDatabase && parent.WD_WW_WhsInfo.HasChanges)
			{
				if (!parent.WD_WL_CrossDock.IsEmpty && parent.WD_WL_CrossDockInfo.OriginalValue.IsValid)
				{
					parent.WD_WW_WhsInfo.AddError(Res.GetString("9521C7A0-E930-456C-9C97-0DEBA8B5471D", "The order already has Cross Dock Location, the Cross Dock Location should be removed prior to changing the warehouse."));
				}
				else if (parent.HasReservedStock)
				{
					var originalWarehouse = parent.Factory.Load<WhsWarehouse>((ZGuid)parent.WD_WW_WhsInfo.OriginalValue);
					var warehouseCode = originalWarehouse != null ? originalWarehouse.WW_WarehouseCode : ZString.Empty;
					Parent.WD_WW_WhsInfo.AddError(Res.GetString("4086402C-A9DA-4D38-B6EC-81F977FEA2A6", "This order has Cross-Dock Allocation(s) linked to Warehouse {0}, the Cross-Dock Allocation(s) should be removed prior to changing the warehouse.", warehouseCode));
				}
			}
		}

		#endregion

		#region CheckWD_ContainerMode

		protected override void CheckWD_ContainerMode()
		{
			base.CheckWD_ContainerMode();

			ListValidation.ErrorIfInvalidCode(Parent.WD_ContainerModeInfo);
		}

		#endregion

		#region CheckWD_TransportMode

		protected override void CheckWD_TransportMode()
		{
			base.CheckWD_TransportMode();

			ListValidation.ErrorIfInvalidCode(Parent.WD_TransportModeInfo);
		}

		#endregion

		#region CheckWD_DocketType

		protected override void CheckWD_DocketType()
		{
			base.CheckWD_DocketType();
			if (Parent.WD_DocketType != CodeLists.DocketType.Codes.Order)
			{
				Parent.WD_DocketTypeInfo.AddError(Res.GetString("ff1e1bb0-63b6-4845-8190-df20b9a959fc", "The Docket Type is not set to Order."));
			}
		}

		#endregion

		#region CheckWD_DocketStatus

		protected override void CheckWD_DocketStatus()
		{
			base.CheckWD_DocketStatus();
			if (!Parent.WD_DocketStatusInfo.HasErrors() &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.New) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Entered) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.AttachedToPick) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Picking) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Cancelled) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Error) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Held) &&
				(Parent.WD_DocketStatus != WhsOrderStatus.Codes.Departed))
			{
				Parent.WD_DocketStatusInfo.AddError(Res.GetString("122edb95-cb22-4095-9a9c-addf41bda3f8", "The Docket Status '{0}' is invalid for this docket type.", Parent.WD_DocketStatus));
			}
		}

		protected override CodeDescriptionPairList GetValidCodeList()
		{
			var docketStatus = new DocketStatus();
			docketStatus.AddPair(WhsOrderStatus.Codes.Departed);
			return docketStatus;
		}

		#endregion

		#region CheckWD_RequiredDate

		protected override void CheckWD_RequiredDate()
		{
			base.CheckWD_RequiredDate();
			MandatoryValidation.CheckEntered(Parent.WD_RequiredDateInfo);

			if (Parent.Warehouse != null && !WhsOrderPreDateAllowed &&
				Parent.Warehouse.WW_UseRequiredDateForOutwardsFinalisedDate)
			{
				if (Parent.WD_RequiredDate < ZDateTimeOffset.Today.AddDays(-7))
				{
					Parent.WD_RequiredDateInfo.AddError(Res.GetString("0a7366f2-8d3b-4d14-9345-dc7bb2883835", "You do not have the required security rights to enter a required date more than a week in the past."));
				}
			}
		}

		#endregion

		#region CheckWD_PL_NKCarrierServiceLevel

		protected override void CheckWD_PL_NKCarrierServiceLevel()
		{
			base.CheckWD_PL_NKCarrierServiceLevel();
			if (Parent.WD_WP.IsValid && Parent.WD_PL_NKCarrierServiceLevelInfo.HasChanges && Parent.IsInDatabase)
			{
				var orderStatus = Parent.WarehouseOrderStatus;
				if (orderStatus != WhsOrderStatus.Codes.ReadyToPack && orderStatus != WhsOrderStatus.Codes.Departed && !Parent.IsOrderAssignedToLoad && Parent.IsAnyPackageInConsolidationLocationOrOnConsolidationHU())
				{
					Parent.WD_PL_NKCarrierServiceLevelInfo.AddError(Res.GetString("d60c6011-f033-4cb8-b425-263b30916ed3", "Cannot change the Carrier Service Level when any packages are in a Packing Consolidation Location or on a Handling Unit."));
				}
			}
		}

		#endregion

		#region CheckWD_TotalUnits

		protected override void CheckWD_TotalUnits()
		{
			base.CheckWD_TotalUnits();
			if (Parent.WD_TotalUnits != Parent.WD_TotalUnitsFromLines)
			{
				AddWD_TotalUnitsFromLinesMismatchWarning(Res.GetString("a9905bf4-fd02-41c6-88c9-ba553a78073d", "Total Units {0} does not equal the total of all line units {1}.", Parent.WD_TotalUnits, Parent.WD_TotalUnitsFromLines));
			}

			if (Parent.ShortfallExists)
			{
				Parent.WD_TotalUnitsInfo.AddWarning(Res.GetString("8c0dc52f-f967-49cf-a009-52a249585160", "One or more Order Lines are in Shortfall."));
			}
		}

		protected virtual void AddWD_TotalUnitsFromLinesMismatchWarning(string message)
		{
			Parent.WD_TotalUnitsInfo.AddWarning(message);
		}

		#endregion

		#region CheckWD_TotalOrderValue

		protected override void CheckWD_TotalOrderValue()
		{
			base.CheckWD_TotalOrderValue();
			if (Parent.WD_TotalOrderValue != Parent.GetTotalOrderLineValue())
			{
				Parent.WD_TotalOrderValueInfo.AddWarning(Res.GetString("3ddbb41c-16a7-4c16-b65b-76b26102023f", "Total Order Value does not equal total of all lines value."));
			}
		}

		#endregion

		#region CheckWD_UnitsSent

		protected override void CheckWD_UnitsSent()
		{
			base.CheckWD_UnitsSent();
			if (Parent.ParentLines.Sum(l => l.SumOfUnitsMet) != Parent.WD_UnitsSent)
			{
				Parent.WD_UnitsSentInfo.AddWarning(Res.GetString("bdefec78-9e03-40be-a197-ae2f25b55785",
					"The quantity in the Units Sent field on the Release tab, does not match the sum of the Quantity Met fields on the Order Lines tab."));
			}
		}

		#endregion

		#region CheckWD_WL_CrossDock

		protected override void CheckWD_WL_CrossDock()
		{
			base.CheckWD_WL_CrossDock();

			if (Parent.WD_WL_CrossDock.IsEmpty)
			{
				if (IsAtleastOneOrderLineCrossDocked)
				{
					Parent.WD_WL_CrossDockInfo.AddWarning(Res.GetString("9f8331b8-5abe-42b0-9437-5da8d8df09e8", "No cross-dock location has been specified for the cross docked lines"));
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.WD_WL_CrossDockInfo);

				if (!Parent.WD_WL_CrossDockInfo.HasErrors() && (!Parent.CrossDockLocation?.IsDockDoorLocation ?? false))
				{
					Parent.WD_WL_CrossDockInfo.AddError(Res.GetString("WhsOrder|CheckWD_WL_CrossDock|NotDDLType", "The cross-dock location should have a Dock Door Location Type"));
				}

				if (!Parent.WD_WL_CrossDockInfo.HasErrors() && Parent.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations < 0m)
				{
					Parent.WD_WL_CrossDockInfo.AddWarning(Res.GetString("5df2e325-e062-4d84-90e3-bc2651c16dc8", "The cross-dock location maximum volume has been exceeded"));
				}
			}
		}

		bool IsAtleastOneOrderLineCrossDocked
		{
			get
			{
				bool result = false;

				if (Parent.WD_WP.IsEmpty)
				{
					result = Parent.Lines.Cast<WhsOrderLine>().Any(x => x.ReservedPickLines.Count > 0);
				}

				return result;
			}
		}

		#endregion

		#region CheckWD_DocketSubType

		protected override void CheckWD_DocketSubType()
		{
			base.CheckWD_DocketSubType();

			var parent = Parent;
			if (parent.Warehouse.IsFTZBondedEnabledAndUSJurisdiction())
			{
				if (!parent.IsCustomsTransaction)
				{
					parent.WD_DocketSubTypeInfo.AddError(Res.GetString("4ab694bc-2a3b-4556-9bb3-ba431de12fc8", "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'."));
				}
				else if (parent.IsInDatabase && parent.WD_DocketSubTypeInfo.HasChanges)
				{
					parent.WD_DocketSubTypeInfo.AddError(Res.GetString("dafad34c-7d70-4fac-b675-0cf8db6f63ef", "Cannot Change US FTZ Warehouse Order Type."));
				}
				else if (parent.IsImportingData && !parent.WD_DocketSubType.EqualsIgnoringCase(OrderType.Codes.Customs))
				{
					parent.WD_DocketSubTypeInfo.AddError(Res.GetString("2f17c786-1216-431c-b1bf-8b324f1b9e31", "Importing orders placed with a US FTZ Warehouse must have an Order Type 'CUS'."));
				}
				else if (!parent.IsImportingData && !parent.IsInDatabase && !parent.WD_DocketSubType.EqualsIgnoringCase(OrderType.Codes.CustomsReleaseWithPermit))
				{
					parent.WD_DocketSubTypeInfo.AddWarning(Res.GetString("f9ee7ee7-4420-46dd-a8b8-16edff40b8a4", "Manual orders placed with a US FTZ Warehouse with an Order Type 'CUS' will not use permits."));
				}
			}
			else if (parent.WD_DocketSubType.EqualsIgnoringCase(OrderType.Codes.CustomsReleaseWithPermit))
			{
				parent.WD_DocketSubTypeInfo.AddError(Res.GetString("10bd53f3-247a-4187-a45a-34e21387155e", "Only orders placed with a US FTZ Warehouse can have an Order Type 'CPS'."));
			}
		}

		#endregion

		#region CheckWD_INCO

		protected override void CheckWD_INCO()
		{
			if (!(Parent.IsFinalised && Parent.IsPickFinalised && !Parent.IsPostFinalizeEditAllowed))
			{
				base.CheckWD_INCO();
				ListValidation.ErrorIfInvalidCode(Parent.WD_INCOInfo);
				if (!Parent.WD_INCOInfo.HasErrors()
					&& Parent.WD_RequiredDate.Date >= IncoTerms2010StartDate
					&& !IsIncoTermValidUnderIncoTerms2010Rules(Parent.WD_INCO))
				{
					Parent.WD_INCOInfo.AddWarning(Res.GetString("38d55829-367c-4a03-96c2-12d463712857", "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules."));
				}
			}
		}

		public bool IsIncoTermValidUnderIncoTerms2010Rules(ZString incoTerm)
		{
			return incoTerm != "DAF" && incoTerm != "DES" && incoTerm != "DEQ" && incoTerm != "DDU";
		}

		ZDate IncoTerms2010StartDate
		{
			get { return new ZDate(2011, 01, 01); }
		}

		#endregion

		#region CheckWD_CODPayMethod

		protected override void CheckWD_CODPayMethod()
		{
			base.CheckWD_CODPayMethod();
			ListValidation.ErrorIfInvalidCode(Parent.WD_CODPayMethodInfo);
		}

		#endregion

		#region CheckWD_UseDirectedPackingConsolidation

		protected override void CheckWD_UseDirectedPackingConsolidation()
		{
			base.CheckWD_UseDirectedPackingConsolidation();

			var info = Parent.WD_UseDirectedPackingConsolidationInfo;
			if (!info.HasErrors() &&
				Parent.WD_UseDirectedPackingConsolidation &&
				!info.ReadOnly &&
				(!Parent.Warehouse?.HasPackingConsolidationLocations ?? false))
			{
				info.AddError(
					Res.GetString(
						"534942e1-b6c0-45cc-a970-4353c8db042d",
						"Use Directed Packing Consolidation should not be enabled as there are no Packing Consolidation Locations in the Order's Warehouse."));
			}
		}

		#endregion

		#region CheckConsigneeNameOrPK

		protected override void CheckConsigneeNameOrPK()
		{
			var order = Parent;
			if (order != null && !order.IsFinalisedOrCancelled && !order.IsAttachedToPickButNotFinalised)
			{
				base.CheckConsigneeNameOrPK();
			}
		}

		#endregion

		#region CheckWD_GS_NKAssignedPacker

		protected override void CheckWD_GS_NKAssignedPacker()
		{
			base.CheckWD_GS_NKAssignedPacker();

			if (!Parent.WD_GS_NKAssignedPacker.IsEmpty && Parent.WD_WP.IsEmpty)
			{
				Parent.WD_GS_NKAssignedPackerInfo.AddError(Res.GetString("c43699a0-f7bc-4d94-a7af-5a5f73415c39", "You cannot assign a packer if the order is not yet attached to a pick."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			Parent.ConsigneeDocAddress.Validation.ValidateAll();
		}

		#endregion

		#region Implementation

		protected override ZString TypeInMsg
		{
			get { return Res.GetString("1185bc3d-58c0-4dda-aec9-5822dff3349c", "Order"); }
		}

		bool WhsOrderPreDateAllowed
		{
			get
			{
				if (!fWhsOrderPreDateAllowedCached)
				{
					fWhsOrderPreDateAllowed = Env.Security.WhsOrderPreDate.IsAllowed;
					fWhsOrderPreDateAllowedCached = true;
				}
				return fWhsOrderPreDateAllowed;
			}
		}

		bool fWhsOrderPreDateAllowed;
		bool fWhsOrderPreDateAllowedCached;

		#endregion
	}
}
