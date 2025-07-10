using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFFromShipmentCreator : NonPersistentBusinessObject, IObsoleteValidation, IISFFromShipmentCreator
	{
		#region Schema

		public static class Schema
		{
			public const string ShipmentPK = "ShipmentPK";
		}

		#endregion

		public ISFFromShipmentCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusISFHeader Create(BusinessObjectFactory factoryToCreateFor)
		{
			CusISFHeader result = null;
			ForwardingShipment shipmentInPassedInFactory = factoryToCreateFor.Load<ForwardingShipment>(ShipmentPK);
			if (shipmentInPassedInFactory != null)
			{
				result = factoryToCreateFor.New<CusISFHeader>();
				result.BF_ShipmentType = ShipmentType;

				var declaration = shipmentInPassedInFactory.DeclarationForDocuments as JobDeclaration;
				if (declaration != null)
				{
					CreateFromDeclaration(declaration, result);
				}
				else
				{
					CreateFromShipment(shipmentInPassedInFactory, result);
				}
				Dictionary<CusISFHeader, ForwardingShipment> dictionary = null;
				if (!TransferredLogsList.TryGetValue(factoryToCreateFor, out dictionary))
				{
					dictionary = new Dictionary<CusISFHeader, ForwardingShipment>();
					TransferredLogsList.Add(factoryToCreateFor, dictionary);
				}

				dictionary.Add(result, shipmentInPassedInFactory);
				factoryToCreateFor.Saving -= factoryToCreateFor_Saving;
				factoryToCreateFor.Saving += factoryToCreateFor_Saving;
				factoryToCreateFor.Saved -= factoryToCreateFor_Saved;
				factoryToCreateFor.Saved += factoryToCreateFor_Saved;
			}
			return result;
		}

		public static IISFFromShipmentCreator GetCreatorForShipment(ForwardingShipment shipment)
		{
			if (shipment.IsHighVolumeLowValue)
			{
				return ObjectFactory.Get<IISFFromShipmentCreator>("IUSImporterSecurityFilingCreator", shipment);
			}

			return new ISFFromShipmentCreator(shipment.Factory);
		}

		protected virtual ZString ShipmentType => ShipmentTypeList.Codes.StandardOrRegularFilings;

		Dictionary<BusinessObjectFactory, Dictionary<CusISFHeader, ForwardingShipment>> TransferredLogsList
		{
			get { return transferredLogsList ?? (transferredLogsList = new Dictionary<BusinessObjectFactory, Dictionary<CusISFHeader, ForwardingShipment>>()); }
		}
		Dictionary<BusinessObjectFactory, Dictionary<CusISFHeader, ForwardingShipment>> transferredLogsList;

		void factoryToCreateFor_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saving -= factoryToCreateFor_Saving;
				factory.Saved -= factoryToCreateFor_Saved;
			}
		}

		void factoryToCreateFor_Saving(BusinessObjectFactory factory)
		{
			OnFactorySaving(factory);
		}

		protected virtual void OnFactorySaving(BusinessObjectFactory factory)
		{
			if (transferredLogsList != null)
			{
				Dictionary<CusISFHeader, ForwardingShipment> dictionary = null;
				if (transferredLogsList.TryGetValue(factory, out dictionary))
				{
					foreach (var pair in dictionary)
					{
						if (pair.Key.BF_JobReference.IsEmpty)
						{
							pair.Key.PopulateJobReferenceIfNeeded();
						}
						AddTransferredLog(pair.Value, pair.Key.BF_JobReference);
					}
					transferredLogsList.Remove(factory);
				}
			}
		}

		void AddTransferredLog(ForwardingShipment shipment, ZString isfJobReference)
		{
			shipment.Logs.AddNew(Events.Transferred, "To " + isfJobReference);
		}

		public ForwardingShipment Shipment
		{
			get { return Factory.Load<ForwardingShipment>(ShipmentPK); }
		}

		public ForwardingModuleShipmentCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					var transportQuery = new ZQuery(JobShipmentSchema.JS_TransportMode, ValidTransportModes);

					transportQuery.DefaultJoinCondition = JoinCondition.Or;

					var query = new ZQuery(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.DoesNotStartWith, "US");
					query.AddToFilter(transportQuery);

					shipments = new ForwardingModuleShipmentCollection(Factory);
					shipments.AddRelationshipFilter(query);
				}

				return shipments;
			}
		}
		ForwardingModuleShipmentCollection shipments;

		#region ShipmentPK

		[List(nameof(Shipments))]
		public ZGuid ShipmentPK
		{
			get { return shipmentPK; }
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentPKInfo, ref shipmentPK, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateShipmentPK();
					}
				}
			}
		}
		ZGuid shipmentPK;

		public ZPropertyInfo ShipmentPKInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentPK); }
		}

		#endregion

		public void Reset()
		{
			ShipmentPK = ZGuid.Empty;
			ShipmentPKInfo.ClearAllNotifications();
		}

		public void ValidateShipmentPK()
		{
			ShipmentPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ShipmentPKInfo);
			if (!ShipmentPK.IsEmpty)
			{
				var shipment = Shipment;
				if (shipment == null
					|| shipment.JS_IsCancelled
					|| !ValidTransportModes.Contains(shipment.JS_TransportMode.ToString())
					|| shipment.JS_RL_NKOrigin.StartsWith(Constants.CountryCodes.UnitedStates, System.StringComparison.OrdinalIgnoreCase))
				{
					ShipmentPKInfo.AddError(ResString.GetMultilingualString("D66ACF55-2BA1-42CE-AA89-B69EFEB6A248", InvalidShipmentNumber));
				}
			}
		}

		internal const string InvalidShipmentNumber = "This shipment is not valid for creating a US Import Security Filing record, please choose a valid shipment number.";

		string[] ValidTransportModes => new[]
		{
			Constants.TransportModes.Sea,
			Constants.TransportModes.SeaAir,
			Constants.TransportModes.AirSea
		};

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateShipmentPK();
		}

		protected virtual void CreateFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			header.BF_OH_Importer = shipmentInPassedInFactory.ConsigneePK;
			var consol = GetConsolFromShipment(shipmentInPassedInFactory);
			if (consol != null)
			{
				ImportFromConsol(consol, header);
				
				var amsNumber = consol.GetUpperCaseAMSBill();
				var billNumber = !amsNumber.IsEmpty ? amsNumber : CreateBillNumberWithSCAC(consol, consol.JK_MasterBillNum, shipmentInPassedInFactory.TransportMode);

				if (consol.IsDirect)
				{
					header.BF_OceanBill = billNumber;
				}
				else
				{
					header.BF_MasterBill = billNumber;
					ImportHouseBillFromShipment(shipmentInPassedInFactory, header);
				}
			}
			else
			{
				ImportHouseBillFromShipment(shipmentInPassedInFactory, header);
				ImportTransportDetails(shipmentInPassedInFactory.Transports, header);
			}
			if (!shipmentInPassedInFactory.ConsigneeDeliveryAddress.IsEmpty)
			{
				ImportOrganization(shipmentInPassedInFactory.ConsigneeDeliveryAddress, header.MainShipToParty);
			}
			if (!shipmentInPassedInFactory.BuyerDocAddress.IsEmpty)
			{
				ImportOrganization(shipmentInPassedInFactory.BuyerDocAddress, header.BuyingParty);
			}
			else if (!shipmentInPassedInFactory.ConsigneeDocumentaryAddress.IsEmpty)
			{
				ImportOrganization(shipmentInPassedInFactory.ConsigneeDocumentaryAddress, header.BuyingParty);
			}
			CreateSellers(header, shipmentInPassedInFactory, consol, null);
			CreateManufacturers(header, shipmentInPassedInFactory, consol, null);
			ImportContainersFromShipment(consol, shipmentInPassedInFactory, header);
			ImportLowValueDetailsFromShipment(shipmentInPassedInFactory, header);
			ImportLinesFromShipment(shipmentInPassedInFactory, header);
			header.BF_RL_NKPlaceOfDelivery = shipmentInPassedInFactory.JS_RL_NKDestination;

			ImportImporterAndConsigneeCodeTypeFromShipment(shipmentInPassedInFactory, header);
		}

		protected virtual void ImportImporterAndConsigneeCodeTypeFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
		}
		protected virtual void ImportLowValueDetailsFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			header.BF_EstimatedQuantity = shipmentInPassedInFactory.JS_TotalPackageCount;
			header.BF_EstimatedValue = CurrencyConverter.ConvertExact(new Money(shipmentInPassedInFactory.JS_GoodsValue, shipmentInPassedInFactory.GoodsValueCurr), USD).Amount;
			header.BF_EstimatedWeight = shipmentInPassedInFactory.JS_ActualWeight.Round(0).ToZInt();
			header.BF_EstimatedWeightUQ = shipmentInPassedInFactory.JS_UnitOfWeight;
		}

		RefCurrency USD
		{
			get { return usd ?? (usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates)); }
		}
		RefCurrency usd;

		RefCurrencyCurrencyConverter CurrencyConverter
		{
			get { return currencyConverter ?? (currencyConverter = new RefCurrencyCurrencyConverter(Factory, ZDateTime.Today, ExchangeRateType.Customs, 365)); }
		}
		RefCurrencyCurrencyConverter currencyConverter;

		void ImportHouseBillFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			if (!(shipmentInPassedInFactory.IsMasterShipmentRepresentingAllChildShipments || shipmentInPassedInFactory.IsHighVolumeLowValue))
			{
				var amsNumber = shipmentInPassedInFactory.GetUpperCaseAMSBill();
				var billNumber = !amsNumber.IsEmpty
					? amsNumber
					: CreateBillNumberWithSCAC(shipmentInPassedInFactory, shipmentInPassedInFactory.JS_HouseBill, shipmentInPassedInFactory.TransportMode);

				header.BF_HouseBill = billNumber;
			}

			ImportReferenceDataFromShipment(shipmentInPassedInFactory, header);
		}

		protected virtual void ImportReferenceDataFromShipment(ForwardingShipment shipmentInPassedInFactory, CusISFHeader header)
		{
			foreach (ForwardingShipment subShipment in shipmentInPassedInFactory.CoLoadShipments)
			{
				if (!subShipment.JS_HouseBill.IsEmpty)
				{
					var amsNumber = subShipment.GetUpperCaseAMSBill();
					var billNumber = !amsNumber.IsEmpty
						? amsNumber
						: CreateBillNumberWithSCAC(subShipment, subShipment.JS_HouseBill, shipmentInPassedInFactory.TransportMode);

					if (header.BF_HouseBill.IsEmpty)
					{
						header.BF_HouseBill = billNumber;
					}
					else
					{
						CreateReferenceData(header, BillTypeList.Codes.HouseBillOfLading, billNumber);
					}
				}
			}
		}

		protected virtual void ImportLinesFromShipment(ForwardingShipment shipment, CusISFHeader header)
		{
			foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
			{
				CreateLine(header, null, ZString.Empty, packLine.JL_HarmonisedCode, packLine.JL_RN_NKOrigin);
			}
		}

		protected virtual void ImportContainersFromShipment(ForwardingConsol consol, ForwardingShipment shipment, CusISFHeader header)
		{
			if (consol != null)
			{
				foreach (ForwardingContainer container in shipment.Containers)
				{
					if (consol.Containers.Contains(container))
					{
						CreateEquipment(header, container.JC_ContainerNum, container.Container);
					}
				}
			}
		}

		protected virtual void ImportFromConsol(ForwardingConsol consol, CusISFHeader header)
		{
			OrgHeader shippingLine = consol.ShippingLine;
			if (shippingLine != null)
			{
				header.BF_SCAC = shippingLine.USLocalCustomsCarrierCode();
			}
			ImportTransportDetails(consol.Transports, header);
			ImportOrganization(consol.PackDepotAddress, header.StuffingLocation);
			var shipper = Shipment.ConsignorPK;
			if (consol.JK_AgentType == Constants.AgentType.CoLoad && consol.CreditorAddress != null)
			{
				ImportOrganization(consol.CreditorAddress, header.Consolidator);
			}
			else if (!shipper.IsEmpty && consol.JK_OA_PackDepotAddress_ZAddress.OrgPK == shipper)
			{
				ImportOrganization(consol.PackDepotAddress, header.Consolidator);
			}
			else
			{
				ImportOrganization(consol.SendingForwarder, header.Consolidator);
			}
			header.BF_RL_NKPortOfUnload = consol.JK_RL_NKDischargePort;
		}

		void ImportTransportDetails(TransportCollection transports, CusISFHeader header)
		{
			foreach (Transport sourceTransport in transports)
			{
				CreateTransport(sourceTransport, header);
			}
		}

		void CreateTransport(Transport sourceTransport, CusISFHeader header)
		{
			BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(new string[]
			{
				Transport.Schema.JW_ParentGUID,
				Transport.Schema.JW_ParentType,
			});

			Transport transport = (Transport)sourceTransport.Clone(args);
			transport.ParentType = typeof(CusISFHeader);
			transport.JW_ParentType = Core.Constants.TransportParentTypes.ImporterSecurityFiling;
			transport.JW_ParentGUID = header.PK;
		}

		void CreateFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			header.BF_OH_Importer = declaration.JE_OH_Importer;
			header.BF_SCAC = declaration.US_UI_NKCarrierSCAC.Left(header.BF_SCACInfo.MaxLength);
			header.BF_OwnerReference = declaration.JE_OwnerRef.Left(header.BF_OwnerReferenceInfo.MaxLength);
			CreateBillsFromDeclaration(declaration, header);
			if (!declaration.ImportEntryNumber.IsEmpty)
			{
				ZString entryNumber = declaration.US_EntryFilerCode + declaration.ImportEntryNumber;
				header.BF_EntryNumber = entryNumber.Left(header.BF_EntryNumberInfo.MaxLength);
			}
			ForwardingShipment shipment = declaration.Shipment;
			ImportConsigneeDetailsFromDeclaration(declaration, header);
			if (!declaration.ImporterDeliveryAddress.IsEmpty)
			{
				ImportOrganization(declaration.ImporterDeliveryAddress, header.MainShipToParty);
			}
			ImportOrganization(declaration.Buyer, header.BuyingParty);
			if (header.BuyingParty.IsEmpty)
			{
				if (!shipment.BuyerDocAddress.IsEmpty)
				{
					ImportOrganization(shipment.BuyerDocAddress, header.BuyingParty);
				}
				else if (!shipment.ConsigneeDocumentaryAddress.IsEmpty)
				{
					ImportOrganization(shipment.ConsigneeDocumentaryAddress, header.BuyingParty);
				}
			}
			ForwardingConsol consol = GetConsolFromShipment(shipment);
			CreateSellers(header, shipment, consol, declaration);
			if (consol != null)
			{
				ImportOrganization(consol.PackDepotAddress, header.StuffingLocation);
				ImportOrganization(consol.SendingForwarder, header.Consolidator);
			}
			if (!declaration.US_SuretyCode.IsEmpty && header.IsBond16SingleTransaction)
			{
				header.BF_SuretyCode = declaration.US_SuretyCode.Left(header.BF_SuretyCodeInfo.MaxLength);
			}
			ImportTransportDetailsFromDeclaration(declaration, header);
			ImportContainersFromDeclaration(declaration, header);
			ImportLowValueDetailsFromDeclaration(declaration, header);
			CreateManufacturers(header, shipment, consol, declaration);
			ImportLinesFromDeclaration(declaration, header);
			header.BF_RL_NKPortOfUnload = declaration.JE_RL_NKPortOfArrival;
			header.BF_RL_NKPlaceOfDelivery = declaration.JE_RL_NKFinalDestination;
		}

		void ImportLowValueDetailsFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			header.BF_EstimatedQuantity = declaration.JE_TotalNoOfPacks;
			header.BF_EstimatedQuantityUQ = declaration.JE_TotalNoOfPacksPackType;
			header.BF_EstimatedValue = declaration.TotalFOBInLocalCurrency.Amount;
			header.BF_EstimatedWeight = declaration.JE_TotalWeight.Round(0).ToZInt();
			header.BF_EstimatedWeightUQ = declaration.JE_TotalWeightUnit;
		}

		ForwardingConsol GetConsolFromShipment(ForwardingShipment shipment)
		{
			return Enterprise.Customs.Business.ShipmentExtensions.ConsolForCountry(shipment, Core.Constants.CountryCodes.UnitedStates);
		}

		void ImportLinesFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			Dictionary<ZGuid, JobDocAddress> manufacturers = new Dictionary<ZGuid, JobDocAddress>();
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				JobDocAddress manufacturerDocAddress = null;
				OrgAddress manufacturerAddress = invoiceLine.ManufacturerAddress;
				if (manufacturerAddress != null)
				{
					manufacturerDocAddress = header.ManufacturerAddresses.FirstOrDefault(x => x.E2_OA_Address == manufacturerAddress.PK);
				}
				CreateLine(header, manufacturerDocAddress, invoiceLine.JI_PartNo, invoiceLine.JI_Tariff, invoiceLine.US_UC_NKCountryOfOrigin);
			}
		}

		void CreateLine(CusISFHeader header, JobDocAddress manufacturerDocAddress, ZString partNo, ZString tariff, ZString countryOfOrigin)
		{
			if (manufacturerDocAddress != null || !partNo.IsEmpty || !tariff.IsEmpty || !countryOfOrigin.IsEmpty)
			{
				ZQuery query = new ZQuery(CusISFLineSchema.BL_TextProductCode, partNo);
				if (manufacturerDocAddress != null)
				{
					query.AddToFilter(CusISFLineSchema.BL_ManufacturerDocAddressPK, manufacturerDocAddress.PK);
				}
				else
				{
					query.AddToFilter(CusISFLineSchema.BL_ManufacturerDocAddressPK, null);
				}
				query.AddToFilter(CusISFLineSchema.BL_HarmonisedNum, tariff);
				query.AddToFilter(CusISFLineSchema.BL_RN_NKGoodsOrigin, countryOfOrigin);
				List<CusISFLine> list = new List<CusISFLine>(header.Lines.Find(query));
				if (list.Count == 0)
				{
					CusISFLine line = header.Lines.AddNew();
					line.BL_ManufacturerDocAddressPK = (manufacturerDocAddress != null) ? manufacturerDocAddress.PK : ZGuid.Empty;
					line.BL_TextProductCode = partNo;
					line.BL_HarmonisedNum = tariff;
					line.BL_RN_NKGoodsOrigin = countryOfOrigin;
				}
			}
		}

		void ImportContainersFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			foreach (CusContainer container in declaration.CusContainers)
			{
				CreateEquipment(header, container.CO_ContainerNumber, container.Container);
			}
		}

		void CreateEquipment(CusISFHeader header, ZString containerNumber, RefContainer containerType)
		{
			if (!containerNumber.IsEmpty || containerType != null)
			{
				CusISFEquip equipment = header.Equipments.AddNew();
				equipment.BE_ContainerNum = containerNumber;
				if (containerType != null)
				{
					equipment.BE_ContainerISO = containerType.RC_ISOType;
					equipment.BE_EquipCode = containerType.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				}
			}
		}

		void ImportTransportDetailsFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			Transport transport = header.Transports.AddNew();
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = declaration.JE_VesselName;
			transport.JW_VoyageFlight = declaration.JE_VoyageFlightNo;
			transport.JW_RL_NKLoadPort = declaration.JE_RL_NKPortOfLoading;
			transport.JW_RL_NKDiscPort = declaration.JE_RL_NKPortOfArrival;
			transport.JW_ETD = declaration.JE_ExportDate;
			transport.JW_ETA = declaration.JE_DateOfArrival;
			if (declaration.ShippingLine != null)
			{
				transport.CarrierPK = declaration.JE_OH_ShippingLine;
			}
		}

		void ImportOrganization(JobDocAddress sourceDocAddress, JobDocAddress destinationDocAddress)
		{
			BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentTableCode, JobDocAddress.Schema.E2_ParentID, JobDocAddress.Schema.E2_AddressSequence, JobDocAddress.Schema.E2_AddressType, JobDocAddress.Schema.E2_AddressOverride });
			destinationDocAddress.E2_AddressOverride = sourceDocAddress.E2_AddressOverride;
			destinationDocAddress.CopyPersistentValuesFrom(sourceDocAddress, args);
		}

		void ImportOrganization(OrgHeader org, JobDocAddress docAddress)
		{
			if (org != null)
			{
				ImportOrganization(org.MainAddress, docAddress);
			}
		}

		void ImportOrganization(OrgAddress orgAddress, JobDocAddress docAddress)
		{
			if (orgAddress != null)
			{
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = orgAddress.PK;
			}
		}

		void ImportConsigneeDetailsFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			ZString consigneeID = declaration.UltimateConsigneeCustomsClientNumber;
			if (!consigneeID.IsEmpty)
			{
				header.BF_ConsigneeCodeType = CodeTypeList.Codes.IRS;
				header.BF_ConsigneeCode = consigneeID;
			}
		}

		void CreateBillsFromDeclaration(JobDeclaration declaration, CusISFHeader header)
		{
			if (declaration.JE_HouseBill.IsEmpty)
			{
				header.BF_OceanBill = GetBillWithSCAC(declaration.JE_MasterBill, declaration.JE_MasterBillIssuerSCAC);
				foreach (Bill bill in declaration.Bills.FindByBillType(Customs.Business.BillTypeList.Codes.MasterBill))
				{
					if (bill.CU_BillNum != declaration.JE_MasterBill)
					{
						var billWithSCAC = GetBillWithSCAC(bill.CU_BillNum, bill.US_UI_NKBillIssuerSCAC);
						CreateReferenceData(header, BillTypeList.Codes.OceanBillOfLading, billWithSCAC);
					}
				}
			}
			else
			{
				header.BF_HouseBill = GetBillWithSCAC(declaration.JE_HouseBill, declaration.JE_HouseBillIssuerSCAC);
				header.BF_MasterBill = GetBillWithSCAC(declaration.JE_MasterBill, declaration.JE_MasterBillIssuerSCAC);
				foreach (Bill bill in declaration.Bills)
				{
					var billWithSCAC = GetBillWithSCAC(bill.CU_BillNum, bill.US_UI_NKBillIssuerSCAC);

					if (bill.IsHouseBill && bill.CU_BillNum != declaration.JE_HouseBill)
					{
						CreateReferenceData(header, BillTypeList.Codes.HouseBillOfLading, billWithSCAC);
					}
					else if (bill.IsMasterBill && bill.CU_BillNum != declaration.JE_MasterBill)
					{
						CreateReferenceData(header, BillTypeList.Codes.MasterBillOfLading, billWithSCAC);
					}
				}
			}
		}

		protected ZString CreateBillNumberWithSCAC(IBillDetails billDetails, ZString originalBillNumber, ZString transportMode)
		{
			var billNumber = originalBillNumber.KeepValidBillNumberCharacters().ToUpper();
			var validSCACs = billDetails.GetValidSCACIssuerCodes(transportMode);
			var scac = billNumber.GetSCAC(validSCACs);

			billNumber = billNumber.ShouldTrimSCACFromBills(validSCACs) ? billNumber.GetBillNumberTrimSCAC() : billNumber;
			return GetBillWithSCAC(billNumber, scac);
		}

		ZString GetBillWithSCAC(ZString billNumber, ZString scac)
		{
			ZString result = scac.Trim() + billNumber;
			return result.Left(CusISFBill.Schema.BB_BillNumMaxLength);
		}

		void CreateReferenceData(CusISFHeader header, ZString referenceType, ZString referenceID)
		{
			if (!referenceType.IsEmpty || !referenceID.IsEmpty)
			{
				CusISFBill referenceData = header.ReferenceDatas.AddNew();
				referenceData.BB_BillType = referenceType;
				referenceData.BB_BillNum = referenceID;
			}
		}

		#region Create Manufacturer

		protected virtual void CreateManufacturers(CusISFHeader header, ForwardingShipment shipment, ForwardingConsol consol, JobDeclaration declaration)
		{
			AddManufacturer(header, !shipment.ManufacturerDocAddress.IsEmpty ? shipment.ManufacturerDocAddress : shipment.ConsignorDocumentaryAddress);

			if (declaration != null)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					AddManufacturer(header, invoiceLine.ManufacturerAddress);
				}
			}
			if (consol != null && consol.IsBuyersConsol)
			{
				foreach (ForwardingShipment coloadShipment in shipment.CoLoadShipments)
				{
					if (!coloadShipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						AddManufacturer(header, !coloadShipment.ManufacturerDocAddress.IsEmpty ? coloadShipment.ManufacturerDocAddress : coloadShipment.ConsignorDocumentaryAddress);
					}
				}
				if (!shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					AddManufacturer(header, !shipment.ManufacturerDocAddress.IsEmpty ? shipment.ManufacturerDocAddress : shipment.ConsignorDocumentaryAddress);
				}
			}
			if (header.ManufacturerAddresses.Count == 0)
			{
				AddManufacturer(header, header.SellingParty);
			}
		}

		void AddManufacturer(CusISFHeader header, OrgAddress addressToAdd)
		{
			if (addressToAdd != null)
			{
				foreach (var manufacturer in header.ManufacturerAddresses)
				{
					if (manufacturer.E2_OA_Address == addressToAdd.PK)
					{
						return;
					}
				}
				ImportOrganization(addressToAdd, header.ManufacturerAddresses.AddNew());
			}
		}

		void AddManufacturer(CusISFHeader header, JobDocAddress docToAdd)
		{
			if (!docToAdd.IsEmpty)
			{
				foreach (var manufacturer in header.ManufacturerAddresses)
				{
					if (manufacturer.IsMatchedAgainst(docToAdd))
					{
						return;
					}
				}
				ImportOrganization(docToAdd, header.ManufacturerAddresses.AddNew());
			}
		}

		#endregion

		#region Create Seller

		void CreateSellers(CusISFHeader header, ForwardingShipment shipment, ForwardingConsol consol, JobDeclaration declaration)
		{
			if (declaration != null)
			{
				AddSeller(header, declaration.Seller);
			}

			if (header.SellingParty.IsEmpty && !shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				AddSeller(header, shipment.ConsignorDocumentaryAddress);
			}

			if (consol != null && consol.IsBuyersConsol)
			{
				foreach (ForwardingShipment coloadShipment in shipment.CoLoadShipments)
				{
					if (!coloadShipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						AddSeller(header, coloadShipment.ConsignorDocumentaryAddress);
					}
				}
			}
		}

		void AddSeller(CusISFHeader header, OrgHeader orgToAdd)
		{
			if (orgToAdd != null)
			{
				foreach (var seller in header.DocAddresses.FindDocAddressesByType(MasterFiles.Integration.DocAddressType.SellingParty))
				{
					if (!seller.E2_AddressOverride && seller.OrganisationPK == orgToAdd.PK)
					{
						return;
					}
				}
				ImportOrganization(orgToAdd, GetNewSeller(header));
			}
		}

		void AddSeller(CusISFHeader header, JobDocAddress docToAdd)
		{
			if (docToAdd != null && !docToAdd.IsEmpty)
			{
				foreach (var seller in header.DocAddresses.FindDocAddressesByType(MasterFiles.Integration.DocAddressType.SellingParty))
				{
					if (seller.IsMatchedAgainst(docToAdd))
					{
						return;
					}
				}
				ImportOrganization(docToAdd, GetNewSeller(header));
			}
		}

		JobDocAddress GetNewSeller(CusISFHeader header)
		{
			var result = header.SellingParty;
			if (!result.IsEmpty)
			{
				result = header.DocAddresses.CreateWithAddressType(MasterFiles.Integration.DocAddressType.SellingParty);
			}
			return result;
		}

		#endregion
	}
}

#region Implementation
#endregion
