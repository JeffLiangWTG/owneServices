using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public class LineReleaseDeclarationCreator
	{
		public LineReleaseDeclarationCreator(LineReleaseMQEDIMessage message)
		{
			this.message = message;
			generator = new ABIOutputBlockControlGenerator();
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			lrlx10 = generator.MessageBlocks.OfType<LRLX10>().FirstOrDefault();
			if (lrlx10 == null)
			{
				throw new InvalidMessageFormatException("Entry Details record is missing.");
			}
			EnsureEntryFilerExists();
		}

		class BillData
		{
			public BillData(LRLX25 lrlx25)
			{
				UpdateBillDetails(lrlx25.MasterBillNumber, ref MasterBillIssuerSCAC, ref MasterBillNumber);
				UpdateBillDetails(lrlx25.HouseBillNumber, ref HouseBillIssuerSCAC, ref HouseBillNumber);
				UpdateBillDetails(lrlx25.SubHouseBillNumber, ref SubHouseBillIssuerSCAC, ref SubHouseBillNumber);
			}

			public ZString MasterBillIssuerSCAC;
			public ZString MasterBillNumber;
			public ZString HouseBillIssuerSCAC;
			public ZString HouseBillNumber;
			public ZString SubHouseBillIssuerSCAC;
			public ZString SubHouseBillNumber;

			void UpdateBillDetails(ZString issuerAndNumber, ref ZString issuerSCAC, ref ZString number)
			{
				ZString trimIssuerAndNumber = issuerAndNumber.TrimEnd();
				issuerSCAC = trimIssuerAndNumber.Left(4);
				number = trimIssuerAndNumber.SubstringSafe(4, 12);
			}
		}

		public JobDeclaration AddToShipment(ZGuid shipmentPK, BusinessObjectFactory factory)
		{
			JobDeclaration result = null;
			ForwardingShipment shipment = factory.Load<ForwardingShipment>(shipmentPK);
			if (shipment != null)
			{
				result = (JobDeclaration)JobDeclaration.Load(shipment);
				if (result == null)
				{
					result = factory.New<JobDeclaration>();
					result.JE_JS = shipment.PK;
					SetMessageLinkToDeclaration(factory, result);
					result.CreationSource = LineReleasePrefix + message.EM_ApplicationReference;
				}
				result.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Force));
				result.JE_OverrideFreightDefaults = true;
				LoadLineReleaseData(result);
			}
			return result;
		}

		public JobDeclaration CreateANewDeclaration()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration result = factory.New<JobDeclaration>();

			SetMessageLinkToDeclaration(factory, result);

			result.CreationSource = LineReleasePrefix + message.EM_ApplicationReference;
			LoadLineReleaseData(result);
			return result;
		}

		void SetMessageLinkToDeclaration(BusinessObjectFactory factory, JobDeclaration result)
		{
			var msg = GetMessageFromDeclarationFactory(factory, message.PK);
			if (msg != null)
			{
				msg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
				msg.EM_LinkUniqueID = result.PK;
			}
		}

		public readonly LineReleaseMQEDIMessage message;
		readonly ABIOutputBlockControlGenerator generator;
		readonly LRLX10 lrlx10;
		LineReleaseMQEDIMessage declarationFactoryMessage;
		const string LineReleasePrefix = "LR-";

		void LoadLineReleaseData(JobDeclaration declaration)
		{
			using (declaration.SuspendSetMasterBillForHandCarriedTransport())
			{
				UpdateDeclaration(declaration);
				JobComInvoiceHeader firstInvoice = null;
				var totalNoOfPackages = 0;
				foreach (var lrlx25 in generator.MessageBlocks.OfType<LRLX25>())
				{
					BillData billData = new BillData(lrlx25);
					Bill masterBill = null;
					Bill houseBill = null;
					Bill subHouseBill = null;
					GetExistingOrCreateNew(declaration, billData, ref masterBill, ref houseBill, ref subHouseBill);

					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					if (firstInvoice == null)
					{
						firstInvoice = invoice;
					}

					if (subHouseBill != null)
					{
						UpdateBillQuantity(subHouseBill, lrlx25.Quantity);
					}
					else if (houseBill != null)
					{
						UpdateBillQuantity(houseBill, lrlx25.Quantity);
					}
					else if (masterBill != null)
					{
						UpdateBillQuantity(masterBill, lrlx25.Quantity);
					}

					totalNoOfPackages += lrlx25.Quantity;
				}

				declaration.JE_TotalNoOfPacks = totalNoOfPackages;

				if (firstInvoice == null)
				{
					firstInvoice = declaration.Invoices.AddNew();
				}

				foreach (var lrlx20 in generator.MessageBlocks.OfType<LRLX20>())
				{
					AddInvoiceLines(firstInvoice, lrlx20);
				}

				if (!declaration.IsFormalImport)
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import; // in case of setting the importer has changed this.
				}

				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;
				declaration.US_CertifyCargoRelease = false;
				declaration.US_PGAExpeditedRelease = true;

				foreach (JobComInvoiceHeader invoice in declaration.Invoices)
				{
					if (invoice.JobComInvoiceLines.Count == 0)
					{
						invoice.JobComInvoiceLines.AddNew(); // add dummy for merge
					}
				}

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				UpdateReleaseDetails(declaration);
				SetMessageToComplete(declaration.Factory, message.PK);
			}
		}

		void EnsureEntryFilerExists()
		{
			if (USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode != lrlx10.EntryFilerCode)
			{
				throw new InvalidOperationException("The Entry Filer Code '" + lrlx10.EntryFilerCode + "' does not belong to this company.\r\nPlease check Admin -> System -> Registry -> " + ((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location);
			}
		}

		void SetMessageToComplete(BusinessObjectFactory factory, ZGuid messagePK)
		{
			var mess = GetMessageFromDeclarationFactory(factory, messagePK);
			if (mess != null)
			{
				mess.SetToComplete();
			}
		}

		LineReleaseMQEDIMessage GetMessageFromDeclarationFactory(IFactory factory, ZGuid messagePK)
		{
			return declarationFactoryMessage ?? (declarationFactoryMessage = factory.Load<LineReleaseMQEDIMessage>(messagePK));
		}

		void UpdateDeclaration(JobDeclaration declaration)
		{
			UpdateDataRelatedToImporterNumber(declaration, lrlx10.ImporterNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = lrlx10.EntryFilerCode;
			declaration.US_SchDEntry = lrlx10.DistrictPortOfEntry;
			declaration.US_SchDArrival = lrlx10.DistrictPortOfEntry;
			declaration.ImportEntryNumber = lrlx10.EntryNumber;
			UpdateTransportDetails(declaration);
			UpdateDataRelatedToFIRMSCode(declaration, lrlx10.FIRMSCode);
		}

		void UpdateTransportDetails(JobDeclaration declaration)
		{
			ZString transportMode = TransportTypeList.Codes.Truck;
			ZString containerMode = Core.Constants.ContainerModes.NonContainerised;

			var lrlx25 = generator.MessageBlocks.OfType<LRLX25>().FirstOrDefault(x => !x.ModeOfTransportationMOTCode.IsEmpty);
			if (lrlx25 != null)
			{
				transportMode = TransportTypeList.ConvertFromTransportCode(lrlx25.ModeOfTransportationMOTCode);
				containerMode = TransportModeCodes.IsContainerised(lrlx25.ModeOfTransportationMOTCode) ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
			}
			declaration.JE_TransportMode = transportMode;
			declaration.JE_ContainerMode = containerMode;
		}

		void UpdateReleaseDetails(JobDeclaration declaration)
		{
			var releaseDateTime = lrlx10.ReleaseDate.AddHours(ZInt.ParseEmptyAsZero(lrlx10.ReleaseTime.Left(2))).AddMinutes(ZInt.ParseEmptyAsZero(lrlx10.ReleaseTime.SubstringSafe(2, 2)));
			declaration.JE_ExportDate = releaseDateTime;
			declaration.JE_DateOfArrival = releaseDateTime;
			declaration.US_EntryDate = releaseDateTime;

			//only ever one entry created for non-exwarehouse imports
			CusEntryHeader[] entries = declaration.ActiveEntryHeaders.GetEntryWithType(Customs.Common.US.ImportMessageStatusList.MessageType.EntrySummary);
			if (entries.Length == 1)
			{
				declaration.JE_EntryAuthorisationDate = releaseDateTime;
			}
		}

		void UpdateBillQuantity(Bill bill, ZInt quantity)
		{
			bill.CU_NoOfPacks = new ZDecimal(quantity);
			PackingGroup packingGroup = bill.PackingGroups.Count > 0 ? bill.PackingGroups[0] : bill.PackingGroups.AddNew();
			Package package = null;
			if (packingGroup.Packages.Count > 0)
			{
				package = packingGroup.Packages[0];
			}
			if (package == null || !package.CW_PackQty.IsEmpty)
			{
				package = packingGroup.Packages.AddNew();
			}
			package.CW_PackQty = quantity;
		}

		void AddInvoiceLines(JobComInvoiceHeader invoice, LRLX20 lrlx20)
		{
			if (lrlx20 != null)
			{
				JobComInvoiceLine headerLine = invoice.JobComInvoiceLines.AddNew();
				UpdateManufacturerRelatedData(headerLine, lrlx20.ManufacturerSupplierCode);
				headerLine.JI_Tariff = lrlx20.TariffNumber1;
				headerLine.JI_InvoiceQuantity = new ZDecimal(lrlx20.Quantity);
				headerLine.JI_InvoiceUQ = lrlx20.UnitOfMeasure;
				headerLine.US_UC_NKCountryOfOrigin = lrlx20.CountryOfOrigin1;
				headerLine.US_UC_NKCountryOfExport = lrlx20.CountryOfOrigin1;
				if (!lrlx20.TariffNumber2.IsEmpty)
				{
					JobComInvoiceLine secondaryLine = invoice.JobComInvoiceLines.AddNew();
					secondaryLine.JI_Tariff = lrlx20.TariffNumber2;
					secondaryLine.JI_ParentID = headerLine.PK;
				}
			}
		}

		void UpdateManufacturerRelatedData(JobComInvoiceLine invoiceLine, ZString manufacturerID)
		{
			if (!manufacturerID.IsEmpty)
			{
				ZDBOnlySubQuery orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCusCodeSchema.OK_OH);
				orgQuery.AddToFilter(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
				ZDBOnlyQuery registrationQuery = new ZDBOnlyQuery(typeof(OrgCusCode));
				registrationQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				registrationQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, manufacturerID);
				registrationQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
				registrationQuery.AddSubQuery(orgQuery, JoinCondition.And);
				OrgCusCode cusCode = invoiceLine.Factory.LoadTop1<OrgCusCode>(registrationQuery);
				if (cusCode != null)
				{
					invoiceLine.JI_OA_ManufacturerAddress = cusCode.OK_OA_PremisesAddress;
				}
			}
		}

		void GetExistingOrCreateNew(JobDeclaration declaration, BillData billData, ref Bill masterBill, ref Bill houseBill, ref Bill subHouseBill)
		{
			GetExistingBills(declaration, billData, ref masterBill, ref houseBill, ref subHouseBill);
			if (masterBill == null && !billData.MasterBillNumber.IsEmpty)
			{
				masterBill = declaration.Bills.AddNew();
				masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				masterBill.CU_BillNum = billData.MasterBillNumber;
				masterBill.US_UI_NKBillIssuerSCAC = billData.MasterBillIssuerSCAC;
			}
			if (houseBill == null && !billData.HouseBillNumber.IsEmpty)
			{
				houseBill = masterBill.ChildBills.AddNew();
				houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill.CU_BillNum = billData.HouseBillNumber;
				houseBill.US_UI_NKBillIssuerSCAC = billData.HouseBillIssuerSCAC;
			}
			if (subHouseBill == null && !billData.SubHouseBillNumber.IsEmpty)
			{
				subHouseBill = houseBill.ChildBills.AddNew();
				subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
				subHouseBill.CU_BillNum = billData.SubHouseBillNumber;
				subHouseBill.US_UI_NKBillIssuerSCAC = billData.SubHouseBillIssuerSCAC;
			}
		}

		void GetExistingBills(JobDeclaration declaration, BillData billData, ref Bill masterBill, ref Bill houseBill, ref Bill subHouseBill)
		{
			masterBill = declaration.Bills.FindByBillNumberAndType(billData.MasterBillNumber, Customs.Business.BillTypeList.Codes.MasterBill);
			if (masterBill != null)
			{
				if (!billData.HouseBillNumber.IsEmpty)
				{
					Bill[] houseBills = (Bill[])masterBill.ChildBills.Find(new ZQuery(CusDecHouseBillSchema.CU_BillNum, billData.HouseBillNumber));
					if (!billData.SubHouseBillNumber.IsEmpty)
					{
						foreach (Bill bill in houseBills)
						{
							Bill[] subHouseBills = (Bill[])bill.ChildBills.Find(new ZQuery(CusDecHouseBillSchema.CU_BillNum, billData.SubHouseBillNumber));
							if (subHouseBills.Length > 0)
							{
								subHouseBill = subHouseBills[0];
								houseBill = bill;
							}
						}
						if (houseBill == null && houseBills.Length > 0)
						{
							houseBill = houseBills[0];
						}
					}
					else if (houseBills.Length > 0)
					{
						houseBill = houseBills[0];
					}
				}
			}
		}

		void UpdateDataRelatedToFIRMSCode(JobDeclaration declaration, ZString fIRMSCode)
		{
			if (!fIRMSCode.IsEmpty)
			{
				declaration.US_US_NKLocationOfGoods = fIRMSCode;
			}
		}

		void UpdateDataRelatedToImporterNumber(JobDeclaration declaration, ZString importerNumber)
		{
			BusinessObjectFactory factory = declaration.Factory;
			OrgHeader organisation = null;
			ZString[] matchType = new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber };
			if (GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNoMatching(matchType) == importerNumber)
			{
				organisation = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			}
			else
			{
				ZDBOnlyQuery registrationQuery = new ZDBOnlyQuery(typeof(OrgCusCode));
				registrationQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				registrationQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, importerNumber);
				registrationQuery.AddToFilter(new ZQuery(OrgCusCodeSchema.OK_CodeType, matchType));

				ZDBOnlySubQuery orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgCusCodeSchema.OK_OH);
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingLine, ZBool.True);
				orgQuery.AddToFilter(query);

				registrationQuery.AddSubQuery(orgQuery, JoinCondition.And);
				OrgCusCode cusCode = factory.LoadTop1<OrgCusCode>(registrationQuery);
				if (cusCode != null)
				{
					organisation = cusCode.Header;
				}
			}

			if (organisation != null)
			{
				if (organisation.OH_IsConsignee)
				{
					declaration.JE_OH_Importer = organisation.PK;
				}
				if (organisation.OH_IsShippingLine)
				{
					declaration.JE_OH_ShippingLine = organisation.PK;
				}
				declaration.IOROrgPK = organisation.PK;
			}
		}
	}
}
