using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationValueObjectDataAdapter : CustomsBusinessObjectValueObjectDataAdapter<BaseJobDeclaration, Xsd.ConsolAndShipment>, Integration.Customs.DataTransfer.IDeclarationValueObjectDataAdapter
	{
		#region Constructor

		protected DeclarationValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		protected DeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}
		protected readonly EventsWithSourceType TriggeredByEvents;

		public static DeclarationValueObjectDataAdapter New()
		{
			return New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, EventsWithSourceType.Empty);
		}

		public static DeclarationValueObjectDataAdapter New(BaseJobDeclaration declaration, EventsWithSourceType triggeredByEvents)
		{
			return New(declaration.CountryCode, triggeredByEvents);
		}

		public static DeclarationValueObjectDataAdapter New(string countryCode, EventsWithSourceType triggeredByEvents)
		{
			DeclarationValueObjectDataAdapter result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = GetCountrySpecificDataAdapter(countryCode, triggeredByEvents);

				if (result == null)
				{
					result = new DeclarationValueObjectDataAdapter(triggeredByEvents);
				}
			}
			return result;
		}

		static DeclarationValueObjectDataAdapter GetCountrySpecificDataAdapter(string countryCode, EventsWithSourceType triggeredByEvents)
		{
			DeclarationValueObjectDataAdapter result = null;

			Hashtable types = (Hashtable)ObjectFactory.Get("DeclarationValueObjectDataAdapterList");

			if (types.Contains(countryCode))
			{
				result = New(((Type)types[countryCode]), triggeredByEvents);
			}
			return result;
		}

		static DeclarationValueObjectDataAdapter New(Type type, EventsWithSourceType triggeredByEvents)
		{
			return (DeclarationValueObjectDataAdapter)Activator.CreateInstance(type, new object[] { triggeredByEvents });
		}

		protected delegate DeclarationValueObjectDataAdapter NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Overrides

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Consols"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Consol"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ConsolAndShipmentSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		#endregion

		#region FindBusinessObjects

		internal BaseJobDeclaration[] FindJobDeclarations(Xsd.ConsolAndShipment declarationValue, IValueObjectImportContext context)
		{
			BaseJobDeclaration[] result = Array.Empty<BaseJobDeclaration>();

			if (ImportDeclarationNoFromXml)
			{
				string agentReference = declarationValue.Consol.ConsolDetail.AgentReference;
				ZQuery filter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, agentReference);
				filter.IgnoreActiveFilter = true;

				var existingDeclarations = context.Factory.Load<BaseJobDeclaration>(filter);
				if (existingDeclarations.Length > 0)
				{
					List<BaseJobDeclaration> declarationsToUpdate = new List<BaseJobDeclaration>();
					if (!AllowDeclarationUpdate)
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("af77c638-14d7-4cc8-9e00-8b160a4f4961", "Duplicate declarations found with job number(s) {0}", agentReference)));
					}
					else
					{
						foreach (BaseJobDeclaration existingDeclaration in existingDeclarations)
						{
							if (existingDeclaration.Branch != null && existingDeclaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
							{
								declarationsToUpdate.Add(existingDeclaration);
							}
						}

						foreach (BaseJobDeclaration existingDeclaration in existingDeclarations)
						{
							if (!declarationsToUpdate.Contains(existingDeclaration))
							{
								if (declarationsToUpdate.Count == 0 // DeclarationXmlDataAdapter is used to create a stand-alone declaration. If existing ones have shipment, then it will be definitely a different declaration 
									|| declarationsToUpdate.Exists(x => x.JE_JS != existingDeclaration.JE_JS))
								{
									context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("7bc8b27b-0d37-44db-a7a0-37b03bd82f32", "A declaration exists and it belongs to a different company, {0}. If you are able to log into the company, please try to import this file in the company.", existingDeclaration.Branch.Company.GC_Code)));
									break;
								}
							}
						}
					}

					result = declarationsToUpdate.ToArray();
				}
			}
			else
			{
				Xsd.MasterAndHouseBill[] masterAndHouseBills = declarationValue.Shipment.GetMasterAndHouseBillIdentifiers(declarationValue.Consol);

				if (masterAndHouseBills.Length > 0)
				{
					ZQuery houseBillQuery = GetHouseBillQuery(masterAndHouseBills);
					result = LoadJobDeclarationsFromHouseBillQuery(context.Factory, houseBillQuery, masterAndHouseBills);
				}
			}

			return result;
		}

		protected override BaseJobDeclaration FindBusinessObject(Xsd.ConsolAndShipment declarationValue, IValueObjectImportContext context)
		{
			var declarations = FindJobDeclarations(declarationValue, context);

			if (declarations.Length > 1)
			{
				Xsd.MasterAndHouseBill[] masterAndHouseBills = declarationValue.Shipment.GetMasterAndHouseBillIdentifiers(declarationValue.Consol);
				ZString masterAndHouseBillsAsString = Xsd.MasterAndHouseBill.GetMasterAndHouseBillsAsString(masterAndHouseBills);
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("859cd98a-b7e5-4d9e-aa7f-2f2e5e758324", "Duplicate declarations found with house/master bill(s) {0}", masterAndHouseBillsAsString)));
			}

			return declarations.Length > 0 ? declarations[0] : null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetHouseBillQuery(Xsd.MasterAndHouseBill[] masterAndHouseBills)
		{
			ZQuery result = new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill);

			ZQuery billNumQuery = new ZQuery();
			foreach (Xsd.MasterAndHouseBill masterAndHouseBill in masterAndHouseBills)
			{
				ZQuery oneBillQuery = new ZQuery(CusDecHouseBillSchema.CU_BillNum, masterAndHouseBill.HouseBill);
				oneBillQuery.AddToFilter(JoinCondition.And, CusDecHouseBillSchema.CU_CU_ParentBill, masterAndHouseBill.MasterBill.IsEmpty ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, DBNull.Value);

				billNumQuery.AddToFilter(oneBillQuery, JoinCondition.Or);
			}
			result.AddToFilter(billNumQuery);
			return result;
		}

		BaseJobDeclaration[] LoadJobDeclarationsFromHouseBillQuery(BusinessObjectFactory factory, ZQuery houseBillQuery, Xsd.MasterAndHouseBill[] masterAndHouseBills)
		{
			Bill[] houseBills = (Bill[])factory.Load(typeof(Bill), houseBillQuery);

			List<BaseJobDeclaration> result = new List<BaseJobDeclaration>();

			foreach (Bill bill in houseBills)
			{
				var declaration = bill.Declaration;

				foreach (Xsd.MasterAndHouseBill billNums in masterAndHouseBills)
				{
					if (bill.CU_BillNum.EqualsIgnoringCase(billNums.HouseBill) &&
						bill.CU_MasterBill.EqualsIgnoringCase(billNums.MasterBill) &&
						declaration != null &&
						!declaration.JE_IsCancelled &&
						!result.Contains(declaration) &&
						declaration.Branch != null && declaration.Branch.GB_GC == GlbCompany.CurrentCompany.PK)
					{
						result.Add(bill.Declaration);
					}
				}
			}

			return result.ToArray();
		}

		bool ImportDeclarationNoFromXml
		{
			get { return SystemDataRegistry.Instance.ImportDeclarationNoFromXml.Value; }
		}

		bool AllowDeclarationUpdate
		{
			get { return SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.Value; }
		}

		#endregion

		#region Import

		public void ImportFromValueObject(BaseJobDeclarationCollection jobDeclarations, Xsd.Consols consols, Type jobDeclarationType, IValueObjectImportContext context)
		{
			if (consols != null)
			{
				foreach (Xsd.Consol consol in consols.Consol)
				{
					ImportFromValueObject(jobDeclarations, consol, jobDeclarationType, context);
				}
			}
		}

		public void ImportFromValueObject(BaseJobDeclarationCollection jobDeclarations, Xsd.Consols consols, IValueObjectImportContext context)
		{
			ImportFromValueObject(jobDeclarations, consols, typeof(BaseJobDeclaration), context);
		}

		public void ImportFromValueObject(BaseJobDeclarationCollection jobDeclarations, Xsd.Consol consol, ValueObjectImportContext context)
		{
			ImportFromValueObject(jobDeclarations, consol, typeof(BaseJobDeclaration), context);
		}

		public void ImportFromValueObject(BaseJobDeclarationCollection jobDeclarations, Xsd.Consol consol, Type jobDeclarationType, IValueObjectImportContext context)
		{
			if (consol != null && (jobDeclarationType.IsSubclassOf(typeof(BaseJobDeclaration)) || jobDeclarationType == typeof(BaseJobDeclaration)))
			{
				foreach (Xsd.Shipment shipment in consol.Shipments)
				{
					Xsd.ConsolAndShipment declaration = new Xsd.ConsolAndShipment();
					declaration.Consol = consol;
					declaration.Shipment = shipment;

					BaseJobDeclaration jobDec = jobDeclarations.AddNew(jobDeclarationType);
					jobDec.SuspendValidation();

					ImportFromValueObject(jobDec, declaration, context);
				}
			}
			else
			{
				ErrorNotification error = new ErrorNotification(ErrorType.Error, Res.GetString("ca0341f5-15ce-4111-aa8e-2e81607bf372", "Could not Import Declaration as Consol and/or its Shipment Collection is null"));
				context.Notify(error);
			}
		}

		protected override void ImportFromValueObjectCore(BaseJobDeclaration jobDec, Xsd.ConsolAndShipment value, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notify = context;

			Xsd.ConsolAndShipment consolAndShipment = value;

			if (consolAndShipment.Consol != null && consolAndShipment.Consol.ConsolDetail != null && consolAndShipment.Shipment != null && consolAndShipment.Shipment.ShipmentDetails != null)
			{
				ImportFromBranchDetails(jobDec, consolAndShipment.Shipment.Declaration.Branch, context);
				if (ImportDeclarationNoFromXml)
				{
					if (!value.Consol.ConsolDetail.AgentReference.IsEmpty)
					{
						jobDec.ShouldDeclarationReferenceBePopulatedFromXml = true;
						BaseJobDeclaration result = null;
						string agentReference = value.Consol.ConsolDetail.AgentReference;
						ZQuery filter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, agentReference);
						result = context.Factory.LoadTop1<BaseJobDeclaration>(filter);
						if (result != null && !AllowDeclarationUpdate)
						{
							notify.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("42459f97-5c7c-458d-8f58-c668951c3da9", "Duplicate declarations found with job number(s) {0}", agentReference)));
						}
						else
						{
							jobDec.JE_DeclarationReference = value.Consol.ConsolDetail.AgentReference;
						}
					}
				}
				context.SetPropertyInfoValue(jobDec.JE_TransportModeInfo, TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(consolAndShipment.Shipment.ShipmentDetails.TransportMode.ToString(), "", context), true, Res.GetString("e721f3e7-8845-4fd7-8421-6a0282df3da8", "Job Declaration Transport Mode"));
				ImportConsolDetails(jobDec, consolAndShipment.Consol, context);
				SetShipmentTypeDetails(jobDec, value, context);
				ImportForwarder(jobDec, consolAndShipment.Consol, context);
				ImportContainers(jobDec, consolAndShipment.Consol.ConsolDetail.Containers, context);
				ImportShipmentDetails(jobDec, consolAndShipment.Consol, consolAndShipment.Shipment, context);
				ImportOrders(jobDec, consolAndShipment.Shipment, context);
				ImportDeclarationDetails(jobDec, consolAndShipment.Shipment.Declaration, consolAndShipment.Consol.Masterbill, context);
				ImportInvoiceDetails(consolAndShipment.Shipment.Invoices, jobDec, context);
				ImportEntryReferences(consolAndShipment.Shipment, jobDec, context);
				ImportBilling(jobDec, consolAndShipment.Shipment, context);
			}

			AddImportEvent(jobDec, DataImportReference);
		}

		public void ImportEntryReferences(Xsd.Shipment xsdShipment, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			Xsd.EntryHeaderCollection entryHeaderCollection = xsdShipment.Declaration.EntryHeader;

			if (entryHeaderCollection.IsSpecified)
			{
				foreach (Xsd.EntryHeader xsdHeader in entryHeaderCollection)
				{
					CusEntryHeader toUpdateHeader = AddReferenceIfNessesary(jobDec, xsdHeader.EntryReference);
					foreach (Xsd.EntryLine xsdLine in xsdHeader.EntryLines)
					{
						ZString headerNo = null;
						ZShort lineNo = ZShort.Zero;
						GetInvoiceNumbers(xsdShipment.Invoices, xsdLine.EntryLineNumber, xsdHeader.EntryReference, out headerNo, out lineNo);
						if (!headerNo.IsEmpty && lineNo != ZShort.Zero)
						{
							BaseJobComInvoiceHeader invoiceHeader = null;
							foreach (var curInvoiceHeader in jobDec.Invoices)
							{
								if (curInvoiceHeader.JZ_InvoiceNumber == headerNo)
								{
									invoiceHeader = curInvoiceHeader;
									break;
								}
							}
							if (invoiceHeader == null)
							{
								invoiceHeader = jobDec.Invoices.AddNew();
								invoiceHeader.JZ_InvoiceNumber = headerNo;
							}

							BaseJobComInvoiceLine invoiceLine = null;
							foreach (BaseJobComInvoiceLine curInvLine in invoiceHeader.JobComInvoiceLines)
							{
								if (curInvLine.JI_LineNo == lineNo)
								{
									invoiceLine = curInvLine;
									break;
								}
							}
							if (invoiceLine == null)
							{
								invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
								invoiceLine.JI_LineNo = lineNo;
							}
							if (invoiceLine.CusEntryLine == null)
							{
								CusEntryLine entryLine = toUpdateHeader.AllEntryLines.AddNew();
								invoiceLine.JI_CL = entryLine.PK;
								entryLine.CL_LineNumber = ZShort.Parse(xsdLine.EntryLineNumber);
							}
							foreach (Xsd.EntryLineCharge xsdCharge in xsdLine.EntryLineCharges)
							{
								var i = invoiceLine.CusEntryLine.Fees.AddOrUpdate(xsdCharge.ChargeType, xsdCharge.ChargeValue.Value);
							}
						}
					}
				}
			}
			else if (xsdShipment.Invoices.IsSpecified)
			{
				foreach (Xsd.InvoiceHeader xsdInvoiceHeader in xsdShipment.Invoices)
				{
					foreach (Xsd.InvoiceLine xsdInvoiceLine in xsdInvoiceHeader.InvoiceLines)
					{
						AddReferenceIfNessesary(jobDec, xsdInvoiceLine.EntryReference);
					}
				}
			}
			return;
		}

		public CusEntryHeader AddReferenceIfNessesary(BaseJobDeclaration jobDec, ZString entryReference)
		{
			CusEntryHeader toUpdateHeader = null;
			foreach (CusEntryHeader cusEntryHeader in jobDec.CustomsEntryHeaders)
			{
				if (cusEntryHeader.CH_BGMReference == entryReference)
				{
					toUpdateHeader = cusEntryHeader;
					break;
				}
			}
			if (toUpdateHeader == null && !entryReference.IsEmpty)
			{
				toUpdateHeader = jobDec.CustomsEntryHeaders.AddNew();
				toUpdateHeader.CH_BGMReference = entryReference.Substring(0, 35);
			}

			return toUpdateHeader;
		}

		public void GetInvoiceNumbers(Xsd.InvoiceHeaderCollection invoceCollection, ZString entryLineNumber, ZString entryReference, out ZString headerNo, out ZShort lineNo)
		{
			headerNo = ZString.Empty;
			lineNo = ZShort.Zero;
			foreach (Xsd.InvoiceHeader xsdInvHeader in invoceCollection)
			{
				foreach (Xsd.InvoiceLine xsdInvLine in xsdInvHeader.InvoiceLines)
				{
					if (xsdInvLine.EntryReference == entryReference && xsdInvLine.EntryLineNumber == entryLineNumber)
					{
						headerNo = xsdInvHeader.InvoiceNumber;
						ZShort.TryParse(xsdInvLine.InvoiceLineNumber, out lineNo);
						return;
					}
				}
			}
		}

		#region Import Billing

		void ImportBilling(BaseJobDeclaration declaration, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.Billing.IsSpecified && ShouldAllowBillingImport(declaration))
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				BusinessObject bizObj = declaration.Shipment == null ? declaration : declaration.Shipment;
				dataAdapter.ImportFromValueObject(bizObj, shipmentValue.Billing, context);
			}
		}

		#endregion

		void ImportDocAddresses(BaseJobDeclaration jobDec, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			if (shipment.ShipmentDetails.DocAddresses.IsSpecified)
			{
				DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("");
				helper.ImportFromValueObjectCollection(shipment.ShipmentDetails.DocAddresses.DocAddress, jobDec.DocAddresses, context);
			}

			if (jobDec.Shipment != null)
			{
				if (shipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRD) == null && shipment.ShipmentDetails.Consignor.IsSpecified)
				{
					jobDec.Shipment.ConsignorPK = context.FindOrCreateTempOrganisationPK(shipment.ShipmentDetails.Consignor, jobDec.Shipment, OrganisationTypes.Consignor);
				}
				if (shipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CED) == null && shipment.ShipmentDetails.Consignee.IsSpecified)
				{
					jobDec.Shipment.ConsigneePK = context.FindOrCreateTempOrganisationPK(shipment.ShipmentDetails.Consignee, jobDec.Shipment, OrganisationTypes.Consignee);
				}
				if (shipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRG) == null && shipment.ShipmentDetails.Pickup.Address.IsSpecified)
				{
					SetBizObjOrgAddress(shipment.ShipmentDetails.Pickup.Address, jobDec.Shipment.ConsignorPickupAddress, Res.GetString("2813c35a-e240-4b41-891b-d5f835166925", "Pickup"), context);
				}
				if (shipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CEG) == null && shipment.ShipmentDetails.Deliver.Address.IsSpecified)
				{
					SetBizObjOrgAddress(shipment.ShipmentDetails.Deliver.Address, jobDec.Shipment.ConsigneeDeliveryAddress, Res.GetString("ec3565d2-58b3-4dc1-ad10-fc3f0aeb2718", "Delivery"), context);
				}
				if (shipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.NPP) == null)
				{
					ImportNotifyParty(jobDec.Shipment, shipment, context);
				}
			}
		}

		void ImportNotifyParty(ForwardingShipment shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			ContactValueObjectHelper helper = new ContactValueObjectHelper("");
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.NotifyParty.Organisation, shipment, OrganisationTypes.None);
			shipment.NotifyContact = helper.FromContactReferenceGetContactName(value.ShipmentDetails.NotifyParty, context);
		}

		void SetBizObjOrgAddress(Xsd.OrgAddress address, JobDocAddress docAddress, string description, IValueObjectImportContext context)
		{
			docAddress.E2_AddressOverride = true;
			context.SetPropertyInfoValue(docAddress.E2_Address1Info, address.AddressLine1, address.AddressLine1Specified, Res.GetString("507f7045-b71f-4566-a642-1db64af6bfb0", "{0} Address 1", description));
			context.SetPropertyInfoValue(docAddress.E2_Address2Info, address.AddressLine2, address.AddressLine2Specified, Res.GetString("2fa2a5ee-f484-463d-94bf-06830088ffe9", "{0} Address 2", description));
			context.SetPropertyInfoValue(docAddress.E2_CityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified, Res.GetString("094c684d-f594-46ba-af59-99c744325b02", "{0} City", description));
			context.SetPropertyInfoValue(docAddress.E2_StateInfo, address.StateOrProvince, address.StateOrProvinceSpecified, Res.GetString("2631fd17-cca6-456c-9f3a-1af0e3cf5e6b", "{0} State", description));
			context.SetPropertyInfoValue(docAddress.E2_PostcodeInfo, address.PostCode, address.PostCodeSpecified, Res.GetString("1270d2ae-b624-4a5a-be57-a0e2343783ec", "{0} Postcode", description));
		}

		#region ImportInvoiceDetails

		protected virtual InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration jobDec)
		{
			return new InvoicesGeneratorFromXSD(jobDec);
		}

		public void ImportInvoiceDetails(Xsd.InvoiceHeaderCollection xmlInvoices, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			InvoicesGeneratorFromXSD invoiceGenerator = GetNewInvoicesGenerator(jobDec);

			invoiceGenerator.ImportInvoicesDetails(xmlInvoices, jobDec, context);
		}

		#endregion

		#region ImportConsolDetails
		protected virtual void ImportConsolDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
		{
			if (consol.ConsolDetail.IsSpecified)
			{
				Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
				INotifications notification = context;

				Xsd.ConsolConsolDetail consolConsolDetail = consol.ConsolDetail;
				ImportMasterbillDetails(jobDec, consol, context);

				XsdMovement.ToPortEstimatedActualDates(consolConsolDetail.PortOfLoading, jobDec.JE_RL_NKPortOfLoadingInfo, jobDec.JE_ExportDateInfo, jobDec.JE_ExportDateInfo, "PortOfLoading", context);
				XsdMovement.ToPortEstimatedActualDates(consolConsolDetail.PortOfDischarge, jobDec.JE_RL_NKPortOfArrivalInfo, jobDec.JE_DateOfArrivalInfo, jobDec.JE_DateOfArrivalInfo, "PortOfArrival", context);
				XsdMovement.ToPortEstimatedActualDates(consolConsolDetail.PortFirstArrival, jobDec.JE_RL_NKPortOfFirstArrivalInfo, jobDec.JE_DateOfFirstArrivalInfo, jobDec.JE_DateOfFirstArrivalInfo, "PortOfFirstArrival", context);

				if (consol.ConsolDetail.ContainerModeSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(jobDec.JE_ContainerModeInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(consol.ConsolDetail.ContainerMode, Res.GetString("7da83e0e-3c35-4eb9-a5ce-eb02e933a004", "Container mode"), context));
				}
				if (jobDec.TransportMode == Core.Constants.TransportModes.Sea)
				{
					Xsd.SailingWithVesselVoyage sailingWithVesselVoyage = consolConsolDetail.Item as Xsd.SailingWithVesselVoyage;
					ImportVesselInformation(jobDec, sailingWithVesselVoyage, context);
				}
				else if (jobDec.TransportMode == Core.Constants.TransportModes.Air)
				{
					Xsd.FlightWithFlightNumber roadRailFlight = consolConsolDetail.Item as Xsd.FlightWithFlightNumber;
					if (roadRailFlight != null)
					{
						context.SetPropertyInfoValue(jobDec.JE_VoyageFlightNoInfo, roadRailFlight.FlightNoJourneyNoTruckRegNo, roadRailFlight.FlightNoJourneyNoTruckRegNoSpecified, Res.GetString("8f2b0b62-ab77-40ca-bdf0-47c81202fb35", "Job Declaration Voyage/Flight Number"));
					}
				}

				if (consol.ConsolDetail.Carrier.IsSpecified)
				{
					jobDec.JE_OH_ShippingLine = GetMatchedOrganisation(consol.ConsolDetail.Carrier, context, jobDec, OrganisationTypes.Carrier);
				}
			}
		}

		void ImportForwarder(BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
		{
			if (consol.ConsolDetail.SendingAgent.IsSpecified || consol.ConsolDetail.ReceivingAgent.IsSpecified)
			{
				if (jobDec.IsImport)
				{
					jobDec.JE_OH_Forwarder = GetMatchedOrganisation(consol.ConsolDetail.SendingAgent, context, jobDec, OrganisationTypes.Forwarder);
				}
				else
				{
					jobDec.JE_OH_Forwarder = GetMatchedOrganisation(consol.ConsolDetail.ReceivingAgent, context, jobDec, OrganisationTypes.Forwarder);
				}
			}
		}
		#endregion

		#region ImportMasterbillDetails

		protected virtual void ImportMasterbillDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
		{
			Xsd.ConsolIdentifierCollection masterBillCollection = consol.ConsolIdentifier.Find(Xsd.ConsolIdentifierType.MasterWaybill);
			foreach (Xsd.ConsolIdentifier consolIdentifier in masterBillCollection)
			{
				if (jobDec.JE_MasterBill.IsEmpty)
				{
					context.SetPropertyInfoValue(jobDec.JE_MasterBillInfo, consolIdentifier.Value, consolIdentifier.ValueSpecified, Res.GetString("eff5f0a7-c0a8-410d-b670-91e90bb96e93", "Declaration Master bill"));
				}
				AddMasterBillToHouseBills(jobDec, consolIdentifier.Value, context);
			}
		}

		void AddMasterBillToHouseBills(BaseJobDeclaration jobDec, string masterBillNo, IValueObjectImportContext context)
		{
			Bill bill = jobDec.Bills.FindByBillNumberAndType(masterBillNo, BillTypeList.Codes.MasterBill);
			if (bill == null)
			{
				bill = jobDec.Bills.FindByBillNumberAndType(ZString.Empty, BillTypeList.Codes.MasterBill);
				if (bill == null)
				{
					bill = jobDec.Bills.AddNew();
					bill.CU_BillType = BillTypeList.Codes.MasterBill;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(bill.CU_BillNumInfo, masterBillNo);
			}
		}
		#endregion

		#region ImportHousebillDetails

		protected void ImportHousebillDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notify = context;

			Xsd.ShipmentIdentifierCollection houseBillCollection = shipment.ShipmentIdentifier.Find(Xsd.ShipmentIdentifierType.Housebill);
			Xsd.ShipmentIdentifierCollection directShipments = new Xsd.ShipmentIdentifierCollection();
			Xsd.ShipmentIdentifierCollection indirectShipments = new Xsd.ShipmentIdentifierCollection();
			foreach (Xsd.ShipmentIdentifier shipmentIdentifier in houseBillCollection)
			{
				if (shipmentIdentifier.Value.IsEmpty)
				{
					directShipments.Add(shipmentIdentifier);
				}
				else
				{
					indirectShipments.Add(shipmentIdentifier);
				}
			}
			foreach (Xsd.ShipmentIdentifier shipmentIdentifier in indirectShipments)
			{
				AddHouseBillToHouseBills(jobDec, shipmentIdentifier.Value, shipmentIdentifier.GetMasterBill(consol), context);
			}
			foreach (Xsd.ShipmentIdentifier shipmentIdentifier in directShipments)
			{
				AddHouseBillToHouseBills(jobDec, shipmentIdentifier.Value, shipmentIdentifier.GetMasterBill(consol), context);
			}
		}

		protected virtual void AddHouseBillToHouseBills(BaseJobDeclaration jobDec, ZString houseBillNo, ZString masterBillNo, IValueObjectImportContext context)
		{
			Bill bill = jobDec.Bills.FindHouseBillsByHouseBillNumAndParentMasterBill(houseBillNo, masterBillNo);
			if (bill == null)
			{
				Bill parentMasterBill = null;

				if (!masterBillNo.IsEmpty)
				{
					parentMasterBill = jobDec.Bills.FindByBillNumberAndType(masterBillNo, BillTypeList.Codes.MasterBill);
					if (parentMasterBill == null)
					{
						parentMasterBill = jobDec.Bills.AddNew();
						parentMasterBill.CU_BillType = BillTypeList.Codes.MasterBill;
						context.SetPropertyInfoValueIfValueNotEmpty(parentMasterBill.CU_BillNumInfo, masterBillNo);
					}
				}

				bill = jobDec.Bills.FindHouseBillsByHouseBillNumAndParentMasterBill(ZString.Empty, masterBillNo);
				if (bill == null)
				{
					bill = jobDec.Bills.AddNew();
					bill.CU_BillType = BillTypeList.Codes.HouseBill;
				}

				if (parentMasterBill != null)
				{
					bill.CU_CU_ParentBill = parentMasterBill.PK;
				}
				else
				{
					bill.CU_CU_ParentBill = ZGuid.Empty;
				}

				context.SetPropertyInfoValueIfValueNotEmpty(bill.CU_BillNumInfo, houseBillNo);
			}
		}

		#endregion

		#region ImportVesselInformation

		protected virtual void ImportVesselInformation(BaseJobDeclaration jobDec, Xsd.SailingWithVesselVoyage sailingWithVesselVoyage, IValueObjectImportContext context)
		{
			if (sailingWithVesselVoyage != null && sailingWithVesselVoyage.IsSpecified)
			{
				string vesselCode = VesselNameImportHelper.MatchVesselAndGetVesselName(sailingWithVesselVoyage, context.Factory);

				context.SetPropertyInfoValue(jobDec.JE_VesselNameInfo, vesselCode, true, Res.GetString("fe941ffb-4394-4ff8-9400-1759bf256de7", "Job Declaration Vessel Info"));
				context.SetPropertyInfoValue(jobDec.JE_VoyageFlightNoInfo, sailingWithVesselVoyage.VoyageNo, sailingWithVesselVoyage.VoyageNoSpecified, Res.GetString("fd85edb1-3c26-4188-a063-519fd4182e80", "Job Declaration Voyage/Flight Number"));
				ImportVesselInformationDates(sailingWithVesselVoyage, jobDec, context);
			}
		}

		protected void ImportVesselInformationDates(Xsd.SailingWithVesselVoyage xmlVessel, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			if (xmlVessel.FCLDates.AvailableDate.IsValid)
			{
				jobDec.DocsAndCartage.JP_FCLAvailable = xmlVessel.FCLDates.AvailableDate;
			}

			if (xmlVessel.LCLDates.AvailableDate.IsValid)
			{
				jobDec.DocsAndCartage.JP_LCLAvailable = xmlVessel.LCLDates.AvailableDate;
			}
		}

		#endregion

		#region ImportContainers

		protected virtual void ImportContainers(BaseJobDeclaration jobDec, Xsd.ContainerCollection containers, IValueObjectImportContext context)
		{
			if (containers != null)
			{
				Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
				INotifications notification = context;

				foreach (Xsd.Container containerValue in containers)
				{
					BaseCusContainer cusContainer = null;
					foreach (BaseCusContainer existingContainer in jobDec.CusContainers)
					{
						if (existingContainer.CO_ContainerNumber.EqualsIgnoringCase(containerValue.ContainerNumber))
						{
							cusContainer = existingContainer;
						}
					}
					if (cusContainer == null)
					{
						cusContainer = jobDec.CusContainers.AddNew();
					}

					context.SetPropertyInfoValue(cusContainer.CO_ContainerNumberInfo, containerValue.ContainerNumber, containerValue.ContainerNumberSpecified, Res.GetString("018bc368-c0c7-4918-a858-e52608795e4b", "Job Declaration Container Number"));
					context.SetPropertyInfoValue(cusContainer.CO_FCL_LCL_AIRInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(containerValue.PackingMode.ToString(), "", null), true, Res.GetString("c6caf8e8-01b6-445a-b7b8-715462214e73", "Job Declaration Container FCL/LCL/AIR"));
					context.SetPropertyInfoValue(cusContainer.CO_SealInfo, containerValue.Seal, containerValue.SealSpecified, Res.GetString("288cd3b6-d6e0-4111-86ab-056fee52cbad", "Job Declaration Container Seal"));
					context.SetPropertyInfoValue(cusContainer.CO_SecondSealInfo, containerValue.Seal2, containerValue.Seal2Specified, Res.GetString("3be78d55-fc24-40a6-b0f5-3f955e04eb61", "Job Declaration Container Second Seal"));
					context.SetPropertyInfoValue(cusContainer.CO_WeightInfo, containerValue.Weight.ToString(), true, Res.GetString("bfd8fce3-cd61-44ca-8cd2-58e09a1f4eb3", "Job Declaration Container Net Weight"));
					new ContainerValueObjectHelper(context).ImportContainerType(cusContainer.CO_RCInfo, containerValue.ContainerType);

					#region Container Processes
					if (cusContainer.JobContainer != null)
					{
						using (cusContainer.JobContainer.SuspendContainerPenalties())
						{
							CommonContainer freightContainer = cusContainer.JobContainer;

							#region Export Process
							if (containerValue.ExportProcess.IsSpecified)
							{
								if (!containerValue.ExportProcess.ReleaseNumber.IsEmpty)
								{
									context.SetPropertyInfoValue(freightContainer.JC_ReleaseNumInfo, containerValue.ExportProcess.ReleaseNumber, containerValue.ExportProcess.ReleaseNumberSpecified);
								}
								if (containerValue.ExportProcess.EmptyRequiredBy.IsValid)
								{
									freightContainer.JC_EmptyRequired = containerValue.ExportProcess.EmptyRequiredBy;
								}
								if (containerValue.ExportProcess.EstimatedFullPickup.IsValid)
								{
									freightContainer.JC_DepartureEstimatedPickup = containerValue.ExportProcess.EstimatedFullPickup;
								}
								if (containerValue.ExportProcess.CartageAdvised.IsValid)
								{
									freightContainer.JC_DepartureCartageAdvised = containerValue.ExportProcess.CartageAdvised;
								}
								context.SetPropertyInfoValue(freightContainer.JC_DepartureSlotReferenceInfo, containerValue.ExportProcess.SlotBookingRef, containerValue.ExportProcess.SlotBookingRefSpecified);

								if (containerValue.ExportProcess.SlotDate.IsValid)
								{
									freightContainer.JC_DepartureSlotDateTime = containerValue.ExportProcess.SlotDate;
								}
								context.SetPropertyInfoValue(freightContainer.JC_DepartureCartageRefInfo, containerValue.ExportProcess.CartageRef, containerValue.ExportProcess.CartageRefSpecified);

								if (containerValue.ExportProcess.PickupEmptyFrom != null)
								{
									freightContainer.JC_OA_DepartureContainerYardAddress = new AddressValueObjectHelper(Res.GetString("9c4af502-627c-4f7a-8d4c-848d15182da3", "Pickup Empty From on {0}", freightContainer.JC_ContainerNum)).FromAddressReferenceGetAddressPK(containerValue.ExportProcess.PickupEmptyFrom, context);
								}
								if (containerValue.ExportProcess.ContainerYardGateOut.IsValid)
								{
									freightContainer.JC_ContainerYardEmptyPickupGateOut = containerValue.ExportProcess.ContainerYardGateOut;
								}
								if (containerValue.ExportProcess.WharfGateIn.IsValid)
								{
									freightContainer.JC_FCLWharfGateIn = containerValue.ExportProcess.WharfGateIn;
								}
								if (containerValue.ExportProcess.CartageComplete.IsValid)
								{
									freightContainer.JC_DepartureCartageComplete = containerValue.ExportProcess.CartageComplete;
								}
								if (containerValue.ExportProcess.ShippedOnboard.IsValid)
								{
									freightContainer.JC_FCLOnBoardVessel = containerValue.ExportProcess.ShippedOnboard;
								}
								if (containerValue.ExportProcess.DemurrageTime.IsValid)
								{
									freightContainer.DepartureTruckWaitTime = containerValue.ExportProcess.DemurrageTime;
								}
								freightContainer.DepartureTruckWaitCost = containerValue.ExportProcess.DemurrageCharge;

								if (containerValue.ExportProcess.IsArrivingAtCTOByRailSpecified)
								{
									freightContainer.JC_DepartureDeliveryByRail = containerValue.ExportProcess.IsArrivingAtCTOByRail;
								}
							}
							#endregion

							#region Import Process
							if (containerValue.ImportProcess.IsSpecified)
							{
								if (containerValue.ImportProcess.FCLAvailable.IsValid)
								{
									freightContainer.JC_FCLAvailable = containerValue.ImportProcess.FCLAvailable;
								}
								if (containerValue.ImportProcess.FCLStorage.IsValid)
								{
									freightContainer.JC_ArrivalCTOStorageStartDate = containerValue.ImportProcess.FCLStorage;
								}
								if (containerValue.ImportProcess.LCLAvailable.IsValid)
								{
									freightContainer.JC_LCLAvailable = containerValue.ImportProcess.LCLAvailable;
								}
								if (containerValue.ImportProcess.LCLStorage.IsValid)
								{
									freightContainer.JC_LCLStorageCommences = containerValue.ImportProcess.LCLStorage;
								}
								if (containerValue.ImportProcess.WharfUnload.IsValid)
								{
									freightContainer.JC_FCLUnloadFromVessel = containerValue.ImportProcess.WharfUnload;
								}
								if (containerValue.ImportProcess.SlotDate.IsValid)
								{
									freightContainer.JC_ArrivalSlotDateTime = containerValue.ImportProcess.SlotDate;
								}
								context.SetPropertyInfoValue(freightContainer.JC_ArrivalSlotReferenceInfo, containerValue.ImportProcess.SlotBookingRef, containerValue.ImportProcess.SlotBookingRefSpecified);
								context.SetPropertyInfoValue(freightContainer.JC_ArrivalCartageRefInfo, containerValue.ImportProcess.CartageRef, containerValue.ImportProcess.CartageRefSpecified);

								if (containerValue.ImportProcess.WharfGateOut.IsValid)
								{
									freightContainer.JC_FCLWharfGateOut = containerValue.ImportProcess.WharfGateOut;
								}
								if (containerValue.ImportProcess.EstimatedDelivery.IsValid)
								{
									freightContainer.JC_ArrivalEstimatedDelivery = containerValue.ImportProcess.EstimatedDelivery;
								}
								if (containerValue.ImportProcess.CartageAdvised.IsValid)
								{
									freightContainer.JC_ArrivalCartageAdvised = containerValue.ImportProcess.CartageAdvised;
								}
								if (containerValue.ImportProcess.CartageComplete.IsValid)
								{
									freightContainer.JC_ArrivalCartageComplete = containerValue.ImportProcess.CartageComplete;
								}

								if (containerValue.ExportProcess.PickupEmptyFrom != null)
								{
									freightContainer.JC_OA_ArrivalContainerYardAddress = new AddressValueObjectHelper(Res.GetString("5af86e8c-eaa9-4acb-9dbe-1fc8956b852d", "Pickup Empty From on {0}", freightContainer.JC_ContainerNum)).FromAddressReferenceGetAddressPK(containerValue.ImportProcess.DeliverEmptyTo, context);
								}
								if (containerValue.ImportProcess.EmptyReady.IsValid)
								{
									freightContainer.JC_EmptyReadyForReturn = containerValue.ImportProcess.EmptyReady;
								}
								if (containerValue.ImportProcess.EmptyReturnRequiredBy.IsValid)
								{
									freightContainer.JC_EmptyReturnedBy = containerValue.ImportProcess.EmptyReturnRequiredBy;
								}
								if (containerValue.ImportProcess.EmptyReturnedOn.IsValid)
								{
									freightContainer.JC_ContainerYardEmptyReturnGateIn = containerValue.ImportProcess.EmptyReturnedOn;
								}
								if (containerValue.ImportProcess.HeldForFCLTransitStagingSpecified)
								{
									freightContainer.JC_FCLHeldInTransitStaging = containerValue.ImportProcess.HeldForFCLTransitStaging;
								}
								if (containerValue.ImportProcess.PickupByRailSpecified)
								{
									freightContainer.JC_ArrivalPickupByRail = containerValue.ImportProcess.PickupByRail;
								}
								context.SetPropertyInfoValue(freightContainer.ArrivalCTOStorageDaysInfo, containerValue.ImportProcess.StorageDays, containerValue.ImportProcess.StorageDaysSpecified);

								if (containerValue.ImportProcess.StorageChargeSpecified)
								{
									freightContainer.ArrivalCTOStorageCost = containerValue.ImportProcess.StorageCharge;
								}
								if (containerValue.ImportProcess.DemurrageTime.IsValid)
								{
									freightContainer.ArrivalTruckWaitTime = containerValue.ImportProcess.DemurrageTime;
								}
								if (containerValue.ImportProcess.DemurrageChargeSpecified)
								{
									freightContainer.ArrivalTruckWaitCost = containerValue.ImportProcess.DemurrageCharge;
								}
								context.SetPropertyInfoValue(freightContainer.ArrivalCarrierDetentionDaysInfo, containerValue.ImportProcess.DetentionDays, containerValue.ImportProcess.DetentionDaysSpecified);

								if (containerValue.ImportProcess.DetentionChargeSpecified)
								{
									freightContainer.ArrivalCarrierDetentionCost = containerValue.ImportProcess.DetentionCharge;
								}
							}
							#endregion
						}
					}
					#endregion

					#region Custom Fields
					context.SetPropertyInfoValue(cusContainer.CO_CustomAttrib1Info, containerValue.Custom.CustomAttribute1, containerValue.Custom.CustomAttribute1Specified, Res.GetString("2dac2116-7fe4-4585-907c-f22d3dd5b008", "Container Custom Field Attribute 1"));
					if (containerValue.Custom.Date1.IsValid)
					{
						cusContainer.CO_CustomDate1 = containerValue.Custom.Date1;
					}
					if (containerValue.Custom.Decimal1Specified)
					{
						cusContainer.CO_CustomDecimal1 = containerValue.Custom.Decimal1;
					}
					if (containerValue.Custom.Flag1Specified)
					{
						cusContainer.CO_CustomFlag1 = containerValue.Custom.Flag1;
					}
					#endregion
				}
			}
		}

		#endregion

		#region ImportShipmentDetails

		protected virtual void ImportShipmentDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notification = context;

			ImportHousebillDetails(jobDec, consol, shipment, context);

			new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(jobDec.Notes, shipment.Notes, context);

			if (shipment.ShipmentDetails != null)
			{
				ImportDocAddresses(jobDec, shipment, context);

				if (shipment.ShipmentDetails.PortOfOrigin != null && shipment.ShipmentDetails.PortOfOrigin.Port != null)
				{
					context.SetPropertyInfoValue(jobDec.JE_RL_NKOriginInfo, shipment.ShipmentDetails.PortOfOrigin.Port.Value, ForeignKeyType.PortNK, Res.GetString("ce979cf1-c246-40ca-94b3-aa5967b4b7be", "Job Declaration Origin"));
					if (shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime.IsValid)
					{
						jobDec.JE_DateAtOrigin = shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime;
					}
				}

				if (!shipment.ShipmentDetails.ServiceLevel.IsEmpty)
				{
					context.SetPropertyInfoValue(jobDec.JE_RS_NKServiceLevelInfo, shipment.ShipmentDetails.ServiceLevel, ForeignKeyType.RefServiceLevelNK, Res.GetString("7c1c6b52-5b09-460d-a19c-416ca8c4937a", "Job Declaration Service Level"));
				}

				SetMarksAndNumbers(jobDec, shipment.ShipmentDetails.MarksAndNumbers, context);

				if (shipment.ShipmentDetails.PortofDestination.IsSpecified)
				{
					string destinationPort = (shipment.ShipmentDetails.PortofDestination.Port.IsSpecified) ? (string)shipment.ShipmentDetails.PortofDestination.Port.Value : "";
					context.SetPropertyInfoValue(jobDec.JE_RL_NKFinalDestinationInfo, destinationPort, ForeignKeyType.PortNK, Res.GetString("5c9e7113-7494-4500-8c09-75a0fdf1113f", "Job Declaration Final Destination"));
					if (shipment.ShipmentDetails.PortofDestination.EstimatedDateTime.IsValid)
					{
						jobDec.JE_DateAtFinalDestination = shipment.ShipmentDetails.PortofDestination.EstimatedDateTime;
					}
				}

				context.SetPropertyInfoValue(jobDec.JE_GoodsDescriptionInfo, shipment.ShipmentDetails.GoodsDescription, shipment.ShipmentDetails.GoodsDescriptionSpecified, Res.GetString("be678c06-1a86-419b-b091-eec1554b8072", "Job Declaration Goods Description"));
				context.SetPropertyInfoValue(jobDec.JE_OwnerRefInfo, shipment.ShipmentDetails.OwnerReference, shipment.ShipmentDetails.OwnerReferenceSpecified, Res.GetString("e5a24b81-ae29-4752-bb22-9face564f491", "Job Declaration Owner Reference"));
				if (shipment.Declaration.IsSpecified && shipment.Declaration.Broker.IsSpecified)
				{
					context.SetPropertyInfoValue(jobDec.JE_GS_NKCusAgentInfo, shipment.Declaration.Broker.Value, Res.GetString("30c741db-d27a-4b1b-868a-bdff2a38afcf", "Job Declaration Broker"));
				}

				if (shipment.ShipmentDetails.Weight.IsSpecified)
				{
					jobDec.JE_TotalWeight = shipment.ShipmentDetails.Weight.Value;
					context.SetPropertyInfoValue(jobDec.JE_TotalWeightUnitInfo, shipment.ShipmentDetails.Weight.DimensionType, shipment.ShipmentDetails.Weight.DimensionTypeSpecified, Res.GetString("15f222b6-23d5-403a-8c7e-422e29477ada", "Job Declaration Total Weight"));
				}

				if (shipment.ShipmentDetails.Volume.IsSpecified)
				{
					jobDec.JE_TotalVolume = shipment.ShipmentDetails.Volume.Value;
					context.SetPropertyInfoValue(jobDec.JE_TotalVolumeUnitInfo, shipment.ShipmentDetails.Volume.DimensionType, shipment.ShipmentDetails.Volume.DimensionTypeSpecified, Res.GetString("bf2ef947-fd59-427c-95d6-cfcb8b14ff2f", "Job Declaration Total Volume"));
				}

				if (shipment.ShipmentDetails.TotalOuterPacksQty.IsSpecified)
				{
					jobDec.JE_TotalNoOfPacks = (int)(decimal)shipment.ShipmentDetails.TotalOuterPacksQty.Value;
					context.SetPropertyInfoValue(jobDec.JE_TotalNoOfPacksPackTypeInfo, shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType, shipment.ShipmentDetails.TotalOuterPacksQty.DimensionTypeSpecified, Res.GetString("f89d87fb-1201-48d1-b85d-2a49b8cd9331", "Job Declaration Total Number of Packs"));
				}

				if (shipment.ShipmentDetails.TotalInnerPacksQty.IsSpecified)
				{
					jobDec.JE_TotalNoOfPiecesInfo.Value = (ZInt)(decimal)shipment.ShipmentDetails.TotalInnerPacksQty.Value;
				}

				if (shipment.ShipmentDetails.Consignor.IsSpecified)
				{
					jobDec.JE_OH_Supplier = GetMatchedOrganisation(shipment.ShipmentDetails.Consignor, context, jobDec, OrganisationTypes.Consignor);
				}
				if (shipment.ShipmentDetails.Consignee.IsSpecified)
				{
					jobDec.JE_OH_Importer = GetMatchedOrganisation(shipment.ShipmentDetails.Consignee, context, jobDec, OrganisationTypes.Consignee);

					OrgHeader importer = (OrgHeader)jobDec.Factory.Load(typeof(OrgHeader), jobDec.JE_OH_Importer);
					if (importer != null && importer.MiscServ != null)
					{
						context.SetPropertyInfoValue(jobDec.JE_MergeByInfo, importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy, true, Res.GetString("2545f147-147d-4bc8-b4ee-ebf7748e9ea6", "Job Declaration Invoice Lines Merge By"));
					}
				}

				context.SetPropertyInfoValue(jobDec.JE_ShipmentIncoTermInfo, shipment.ShipmentDetails.Incoterm, shipment.ShipmentDetails.IncotermSpecified, Res.GetString("b757e566-014a-c4a9-451f-46c7e46159cc", "Job Declaration Shipment Incoterm"));
				ImportPickupAndDeliveryInformation(jobDec, shipment.ShipmentDetails, context);

				context.SetPropertyInfoValue(jobDec.JE_AgentsReferenceInfo, shipment.ShipmentDetails.AgentReference, shipment.ShipmentDetails.AgentReferenceSpecified);
				context.SetPropertyInfoValue(jobDec.JE_MessageSubTypeInfo, shipment.ShipmentDetails.DeclarationStyle, shipment.ShipmentDetails.DeclarationStyleSpecified);

				SetShipmentCustomAttributes(jobDec, shipment, context);

				if (shipment.ShipmentDetails.LocalClient.IsSpecified && ShouldAllowBillingImport(jobDec))
				{
					var invoicingParent = (IJobHeaderParent)jobDec.Shipment ?? jobDec;
					JobHeader.Loader loader = new JobHeader.Loader(invoicingParent);
					JobHeader jobHeader = loader.TryLoadOrCreate();
					OrgHeader org = jobHeader.Factory.Load<OrgHeader>(GetMatchedOrganisation(shipment.ShipmentDetails.LocalClient, context, jobDec, OrganisationTypes.None));
					jobHeader.JH_OA_LocalChargesAddr = org != null ? org.MainAddress.PK : ZGuid.Empty;
					jobHeader.JH_ParentID = invoicingParent.PK;
					jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
				}

				if (!shipment.ShipmentDetails.ExporterStatement.IsEmpty && jobDec.Shipment != null && !jobDec.Shipment.IsDeleted)
				{
					jobDec.Shipment.DocsAndCartage.JP_ExportStatement = shipment.ShipmentDetails.ExporterStatement.Left(jobDec.Shipment.DocsAndCartage.JP_ExportStatementInfo.MaxLength);
				}

				DocDataValueObjectDataAdapter.ImportFromValueObject(GetDocNote(jobDec), shipment.DocData, context);
			}
		}

		bool ShouldAllowBillingImport(BaseJobDeclaration declaration)
		{
			var registryItem = declaration.Shipment != null ? SystemDataRegistry.Instance.AllowBillingImportIntoShipment : CustomsDataRegistry.Instance.AllowBillingImportIntoDeclaration;
			return registryItem.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
		}

		DocumentNote GetDocNote(BaseJobDeclaration jobDec)
		{
			DocumentNote docNote = jobDec.DocNote;
			if (jobDec.Shipment != null)
			{
				docNote = DocumentNote.LoadNote(jobDec.Shipment);
			}
			return docNote;
		}

		#endregion

		#region ImportPickupAndDeliveryInformation

		void ImportPickupAndDeliveryInformation(BaseJobDeclaration jobDec, Xsd.ShipmentShipmentDetails shipmentDetails, IValueObjectImportContext context)
		{
			ImportDeliveryInformation(jobDec, shipmentDetails, context);
			ImportPickupInformation(jobDec, shipmentDetails, context);
		}

		void ImportDeliveryInformation(BaseJobDeclaration jobDec, Xsd.ShipmentShipmentDetails xsdShipmentDetails, IValueObjectImportContext context)
		{
			jobDec.DocsAndCartage.DeliveryCartageCoPK = context.FindOrCreateTempOrganisationPK(xsdShipmentDetails.Deliver.CartageCompany, jobDec, OrganisationTypes.Services);

			if (xsdShipmentDetails.Deliver.DeliveryFrom.IsValid)
			{
				jobDec.DocsAndCartage.JP_EstimatedDelivery = xsdShipmentDetails.Deliver.DeliveryFrom;
			}

			if (xsdShipmentDetails.Deliver.DeliveryRequiredBy.IsValid)
			{
				jobDec.DocsAndCartage.JP_DeliveryRequiredBy = xsdShipmentDetails.Deliver.DeliveryRequiredBy;
			}

			if (xsdShipmentDetails.Deliver.CartageAdvised.IsValid)
			{
				jobDec.DocsAndCartage.JP_DeliveryCartageAdvised = xsdShipmentDetails.Deliver.CartageAdvised;
			}

			if (xsdShipmentDetails.Deliver.GoodsDelivered.IsValid)
			{
				jobDec.DocsAndCartage.JP_DeliveryCartageCompleted = xsdShipmentDetails.Deliver.GoodsDelivered;
			}

			Xsd.OrgAddress address = xsdShipmentDetails.Deliver.Address;
			if (address.IsSpecified)
			{
				jobDec.ImporterDeliveryAddress.E2_AddressOverride = true;
				ZString companyName = GetDeliveryCompanyName(xsdShipmentDetails, jobDec);
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_CompanyNameInfo, companyName, address.CompanyNameSpecified || !string.IsNullOrEmpty(companyName), Res.GetString("330d5c3e-1f4f-4eae-b0ba-3d81ffba63c8", "Delivery Company Name"));
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_Address1Info, address.AddressLine1, address.AddressLine1Specified, Res.GetString("4be98d46-1eaa-4f7b-8282-d5a479583d68", "Delivery Address 1"));
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_Address2Info, address.AddressLine2, address.AddressLine2Specified, Res.GetString("2be4cb0e-29af-4f09-8e5e-fb983dc0c623", "Delivery Address 2"));
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_CityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified, Res.GetString("62db09aa-7a17-4fa0-a6a8-44985f60f2e5", "Delivery City"));
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_StateInfo, address.StateOrProvince, address.StateOrProvinceSpecified, Res.GetString("5aa4071a-b280-426c-a679-e384ccd03120", "Delivery State"));
				context.SetPropertyInfoValue(jobDec.ImporterDeliveryAddress.E2_PostcodeInfo, address.PostCode, address.PostCodeSpecified, Res.GetString("5952882e-6d5f-4737-8ef1-717647994414", "Delivery Postcode"));
			}
		}

		protected virtual ZString GetDeliveryCompanyName(Xsd.ShipmentShipmentDetails shipmentDetails, BaseJobDeclaration jobDec)
		{
			Xsd.OrgAddress address = shipmentDetails.Deliver.Address;
			return address.CompanyName;
		}

		void ImportPickupInformation(BaseJobDeclaration jobDec, Xsd.ShipmentShipmentDetails xsdShipmentDetails, IValueObjectImportContext context)
		{
			jobDec.DocsAndCartage.PickupCartageCoPK = context.FindOrCreateTempOrganisationPK(xsdShipmentDetails.Pickup.CartageCompany, jobDec, OrganisationTypes.Services);

			if (xsdShipmentDetails.Pickup.PickupFrom.IsValid)
			{
				jobDec.DocsAndCartage.JP_EstimatedPickup = xsdShipmentDetails.Pickup.PickupFrom;
			}

			if (xsdShipmentDetails.Pickup.PickupRequiredBy.IsValid)
			{
				jobDec.DocsAndCartage.JP_PickupRequiredBy = xsdShipmentDetails.Pickup.PickupRequiredBy;
			}

			if (xsdShipmentDetails.Pickup.CartageAdvised.IsValid)
			{
				jobDec.DocsAndCartage.JP_PickupCartageAdvised = xsdShipmentDetails.Pickup.CartageAdvised;
			}

			if (xsdShipmentDetails.Pickup.GoodsPickup.IsValid)
			{
				jobDec.DocsAndCartage.JP_PickupCartageCompleted = xsdShipmentDetails.Pickup.GoodsPickup;
			}

			Xsd.OrgAddress address = xsdShipmentDetails.Pickup.Address;
			if (address.IsSpecified)
			{
				jobDec.SupplierPickupAddress.E2_AddressOverride = true;
				ZString companyName = GetPickupCompanyName(xsdShipmentDetails, jobDec);
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_CompanyNameInfo, companyName, address.CompanyNameSpecified || !string.IsNullOrEmpty(companyName), Res.GetString("18a36c8b-52aa-472a-b4e6-e68e1cd153f6", "Pickup Company Name"));
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_Address1Info, address.AddressLine1, address.AddressLine1Specified, Res.GetString("67f1e6d7-4d3e-4781-aee4-08a0a165f721", "Pickup Address 1"));
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_Address2Info, address.AddressLine2, address.AddressLine2Specified, Res.GetString("c2140536-ac75-4997-bf03-7f47c5855afe", "Pickup Address 2"));
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_CityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified, Res.GetString("2c74b032-041d-4c0b-b49c-9a9be0d8d23b", "Pickup City"));
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_StateInfo, address.StateOrProvince, address.StateOrProvinceSpecified, Res.GetString("30b8236e-56a1-4f8e-b682-54fbbf9bc36c", "Pickup State"));
				context.SetPropertyInfoValue(jobDec.SupplierPickupAddress.E2_PostcodeInfo, address.PostCode, address.PostCodeSpecified, Res.GetString("60acc911-afbc-4737-86b9-f9cd8635df15", "Pickup Postcode"));
			}
		}

		protected virtual ZString GetPickupCompanyName(Xsd.ShipmentShipmentDetails shipmentDetails, BaseJobDeclaration jobDec)
		{
			Xsd.OrgAddress address = shipmentDetails.Pickup.Address;
			return address.CompanyName;
		}
		#endregion

		protected virtual ZGuid GetMatchedOrganisation(Xsd.Organisation organisation, IValueObjectImportContext context, BaseJobDeclaration declaration, OrganisationTypes organisationType)
		{
			return context.FindOrCreateTempOrganisationPK(organisation, declaration, organisationType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special Code")]
		void SetMarksAndNumbers(BaseJobDeclaration jobDec, string marksAndNumbers, IValueObjectImportContext context)
		{
			Notes notesBizo = GetNotesBizo(jobDec);
			if (notesBizo != null && !string.IsNullOrEmpty(marksAndNumbers))
			{
				StmNote[] notesFound = notesBizo.FindByDescription("Marks & Numbers");
				if (notesFound.Length == 1)
				{
					context.SetPropertyInfoValue(notesFound[0].ST_NoteTextInfo, marksAndNumbers, true, Res.GetString("facf0165-9dc7-43b4-b285-0d32e6b2058f", "Job Declaration Marks & Numbers"));
				}
				else
				{
					StmNote note = notesBizo.AddNew();
					context.SetPropertyInfoValue(note.ST_DescriptionInfo, Res.GetString("8d2a18bc-e368-4c87-b897-1df5922f4163", "Marks & Numbers"), true, Res.GetString("66decfbf-0f76-431e-9ff4-da845e576cef", "Job Declaration Marks & Numbers Title"));
					context.SetPropertyInfoValue(note.ST_NoteTextInfo, marksAndNumbers, true, Res.GetString("212ab45d-ea4d-43e3-a92f-467ec6202424", "Job Declaration Marks & Numbers"));
				}
			}
		}

		void SetShipmentTypeDetails(BaseJobDeclaration jobDec, Xsd.ConsolAndShipment xsdJobDec, IValueObjectImportContext context)
		{
			if (xsdJobDec.Shipment.ShipmentDetails.ShipmentTypeSpecified)
			{
				context.SetPropertyInfoValue(jobDec.JE_MessageTypeInfo, MessageTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xsdJobDec.Shipment.ShipmentDetails.ShipmentType, "", context), xsdJobDec.Shipment.ShipmentDetails.ShipmentTypeSpecified, Res.GetString("0a7c1ee7-5484-46b2-a4d3-2a3d24455209", "Job Declaration Shipment Mode"));
			}
			else
			{
				SetShipmentTypeDetailsCore(jobDec);
			}
		}

		protected virtual void SetShipmentTypeDetailsCore(BaseJobDeclaration jobDec)
		{
		}

		protected virtual void ImportOrders(BaseJobDeclaration jobDec, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			if (shipment.ShipmentDetails.IsSpecified && shipment.ShipmentDetails.OrderReferences != null)
			{
				var orderItems = jobDec.DocsAndCartage.OrderItems;
				foreach (string orderReference in shipment.ShipmentDetails.OrderReferences)
				{
					if (!string.IsNullOrEmpty(orderReference) && orderItems[orderReference] == null)
					{
						var order = orderItems.AddNew();
						order.JT_OrderReference = orderReference;
					}
				}
			}
		}

		protected virtual void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString masterBillOnConsol, IValueObjectImportContext context)
		{
			if (declarationXsd != null)
			{
				if (jobDec.SupportJE_PaymentMethodUsage && declarationXsd.PaymentTermsSpecified)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(jobDec.JE_PaymentMethodInfo, declarationXsd.PaymentTerms);
				}
				ImportDeclarationAdditionalInfo(jobDec, declarationXsd.AddCustomsDetails, context);
				ImportBillContainerPacks(jobDec, declarationXsd, masterBillOnConsol, context);
				ImportBondedWarehouseAddress(jobDec, declarationXsd.BondedWarehouse, context);
			}
		}

		protected virtual void ImportBillContainerPacks(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString masterBillOnConsol, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notify = context;

			foreach (Xsd.DeclarationBillContainerPack billContainerPack in declarationXsd.BillContainerPacks)
			{
				BaseCusContainer cusContainer = null;
				if (!billContainerPack.ContainerNumber.IsEmpty)
				{
					cusContainer = jobDec.CusContainers.Find(billContainerPack.ContainerNumber);
					if (cusContainer == null && jobDec.IsSea)
					{
						ErrorNotification error = new ErrorNotification(ErrorType.Error, Res.GetString("63a2dc82-a031-43ba-a023-75f0a15ac717", "Could not find container number '{0}' while importing bill container packs", billContainerPack.ContainerNumber));
						notify.Notify(error);
					}
				}

				//should be able to find master bills if BillContainerPack.BillNumber is empty
				ZString masterBill = billContainerPack.MasterbillNumber.IsEmpty ? masterBillOnConsol : billContainerPack.MasterbillNumber;
				ZString billNumber = billContainerPack.BillNumber;

				Bill bill = jobDec.Bills.FindAnyBillWithHouseBillMasterBillCombination(billNumber, masterBill);

				if (bill == null && jobDec.Bills.Count == 1)
				{
					bill = jobDec.Bills[0];
				}

				if (bill != null)
				{
					BasePackingGroup packingGroup = new BasePackingGroup.Loader(bill.Factory).GetPackingGroupMatching(bill, cusContainer);
					if (packingGroup == null)
					{
						packingGroup = bill.PackingGroups.AddNew();
						if (cusContainer != null)
						{
							packingGroup.CR_CO_Container = cusContainer.PK;
						}
					}

					BasePackage package = packingGroup.Packages.Count == 1 ? packingGroup.Packages[0] : packingGroup.Packages.AddNew();
					package.CW_PackQty = (ZInt)billContainerPack.PackQty.Value;
					context.SetPropertyInfoValue(package.CW_PackTypeInfo, billContainerPack.PackQty.DimensionType, billContainerPack.PackQty.DimensionTypeSpecified, Res.GetString("042fffce-1937-447f-9961-538d23ba3746", "Package Type"));
				}
				else
				{
					ErrorNotification error = new ErrorNotification(ErrorType.Error, Res.GetString("827ad02a-28bf-472f-acbb-b64c1c4ad00d", "Could not import bill container packs because House bill / Master bill combination does not exist"));
					notify.Notify(error);
				}
			}

			CreateDefaultPackingGroupIfRequired(jobDec);
		}

		void CreateDefaultPackingGroupIfRequired(BaseJobDeclaration jobDec)
		{
			if (jobDec.PackingGroups.Count == 0 && jobDec.Bills.Count == 1)
			{
				BasePackingGroup packingGroup = jobDec.Bills[0].PackingGroups.AddNew();
				if (jobDec.CusContainers.Count == 1)
				{
					packingGroup.CR_CO_Container = jobDec.CusContainers[0].PK;
				}
				packingGroup.AddTotalOuterPackageIfRequired(jobDec.JE_TotalNoOfPacks);
			}
		}

		void ImportBondedWarehouseAddress(BaseJobDeclaration jobDec, Xsd.AddressReference bondedWarehouseAddress, IValueObjectImportContext context)
		{
			if (bondedWarehouseAddress.IsSpecified)
			{
				AddressValueObjectHelper helper = new AddressValueObjectHelper(Res.GetString("e4dc3e2f-c901-4804-9fa2-b09eccdcbd37", "Bonded Warehouse"));
				ZGuid bondedWarehouseAddressPK = helper.FromAddressReferenceGetAddressPK(bondedWarehouseAddress, context);

				if (!bondedWarehouseAddressPK.IsEmpty)
				{
					jobDec.WarehouseDocAddress.E2_OA_Address = bondedWarehouseAddressPK;
				}
			}
		}

		void SetShipmentCustomAttributes(BaseJobDeclaration jobDec, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(jobDec.DocsAndCartage.JP_CustomAttrib1Info, shipment.ShipmentDetails.Custom.CustomAttribute1, shipment.ShipmentDetails.Custom.CustomAttribute1Specified);
			context.SetPropertyInfoValue(jobDec.DocsAndCartage.JP_CustomAttrib2Info, shipment.ShipmentDetails.Custom.CustomAttribute2, shipment.ShipmentDetails.Custom.CustomAttribute2Specified);
			if (!shipment.ShipmentDetails.Custom.Date1.IsEmpty)
			{
				context.SetPropertyInfoValue(jobDec.DocsAndCartage.JP_CustomDate1Info, shipment.ShipmentDetails.Custom.Date1.ToDateTime());
			}

			if (!shipment.ShipmentDetails.Custom.Date2.IsEmpty)
			{
				context.SetPropertyInfoValue(jobDec.DocsAndCartage.JP_CustomDate2Info, shipment.ShipmentDetails.Custom.Date2.ToDateTime());
			}

			if (shipment.ShipmentDetails.Custom.Decimal1Specified)
			{
				jobDec.DocsAndCartage.JP_CustomDecimal1 = shipment.ShipmentDetails.Custom.Decimal1;
			}

			if (shipment.ShipmentDetails.Custom.Decimal2Specified)
			{
				jobDec.DocsAndCartage.JP_CustomDecimal2 = shipment.ShipmentDetails.Custom.Decimal2;
			}

			if (shipment.ShipmentDetails.Custom.Flag1Specified)
			{
				jobDec.DocsAndCartage.JP_CustomFlag1 = (shipment.ShipmentDetails.Custom.Flag1 == Xsd.TrueFalse.@true);
			}

			if (shipment.ShipmentDetails.Custom.Flag2Specified)
			{
				jobDec.DocsAndCartage.JP_CustomFlag2 = (shipment.ShipmentDetails.Custom.Flag2 == Xsd.TrueFalse.@true);
			}
		}

		protected virtual void ImportDeclarationAdditionalInfo(BaseJobDeclaration jobDec, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
		}

		#region ImportFromInterchangeDetails

		protected void ImportFromBranchDetails(BaseJobDeclaration jobDec, Xsd.DeclarationBranch branch, IValueObjectImportContext context)
		{
			ZString branchCode = branch.IsSpecified ? branch.Value : ZString.Empty;
			if (branchCode.IsEmpty)
			{
				var interchange = (Xsd.XmlInterchange)context.Interchange;
				branchCode = interchange.InterchangeInfo.Target.BranchCode;
			}

			if (!branchCode.IsEmpty)
			{
				GlbBranch branch1 = (GlbBranch)context.Factory.LoadFromUniqueKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, branchCode);
				if (branch1 != null)
				{
					jobDec.JE_GB = branch1.PK;
				}
				else
				{
					context.Notify(new WarningNotification(Res.GetString("ba281cd3-dede-4772-b615-a2dbe1c7a66f", "Branch Code not found: {0}", branchCode)));
				}
			}
		}

		#endregion

		#endregion

		#region Export

		public Xsd.Consols ExportConsolsValueObject(BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();

			ExportToValueObjectCore(jobDec, consolAndShipment, context);
			Xsd.Consols consols = new Xsd.Consols();
			consols.Consol = new Xsd.ConsolCollection();
			consols.Consol.Add(consolAndShipment.Consol);
			consols.Consol[0].Shipments.Clear();
			consols.Consol[0].Shipments.Add(consolAndShipment.Shipment);

			return consols;
		}

		protected override void ExportToValueObjectCore(BaseJobDeclaration jobDec, Xsd.ConsolAndShipment result, IValueObjectExportContext context)
		{
			string errorContext = Res.GetString("e9c415ce-b316-4beb-94d8-6bcf1734570f", "Declaration {0}", jobDec.JE_DeclarationReference);

			Xsd.Consol toConsol = new Xsd.Consol();
			Xsd.Shipment toShipment = new Xsd.Shipment();

			toConsol.Events = StmALogValueObjectDataAdapter.New(jobDec, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);
			toShipment.Events.IsSpecified = false;

			organisationDataAdapter = new OrganisationValueObjectDataAdapter(jobDec);

			ExportConsolValues(toConsol, jobDec, context);
			ExportShipmentValues(toShipment, jobDec, context);
			ExportContainers(toConsol.ConsolDetail, jobDec, context);
			ExportARInvoices(toShipment, jobDec, context);

			toShipment.Declaration.AddCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();

			ExportDeclarationAdditionalInfo(toShipment.Declaration.AddCustomsDetails, jobDec, context);
			ExportPreAdviceIdentifier(jobDec, toShipment, context);

			toShipment.Invoices = ExportInvoiceHeaders(jobDec, context);

			toConsol.Shipments = new Xsd.ShipmentCollection();
			toConsol.Shipments.Add(toShipment);

			result.Consol = toConsol;
			result.Shipment = toShipment;

			AddExportEvent(result, jobDec, context, DataExportReference);
		}

		OrganisationValueObjectDataAdapter organisationDataAdapter;

