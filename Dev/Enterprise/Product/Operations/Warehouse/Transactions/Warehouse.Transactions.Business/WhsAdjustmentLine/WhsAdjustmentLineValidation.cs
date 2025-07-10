using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLineValidation : WhsDocketLineValidation
	{
		public WhsAdjustmentLineValidation(AutoWhsDocketLine parent)
			: base(parent)
		{
		}

		#region CheckWE_AdjustmentArrivalDateIsValidZDateTimeRange

		protected override void CheckWE_AdjustmentArrivalDateIsValidZDateTimeOffsetRange()
		{
			if (!Parent.IsAdjustmentOut)
			{
				base.CheckWE_AdjustmentArrivalDateIsValidZDateTimeOffsetRange();
			}
		}

		#endregion

		#region CheckWE_ReasonCode

		protected override void CheckWE_ReasonCode()
		{
			base.CheckWE_ReasonCode();
			MandatoryValidation.CheckEntered(Parent.WE_ReasonCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WE_ReasonCodeInfo);
		}

		#endregion

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			CheckForZeroUnits();

			var parent = Parent;
			var info = parent.WE_TransactionQuantityInfo;
			if (Parent.IsAdjustmentOut && IsAllowedToValidateUnits) // Committing is only necessary for adjusting out stock
			{
				parent.CheckEnoughInventoryExistsToCommit(info);
			}

			base.CheckWE_TransactionQuantity();
			CheckQtyIsNegativeForNewOwnershipAdjustment();

			var docket = parent.Docket;
			if (!info.HasErrors() &&
				docket != null &&
				parent.IsAdjustmentIn &&
				(docket.IsFinalising || ((IBusinessObjectInternals)parent).IsInPreSaveValidation) &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value &&
				IsSerialNumberUsed(parent.Product, docket.Client) &&
				parent.WE_TransactionQuantity != parent.SerialNumbers.Count)
			{
				info.AddError(Res.GetString("7153F394-F753-4090-9A6D-AFF681F87BC5", "The number of entered serial numbers {0} does not match the required quantity {1}. Please ensure the serial numbers match the specified quantity.",
					parent.SerialNumbers.Count,
					parent.WE_TransactionQuantity
				));
			}
		}

		bool IsSerialNumberUsed(WhsProduct product, OrgHeader client) =>
			product != null &&
			client != null &&
			!product.IsSerialNumberReleaseCaptured(client) &&
			product.IsSerialNumberUsed(client);

		/// <summary>
		/// Validation should be allowed if we are not doing a dodgy bonded finalise or the adjustment line is being validated directly.
		/// We know if we are being directly validated by checking if the parent docket is in PreSaveValidation. Remove this check when
		/// Old Bonded Logic is removed.
		/// </summary>
		bool IsAllowedToValidateUnits
		{
			get
			{
				var docket = Parent.Docket;
				return docket == null || (!docket.IsAttemptingDodgyBondedFinalise || !((IBusinessObjectInternals)docket).IsInPreSaveValidation);
			}
		}

		void CheckForZeroUnits()
		{
			if (Parent.WE_TransactionQuantity == 0m)
			{
				Parent.WE_TransactionQuantityInfo.AddError(ErrorMsgCannotBeZero);
			}
		}

		void CheckQtyIsNegativeForNewOwnershipAdjustment()
		{
			if (!Parent.WE_TransactionQuantityInfo.HasErrors())
			{
				var docket = Parent.Docket;
				if (docket != null && docket.IsNewOwnershipAdjustmentParent && !Parent.IsAdjustmentOut)
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("8cf62228-1a1d-4aa3-ae06-d1e7c26ba564", "Units should be negative."));
				}
			}
		}

		#endregion

		#region CheckWE_WL

		protected override void CheckWE_WL()
		{
			base.CheckWE_WL();

			if (Parent.IsDocketFinalising)
			{
				// somehow there is a bug allowing a null location to slip thru
				// haven't been able to reproduce it, but code below is just a dbl check. CM
				MandatoryValidation.CheckEntered(Parent.WE_WLInfo);
			}

			if (!Parent.IsAdjustmentOut)
			{
				WhsValidationHelper.CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.WE_WLInfo, Parent.Location, Parent.WE_PalletID);
			}
		}

		#endregion

		#region CheckLocationString

		#region CheckLocationString

		protected override void CheckLocationString()
		{
			base.CheckLocationString();

			MandatoryValidation.CheckEntered(Parent.LocationStringInfo);

			var parent = Parent;
			if (!parent.LocationStringInfo.HasErrors())
			{
				var location = parent.Location;
				if (location != null)
				{
					ValidateLocationForAdjustmentIn(location);
				}
			}

			void ValidateLocationForAdjustmentIn(WhsLocation location)
			{
				if (parent.IsAdjustmentIn)
				{
					CheckLocationIsNotVoid(location, ErrorAdjustmentStockIntoVoidLocation);

					var adjustmentValidationStrategy = Factory.GetCachedValue(nameof(WhsAdjustmentValidationStrategy), () => new WhsAdjustmentValidationStrategy());
					adjustmentValidationStrategy.CheckProductAssignedToCorrectFixedOrDynamicLocation(parent);

					WhsValidationHelper.CheckLocationIsNotDockDoorLocation(Parent.LocationStringInfo, Parent, Res.GetString("9cd764c8-d0cc-43d7-bb28-afa4591a16e4", "You cannot adjust into Dock Door Locations."));
					WhsValidationHelper.CheckLocationIsNotPackingStationLocation(Parent.LocationStringInfo, location, Res.GetString("92f7e76f-6b55-40c5-b114-815d9b2d9431", "You cannot adjust into Packing Station Locations."));
					WhsValidationHelper.CheckLocationIsNotPackingConsolidationLocation(Parent.LocationStringInfo, location, Res.GetString("12664acb-4e6f-4379-9475-19a8cf28e3e2", "You cannot adjust into Packing Consolidation Locations."));

					CheckAdjustmentLocationAreaType();
					WhsValidationHelper.CheckLocationIsNotInAnInwardProcessingArea(parent.LocationStringInfo, location);
				}
			}
		}

		void CheckAdjustmentLocationAreaType()
		{
			if (!Parent.LocationStringInfo.HasErrors())
			{
				var parentAdjustment = Parent.Docket;
				var locationAreaType = Parent.LocationAreaType;

				if (parentAdjustment.IsCustomsTransaction && !locationAreaType.EqualsIgnoringCase(AreaTypes.Codes.Bonded))
				{
					Parent.LocationStringInfo.AddError(Res.GetString("26458A8E-A0CB-44D4-AA95-B0A2A2FE6B81", "A Bonded Adjustment can only adjust into locations that has a Bonded Pick Area."));
				}
				else if (!parentAdjustment.IsCustomsTransaction && locationAreaType.EqualsIgnoringCase(AreaTypes.Codes.Bonded))
				{
					Parent.LocationStringInfo.AddError(Res.GetString("64B15294-3C38-4C17-8B98-B746FB260AA8", "A Non-Bonded Adjustment can only adjust into locations that does not have a Bonded Pick Area."));
				}
			}
		}

		#endregion

		#region CheckLocationIsNotVoid

		public static string ErrorAdjustmentStockIntoVoidLocation
		{
			get { return Res.GetString("6a587a25-0c47-4059-a25a-7b87e4d3e0b6", "You cannot Adjust stock into Void Location."); }
		}

		#endregion

		#endregion

		#region CheckWE_PalletID

		protected override void CheckWE_PalletID()
		{
			base.CheckWE_PalletID();

			var parent = Parent;
			CheckPalletIdIsEmptyIfLocationIsPickFace();

			if (parent.WE_WL.IsValid && !parent.WE_PalletID.IsEmpty && parent.IsAdjustmentIn &&
				(parent.Inventory.Count == 0 || parent.Inventory[0].Docket == null || !parent.Inventory[0].Docket.IsFinalised || !parent.Inventory[0].IsInDatabase)) // inventory might have been transferred after finalisation, in which case don't run this check. When Adjusting In only one Inventory Line will be created for every AdjustmentLine, so check only first one.
			{
				CheckPalletIDNotUsedInAnotherLocationOnSiblingAdjustmentInLines(parent);

				if (IsPalletInTransit(parent.WE_PalletID))
				{
					parent.WE_PalletIDInfo.AddError(Res.GetString("f261b6fa-b432-4471-8a2d-d2219bb6b6ce", "This pallet is currently In-Transit and cannot be adjusted. Please select another Pallet ID."));
				}
				else
				{
					CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(parent);
				}
			}

			if (!Parent.IsAdjustmentOut)
			{
				WhsValidationHelper.CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.WE_PalletIDInfo, Parent.Location, Parent.WE_PalletID);
			}
		}

		#region CheckPalledIdIsEmptyIfLocationIsPickFace

		void CheckPalletIdIsEmptyIfLocationIsPickFace()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors()
					&& Parent.IsAdjustmentIn
					&& !Parent.WE_WL.IsEmpty
					&& !Parent.WE_PalletID.IsEmpty
					&& Parent.Product != null)
			{
				var pickFace = Parent.Product.PickFaces.FindByLocation(Parent.Docket.WD_OH_Client, Parent.WE_WL);
				if (pickFace != null && !pickFace.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces)
				{
					Parent.WE_PalletIDInfo.AddError(Res.GetString("66c4e660-178d-4d42-8d6e-a0e605de92ac", "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs."));
				}
			}
		}

		#endregion

		#region CheckIfPalletIDExistsInAnotherLocationInThisWarehouse

		void CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(WhsAdjustmentLine parent)
		{
			var anotherLocation = parent.Docket.CheckIfPalletIDExistsInAnotherLocationInThisWarehouse(parent);
			if (!string.IsNullOrEmpty(anotherLocation))
			{
				parent.WE_PalletIDInfo.AddError(WhsValidationHelper.GetDuplicatePalletIdMessage(anotherLocation));
			}
		}

		#endregion

		#region CheckPalletIDNotUsedInAnotherLocationOnSiblingAdjustmentInLines

		static void CheckPalletIDNotUsedInAnotherLocationOnSiblingAdjustmentInLines(WhsAdjustmentLine parent)
		{
			if (parent.Docket.Lines.Cast<WhsAdjustmentLine>().Any(line => line.PK != parent.PK
				&& line.WE_PalletID == parent.WE_PalletID
				&& line.WE_WL.IsValid
				&& line.WE_WL != parent.WE_WL
				&& parent.IsAdjustmentIn
				&& line.IsAdjustmentIn))
			{
				parent.WE_PalletIDInfo.AddError(Res.GetString("83ff36e1-5d05-4488-b673-c4c2707b9d0a", "Another location was already used for the same Pallet ID on this Adjustment."));
			}
		}

		#endregion

		#region IsPalletInTransit

		bool IsPalletInTransit(ZString palletID)
		{
			return Parent.Docket.IsPalletInTransit(palletID);
		}

		#endregion

		#endregion

		#region CheckWE_OriginalInventoryStatus

		protected override void CheckWE_OriginalInventoryStatus()
		{
			base.CheckWE_OriginalInventoryStatus();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.WE_OriginalInventoryStatusInfo);
			ListValidation.ErrorIfInvalidCode(parent.WE_OriginalInventoryStatusInfo);
		}

		#endregion

		#region CheckWE_CurrentInventoryStatus

		protected override void CheckWE_CurrentInventoryStatus()
		{
			base.CheckWE_CurrentInventoryStatus();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.WE_CurrentInventoryStatusInfo);
			ListValidation.ErrorIfInvalidCode(parent.WE_CurrentInventoryStatusInfo);
		}

		#endregion

		#region CheckWE_CurrentHoldReason

		protected override void CheckWE_CurrentHoldReason()
		{
			base.CheckWE_CurrentHoldReason();

			if (Parent.WE_TransactionQuantity > 0 && !Parent.WE_CurrentHoldReason.IsEmpty && Parent.WE_WHC_NKCurrentInventoryHeldCode.IsEmpty)
			{
				Parent.WE_CurrentHoldReasonInfo.AddError(ErrorForHoldReasonWithoutHoldCode);
			}
		}

		#endregion

		#region CheckWE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();
			var line = Parent;
			var adjustment = line.Docket;
			var supplierPart = line.SupplierPart;

			if (adjustment != null && adjustment.IsNewOwnershipAdjustmentParent && supplierPart != null)
			{
				var oldClient = adjustment.Client;
				var newClient = adjustment.OwnershipAdjustedClient;

				CheckClientProductRelationshipType(newClient, supplierPart);
				if (newClient != null && oldClient != null)
				{
					var oldClientProductRelationship = supplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(oldClient.PK, OrgPartRelation.RelationshipTypes.Owner);
					var newClientProductRelationship = supplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner);
					if (newClientProductRelationship != null && oldClientProductRelationship != null)
					{
						CheckNewClientProductOwnership(line, oldClient, newClient, oldClientProductRelationship, newClientProductRelationship);
						CheckJulienBatchNumberFormat(line, supplierPart, oldClient, newClient, oldClientProductRelationship, newClientProductRelationship);
						CheckPackingDateFormat(line, oldClient, newClient, oldClientProductRelationship, newClientProductRelationship);
						CheckExpiryDateFormat(line, oldClient, newClient, oldClientProductRelationship, newClientProductRelationship);
					}
					CheckClientProductRelationshipCanBeCreatedOnNewClient(line, oldClient, newClient, oldClientProductRelationship, newClientProductRelationship);
					CheckMaximumShelfLife(adjustment, line, supplierPart, oldClient, newClient);
				}
			}

			if (supplierPart != null
				&& !Parent.IsAdjustmentOut)
			{
				WhsValidationHelper.CheckProductWithoutPalletConversionInLocationUsingPalletSpaces(Parent.WE_OPInfo, Parent.Location, supplierPart.OP_StockKeepingUnitPerPallet);
			}
		}

		protected override bool AllowInactiveProducts()
		{
			return Parent.IsAdjustmentOut;
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return (!Parent.IsAdjustmentOut || info.Name != WhsDocketLineSchema.Constants.WE_OP) && base.ShouldValidateFKToCancelledRecord(info);
		}

		#region CheckClientProductRelationshipType

		void CheckClientProductRelationshipType(OrgHeader newClient, OrgSupplierPart supplierPart)
		{
			if (!Parent.WE_OPInfo.HasErrors())
			{
				var relatedOrganisations = supplierPart.RelatedOrganisations;
				if (newClient != null
					&& (relatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Both) == null)
					&& (relatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Owner) == null)
					&& relatedOrganisations.FindByOrganisationPKAndRelationship(newClient.PK, OrgPartRelation.RelationshipTypes.Supplier) != null)
				{
					Parent.WE_OPInfo.AddError(ResString.GetMultilingualString("ecc1d8cd-5de5-486b-a813-79327AD726C4",
						"Ownership adjusted client '{0}' has a supplier product relationship for the product '{1}'. Please change it to 'OWN' or 'BOTH'",
						newClient.OH_Code, supplierPart.OP_PartNum));
				}
			}
		}

		#endregion

		#region CheckNewClientProductOwnership

		void CheckNewClientProductOwnership(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			if (!line.WE_OPInfo.HasErrors())
			{
				if (!DoesNotHaveMatchingTypesOrHasMatchingAttributeNamesForMatchingType(oldClient, newClient, oldClientProductRelationship, newClientProductRelationship))
				{
					line.WE_OPInfo.AddError(ResString.GetMultilingualString("E766AAA6-06EC-4196-88E6-216A52F6071D", "Duplicated attribute types found on old client '{0}' and new client '{1}' without matching names in product master file.", oldClient.OH_Code, newClient.OH_Code));
				}
				else if (NotMatchingProductClientDefinition(oldClient, newClient, oldClientProductRelationship, newClientProductRelationship))
				{
					line.WE_OPInfo.AddError(ResString.GetMultilingualString("172d493e-50f0-4102-a2d7-da3badada9c8", "Attributes specified for old client '{0}' doesn't match with attributes specified for new client '{1}' in product master file.",
						oldClient.OH_Code, newClient.OH_Code));
				}
			}
		}

		#region DoesNotHaveMatchingTypesOrHasMatchingAttributeNamesForMatchingType

		bool DoesNotHaveMatchingTypesOrHasMatchingAttributeNamesForMatchingType(OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			var duplicatedTypesHaveMatchingNamesOrDoesNotHaveMatchingTypes = true;

			var newClientAttribute1Type = newClient.MiscServ.OM_IMPartAttrib1Type;
			var newClientAttribute2Type = newClient.MiscServ.OM_IMPartAttrib2Type;
			var newClientAttribute3Type = newClient.MiscServ.OM_IMPartAttrib3Type;

			var duplicateType = ZString.Empty;
			duplicateType = (newClientProductRelationship.OU_UsePartAttrib1 && newClientProductRelationship.OU_UsePartAttrib2 && !newClientAttribute1Type.IsEmpty && newClientAttribute1Type == newClientAttribute2Type) ? newClientAttribute1Type : duplicateType;
			duplicateType = (duplicateType.IsEmpty && newClientProductRelationship.OU_UsePartAttrib2 && newClientProductRelationship.OU_UsePartAttrib3 && !newClientAttribute2Type.IsEmpty && newClientAttribute2Type == newClientAttribute3Type) ? newClientAttribute2Type : duplicateType;
			duplicateType = (duplicateType.IsEmpty && newClientProductRelationship.OU_UsePartAttrib3 && newClientProductRelationship.OU_UsePartAttrib1 && !newClientAttribute3Type.IsEmpty && newClientAttribute3Type == newClientAttribute1Type) ? newClientAttribute1Type : duplicateType;

			if (!duplicateType.IsEmpty)
			{
				var newClientNames = new HashSet<ZString>();
				AddToPartAttributeNameSetCaseInsensitive(newClient.MiscServ, newClientProductRelationship, duplicateType, newClientNames);

				var oldClientNames = new HashSet<ZString>();
				AddToPartAttributeNameSetCaseInsensitive(oldClient.MiscServ, oldClientProductRelationship, duplicateType, oldClientNames);

				duplicatedTypesHaveMatchingNamesOrDoesNotHaveMatchingTypes = newClientNames.SetEquals(oldClientNames);
			}

			return duplicatedTypesHaveMatchingNamesOrDoesNotHaveMatchingTypes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		void AddToPartAttributeNameSetCaseInsensitive(OrgMiscServ clientMiscServ, OrgPartRelation clientProductRelationship, ZString duplicateType, HashSet<ZString> partAttributeNames)
		{
			if (clientProductRelationship.OU_UsePartAttrib1 && clientMiscServ.OM_IMPartAttrib1Type == duplicateType)
			{
				partAttributeNames.Add(clientMiscServ.OM_IMPartAttrib1Name.ToLower());
			}

			if (clientProductRelationship.OU_UsePartAttrib2 && clientMiscServ.OM_IMPartAttrib2Type == duplicateType)
			{
				partAttributeNames.Add(clientMiscServ.OM_IMPartAttrib2Name.ToLower());
			}

			if (clientProductRelationship.OU_UsePartAttrib3 && clientMiscServ.OM_IMPartAttrib3Type == duplicateType)
			{
				partAttributeNames.Add(clientMiscServ.OM_IMPartAttrib3Name.ToLower());
			}
		}

		#endregion

		#region NotMatchingProductClientDefinition

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		bool NotMatchingProductClientDefinition(OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			var attributeNumbers = new Dictionary<PartAttributeNumber, PartAttributeNumber>();

			return (oldClientProductRelationship.OU_UseExpiryDate != newClientProductRelationship.OU_UseExpiryDate ||
				oldClientProductRelationship.OU_UsePackingDate != newClientProductRelationship.OU_UsePackingDate ||
				oldClientProductRelationship.OU_UseSerialNumber != newClientProductRelationship.OU_UseSerialNumber ||
				CheckNotMatchingAttributeUsage(oldClientProductRelationship.OU_UsePartAttrib1, oldClient.MiscServ.OM_IMPartAttrib1Type, oldClient.MiscServ.OM_IMPartAttrib1Name, newClient, newClientProductRelationship, PartAttributeNumber.One, attributeNumbers) ||
				CheckNotMatchingAttributeUsage(oldClientProductRelationship.OU_UsePartAttrib2, oldClient.MiscServ.OM_IMPartAttrib2Type, oldClient.MiscServ.OM_IMPartAttrib2Name, newClient, newClientProductRelationship, PartAttributeNumber.Two, attributeNumbers) ||
				CheckNotMatchingAttributeUsage(oldClientProductRelationship.OU_UsePartAttrib3, oldClient.MiscServ.OM_IMPartAttrib3Type, oldClient.MiscServ.OM_IMPartAttrib3Name, newClient, newClientProductRelationship, PartAttributeNumber.Three, attributeNumbers) ||
				CheckAllOldClientAttributesMatchWithNewClientAttributes(newClientProductRelationship, attributeNumbers));
		}

		bool CheckNotMatchingAttributeUsage(bool oldClientProductAttribute, ZString attributeType, ZString attributeName, OrgHeader newClient, OrgPartRelation newRelation, PartAttributeNumber oldClientAttributeNumber,
			Dictionary<PartAttributeNumber, PartAttributeNumber> previouslyMatchAttrbiuteNumber)
		{
			var result = oldClientProductAttribute;

			var number = PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(attributeType, attributeName, newClient.MiscServ, oldClientAttributeNumber, previouslyMatchAttrbiuteNumber);
			if (number == PartAttributeNumber.One)
			{
				result = newRelation.OU_UsePartAttrib1 != oldClientProductAttribute;
			}
			else if (number == PartAttributeNumber.Two)
			{
				result = newRelation.OU_UsePartAttrib2 != oldClientProductAttribute;
			}
			else if (number == PartAttributeNumber.Three)
			{
				result = newRelation.OU_UsePartAttrib3 != oldClientProductAttribute;
			}
			return result;
		}

		bool CheckAllOldClientAttributesMatchWithNewClientAttributes(OrgPartRelation newClientProductRelationship, Dictionary<PartAttributeNumber, PartAttributeNumber> attributeNumbers)
		{
			return (newClientProductRelationship.OU_UsePartAttrib1 && !attributeNumbers.ContainsValue(PartAttributeNumber.One)) ||
				(newClientProductRelationship.OU_UsePartAttrib2 && !attributeNumbers.ContainsValue(PartAttributeNumber.Two)) ||
				(newClientProductRelationship.OU_UsePartAttrib3 && !attributeNumbers.ContainsValue(PartAttributeNumber.Three));
		}

		#endregion

		#endregion

		#region CheckJulienBatchNumberFormat

		void CheckJulienBatchNumberFormat(WhsAdjustmentLine line, OrgSupplierPart supplierPart, OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			if (!line.WE_OPInfo.HasErrors() && oldClient.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(supplierPart) &&
				oldClientProductRelationship.OU_JulianBatchNoFormat != newClientProductRelationship.OU_JulianBatchNoFormat)
			{
				line.WE_OPInfo.AddError(ResString.GetMultilingualString("252b9be2-6995-4dfb-a026-59131d3ffdaf", "Julian Batch number format in product master file for old client '{0}' doesn't match with Julian Batch number format specified for new client '{1}' in product master file.",
							oldClient.OH_Code, newClient.OH_Code));
			}
		}

		#endregion

		#region CheckPackingDateFormat

		void CheckPackingDateFormat(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			if (!line.WE_OPInfo.HasErrors() && oldClientProductRelationship.OU_UsePackingDate && newClientProductRelationship.OU_UsePackingDate &&
				oldClientProductRelationship.OU_PackingDateFormatString != newClientProductRelationship.OU_PackingDateFormatString)
			{
				line.WE_OPInfo.AddError(ResString.GetMultilingualString("457228fa-c9eb-4942-9e9d-3db6c039fddb", "RF Packing Date format in product master file for old client '{0}' doesn't match with RF Packing Date format specified for new client '{1}' in product master file.",
							oldClient.OH_Code, newClient.OH_Code));
			}
		}

		#endregion

		#region CheckExpiryDateFormat

		void CheckExpiryDateFormat(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			if (!line.WE_OPInfo.HasErrors() && oldClientProductRelationship.OU_UseExpiryDate && newClientProductRelationship.OU_UseExpiryDate &&
				oldClientProductRelationship.OU_ExpiryDateFormatString != newClientProductRelationship.OU_ExpiryDateFormatString)
			{
				line.WE_OPInfo.AddError(ResString.GetMultilingualString("9a838fe6-78e8-44f1-869d-06a0bcb46e24", "RF Expiry Date format in product master file for old client '{0}' doesn't match with RF Expiry Date format specified for new client '{1}' in product master file.",
							oldClient.OH_Code, newClient.OH_Code));
			}
		}

		#endregion

		#region CheckClientProductRelationshipCanBeCreatedOnNewClient

		void CheckClientProductRelationshipCanBeCreatedOnNewClient(WhsAdjustmentLine line, OrgHeader oldClient, OrgHeader newClient, OrgPartRelation oldClientProductRelationship, OrgPartRelation newClientProductRelationship)
		{
			if (!line.WE_OPInfo.HasErrors() && oldClientProductRelationship != null && newClientProductRelationship == null &&
				CheckOldClientProductRelationshipNotCompatibleWithNewClient(oldClientProductRelationship, oldClient.MiscServ, newClient.MiscServ))
			{
				line.WE_OPInfo.AddError(ResString.GetMultilingualString("6090c0d2-737e-4591-9534-8a80d4e72466", "Attributes specified on client level for old client '{0}' cannot be specified for new client '{1}'.",
							oldClient.OH_Code, newClient.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		bool CheckOldClientProductRelationshipNotCompatibleWithNewClient(OrgPartRelation oldClientProductRelation, OrgMiscServ oldClientMiscServ, OrgMiscServ newClientMiscServ)
		{
			var attributeNumbers = new List<PartAttributeNumber>();
			return (oldClientProductRelation.OU_UsePartAttrib1 && PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientMiscServ.OM_IMPartAttrib1Type, oldClientMiscServ.OM_IMPartAttrib1Name, newClientMiscServ, attributeNumbers) == PartAttributeNumber.None) ||
				(oldClientProductRelation.OU_UsePartAttrib2 && PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientMiscServ.OM_IMPartAttrib2Type, oldClientMiscServ.OM_IMPartAttrib2Name, newClientMiscServ, attributeNumbers) == PartAttributeNumber.None) ||
				(oldClientProductRelation.OU_UsePartAttrib3 && PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(oldClientMiscServ.OM_IMPartAttrib3Type, oldClientMiscServ.OM_IMPartAttrib3Name, newClientMiscServ, attributeNumbers) == PartAttributeNumber.None) ||
				(oldClientProductRelation.OU_UsePackingDate && !newClientMiscServ.OM_IMUsePackingDate) ||
				(oldClientProductRelation.OU_UseExpiryDate && !newClientMiscServ.OM_IMUseExpiryDate) ||
				(oldClientProductRelation.OU_UseSerialNumber && !newClientMiscServ.OM_IMUseSerialNumber);
		}

		#endregion

		#region CheckMaximumShelfLife

		void CheckMaximumShelfLife(WhsAdjustment adjustment, WhsAdjustmentLine line, OrgSupplierPart supplierPart, OrgHeader oldClient, OrgHeader newClient)
		{
			if (!line.WE_OPInfo.HasErrors())
			{
				var product = WhsProduct.GetWhsProduct(supplierPart);
				var oldClientProductParams = product.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(oldClient.OH_Code, adjustment.WD_WW_Whs);
				var newClientProductParams = product.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(newClient.OH_Code, adjustment.WD_WW_Whs);
				if (oldClientProductParams != null && newClientProductParams != null && oldClient.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(supplierPart) &&
					oldClientProductParams.W3_MaximumShelfLife != newClientProductParams.W3_MaximumShelfLife)
				{
					line.WE_OPInfo.AddError(ResString.GetMultilingualString("46b61d96-8b2a-439f-ae6e-3293b45990ab", "Maximum Shelf Life defined for Product: {0}, Client: {1}, Warehouse: {2} does not match with Maximum Shelf Life defined for Product: {0}, Client: {3}, Warehouse: {2}.",
								supplierPart.OP_Desc, oldClient.OH_Code, adjustment.Warehouse.WW_WarehouseCode, newClient.OH_Code));
				}
			}
		}

		#endregion

		#endregion

		#region CheckWE_BondedEntryKey

		protected override void CheckWE_BondedEntryKey()
		{
			base.CheckWE_BondedEntryKey();

			if (!Parent.WE_BondedEntryKeyInfo.HasErrors())
			{
				var docket = Parent.Docket;
				if (docket != null && !docket.IsCustomsTransaction && !Parent.WE_BondedEntryKey.IsEmpty)
				{
					Parent.WE_BondedEntryKeyInfo.AddError(Res.GetString("F9CD64D7-A9FE-4E02-B59E-CC916FBA90ED", "Bonded entry key should not be entered for Non-Bonded adjustments."));
				}
			}
		}

		#endregion

		#region IsAttributeValidationRequired

		public override bool IsAttributeValidationRequired
		{
			get
			{
				var parent = Parent;
				return parent.SupplierPart != null && parent.Docket?.Client != null;
			}
		}

		#endregion

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
		{
			return !Parent.IsAdjustmentOut || !partAttributeInfo.Value.IsEmpty;
		}

		#endregion

		#region PartAttributeValidation

		protected override PartAttributeValidation PartAttributeValidation
		{
			get
			{
				var parent = Parent;
				return (parent.IsAdjustmentOut)
					? new WhsAdjustmentLinePartAttributeValidationAdjustmentOut(parent)
					: base.PartAttributeValidation;
			}
		}

		protected override void CheckWE_SerialNumber()
		{
			if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				base.CheckWE_SerialNumber();
			}
		}

		#endregion

		#region Implementation

		protected new WhsAdjustmentLine Parent
		{
			get { return (WhsAdjustmentLine)base.Parent; }
		}

		protected WhsAdjustment Adjustment
		{
			get { return Parent.Docket; }
		}

		public static string ErrorMsgMustBeZero
		{
			get { return Res.GetString("53966d71-5783-4730-b356-cd2c41a7c54d", "Must be zero"); }
		}
		public static string ErrorMsgCannotBeZero
		{
			get { return Res.GetString("b743719b-63b4-40bd-9e05-247dda6ab475", "Can be negative or positive, but not zero."); }
		}
		public static string ErrorMsgCannotBeGreaterThanUnits
		{
			get { return Res.GetString("5b0b9bc9-794f-4394-9a9c-b901f3384919", "Cannot be greater than units"); }
		}
		public static string ErrorMsgEntryKeyDoesNotExist
		{
			get { return Res.GetString("e9fa122e-e4cb-4b8c-9dab-b2e5ad92fe7c", "This Entry Key does not exist in the system. You can only adjust an existing Entry Key."); }
		}

		#endregion
	}
}
