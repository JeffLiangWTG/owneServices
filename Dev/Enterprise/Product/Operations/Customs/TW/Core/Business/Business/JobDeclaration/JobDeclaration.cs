using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobDeclaration : AutoTWJobDeclaration,
		Integration.Customs.TW.IJobDeclaration,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IMessageManageableBizObj,
		IApportionInvoiceHolder,
		IReservedFieldSupporter,
		IInvoicesProvider,
		ICusEntryNumberParent,
		IEntryNumberGeneratorProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : AutoTWJobDeclaration.Schema
		{
			public const string TotalDeclarationPackingNetWeightInKilograms = nameof(JobDeclaration.TotalDeclarationPackingNetWeightInKilograms);
			public const string TotalDeclarationPackingGrossWeightInKilograms = nameof(JobDeclaration.TotalDeclarationPackingGrossWeightInKilograms);
			public const string DeclarationDate = nameof(JobDeclaration.DeclarationDate);
			public const string ClearanceStatus = nameof(JobDeclaration.ClearanceStatus);
			public const string ImporterChineseName = nameof(JobDeclaration.ImporterChineseName);
			public const string SupplierChineseName = nameof(JobDeclaration.SupplierChineseName);
			public const string DeclarationType = nameof(JobDeclaration.DeclarationType);
			public const string AgencyResponseCode = nameof(JobDeclaration.AgencyResponseCode);
			public const string RequiredFormalitiesCode = nameof(JobDeclaration.RequiredFormalitiesCode);
			public const string ClearanceCode = nameof(JobDeclaration.ClearanceCode);
			public const string DeclarationNumberDisplay = nameof(JobDeclaration.DeclarationNumberDisplay);
		}
		#endregion

		#region Properties

		protected override bool IsDefaultSupplierPickupAddressEnabled => SupplierPickupAddress.DocAddressType != DocAddressType.ConsignorPickupDeliveryAddress;

		protected override bool SupportsCusPackingListCore => TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.Value;

		protected override ZString EntryStatusNotSendCode => EntryStatusCodeList.Codes.NotReceive;

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TWTransportCodeList))]
		[ResourceStringData("60C50C59-14A7-46E6-A163-B46BB647364B", Caption = "Transport Code", FullDescription = "The code of the cargo's transport mode.")]
		public ZString JE_Calc_TWTransportCode => CommonHelper.GetTWTransportCode(JE_TransportMode, JE_ContainerMode);

		public ZPropertyInfo JE_Calc_TWTransportCodeInfo => GetZPropertyInfo(nameof(JE_Calc_TWTransportCode));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_VesselArrivalReg", Caption = "Vessel Reg.", FullDescription = "The vessel registration number of Customs.")]
		public override ZString JE_VesselArrivalReg { get => base.JE_VesselArrivalReg; set => base.JE_VesselArrivalReg = value; }

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && JE_TransportMode != oldValue)
				{
					JE_VesselArrivalReg = ZString.Empty;

					switch (JE_TransportMode)
					{
						case TransportTypeList.Codes.Sea:
							JE_ContainerMode = ContainerModeList.Codes.Containerized;
							InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invoiceLine => { invoiceLine.JI_HazMatCode = invoiceLine.JI_HazMatCode.SubstringSafe(0, UNDGSubstance.Schema.DG_CodeMaxLength); });
							break;
						case TransportTypeList.Codes.Air:
							JE_ContainerMode = ContainerModeList.Codes.Loose;
							break;
					}

					if (ShouldResetSplitMarkToFalse)
					{
						JE_SplitMark = false;
					}
				}
			}
		}

		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_CustomsOffice", Caption = "Transport Code", FullDescription = "The code of the cargo's transport mode.")]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set
			{
				var oldValue = JE_CustomsOffice;
				base.JE_CustomsOffice = value;
				if (!IsCopying && JE_CustomsOffice != oldValue)
				{
					SetLocationOfGoodsIfNeeded();
					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_LocationOfGoods();
					}
				}
			}
		}

		void SetLocationOfGoodsIfNeeded()
		{
			if (IsExport)
			{
				var locationOfGoodsCode = RegistryHelper.GetDefaultGoodsLocation(JE_CustomsOffice, JE_MessageType);
				if (!locationOfGoodsCode.IsEmpty)
				{
					JE_LocationOfGoods = locationOfGoodsCode;
				}
			}
		}

		public ZZRefCusCodeListCombined CustomsOffice => TWRefCusCodeListLoader.GetCustomsOffice(Factory, JE_CustomsOffice, DateOfValuation);

		[ResourceStringData("2c9d0c41-92de-4785-92fb-af1e6d157042", Caption = "Goods Location", FullDescription = "Goods Location of Export/Office of Lading/Unlading.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCollection))]
		[RelatedBusinessObject("LocationOfGoods")]
		[MaxLength(8)]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_LocationOfGoods))
				{
					var oldValue = JE_LocationOfGoods;
					base.JE_LocationOfGoods = value;
					if (!IsCopying && oldValue != JE_LocationOfGoods)
					{
						if (CusEntryInstruction.CEI_GoodsLocation.IsEmpty)
						{
							CusEntryInstruction.CEI_GoodsLocation = JE_LocationOfGoods.SubstringSafe(0, CusEntryInstruction.CEI_GoodsLocationInfo.MaxLength);
						}
						Invoices?.ForEach(x => x.MarkAsNeedingValidation());
						InvoiceLines?.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}

		public ZZRefCusCodeListCombined LocationOfGoods => TWRefCusCodeListLoader.GetLocationOfGoods(Factory, JE_LocationOfGoods, DateOfValuation);

		protected override bool SupportMultipleWarehouseEntryCore => true;
		public override bool SupportInvoiceLineRefs => true;

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				bool hasChanged = base.JE_GB != value;
				base.JE_GB = value;
				if (hasChanged && !IsCopying)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				bool hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
					MarkFeesAsNeedingValidation();
				}
			}
		}

		public override OrgHeader NewOwner => CustomsEntryInstructionProvider.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault(x => x.Owner != null)?.Owner;

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var hasChanges = JE_MessageType != value;
				if (!IsCopying && hasChanges)
				{
					NeedToGetNewIncoTermAndChargeFactory = true;
					foreach (var invoice in Invoices)
					{
						invoice.NeedToGetNewIncoTermAndChargeFactory = true;
					}
					if (IsExport)
					{
						CustomsEntryInstructionProvider.CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x => x.CEI_GoodsLocation = ZString.Empty);
					}
					Invoices.MarkAsNeedingValidation();
					ThrowAwayEntryNumber();
				}
				if (hasChanges)
				{
					base.JE_MessageType = value;
					RefreshBinding_CusEntryInstruction_TradersRemarks();
				}
			}
		}

		void RefreshBinding_CusEntryInstruction_TradersRemarks()
		{
			if ((!fCusEntryInstruction?.IsDeleted) ?? false)
			{
				fCusEntryInstruction.TW_TradersRemarksInfo.RefreshBinding();
			}
		}

		string IApportionInvoiceHolder.CountryContext
		{
			get
			{
				var incoTerms = Invoices.InvoiceIncoterms;
				var result = ZString.Empty;
				if (incoTerms.Length > 0)
				{
					result = incoTerms.FirstOrDefault(x => x == Core.Constants.IncoTerms.ExWorks);

					if (result.IsEmpty)
					{
						result = incoTerms.First();
					}
				}
				return CountryCode + this.GetIncoTermChargeFactoryCacheKey(result);
			}
		}

		public ZDateTime DeclarationDate => CusEntryInstruction.CEI_DateForDuty;

		#endregion

		internal void LoadBeforeValationAll()
		{
			SupplierDocumentaryAddress?.SetupLocalAddress();
			ImporterDocumentaryAddress?.SetupLocalAddress();
		}

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);
			ResetValuesinInvoiceLinesForJE_MessageTypeChanged();
			JE_DeclDocType = ZString.Empty;
			if (IsImport)
			{
				JE_LocationOfGoods = ZString.Empty;
			}
			else
			{
				SetLocationOfGoodsIfNeeded();
				JE_OtherBankAccount = ZString.Empty;
			}
			CustomsEntryInstructionProvider.CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x =>
			{
				x.SetDefaultValueForGoodsLocation();
			});

			if (ShouldResetSplitMarkToFalse)
			{
				JE_SplitMark = false;
			}

			CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(entryInstruction =>
			{
				entryInstruction.SetZeroTW_DaysOfDelayedDeclarationIfNeeded();
				entryInstruction.ResetUCRNumber();
				entryInstruction.SetDefaultCEI_Style();
			});
			DefaultValuesFromOrganziationConfig();
			DefaultGuaranteeFromOrganziationConfigIfRequired();
		}

		void ResetValuesinInvoiceLinesForJE_MessageTypeChanged()
		{
			RefreshExRateToLatestRateAvailableIfNeeded();
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(t =>
			{
				if (IsImport)
				{
					t.JI_Procedure = ZString.Empty;
					t.JI_BondedGoodsCode = ZString.Empty;
					t.SetDefaultPrimaryPreferenceValue();
				}
				else if (IsExport)
				{
					t.JI_Procedure = ZString.Empty;
					t.JI_PrimaryPreference = ZString.Empty;
					t.JI_EPTDigit1 = ZString.Empty;
					t.JI_EPTDigit2 = ZString.Empty;
					t.JI_EPTDigit3 = ZString.Empty;
					t.Taxes.RemoveAndDeleteAll();
				}
				else
				{
					t.JI_PrimaryPreference = ZString.Empty;
					t.JI_EPTDigit1 = ZString.Empty;
					t.JI_EPTDigit2 = ZString.Empty;
					t.JI_EPTDigit3 = ZString.Empty;
					t.Taxes.RemoveAndDeleteAll();
				}

				t.JI_UseOneTenthCV = ZBool.False;
				t.SetDefaultRAPRORValuesIfNeeded();
			});
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new CusEntryInstructionCollection CustomsEntryInstructions => CustomsEntryInstructionProvider.CustomsEntryInstructions;

		public CusEntryInstruction CusEntryInstruction
		{
			get
			{
				if (fCusEntryInstruction?.IsDeleted ?? true)
				{
					fCusEntryInstruction = CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault();
					if (fCusEntryInstruction == null)
					{
						fCusEntryInstruction = CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
						using (CusEntryInstruction.GetValidationSuspender())
						{
							fCusEntryInstruction.SetDefaultCEI_Style();
							fCusEntryInstruction.CEI_CustomsOffice = RegistryHelper.DefaultCustomsOfficeCode;
							fCusEntryInstruction.CEI_DateForDuty = ZDateTime.Today;
						}
					}
					if (fCusEntryInstruction != null)
					{
						RegisterEditableChildObject(fCusEntryInstruction);
						if (!IsPersistent)
						{
							fCusEntryInstruction.MakeNonPersistent(); // Otherwise, the stupid declaration-faker thing for standalone invoices would leave a CEI without a JE, which cannot be saved
						}
					}
				}
				return fCusEntryInstruction;
			}
		}
		CusEntryInstruction fCusEntryInstruction;

		public ZString DeclarationType => CusEntryInstruction?.CEI_Style ?? ZString.Empty;

		[ChildEditable(true)]
		public GovernmentUniformInvoiceCollection GovernmentUniformInvoices
		{
			get
			{
				if (governmentUniformInvoices == null)
				{
					governmentUniformInvoices = new GovernmentUniformInvoiceCollection(this);
					governmentUniformInvoices.Load();
					RegisterEditableChildObject(governmentUniformInvoices);
				}
				return governmentUniformInvoices;
			}
		}

		GovernmentUniformInvoiceCollection governmentUniformInvoices;

		[ChildEditable(true)]
		public JobDeclarationReservedFieldCollection ReservedFields
		{
			get
			{
				if (jobDeclarationReservedFieldCollection == null)
				{
					jobDeclarationReservedFieldCollection = new JobDeclarationReservedFieldCollection(this);
					jobDeclarationReservedFieldCollection.Load();
					RegisterEditableChildObject(jobDeclarationReservedFieldCollection);
				}
				return jobDeclarationReservedFieldCollection;
			}
		}
		JobDeclarationReservedFieldCollection jobDeclarationReservedFieldCollection;

		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					JE_Z99FinalDestination = ZString.Empty;
					CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(cusEntryInstruction => { cusEntryInstruction.SetDutyRefundBaseOnFinalDestinationAndDeclarationType(); });
					SetInvoiceHeadersIncoTermPlace(false);
				}
			}
		}

		public override ZString JE_Z99FinalDestination
		{
			get => base.JE_Z99FinalDestination;
			set
			{
				var oldValue = JE_Z99FinalDestination;
				base.JE_Z99FinalDestination = value;
				if (!IsCopying && oldValue != JE_Z99FinalDestination)
				{
					if (IsZ99FinalDestination && !JE_Z99FinalDestination.IsEmpty)
					{
						SetInvoiceHeadersIncoTermPlace(true);
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_TotalNoOfPacks", Caption = "Total Packages No.", FullDescription = "Total number of packages on declaration or of the shipment delivered.")]
		public override ZInt JE_TotalNoOfPacks
		{
			get => base.JE_TotalNoOfPacks;
			set
			{
				var oldValue = JE_TotalNoOfPacks;
				base.JE_TotalNoOfPacks = value;
				if (!IsCopying && JE_TotalNoOfPacks != oldValue)
				{
					Invoices?.MarkAsNeedingValidation();
				}
			}
		}

		#region Bill Type List
		protected override ICodeDescriptionPairList GetBillTypeListCore()
		{
			var key = GetCU_BillTypeListKey();
			return Factory.GetCachedValue("TWBillCU_BillTypeList" + key, () =>
			{
				var result = new CodeDescriptionPairList();
				if (key == "BaseTypes")
				{
					result.AddRange(base.GetBillTypeListCore());
				}
				else if (key == "IncludeCN")
				{
					result.AddRange(base.GetBillTypeListCore());
					result.AddPair(BillTypeList.Codes.ContainerNote, BillTypeList.Descriptions.ContainerNote);
				}
				else if (key == "OnlyCN")
				{
					result.AddPair(BillTypeList.Codes.ContainerNote, BillTypeList.Descriptions.ContainerNote);
				}

				return result;
			});
		}

		bool ShouldIncludeCNInBillTypeList => JE_HouseBill.IsEmpty && JE_MasterBill.IsEmpty && Bills.Count == 0;

		bool ShouldOnlyHasCN => Bills?.Cast<Bill>().Any(x => x.CU_BillType == BillTypeList.Codes.ContainerNote) ?? false;

		string GetCU_BillTypeListKey()
		{
			string result = "BaseTypes";
			if (ShouldIncludeCNInBillTypeList)
			{
				result = "IncludeCN";
			}
			else if (ShouldOnlyHasCN)
			{
				result = "OnlyCN";
			}
			return result;
		}
		#endregion

		[ChildEditable(true)]
		public BondedFactoryCollection BondedFactories => BondedFactoriesCore;

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.GOVUniformInvoice, typeof(GovernmentUniformInvoiceData) },
				{ CusCodeDataTypeList.Codes.ReservedField, typeof(JobDeclarationReservedField) },
				{ CusCodeDataTypeList.Codes.Itinerary, typeof(ItineraryData) }
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public new JobDeclarationDocumentSupporter DocumentSupporter => (JobDeclarationDocumentSupporter)base.DocumentSupporter;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

		bool ShouldResetSplitMarkToFalse => !(IsImport && IsAir);

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CusAgentsTWList))]
		[ResourceStringData("ED2EC2F4-3F2A-4744-BD41-826B5D902ED1", Caption = "Broker Staff", ShortCaption = "Broker Staff", FullDescription = "The broker staff assigned on declaration.")]
		public override ZString JE_GS_NKCusAgent
		{
			get => base.JE_GS_NKCusAgent;
			set
			{
				var oldValue = JE_GS_NKCusAgent;
				base.JE_GS_NKCusAgent = value;
				if (!IsCopying && oldValue != JE_GS_NKCusAgent)
				{
					if (!JE_GS_NKCusAgent.IsEmpty)
					{
						JE_CustomsProfile = Lookups.CustomsProfileList.OfType<ICodeDescription>().Select(x => x.Code).FirstOrDefault() ?? ZString.Empty;
					}
					else
					{
						JE_CustomsProfile = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("217BF5B3-3641-4E19-ACB7-A204D9262A76", Caption = "Broker License", FullDescription = "The Broker License issued by customs to the qualified customs broker.")]
		public ZString CusAgentCertificateNumber => CusAgent?.GetValidTWBrokerCertificateNumber(ZDateTime.Today) ?? ZString.Empty;

		public ZPropertyInfo CusAgentCertificateNumberInfo => GetZPropertyInfo(nameof(CusAgentCertificateNumber));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsProfileList))]
		[ReadOnlyMember(nameof(JE_CustomsProfileReadOnly))]
		[ResourceStringData("0EBA972A-851C-4A36-B835-24A1BDC3BA48", Caption = "Mail Box", FullDescription = "The Mail Box code issued by the third party network company and the sub box number issued by Customs to the customs broker.")]
		[MaxLength(35)]
		public override ZString JE_CustomsProfile { get => base.JE_CustomsProfile; set => base.JE_CustomsProfile = value; }

		public bool JE_CustomsProfileReadOnly
		{
			get { return JE_GS_NKCusAgent.IsEmpty; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CaseNoList))]
		[ResourceStringData("8BBA9A27-AA88-40EC-9C6E-324A3CB88521", Caption = "Guarantee", FullDescription = "The number issued by customs for post-release duty payment type of declaration.")]
		[MaxLength(12)]
		public override ZString JE_DefermentAccountNumber { get => base.JE_DefermentAccountNumber; set => base.JE_DefermentAccountNumber = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PaymentMethodsList))]
		[ResourceStringData("0447C26C-F0F4-4809-93BE-156EFE413CE0", Caption = "Payment Method", FullDescription = "The payment method of the declaration. The default can be set against the Importer's/Exporter's Organization > Details > Configuration > Taiwan.")]
		[MaxLength(1)]
		public override ZString JE_PaymentMethod { get => base.JE_PaymentMethod; set => base.JE_PaymentMethod = value; }

		public new DeclarationLevelPackageCollection Packages => (DeclarationLevelPackageCollection)base.Packages;

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new DeclarationLevelPackageCollection(this);

		protected override bool PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevelCore => true;

		protected override bool ShouldDefaultPackingInfoFromDeclarationToBillsCore => false;

		protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;

		[ResourceStringData("D210CD92-3418-4EF1-9D38-266CD19B5528", Caption = "Bank Account", FullDescription = "The bank account for direct debit. When payment method is set to \"2\", this field must be entered.")]
		public override ZString JE_OtherBankAccount { get => base.JE_OtherBankAccount; set => base.JE_OtherBankAccount = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_SplitMark", Caption = "Split Shipment", FullDescription = "Tick the box for split shipment declaration.")]
		public override ZBool JE_SplitMark { get => base.JE_SplitMark; set => base.JE_SplitMark = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MergeByList))]
		public override ZString JE_MergeBy { get => base.JE_MergeBy; set => base.JE_MergeBy = value; }

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				var oldValue = JE_OH_Importer;
				using (SuspendDefaultProcess())
				{
					base.JE_OH_Importer = value;
				}

				if (!IsCopying && oldValue != JE_OH_Importer)
				{
					ImporterDocumentaryAddress?.MarkAsNeedingValidation();
					DefaultValuesFromOrganziationConfig();
					DefaultGuaranteeFromOrganziationConfigIfRequired();
				}
			}
		}

		void DefaultValuesFromOrganziationConfig()
		{
			if (!IsDefaultingFromOrganziationConfigSuspended)
			{
				var orgHeader = IsImport ? Importer : Supplier;
				if (orgHeader != null)
				{
					DefaultValuesFromOrgImpAddInfo(orgHeader);
					DefaultValuesFromCompanyData(orgHeader);
				}
			}
		}

		void DefaultValuesFromCompanyData(OrgHeader orgHeader)
		{
			if (JE_PaidBy.IsEmpty)
			{
				JE_PaidBy = orgHeader.CompanyData.OB_CusPaidBy;
			}
		}

		void DefaultValuesFromOrgImpAddInfo(OrgHeader orgHeader)
		{
			var orgImpAddInfo = TWOrgImpAddInfo.Get(orgHeader);
			if (orgImpAddInfo != null)
			{
				if (JE_PaymentMethod.IsEmpty)
				{
					JE_PaymentMethod = GetPaymentMethodFromTWOrgImpAddInfo(orgImpAddInfo);
				}
				if (CusEntryInstruction.CEI_ExamMode.IsEmpty)
				{
					CusEntryInstruction.CEI_ExamMode = orgImpAddInfo.ZO_TWDefaultExamMode;
				}
			}
		}

		void DefaultGuaranteeFromOrganziationConfigIfRequired()
		{
			if (!IsDefaultingFromOrganziationConfigSuspended && IsImport && JE_DefermentAccountNumber.IsEmpty)
			{
				OrgHeader org = null;
				if (Importer == null)
				{
					org = DeclarantAddress?.Header;
				}
				else
				{
					switch (TWOrgImpAddInfo.Get(Importer).ZO_TWDefaultIMPPaymentMethod)
					{
						case IMPPaymentMethod.Codes._2:
						case IMPPaymentMethod.Codes._5:
						case IMPPaymentMethod.Codes._7:
							org = DeclarantAddress?.Header;
							break;
						case IMPPaymentMethod.Codes._4:
						case IMPPaymentMethod.Codes._6:
						case IMPPaymentMethod.Codes._8:
							org = Importer;
							break;
					}
				}

				if (org != null)
				{
					JE_DefermentAccountNumber = org.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.PBR);
				}
			}
		}

		ZString GetPaymentMethodFromTWOrgImpAddInfo(TWOrgImpAddInfo orgImpAddInfo) => IsImport ? orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod : orgImpAddInfo.ZO_TWDefaultEXPPaymentMethod;

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				var oldValue = JE_OH_Supplier;
				var supplierPk = value;
				using (SuspendDefaultProcess())
				{
					base.JE_OH_Supplier = supplierPk;
				}
				if (!IsCopying && oldValue != JE_OH_Supplier)
				{
					Invoices.Cast<JobComInvoiceHeader>().ForEach(x => SetInvoiceSupplier(x, supplierPk));
					SupplierDocumentaryAddress?.MarkAsNeedingValidation();
					DefaultValuesFromOrganziationConfig();
				}
			}
		}

		protected void SetInvoiceSupplier(JobComInvoiceHeader invoiceHeader, ZGuid supplierPk)
		{
			if (invoiceHeader != null && !invoiceHeader.IsDeleted)
			{
				invoiceHeader.JZ_OH_Supplier = supplierPk;
			}
		}

		[RelatedBusinessObject("IntermConsignee")]
		public override ZGuid JE_OH_Consignee
		{
			get => base.JE_OH_Consignee;
			set
			{
				var oldValue = JE_OH_Consignee;
				base.JE_OH_Consignee = value;
			}
		}

		public OrgHeader OrgConsignee => Factory.Load<OrgHeader>(JE_OH_Consignee);

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		#region Implementation

		protected override bool IsUNDGSupportedOnInvoiceLines => true;

		public override JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new TWJobDocAddressDependentCollection(this);
					var addressTypeFilter = new ZQuery(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.NotEqual, DocAddressTypes.Codes.BondedFactory);
					addressTypeFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.NotEqual, DocAddressTypes.Codes.ConsignorAddress);
					addressTypeFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.NotEqual, DocAddressTypes.Codes.ConsigneeAddress);
					fDocAddresses.Load(addressTypeFilter);
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		protected virtual BondedFactoryCollection BondedFactoriesCore
		{
			get
			{
				if (bondedFactoryJobDocAddresses == null)
				{
					bondedFactoryJobDocAddresses = new BondedFactoryCollection(this);
					RegisterEditableChildObject(bondedFactoryJobDocAddresses);
				}
				return bondedFactoryJobDocAddresses;
			}
		}

		protected BondedFactoryCollection bondedFactoryJobDocAddresses;

		protected override ZString LocalCurrencyCodeCore
		{
			get { return Core.Constants.CurrencyCodes.Taiwan; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Core.Constants.CurrencyCodes.Taiwan; }
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains(Core.Constants.CountryCodes.Taiwan))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JE_PaymentMethod = ZString.Empty;
			JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
			if (defaultBrokerStaff != null)
			{
				JE_GS_NKCusAgent = defaultBrokerStaff.BrokerStaffCode;
				JE_CustomsProfile = defaultBrokerStaff.Mailbox;
			}
			var address = GlbBranch.CurrentBranch.OrgProxy?.MainAddress;
			if (address != null && address.IsInDatabase)
			{
				JE_OA_DeclarantAddress = address.PK;
			}
		}

		protected override string DefaultTotalNoOfPacksPackType => Core.Constants.PkgUnit.Carton;

		protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			RefreshExRateToLatestRateAvailableIfNeeded();
			return base.DoMergeCore(notifier);
		}
		#endregion

		public MasterFiles.Business.GlbExternalPassword GetCredential()
		{
			MasterFiles.Business.GlbExternalPassword result = null;

			var customsProfile = JE_CustomsProfile;
			var cusAgent = CusAgent;
			if (!customsProfile.IsEmpty && cusAgent != null)
			{
				result = cusAgent.GetCredential(customsProfile);
			}
			return result;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(t => t.UpdateUCR());

			if (IsAir)
			{
				JE_SLD = ZString.Empty;
			}

			if (allocateEntryNumberOnSaving)
			{
				AllocateNextEntryNumber();
			}
			else if (allocateEntryNumberToEntryOnSaving)
			{
				EntryHeader?.AllocateEntryNumber();
			}
		}

		public override ZBool BondedWarehouseEditable => false;

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public override void SetSynchroniserFieldsReadOnly(ZBool isReadOnly)
		{
		}

		#region IMessageManageableBizObj Members
		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		IEnumerable<ReservedField> IReservedFieldSupporter.GetReservedFields()
		{
			return ReservedFields.OrderList;
		}

		public bool IsInAStatusAmendmentSendable => false;
		#endregion

		public override ZDateTime DateOfValuation
		{
			get
			{
				var result = CusEntryInstruction.CEI_DateForDuty;
				if (!result.IsValid)
				{
					result = ZDateTime.Today;
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_SLD|Export", Caption = "SO No", FullDescription = "The shipping order number.", MultipleKey = ExportCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_SLD|Import", Caption = "Manifest", FullDescription = "The manifest number.", MultipleKey = ImportCaptionKey)]
		public override ZString JE_SLD { get => base.JE_SLD; set => base.JE_SLD = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_DateAtOrigin", Caption = "ETD", FullDescription = "The export date on the bills.")]
		public override ZDateTime JE_DateAtOrigin { get => base.JE_DateAtOrigin; set => base.JE_DateAtOrigin = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_DateAtFinalDestination", Caption = "ETA", FullDescription = "The import date of the shipment.")]
		public override ZDateTime JE_DateAtFinalDestination { get => base.JE_DateAtFinalDestination; set => base.JE_DateAtFinalDestination = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_TotalWeight", Caption = "Total Weight", FullDescription = "Total gross weight on declaration. The unit of measurement is KGM. When the unit of measurement entered is not KGM, it will be converted into KGM for declaration.")]
		public override ZDecimal JE_TotalWeight
		{
			get => base.JE_TotalWeight;
			set
			{
				var oldValue = JE_TotalWeight;
				base.JE_TotalWeight = value;
				if (oldValue != JE_TotalWeight)
				{
					EntryHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_TotalWeightUnit
		{
			get => base.JE_TotalWeightUnit;
			set
			{
				var oldValue = JE_TotalWeightUnit;
				base.JE_TotalWeightUnit = value;
				if (oldValue != JE_TotalWeightUnit)
				{
					EntryHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool IsContainerInvoiceLinkRelevantCore => false;

		protected override bool IsContainerPackingRequiredCore => false;

		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				var oldValue = JE_RL_NKOrigin;
				base.JE_RL_NKOrigin = value;
				if (!IsCopying && oldValue != JE_RL_NKOrigin)
				{
					JE_Z99PortOfOrigin = ZString.Empty;
					SetInvoiceHeadersIncoTermPlace(false);
				}
			}
		}

		public override ZGuid JE_JS
		{
			get => base.JE_JS;
			set
			{
				var oldValue = JE_JS;
				base.JE_JS = value;
				if (!IsCopying && oldValue != JE_JS)
				{
					Packages.MarkAsNeedingValidation();
					FilteredInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool JE_OverrideFreightDefaults
		{
			get => base.JE_OverrideFreightDefaults;
			set
			{
				var oldValue = JE_OverrideFreightDefaults;
				base.JE_OverrideFreightDefaults = value;
				if (!IsCopying && oldValue != JE_OverrideFreightDefaults)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool JE_MessageType_ReadOnlyCore => base.JE_MessageType_ReadOnlyCore || EntryNumberReadOnly;

		bool EntryNumberReadOnly
		{
			get
			{
				return CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault()?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false;
			}
		}

		public RefCountry PortOfOriginCountry => Origin?.Country ?? Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, JE_RL_NKOrigin.Left(2));

		public RefCountry FinalDestinationCountry => FinalDestination?.Country ?? Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, JE_RL_NKFinalDestination.Left(2));

		public ZBool IsZ99PortOfOrigin => JE_RL_NKOrigin.EndsWith(Constants.Z99, StringComparison.OrdinalIgnoreCase);

		public ZBool IsZ99FinalDestination => JE_RL_NKFinalDestination.EndsWith(Constants.Z99, StringComparison.OrdinalIgnoreCase);

		public ZString PortOfOriginName => GetPortName(IsZ99PortOfOrigin, Origin, JE_Z99PortOfOrigin);

		public ZString FinalDestinationName => GetPortName(IsZ99FinalDestination, FinalDestination, JE_Z99FinalDestination);

		ZString GetPortName(bool isZ99Port, RefUNLOCO unloco, ZString z99PortName)
		{
			return isZ99Port ? z99PortName : (unloco?.RL_PortName ?? ZString.Empty);
		}

		public ZString PortOfOriginProperName => GetProperName(IsZ99PortOfOrigin, Origin, JE_Z99PortOfOrigin);

		public ZString FinalDestinationProperName => GetProperName(IsZ99FinalDestination, FinalDestination, JE_Z99FinalDestination);

		ZString GetProperName(bool isZ99Port, RefUNLOCO unloco, ZString z99PortName)
		{
			return isZ99Port ? z99PortName : (unloco?.RL_NameWithDiacriticals ?? ZString.Empty);
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|TotalDeclarationPackingGrossWeightInKilograms", Caption = "Total Packing Gross Weight")]
		public ZDecimal TotalDeclarationPackingGrossWeightInKilograms => Packages.Cast<Package>().Sum(x => x.GrossWeightInKilograms);

		public ZPropertyInfo TotalDeclarationPackingGrossWeightInKilogramsInfo => GetZPropertyInfo(Schema.TotalDeclarationPackingGrossWeightInKilograms);

		public ZDecimal TotalDeclarationPackingNetWeightInKilograms => Factory.GetValue(ref totalDeclarationPackingNetWeightInKilogramsCached, () =>
					{
						var result = Packages.Cast<Package>().Sum(x => x.NetWeightInKilograms);
						return Utilities.Round(result, 3);
					});

		CachedProperty<ZDecimal> totalDeclarationPackingNetWeightInKilogramsCached;

		public ZPropertyInfo TotalDeclarationPackingNetWeightInKilogramsInfo => GetZPropertyInfo(Schema.TotalDeclarationPackingNetWeightInKilograms);

		#region IInvoicesProvider Members

		bool IInvoicesProvider.IsImport => IsImport;

		#endregion

		public ZString CustomsMessageType
		{
			get
			{
				var result = ZString.Empty;
				if (IsExport)
				{
					result = MessageTypeList.Codes.ECD;
				}
				else if (IsImport)
				{
					result = MessageTypeList.Codes.ICD;
				}
				return result;
			}
		}

		public CusEntryHeader EntryHeader => CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_EntryStatus", Caption = "Entry Status", FullDescription = "The entry status of the declaration.")]
		public override ZString JE_EntryStatus { get => base.JE_EntryStatus; set => base.JE_EntryStatus = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_MessageStatus", Caption = "Message Status", FullDescription = "The message status of the declaration.")]
		public override ZString JE_MessageStatus { get => base.JE_MessageStatus; set => base.JE_MessageStatus = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_DeclDocType", Caption = "Decl. Doc. Type", FullDescription = "The printing document type of import/export declaration.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclDocTypeList))]
		public override ZString JE_DeclDocType { get => base.JE_DeclDocType; set => base.JE_DeclDocType = value; }

		public ZString DeclDocTypeCodeAndDescription
		{
			get
			{
				var result = ZString.Empty;
				var declDocType = JE_DeclDocType;
				if (!declDocType.IsEmpty)
				{
					result = FormattableString.Invariant($"{declDocType}-{Lookups.DeclDocTypeList.GetDescriptionFromCode(declDocType)}");
				}
				return result;
			}
		}

		protected override void OnInvoicesCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			base.OnInvoicesCollectionCountChange(sender, e);
			RefreshIncotermAndChargeFactory();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockEntryNumberAllocationMutex();
				OnElementReset();
			}
			else
			{
				ClearEntryNumberFieldsWhenFactorySaveFailed();
			}
			allocateEntryNumberOnSaving = false;
			allocateEntryNumberToEntryOnSaving = false;
		}

		void ClearEntryNumberFieldsWhenFactorySaveFailed()
		{
			if (allocateEntryNumberOnSaving)
			{
				EntryNumber = ZString.Empty;
			}
			else if (allocateEntryNumberToEntryOnSaving)
			{
				var entryHeader = EntryHeader;
				if (entryHeader != null)
				{
					entryHeader.EntryNumber = ZString.Empty;
					EntryNumber = ZString.Empty;
				}
			}
		}

		public override void Delete()
		{
			UnlockEntryNumberAllocationMutex();
			DeleteAllCusEntryNumbers();
			base.Delete();
		}

		public override ZString DeclarationNumber
		{
			get
			{
				var result = EntryNumber;
				if (result.IsEmpty)
				{
					result = EntryHeader?.EntryNumber ?? ZString.Empty;
				}
				return result;
			}
			set => base.DeclarationNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|DeclarationNumberDisplay", Caption = "Entry Number", FullDescription = "The entry number of the customs declaration. The number range for entry number allocation can be set up against the login company profile.")]
		public ZString DeclarationNumberDisplay => Factory.GetValue(ref fDeclarationNumberDisplayCached, () =>
		{
			var result = EntryNumber;
			if (result.IsEmpty)
			{
				var entry = EntryHeader;
				result = entry?.EntryNumber ?? ZString.Empty;
				if (!CommonHelper.IsMatchEntryNumber(this, result) && !CommonHelper.IsWaitingForResponseOrHasBeenLodgedAtCustoms(entry))
				{
					result = ZString.Empty;
				}
			}
			return CommonHelper.FormattedEntryNumber(result);
		});

		CachedProperty<ZString> fDeclarationNumberDisplayCached;

		public ZPropertyInfo DeclarationNumberDisplayInfo => GetZPropertyInfo(nameof(DeclarationNumberDisplay));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|ClearanceStatus", Caption = "Clearance Status", FullDescription = "The clearance status of the declaration.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ClearanceStatusList))]
		public ZString ClearanceStatus => EntryHeader?.CusEntryNumber?.CE_EntryStatus ?? ZString.Empty;

		public ZPropertyInfo ClearanceStatusInfo => GetZPropertyInfo(Schema.ClearanceStatus);

		#region Entry Number
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString EntryNumber
		{
			get
			{
				return CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				if (EntryNumber != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayEntryNumber();
					}
					else
					{
						var entryNumber = CusEntryNumber ?? CreateCusEntryNumber();
						entryNumber.CE_EntryNum = value;
					}
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}
				EntryHeader?.DeleteAllCusEntryNumbers();
				EntryNumberInfo.RefreshBinding();
			}
		}

		void ThrowAwayEntryNumber()
		{
			if (CusEntryNumber != null)
			{
				CusEntryNumber.Delete();
			}
		}

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(nameof(EntryNumber));

		internal CusEntryNumber CusEntryNumber
		{
			get
			{
				if (fEntryNumber == null || fEntryNumber.IsDeleted)
				{
					fEntryNumber = LoadCusEntryNumber();
				}
				return fEntryNumber;
			}
		}
		CusEntryNumber fEntryNumber;

		internal void ReloadEntryNumber()
		{
			if (IsInDatabase)
			{
				var oldValue = (fEntryNumber == null || fEntryNumber.IsDeleted) ? ZString.Empty : fEntryNumber.CE_EntryNum;
				if (fEntryNumber == null || fEntryNumber.IsDeleted)
				{
					fEntryNumber = LoadCusEntryNumber(true);
				}
				else if (fEntryNumber.IsInDatabase)
				{
					fEntryNumber.Reload();
				}
				if (fEntryNumber != null && !fEntryNumber.IsDeleted && fEntryNumber.CE_EntryNum != oldValue)
				{
					EntryNumberInfo.RefreshBinding(oldValue);
				}
			}

			var entryNumber = EntryNumber;
			if (!entryNumber.IsEmpty && DefaultEntryNumber != entryNumber)
			{
				DefaultEntryNumber = entryNumber;
			}
		}

		internal ZString DefaultEntryNumber { get; set; }

		internal void RefreshEntryNumberRrelevantInfo()
		{
			DeclarationNumberDisplayInfo.RefreshBinding();
			EntryNumberGenerator.New(this)?.ValidationPropertyInfos?.ForEach(x => x.RefreshBinding());
		}

		CusEntryNumber CreateCusEntryNumber()
		{
			return CusEntryNumber.New(this, EntryNumberType, CountryCode);
		}

		CusEntryNumber LoadCusEntryNumber(bool reLoadExistingRows = false)
		{
			return CusEntryNumber.Load(this, EntryNumberType, CountryCode, reLoadExistingRows);
		}

		public ZString EntryNumberType
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsImport)
				{
					result = CusEntryNumberTypes.Taiwan.Import;
				}
				else if (IsExport)
				{
					result = CusEntryNumberTypes.Taiwan.Export;
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|AgencyResponseCode", Caption = "Agency Response Code")]
		public ZString AgencyResponseCode => ZString.Join(",", ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(c => c.CusDispositions.Where(d => d.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && d.CDI_StatusKey == CusDispositionStatusKeyList.Codes.ARM).Select(e => e.CDI_Status)).ToArray());

		public ZPropertyInfo AgencyResponseCodeInfo => GetZPropertyInfo(nameof(AgencyResponseCode));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|RequiredFormalitiesCode", Caption = "Required Formalities Code")]
		public ZString RequiredFormalitiesCode => ZString.Join(",", ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(c => c.CusDispositions.Where(d => d.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && d.CDI_StatusKey == CusDispositionStatusKeyList.Codes.RFM).Select(e => e.CDI_Status)).ToArray());

		public ZPropertyInfo RequiredFormalitiesCodeInfo => GetZPropertyInfo(nameof(RequiredFormalitiesCode));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|ClearanceCode", Caption = "Clearance Code")]
		public ZString ClearanceCode => ZString.Join(",", ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(c => c.CusDispositions.Where(d => d.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && d.CDI_StatusKey == CusDispositionStatusKeyList.Codes.CLR).Select(e => e.CDI_Status)).ToArray());

		public ZPropertyInfo ClearanceCodeInfo => GetZPropertyInfo(nameof(ClearanceCode));

		public void AllocateEntryNumber(string userEnteredEntryNumber)
		{
			allocateEntryNumberOnSaving = string.IsNullOrEmpty(userEnteredEntryNumber);
			if (!allocateEntryNumberOnSaving)
			{
				EntryNumber = userEnteredEntryNumber;
				CusEntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}
		bool allocateEntryNumberOnSaving;

		public bool AllocateEntryNumberToEntry() => allocateEntryNumberToEntryOnSaving = !EntryNumber.IsEmpty || (EntryHeader?.EntryNumber ?? ZString.Empty).IsEmpty;
		bool allocateEntryNumberToEntryOnSaving;

		protected void AllocateNextEntryNumber()
		{
			var entryNumber = EntryNumberGenerator.New(this)?.GenerateEntryNumber() ?? ZString.Empty;
			if (!entryNumber.IsEmpty)
			{
				EntryNumber = entryNumber;
				CusEntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}

		void DeleteAllCusEntryNumbers()
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			Factory.Load<CusEntryNumber>(filter).DeleteAll();
		}
		#endregion

		#region Allocation Mutex Lock
		ZGlobalMutex EntryNumberAllocationMutex
		{
			get { return entryNumberAllocationMutex ?? (entryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, BaseEntryNumberGenerator.CustomsStmNumsType + PK.ToString())); }
		}
		ZGlobalMutex entryNumberAllocationMutex;

		public void UnlockEntryNumberAllocationMutex()
		{
			if (entryNumberAllocationMutex != null && entryNumberAllocationMutex.IsLocked && entryNumberAllocationMutex.HasLock)
			{
				entryNumberAllocationMutex.Unlock();
			}
		}

		public bool LockEntryNumberAllocationMutex
		{
			get { return EntryNumberAllocationMutex.IsLocked ? (bool)EntryNumberAllocationMutex.HasLock : EntryNumberAllocationMutex.Lock(); }
		}

		public string GetEntryNumberAllocationMutexLockInfo() => EntryNumberAllocationMutex.GetMutexLockByInfo();
		#endregion

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		public DocManagerInfo TrademarkDocManagerInfo => (IsPluggedIntoShipment && Shipment != null) ? Shipment.DocManagerInfo : DocManagerInfo;

		#region ICusEntryNumberParent
		bool ICusEntryNumberParent.CanBeChangedOrDeleted(Common.CusEntryNumber entryNumber, out string errMsg)
		{
			errMsg = string.Empty;
			return string.IsNullOrEmpty(errMsg);
		}

		void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
		{
			Logs.AddNew(Events.CustomsNumberEntered, EntryNumber);
		}

		string ICusEntryNumberParent.EntryNumberChangedCallStack => ZString.Empty;
		#endregion

		protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid je_oh_supplier)
		{
		}

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			var supplier = ZGuid.Empty;
			var supplierDocumentaryAddress = SupplierDocumentaryAddress;
			if (supplierDocumentaryAddress != null)
			{
				supplier = supplierDocumentaryAddress.E2_AddressOverride ? ZGuid.Empty : supplierDocumentaryAddress.OrganisationPK;
			}
			if (JE_OH_Supplier != supplier)
			{
				JE_OH_Supplier = supplier;
			}

			if (IsExport)
			{
				RefreshBinding_CusEntryInstruction_TradersRemarks();
			}
		}

		protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid je_oh_importer)
		{
		}

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			var importerDocumentaryAddress = ImporterDocumentaryAddress;
			var importer = (importerDocumentaryAddress?.E2_AddressOverride ?? ZBool.True) ? ZGuid.Empty : importerDocumentaryAddress.OrganisationPK;
			if (JE_OH_Importer != importer)
			{
				JE_OH_Importer = importer;
			}

			if (IsImport)
			{
				RefreshBinding_CusEntryInstruction_TradersRemarks();
			}
		}

		void SetInvoiceHeadersIncoTermPlace(bool appendZ99)
		{
			Invoices.Cast<JobComInvoiceHeader>().ForEach(x => x.SetIncoTermPlaceIfRequired(appendZ99));
		}

		protected override void PopulateBrokerWhenLogCustomsCommenced(StmALog mostRecentCommencedLog)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobDeclaration|JE_OA_DeclarantAddress", Caption = "Declarant")]
		public override ZGuid JE_OA_DeclarantAddress { get => base.JE_OA_DeclarantAddress; set => base.JE_OA_DeclarantAddress = value; }

		protected override bool IsPackingInformationRelevantCore => !TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.Value;

		public IDisposable SuspendDefaultProcess()
		{
			return new DefaultingFromOrganziationConfigSuspender(this);
		}

		bool IsDefaultingFromOrganziationConfigSuspended
		{
			get { return defaultProcessIndex > 0; }
		}

		int defaultProcessIndex;
		class DefaultingFromOrganziationConfigSuspender : IDisposable
		{
			public DefaultingFromOrganziationConfigSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.defaultProcessIndex++;
			}

			readonly JobDeclaration declaration;

			public void Dispose()
			{
				declaration.defaultProcessIndex--;
			}
		}

		public override ZString ImporterName => ImporterDocumentaryAddress.CompanyName;

		public override ZString SupplierName => SupplierDocumentaryAddress.CompanyName;

		public ZString ImporterChineseName => ImporterDocumentaryAddress.CompanyChineseName;

		public ZString SupplierChineseName => SupplierDocumentaryAddress.CompanyChineseName;

		internal void MarkFeesAsNeedingValidation() => ActiveEntryHeaders?.Cast<CusEntryHeader>()
														.ForEach(header => header.AllEntryLines.Cast<CusEntryLine>()
														.ForEach(line => line.Fees.MarkAsNeedingValidationIncludingChildren()));

		protected override bool ShouldLogEventIfJE_EntryStatusChanged => false;

		protected override Type PackingListType => typeof(CusPackingList);

		public void CalculateDuties()
		{
			DutyCalculatorStrategy.CalculateDuties();
		}

		public bool DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark => DutyCalculatorStrategy.AnyEntryLineHasChangesSinceLastMark;

		public DutyCalculatorStrategy DutyCalculatorStrategy
		{
			get { return dutyCalculatorStrategy ?? (dutyCalculatorStrategy = new DutyCalculatorStrategy(this)); }
		}
		DutyCalculatorStrategy dutyCalculatorStrategy;

		public bool IsFreeTradeZoneDocumentaryAddress => FreeTradeZoneDocumentaryAddress.IsFreeTradeZone;

		TWJobDocAddress FreeTradeZoneDocumentaryAddress => IsImport ? SupplierDocumentaryAddress : ImporterDocumentaryAddress;

		public bool FreeTradeZoneDeclarationTypes
		{
			get
			{
				switch (DeclarationType)
				{
					case Constants.DeclarationTypes.Export.D5:
					case Constants.DeclarationTypes.Export.B8:
					case Constants.DeclarationTypes.Export.B9:
					case Constants.DeclarationTypes.Export.F4:
					case Constants.DeclarationTypes.Import.D8:
					case Constants.DeclarationTypes.Import.B6:
					case Constants.DeclarationTypes.Import.F2:
						return true;
					default:
						return false;
				}
			}
		}

		public bool MasterBillIsRequiredForDeclarationType
		{
			get
			{
				switch (DeclarationType)
				{
					case Constants.DeclarationTypes.Export.G3:
					case Constants.DeclarationTypes.Export.G5:
					case Constants.DeclarationTypes.Import.G1:
					case Constants.DeclarationTypes.Import.G7:
						return true;
					default:
						return false;
				}
			}
		}

		public bool AutomaticallyDeclareNILForDeclarationType
		{
			get
			{
				switch (DeclarationType)
				{
					case Constants.DeclarationTypes.Export.D1:
					case Constants.DeclarationTypes.Export.B1:
					case Constants.DeclarationTypes.Export.B2:
					case Constants.DeclarationTypes.Export.F5:
					case Constants.DeclarationTypes.Import.G2:
					case Constants.DeclarationTypes.Import.D2:
					case Constants.DeclarationTypes.Import.D7:
					case Constants.DeclarationTypes.Import.F3:
						return true;
					default:
						return false;
				}
			}
		}

		protected override Customs.Business.CusPackingList GetNewCusPackingListCore(BusinessObjectFactory factory)
		{
			var packingList = base.GetNewCusPackingListCore(factory);
			packingList.CUL_PackageDescription = CusEntryInstruction.CEI_PackageDescription;
			return packingList;
		}

		public bool HasDaysOfDelayedDeclaration => !JE_DateAtFinalDestination.IsEmpty;

		[ChildEditable(true)]
		public ItineraryDataCollection Itineraries
		{
			get
			{
				if (itineraryDataCollection == null)
				{
					itineraryDataCollection = new ItineraryDataCollection(this);
					itineraryDataCollection.Load();
					RegisterEditableChildObject(itineraryDataCollection);
				}
				return itineraryDataCollection;
			}
		}
		ItineraryDataCollection itineraryDataCollection;

		public override ZString JE_GoodsDescription
		{
			get => base.JE_GoodsDescription;
			set
			{
				var oldValue = JE_GoodsDescription;
				base.JE_GoodsDescription = value;
				if (!IsCopying && oldValue != JE_GoodsDescription)
				{
					InvoiceLines?.ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}

		public override ZDateTime JE_EntrySubmittedDate
		{
			get => base.JE_EntrySubmittedDate;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_EntrySubmittedDate))
				{
					var oldValue = JE_EntrySubmittedDate;
					base.JE_EntrySubmittedDate = value;
					if (!IsCopying && oldValue != JE_EntrySubmittedDate)
					{
						Invoices?.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}

		protected override IReadOnlyList<string> MultipleKeysToUseCore => IsImport ? new[] { ImportCaptionKey } : new[] { ExportCaptionKey };

		#region IEntryNumberGeneratorProvider members
		ZDateTime IEntryNumberGeneratorProvider.EntryNumberDate => CusEntryInstruction.CEI_DateForDuty;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart1Info => CusEntryInstruction.CEI_CustomsOfficeInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart2Info => CusEntryInstruction.CEI_StyleInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.CustomsBrokerageBoxNumberInfo => CusEntryInstruction.CEI_BoxNumberInfo;

		ZString IEntryNumberGeneratorProvider.SequenceNumber => ZString.Empty;

		ZString IEntryNumberGeneratorProvider.ShipmentType => JE_MessageType;

		EntryNumberGeneratorCategory IEntryNumberGeneratorProvider.GetEntryNumberGeneratorCategory() => CusEntryInstruction.GetEntryNumberGeneratorCategory();

		EnterpriseBusinessObject IEntryNumberGeneratorProvider.EntryNumberGeneratorProviderBusinessObject => this;
		#endregion

		public const string ImportCaptionKey = "7AEEB6F0-4091-467F-8E55-D25C298D28F7";

		public const string ExportCaptionKey = "63DBAD9B-15CF-455C-8608-4C342250CEC2";

		protected override void LogCustomsClearedCore()
		{
			var entryHeader = EntryHeader;
			if (entryHeader != null)
			{
				var entryReleaseDateOffset = entryHeader.CH_EntryReleaseDate.ToDateTimeOffset(Branch.HomePort);
				if (entryReleaseDateOffset.IsValid)
				{
					LogsOfDeclarationOrShipment.AddNew(CustomsClearedEventType, entryHeader.CusEntryNumber?.CE_EntryStatus ?? ZString.Empty, entryReleaseDateOffset);
				}
			}
		}

		#region ConsignorDocumentaryAddress

		[ChildEditable(true)]
		public TWConsignorOrConsigneeAddressDependentCollection<TWConsignorAddress> ConsignorAddresses
		{
			get
			{
				if (fConsignorAddresses == null)
				{
					fConsignorAddresses = new TWConsignorOrConsigneeAddressDependentCollection<TWConsignorAddress>(this);
					fConsignorAddresses.Load(new ZQuery(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, DocAddressTypes.Codes.ConsignorAddress));
					RegisterEditableChildObject(fConsignorAddresses);
				}
				return fConsignorAddresses;
			}
		}
		TWConsignorOrConsigneeAddressDependentCollection<TWConsignorAddress> fConsignorAddresses;

		public TWConsignorAddress ConsignorDocumentaryAddress
		{
			get
			{
				if (fConsignorDocumentaryAddress == null || fConsignorDocumentaryAddress.IsDeleted)
				{
					fConsignorDocumentaryAddress = ConsignorAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorAddress);
				}
				return fConsignorDocumentaryAddress;
			}
		}
		TWConsignorAddress fConsignorDocumentaryAddress;

		#endregion

		#region ConsigneeDocumentaryAddress

		[ChildEditable(true)]
		public TWConsignorOrConsigneeAddressDependentCollection<TWConsigneeAddress> ConsigneeAddresses
		{
			get
			{
				if (fConsigneeAddresses == null)
				{
					fConsigneeAddresses = new TWConsignorOrConsigneeAddressDependentCollection<TWConsigneeAddress>(this);
					fConsigneeAddresses.Load(new ZQuery(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, DocAddressTypes.Codes.ConsigneeAddress));
					RegisterEditableChildObject(fConsigneeAddresses);
				}
				return fConsigneeAddresses;
			}
		}
		TWConsignorOrConsigneeAddressDependentCollection<TWConsigneeAddress> fConsigneeAddresses;

		public TWConsigneeAddress ConsigneeDocumentaryAddress
		{
			get
			{
				if (fConsigneeDocumentaryAddress == null || fConsigneeDocumentaryAddress.IsDeleted)
				{
					fConsigneeDocumentaryAddress = ConsigneeAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeAddress);
				}
				return fConsigneeDocumentaryAddress;
			}
		}
		TWConsigneeAddress fConsigneeDocumentaryAddress;

		#endregion

		#region IDocManagerSupport Members
		protected override DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new TWDeclarationDocManagerInfo(this);
		}

		internal IEnumerable<IStorageDocsBaseCollection> GetAllEDocs()
		{
			yield return DocManagerInfo.EDocsView;

			if (Shipment is ForwardingShipment shipment)
			{
				yield return shipment.DocManagerInfo.EDocsView;
			}

			foreach (var relatedBizO in DocManagerInfo.RelatedObjects)
			{
				if (relatedBizO is IDocManagerSupport docManagerSupport)
				{
					yield return docManagerSupport.DocManagerInfo.EDocsView;
				}
			}

			var invoiceLineParts = Invoices.SelectMany(x => x.JobComInvoiceLines.Cast<JobComInvoiceLine>().Select(c => c.Part)).WhereNotNull();
			foreach (var part in invoiceLineParts)
			{
				if (part is IDocManagerSupport docManagerSupport)
				{
					yield return docManagerSupport.DocManagerInfo.EDocsView;
				}
			}
		}
		#endregion
	}
}