#if DEBUG
		protected
#endif
 void ExportLandedCostingHeadings(Xsd.Declaration xmlJobDec, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			if (jobDec.IsImport)
			{
				ZQuery query = new ZQuery(LandedCostHeaderSchema.LT_ParentID, jobDec.PK);
				var landedCostHeader = jobDec.Factory.LoadTop1<Integration.LandedCosting.ILandedCostHeader>(query);

				if (landedCostHeader != null && HasLandedCosting(landedCostHeader))
				{
					xmlJobDec.LandedCostingHeader.IsSpecified = true;
					xmlJobDec.LandedCostingHeader.LandedCostingDate = (ZDateTime)landedCostHeader[LandedCostHeaderSchema.LT_DateOfProcessing.Name];
					SetSpecialTaxHeaders(xmlJobDec.LandedCostingHeader, landedCostHeader);
					SetLandedCostingGroupHeaders(xmlJobDec.LandedCostingHeader, landedCostHeader, context);
					SetLandedCostingTrasportAndLogisticCosts(xmlJobDec.LandedCostingHeader, landedCostHeader, context);
				}
			}
		}

		bool HasLandedCosting(Integration.LandedCosting.ILandedCostHeader landedCostHeader)
		{
			return ((IBusinessObjectCollection)landedCostHeader["Histories"]).Count > 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special Code")]
		void SetLandedCostingGroupHeaders(Xsd.LandedCostingHeader landedCostingHeaderXml, Integration.LandedCosting.ILandedCostHeader landedCostHeader, INotifications notify)
		{
			ZString landedCostGroupLabelPropertyName = "LandedCostGroup";
			ZString costDistributionCodePropertyName = "CostDistributionCode";
			ZString label = "Label";

			for (int i = 1; i <= 6; i++)
			{
				ZString description = (ZString)landedCostHeader[landedCostGroupLabelPropertyName + i.ToString(CultureInfo.InvariantCulture) + label];

				if (!description.IsEmpty)
				{
					Xsd.LandedCostingGroupHeader grpHeaderXml = landedCostingHeaderXml.LandedCostingGroupHeaders.AddNew();
					grpHeaderXml.GroupID = i;
					ZString costDistributionCode = (ZString)landedCostHeader[landedCostGroupLabelPropertyName + i.ToString(CultureInfo.InvariantCulture) + costDistributionCodePropertyName];
					grpHeaderXml.CostDistributionCode = CostDistributionMechanismListToXmlCodeMappings.Instance.GetExternalCode(costDistributionCode, "", notify);
					grpHeaderXml.GroupDescription = description;
				}
			}
		}

		void SetLandedCostingTrasportAndLogisticCosts(Xsd.LandedCostingHeader landedCostingHeaderXml, Integration.LandedCosting.ILandedCostHeader landedCostHeader, INotifications notify)
		{
			ZString costInputs = "CostInputs";
			ZString lCGroupString = "LCGroupString";
			ZString linkedObjectUniqueCode = "LinkedObjectUniqueCode";

			BusinessObjectFactory factory = ((BusinessObject)landedCostHeader).Factory;

			foreach (BusinessObject costInput in (BusinessObjectCollection)landedCostHeader[costInputs])
			{
				Xsd.TransportAndLogisticsCost costValue = landedCostingHeaderXml.TransportAndLogisticsCosts.AddNew();
				AccChargeCode chargeCode = factory.Load<AccChargeCode>((ZGuid)costInput[LandCostInputSchema.LI_AC_ChargeCode]);
				costValue.ChargeCode = chargeCode != null ? chargeCode.AC_Code : ZString.Empty;
				costValue.ChargeDescription = (ZString)costInput[LandCostInputSchema.LI_ChargeDescription];
				costValue.ChargeGroup = (ZString)costInput[lCGroupString];
				costValue.DistributionLevel = (ZString)costInput[linkedObjectUniqueCode];
				costValue.DistributionBy = CostDistributionMechanismListToXmlCodeMappings.Instance.GetExternalCode((ZString)costInput[LandCostInputSchema.LI_DistributeCostBy], ZString.Empty, notify);
				costValue.DistributionAmount = (ZDecimal)costInput[LandCostInputSchema.LI_CostAmount];
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, (ZString)costInput[LandCostInputSchema.LI_RX_NKCostCurrency]);
				costValue.Curr = currency != null ? currency.RX_Code : ZString.Empty;
				costValue.ExRate = (ZDecimal)costInput[LandCostInputSchema.LI_ServiceExRate];
			}
		}

		void SetSpecialTaxHeaders(Xsd.LandedCostingHeader xmlLandedCostingHeader, Integration.LandedCosting.ILandedCostHeader landedCostHeader)
		{
			var customsChargeLCItemSettings = ((ILandedCostHistoryMaster)landedCostHeader)?.CustomsChargeLCItemSettings;

			for (int i = 1; i <= 3; i++)
			{
				Xsd.LandedCostingSpecialTaxHeading xmlSpecialTax = xmlLandedCostingHeader.SpecialTaxHeadings.AddNew();
				xmlSpecialTax.Sequence = i;
				xmlSpecialTax.Description = customsChargeLCItemSettings?.FirstOrDefault(x => x.CostType == "ST" + i.ToString(CultureInfo.InvariantCulture))?.Description ?? ZString.Empty;
			}
		}

		protected virtual ZString DataExportReference
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString DataImportReference
		{
			get { return ZString.Empty; }
		}

		protected virtual Xsd.InvoiceHeaderCollection ExportInvoiceHeaders(BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			Xsd.InvoiceHeaderCollection result = new Xsd.InvoiceHeaderCollection();

			GroupInvoiceValueObjectDataAdapter groupInvoiceXml = GetNewGroupInvoiceAdapter(jobDec);
			ExportInvoiceHeaders(groupInvoiceXml, jobDec.JobComInvoiceGroupHeaders, result, context);

			InvoiceValueObjectDataAdapter invoiceXml = GetNewInvoiceAdapter(jobDec);
			ExportInvoiceHeaders(invoiceXml, jobDec.Invoices, result, context);

			return result;
		}

		protected void ExportInvoiceHeaders(GroupInvoiceValueObjectDataAdapter adapter, IBusinessObjectCollection invoiceHeaders, Xsd.InvoiceHeaderCollection xmlInvoiceHeaders, IValueObjectExportContext context)
		{
			foreach (BaseJobComInvoiceGroupHeader invoiceHeader in invoiceHeaders)
			{
				Xsd.InvoiceHeader xmlInvoiceHeader = adapter.ExportToValueObject(invoiceHeader, context);
				if (xmlInvoiceHeader != null)
				{
					xmlInvoiceHeaders.Add(xmlInvoiceHeader);
				}
			}
		}

		protected void ExportInvoiceHeaders(InvoiceValueObjectDataAdapter adapter, IBusinessObjectCollection invoiceHeaders, Xsd.InvoiceHeaderCollection xmlInvoiceHeaders, IValueObjectExportContext context)
		{
			foreach (BaseJobComInvoiceHeader invoiceHeader in invoiceHeaders)
			{
				Xsd.InvoiceHeader xmlInvoiceHeader = adapter.ExportToValueObject(invoiceHeader, context);

				if (xmlInvoiceHeader != null)
				{
					xmlInvoiceHeaders.Add(xmlInvoiceHeader);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special Code")]
		protected virtual void ExportConsolValues(Xsd.Consol toConsol, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			toConsol.ConsolDetail = new Xsd.ConsolConsolDetail();
			toConsol.ConsolDetail.TransportMode = ConsolTransportModeToXmlCodeMappings.Instance.GetExternalCode(jobDec.JE_TransportMode, "", null);
			toConsol.ConsolDetail.TransportModeSpecified = true;
			toConsol.ConsolDetail.Arrival.IsSpecified = false;
			toConsol.ConsolDetail.Departure.IsSpecified = false;
			toConsol.ConsolDetail.ExternalAgentReference = jobDec.JE_AgentsReference;

			ExportMasterbillDetails(toConsol, jobDec);

			if (AllowExportOfFirstArrivalPort)
			{
				toConsol.ConsolDetail.PortFirstArrival = XsdMovement.FromPortEstimatedActualDates(jobDec.Factory, jobDec.JE_RL_NKPortOfFirstArrival, jobDec.JE_DateOfFirstArrival, ZDateTime.Empty);
			}
			toConsol.ConsolDetail.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(jobDec.Factory, jobDec.JE_RL_NKPortOfLoading, jobDec.JE_ExportDate, ZDateTime.Empty);
			toConsol.ConsolDetail.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(jobDec.Factory, jobDec.JE_RL_NKPortOfArrival, jobDec.JE_DateOfArrival, ZDateTime.Empty);

			if (organisationDataAdapter == null)
			{
				organisationDataAdapter = new OrganisationValueObjectDataAdapter(jobDec);
			}

			if (jobDec.Forwarder != null)
			{
				if (jobDec.IsImport)
				{
					toConsol.ConsolDetail.SendingAgent = organisationDataAdapter.ExportToValueObject(jobDec.Forwarder, context);
				}
				else if (jobDec.IsExport)
				{
					toConsol.ConsolDetail.ReceivingAgent = organisationDataAdapter.ExportToValueObject(jobDec.Forwarder, context);
				}
			}

			if (!jobDec.JE_ContainerMode.IsEmpty)
			{
				toConsol.ConsolDetail.ContainerMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(jobDec.JE_ContainerMode, "Declaration", context);
				toConsol.ConsolDetail.ContainerModeSpecified = true;
			}
			if (toConsol.ConsolDetail.TransportMode == Xsd.ConsolTransportMode.AIR || toConsol.ConsolDetail.TransportMode == Xsd.ConsolTransportMode.RAI)
			{
				Xsd.FlightWithFlightNumber detailFlightJourney = new Xsd.FlightWithFlightNumber();
				toConsol.ConsolDetail.Item = detailFlightJourney;
				detailFlightJourney.FlightNoJourneyNoTruckRegNo = jobDec.JE_VoyageFlightNo;
			}
			else if (toConsol.ConsolDetail.TransportMode == Xsd.ConsolTransportMode.SEA)
			{
				Xsd.SailingWithVesselVoyage xmlVessel = new Xsd.SailingWithVesselVoyage();
				xmlVessel.DepartureCTO = null;
				xmlVessel.DepartureBerth = null;
				xmlVessel.ArrivalCTO = null;
				xmlVessel.ArrivalBerth = null;
				xmlVessel.IsTranshipment = false;
				xmlVessel.IsTranshipmentSpecified = false;
				xmlVessel.VesselName = jobDec.JE_VesselName;

				ZQuery filter = new ZQuery(RefVesselSchema.RV_Code, jobDec.JE_VesselName);
				var vessel = jobDec.Factory.LoadTop1<RefVessel>(filter);
				if (vessel != null && !vessel.RV_LloydsNumber.IsEmpty)
				{
					xmlVessel.LloydsNo = vessel.RV_LloydsNumber;
				}
				xmlVessel.VoyageNo = jobDec.JE_VoyageFlightNo;

				if (!jobDec.DocsAndCartage.JP_LCLAvailable.IsEmpty)
				{
					xmlVessel.LCLDates.AvailableDate = jobDec.DocsAndCartage.JP_LCLAvailable;
				}

				if (!jobDec.DocsAndCartage.JP_FCLAvailable.IsEmpty)
				{
					xmlVessel.FCLDates.AvailableDate = jobDec.DocsAndCartage.JP_FCLAvailable;
				}

				toConsol.ConsolDetail.Item = xmlVessel;
			}

			if (jobDec.ShippingLine != null)
			{
				toConsol.ConsolDetail.Carrier = organisationDataAdapter.ExportToValueObject(jobDec.ShippingLine, context);
			}
		}

		protected virtual bool AllowExportOfFirstArrivalPort
		{
			get { return true; }
		}

		protected virtual void ExportMasterbillDetails(Xsd.Consol toConsol, BaseJobDeclaration jobDec)
		{
			foreach (Bill bill in jobDec.Bills)
			{
				if (bill.CU_BillType == BillTypeList.Codes.MasterBill && !bill.CU_MasterBill.IsEmpty)
				{
					Xsd.ConsolIdentifier masterBill = toConsol.ConsolIdentifier.AddNew();
					masterBill.IsSpecified = true;
					masterBill.ConsolIdentifierTypeSpecified = true;
					masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
					masterBill.Value = bill.CU_BillNum;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special Code")]
		protected virtual void ExportShipmentValues(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			toShipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();
			toShipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			toShipment.ShipmentDetailsSpecified = true;

			Notes notesToExport = jobDec.Shipment != null ? jobDec.Shipment.Notes : jobDec.Notes;
			toShipment.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(notesToExport, context);

			toShipment.ShipmentDetails.DeclarationStyle = jobDec.JE_MessageSubType;
			toShipment.ShipmentDetails.AgentReference = jobDec.JE_DeclarationReference;
			toShipment.ShipmentDetails.ServiceLevel = jobDec.JE_RS_NKServiceLevel;
			toShipment.ShipmentDetails.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(jobDec.JE_TransportMode, "", null);

			toShipment.ShipmentDetails.ShipmentType = MessageTypeToXmlCodeMappings.Instance.GetExternalCode(jobDec.JE_MessageType, "", context);
			toShipment.ShipmentDetails.ShipmentTypeSpecified = !jobDec.JE_MessageType.IsEmpty;

			Notes notesBizo = GetNotesBizo(jobDec);
			if (notesBizo != null)
			{
				StmNote[] notes = notesBizo.FindByDescription("Marks & Numbers");
				if (notes.Length == 1)
				{
					toShipment.ShipmentDetails.MarksAndNumbers = notes[0].ST_NoteText;
				}
			}

			ExportHousebillDetails(toShipment, jobDec);

			if (organisationDataAdapter == null)
			{
				organisationDataAdapter = new OrganisationValueObjectDataAdapter(jobDec);
			}

			if (jobDec.Supplier != null)
			{
				toShipment.ShipmentDetails.Consignor = organisationDataAdapter.ExportToValueObject(jobDec.Supplier, context);
			}

			if (jobDec.Importer != null)
			{
				toShipment.ShipmentDetails.Consignee = organisationDataAdapter.ExportToValueObject(jobDec.Importer, context);
			}

			toShipment.ShipmentDetails.PortOfOrigin = XsdMovement.FromPortEstimatedActualDates(jobDec.Factory, jobDec.JE_RL_NKOrigin, jobDec.JE_DateAtOrigin, ZDateTime.Empty);
			toShipment.ShipmentDetails.PortofDestination = XsdMovement.FromPortEstimatedActualDates(jobDec.Factory, jobDec.JE_RL_NKFinalDestination, jobDec.JE_DateAtFinalDestination, ZDateTime.Empty);

			toShipment.ShipmentDetails.GoodsDescription = jobDec.JE_GoodsDescription;
			toShipment.ShipmentDetails.OwnerReference = jobDec.JE_OwnerRef;
			GlbStaff cusAgent = jobDec.CusAgent;
			if (cusAgent != null)
			{
				toShipment.Declaration.Broker.Value = ZString.Empty; // don't export the code
				toShipment.Declaration.Broker.Name = cusAgent.GS_FullName;
			}
			GlbBranch branch = jobDec.Branch;
			if (branch != null)
			{
				toShipment.Declaration.Branch.Value = ZString.Empty; // don't export the code
				toShipment.Declaration.Branch.Name = branch.GB_BranchName;
			}

			if (jobDec.Shipment != null)
			{
				toShipment.ShipmentDetails.BookingReference = jobDec.Shipment.JS_BookingReference;

				if (!jobDec.Shipment.DocsAndCartage.JP_ExportStatement.IsEmpty)
				{
					toShipment.ShipmentDetails.ExporterStatement = jobDec.Shipment.DocsAndCartage.JP_ExportStatement;
				}
			}

			toShipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(jobDec.JE_TotalWeight, jobDec.JE_TotalWeightUnit);
			toShipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(jobDec.JE_TotalVolume, jobDec.JE_TotalVolumeUnit);
			toShipment.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(jobDec.JE_TotalNoOfPacks), jobDec.JE_TotalNoOfPacksPackType);
			toShipment.ShipmentDetails.TotalInnerPacksQty = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(jobDec.JE_TotalNoOfPieces), Core.Constants.PkgUnit.Package);

			if (jobDec.Shipment != null)
			{
				toShipment.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(jobDec.Shipment.JS_ActualChargeable, jobDec.Shipment.JS_ChargeableUnit);
			}

			toShipment.ShipmentDetails.ChargeableWeight.IsSpecified = true;

			if (jobDec.Job != null)
			{
				if (jobDec.Job.RepSales != null)
				{
					toShipment.ShipmentDetails.SalesRep = jobDec.Job.RepSales.GS_Code;
				}
				if (jobDec.Job.LocalCharges != null)
				{
					toShipment.ShipmentDetails.LocalClient = organisationDataAdapter.ExportToValueObject(jobDec.Job.LocalCharges, context);
				}
			}

			toShipment.ShipmentDetails.TEU = TotalTEU(jobDec);
			toShipment.ShipmentDetails.TEUSpecified = (toShipment.ShipmentDetails.TEU > 0);
			toShipment.ShipmentDetails.Incoterm = jobDec.JE_ShipmentIncoTerm;
			ExportPickupAndDeliveryInformation(jobDec, toShipment.ShipmentDetails, context);
			ExportEntryHeaders(toShipment.ShipmentDetails.CustomsEntries, jobDec.CustomsEntryHeaders, context);
			ExportOrders(toShipment, jobDec, context);
			ExportDeclarationDetails(toShipment.Declaration, jobDec, context);
			ExportCustomAttributes(toShipment, jobDec);
			ExportBilling(toShipment, jobDec, context);

			ExportDocAddresses(jobDec.DocAddresses, toShipment, context);
			if (jobDec.Shipment != null)
			{
				ExportDocAddresses(jobDec.Shipment.DocAddresses, toShipment, context);
			}

			DocDataValueObjectDataAdapter.ExportToValueObject(GetDocNote(jobDec), toShipment.DocData, context);
		}

		#region Export Billing

		void ExportBilling(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			if (CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.Value)
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ExportToValueObject(jobDec, toShipment.Billing, context);
			}
		}

		#endregion

		protected virtual void ExportDocAddresses(JobDocAddressDependentCollection docAddresses, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("");
			helper.ExportToValueObjectCollection(docAddresses, shipmentValue.ShipmentDetails.DocAddresses.DocAddress, context);
		}

		ZDecimal TotalTEU(BaseJobDeclaration jobDec)
		{
			ZDecimal result = 0;
			if (jobDec != null)
			{
				foreach (BaseCusContainer cusContainer in jobDec.CusContainers)
				{
					if (cusContainer.ContainerModeForBinding == Core.Constants.ContainerModes.FCL)
					{
						RefContainer refContainer = cusContainer.Container;
						if (refContainer != null)
						{
							result += refContainer.RC_TEU;
						}
					}
				}
			}
			return result;
		}

		protected virtual void ExportDeclarationDetails(Xsd.Declaration xsdDec, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			if (jobDec.SupportJE_PaymentMethodUsage)
			{
				xsdDec.PaymentTerms = jobDec.JE_PaymentMethod;
			}
			ExportBillContainerPacks(xsdDec, jobDec);
			ExportBondedWarehouseAddress(xsdDec, jobDec, context);
			ExportLandedCostingHeadings(xsdDec, jobDec, context);
		}

		protected virtual void ExportBillContainerPacks(Xsd.Declaration declarationXsd, BaseJobDeclaration jobDec)
		{
			foreach (BasePackingGroup package in jobDec.PackingGroups)
			{
				declarationXsd.IsSpecified = true;
				var billContPack = declarationXsd.BillContainerPacks.AddNew();

				var bill = package.Bill;
				if (bill != null)
				{
					var masterBill = bill.CU_MasterBill;
					var billNum = bill.CU_BillNum;
					billContPack.BillNumber = billNum;

					if (masterBill != billNum)
					{
						billContPack.MasterbillNumber = masterBill;
					}
				}

				billContPack.PackQty.Value = (decimal)package.TotalPackageCount();

				var container = package.Container;
				if (container != null)
				{
					billContPack.ContainerNumber = container.CO_ContainerNumber;
				}
			}
		}

		void ExportBondedWarehouseAddress(Xsd.Declaration xsdDec, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			if (jobDec.WarehouseDocAddress.Address != null)
			{
				AddressValueObjectHelper helper = new AddressValueObjectHelper(Res.GetString("894e8163-0de5-4071-b574-a9ba28aa77be", "Bonded Warehouse"));
				xsdDec.BondedWarehouse = helper.ToAddressReference(jobDec.WarehouseDocAddress.Address, context);
			}
		}

		protected virtual void ExportEntryHeaders(Xsd.CustomsEntryCollection entryHeaderXsds, ICusEntryHeaderCollection<CusEntryHeader> entryHeaders, INotifications notificationSubscriber)
		{
			foreach (var entryHeader in entryHeaders)
			{
				Xsd.CustomsEntry entryHeaderXsd = entryHeaderXsds.AddNew();
				ExportEntryHeader(entryHeaderXsd, entryHeader, entryHeader.LocalCurrency);
			}
		}

		protected virtual void ExportEntryHeader(Xsd.CustomsEntry entryHeaderXsd, CusEntryHeader entryHeader, RefCurrency localCurrency)
		{
			CusEntryNumber entryNumber = entryHeader.CusEntryNumber;
			if (entryNumber != null && !entryNumber.CE_EntryNum.IsEmpty)
			{
				ZString countryCode = entryNumber.CE_RN_NKCountryCode;
				if (countryCode.IsEmpty)
				{
					countryCode = entryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode;
				}

				entryHeaderXsd.CustomsEntryNumber.Country = countryCode;
				entryHeaderXsd.CustomsEntryNumber.Type = entryNumber.CE_EntryType;
				entryHeaderXsd.CustomsEntryNumber.Number = entryNumber.CE_EntryNum;
			}
			else
			{
				entryHeaderXsd.CustomsEntryNumber = null;
			}

			entryHeaderXsd.EntryDate = entryHeader.Declaration.JE_EntrySubmittedDate.Date;

			entryHeaderXsd.CustomsValue = Xsd.FinancialValue.FromAmountAndCurrency(entryHeader.CustomsValue, localCurrency);
			ZDecimal localTAndIValue = entryHeader.CurrencyConverter.ConvertExact(entryHeader.TotalTAndI, localCurrency).Amount;
			entryHeaderXsd.TransportAndInsurance = Xsd.FinancialValue.FromAmountAndCurrency(localTAndIValue, localCurrency);
			ZDecimal exchangeRate = GetExchangeRateWhereAllInvoiceHeadersHaveTheSameCurrency(entryHeader.InvoiceHeaders);
			if (exchangeRate != ZDecimal.Zero)
			{
				entryHeaderXsd.ExchangeRate = exchangeRate;
			}

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				Xsd.CustomsEntryLine entryLineXsd = entryHeaderXsd.CustomsEntryLines.AddNew();
				ExportEntryLine(entryLineXsd, entryLine, localCurrency);
			}

			foreach (CusEntryHeaderCharges entryHeaderCharge in entryHeader.Charges)
			{
				Xsd.ChargesCharge entryHeaderChargeXsd = entryHeaderXsd.Charges.AddNew();
				entryHeaderChargeXsd.ChargeType = entryHeaderCharge.C1_ChargeType;
				entryHeaderChargeXsd.ChargeAmount = Xsd.FinancialValue.FromAmountAndCurrency(entryHeaderCharge.C1_ChargeAmount, localCurrency);
			}
		}

		protected virtual void ExportEntryLine(Xsd.CustomsEntryLine entryLineXsd, CusEntryLine entryLine, RefCurrency localCurrency)
		{
			foreach (CusEntryLineFee entryLineFee in entryLine.Fees)
			{
				Xsd.FeesFee entryLineFeeXsd = entryLineXsd.Fees.AddNew();
				entryLineFeeXsd.FeeType = entryLineFee.CF_ChargeType;
				entryLineFeeXsd.FeeAmount = Xsd.FinancialValue.FromAmountAndCurrency(entryLineFee.CF_ChargeAmount, localCurrency);
			}
		}

		ZDecimal GetExchangeRateWhereAllInvoiceHeadersHaveTheSameCurrency(BaseJobComInvoiceHeader[] invoiceHeaders)
		{
			ZDecimal result = 0;
			if (invoiceHeaders.Length > 0)
			{
				BaseJobComInvoiceHeader firstInvoiceHeader = invoiceHeaders[0];
				ZString firstInvoiceHeaderCurrencyNK = firstInvoiceHeader.JZ_RX_NKInvoice_Currency;

				bool isMultipleCurrency = false;
				for (int index = 1; index < invoiceHeaders.Length; index++)
				{
					if (invoiceHeaders[index].JZ_RX_NKInvoice_Currency != firstInvoiceHeaderCurrencyNK)
					{
						isMultipleCurrency = true;
						break;
					}
				}

				if (isMultipleCurrency)
				{
					result = 1;
				}
				else
				{
					RefCurrency currency = firstInvoiceHeader.Invoice_Currency;
					if (currency != null)
					{
						result = invoiceHeaders[0].CurrencyConverter.GetExchangeRate(currency);
					}
				}
			}
			return result;
		}

		protected virtual void ExportOrders(Xsd.Shipment shipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			var orderReferences = new List<string>();
			foreach (OrderItem item in jobDec.DocsAndCartage.OrderItems)
			{
				if (!item.JT_OrderReference.IsEmpty)
				{
					orderReferences.Add(item.JT_OrderReference);
				}
			}

			if (orderReferences.Count > 0)
			{
				shipment.ShipmentDetails.OrderReferences = orderReferences.ToArray();
			}
		}

		protected virtual void ExportPickupAndDeliveryInformation(BaseJobDeclaration jobDec, Xsd.ShipmentShipmentDetails xsdShipmentDetails, IValueObjectExportContext context)
		{
			if (organisationDataAdapter == null)
			{
				organisationDataAdapter = new OrganisationValueObjectDataAdapter(jobDec);
			}

			xsdShipmentDetails.Pickup.CartageCompany = organisationDataAdapter.ExportToValueObject(jobDec.DocsAndCartage.PickupCartageCo, context);
			xsdShipmentDetails.Deliver.CartageCompany = organisationDataAdapter.ExportToValueObject(jobDec.DocsAndCartage.DeliveryCartageCo, context);

			xsdShipmentDetails.Deliver.DeliveryFrom = jobDec.DocsAndCartage.JP_EstimatedDelivery;
			xsdShipmentDetails.Deliver.DeliveryRequiredBy = jobDec.DocsAndCartage.JP_DeliveryRequiredBy;
			xsdShipmentDetails.Deliver.CartageAdvised = jobDec.DocsAndCartage.JP_DeliveryCartageAdvised;
			xsdShipmentDetails.Deliver.GoodsDelivered = jobDec.DocsAndCartage.JP_DeliveryCartageCompleted;
			xsdShipmentDetails.Deliver.Address.CompanyName = jobDec.ImporterDeliveryAddress.E2_CompanyNameTruncated;
			xsdShipmentDetails.Deliver.Address.AddressLine1 = jobDec.ImporterDeliveryAddress.E2_Address1;
			xsdShipmentDetails.Deliver.Address.AddressLine2 = jobDec.ImporterDeliveryAddress.E2_Address2;
			xsdShipmentDetails.Deliver.Address.CityOrSuburb = jobDec.ImporterDeliveryAddress.E2_City;
			xsdShipmentDetails.Deliver.Address.StateOrProvince = jobDec.ImporterDeliveryAddress.E2_State;
			xsdShipmentDetails.Deliver.Address.PostCode = jobDec.ImporterDeliveryAddress.E2_Postcode;

			AddressValueObjectHelper helper = new AddressValueObjectHelper(Res.GetString("8eb33b2f-9f4b-405a-a060-8efe0722fb38", "CFS Depot Address"));
			Xsd.AddressReference depotAddress = helper.ToAddressReference(jobDec.DepotDocAddress.Address, context);

			if (jobDec.IsImport)
			{
				xsdShipmentDetails.Deliver.CFS.Address = depotAddress;
			}
			else if (jobDec.IsExport)
			{
				xsdShipmentDetails.Pickup.CFS.Address = depotAddress;
			}

			xsdShipmentDetails.Pickup.PickupFrom = jobDec.DocsAndCartage.JP_EstimatedPickup;
			xsdShipmentDetails.Pickup.PickupRequiredBy = jobDec.DocsAndCartage.JP_PickupRequiredBy;
			xsdShipmentDetails.Pickup.CartageAdvised = jobDec.DocsAndCartage.JP_PickupCartageAdvised;
			xsdShipmentDetails.Pickup.GoodsPickup = jobDec.DocsAndCartage.JP_PickupCartageCompleted;
			xsdShipmentDetails.Pickup.Address.CompanyName = jobDec.SupplierPickupAddress.E2_CompanyNameTruncated;
			xsdShipmentDetails.Pickup.Address.AddressLine1 = jobDec.SupplierPickupAddress.E2_Address1;
			xsdShipmentDetails.Pickup.Address.AddressLine2 = jobDec.SupplierPickupAddress.E2_Address2;
			xsdShipmentDetails.Pickup.Address.CityOrSuburb = jobDec.SupplierPickupAddress.E2_City;
			xsdShipmentDetails.Pickup.Address.StateOrProvince = jobDec.SupplierPickupAddress.E2_State;
			xsdShipmentDetails.Pickup.Address.PostCode = jobDec.SupplierPickupAddress.E2_Postcode;
		}

		protected virtual void ExportHousebillDetails(Xsd.Shipment toShipment, BaseJobDeclaration jobDec)
		{
			foreach (Bill bill in jobDec.Bills)
			{
				if (bill.CU_BillType == BillTypeList.Codes.HouseBill && !bill.CU_BillNum.IsEmpty)
				{
					Xsd.ShipmentIdentifier shipmentIdentifier = toShipment.ShipmentIdentifier.AddNew();
					shipmentIdentifier.IsSpecified = true;
					shipmentIdentifier.Value = bill.CU_BillNum;
					shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

					if (!bill.CU_MasterBill.IsEmpty)
					{
						shipmentIdentifier.Masterbill = bill.CU_MasterBill;
					}
				}
			}
		}

		protected virtual void ExportContainers(Xsd.ConsolConsolDetail consolDetail, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			consolDetail.Containers = new Xsd.ContainerCollection();
			if (jobDec != null)
			{
				foreach (BaseCusContainer cusContainer in jobDec.CusContainers)
				{
					Xsd.Container xsdContainer = consolDetail.Containers.AddNew();

					xsdContainer.ContainerNumber = cusContainer.CO_ContainerNumber;
					xsdContainer.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(cusContainer.CO_FCL_LCL_AIR, "", null);
					xsdContainer.Seal = cusContainer.CO_Seal;
					xsdContainer.Seal2 = cusContainer.CO_SecondSeal;
					xsdContainer.WeightSpecified = true;
					xsdContainer.Weight = cusContainer.CO_Weight;

					RefContainer refContainer = cusContainer.Container;
					if (refContainer != null)
					{
						xsdContainer.ContainerType = new Xsd.ContainerType();
						xsdContainer.ContainerType.ISOCode = refContainer.RC_ISOType;
						xsdContainer.ContainerType.Height = refContainer.RC_Height;
						xsdContainer.ContainerType.HeightSpecified = true;
						xsdContainer.ContainerType.Length = refContainer.RC_Length;
						xsdContainer.ContainerType.LengthSpecified = true;
						xsdContainer.ContainerType.Width = refContainer.RC_Width;
						xsdContainer.ContainerType.WidthSpecified = true;
						xsdContainer.ContainerType.ContainerCode = refContainer.RC_Code;

						var usCode = refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
						if (usCode.IsEmpty)
						{
							usCode = refContainer.RC_USContainerCode;
						}
						xsdContainer.ContainerType.USContainerCode = usCode;
					}

					#region Container Processes
					CommonContainer jobContainer = cusContainer.JobContainer;
					if (cusContainer.JobContainer != null)
					{
						#region Export Process
						xsdContainer.ExportProcess.ReleaseNumber = jobContainer.JC_ReleaseNum;
						xsdContainer.ExportProcess.IsArrivingAtCTOByRail = jobContainer.JC_DepartureDeliveryByRail;
						xsdContainer.ExportProcess.IsArrivingAtCTOByRailSpecified = true;

						xsdContainer.ExportProcess.EmptyRequiredBy = jobContainer.JC_EmptyRequired;
						xsdContainer.ExportProcess.EstimatedFullPickup = jobContainer.JC_DepartureEstimatedPickup;
						xsdContainer.ExportProcess.CartageAdvised = jobContainer.JC_DepartureCartageAdvised;
						xsdContainer.ExportProcess.SlotBookingRef = jobContainer.JC_DepartureSlotReference;
						xsdContainer.ExportProcess.SlotDate = jobContainer.JC_DepartureSlotDateTime;
						xsdContainer.ExportProcess.CartageRef = jobContainer.JC_DepartureCartageRef;

						xsdContainer.ExportProcess.PickupEmptyFrom = new AddressValueObjectHelper(Res.GetString("5cbc9394-98bf-4c0c-98b9-3969dc041c6f", "Pickup Empty From for {0}", jobContainer.JC_ContainerNum)).ToAddressReference(jobContainer.DepartureContainerYardAddress, context);

						xsdContainer.ExportProcess.ContainerYardGateOut = jobContainer.JC_ContainerYardEmptyPickupGateOut;
						xsdContainer.ExportProcess.WharfGateIn = jobContainer.JC_FCLWharfGateIn;
						xsdContainer.ExportProcess.CartageComplete = jobContainer.JC_DepartureCartageComplete;
						xsdContainer.ExportProcess.ShippedOnboard = jobContainer.JC_FCLOnBoardVessel;
						xsdContainer.ExportProcess.DemurrageTime = jobContainer.DepartureTruckWaitTime;
						xsdContainer.ExportProcess.DemurrageCharge = jobContainer.DepartureTruckWaitCost;
						xsdContainer.ExportProcess.DemurrageChargeSpecified = true;
						#endregion

						#region Import Process
						xsdContainer.ImportProcess.FCLAvailable = jobContainer.JC_FCLAvailable;
						xsdContainer.ImportProcess.FCLStorage = jobContainer.JC_ArrivalCTOStorageStartDate;
						xsdContainer.ImportProcess.LCLAvailable = jobContainer.JC_LCLAvailable;
						xsdContainer.ImportProcess.LCLStorage = jobContainer.JC_LCLStorageCommences;
						xsdContainer.ImportProcess.WharfUnload = jobContainer.JC_FCLUnloadFromVessel;
						xsdContainer.ImportProcess.SlotBookingRef = jobContainer.JC_ArrivalSlotReference;
						xsdContainer.ImportProcess.SlotDate = jobContainer.JC_ArrivalSlotDateTime;
						xsdContainer.ImportProcess.CartageRef = jobContainer.JC_ArrivalCartageRef;
						xsdContainer.ImportProcess.WharfGateOut = jobContainer.JC_FCLWharfGateOut;
						xsdContainer.ImportProcess.EstimatedDelivery = jobContainer.JC_ArrivalEstimatedDelivery;
						xsdContainer.ImportProcess.CartageAdvised = jobContainer.JC_ArrivalCartageAdvised;
						xsdContainer.ImportProcess.CartageComplete = jobContainer.JC_ArrivalCartageComplete;

						xsdContainer.ImportProcess.DeliverEmptyTo = new AddressValueObjectHelper(Res.GetString("07b07811-ead4-4b5a-8001-acb00c8f5a75", "Deliver Empty To for {0}", jobContainer.JC_ContainerNum)).ToAddressReference(jobContainer.ArrivalContainerYardAddress, context);

						xsdContainer.ImportProcess.EmptyReady = jobContainer.JC_EmptyReadyForReturn;
						xsdContainer.ImportProcess.EmptyReturnRequiredBy = jobContainer.JC_EmptyReturnedBy;
						xsdContainer.ImportProcess.EmptyReturnedOn = jobContainer.JC_ContainerYardEmptyReturnGateIn;

						xsdContainer.ImportProcess.PickupByRail = jobContainer.JC_ArrivalPickupByRail;
						xsdContainer.ImportProcess.PickupByRailSpecified = true;
						xsdContainer.ImportProcess.HeldForFCLTransitStaging = jobContainer.JC_FCLHeldInTransitStaging;
						xsdContainer.ImportProcess.HeldForFCLTransitStagingSpecified = true;

						xsdContainer.ImportProcess.StorageDays = jobContainer.ArrivalCTOStorageDays.ToString();
						xsdContainer.ImportProcess.StorageDaysSpecified = true;
						xsdContainer.ImportProcess.StorageCharge = jobContainer.ArrivalCTOStorageCost;
						xsdContainer.ImportProcess.StorageChargeSpecified = true;

						xsdContainer.ImportProcess.DemurrageTime = jobContainer.ArrivalTruckWaitTime;
						xsdContainer.ImportProcess.DemurrageCharge = jobContainer.ArrivalTruckWaitCost;
						xsdContainer.ImportProcess.DemurrageChargeSpecified = true;

						xsdContainer.ImportProcess.DetentionDays = jobContainer.ArrivalCarrierDetentionDays.ToString();
						xsdContainer.ImportProcess.DetentionDaysSpecified = true;
						xsdContainer.ImportProcess.DetentionCharge = jobContainer.ArrivalCarrierDetentionCost;
						xsdContainer.ImportProcess.DetentionChargeSpecified = true;
						#endregion
					}
					#endregion

					#region Custom Fields
					xsdContainer.Custom.CustomAttribute1 = cusContainer.CO_CustomAttrib1;
					xsdContainer.Custom.Date1 = cusContainer.CO_CustomDate1;
					xsdContainer.Custom.Decimal1 = cusContainer.CO_CustomDecimal1;
					xsdContainer.Custom.Decimal1Specified = true;
					xsdContainer.Custom.Flag1 = cusContainer.CO_CustomFlag1;
					xsdContainer.Custom.Flag1Specified = true;
					#endregion
				}
			}
		}

		protected virtual void ExportARInvoices(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			if (IncludeARInvoicesToDeclarationXML)
			{
				ForwardingJobInvoicesExporter invoiceExporter = new ForwardingJobInvoicesExporter(jobDec.Factory);
				toShipment.ARInvoices = invoiceExporter.PopulateInvoicesToXSD(jobDec.JE_DeclarationReference, context, false);
			}
		}

		bool IncludeARInvoicesToDeclarationXML
		{
			get { return CustomsDataRegistry.Instance.IncludeARInvoicesInXMLFile.Value; }
		}

		void ExportCustomAttributes(Xsd.Shipment shipment, BaseJobDeclaration jobDec)
		{
			shipment.ShipmentDetails.Custom.CustomAttribute1 = jobDec.DocsAndCartage.JP_CustomAttrib1;
			shipment.ShipmentDetails.Custom.CustomAttribute2 = jobDec.DocsAndCartage.JP_CustomAttrib2;

			if (!jobDec.DocsAndCartage.JP_CustomDate1.IsEmpty)
			{
				shipment.ShipmentDetails.Custom.Date1 = jobDec.DocsAndCartage.JP_CustomDate1;
			}

			if (!jobDec.DocsAndCartage.JP_CustomDate2.IsEmpty)
			{
				shipment.ShipmentDetails.Custom.Date2 = jobDec.DocsAndCartage.JP_CustomDate2;
			}

			shipment.ShipmentDetails.Custom.Decimal1 = jobDec.DocsAndCartage.JP_CustomDecimal1;
			shipment.ShipmentDetails.Custom.Decimal2 = jobDec.DocsAndCartage.JP_CustomDecimal2;
			shipment.ShipmentDetails.Custom.Flag1 = jobDec.DocsAndCartage.JP_CustomFlag1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			shipment.ShipmentDetails.Custom.Flag2 = jobDec.DocsAndCartage.JP_CustomFlag2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			shipment.ShipmentDetails.Custom.Decimal1Specified = true;
			shipment.ShipmentDetails.Custom.Decimal2Specified = true;
			shipment.ShipmentDetails.Custom.Flag1Specified = true;
			shipment.ShipmentDetails.Custom.Flag2Specified = true;
		}

		protected virtual void ExportDeclarationAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobDeclaration jobDec, INotifications notification)
		{
		}

		void ExportPreAdviceIdentifier(BaseJobDeclaration jobDec, Xsd.Shipment shipmentValue, INotifications notify)
		{
			if (jobDec.PreAdvice != null)
			{
				Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
				identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.PreadviceIdentifier;
				identifier.Value = jobDec.PreAdvice.EF_PreshipID;
			}
		}

		#endregion

		#region GetNewInvoiceAdapter / GetNewGroupInvoiceAdapter

		protected virtual InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec)
		{
			return new InvoiceValueObjectDataAdapter(jobDec);
		}

		protected virtual GroupInvoiceValueObjectDataAdapter GetNewGroupInvoiceAdapter(BaseJobDeclaration jobDec)
		{
			return new GroupInvoiceValueObjectDataAdapter(jobDec);
		}

		#endregion

		#region Get Billing Data Adapter

		IValueObjectDataAdapter CreateBillingDataAdapter()
		{
			return (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.IBillingDataAdapter>());
		}

		#endregion

		Notes GetNotesBizo(BaseJobDeclaration jobDec)
		{
			Notes result;
			if (jobDec.Shipment == null)
			{
				result = jobDec.Notes;
			}
			else
			{
				result = jobDec.Shipment.Notes;
			}
			return result;
		}

		DocDataValueObjectDataAdapter DocDataValueObjectDataAdapter
		{
			get { return docDataValueObjectDataAdapter ?? (docDataValueObjectDataAdapter = new DocDataValueObjectDataAdapter()); }
		}
		DocDataValueObjectDataAdapter docDataValueObjectDataAdapter;
	}
}
