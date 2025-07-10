using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(JobRequiredDocument.Schema.EQ_DocType), DescriptionProperty(JobRequiredDocument.Schema.EQ_DocDescription)]
	public class JobRequiredDocument : AutoJobRequiredDocument, IDocManagerSupportProvider, IDocumentSupportable
	{
		#region Static

		public new class Schema : AutoJobRequiredDocument.Schema
		{
			public const string EQ_Calc_ParentUniqueConsignRef = "EQ_Calc_ParentUniqueConsignRef";
			public const string EQ_Calc_DocumentOwnerCode = "EQ_Calc_DocumentOwnerCode";
			public const string EQ_Calc_ParentHouseBill = "EQ_Calc_ParentHouseBill";
			public const string EQ_Calc_ParentMasterBill = "EQ_Calc_ParentMasterBill";
			public const string EQ_Calc_ParentExportBrokerCode = "EQ_Calc_ParentExportBrokerCode";
			public const string EQ_DateReceivedUtc = "EQ_DateReceivedUtc";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobRequiredDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!this.ReadOnly)
			{
				this.ReadOnly = EQ_DocType != ZString.Empty && !Env.Security.GetDocumentTypeUploadCheckPoint(EQ_DocType).IsAllowed;
			}

			if (!IsInDatabase)
			{
				JobRequiredDocumentConcurrencyChecker.Register(factory);
			}

			InitializeParentLazy();
			RegisterLinkedObjectTypes();
		}

		#region Constants

		public static class DocUsage
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Both = "BTH";
			public const string Domestic = "DOM";
			public const string All = "ALL";

			public const string Debtor = "DBT";
			public const string Creditor = "CRT";
			public const string ImporterConsignee = "ICE";
			public const string SupplierConsignor = "SCE";
			public const string TransportClient = "TCT";
			public const string Warehouse = "WAH";
			public const string Carrier = "CRR";
			public const string ForwarderAgent = "FAG";
			public const string Broker = "BRK";
			public const string Services = "SVS";
			public const string Competitor = "COM";
			public const string AttorneyForCustomsProcedures = "ACP";
		}

		#endregion

		#region Parent

		public IHaveRequiredDocuments Parent
		{
			get { return parentLazy.Value; }
		}
		Lazy<IHaveRequiredDocuments> parentLazy;

		void InitializeParentLazy()
		{
			parentLazy = new Lazy<IHaveRequiredDocuments>(() => EQ_ParentID.IsValid && EQ_ParentTableCode.IsValid ? (IHaveRequiredDocuments)Factory.Load(ParentType, EQ_ParentID) : null);
		}

		public Type ParentType
		{
			get
			{
				// DONT ATTEMPT TO GUESS THE PARENT TYPE HERE, YOU WILL GET IT WRONG!
				// If you get the JobRequiredDocument from the parent then the correct parent should be set by the JobRequiredDocumentCollection.
				// If you load the JobRequiredDocument yourself then you should set the parent type as appropriate for the context
				// in which you plan to use it.
				if (parentType == null)
				{
					throw new InvalidOperationException("Required Documents Parent Type Not Set Yet");
				}

				return parentType;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (!typeof(IHaveRequiredDocuments).IsAssignableFrom(value))
				{
					throw new ArgumentException(null, nameof(value));
				}

				parentType = value;
				InitializeParentLazy();
			}
		}

		Type parentType;

		internal bool IsParentTypeSet => parentType != null;

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("f3cd7ae4-0ce8-4e04-8632-207f8c1e24b6", "{0} Document: {1}", EQ_DocType, EQ_DocDescriptionMultilingual);

		public override bool ReadOnly
		{
			get
			{
				return base.ReadOnly ||
					(EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport && EQ_OH_DocumentOwner != Env.CurrentCompany.OrganisationPK);
			}
			set { base.ReadOnly = value; }
		}

		public override bool CanDelete
		{
			get
			{
				return AddInfos.Count == 0 &&
					(EQ_DocCategory != Constants.ReferenceTypes.ComplianceReport || EQ_OH_DocumentOwner == Env.CurrentCompany.OrganisationPK);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				List<MultilingualString> instructions = new List<MultilingualString>();
				if (EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport && EQ_OH_DocumentOwner != Env.CurrentCompany.OrganisationPK)
				{
					instructions.Add(ResString.GetMultilingualString("5464404c-9f5e-4dbb-bc3a-20b5880bb579", "Compliance Report exclusion is related to a different Company: {0}.", DocumentOwner != null ? DocumentOwner.OH_FullName : ZString.Empty));
				}
				else
				{
					var distictCompanies = AddInfos.GetDistinctCompanies();
					instructions.Add(ResString.GetMultilingualString("61881d69-8c2c-4248-9451-1c3a3c3208f9", "Document Imaging System records exist for this row. These records should be deleted prior to you removing this row."));

					foreach (var company in distictCompanies)
					{
						var instruction = (NoResString)string.Empty;

						if (company.PK != GlbCompany.CurrentCompany.PK)
						{
							var shipmentNumber = Parent != null ? ResString.GetMultilingualString("b79b2914-05e5-4cb2-a264-44ce641ee510", "Shipment {0}", Parent.UniqueConsignRef.ToString()) : ResString.GetMultilingualString("aa6fc557-2488-49a0-8cf2-e3ecba6f0869", "this shipment");

							instructions.Add(ResString.GetMultilingualString("7848ecd0-f416-4909-9fdf-442742176192", "Please log in to {0}, locate {1} and ", company.GC_Name, shipmentNumber));
						}

						instruction = (NoResString)AddInfos.GetInstructionsOnHowToDeleteAddInfos(company);
						instructions.Add(instruction);
					}
				}

				return MultilingualString.Join(System.Environment.NewLine, instructions.ToArray());
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "EQDocDescription checking raw value")]
		public override void OnSaving()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China &&
				EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan)
			{
				if (IsInDatabase)
				{
					AddorCancelLog(EQ_DateReceived.ToUtcZDateTime(), ((ZDateTimeOffset)EQ_DateReceivedInfo.OriginalValue).ToUtcZDateTime(), Events.HXDReceivedFromShipper);
					AddorCancelLog(EQ_SntToCustomsBroker, (ZDateTime)EQ_SntToCustomsBrokerInfo.OriginalValue, Events.HXDToCustomsBroker);
					AddorCancelLog(EQ_RcvFromCustomsBroker, (ZDateTime)EQ_RcvFromCustomsBrokerInfo.OriginalValue, Events.HXDFromCustomsBroker);
					AddorCancelLog(EQ_ReturnToShipper, (ZDateTime)EQ_ReturnToShipperInfo.OriginalValue, Events.HXDReturnedToShipper);
				}
				else
				{
					AddNewLog(EQ_DateReceived.ToUtcZDateTime(), Events.HXDReceivedFromShipper);
					AddNewLog(EQ_SntToCustomsBroker, Events.HXDToCustomsBroker);
					AddNewLog(EQ_RcvFromCustomsBroker, Events.HXDFromCustomsBroker);
					AddNewLog(EQ_ReturnToShipper, Events.HXDReturnedToShipper);
				}
			}

			if (EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument && string.IsNullOrEmpty(EQ_DocDescription))
			{
				string message = "EQ_DateReceived: " + EQ_DateReceived;
				ErrorReporter.ReportOnce("MiscellaneousDocument_With_EmptyDescription", message);
			}

			PrepareLogs();

			base.OnSaving();
			GenerateDeclarationOfIntentData();
		}

		#region Logs

		void PrepareLogs()
		{
			if (!IsDeleted && IsParentOrgHeader)
			{
				if ((ZString)EQ_DocTypeInfo.OriginalValue == AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines)
				{
					if (!IsInDatabase)
					{
						AddCreateRsbLog();
					}
					else if (EQ_DocType != AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines)
					{
						AddDeleteRsbLog();
					}
					else
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						(Parent as OrgHeader)?.Logs.AddNew(Events.EditedARecord, $"Modified {AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines} Document Tracking record for reporting period '{EQ_ValidToDate}'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
				else if (EQ_DocType == AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines)
				{
					AddCreateRsbLog();
				}
			}
		}

		public void TryAddDeleteLog()
		{
			if (IsInDatabase && !IsDeleted && IsParentOrgHeader && (ZString)EQ_DocTypeInfo.OriginalValue == AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines)
			{
				AddDeleteRsbLog();
			}
		}

		void AddDeleteRsbLog()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			(Parent as OrgHeader)?.Logs.AddNew(Events.EditedARecord, $"Deleted {AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines} Document Tracking record for reporting period '{EQ_ValidToDate}'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		void AddCreateRsbLog()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			(Parent as OrgHeader)?.Logs.AddNew(Events.EditedARecord, $"Added a new {AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines} Document Tracking record for reporting period '{EQ_ValidToDate}'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		bool IsParentOrgHeader => EQ_ParentTableCode == OrgHeaderSchema.Constants.Prefix;

		protected override bool EnableLightValidationIfAvailable => !HasChanges;

		#endregion

		void GenerateDeclarationOfIntentData()
		{
			if (!IsInDatabase
				&& GlbCompany.CurrentCompany.Country.SupportDeclarationOfIntent
				&& RelatedCountrySupportDeclarationOfIntent
				&& EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption)
			{
				if (!Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CompanyCode))
				{
					var attribute = Attributes.AddNew();
					attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
					attribute.D0_AttribValue = GlbCompany.CurrentCompany.PK.ToString();
				}

				ZString yearString = EQ_ValidToDate.Year.ToString(CultureInfo.InvariantCulture);
				if (EQ_DocUsage == JobRequiredDocument.DocUsage.Debtor)
				{
					if (!Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.SellerControlNumber))
					{
						// add the seller ctrl number to the attributes collection if does not already exist
						var numberFoutain = Env.NumberFountains.EXVSellerControlNumber.GetPeriodFountain(yearString, 6);
						var sellerControlNumber = numberFoutain.GetNextFormatted(Factory);
						var attribute = Attributes.AddNew();
						attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.SellerControlNumber;
						attribute.D0_AttribValue = sellerControlNumber.Insert(4, "-");
					}
				}
				else if (EQ_DocUsage == JobRequiredDocument.DocUsage.Creditor)
				{
					// add the buyer ctrl number to EQ_DocUsage if it's empty
					if (EQ_DocNumber.IsEmpty)
					{
						var numberFoutain = Env.NumberFountains.EXVBuyerControlNumber.GetPeriodFountain(yearString, 6);
						var buyerNumber = numberFoutain.GetNextFormatted(Factory);
						EQ_DocNumber = buyerNumber.Insert(4, "-");
					}
				}
			}
		}

		public bool IsCostaRicaExporterExemptionDocumentForDebtor => EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.CostaRica
																		&& EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption
																			&& EQ_DocUsage == JobRequiredDocument.DocUsage.Debtor;

		public bool IsTaiwanAttorneyDocumentForBroker => EQ_RN_NKRelatedCountry == CountryCodes.Taiwan && IsAttorney && EQ_DocUsage == DocUsage.Broker;

		public bool RelatedCountrySupportDeclarationOfIntent
		{
			get
			{
				var relatedCountry = RefCountry.LoadFromCountryCode(Factory, EQ_RN_NKRelatedCountry);
				return relatedCountry != null && relatedCountry.SupportDeclarationOfIntent;
			}
		}

		public RefDocType DocType
		{
			get
			{
				var query = new DocTypeCategoryQuery(Factory, EQ_DocCategory);
				query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.Equal, EQ_DocType);
				return Factory.LoadTop1<RefDocType>(query);
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobRequiredDocumentFetchStrategy(this);
		}

		public override ZGuid EQ_ParentID
		{
			get => base.EQ_ParentID;
			set
			{
				base.EQ_ParentID = value;
				InitializeParentLazy();
			}
		}

		public override ZString EQ_ParentTableCode
		{
			get => base.EQ_ParentTableCode;
			set
			{
				base.EQ_ParentTableCode = value;
				InitializeParentLazy();
			}
		}

		public JobRequiredDocAttrib BoxNumberDocAttrib => Attributes?.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BoxNumber);

		public JobRequiredDocAttrib CustomsDistrictDocAttrib => Attributes?.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.CustomsDistrict);

		public ZString CustomsDistrict => CustomsDistrictDocAttrib?.D0_AttribDisplayValue ?? ZString.Empty;

		public virtual Enterprise.Integration.Customs.IBoxNumberProvider GetBoxNumberProvider()
		{
			string country = EQ_RN_NKRelatedCountry;
			var provider = ObjectFactory.Get<Hashtable>("BoxNumberProviders")[country] as ObjectHandle;
			return provider?.GetObject() as Enterprise.Integration.Customs.IBoxNumberProvider;
		}

		public ZString GetDefaultBoxNumber(ZString customsDistrict) => GetBoxNumberProvider()?.GetDefaultBoxNumber(Factory, customsDistrict) ?? ZString.Empty;
		#endregion

		#region Related Business Objects

		public DynamicBusinessObjectCollection LocalOutstandingInvoices
		{
			get
			{
				ZString sQLString = "SELECT " +
					"(CASE WHEN " + AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + " != @LocalCurrency THEN " +
					"CASE WHEN " + AccTransactionHeaderSchema.Constants.AH_OutstandingAmount + " = " + AccTransactionHeader.AH_LocalTotalSQLFormula + " THEN " +
					AccTransactionHeaderSchema.Constants.AH_OSTotal +
					" ELSE " +
					"CASE WHEN " + GlbCompanySchema.Constants.GC_IsReciprocal + " = @BooleanTrue THEN " +
					"ROUND(" + AccTransactionHeaderSchema.Constants.AH_OutstandingAmount + "/" + AccTransactionHeaderSchema.Constants.AH_ExchangeRate + ", 2) " +
					"ELSE " +
					"ROUND(" + AccTransactionHeaderSchema.Constants.AH_OutstandingAmount + "*" + AccTransactionHeaderSchema.Constants.AH_ExchangeRate + ", 2) " +
					" END " +
					" END " +
					" ELSE " +
					AccTransactionHeaderSchema.Constants.AH_OutstandingAmount +
					" END) AS OutstandingAmt, " +

					RefCurrencySchema.Constants.RX_Code + ", " +
					OrgHeaderSchema.Constants.OH_FullName + ", " +
					AccTransactionHeaderSchema.Constants.AH_TransactionType + ", " +
					AccTransactionHeaderSchema.Constants.AH_TransactionNum +
					" FROM " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName +
					" INNER JOIN " + JobHeaderSchema.Constants.SqlSchemaName + "." + JobHeaderSchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.AH_JH + " = " + JobHeaderSchema.Constants.PK +
					" INNER JOIN " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.AH_OH + " = " + OrgHeaderSchema.Constants.PK +
					" INNER JOIN " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName + " ON " + JobHeaderSchema.Constants.JH_GC + " = " + GlbCompanySchema.Constants.PK +
					" INNER JOIN " + RefCountrySchema.Constants.SqlSchemaName + "." + RefCountrySchema.Constants.TableName + " ON " + GlbCompanySchema.Constants.GC_RN_NKCountryCode + " = " + RefCountrySchema.Constants.RN_Code +
					" INNER JOIN " + JobShipmentSchema.Constants.SqlSchemaName + "." + JobShipmentSchema.Constants.TableName + " ON " + JobHeaderSchema.Constants.JH_ParentID + " = " + JobShipmentSchema.Constants.PK +
					" INNER JOIN " + JobDocsAndCartageSchema.Constants.SqlSchemaName + "." + JobDocsAndCartageSchema.Constants.TableName + " ON " + JobShipmentSchema.Constants.PK + " = " + JobDocsAndCartageSchema.Constants.JP_ParentID +
					" INNER JOIN " + RefCurrencySchema.Constants.SqlSchemaName + "." + RefCurrencySchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + " = " + RefCurrencySchema.Constants.RX_Code +
					" WHERE " +
					AccTransactionHeaderSchema.Constants.AH_FullyPaidDate + " IS NULL AND " +
					AccTransactionHeaderSchema.Constants.AH_Ledger + " = @Ledger AND " +
					AccTransactionHeaderSchema.Constants.AH_TransactionType + " = @TransactionType AND " +
					OrgHeaderSchema.Constants.OH_RL_NKClosestPort + " LIKE @Expr AND " +
					JobHeaderSchema.Constants.JH_GC + " = @CompanyGuid AND " +
					JobDocsAndCartageSchema.Constants.PK + " = @JobDocsPK " +
					"ORDER BY " + OrgHeaderSchema.Constants.OH_FullName;

				DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@LocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency);
				parameters.Add("@BooleanTrue", Core.Constants.BooleanTrueString, GlbCompanySchema.GC_IsReciprocal);
				parameters.Add("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@TransactionType", ZArchitecture.Core.TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@Expr", "CN%", OrgHeaderSchema.OH_RL_NKClosestPort);
				parameters.Add("@CompanyGuid", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				parameters.Add("@JobDocsPK", Parent == null ? ZGuid.Empty : Parent.PK, JobDocsAndCartageSchema.PK);

				dynBizOs.Load(sQLString, parameters);

				return dynBizOs;
			}
		}

		[ChildEditable]
		public JobRequiredDocAttribCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new JobRequiredDocAttribCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		JobRequiredDocAttribCollection attributes;

		[ChildEditable]
		public JobRequiredDocumentAddInfoDependentCollection AddInfos
		{
			get
			{
				if (addInfos == null)
				{
					addInfos = new JobRequiredDocumentAddInfoDependentCollection(this);
					addInfos.Load();
					RegisterEditableChildObject(addInfos);
				}
				return addInfos;
			}
		}
		JobRequiredDocumentAddInfoDependentCollection addInfos;

		#endregion

		#region Property Overrides
		[List("Lookups.DocType_List")]
		[LightValidationTestExempt]//JobRequiredDocument.EQ_DocType is supposed to call MarkAsNeedingValidation on CusISFHeader, but they use a different factory
		public override ZString EQ_DocType
		{
			get { return base.EQ_DocType; }
			set
			{
				if (value != Core.Constants.RefDocTypes.MiscellaneousDocument)
				{
					var multilingualDescription = Lookups.DocType_List.GetMultilingualDescriptionFromCode(value);
					var description = multilingualDescription == null ? string.Empty : multilingualDescription.GetUnresolvedString();

					if (description != null && description.Length > EQ_DocDescriptionInfo.MaxLength)
					{
						description = description.Substring(0, EQ_DocDescriptionInfo.MaxLength);
					}

					EQ_DocDescription = description;
				}
				else if (value != base.EQ_DocType)
				{
					EQ_DocDescription = ZString.Empty;
				}

				if (!value.IsEmpty && value.IsValid)
				{
					EQ_CreditControlDoc = IsRegistryCreditControlDoc(value);
				}

				if (Parent != null &&
					(value == Core.Constants.RefDocTypes.PowerOfAttorney
					|| value == Core.Constants.RefDocTypes.PowerOfAttorneyCustoms
					|| value == Core.Constants.RefDocTypes.PowerOfAttorneyForwarding))
				{
					if (typeof(OrgHeader).IsAssignableFrom(Parent.GetType()))
					{
						EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
					}
					else
					{
						EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
						EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
					}
				}

				if (value == Core.Constants.RefDocTypes.VATExporterExemption)
				{
					EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
					EQ_DocUsage = ZString.Empty;
				}
				if (value == Core.Constants.RefDocTypes.WithholdingTaxExemption)
				{
					EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
					EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				}

				if (value == Core.Constants.RefDocTypes.Nafta || value == Core.Constants.RefDocTypes.Cotton)
				{
					EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				}

				base.EQ_DocType = value;
				if (value == Core.Constants.RefDocTypes.MiscellaneousDocument)
				{
					this.Validation.ValidateEQ_DocDescription();
				}

				if (IsTaiwanAttorney)
				{
					this.Validation.ValidateEQ_ValidToDate();
				}
				if (IsTaiwanAttorneyDocumentForBroker)
				{
					AddTaiwanAttorneyDocumentForBrokerAttribute();
				}

				EQ_DocDescriptionInfo.RefreshBinding();
			}
		}

		public IDisposable SuspendDocTypeUniquenessCheck()
		{
			suspendDocTypeUniquenessCheck = true;
			return new DisposableAction(() => { suspendDocTypeUniquenessCheck = false; });
		}

		bool suspendDocTypeUniquenessCheck;

		bool IsAttorney => EQ_DocType == RefDocTypes.PowerOfAttorney || EQ_DocType == RefDocTypes.PowerOfAttorneyCustoms;

		public bool IsTaiwanAttorney => IsAttorney && (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Taiwan || EQ_RN_NKRelatedCountry == CountryCodes.Taiwan);

		void AddCustomsDistrictAttribute()
		{
			if (!Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict))
			{
				var attribute = Attributes.AddNew();
				attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			}
		}

		void AddBoxNumberAttribute()
		{
			if (!Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BoxNumber))
			{
				var attribute = Attributes.AddNew();
				attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
				attribute.D0_AttribDisplayValue = GetDefaultBoxNumber(CustomsDistrict);
			}
		}

		void AddBondedIDAttribute()
		{
			if (!Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID))
			{
				var attribute = Attributes.AddNew();
				attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BondedID;
			}
		}

		[List("Lookups.DocPeriod_List")]
		public override ZString EQ_DocPeriod
		{
			get { return base.EQ_DocPeriod; }
			set
			{
				if (base.EQ_DocPeriod != value)
				{
					base.EQ_DocPeriod = value;

					if (EQ_DocPeriod == Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment)
					{
						EQ_ValidToDate = ZDateTime.Empty;
					}

					EQ_ValidToDateInfo.RefreshBinding();

					if (!IsValidationSuspended && !EQ_DocType.IsEmpty && !EQ_DocTypeInfo.ReadOnly)
					{
						Validation.ValidateEQ_DocType();
					}
				}
			}
		}

		[List("Lookups.DocUsage_List")]
		public override ZString EQ_DocUsage
		{
			get
			{
				return base.EQ_DocUsage;
			}
			set
			{
				base.EQ_DocUsage = value;

				if (!IsValidationSuspended && !EQ_DocType.IsEmpty && !EQ_DocTypeInfo.ReadOnly)
				{
					Validation.ValidateEQ_DocType();
				}
				if (IsTaiwanAttorneyDocumentForBroker)
				{
					AddTaiwanAttorneyDocumentForBrokerAttribute();
				}
			}
		}

		[List("Lookups.RefCountry_List")]
		public override ZString EQ_RN_NKRelatedCountry
		{
			get
			{
				return base.EQ_RN_NKRelatedCountry;
			}
			set
			{
				base.EQ_RN_NKRelatedCountry = value;

				if (!IsValidationSuspended && !EQ_DocType.IsEmpty && !EQ_DocTypeInfo.ReadOnly)
				{
					Validation.ValidateEQ_DocType();
					if (IsTaiwanAttorney)
					{
						Validation.ValidateEQ_ValidToDate();
					}
					if (IsTaiwanAttorneyDocumentForBroker)
					{
						AddTaiwanAttorneyDocumentForBrokerAttribute();
					}
				}
			}
		}

		void AddTaiwanAttorneyDocumentForBrokerAttribute()
		{
			AddCustomsDistrictAttribute();
			AddBoxNumberAttribute();
			AddBondedIDAttribute();
		}

		[List("Lookups.DocumentOwners")]
		public override ZGuid EQ_OH_DocumentOwner
		{
			get
			{
				return base.EQ_OH_DocumentOwner;
			}
			set
			{
				base.EQ_OH_DocumentOwner = value;
			}
		}

		public override ZDateTime EQ_ValidToDate
		{
			get
			{
				return base.EQ_ValidToDate;
			}
			set
			{
				base.EQ_ValidToDate = value;
				Validation.ValidateEQ_DocType();
			}
		}

		public override ZString EQ_DocNumber
		{
			get
			{
				return base.EQ_DocNumber;
			}
			set
			{
				base.EQ_DocNumber = value;
				Validation.ValidateEQ_DocType();
			}
		}

		[LinkedTranslatableDataField(typeof(RefDocType), RefDocType.Schema.RT_Desc)]
		public override ZString EQ_DocDescription
		{
			get { return base.EQ_DocDescription; }
			set { base.EQ_DocDescription = value; }
		}

		[MaxLength(Schema.EQ_DocDescriptionMaxLength)]
		public MultilingualString EQ_DocDescriptionMultilingual
		{
			get { return GetMultilingual(EQ_DocDescriptionInfo); }
			set { EQ_DocDescription = value; }
		}

		public ZPropertyInfo EQ_DocDescriptionMultilingualInfo
		{
			get { return EQ_DocDescriptionInfo; }
		}

		public bool EQ_DocDescriptionMultilingual_ReadOnly
		{
			get { return EQ_DocDescriptionInfo.ReadOnly; }
		}

		protected bool EQ_DocDescription_ReadOnly
		{
			get { return EQ_DocType != Core.Constants.RefDocTypes.MiscellaneousDocument; }
		}

		protected bool EQ_CreditControlDoc_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDocumentCreditControlStatus.IsAllowed; }
		}

		protected bool EQ_DocNumber_ReadOnly
		{
			get { return IsEXVCreditor && RelatedCountrySupportDeclarationOfIntent; }
		}

		public bool IsEXVCreditor
		{
			get
			{
				return EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption && EQ_DocUsage == JobRequiredDocument.DocUsage.Creditor;
			}
		}

		public bool IsEXVDebtor
		{
			get
			{
				return EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption && EQ_DocUsage == JobRequiredDocument.DocUsage.Debtor;
			}
		}

		protected virtual bool EQ_SntToCustomsBroker_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerIndividualRecords.IsAllowed; }
		}

		protected virtual bool EQ_RcvFromCustomsBroker_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerIndividualRecords.IsAllowed; }
		}

		protected virtual bool EQ_ReturnToShipper_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperIndividualRecords.IsAllowed; }
		}

		protected bool EQ_ValidToDate_ReadOnly
		{
			get { return EQ_DocPeriod != Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic; }
		}

		protected bool EQ_RN_NKRelatedCountry_ReadOnly
		{
			get { return EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport; }
		}

		protected bool EQ_OH_DocumentOwner_ReadOnly
		{
			get { return EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport; }
		}

		#endregion

		#region Calculated Properties

		public ZString EQ_Calc_DocumentOwnerCode
		{
			get { return DocumentOwner != null ? DocumentOwner.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo EQ_Calc_DocumentOwnerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EQ_Calc_DocumentOwnerCode); }
		}

		[MaxLength(255)]
		public ZString EQ_Calc_ParentUniqueConsignRef
		{
			get { return Parent != null ? Parent.UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo EQ_Calc_ParentUniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(Schema.EQ_Calc_ParentUniqueConsignRef); }
		}

		[MaxLength(255)]
		public ZString EQ_Calc_ParentHouseBill
		{
			get { return Parent != null ? Parent.HouseBill : ZString.Empty; }
		}

		public ZPropertyInfo EQ_Calc_ParentHouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.EQ_Calc_ParentHouseBill); }
		}

		[MaxLength(255)]
		public ZString EQ_Calc_ParentMasterBill
		{
			get { return Parent != null ? Parent.MasterBill : ZString.Empty; }
		}

		public ZPropertyInfo EQ_Calc_ParentMasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.EQ_Calc_ParentMasterBill); }
		}

		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString EQ_Calc_ParentExportBrokerCode
		{
			get { return Parent != null && Parent.ExportBroker != null ? Parent.ExportBroker.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo EQ_Calc_ParentExportBrokerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EQ_Calc_ParentExportBrokerCode); }
		}

		public ZBool IsPeriodic
		{
			get { return EQ_DocPeriod == Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic; }
		}

		public ZBool IsPowerOfAttorney
		{
			get
			{
				return IsPowerOfAttorneyDocType(EQ_DocType);
			}
		}

		static bool IsPowerOfAttorneyDocType(string docType)
		{
			return docType == RefDocTypes.PowerOfAttorney
				   || docType == RefDocTypes.PowerOfAttorneyCustoms
				   || docType == RefDocTypes.PowerOfAttorneyForwarding;
		}

		public static bool ShouldCheckDocTypeDuplication(string docType)
		{
			return docType != RefDocTypes.MiscellaneousDocument
				   && docType != RefDocTypes.HeXiaoDan
				   && !IsPowerOfAttorneyDocType(docType);
		}

		internal bool IsDocTypeDuplicate => !suspendDocTypeUniquenessCheck && ShouldCheckDocTypeDuplication(EQ_DocType) && !IsUniqueDocType;

		bool IsUniqueDocType
		{
			get
			{
				if (IsParentTypeSet && Parent?.RequiredDocuments != null && DocType != null)
				{
					if (!DocType.RT_AllowMultiplePeriodicDocs || EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic)
					{
						var otherDocs = FindRequiredDocumentWithDuplicateDocType();
						if (otherDocs != null)
						{
							foreach (var otherDoc in otherDocs)
							{
								if (DocType.RT_AllowMultiplePeriodicDocs)
								{
									if (EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic && otherDoc.EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic)
									{
										return false;
									}
								}
								else if (EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic || otherDoc.EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic ||
										 (otherDoc.EQ_ValidToDate == EQ_ValidToDate && otherDoc.EQ_DocNumber == EQ_DocNumber))
								{
									var otherTradePreference = otherDoc.Attributes[JobRequiredDocAttribTypeList.Codes.TradePreferenceCode]?.D0_AttribValue ?? ZString.Empty;
									var tradePreference = Attributes[JobRequiredDocAttribTypeList.Codes.TradePreferenceCode]?.D0_AttribValue ?? ZString.Empty;
									if (EQ_DocType != RefDocTypes.CertificateOfOrigin || otherTradePreference.IsEmpty || tradePreference.IsEmpty || otherTradePreference == tradePreference)
									{
										return false;
									}
								}
							}
						}
					}
				}
				return true;
			}
		}

		JobRequiredDocument[] FindRequiredDocumentWithDuplicateDocType()
		{
			ZQuery query = new ZQuery(Parent.RequiredDocuments.CompleteFilter);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, EQ_DocType);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, EQ_DocCategory);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocUsage, EQ_DocUsage);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_RN_NKRelatedCountry, EQ_RN_NKRelatedCountry);

			if (DocType.RT_AllowMultiplePeriodicDocs && EQ_DocPeriod != JobRequiredDocuments.DocumentPeriods.Periodic)
			{
				query.AddToFilter(JobRequiredDocumentSchema.EQ_DocPeriod, EQ_DocPeriod);
			}

			query.AddToFilter(JobRequiredDocumentSchema.PK, SQLComparisonOperator.NotEqual, PK);
			query.ReLoadExistingRows = true;

			var otherDocs = Factory.Load(GetType(), query) as JobRequiredDocument[];
			return otherDocs;
		}

		#endregion

		#region New Properties
		[List("Lookups.CategoryType_List")]
		[MaxLength(3)]
		public override ZString EQ_DocCategory
		{
			get { return base.EQ_DocCategory; }
			set
			{
				var oldValue = EQ_DocCategory;
				base.EQ_DocCategory = value;
				CheckMaximumLength(EQ_DocCategoryInfo, value);
				Lookups.ResetDocAndCategoryTypeList();

				if (Parent != null && typeof(OrgHeader).IsAssignableFrom(Parent.GetType())
					&& base.EQ_DocCategory == Core.Constants.ReferenceTypes.SupplyChainLogistics)
				{
					base.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				}
				if (base.EQ_DocCategory == Core.Constants.ReferenceTypes.ClientSupplierRelationship)
				{
					if (IsPowerOfAttorney && Parent != null && !typeof(OrgHeader).IsAssignableFrom(Parent.GetType()))
					{
						base.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
					}
					else
					{
						base.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
					}
				}
				else
				{
					base.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
				}

				if (EQ_DocCategory != oldValue && EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
				{
					EQ_RN_NKRelatedCountry = Env.CurrentCompany.Country.Code;
					EQ_OH_DocumentOwner = Env.CurrentCompany.OrganisationPK;
					EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
					EQ_DocUsage = string.Empty;
				}

				if (EQ_DocCategory != oldValue && oldValue == Constants.ReferenceTypes.ComplianceReport)
				{
					EQ_OH_DocumentOwner = ZGuid.Empty;
				}

				if (!IsValidationSuspended && !EQ_DocCategoryInfo.ReadOnly)
				{
					Validation.ValidateEQ_DocCategory();
				}
				EQ_DocCategoryInfo.RefreshBinding();

				if (!IsValidationSuspended && !EQ_DocType.IsEmpty && !EQ_DocTypeInfo.ReadOnly)
				{
					Validation.ValidateEQ_DocType();
				}
			}
		}

		public override ZPropertyInfo EQ_DocCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(EQ_DocCategory)); }
		}

		public enum JRDOrigin
		{
			Unknown = 0,
			AddedEDoc = 1,
			FromRequirements = 2,
		}

		public JRDOrigin Origin { get; set; }

		#endregion

		#region Date Received Event on Parent

		public override ZDateTimeOffset EQ_DateReceived
		{
			get
			{
				if (IsDateReceivedTransformRunning && IsEQ_DateReceivedOffsetZero && !string.IsNullOrEmpty(CreationUNLOCOCode) && !string.IsNullOrEmpty(Env.CurrentBranch.NKUNLOCO))
				{
					return Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(CreationUNLOCOCode, base.EQ_DateReceived.ToDateTime(), Env.CurrentBranch.NKUNLOCO);
				}

				if (!string.IsNullOrEmpty(Env.CurrentBranch.NKUNLOCO) && base.EQ_DateReceived.IsValid)
				{
					return base.EQ_DateReceived.ToUtcZDateTime().UtcToDateTimeOffset();
				}
				return base.EQ_DateReceived;
			}
			set
			{
				JobRequiredDocumentAllDocumentsReceivedEventLogger.LogEventOnParentsOnSave(Parent);
				if (IsDateReceivedTransformRunning && value.IsValid && value.Offset.TotalSeconds == 0 && IsUsingDifferentUnloco)
				{
					var dateTime = value.ToDateTime();
					var offsetAtCreationUnloco = Env.Time.GetUtcOffsetBasedOnLocal(CreationUNLOCOCode, dateTime);
					var dateTimeAtCreationUnloco = Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(Env.CurrentBranch.NKUNLOCO, dateTime, CreationUNLOCOCode);
					value = new ZDateTimeOffset(dateTimeAtCreationUnloco, offsetAtCreationUnloco);
				}

				base.EQ_DateReceived = value;
			}
		}

		internal string CreationUNLOCOCode
		{
			get
			{
				if (creationUNLOCOCode == null)
				{
					creationUNLOCOCode = ZString.Empty;
					var addedLog = this.GetFirstMatchingLog(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.AddedARecordToTheSystem));
					if (addedLog != null)
					{
						var creationBranchCode = addedLog.SL_GB_NKBranch;
						var glbBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, creationBranchCode);
						if (glbBranch != null)
						{
							creationUNLOCOCode = glbBranch.GB_RL_NKHomePort;
						}
					}
				}
				return creationUNLOCOCode;
			}
		}
		string creationUNLOCOCode;

		bool IsEQ_DateReceivedOffsetZero => base.EQ_DateReceived.IsValid && base.EQ_DateReceived.Offset.TotalSeconds == 0;
		bool IsUsingDifferentUnloco => !string.IsNullOrEmpty(CreationUNLOCOCode) && !string.IsNullOrEmpty(Env.CurrentBranch.NKUNLOCO) && Env.CurrentBranch.NKUNLOCO != CreationUNLOCOCode;
		const string TransformIsRunningName = "AdjustTimezoneOffsetDateReceived.IsRunning";

		static readonly Overridable<bool> isDateReceivedTransformRunning = new Overridable<bool>(true);
		bool IsDateReceivedTransformRunning
		{
			get
			{
				if (isDateReceivedTransformRunning.Value)
				{
					isDateReceivedTransformRunning.Value = IsAdjustTimezoneOffsetDateReceivedTransformRunning();
				}
				return isDateReceivedTransformRunning.Value;
			}
		}

		bool IsAdjustTimezoneOffsetDateReceivedTransformRunning()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, TransformIsRunningName);
			return Factory.ExistsInDatabase(StmDataSchema.Constants.TableName, query);
		}

		public ZDateTime EQ_DateReceivedUtc
		{
			get
			{
				if (IsEQ_DateReceivedOffsetZero)
				{
					return Env.Time.GetUtcFromLocalTime(EQ_DateReceived.ToDateTime());
				}
				if (base.EQ_DateReceived.IsValid)
				{
					return base.EQ_DateReceived.ToUtcDateTime();
				}
				return ZDateTime.Empty;
			}
			set => EQ_DateReceived = value.IsValid ? value.ToLocalBranchTimeOffset(null) : ZDateTimeOffset.Empty;
		}

		public ZWrappedPropertyInfo EQ_DateReceivedUtcInfo => GetWrappedZPropertyInfo(Schema.EQ_DateReceivedUtc, x => EQ_DateReceivedInfo);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				JobRequiredDocumentAllDocumentsReceivedEventLogger.LogEventOnParentsOnSave(Parent);
			}
			Attributes.DeleteAll();
			AddInfos.DeleteAll();
			base.Delete();
		}

		internal static void LogEventOnParentsOnSave(IHaveRequiredDocuments parent)
		{
			JobRequiredDocumentAllDocumentsReceivedEventLogger.LogEventOnParentsOnSave(parent);
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Transient Values Handling

		public override void CopyTransientProperties(BusinessObject copy)
		{
			((JobRequiredDocument)copy).parentType = parentType;
		}

		#endregion

		#region Implementation

		public bool IsValidForReferenceType(ZString refType)
		{
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, refType);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, EQ_DocType);
			RefDocType foundDocType = Factory.LoadTop1<RefDocType>(query);
			return foundDocType != null;
		}

		void AddorCancelLog(ZDateTime currentValue, ZDateTime originalValue, Event hXDEvent)
		{
			if (Parent.UltimateDocumentParent != null)
			{
				if (!currentValue.IsEmpty && originalValue.IsEmpty)
				{
					Parent.UltimateDocumentParent.GetLogs().AddNew(hXDEvent);
				}
				else if (currentValue.IsEmpty && !originalValue.IsEmpty)
				{
					var logToCancel = Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(hXDEvent);
					if (logToCancel != null)
					{
						logToCancel.Cancel();
					}
				}
			}
		}

		protected void AddNewLog(ZDateTime hXDField, Event hXDEvent)
		{
			if (Parent.UltimateDocumentParent != null && !hXDField.IsEmpty)
			{
				Parent.UltimateDocumentParent.GetLogs().AddNew(hXDEvent);
			}
		}

		protected bool IsRegistryCreditControlDoc(string docType)
		{
			// TODO: Fix

			//ICodeDescriptionBoolList RegistryDocs = //DocumentsDataRegistry.Instance.RequiredDocumentTypes.Value;
			//return RegistryDocs.GetBoolFromCode(DocType);

			return false;
		}

		#endregion

		void RegisterLinkedObjectTypes()
		{
			RegisteredLinkedObjectTypes = new List<Type>();
			RegisteredLinkedObjectTypes.Add(ObjectFactory.GetType<Enterprise.Integration.Freight.IJobDocsAndCartage>());
			RegisteredLinkedObjectTypes.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusPermitHeader>());
		}
