using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class JobDeclarationCreator
	{
		public JobDeclarationCreator(ZGuid headerPK)
		{
			this.headerPK = headerPK;
			if (Header == null)
			{
				throw new ArgumentException("CusISFHeader with PK = '" + headerPK.ToString() + "' does not exist", nameof(headerPK));
			}
		}
		readonly ZGuid headerPK;

		public JobDeclaration CreateDeclaration()
		{
			factory = null;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.JE_TotalNoOfPacksPackType = "PK";
			UpdateImporter();
			UpdateSoldToParty();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			if (!header.MainShipToParty.IsEmpty)
			{
				UpdateOrganisation(header.MainShipToParty, Declaration.ImporterDeliveryAddress);
			}
			UpdateConsignee();
			UpdateOrganisation(Declaration.JE_OH_BuyerInfo, Header.BuyingParty);
			UpdateOrgAddress(Declaration.JE_OA_ShipToPartyAddressInfo, Header.MainShipToParty);
			UpdateOrgAddress(Declaration.JE_OA_SellerAddressInfo, Header.SellingParty);
			UpdateTransportDetails();
			Declaration.JE_OwnerRef = Header.BF_OwnerReference.Left(Declaration.JE_OwnerRefInfo.MaxLength);
			UpdateContainers();
			UpdateBills();
			Declaration.US_UI_NKCarrierSCAC = Header.BF_SCAC;
			UpdateLines();
			UpdateLowValueDetails();
			CopyeDocsToDeclaration();
			var dictionary = new Dictionary<JobDeclaration, CusISFHeader>();
			dictionary.Add(Declaration, Header);
			TransferredLogsList.Add(Factory, dictionary);
			Factory.Saving -= Factory_Saving;
			Factory.Saving += Factory_Saving;
			Factory.Saved -= Factory_Saved;
			Factory.Saved += Factory_Saved;

			return Declaration;
		}

		void CopyeDocsToDeclaration()
		{
			var docManagerInfo = ((IDocManagerSupport)Header).DocManagerInfo;
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			var eDocs = docManagerInfo.EDocsView;
			Declaration.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			Declaration.ShouldSaveEDocsMasterFactoryTogether = eDocs != null && eDocs.Count > 0;
			foreach (IeDoc document in eDocs)
			{
				if (!document.IsCustomisableDocTypes)
				{
					Declaration.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType);
				}
				else
				{
					Declaration.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType, description: document.Description);
				}
			}
		}

		void UpdateLowValueDetails()
		{
			Declaration.JE_TotalNoOfPacks = Header.BF_EstimatedQuantity;
			Declaration.JE_TotalNoOfPacksPackType = Header.BF_EstimatedQuantityUQ;
			Declaration.JE_TotalWeight = (decimal)Header.BF_EstimatedWeight;
			Declaration.JE_TotalWeightUnit = Header.BF_EstimatedWeightUQ;
		}

		void UpdateTransportDetails()
		{
			Transport mainTransport = null;
			foreach (Transport transport in Header.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					mainTransport = transport;
					break;
				}
				if (mainTransport == null)
				{
					mainTransport = transport;
				}
			}
			if (mainTransport != null)
			{
				Declaration.JE_VesselName = mainTransport.JW_Vessel;
				Declaration.JE_VoyageFlightNo = mainTransport.JW_VoyageFlight;
				Declaration.JE_RL_NKPortOfLoading = mainTransport.JW_RL_NKLoadPort;
				Declaration.JE_RL_NKPortOfArrival = mainTransport.JW_RL_NKDiscPort;
				Declaration.JE_ExportDate = mainTransport.JW_ATD.IsEmpty ? mainTransport.JW_ETD : mainTransport.JW_ATD;
				Declaration.JE_DateOfArrival = mainTransport.JW_ATA.IsEmpty ? mainTransport.JW_ETA : mainTransport.JW_ATA;
				if (mainTransport.Carrier != null)
				{
					Declaration.JE_OH_ShippingLine = mainTransport.CarrierPK;
				}
			}
		}

		void UpdateOrganisation(ZPropertyInfo orgInfo, ISFDocAddress docAddress)
		{
			if (docAddress.HasRealAddress)
			{
				orgInfo.Value = docAddress.OrganisationPK;
			}
		}

		void UpdateOrgAddress(ZPropertyInfo orgInfo, ISFDocAddress docAddress)
		{
			if (docAddress.HasRealAddress)
			{
				orgInfo.Value = docAddress.Address.PK;
			}
		}

		void UpdateSoldToParty()
		{
			if (Declaration.JE_OA_SoldToPartyAddress.IsEmpty && Header.BuyingParty is ISFDocAddress buyerAddress && !buyerAddress.E2_OA_Address.IsEmpty)
			{
				Declaration.JE_OA_SoldToPartyAddress = buyerAddress.E2_OA_Address;
			}
		}

		void UpdateOrganisation(JobDocAddress sourceDocAddress, JobDocAddress destinationDocAddress)
		{
			var args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentTableCode, JobDocAddress.Schema.E2_ParentID, JobDocAddress.Schema.E2_AddressSequence, JobDocAddress.Schema.E2_AddressType, JobDocAddress.Schema.E2_AddressOverride });
			destinationDocAddress.E2_AddressOverride = sourceDocAddress.E2_AddressOverride;
			destinationDocAddress.CopyPersistentValuesFrom(sourceDocAddress, args);
		}

		void UpdateLines()
		{
			JobComInvoiceHeader invoice = null;
			invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = Header.BF_EstimatedValue;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			foreach (var line in Header.Lines.Where(l => l.BL_BL_Parent.IsEmpty))
			{
				if (invoice == null)
				{
					invoice = Declaration.Invoices.AddNew();
				}

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = line.BL_TextProductCode;
				invoiceLine.JI_PartAttrib1 = line.BL_PartAttrib1;
				invoiceLine.JI_PartAttrib2 = line.BL_PartAttrib2;
				invoiceLine.JI_PartAttrib3 = line.BL_PartAttrib3;

				var hasMatchTariffOnProduct = false;
				if (line.Pivot != null)
				{
					hasMatchTariffOnProduct = line.Pivot.TariffNumber == line.BL_HarmonisedNum;

					if (!hasMatchTariffOnProduct && line.Pivot.Children.Count > 0)
					{
						hasMatchTariffOnProduct = line.Pivot.Children.Find(x => x.TariffNumber == line.BL_HarmonisedNum).Any();
					}
				}

				UpdateLine(invoiceLine, line, !hasMatchTariffOnProduct);

				var childLines = invoiceLine.ChildLines.ToList();
				foreach (var isfChildLine in line.ChildLines)
				{
					if (!UpdateLineByMatchingISFLine(isfChildLine, childLines))
					{
						UpdateLine(AddSecondaryInvoiceLine(invoiceLine), isfChildLine, true);
					}
				}

				var productRelatedLines = invoiceLine.ProductRelatedLines.ToList();
				foreach (var isfProductRelatedLine in line.ProductRelatedLines)
				{
					if (!UpdateLineByMatchingISFLine(isfProductRelatedLine, productRelatedLines))
					{
						UpdateLine(AddProductRelatedInvoiceLine(invoiceLine), isfProductRelatedLine, true);
					}
				}
			}
		}

		void UpdateLine(JobComInvoiceLine invoiceLine, CusISFLine line, bool shouldUpdateTariff)
		{
			if (shouldUpdateTariff)
			{
				invoiceLine.JI_Tariff = line.BL_HarmonisedNum;
			}

			var docAddress = line.ManufacturerDocAddress;
			if (docAddress != null && docAddress.HasRealAddress)
			{
				invoiceLine.JI_OA_ManufacturerAddress = docAddress.E2_OA_Address;
			}

			if (!line.BL_RN_NKGoodsOrigin.IsEmpty && !invoiceLine.IsChildLine)
			{
				invoiceLine.US_UC_NKCountryOfOrigin = line.BL_RN_NKGoodsOrigin;
			}
		}

		bool UpdateLineByMatchingISFLine(CusISFLine line, List<JobComInvoiceLine> invoiceLines)
		{
			var foundMatchingLine = false;
			var tariff = line.BL_HarmonisedNum.Replace(".", "");
			var matchingLine = invoiceLines.FirstOrDefault(l => l.JI_Tariff == tariff);
			if (matchingLine != null)
			{
				UpdateLine(matchingLine, line, true);
				invoiceLines.Remove(matchingLine);
				foundMatchingLine = true;
			}

			return foundMatchingLine;
		}

		#region Just for CMR

		public JobComInvoiceLine AddSecondaryInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			JobComInvoiceLine result = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			result.JI_ParentID = invoiceLine.PK;
			return result;
		}

		public JobComInvoiceLine AddProductRelatedInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			JobComInvoiceLine result = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			result.US_JI_ParentProduct = invoiceLine.PK;
			return result;
		}

		#endregion

		void UpdateBills()
		{
			const int SCAClength = 4;
			if (!Header.BF_MasterBill.IsEmpty)
			{
				Declaration.JE_MasterBill = Header.BF_MasterBill.SubstringSafe(SCAClength, Declaration.JE_MasterBillInfo.MaxLength);
				Declaration.JE_MasterBillIssuerSCAC = Header.BF_MasterBill.Left(SCAClength);
			}
			else if (!Header.BF_OceanBill.IsEmpty)
			{
				Declaration.JE_MasterBill = Header.BF_OceanBill.SubstringSafe(SCAClength, Declaration.JE_MasterBillInfo.MaxLength);
				Declaration.JE_MasterBillIssuerSCAC = Header.BF_OceanBill.Left(SCAClength);
			}

			if (!Header.BF_HouseBill.IsEmpty)
			{
				Declaration.JE_HouseBill = Header.BF_HouseBill.SubstringSafe(SCAClength, Declaration.JE_HouseBillInfo.MaxLength);
				Declaration.JE_HouseBillIssuerSCAC = Header.BF_HouseBill.Left(SCAClength);
			}

			foreach (CusISFBill reference in Header.ReferenceDatas)
			{
				if ((reference.IsOceanBillOfLading && reference.BB_BillNum != Header.BF_OceanBill) || (reference.IsHouseBillOfLading && reference.BB_BillNum != Header.BF_HouseBill) || (reference.IsMasterBillOfLading && reference.BB_BillNum != Header.BF_MasterBill))
				{
					Bill bill = Declaration.Bills.AddNew();
					bill.CU_BillType = GetBillType(reference.BB_BillType);
					bill.CU_BillNum = reference.BB_BillNum.SubstringSafe(SCAClength, bill.CU_BillNumInfo.MaxLength);
					bill.US_UI_NKBillIssuerSCAC = reference.BB_BillNum.Left(SCAClength);
				}
			}
		}

		ZString GetBillType(ZString isfBillType)
		{
			switch (isfBillType)
			{
				case BillTypeList.Codes.HouseBillOfLading:
					return Customs.Business.BillTypeList.Codes.HouseBill;
				case BillTypeList.Codes.OceanBillOfLading:
				case BillTypeList.Codes.MasterBillOfLading:
					return Customs.Business.BillTypeList.Codes.MasterBill;
				default:
					return ZString.Empty;
			}
		}

		void UpdateConsignee()
		{
			var consigneeCodeType = Header.BF_ConsigneeCodeType;
			var consigneeCode = Header.BF_ConsigneeCode;
			if (!consigneeCodeType.IsEmpty && !consigneeCode.IsEmpty)
			{
				var orgHeader = Declaration.ConsigneeOrgAddress;
				var shouldUpdateConsignee = orgHeader == null;
				if (!shouldUpdateConsignee)
				{
					var customsRegNo = orgHeader.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, consigneeCodeType);
					shouldUpdateConsignee = customsRegNo != consigneeCode;
				}

				if (shouldUpdateConsignee)
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));

					var orgRegoQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
					orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, consigneeCodeType);
					orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, consigneeCode);
					query.AddSubQuery(orgRegoQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
					query.OrderBy = OrgHeaderSchema.Constants.OH_SystemCreateTimeUtc + " desc";
					var consignee = Factory.LoadTop1<OrgHeader>(query);
					if (consignee != null)
					{
						Declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;
					}
				}
			}
		}

		void UpdateImporter()
		{
			OrgHeader importer = Header.Importer;
			if (importer == null && !Header.BF_ImporterCodeType.IsEmpty && !Header.BF_ImporterCode.IsEmpty)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery orgRegoQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, Header.BF_ImporterCodeType);
				orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Header.BF_ImporterCode);
				query.AddSubQuery(orgRegoQuery, JoinCondition.And);
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				importer = Factory.LoadTop1<OrgHeader>(query);
			}

			if (importer != null && importer.OH_IsActive)
			{
				Declaration.JE_OH_Importer = importer.PK;
			}
		}

		void UpdateContainers()
		{
			Declaration.JE_ContainerMode = Header.BF_TransportMode == TransportModeCodes.Codes.OceanVesselContainerized ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
			foreach (CusISFEquip equipment in Header.Equipments)
			{
				CusContainer container = Declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = equipment.BE_ContainerNum;
				RefContainer containerType = null;
				if (!equipment.BE_ContainerISO.IsEmpty)
				{
					containerType = GetContainerTypeMatchingISOCode(equipment.BE_ContainerISO);
				}

				if (containerType == null && !equipment.BE_EquipCode.IsEmpty)
				{
					containerType = GetContainerTypeMatchingEquipmentCode(equipment.BE_EquipCode);
				}

				if (containerType != null)
				{
					container.CO_RC = containerType.PK;
				}
			}
		}

		RefContainer GetContainerTypeMatchingISOCode(ZString isoCode)
		{
			RefContainer result = null;
			if (!isoCode.IsEmpty)
			{
				ZQuery query = new ZQuery(RefContainerSchema.RC_ISOType, isoCode);
				result = Factory.LoadTop1<RefContainer>(query);
			}
			return result;
		}

		RefContainer GetContainerTypeMatchingEquipmentCode(ZString equipCode)
		{
			RefContainer result = null;
			if (!equipCode.IsEmpty)
			{
				if (!ContainerTypeMatchingList.TryGetValue(equipCode, out result))
				{
					var query = new ZDBOnlyQuery(typeof(RefContainer));
					query.AddSubQuery(RefContainerSchema.PK, RefContainer.GetContainerCodeFilter(Core.Constants.CountryCodes.UnitedStates, equipCode), JoinCondition.And);
					result = Factory.LoadTop1<RefContainer>(query);
					ContainerTypeMatchingList.Add(equipCode, result);
				}
			}
			return result;
		}

		Dictionary<ZString, RefContainer> ContainerTypeMatchingList
		{
			get { return containerTypeMatchingList ?? (containerTypeMatchingList = new Dictionary<ZString, RefContainer>()); }
		}
		Dictionary<ZString, RefContainer> containerTypeMatchingList;

		CusISFHeader Header
		{
			get
			{
				if (header == null || header.Factory != Factory)
				{
					header = Factory.Load<CusISFHeader>(headerPK);
				}
				return header;
			}
		}
		CusISFHeader header;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null || declaration.Factory != Factory)
				{
					declaration = Factory.New<JobDeclaration>();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		Dictionary<BusinessObjectFactory, Dictionary<JobDeclaration, CusISFHeader>> TransferredLogsList
		{
			get { return transferredLogsList ?? (transferredLogsList = new Dictionary<BusinessObjectFactory, Dictionary<JobDeclaration, CusISFHeader>>()); }
		}
		Dictionary<BusinessObjectFactory, Dictionary<JobDeclaration, CusISFHeader>> transferredLogsList;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saving -= Factory_Saving;
				factory.Saved -= Factory_Saved;
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (transferredLogsList != null)
			{
				Dictionary<JobDeclaration, CusISFHeader> dictionary = null;
				if (transferredLogsList.TryGetValue(factory, out dictionary))
				{
					foreach (var pair in dictionary)
					{
						if (pair.Key.JE_DeclarationReference.IsEmpty)
						{
							pair.Key.PopulateJE_DeclarationReferenceIfNeeded();
						}
						pair.Value.Logs.AddNew(Events.TransferToCustomsImportsDec, pair.Key.JE_DeclarationReference);
					}
					transferredLogsList.Remove(factory);
				}
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
