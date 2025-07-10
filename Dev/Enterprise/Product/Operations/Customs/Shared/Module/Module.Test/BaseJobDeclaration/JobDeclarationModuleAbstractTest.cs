using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class JobDeclarationModuleAbstractTest : ZArchitecture.Modules.Testing.ZModuleBasherWithFetchHintsTest
	{
		public void TestElementType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(GetExpectedJobDeclarationType(), module.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.InstanceType ?? module.GetElementType());
			}
		}

		[RequiresSTA]
		public void TestShowNewExWarehouseEndToEnd()
		{
			using (TestHelperBaseJobDeclarationModule module = new TestHelperBaseJobDeclarationModule())
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					module.HasExWarehouseMenuExposed = true;
					JobDeclarationController controller = null;
					try
					{
						var menuItems = module.GetNewStandardMenuItemsTest();
						menuItems.FindByText("&New").MenuItems.FindByText("New &Ex-warehouse").PerformClick();
						controller = (JobDeclarationController)((IFilterGridModuleInternalsForTesting)module).LastController;
						AssertNotNull("Controller created", controller);
						AssertNotNull("Form shown", controller.LastShownForm);
						AssertEquals("ExWarehouse form", Customs.Business.JobMessageTypeList.Codes.ExWarehouse, ((BaseJobDeclaration)((ZForm)controller.LastShownForm).BusinessEntity).JE_MessageType);
					}
					finally
					{
						controller?.LastShownForm?.Dispose();
					}
				}
			}
		}

		public void TestUniversalCopyType()
		{
			using (var manager = new UniversalCopyManagerForTest(GetExpectedJobDeclarationType(), GetModuleID()))
			{
				var inoivceHeaderPath = new string[] { "JobComInvoiceHeaders" };
				var testInvoiceHeaderNote = new CollectionCopyTemplateNode();
				var invoiceHeaderInnerNode = new EntityCopyTemplateNode(GetExpectedInvoiceHeaderType());
				invoiceHeaderInnerNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobComInvoiceHeaderSchema.JZ_RN_NKCountryOfExport.Name, CopyMethod = CopyMethod.Copy, PropertyType = JobComInvoiceHeaderSchema.JZ_RN_NKCountryOfExport.DotNetType.Name });
				testInvoiceHeaderNote.Name = "JobComInvoiceHeaders";
				testInvoiceHeaderNote.ItemsTableName = JobComInvoiceHeaderSchema.Constants.TableName;
				testInvoiceHeaderNote.ItemPropertyName = JobComInvoiceHeaderSchema.Constants.JZ_JE;
				testInvoiceHeaderNote.InnerNode = invoiceHeaderInnerNode;
				manager.GetFilter(testInvoiceHeaderNote, inoivceHeaderPath);
				AssertEquals("Get type of invoice header as expected", GetExpectedInvoiceHeaderType(), manager.GetComponentTypeFromPath(inoivceHeaderPath, true));
			}

			using (var manager = new UniversalCopyManagerForTest(GetExpectedInvoiceHeaderType(), GetModuleID()))
			{
				var inoivceLinePath = new string[] { "JobComInvoiceLines" };
				AssertEquals("Get type of invoice line as expected", GetExpectedInvoiceLineType(), manager.GetComponentTypeFromPath(inoivceLinePath, true));
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.JobDeclaration;

		protected abstract Type GetExpectedJobDeclarationType();

		protected abstract Type GetExpectedInvoiceHeaderType();

		protected abstract Type GetExpectedInvoiceLineType();

		public void TestCreateAndImportDeclarationFromAnotherDeclaration()
		{
			using (var module = (JobDeclarationModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertType(GetExpectedImportJobDeclarationType_ForCreateAndImportDeclarationFromAnotherDeclaration(), module.GetImportJobDeclarationForCreateAndImportDeclarationFromAnotherDeclaration());
			}
		}

		protected virtual Type GetExpectedImportJobDeclarationType_ForCreateAndImportDeclarationFromAnotherDeclaration() => typeof(ImportJobDeclaration);

		protected void CheckMenuItemExists(string menuText)
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleID))
			{
				Menu.MenuItemCollection menuItemCollection = ((JobDeclarationModule)module).FormActionMenu.FindByText("&Actions").MenuItems;
				MenuItem menuItem = null;
				foreach (MenuItem item in menuItemCollection)
				{
					if (item.Text == menuText)
					{
						menuItem = item;
						break;
					}
				}

				AssertNotNull(menuItem);
			}
		}

		#region TestFetchHintsHaveBeenAddedIfNeeded
		protected override List<string> FetchHintIgnoreField => new List<string> { "RelatedTransportBookingsJobNumbers" };

		protected override bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			var hasFailedFetchHint = base.HasFailedFetchHint(tableSelect);
			if (tableSelect.TableName == OrgAddressAdditionalInfoSchema.Constants.TableName && tableSelect.Value <= 8)
			{
				hasFailedFetchHint = false;
			}

			return hasFailedFetchHint;
		}

		protected sealed override void SetupDataForFetchHintsTest()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}

			var newFactory = new BusinessObjectFactory();
			foreach (CodeDescriptionPair pair in newFactory.GetCachedValue<JobMessageTypeList>())
			{
				for (int i = 0; i < 7; i++)
				{
					CreateDeclarationForFetchHintTest(newFactory, pair.Code, i);
				}
			}

			newFactory.Save();
		}

		protected sealed override ZFilterModule CreateModuleForFetchHintsTest()
		{
			return (JobDeclarationModule)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected virtual BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var organisations = factory.GetCachedValue("BaseOrganisationDeclarationModuleTest", delegate
			{
				var subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				subQuery.AddToFilter(OrgAddressSchema.OA_State, SQLComparisonOperator.NotEqual, "");
				var dbOnlyResult = new ZDBOnlyQuery(typeof(OrgHeader));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);
				return factory.Load<OrgHeader>(new ZQuery(dbOnlyResult)
				{ MaximumRows = 30 });
			});
			var vessels = factory.GetCachedValue("BaseVesselDeclarationModuleTest", delegate
			{
				return factory.Load<RefVessel>(new ZQuery()
				{ MaximumRows = 6 });
			});
			var unlocos = factory.GetCachedValue("BaseUNLOCODeclarationModuleTest", delegate
			{
				return factory.Load<RefUNLOCO>(new ZQuery()
				{ MaximumRows = 11 });
			});
			return CreateDeclarationForFetchHintTest(factory, messageType, i, organisations, vessels, unlocos);
		}

		BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i, OrgHeader[] organisations, RefVessel[] vessels, RefUNLOCO[] unlocos)
		{
			string number = i.ToString();
			string twoDigitsNumber = number.PadLeft(2, '0');
			int mod6 = i % 6;
			int mod3 = i % 3;
			var declaration = factory.New<BaseJobDeclaration>();
			SetJE_ApplicationCode(declaration);
			declaration.JE_OH_Supplier = organisations[i % organisations.Length].PK;
			declaration.JE_OH_Importer = organisations[(i + 1) % organisations.Length].PK;
			declaration.JE_OH_Forwarder = declaration.JE_OH_Supplier;
			declaration.JE_OH_ControllingAgent = organisations[(i + 2) % organisations.Length].PK;
			declaration.JE_OH_ControllingCustomer = organisations[(i + 3) % organisations.Length].PK;
			declaration.JE_OH_ExternalBroker = organisations[(i + 4) % organisations.Length].PK;
			declaration.JE_OH_ShippingLine = organisations[(i + 5) % organisations.Length].PK;
			declaration.JE_MessageType = messageType;
			var lookups = declaration.Lookups;
			var transportTypeList = lookups.TransportTypeList;
			declaration.JE_TransportMode = transportTypeList[i % transportTypeList.Count].Code;
			declaration.JE_VesselName = vessels[i % vessels.Length].RV_Code;
			declaration.JE_VoyageFlightNo = "VF" + number;
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMinutes(i);
			declaration.JE_HouseBill = "HB" + twoDigitsNumber;
			declaration.JE_AgentsReference = "AGF" + twoDigitsNumber;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_AuditDateUtc = new ZDateTime(2021, 05, 18).AddDays(i);
			declaration.JE_GS_NKAuditUser = GlbStaff.CurrentUser.GS_Code;
			declaration.JE_AuditReference = "RE" + i.ToString();
			var cargoIdTypeList = lookups.CargoIdTypeList;
			declaration.JE_ContainerMode = cargoIdTypeList.Count == 0 ? "C" + twoDigitsNumber : cargoIdTypeList[i % cargoIdTypeList.Count].Code;
			declaration.JE_ContainerCount = (ZShort)i;
			declaration.JE_DateOfFirstArrival = ZDateTime.Now.AddHours(i);
			declaration.JE_EFTMode = "E" + twoDigitsNumber;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddMinutes(-i - 1);
			var entryStatusList = lookups.EntryStatusList;
			declaration.JE_EntryStatus = entryStatusList.Count == 0 ? "E" + twoDigitsNumber : entryStatusList[i % entryStatusList.Count].Code;
			declaration.JE_EntrySubmittedDate = ZDateTime.Today.AddDays(i);
			declaration.JE_ExportDate = ZDateTime.Today.AddMinutes(i + 1);
			var exportGoodsType_List = lookups.JE_ExportGoodsType_List;
			declaration.JE_ExportGoodsType = exportGoodsType_List.Count == 0 ? "G" + twoDigitsNumber : exportGoodsType_List[i % exportGoodsType_List.Count].Code;
			declaration.JE_GoodsDescription = "GOODS " + twoDigitsNumber;
			declaration.JE_MasterBill = "MB" + twoDigitsNumber;
			var messageSubTypeList = lookups.MessageSubTypeList;
			declaration.JE_MessageSubType = messageSubTypeList.Count == 0 ? "M" + twoDigitsNumber : messageSubTypeList[i % messageSubTypeList.Count].Code;
			declaration.JE_OwnerRef = "OWNR" + twoDigitsNumber;
			declaration.JE_TotalNoOfPacks = i;
			var totalNoOfPacksPackType_List = lookups.JE_TotalNoOfPacksPackType_List;
			declaration.JE_TotalNoOfPacksPackType = totalNoOfPacksPackType_List.Count == 0 ? "P" + twoDigitsNumber : totalNoOfPacksPackType_List[i % totalNoOfPacksPackType_List.Count].Code;
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "WUP";
			declaration.DeliveryOrPickupCartageCoPK = organisations[(i + 5) % organisations.Length].PK;
			SetupDeclarantForFetchHintTest(i, organisations, declaration);

			CusEntryHeader entry;
			if (declaration.CustomsEntryHeaders.Count > 0)
			{
				entry = declaration.CustomsEntryHeaders[0];
			}
			else
			{
				entry = declaration.CustomsEntryHeaders.AddNew();
			}

			var messageStatusList = entry.Lookups.MessageStatusList;
			entry.CH_Status = messageStatusList.Count == 0 ? "C" + twoDigitsNumber : messageStatusList[i % messageStatusList.Count].Code;
			entry.CH_PhaseStatus = "015";
			if (declaration.Branch.Company.GC_RN_NKCountryCode != "US")
			{
				entry.CH_EntryReleaseDate = ZDateTime.Today.AddDays(i - 2);
			}

			entry.CH_EntrySubmittedDate = ZDateTime.Today.AddDays(i - 1);
			entry.CH_TotalPaid = 32340m + i;
			entry.EntryNumber = "EN" + twoDigitsNumber;
			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddMinutes(i);
			var warehouseTransactionStatusList = lookups.WarehouseTransactionStatusList;
			entry.CH_WarehouseTransactionStatus = warehouseTransactionStatusList[i % warehouseTransactionStatusList.Count].Code;
			declaration.JE_TotalVolume = i;
			declaration.JE_TotalWeight = i;
			declaration.JE_MessageStatus = entry.CH_Status;
			declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			var lastMilestone = declaration.WorkflowItems.Milestones.AddNew();
			lastMilestone.TriggerConditions.TriggerEventCode = "E" + number;
			lastMilestone.P9_Description = "IMP" + number;
			lastMilestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			var order = declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "ORD" + number;
			var orderQuery = new ZQuery(Enterprise.ZArchitecture.Schema.JobOrderHeaderSchema.JD_OrderNumber, order.JD_OrderNumber);
			orderQuery.AddToFilter(Enterprise.ZArchitecture.Schema.JobOrderHeaderSchema.JD_OA_BuyerAddress, order.JD_OA_BuyerAddress);
			orderQuery.AddToFilter(Enterprise.ZArchitecture.Schema.JobOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, order.PK);
			orderQuery.FetchOnlyFromLocalCache = true;
			order.JD_OrderNumberSplit = (ZByte)(order.Factory.Load(order.GetType(), orderQuery).Length + 1);
			var nextMilestone = declaration.WorkflowItems.Milestones.AddNew();
			nextMilestone.P9_Description = "NXT" + number;
			nextMilestone.TriggerConditions.TriggerEventCode = "M" + number;
			nextMilestone.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today.AddDays(13));
			declaration.Logs.AddNew(Events.RecordAudited, "Record Audited.");
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;
			container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			var jobHeader = CreateJobHeader(declaration.Factory, declaration, i);
			AddJobCharge(factory, jobHeader);

			return declaration;
		}

		protected virtual void SetupDeclarantForFetchHintTest(int i, OrgHeader[] organisations, BaseJobDeclaration declaration)
		{
			declaration.JE_OA_DeclarantAddress = organisations[(i + 6) % organisations.Length].Addresses.MainAddress.PK;
		}

		protected virtual void SetJE_ApplicationCode(BaseJobDeclaration declaration)
		{
		}

		JobHeader CreateJobHeader(BusinessObjectFactory factory, BaseJobDeclaration declaration, int i)
		{
			var jobHeader = factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_JobNum = "TEST" + declaration.JE_MessageType + i.ToString(CultureInfo.InvariantCulture);
			jobHeader.JH_Status = JobStatuses[i % JobStatuses.Length];
			jobHeader.JH_HoldReason = "test";
			jobHeader.JH_ProfitLossReasonCode = "USR";

			return jobHeader;
		}

		static void AddJobCharge(BusinessObjectFactory factory, JobHeader jobHeader)
		{
			var chargeCode = factory.NewWithValidTestData<AccChargeCode>();

			var apTransactionLine = factory.NewWithValidTestData<AccTransactionLines>();
			apTransactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			apTransactionLine.AL_LineType = TransactionLineTypes.Accrual;
			apTransactionLine.AL_JH = jobHeader.PK;
			apTransactionLine.AL_AC = chargeCode.PK;
			apTransactionLine.AL_LineAmount = apTransactionLine.AL_OSAmount = 100.000M;

			var arTransactionLine = factory.NewWithValidTestData<AccTransactionLines>();
			arTransactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			arTransactionLine.AL_LineType = TransactionLineTypes.WIP;
			arTransactionLine.AL_JH = jobHeader.PK;
			arTransactionLine.AL_AC = chargeCode.PK;
			arTransactionLine.AL_LineAmount = arTransactionLine.AL_OSAmount = 110.000M;

			var jobCharge = factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_AL_APLine = apTransactionLine.PK;
			jobCharge.JR_AL_ARLine = arTransactionLine.PK;
			jobCharge.JR_OH_SellAccount = jobHeader.LocalChargesPK;
			jobCharge.JR_OSSellAmt = jobCharge.JR_LocalSellAmt = 110.000M;
			jobCharge.JR_OSCostAmt = jobCharge.JR_LocalCostAmt = 100.000M;
		}

		string[] JobStatuses
		{
			get
			{
				return Factory.GetCachedValue("DeclarationModuleTestJobStatuses", () => new JobHeaderStatusList().GetAllCodes());
			}
		}

		protected AccTransactionHeader CreateTransactionData(BaseJobDeclaration declaration, int i)
		{
			BusinessObjectFactory factory = declaration.Factory;
			ZDecimal invoiceAmount = i * 100;
			AccTransactionHeader accTransHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			accTransHeader.AH_JH = declaration.Job.PK;
			accTransHeader.AH_Ledger = "AR";
			accTransHeader.AH_TransactionCategory = "DBT";
			accTransHeader.AH_InvoiceAmount = invoiceAmount;
			accTransHeader.AH_OutstandingAmount = invoiceAmount;
			accTransHeader.AH_TransactionType = "INV";
			CreateTransactionLineData(factory, declaration, accTransHeader, invoiceAmount);
			return accTransHeader;
		}

		void CreateTransactionLineData(BusinessObjectFactory factory, BaseJobDeclaration declaration, AccTransactionHeader accTransHeader, ZDecimal amount)
		{
			AccTransactionLines accTransLine1 = factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine1.AL_AH = accTransHeader.PK;
			accTransLine1.AL_LineAmount = amount * 0.25m;
			accTransLine1.AL_AC = testChgCode.PK;
			AccTransactionLines accTransLine2 = factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine2.AL_AH = accTransHeader.PK;
			accTransLine2.AL_LineAmount = amount * 0.25m;
			accTransLine2.AL_AC = testChgCode2.PK;
			AccTransactionLines accTransLine3 = factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine3.AL_AH = accTransHeader.PK;
			accTransLine3.AL_LineAmount = amount * 0.5m;
			accTransLine3.AL_AC = testChgCode3.PK;
		}

		protected void CreateChgCodes(BusinessObjectFactory factory)
		{
			if (!chgCodeCreated.HasValue)
			{
				testChgCode = factory.NewWithValidTestData<AccChargeCode>();
				testChgCode.AC_ChargeType = "DSB";
				testChgCode.AC_Code = "TST";
				testChgCode.AC_GC = GlbCompany.CurrentCompany.PK;
				testChgCode2 = factory.NewWithValidTestData<AccChargeCode>();
				testChgCode2.AC_ChargeType = "DSB";
				testChgCode2.AC_Code = "TST2";
				testChgCode2.AC_GC = GlbCompany.CurrentCompany.PK;
				testChgCode3 = factory.NewWithValidTestData<AccChargeCode>();
				testChgCode3.AC_ChargeType = "TST";
				testChgCode3.AC_Code = "TST3";
				testChgCode3.AC_GC = GlbCompany.CurrentCompany.PK;
				chgCodeCreated = true;
			}
		}

		bool? chgCodeCreated;
		AccChargeCode testChgCode;
		AccChargeCode testChgCode2;
		AccChargeCode testChgCode3;
		#endregion
	}
}