#if DEBUG
		internal
#endif
		List<Type> RegisteredLinkedObjectTypes;

		IEnumerable<IDocManagerSupport> IDocManagerSupportProvider.DocManagerSupports
		{
			get
			{
				if (DocumentParent is IDocManagerSupport docManagerSupport)
				{
					yield return docManagerSupport;
				}
			}
		}

		public BusinessObject DocumentParent
		{
			get
			{
				if (!IsParentTypeSet)
				{
					IHaveRequiredDocuments parentBO = null;
					foreach (Type bizoType in RegisteredLinkedObjectTypes)
					{
						if (BusinessObjectFactory.GetTableCodeFromType(bizoType) == EQ_ParentTableCode)
						{
							parentBO = (IHaveRequiredDocuments)Factory.Load(bizoType, EQ_ParentID);
							break;
						}
					}
					return parentBO != null ? parentBO.UltimateDocumentParent : null;
				}
				else
				{
					return Parent != null ? Parent.UltimateDocumentParent : null;
				}
			}
		}

		#region IDocumentSupporter
		public DocumentSupporter DocumentSupporter => GetJobRequiredDocumentDocumentSupporterByCountry(EQ_RN_NKRelatedCountry) ?? new JobRequiredDocumentDocumentSupporter(this);

		DocumentSupporter GetJobRequiredDocumentDocumentSupporterByCountry(string country)
		{
			var provider = ObjectFactory.Get<Hashtable>(DocumentSupportersName)[country] as ObjectHandle;
			return provider?.GetObject(this) as DocumentSupporter;
		}

		public const string DocumentSupportersName = "JobRequiredDocumentDocumentSupporters";
		#endregion
	}
}
