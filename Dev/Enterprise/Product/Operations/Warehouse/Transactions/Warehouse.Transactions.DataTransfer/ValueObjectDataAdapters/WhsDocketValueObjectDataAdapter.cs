using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business.ProductMatching;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer.ValueObjectDataAdapters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsDocketValueObjectDataAdapter<TDocket> : WhsValueObjectDataAdapter<TDocket, Xsd.WhsDocket>
		where TDocket : WhsDocket
	{
		protected WhsDocketValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		protected WhsDocketValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#region Overrides

		#region RootCollectionElementName

		public override string RootCollectionElementName => "WhsDockets";

		#endregion

		#region RootElementName

		public override string RootElementName => "WhsDocket";

		#endregion

		#region Schema

		public override System.Xml.Schema.XmlSchema Schema => WarehouseXmlSchemaDefinitions.Instance.SingleWhsDocketSchema;

		#endregion

		#region CollectionSchema

		public override System.Xml.Schema.XmlSchema CollectionSchema => WarehouseXmlSchemaDefinitions.Instance.WhsDocketsSchema;

		#endregion

		#endregion

		#region FindExistingBusinessObject

		public TDocket FindExistingBusinessObject(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			return FindBusinessObject(value, context);
		}

		#endregion

		#region FindBusinessObject

		protected override TDocket FindBusinessObject(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			TDocket whsDocket = null;

			var xsdDocketTypeCode = value.Identifier?.DocketType ?? ZString.Empty;
			if (IsImportingDocketTypeCorrect(xsdDocketTypeCode, AdapterDocketTypeCode))
			{
				var clientPK = context.FindOrganisationPK(value.Identifier.Client, whsDocket, OrganisationTypes.WarehouseClient);
				var query = new ZQuery(WhsDocketSchema.WD_OH_Client, clientPK);
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, value.Identifier.Reference);
				query.AddToFilter(WhsDocketSchema.WD_DocketType, AdapterDocketTypeCode);

				whsDocket = context.Factory.LoadTop1<TDocket>(query);
			}

			return whsDocket;
		}

		#region AdapterDocketTypeCode

		string AdapterDocketTypeCode => adapterDocketTypeCode ?? (adapterDocketTypeCode = new WhsDocketTypeDecider().GetDocketTypeCodeFromType(typeof(TDocket)));
		string adapterDocketTypeCode;

		#endregion

		#region IsImportingDocketTypeCorrect

		bool IsImportingDocketTypeCorrect(ZString xsdDocketTypeCode, ZString docketTypeCode)
		{
			return xsdDocketTypeCode.IsEmpty
					|| xsdDocketTypeCode == docketTypeCode
					|| xsdDocketTypeCode == GetMappedDataTransferDocketType(docketTypeCode);
		}

		#region GetMappedDataTransferDocketType

		ZString GetMappedDataTransferDocketType(ZString warehouseDocketType)
		{
			switch (warehouseDocketType)
			{
				case DocketType.Codes.Order:
					return Warehouse.DataTransfer.CodeLists.DocketTypes.Codes.WhsOrder;
				case DocketType.Codes.Adjustment:
					return Warehouse.DataTransfer.CodeLists.DocketTypes.Codes.WhsAdjustment;
				case DocketType.Codes.Receive:
					return Warehouse.DataTransfer.CodeLists.DocketTypes.Codes.WhsASN;
				default:
					return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Import

		#region ShouldCreateOrUpdateBusinessObject

		protected override bool ShouldCreateOrUpdateBusinessObject(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			bool result = base.ShouldCreateOrUpdateBusinessObject(value, context);

			if (result)
			{
				var xsdDocketTypeCode = value.Identifier?.DocketType ?? ZString.Empty;
				if (!IsImportingDocketTypeCorrect(xsdDocketTypeCode, AdapterDocketTypeCode))
				{
					var errorMessage = Res.GetString("84c380db-9b08-42b4-b083-aa5dd6ea997c", "Docket Type '{0}' is not valid for this import.", xsdDocketTypeCode);
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, errorMessage));
					result = false;
				}
			}

			return result;
		}

		#endregion

		#region Cross Dock

		#region ImportLineCrossDockLineLinks

		protected void ImportLineCrossDockLineLinks(TDocket bizObj, WhsDocketLine importLine, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			ExamineAndUpdateIfNeededExistingCrossDockLinks(importLine);
			foreach (Xsd.WhsDocketLineCrossDockLine crossDockLine in xsdLine.CrossDockLines)
			{
				var existingLine = FindExistingDocketLine(crossDockLine.Reference, bizObj.Client.PK, bizObj.Warehouse.PK, importLine.SupplierPart.PK, crossDockLine.LineNumber, crossDockLine.SubLineNumber, context);
				if (existingLine != null)
				{
					ImportLineAddOrUpdateCrossDockLineLink(importLine, existingLine, crossDockLine.AllocationQty, crossDockLine.AllocationQtyUQ);
				}
			}
			ClearUpUnusedCrossDockLinks(importLine);
		}

		protected virtual void ClearUpUnusedCrossDockLinks(WhsDocketLine importLine)
		{
		}

		#endregion

		#region ExamineAndUpdateIfNeededExistingCrossDockLinks

		protected virtual void ExamineAndUpdateIfNeededExistingCrossDockLinks(WhsDocketLine importLine)
		{
		}

		#endregion

		#region GetCrossDockLinkOrderLine

		protected virtual WhsOrderLine GetCrossDockLinkOrderLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			return null;
		}

		#endregion

		#region GetGrossDockLinkInventoryLine

		protected virtual WhsInventoryView GetGrossDockLinkInventoryLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			return null;
		}

		#endregion

		#region ImportLineAddOrUpdateCrossDockLineLink

		protected void ImportLineAddOrUpdateCrossDockLineLink(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine, ZDecimal allocationQty, ZString allocationQtyUQ)
		{
			if (allocationQty > 0m)
			{
				var reservedPickLine = GetReservedPickLine(importLine, importLineCrossDockLine);
				if (reservedPickLine != null)
				{
					var part = importLine.SupplierPart;
					reservedPickLine.ReservedQuantity = part.UnitConverter.Convert(allocationQty, allocationQtyUQ, part.OP_StockKeepingUnit);
				}
			}
		}

		#endregion

		#region GetReservedPickLine

		protected WhsPickLine GetReservedPickLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			WhsPickLine pickLine = null;
			var crossDockOrderLine = GetCrossDockLinkOrderLine(importLine, importLineCrossDockLine);
			var crossDockInventoryLine = GetGrossDockLinkInventoryLine(importLine, importLineCrossDockLine);

			foreach (var existingPickLine in crossDockOrderLine.ReservedPickLines)
			{
				if (existingPickLine.WZ_WE_InventoryLine == crossDockInventoryLine.WI_WE_InDocketLine)
				{
					pickLine = existingPickLine;
					break;
				}
			}

			if (pickLine == null)
			{
				pickLine = crossDockOrderLine.ReserveStockIfAbleTo(crossDockInventoryLine);
			}

			return pickLine;
		}

		#endregion

		#region FindExistingDocketLine

		protected virtual WhsDocketLine FindExistingDocketLine(ZString docketRef, ZGuid clientPK, ZGuid warehousePK, ZGuid partPK, ZShort lineNo, ZShort subLineNo, IValueObjectImportContext context)
		{
			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.Equal, docketRef);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_OH_Client, SQLComparisonOperator.Equal, clientPK);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehousePK);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketType, SQLComparisonOperator.Equal, GetExpectedCrossDockDocketType());
			var docket = context.Factory.LoadTop1<WhsDocket>(query);
			WhsDocketLine result = null;
			if (docket != null)
			{
				foreach (var line in docket.Lines)
				{
					if (line.WE_LineNo == lineNo && line.WE_SubLineNo == subLineNo && line.SupplierPart.PK == partPK)
					{
						result = line;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region GetExpectedCrossDockDocketType

		protected virtual ZString GetExpectedCrossDockDocketType()
		{
			return "";
		}

		#endregion

		#endregion

		#region ImportFromValueObject

		public void ImportFromValueObject(WhsDocketCollection whsDockets, Xsd.WhsDockets value, ValueObjectImportContext context)
		{
			foreach (Xsd.WhsDocket xsdValue in value.WhsDocket)
			{
				var docket = CreateOrUpdateFromValueObject(xsdValue, context);
				if (docket != null && !whsDockets.Contains(docket))
				{
					whsDockets.Add(docket);
				}
			}
		}

		#endregion

		#region ConfirmUpdateOfExistingBusinessObject

		protected override bool ConfirmUpdateOfExistingBusinessObject(TDocket bizObj, INotifications notifications)
		{
			var queryArgs = new QueryUserYesNoYesAllNoAllEventArgs();
			queryArgs.Message = Res.GetString("ce9e1948-6a59-44b3-8613-331b2149b687", "Found {0}: {1} of {2}. Is it OK to update it?", GetBizObjHumanReadableName(bizObj), bizObj.WD_ExternalReference, (bizObj.Client == null ? ZString.Empty : bizObj.Client.OH_Code));
			notifications.QueryUser(queryArgs);
			return queryArgs.Response;
		}

		#endregion

		#region DocketDataFormatter

		public WhsDocketDataFormatter DocketDataFormatter => docketDataFormatter ?? (docketDataFormatter = GetNewDocketDataFormatter());
		WhsDocketDataFormatter docketDataFormatter;

		protected virtual WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsDocketDataFormatter();
		}

		#endregion

		#region Error Handler

		public IErrorHandler DocketErrorHandler => docketErrorHandler ?? (docketErrorHandler = GetNewDocketErrorHandler());
		IErrorHandler docketErrorHandler;

		protected abstract IErrorHandler GetNewDocketErrorHandler();

		#endregion

		#region GetBizObjHumanReadableName

		protected virtual ZString GetBizObjHumanReadableName(TDocket bizObj)
		{
			return bizObj.HumanReadableName;
		}

		#endregion

		#region AfterImportFromValueObject

		protected override void AfterImportFromValueObject(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			base.AfterImportFromValueObject(bizObj, value, context);
			if (AllowDocketModifications(bizObj))
			{
				DocketErrorHandler.SetDocketTypeAndLogDataErrors(bizObj, value);
				if (value.DocketLines.Count == 0)
				{
					if (bizObj.WD_DocketStatus == DocketStatus.Codes.New || bizObj.WD_DocketStatus == DocketStatus.Codes.Error)
					{
						bizObj.WD_DocketStatus = DocketStatus.Codes.Entered;
					}
					bizObj.CancelReactivateDocket();
				}
			}
		}

		#endregion

		#region CheckDocketForErrors

		void CheckDocketForErrors(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			if (!AllowDocketModifications(bizObj))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, GetBizObjHumanReadableName(bizObj) + Res.GetString("743c9d64-cef1-4abf-889b-a2cab737e826", ": {0} with Status {1} can not be modified.", xsdWhsDocket.Identifier.Reference, bizObj.WD_DocketStatusDescription)));
			}

			if (!xsdWhsDocket.Identifier.Client.IsSpecified)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("15de4905-244c-4853-b824-33014c0656b4", "Client not specified.") + " " + GetBizObjHumanReadableName(bizObj) + ": " + xsdWhsDocket.Identifier.Reference));
			}

			if (!CanMatchWarehouse(bizObj, xsdWhsDocket, context))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("834c4990-bcdb-400c-92be-fec27b9a8da6", "Invalid Warehouse Code: {0} on", xsdWhsDocket.DocketDetail.WarehouseCode) + " " + GetBizObjHumanReadableName(bizObj) + ":" + xsdWhsDocket.Identifier.Reference));
			}
		}

		#endregion

		#region AllowDocketModifications

		protected virtual bool AllowDocketModifications(TDocket bizObj)
		{
			var result = false;
			if (bizObj.WD_DocketStatus == DocketStatus.Codes.Entered ||
				bizObj.WD_DocketStatus == DocketStatus.Codes.New ||
				bizObj.WD_DocketStatus == DocketStatus.Codes.Error ||
				bizObj.WD_DocketStatus == DocketStatus.Codes.Held)
			{
				result = true;
			}
			return result;
		}

		#endregion

		#region CanMatchWarehouse

		/// <summary>
		/// Try to match and assign the Warehouse from the XSD value object
		/// to the Business Object Docket
		/// </summary>
		bool CanMatchWarehouse(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			var existingWhs = context.Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, xsdWhsDocket.DocketDetail.WarehouseCode));
			if (existingWhs != null)
			{
				bizObj.WD_WW_Whs = existingWhs.PK;
				return true;
			}

			context.SetPropertyInfoValue(bizObj.WD_WW_WhsInfo, xsdWhsDocket.DocketDetail.WarehouseCode, ForeignKeyType.WarehouseNK);
			if (!bizObj.WD_WW_Whs.IsEmpty)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			DocketDataFormatter.FormatXsdDocketData(value);

			CheckDocketForErrors(bizObj, value, context);
			if (AllowDocketModifications(bizObj))
			{
				bizObj.WD_OH_Client = context.FindOrCreateTempOrganisationPK(value.Identifier.Client, bizObj, OrganisationTypes.WarehouseClient);

				ImportTransportServiceLevel(bizObj, value, context);
				ImportServiceLevel(bizObj, value, context);
				ImportDocketAddresses(bizObj, value, context);
				ImportDocketAttributes(bizObj, value, context);
				ImportCustomValues(bizObj, value.DocketDetail, context);
				ImportPackageUQDetails(bizObj, value, context);
				ImportNotes(bizObj, value, context);
				ImportDocketReferences(bizObj, value, context);
				ImportDocketContainers(bizObj, value, context);
				ImportWhsDocketOtherDetails(bizObj, value, context);
				ImportLines(bizObj, value, context);

				AddImportEvent(bizObj);
			}
		}

		#endregion

		#region Importers

		#region ImportTransportServiceLevel

		void ImportTransportServiceLevel(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(bizObj.WD_PL_NKCarrierServiceLevelInfo, xsdWhsDocket.DocketDetail.TransportServiceLevel,
				xsdWhsDocket.DocketDetail.TransportServiceLevelSpecified, Res.GetString("7ad39fe0-a54b-441c-88df-ec33a1c5938e", "Transport Service Level"));
		}

		#endregion

		#region ImportServiceLevel

		void ImportServiceLevel(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(bizObj.WD_RS_NKServiceLevelInfo, xsdWhsDocket.DocketDetail.ServiceLevel,
				xsdWhsDocket.DocketDetail.ServiceLevelSpecified, Res.GetString("5c05741b-8a4e-49ec-b8c2-3f4de48d5b66", "Service Level"));
		}

		#endregion

		#region ImportNotes

		void ImportNotes(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			var noteAdapter = new NoteValueObjectDataAdapter();
			noteAdapter.ImportNotesAndAttachToBusinessObjectNotes(bizObj.Notes, xsdWhsDocket.Notes, context);
		}

		#endregion

		#region ImportPackageUQDetails

		void ImportPackageUQDetails(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			bizObj.CalculateTotalsEnabled = false;
			bizObj.WD_PackagesSent = (ZInt)value.DocketDetail.Packages.Value;
			context.SetPropertyInfoValue(bizObj.WD_F3_NKTotalPackTypeInfo, value.DocketDetail.Packages.DimensionType, value.DocketDetail.Packages.DimensionTypeSpecified, Res.GetString("83abc50c-45f2-43a4-b708-f65265ca0e77", "Packages UQ"));
			bizObj.CalculateTotalsEnabled = true;
		}

		#endregion

		#region ImportDocketAddresses

		void ImportDocketAddresses(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			ImportAddress(bizObj, xsdWhsDocket.DocketDetail.TransportCompany, context, "TransportCo");

			ImportAddress(bizObj, xsdWhsDocket.DocketDetail.TransportBilledTo, context, "TransportBillTo");
		}

		protected void ImportAddress(WhsDocket docket, Xsd.DocAddress xsdDocketAddress, IValueObjectImportContext context, string errorContext = "")
		{
			if (xsdDocketAddress.IsSpecified)
			{
				var addressType = DocAddressTypes.GetDocAddressTypeFromCode(context.Factory, xsdDocketAddress.AddressType.ToString());
				var docAddressToUpdate = docket.DocAddresses.FindByDocAddressType(addressType);
				if (docAddressToUpdate != null && !docAddressToUpdate.E2_AddressOverride)
				{
					docAddressToUpdate.ShouldClearAddressFieldsWhenOverride = true;
				}
				new DocAddressValueObjectHelper(errorContext).CreateOrUpdateFromValueObject(docket.DocAddresses, xsdDocketAddress, context);
			}
		}

		#endregion

		#region ImportCustomAttribute

		void ImportCustomAttribute(ZString xsdCustomAttribute, bool isCustomAttributeSpecified, ZPropertyInfo customAttributeInfo, ZString description, IValueObjectImportContext context)
		{
			if (xsdCustomAttribute.IsValid)
			{
				context.SetPropertyInfoValue(customAttributeInfo, xsdCustomAttribute, isCustomAttributeSpecified, description);
			}
		}

		#endregion

		#region ImportCustomDate

		void ImportCustomDate(ZDateTime xsdCustomDate, ZPropertyInfo customAttributeInfo, IValueObjectImportContext context)
		{
			if (xsdCustomDate.IsValid)
			{
				context.SetPropertyInfoValue(customAttributeInfo, xsdCustomDate.ToDateTime());
			}
		}

		#endregion

		#region ImportCustomDecimal

		void ImportCustomDecimal(ZDecimal xsdCustomDecimal, bool isCustomDecimalSpecified, ZPropertyInfo customDecimalInfo)
		{
			if (isCustomDecimalSpecified)
			{
				customDecimalInfo.Value = xsdCustomDecimal;
			}
		}

		#endregion

		#region ImportCustomFlag

		void ImportCustomFlag(bool xsdCustomFlag, bool isCustomFlagSpecified, ZPropertyInfo customFlagInfo)
		{
			if (isCustomFlagSpecified)
			{
				customFlagInfo.Value = (ZBool)xsdCustomFlag;
			}
		}

		#endregion

		#region ImportDocketAttributes

		protected void ImportDocketAttributes(TDocket bizObj, Xsd.WhsDocket xsdWhsDocket, IValueObjectImportContext context)
		{
			var attributes = xsdWhsDocket.DocketDetail.CustomAttributes;

			ImportCustomAttribute(attributes.CustomAttrib1, attributes.CustomAttrib1Specified, bizObj.WD_CustomAttrib1Info, Res.GetString("12330c56-c7d8-4c94-8c84-cbfc4932be0c", "Custom Attribute 1"), context);
			ImportCustomAttribute(attributes.CustomAttrib2, attributes.CustomAttrib2Specified, bizObj.WD_CustomAttrib2Info, Res.GetString("4ff9e2d0-6b72-41e2-bd42-813959be8aec", "Custom Attribute 2"), context);
			ImportCustomAttribute(attributes.CustomAttrib3, attributes.CustomAttrib3Specified, bizObj.WD_CustomAttrib3Info, Res.GetString("b54282b1-f600-4592-84c5-f87110677428", "Custom Attribute 3"), context);
			ImportCustomAttribute(attributes.CustomAttrib4, attributes.CustomAttrib4Specified, bizObj.WD_CustomAttrib4Info, Res.GetString("126e96f0-eb22-4705-8019-48b5c3fd7249", "Custom Attribute 4"), context);
			ImportCustomAttribute(attributes.CustomAttrib5, attributes.CustomAttrib5Specified, bizObj.WD_CustomAttrib5Info, Res.GetString("d6338b15-53e0-405a-b68f-1a71f97a1996", "Custom Attribute 5"), context);

			ImportCustomDate(attributes.CustomDate1, bizObj.WD_CustomDate1Info, context);
			ImportCustomDate(attributes.CustomDate2, bizObj.WD_CustomDate2Info, context);

			ImportCustomDecimal(attributes.CustomDecimal1, attributes.CustomDecimal1Specified, bizObj.WD_CustomDecimal1Info);
			ImportCustomDecimal(attributes.CustomDecimal2, attributes.CustomDecimal2Specified, bizObj.WD_CustomDecimal2Info);
			ImportCustomDecimal(attributes.CustomDecimal3, attributes.CustomDecimal3Specified, bizObj.WD_CustomDecimal3Info);
			ImportCustomDecimal(attributes.CustomDecimal4, attributes.CustomDecimal4Specified, bizObj.WD_CustomDecimal4Info);
			ImportCustomDecimal(attributes.CustomDecimal5, attributes.CustomDecimal5Specified, bizObj.WD_CustomDecimal5Info);

			ImportCustomFlag(attributes.CustomFlag1, attributes.CustomFlag1Specified, bizObj.WD_CustomFlag1Info);
			ImportCustomFlag(attributes.CustomFlag2, attributes.CustomFlag2Specified, bizObj.WD_CustomFlag2Info);
			ImportCustomFlag(attributes.CustomFlag3, attributes.CustomFlag3Specified, bizObj.WD_CustomFlag3Info);
			ImportCustomFlag(attributes.CustomFlag4, attributes.CustomFlag4Specified, bizObj.WD_CustomFlag4Info);
			ImportCustomFlag(attributes.CustomFlag5, attributes.CustomFlag5Specified, bizObj.WD_CustomFlag5Info);
		}

		#endregion

		#region ImportDocketReferences

		protected virtual void ImportDocketReferences(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			foreach (Xsd.WhsDocketDocketDetailReference xsdReference in value.DocketDetail.References)
			{
				var reference = FindExistingDocketReference(bizObj, xsdReference.Type)
					?? bizObj.References.AddNew();
				context.SetPropertyInfoValue(reference.WX_ReferenceInfo, xsdReference.Value, xsdReference.ValueSpecified, Res.GetString("9ac44e25-bb43-44a3-9b9a-2599246cdd39", "Docket Reference Value"));
				context.SetPropertyInfoValue(reference.WX_RefTypeInfo, xsdReference.Type, xsdReference.TypeSpecified, Res.GetString("ae842e78-2785-446a-9154-31398414cd18", "Docket Reference Type"));
			}

			context.SetPropertyInfoValue(bizObj.WD_ExternalReferenceInfo, value.Identifier.Reference, value.Identifier.ReferenceSpecified, GetBizObjHumanReadableName(bizObj) + " " + Res.GetString("9f37a129-94c4-4068-9d77-0ee0399badc9", "Number"));
			context.SetPropertyInfoValue(bizObj.WD_CustomerReferenceInfo, value.DocketDetail.CustomerReference, value.DocketDetail.CustomerReferenceSpecified, Res.GetString("a82fc28a-c248-4f19-8249-efe647697bba", "Customer Reference"));
			context.SetPropertyInfoValue(bizObj.WD_TransportReferenceInfo, value.DocketDetail.TransportReference, value.DocketDetail.TransportReferenceSpecified, Res.GetString("366c762a-2592-4674-9376-9b812e91065f", "Transport Reference"));
		}

		#endregion

		#region ImportDocketContainers

		protected virtual void ImportDocketContainers(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			bizObj.Containers.DeleteAll();
			foreach (Xsd.WhsDocketDocketDetailContainer xsdContainer in value.DocketDetail.Containers)
			{
				var container = FindExistingDocketContainer(bizObj, xsdContainer.ContainerNo)
					?? bizObj.Containers.AddNew();
				context.SetPropertyInfoValue(container.WC_ContainerNumInfo, xsdContainer.ContainerNo, xsdContainer.ContainerNoSpecified, Res.GetString("51149505-22ef-4038-810f-67c0bd201dc1", "Docket Container No"));
				context.SetPropertyInfoValue(container.WC_SealNumInfo, xsdContainer.SealNo, xsdContainer.SealNoSpecified, Res.GetString("3432a059-fdcf-4412-b3d4-8b91600f35d3", "Docket Seal No"));

				var containerRef = FindExistingContainerType(xsdContainer.Type, context);
				container.WC_RC = (containerRef != null ? containerRef.PK : ZGuid.Empty);

				container.WC_PalletCount = xsdContainer.Pallets;
				container.WC_ItemCount = xsdContainer.Packages;
				container.WC_IsPalletised = xsdContainer.Palletised;
				container.WC_IsChargeable = xsdContainer.Chargeable;
			}
		}

		#endregion

		#region FindExistingContainerType

		RefContainer FindExistingContainerType(ZString type, IValueObjectImportContext context)
		{
			var query = new ZQuery();
			query.AddToFilter(RefContainerSchema.RC_Code, type);

			return context.Factory.LoadTop1<RefContainer>(query);
		}

		#endregion

		#region FindExistingDocketReference

		WhsDocketReference FindExistingDocketReference(TDocket bizObj, ZString refType)
		{
			WhsDocketReference result = null;
			foreach (WhsDocketReference reference in bizObj.References)
			{
				if (reference.WX_RefType == refType)
				{
					result = reference;
					break;
				}
			}
			return result;
		}

		#endregion

		#region FindExistingDocketContainer

		WhsDocketContainer FindExistingDocketContainer(TDocket bizObj, ZString containerNo)
		{
			WhsDocketContainer result = null;
			foreach (WhsDocketContainer container in bizObj.Containers)
			{
				if (container.WC_ContainerNum.ToUpper() == containerNo.ToUpper())
				{
					result = container;
					break;
				}
			}
			return result;
		}

		#endregion

		#region FindOrCreateDocketLine

		protected WhsDocketLine FindOrCreateDocketLine(TDocket bizObj, Xsd.WhsDocketLine xsdLine)
		{
			WhsDocketLine result = null;
			foreach (WhsDocketLine line in bizObj.Lines)
			{
				var lineNo = line.WE_LineNo; // for performance reasons.
				if (lineNo != 0 &&
					lineNo == xsdLine.LineNumber &&
					line.WE_SubLineNo == xsdLine.SubLineNumber)
				{
					result = line;
					break;
				}
			}

			if (result == null)
			{
				result = bizObj.Lines.AddNew();
				result.WE_LineNo = xsdLine.LineNumber;
				result.WE_SubLineNo = xsdLine.SubLineNumber;
			}

			return result;
		}

		#endregion

		#region ImportLine

		protected void ImportLine(TDocket bizObj, Xsd.WhsDocket value, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			DocketDataFormatter.FormatXsdDocketLineData(xsdLine);
			var line = FindOrCreateDocketLine(bizObj, xsdLine);

			var part = MatchProduct(bizObj, xsdLine, context);

			if (part != null)
			{
				line.WE_OP = part.PK;
			}
			else
			{
				part = GetINVALIDProductPK(bizObj, value, bizObj.Client, context);
				if (part != null)
				{
					line.WE_OP = part.PK;
				}
				else
				{
					line.WE_OP = ZGuid.Empty;
				}
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("12c9453c-f0a4-4eb9-aa3c-f6a8ee5fe5af", "Product: {0}/Client: {1} could not be found. {2} no: {3}", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, GetBizObjHumanReadableName(bizObj), value.Identifier.Reference)));
			}

			context.SetPropertyInfoValue(line.WE_LineCommentInfo, xsdLine.LineComments, xsdLine.LineCommentsSpecified, Res.GetString("5bbb85b6-b5c7-4276-ac91-f5d8df6f61e9", "Line Comments"));

			ImportLinePackageType(line.WE_F3_NKPackTypeInfo, xsdLine.ProductUQ, context, xsdLine.ProductUQSpecified);
			ImportLineQuantity(line, xsdLine, context);

			#region Atttributes

			if (bizObj.Client != null)
			{
				var attributeManager = new PartAttributeManager(bizObj.Client);

				context.SetPropertyInfoValue(line.WE_BondedEntryKeyInfo, xsdLine.LineAttributes.BondedEntryKey, xsdLine.LineAttributes.BondedEntryKeySpecified, Res.GetString("7513340f-73a0-4e08-a62a-f5eb6560eab3", "Bonded Entry Key"));

				if (xsdLine.LineAttributes.PackingDate.IsValid && !xsdLine.LineAttributes.PackingDate.IsEmpty)
				{
					if (attributeManager.IsPackingDateUsedByProduct(part))
					{
						context.SetPropertyInfoValue(line.WE_PackingDateInfo, xsdLine.LineAttributes.PackingDate.ToDateTime());
					}
					else
					{
						context.Notify(new InfoNotification(Res.GetString("aaf3ed4a-1664-46a8-9910-45403db0b0db", "Product: {0}/Client: {1} is not setup to use Packing Date. Packing Date: {2} has been ignored.", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, xsdLine.LineAttributes.PackingDate)));
					}
				}
				else
				{
					line.WE_PackingDate = ZDate.Empty;
				}

				if (xsdLine.LineAttributes.ExpiryDate.IsValid && !xsdLine.LineAttributes.ExpiryDate.IsEmpty)
				{
					if (attributeManager.IsExpiryDateUsedByProduct(part))
					{
						context.SetPropertyInfoValue(line.WE_ExpiryDateInfo, xsdLine.LineAttributes.ExpiryDate.ToDateTime());
					}
					else
					{
						context.Notify(new InfoNotification(Res.GetString("e45dd15d-4def-4af9-be66-2b2c5520665c", "Product: {0}/Client: {1} is not setup to use Expiry Date. Expiry Date: {2} has been ignored.", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, xsdLine.LineAttributes.ExpiryDate)));
					}
				}
				else
				{
					line.WE_ExpiryDate = ZDate.Empty;
				}

				if (!xsdLine.LineAttributes.PartAttribute1.IsEmpty)
				{
					if (attributeManager.IsPartAttributeUsedByProduct(part, 1))
					{
						context.SetPropertyInfoValue(line.WE_PartAttrib1Info, xsdLine.LineAttributes.PartAttribute1, xsdLine.LineAttributes.PartAttribute1Specified, Res.GetString("4bfb812f-370d-43f2-ad38-c47168867c9b", "Part Attribute 1"));
					}
					else
					{
						context.Notify(new InfoNotification(Res.GetString("e50f2898-f81b-47a6-80c9-a5cd71c09286", "Product: {0}/Client: {1} is not setup to use Part Attribute 1. Part Attribute 1: {2} has been ignored.", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, xsdLine.LineAttributes.PartAttribute1)));
					}
				}

				if (!xsdLine.LineAttributes.PartAttribute2.IsEmpty)
				{
					if (attributeManager.IsPartAttributeUsedByProduct(part, 2))
					{
						context.SetPropertyInfoValue(line.WE_PartAttrib2Info, xsdLine.LineAttributes.PartAttribute2, xsdLine.LineAttributes.PartAttribute2Specified, Res.GetString("bca78446-55f8-4aaf-b51d-29c9af0dd4f0", "Part Attribute 2"));
					}
					else
					{
						context.Notify(new InfoNotification(Res.GetString("2ac7900d-5785-4a05-8278-15c94c300d97", "Product: {0}/Client: {1} is not setup to use Part Attribute 2. Part Attribute 2: {2} has been ignored.", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, xsdLine.LineAttributes.PartAttribute2)));
					}
				}

				if (!xsdLine.LineAttributes.PartAttribute3.IsEmpty)
				{
					if (attributeManager.IsPartAttributeUsedByProduct(part, 3))
					{
						context.SetPropertyInfoValue(line.WE_PartAttrib3Info, xsdLine.LineAttributes.PartAttribute3, xsdLine.LineAttributes.PartAttribute3Specified, Res.GetString("a974e64c-7a17-4489-aea1-1348db19171d", "Part Attribute 3"));
					}
					else
					{
						context.Notify(new InfoNotification(Res.GetString("63b14e50-25d5-45c5-897a-e146eb30e2d3", "Product: {0}/Client: {1} is not setup to use Part Attribute 3. Part Attribute 3: {2} has been ignored.", xsdLine.Product, value.Identifier.Client.OrganisationDetails.Name, xsdLine.LineAttributes.PartAttribute3)));
					}
				}
			}

			var attributes = xsdLine.LineAttributes;

			context.SetPropertyInfoValue(line.WE_CustomAttrib1Info, attributes.CustomAttribute1, attributes.CustomAttribute1Specified, Res.GetString("46ed5551-7b57-40b7-ae4c-e671085b716d", "Custom Attribute 1"));
			context.SetPropertyInfoValue(line.WE_CustomAttrib2Info, attributes.CustomAttribute2, attributes.CustomAttribute2Specified, Res.GetString("aeb184c8-2d2e-4214-8bb3-c9af888ff285", "Custom Attribute 2"));
			context.SetPropertyInfoValue(line.WE_CustomAttrib3Info, attributes.CustomAttribute3, attributes.CustomAttribute3Specified, Res.GetString("01527cd1-5f92-47a1-922b-19b8ace60ab2", "Custom Attribute 3"));
			context.SetPropertyInfoValue(line.WE_CustomAttrib4Info, attributes.CustomAttribute4, attributes.CustomAttribute4Specified, Res.GetString("9569f8a5-86f6-48be-8508-a75d6f7a3b38", "Custom Attribute 4"));
			context.SetPropertyInfoValue(line.WE_CustomAttrib5Info, attributes.CustomAttribute5, attributes.CustomAttribute5Specified, Res.GetString("6d893ebe-8557-40f5-bf1e-1b6f2f6391a6", "Custom Attribute 5"));
			context.SetPropertyInfoValue(line.WE_CustomAttrib6Info, attributes.CustomAttribute6, attributes.CustomAttribute6Specified, Res.GetString("828cee45-5133-45af-a61d-6a146002a9e5", "Custom Attribute 6"));

			ImportCustomDate(attributes.CustomDate1, line.WE_CustomDate1Info, context);
			ImportCustomDate(attributes.CustomDate2, line.WE_CustomDate2Info, context);
			ImportCustomDate(attributes.CustomDate3, line.WE_CustomDate3Info, context);
			ImportCustomDate(attributes.CustomDate4, line.WE_CustomDate4Info, context);
			ImportCustomDate(attributes.CustomDate5, line.WE_CustomDate5Info, context);

			ImportCustomDecimal(attributes.CustomDecimal1, attributes.CustomDecimal1Specified, line.WE_CustomDecimal1Info);
			ImportCustomDecimal(attributes.CustomDecimal2, attributes.CustomDecimal2Specified, line.WE_CustomDecimal2Info);
			ImportCustomDecimal(attributes.CustomDecimal3, attributes.CustomDecimal3Specified, line.WE_CustomDecimal3Info);
			ImportCustomDecimal(attributes.CustomDecimal4, attributes.CustomDecimal4Specified, line.WE_CustomDecimal4Info);
			ImportCustomDecimal(attributes.CustomDecimal5, attributes.CustomDecimal5Specified, line.WE_CustomDecimal5Info);

			ImportCustomFlag(attributes.CustomFlag1, attributes.CustomFlag1Specified, line.WE_CustomFlag1Info);
			ImportCustomFlag(attributes.CustomFlag2, attributes.CustomFlag2Specified, line.WE_CustomFlag2Info);
			ImportCustomFlag(attributes.CustomFlag3, attributes.CustomFlag3Specified, line.WE_CustomFlag3Info);
			ImportCustomFlag(attributes.CustomFlag4, attributes.CustomFlag4Specified, line.WE_CustomFlag4Info);
			ImportCustomFlag(attributes.CustomFlag5, attributes.CustomFlag5Specified, line.WE_CustomFlag5Info);

			#endregion

			#region Bonded Attributes

			if (xsdLine.CustomsData.IsSpecified)
			{
				line.CustomsData.WB_BondedWhsQty = xsdLine.CustomsData.BondedWhsQuantity;
				line.CustomsData.WB_CustomsQty = xsdLine.CustomsData.CustomsQuantity;
				line.CustomsData.WB_EntryLineNo = xsdLine.CustomsData.EntryLineNumber;
				line.CustomsData.WB_TILV = xsdLine.CustomsData.TILVAmount;
				line.CustomsData.WB_ValueForDuty = xsdLine.CustomsData.ValueForDuty;

				context.SetPropertyInfoValue(line.CustomsData.WB_AddInfoInfo, xsdLine.CustomsData.AddInfo, xsdLine.CustomsData.AddInfoSpecified, Res.GetString("9da17f70-5bbf-4a1e-9c48-877b20b37960", "Line Bonded Attribute Add Info"));
				context.SetPropertyInfoValue(line.CustomsData.WB_BondedWhsUnitOfQtyInfo, xsdLine.CustomsData.BondedWhsQuantityUnit, xsdLine.CustomsData.BondedWhsQuantityUnitSpecified, Res.GetString("a8b327cc-cb1a-46e2-b733-1c39f2570ebd", "Line Bonded Attribute Bonded Whs. Quantity Unit"));
				context.SetPropertyInfoValue(line.CustomsData.WB_CustomsUnitOfQtyInfo, xsdLine.CustomsData.CustomsQuantityUnit, xsdLine.CustomsData.CustomsQuantityUnitSpecified, Res.GetString("fcbd009a-cc44-422e-8c9b-715fa731639c", "Line Bonded Attribute Customs Quantity Unit"));

				if (!xsdLine.CustomsData.EntryDate.IsEmpty)
				{
					context.SetPropertyInfoValue(line.CustomsData.WB_EntryDateInfo, xsdLine.CustomsData.EntryDate.ToDateTime());
				}
				else
				{
					line.CustomsData.WB_EntryDate = ZDateTime.Empty;
				}

				context.SetPropertyInfoValue(line.CustomsData.WB_EntryKeyInfo, xsdLine.CustomsData.EntryKey, xsdLine.CustomsData.EntryKeySpecified, Res.GetString("0d24181e-4638-4e24-88b3-87464f836d11", "Line Bonded Attribute Entry Key"));
				context.SetPropertyInfoValue(line.CustomsData.WB_RN_NKCountryOfOriginInfo, xsdLine.CustomsData.CountryOfOrigin, xsdLine.CustomsData.CountryOfOriginSpecified, Res.GetString("86a0cc74-e922-477f-91f6-329ed690ecb0", "Line Bonded Attribute Country/Region Of Origin"));
				context.SetPropertyInfoValue(line.CustomsData.WB_RX_NKTILVCurrencyInfo, xsdLine.CustomsData.TILVCurrency, xsdLine.CustomsData.TILVCurrencySpecified, Res.GetString("dfe39ee0-0480-4ce4-89c3-d80b06691aba", "Line Bonded Attribute TILV Currency"));
				context.SetPropertyInfoValue(line.CustomsData.WB_DeclarationReferenceInfo, xsdLine.CustomsData.DeclarationReference, xsdLine.CustomsData.DeclarationReferenceSpecified, Res.GetString("d9a2d481-3e96-4412-a51d-10fa764aefc0", "Line Bonded Attribute Dec. Ref."));
			}

			#endregion

			ImportLineAdditionalDetails(bizObj, line, xsdLine, context);
			ImportLineCrossDockLineLinks(bizObj, line, xsdLine, context);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(bizObj, line, xsdLine);
		}

		#endregion

		#endregion

		#region MatchProduct

		OrgSupplierPart MatchProduct(TDocket bizObj, Xsd.WhsDocketLine xsdDocketLine, IValueObjectImportContext context)
		{
			var factory = new BusinessObjectFactory();
			var whsPart = new WhsPart
			{
				PartNum = xsdDocketLine.Product,
				Description = xsdDocketLine.Description,
				Buyer = bizObj.Client,
				StockKeepingUnit = xsdDocketLine.ProductUQ,
			};

			var allowCreate = Registry.Business.SystemDataRegistry.Instance.CreateMissingWarehouseProduct.Value;
			new ProductMatcher(factory).MatchAllowEmptySupplier(whsPart, allowCreate, out var orgPart, out _);
			if (orgPart != null)
			{
				context.Notify(new InfoNotification(Res.GetString("7CEB5F93-7EF9-4893-8616-192411BEE845", "Successfully created new product '{0}' for client '{1}'", xsdDocketLine.Product, bizObj.Client.OH_Code)));
			}

			return orgPart;
		}

		#endregion

		#region GetINVALIDProductPK

		protected virtual OrgSupplierPart GetINVALIDProductPK(TDocket bizObj, Xsd.WhsDocket value, OrgHeader client, IValueObjectImportContext context)
		{
			if (client == null)
			{
				return null;
			}
			var factory = new BusinessObjectFactory();
			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, ProductType.Codes.Invalid);

			var partRelationQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			partRelationQuery.AddToFilter(OrgPartRelationSchema.OU_OH, client.PK);
			partRelationQuery.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			productQuery.AddSubQuery(partRelationQuery, JoinCondition.And);

			var result = factory.LoadTop1<OrgSupplierPart>(productQuery);
			if (result == null)
			{
				productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, ProductType.Codes.Invalid);
				result = factory.LoadTop1<OrgSupplierPart>(productQuery);

				if (result == null)
				{
					result = CreateINVALIDProduct(client, context, factory);
				}

				if (result != null)
				{
					var relation = result.RelatedOrganisations.AddNew();
					relation.OU_OH = client.PK;
					relation.OU_OP = result.PK;
					relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
					try
					{
						factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						context.Notify(new ErrorNotification(ErrorType.ImportingDataError, GetBizObjHumanReadableName(bizObj) + (NoResString)" no: " + value.Identifier.Reference + (NoResString)". Error: " + e.Message)); // Fix this text
					}
				}
			}
			return result;
		}

		#endregion

		#region CreateINVALIDProduct

		protected virtual OrgSupplierPart CreateINVALIDProduct(OrgHeader client, IValueObjectImportContext context, BusinessObjectFactory factory)
		{
			var result = factory.New<OrgSupplierPart>();
			result.OP_PartNum = ProductType.Codes.Invalid;
			result.OP_Desc = ProductType.Descriptions.Invalid;
			result.OP_StockKeepingUnit = "UNT";
			result.OP_WeightUQ = "KG";
			result.OP_CubicUQ = "M3";
			return result;
		}

		#endregion

		#region ImportLinePackageType

		void ImportLinePackageType(ZPropertyInfo propertyInfo, ZString packageType, IValueObjectImportContext context, bool isSpecified)
		{
			if (isSpecified)
			{
				// standard ref package type
				ZString enterprisePackType = new PkgUnitXmlCodeMappingsIncludingReferenceFiles(propertyInfo.BizObj.Factory).GetEnterpriseCode(packageType, "", null);
				if (!enterprisePackType.IsEmpty)
				{
					context.SetPropertyInfoValue(propertyInfo, packageType, isSpecified, Res.GetString("eba7f969-048d-41d0-8a20-0de544de35ca", "Pack Type"));
				}
				// non-standard package type, look for a mapping
				else
				{
					var mappedPackType = (ZString)context.Converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.PackTypeCodeNK, context.Factory, packageType, context);
					if (mappedPackType.Length > propertyInfo.MaxLength)
					{
						var maxLengthWarningMessage = Res.GetString("cde5716b-dba4-4786-a9be-683ef4b13e32",
							"Pack Type ({0}) has exceeded the maximum length allowed by the system. When you edit this record, the package type field will display a warning. Please use Code Mapping to map this value to the appropriate {1} package type.", mappedPackType, Constants.ProductName);
						context.SetPropertyInfoValue(propertyInfo, mappedPackType, ForeignKeyType.PackTypeCodeNK, maxLengthWarningMessage);
					}
					else
					{
						var mappingFound = (packageType != mappedPackType);
						if (!mappingFound)
						{
							var undefinedPackTypeWarningMessage = Res.GetString("0a9acb61-187d-4df7-870c-835cb2ac5e9a",
								"Pack Type ({0}) is not valid. When you edit this record, the pack type field will display a warning. To avoid this warning you can either use Code Mapping to map this value to the appropriate {1} package type or you can add this value to the reference files (Reference Files -> Package Types).", packageType, Constants.ProductName);
							context.Notify(new WarningNotification(undefinedPackTypeWarningMessage));
						}

						context.SetPropertyInfoValue(propertyInfo, mappedPackType, ForeignKeyType.PackTypeCodeNK);
					}
				}
			}
		}

		#endregion

		#region ImportLineQuantity

		protected virtual void ImportLineQuantity(WhsDocketLine line, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			line.WE_PackQuantity = (xsdLine.QuantityFromClientOrder == 0) ? xsdLine.QuantityActuallyOrdered : xsdLine.QuantityFromClientOrder;
		}

		#endregion

		#region ImportLines

		protected void ImportLines(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			bool lineIsNotIncludedInImport;
			foreach (WhsDocketLine line in bizObj.Lines)
			{
				lineIsNotIncludedInImport = true;
				foreach (Xsd.WhsDocketLine xsdLine in value.DocketLines)
				{
					if (line.WE_LineNo != 0)
					{
						if (line.WE_LineNo == xsdLine.LineNumber && line.WE_SubLineNo == xsdLine.SubLineNumber)
						{
							lineIsNotIncludedInImport = false;
							break;
						}
					}
				}

				if (lineIsNotIncludedInImport)
				{
					ProcessExistingLineNotIncludedInImport(bizObj, line, context);
				}
			}

			foreach (Xsd.WhsDocketLine xsdLine in value.DocketLines)
			{
				ImportLine(bizObj, value, xsdLine, context);
			}
		}

		#endregion

		#region ProcessExistingLineNotIncludedInImport

		protected virtual void ProcessExistingLineNotIncludedInImport(TDocket bizObj, WhsDocketLine line, IValueObjectImportContext context)
		{
			if (bizObj.Client.MiscServ.OM_WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage)
			{
				line.WE_TransactionQuantity = 0;
			}
		}

		#endregion

		#region ImportWhsDocketOtherDetails

		protected virtual void ImportWhsDocketOtherDetails(TDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
		}

		#endregion

		#region ImportLineAdditionalDetails

		protected virtual void ImportLineAdditionalDetails(TDocket bizObj, WhsDocketLine line, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
		}

		#endregion

		#endregion

		#region Export

		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(TDocket bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			value.Identifier.Client = new OrganisationValueObjectDataAdapter().ExportToValueObject(bizObj.Client, context);

			value.DocketDetail = new Xsd.WhsDocketDocketDetail();
			value.DocketDetail.WarehouseCode = (bizObj.Warehouse == null ? ZString.Empty : bizObj.Warehouse.WW_WarehouseCode);
			value.DocketDetail.Status = bizObj.WD_DocketStatus;

			value.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(bizObj.Notes, context);

			var errorContext = Res.GetString("8a1fad78-99ef-4383-9050-89f1ebad4e80", "Docket {0}", bizObj.WD_DocketID);
			value.Events = StmALogValueObjectDataAdapter.New(bizObj, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);

			value.Identifier.DocketID = bizObj.WD_DocketID;
			value.Identifier.Reference = bizObj.WD_ExternalReference;
			value.Identifier.DocketType = GetXsdWhsDocketType();

			value.DocketDetail.CustomerReference = bizObj.WD_CustomerReference;

			if (bizObj is IJobWithTransportCompany jobWithTransportCompany)
			{
				value.DocketDetail.TransportCompany = new DocAddressValueObjectHelper((NoResString)"Transport Co And Address").ExportToValueObject(jobWithTransportCompany.TransportCoDocAddress, context); // Hard-coded constant
			}

			value.DocketDetail.TransportBilledTo = new DocAddressValueObjectHelper((NoResString)"Transport Billed To And Address").ExportToValueObject(bizObj.TransportBillToDocAddress, context); // Hard-coded constant
			value.DocketDetail.TransportReference = bizObj.WD_TransportReference;
			value.DocketDetail.TransportServiceLevel = bizObj.WD_PL_NKCarrierServiceLevel;
			value.DocketDetail.ServiceLevel = bizObj.WD_RS_NKServiceLevel;
			value.DocketDetail.Packages.Value = (ZDecimal)bizObj.WD_PackagesSent;
			value.DocketDetail.Packages.DimensionType = bizObj.WD_F3_NKTotalPackType;

			value.DocketDetail.Weight.IsSpecified = true;
			value.DocketDetail.Cubic.IsSpecified = true;

			ExportDocketAttributes(bizObj, value, context);
			ExportCustomValues(bizObj, value.DocketDetail, context);
			ExportDocketReferences(bizObj, value, context);
			ExportDocketContainers(bizObj, value, context);
			ExportBilling(bizObj, value, context);

			ExportAdditionalToValueObjectCore(bizObj, value, context);

			ExportDocketLines(bizObj, value, context);

			if (IncludeeDocs)
			{
				ExportStorageDocs(bizObj, value, context);
			}

			AddExportEvent(value, bizObj, context);
		}

		#endregion

		#region IncludeeDocs

		protected virtual bool IncludeeDocs => false;

		#endregion

		#region ExportStorageDocs

		void ExportStorageDocs(TDocket docket, Xsd.WhsDocket docketValue, IValueObjectExportContext context)
		{
			var adapterTypeToCreate1 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageDocsValueObjectDataAdapter>());
			var adapterTypeToCreate2 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageFilesValueObjectDataAdapter>());
			var storageDocsDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate1);
			var storageFilesDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate2);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(docket.Factory);
			var storageMain = documentFactory.GetStorageMain(docket.PK);

			if (storageMain != null)
			{
				foreach (BusinessObject storageDoc in storageMain.DocumentCollectionView)
				{
					docketValue.Documents.Add((Xsd.Document)storageDocsDataAdapter.ExportToValueObject(storageDoc, context));
				}
				foreach (BusinessObject storageDoc in storageMain.PDFFilesCollectionView)
				{
					docketValue.Documents.Add((Xsd.Document)storageFilesDataAdapter.ExportToValueObject(storageDoc, context));
				}
			}
		}

		#endregion

		#region ExportBilling

		void ExportBilling(TDocket docket, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			var header = new JobHeader.Loader(docket).Load();

			if (header == null || !IncludeBillingInfoInWarehouseXML)
			{
				value.Billing.IsSpecified = false;
			}
			else
			{
				var dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ExportToValueObject(docket, value.Billing, context);
			}
		}

		#endregion

		#region CreateBillingDataAdapter

		IValueObjectDataAdapter CreateBillingDataAdapter()
		{
			return (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.IBillingDataAdapter>());
		}

		#endregion

		#region IncludeBillingInfoInWarehouseXML

		protected virtual bool IncludeBillingInfoInWarehouseXML => SystemDataRegistry.Instance.IncludeBillingInfoInWarehouseXML.Value;

		#endregion

		#region ExportDocketAttributes

		protected virtual void ExportDocketAttributes(TDocket bizObj, Xsd.WhsDocket value, INotifications notifications)
		{
			if (!bizObj.WD_CustomAttrib1.IsEmpty)
			{
				value.DocketDetail.CustomAttributes.CustomAttrib1 = bizObj.WD_CustomAttrib1;
			}

			if (!bizObj.WD_CustomAttrib2.IsEmpty)
			{
				value.DocketDetail.CustomAttributes.CustomAttrib2 = bizObj.WD_CustomAttrib2;
			}

			if (!bizObj.WD_CustomAttrib3.IsEmpty)
			{
				value.DocketDetail.CustomAttributes.CustomAttrib3 = bizObj.WD_CustomAttrib3;
			}

			if (!bizObj.WD_CustomAttrib4.IsEmpty)
			{
				value.DocketDetail.CustomAttributes.CustomAttrib4 = bizObj.WD_CustomAttrib4;
			}

			if (!bizObj.WD_CustomAttrib5.IsEmpty)
			{
				value.DocketDetail.CustomAttributes.CustomAttrib5 = bizObj.WD_CustomAttrib5;
			}

			if (bizObj.WD_CustomDate1.IsValid)
			{
				value.DocketDetail.CustomAttributes.CustomDate1 = bizObj.WD_CustomDate1;
			}

			if (bizObj.WD_CustomDate2.IsValid)
			{
				value.DocketDetail.CustomAttributes.CustomDate2 = bizObj.WD_CustomDate2;
			}

			if (bizObj.Client != null)
			{
				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomDecimal1) != null)
				{
					value.DocketDetail.CustomAttributes.CustomDecimal1 = bizObj.WD_CustomDecimal1;
					value.DocketDetail.CustomAttributes.CustomDecimal1Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomDecimal2) != null)
				{
					value.DocketDetail.CustomAttributes.CustomDecimal2 = bizObj.WD_CustomDecimal2;
					value.DocketDetail.CustomAttributes.CustomDecimal2Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomDecimal3) != null)
				{
					value.DocketDetail.CustomAttributes.CustomDecimal3 = bizObj.WD_CustomDecimal3;
					value.DocketDetail.CustomAttributes.CustomDecimal3Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomDecimal4) != null)
				{
					value.DocketDetail.CustomAttributes.CustomDecimal4 = bizObj.WD_CustomDecimal4;
					value.DocketDetail.CustomAttributes.CustomDecimal4Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomDecimal5) != null)
				{
					value.DocketDetail.CustomAttributes.CustomDecimal5 = bizObj.WD_CustomDecimal5;
					value.DocketDetail.CustomAttributes.CustomDecimal5Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomFlag1) != null)
				{
					value.DocketDetail.CustomAttributes.CustomFlag1 = bizObj.WD_CustomFlag1;
					value.DocketDetail.CustomAttributes.CustomFlag1Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomFlag2) != null)
				{
					value.DocketDetail.CustomAttributes.CustomFlag2 = bizObj.WD_CustomFlag2;
					value.DocketDetail.CustomAttributes.CustomFlag2Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomFlag3) != null)
				{
					value.DocketDetail.CustomAttributes.CustomFlag3 = bizObj.WD_CustomFlag3;
					value.DocketDetail.CustomAttributes.CustomFlag3Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomFlag4) != null)
				{
					value.DocketDetail.CustomAttributes.CustomFlag4 = bizObj.WD_CustomFlag4;
					value.DocketDetail.CustomAttributes.CustomFlag4Specified = true;
				}

				if (bizObj.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocket.CustomFlag5) != null)
				{
					value.DocketDetail.CustomAttributes.CustomFlag5 = bizObj.WD_CustomFlag5;
					value.DocketDetail.CustomAttributes.CustomFlag5Specified = true;
				}
			}
		}

		#endregion

		#region ExportDocketContainers

		protected virtual void ExportDocketContainers(TDocket bizObj, Xsd.WhsDocket value, INotifications notifications)
		{
			foreach (WhsDocketContainer container in bizObj.Containers)
			{
				var xsdContainer = value.DocketDetail.Containers.AddNew();
				xsdContainer.Chargeable = container.WC_IsChargeable;
				xsdContainer.ContainerNo = container.WC_ContainerNum;
				xsdContainer.Packages = container.WC_ItemCount;
				xsdContainer.Palletised = container.WC_IsPalletised;
				xsdContainer.Pallets = container.WC_PalletCount;
				xsdContainer.SealNo = container.WC_SealNum;
				xsdContainer.Type = (container.Container != null ? container.Container.RC_Code : ZString.Empty);
			}
		}

		#endregion

		#region ExportDocketReferences

		protected virtual void ExportDocketReferences(TDocket bizObj, Xsd.WhsDocket value, INotifications notifications)
		{
			foreach (WhsDocketReference reference in bizObj.References)
			{
				var xsdReference = value.DocketDetail.References.AddNew();
				xsdReference.Type = reference.WX_RefType;
				xsdReference.Value = reference.WX_Reference;
			}
		}

		#endregion

		#region ExportDocketLines

		protected void ExportDocketLines(TDocket bizObj, Xsd.WhsDocket value, INotifications notifications)
		{
			foreach (WhsDocketLine line in bizObj.Lines)
			{
				ExportDocketLine(line, value.DocketLines.AddNew(), notifications);
			}
		}

		#endregion

		#region ExportDocketLine

		protected void ExportDocketLine(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
			value.Description = line.ProductDesc;
			value.LineComments = line.WE_LineComment;
			value.LineNumber = line.WE_LineNo;
			value.SubLineNumber = line.WE_SubLineNo;
			value.Product = (line.SupplierPart == null ? ZString.Empty : line.SupplierPart.OP_PartNum);
			value.ProductUQ = line.WE_F3_NKPackType;
			value.QuantityActuallyOrdered = line.WE_PackQuantity;
			value.QuantityFromClientOrder = line.WE_PackQuantity;

			value.LineAttributes.BondedEntryKey = line.WE_BondedEntryKey;
			value.LineAttributes.PartAttribute1 = line.WE_PartAttrib1;
			value.LineAttributes.PartAttribute2 = line.WE_PartAttrib2;
			value.LineAttributes.PartAttribute3 = line.WE_PartAttrib3;
			value.LineAttributes.ExpiryDate = line.WE_ExpiryDate;
			value.LineAttributes.PackingDate = line.WE_PackingDate;

			value.CustomsData.AddInfo = line.CustomsData.WB_AddInfo;
			value.CustomsData.BondedWhsQuantity = line.CustomsData.WB_BondedWhsQty;
			value.CustomsData.BondedWhsQuantityUnit = line.CustomsData.WB_BondedWhsUnitOfQty;
			value.CustomsData.CustomsQuantity = line.CustomsData.WB_CustomsQty;
			value.CustomsData.CustomsQuantityUnit = line.CustomsData.WB_CustomsUnitOfQty;
			value.CustomsData.DeclarationReference = line.CustomsData.WB_DeclarationReference;
			value.CustomsData.EntryDate = line.CustomsData.WB_EntryDate.Date;
			value.CustomsData.EntryKey = line.CustomsData.WB_EntryKey;
			value.CustomsData.EntryLineNumber = line.CustomsData.WB_EntryLineNo;
			value.CustomsData.CountryOfOrigin = line.CustomsData.WB_RN_NKCountryOfOrigin;
			value.CustomsData.TILVCurrency = line.CustomsData.WB_RX_NKTILVCurrency;
			value.CustomsData.TILVAmount = line.CustomsData.WB_TILV;
			value.CustomsData.ValueForDuty = line.CustomsData.WB_ValueForDuty;

			ExportDocketLineCustomAttributes(line, value);
			ExportDocketLineAdditionalInfo(line, value, notifications);
			ExportCrossDockLines(line, value);
		}

		#endregion

		#region ExportDocketLineCustomAttributes

		protected void ExportDocketLineCustomAttributes(WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			if (!line.WE_CustomAttrib1.IsEmpty)
			{
				value.LineAttributes.CustomAttribute1 = line.WE_CustomAttrib1;
			}
			if (!line.WE_CustomAttrib2.IsEmpty)
			{
				value.LineAttributes.CustomAttribute2 = line.WE_CustomAttrib2;
			}
			if (!line.WE_CustomAttrib3.IsEmpty)
			{
				value.LineAttributes.CustomAttribute3 = line.WE_CustomAttrib3;
			}
			if (!line.WE_CustomAttrib4.IsEmpty)
			{
				value.LineAttributes.CustomAttribute4 = line.WE_CustomAttrib4;
			}
			if (!line.WE_CustomAttrib5.IsEmpty)
			{
				value.LineAttributes.CustomAttribute5 = line.WE_CustomAttrib5;
			}
			if (!line.WE_CustomAttrib6.IsEmpty)
			{
				value.LineAttributes.CustomAttribute6 = line.WE_CustomAttrib6;
			}

			if (line.WE_CustomDate1.IsValid)
			{
				value.LineAttributes.CustomDate1 = line.WE_CustomDate1;
			}
			if (line.WE_CustomDate2.IsValid)
			{
				value.LineAttributes.CustomDate2 = line.WE_CustomDate2;
			}
			if (line.WE_CustomDate3.IsValid)
			{
				value.LineAttributes.CustomDate3 = line.WE_CustomDate3;
			}
			if (line.WE_CustomDate4.IsValid)
			{
				value.LineAttributes.CustomDate4 = line.WE_CustomDate4;
			}
			if (line.WE_CustomDate5.IsValid)
			{
				value.LineAttributes.CustomDate5 = line.WE_CustomDate5;
			}

			if (line.Docket.Client != null)
			{
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal1) != null)
				{
					value.LineAttributes.CustomDecimal1 = line.WE_CustomDecimal1;
					value.LineAttributes.CustomDecimal1Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal2) != null)
				{
					value.LineAttributes.CustomDecimal2 = line.WE_CustomDecimal2;
					value.LineAttributes.CustomDecimal2Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal3) != null)
				{
					value.LineAttributes.CustomDecimal3 = line.WE_CustomDecimal3;
					value.LineAttributes.CustomDecimal3Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal4) != null)
				{
					value.LineAttributes.CustomDecimal4 = line.WE_CustomDecimal4;
					value.LineAttributes.CustomDecimal4Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal5) != null)
				{
					value.LineAttributes.CustomDecimal5 = line.WE_CustomDecimal5;
					value.LineAttributes.CustomDecimal5Specified = true;
				}

				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag1) != null)
				{
					value.LineAttributes.CustomFlag1 = line.WE_CustomFlag1;
					value.LineAttributes.CustomFlag1Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag2) != null)
				{
					value.LineAttributes.CustomFlag2 = line.WE_CustomFlag2;
					value.LineAttributes.CustomFlag2Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag3) != null)
				{
					value.LineAttributes.CustomFlag3 = line.WE_CustomFlag3;
					value.LineAttributes.CustomFlag3Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag4) != null)
				{
					value.LineAttributes.CustomFlag4 = line.WE_CustomFlag4;
					value.LineAttributes.CustomFlag4Specified = true;
				}
				if (line.Docket.Client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag5) != null)
				{
					value.LineAttributes.CustomFlag5 = line.WE_CustomFlag5;
					value.LineAttributes.CustomFlag5Specified = true;
				}
			}
		}

		#endregion

		#region ExportCrossDockLines

		protected void ExportCrossDockLines(WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			var reservedPickLines = GetExportLineReservedPickLines(line);
			if (reservedPickLines != null)
			{
				foreach (var pickLine in reservedPickLines)
				{
					var xsdCrossDockLineValue = value.CrossDockLines.AddNew();
					xsdCrossDockLineValue.AllocationQty = pickLine.ReservedQuantity;
					xsdCrossDockLineValue.AllocationQtyUQ = line.SupplierPart.OP_StockKeepingUnit;
					SetExportLineReservedPickLineReferences(pickLine, xsdCrossDockLineValue);
				}
			}
		}

		#endregion

		#region SetExportLineReservedPickLineReferences

		protected virtual void SetExportLineReservedPickLineReferences(WhsPickLine reservedPickLine, Xsd.WhsDocketLineCrossDockLine xsdCrossDockLine)
		{
		}

		#endregion

		#region GetExportLineReservedPickLines

		protected virtual WhsPickLineCollection GetExportLineReservedPickLines(WhsDocketLine line)
		{
			return null;
		}

		#endregion

		#region ExportDocketLineAdditionalInfo

		protected virtual void ExportDocketLineAdditionalInfo(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
		}

		#endregion

		#region ExportAdditionalToValueObjectCore

		protected virtual void ExportAdditionalToValueObjectCore(TDocket bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
		}

		#endregion

		protected abstract ZString GetXsdWhsDocketType();

		#endregion
	}
}
