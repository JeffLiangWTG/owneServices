using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USInvoiceLineFSISLine : USFSISLine, IFSISLine, ICusAddInfoTypeSupporter, IParentDocManagerSupport, IPGADataCorrection, ICanDelete, ICustomsBrokerDetails, ICusDispositionParent
	{
		public USInvoiceLineFSISLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : AutoUSFSISLine.Schema
		{
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#region Override Properties

		[ReadOnly(true)]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						base.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		[List(nameof(DeclarationFSISLines))]
		public override ZString US_HealthCertificateNumber
		{
			get { return base.US_HealthCertificateNumber; }
			set
			{
				var oldValue = US_HealthCertificateNumber;

				if (oldValue != value && !IsCopying)
				{
					RefreshInvoiceLineFSISLines();
					base.US_HealthCertificateNumber = value;
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					DefaultValuesFromDeclarationFSISLIneIfRequired(declarationFSISLine);
					RefreshInvoiceLineFSISLines();
				}
			}
		}

		public USDeclarationFSISLineCollection DeclarationFSISLines
		{
			get
			{
				var invoiceLine = Parent;
				return invoiceLine != null && invoiceLine.Declaration != null ? Parent.Declaration.FSISLines : null;
			}
		}

		internal USDeclarationFSISLine GetOrCreateDeclarationFSISLine()
		{
			var result = GetDeclarationFSISLine();

			if (result == null && !US_HealthCertificateNumber.IsEmpty && Parent != null)
			{
				result = Parent.Declaration.FSISLines.AddNew();
				result.US_HealthCertificateNumber = US_HealthCertificateNumber;
			}

			return result;
		}

		internal USDeclarationFSISLine GetDeclarationFSISLine()
		{
			USDeclarationFSISLine result = null;
			if (Parent != null && Parent.Declaration != null)
			{
				result = (USDeclarationFSISLine)Parent.Declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber.EqualsIgnoringCase(US_HealthCertificateNumber));
			}
			return result;
		}

		void DefaultValuesFromDeclarationFSISLIneIfRequired(USDeclarationFSISLine declarationFSISLine)
		{
			if (declarationFSISLine != null)
			{
				US_CommercialDescription = declarationFSISLine.US_CommercialDescription;
				US_ExportingEstNo = declarationFSISLine.US_ExportingEstNo;
				US_ImportingEstNo = declarationFSISLine.US_ImportingEstNo;
				US_UC_NKCertificateIssuerCountry = declarationFSISLine.US_UC_NKCertificateIssuerCountry.IsEmpty ? Parent.US_UC_NKCountryOfExport : declarationFSISLine.US_UC_NKCertificateIssuerCountry;
				US_UC_NKCountryOfOrigin = declarationFSISLine.US_UC_NKCountryOfOrigin.IsEmpty ? Parent.US_UC_NKCountryOfOrigin : declarationFSISLine.US_UC_NKCountryOfOrigin;
				US_ProductIDQualifier = declarationFSISLine.US_ProductIDQualifier;
				US_ProductID = declarationFSISLine.US_ProductID;
				US_IntendedUseCode = declarationFSISLine.US_IntendedUseCode;
				US_SealNumbers = declarationFSISLine.US_SealNumbers;
				US_DateOfInspection = declarationFSISLine.US_DateOfInspection;
				US_CertifyingIndividual = declarationFSISLine.US_CertifyingIndividual;
				US_PGAContactName = declarationFSISLine.US_PGAContactName;
				US_PGAContactPhoneNo = declarationFSISLine.US_PGAContactPhoneNo;
				US_PGAContactEmail = declarationFSISLine.US_PGAContactEmail;
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_CertifyingIndividual
		{
			get
			{
				return base.US_CertifyingIndividual;
			}
			set
			{
				var hasChanges = base.US_CertifyingIndividual != value;
				base.US_CertifyingIndividual = value;

				if (hasChanges && !IsCopying)
				{
					var invoiceLine = Parent;
					if (invoiceLine != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							var importerWrapper = Parent.IORWrapper as IPGAContactDetails;
							if (importerWrapper != null)
							{
								US_PGAContactName = importerWrapper.Name;
								US_PGAContactPhoneNo = importerWrapper.PhoneNumber.Left(AutoUSFSISLineAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = importerWrapper.EmailAddress;
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							var brokerWrapper = invoiceLine as ICustomsBrokerDetails;
							if (brokerWrapper != null)
							{
								US_PGAContactName = brokerWrapper.ContactName;
								US_PGAContactPhoneNo = brokerWrapper.ContactPhone;
								US_PGAContactEmail = brokerWrapper.ContactEmail;
							}
						}
					}
					if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
					{
						var declarationFSISLine = GetOrCreateDeclarationFSISLine();
						if (declarationFSISLine != null && declarationFSISLine.US_CertifyingIndividual != US_CertifyingIndividual)
						{
							declarationFSISLine.US_CertifyingIndividual = US_CertifyingIndividual;
						}
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_CommercialDescription
		{
			get { return base.US_CommercialDescription; }
			set
			{
				base.US_CommercialDescription = value;
				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_CommercialDescription != US_CommercialDescription)
					{
						declarationFSISLine.US_CommercialDescription = US_CommercialDescription;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsFieldReadonlyWhenCertificateAlreadyInUseOrElectronic))]
		public override ZString US_ExportingEstNo
		{
			get { return base.US_ExportingEstNo; }
			set
			{
				base.US_ExportingEstNo = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce && !IsElectronicallyCertificated)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_ExportingEstNo != US_ExportingEstNo)
					{
						declarationFSISLine.US_ExportingEstNo = US_ExportingEstNo;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_ImportingEstNo
		{
			get { return base.US_ImportingEstNo; }
			set
			{
				base.US_ImportingEstNo = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_ImportingEstNo != US_ImportingEstNo)
					{
						declarationFSISLine.US_ImportingEstNo = US_ImportingEstNo;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_UC_NKCertificateIssuerCountry
		{
			get { return base.US_UC_NKCertificateIssuerCountry; }
			set
			{
				base.US_UC_NKCertificateIssuerCountry = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_UC_NKCertificateIssuerCountry != US_UC_NKCertificateIssuerCountry)
					{
						declarationFSISLine.US_UC_NKCertificateIssuerCountry = US_UC_NKCertificateIssuerCountry;
					}
				}

				Lots.SetReadOnlyIncludingChildren(IsElectronicallyCertificated);
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_UC_NKCountryOfOrigin
		{
			get { return base.US_UC_NKCountryOfOrigin; }
			set
			{
				base.US_UC_NKCountryOfOrigin = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_UC_NKCountryOfOrigin != US_UC_NKCountryOfOrigin)
					{
						declarationFSISLine.US_UC_NKCountryOfOrigin = US_UC_NKCountryOfOrigin;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsFieldReadonlyWhenCertificateAlreadyInUseOrElectronic))]
		public override ZString US_ProductIDQualifier
		{
			get { return base.US_ProductIDQualifier; }
			set
			{
				base.US_ProductIDQualifier = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce && !IsElectronicallyCertificated)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_ProductIDQualifier != US_ProductIDQualifier)
					{
						declarationFSISLine.US_ProductIDQualifier = US_ProductIDQualifier;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsFieldReadonlyWhenCertificateAlreadyInUseOrElectronic))]
		public override ZString US_ProductID
		{
			get { return base.US_ProductID; }
			set
			{
				base.US_ProductID = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce && !IsElectronicallyCertificated)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_ProductID != US_ProductID)
					{
						declarationFSISLine.US_ProductID = US_ProductID;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsFieldReadonlyWhenCertificateAlreadyInUseOrElectronic))]
		public override ZString US_IntendedUseCode
		{
			get { return base.US_IntendedUseCode; }
			set
			{
				base.US_IntendedUseCode = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce && !IsElectronicallyCertificated)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_IntendedUseCode != US_IntendedUseCode)
					{
						declarationFSISLine.US_IntendedUseCode = US_IntendedUseCode;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		[BusinessObjectTestExclude]
		public override ZString US_SealNumbers
		{
			get { return base.US_SealNumbers; }
			set
			{
				base.US_SealNumbers = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_SealNumbers != US_SealNumbers)
					{
						declarationFSISLine.US_SealNumbers = US_SealNumbers;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZDateTime US_DateOfInspection
		{
			get { return base.US_DateOfInspection; }
			set
			{
				base.US_DateOfInspection = value;

				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_DateOfInspection != US_DateOfInspection)
					{
						declarationFSISLine.US_DateOfInspection = US_DateOfInspection;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_PGAContactName
		{
			get { return base.US_PGAContactName; }
			set
			{
				base.US_PGAContactName = value;
				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_PGAContactName != US_PGAContactName)
					{
						declarationFSISLine.US_PGAContactName = US_PGAContactName;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_PGAContactPhoneNo
		{
			get { return base.US_PGAContactPhoneNo; }
			set
			{
				base.US_PGAContactPhoneNo = value;
				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_PGAContactPhoneNo != US_PGAContactPhoneNo)
					{
						declarationFSISLine.US_PGAContactPhoneNo = US_PGAContactPhoneNo;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsCertificateEmptyOrAlreadyInUseMoreThanOnce))]
		public override ZString US_PGAContactEmail
		{
			get { return base.US_PGAContactEmail; }
			set
			{
				base.US_PGAContactEmail = value;
				if (!IsCertificateEmptyOrAlreadyInUseMoreThanOnce)
				{
					var declarationFSISLine = GetOrCreateDeclarationFSISLine();
					if (declarationFSISLine != null && declarationFSISLine.US_PGAContactEmail != US_PGAContactEmail)
					{
						declarationFSISLine.US_PGAContactEmail = US_PGAContactEmail;
					}
				}
			}
		}

		bool IsCertificateEmptyOrAlreadyInUseMoreThanOnce
		{
			get
			{
				var declaration = Parent != null ? Parent.Declaration : null;

				return US_HealthCertificateNumber.IsEmpty
					|| declaration != null && declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(line => line.FSISLines.Cast<USInvoiceLineFSISLine>())
						.Where(fsisLine => fsisLine.US_HealthCertificateNumber == US_HealthCertificateNumber).Take(2).Count() > 1;
			}
		}

		bool IsFieldReadonlyWhenCertificateAlreadyInUseOrElectronic
		{
			get { return IsElectronicallyCertificated || IsCertificateEmptyOrAlreadyInUseMoreThanOnce; }
		}

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USInvoiceLineFSISLine|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = Parent;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		[ReadOnlyMember(nameof(IsElectronicallyCertificated))]
		[ChildEditable(true)]
		public USFSISLotCollection Lots
		{
			get
			{
				if (fLots == null)
				{
					fLots = new USFSISLotCollection(this);
					fLots.Load();
					RegisterEditableChildObject(fLots);
				}
				return fLots;
			}
		}
		USFSISLotCollection fLots;

		public override void Delete()
		{
			RefreshInvoiceLineFSISLines();
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Lots.RemoveAndDeleteAll();
			base.Delete();
		}

		void RefreshInvoiceLineFSISLines()
		{
			if (!US_HealthCertificateNumber.IsEmpty)
			{
				var declarationFSISLine = GetDeclarationFSISLine();
				if (declarationFSISLine != null)
				{
					declarationFSISLine.RefreshInvoiceLineFSISLines();
				}
			}
		}

		public new USInvoiceLineFSISLineValidation Validation
		{
			get { return (USInvoiceLineFSISLineValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new USInvoiceLineFSISLineValidation(this);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (USInvoiceLineFSISLine)base.CloneInternal(args);

			foreach (USFSISLot lot in Lots)
			{
				result.Lots.Add((USFSISLot)lot.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(USFSISLot), false)));
			}
			result.Lots.OfType<USFSISLot>().ToList().ForEach(x => x.ResetValueAfterClone());

			ClearValuesAfterClone(result);
			return result;
		}

		#region IFSISLine

		ZInt IFSISLine.LineNumber
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IFSISLine.CountryOfOrigin
		{
			get { return US_UC_NKCountryOfOrigin; }
		}

		ZString IFSISLine.CertificateIssuerCountry
		{
			get { return US_UC_NKCertificateIssuerCountry; }
		}

		ZString IFSISLine.HealthCertifcateNumber
		{
			get { return US_HealthCertificateNumber; }
		}

		ZString IFSISLine.ExportingEstNo
		{
			get { return US_ExportingEstNo; }
		}

		ZDateTime IFSISLine.ScheduleInspectionDate
		{
			get { return US_DateOfInspection; }
		}

		ZString IFSISLine.ImportingEstNo
		{
			get { return US_ImportingEstNo; }
		}

		IEnumerable<ZString> IFSISLine.SealNumbers
		{
			get { return new SealNumberBusinessObjectCollection(this.US_SealNumbers).GetSealNumbersAsCollection(); }
		}

		IEnumerable<IFSISLot> IFSISLine.Lots
		{
			get { return Lots.Cast<IFSISLot>(); }
		}

		IPGAContactDetails IFSISLine.Importer
		{
			get { return Parent.Declaration.IORWrapper; }
		}

		IPGAContactDetails IFSISLine.Consignee
		{
			get { return OrgHeaderWrapper.New(Parent.InvoiceHeader.Importer); }
		}

		ZString IFSISLine.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString IFSISLine.ProductID
		{
			get { return US_ProductID; }
		}

		ZString IFSISLine.ProductIDQualifier
		{
			get { return US_ProductIDQualifier; }
		}

		ICustomsBrokerDetails IFSISLine.Broker
		{
			get { return Parent; }
		}

		ICustomsBrokerDetails IFSISLine.ContactDetails
		{
			get { return this; }
		}

		ZDate IFSISLine.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_FSISSignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_FSISSignDate = value;
				}
			}
		}

		ZString IFSISLine.DeclarationCertificate
		{
			get
			{
				return ((IFSISLine)this).CertifySignatureDate.IsValid ? "Y" : "";
			}
		}

		ZString IFSISLine.CertifyingIndividual
		{
			get { return this.US_CertifyingIndividual; }
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USFSISLot, typeof(USFSISLot));
			return result;
		}

		#endregion

		#region Document Properties

		public USCCountry CountryOfOrigin
		{
			get { return Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, US_UC_NKCountryOfOrigin); }
		}

		public ZZRefCusCodeListCombined FSISImportEstablishment
		{
			get
			{
				return Factory.GetCachedValue("FSISImportEstablishment" + US_ImportingEstNo, delegate()
				{
					var query = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers);
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, US_ImportingEstNo);
					return Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
				});
			}
		}

		public ZString ImportEstablishmentName
		{
			get
			{
				var result = ZString.Empty;
				var fsisImportEstablishment = FSISImportEstablishment;
				if (fsisImportEstablishment != null)
				{
					result = fsisImportEstablishment.GetAttribute(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberCompany);
				}
				return result;
			}
		}

		public ZString ImportEstablishmentAddress
		{
			get
			{
				var result = ZString.Empty;
				var fsisImportEstablishment = FSISImportEstablishment;
				if (fsisImportEstablishment != null)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", fsisImportEstablishment.GetAttribute(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberStreet), fsisImportEstablishment.GetAttribute(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberCity), fsisImportEstablishment.GetAttribute(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberState), fsisImportEstablishment.GetAttribute(RefCusCodeListAttributeTypes.Codes.USFSISEstablishmentNumberZip));
				}
				return result;
			}
		}

		public ZBool HasFSISLots
		{
			get { return Lots.Count > 0; }
		}

		public ZString ExpirationDate => RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540ED);

		public ZString RevisionStatementDate => RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RD);

		public ZString ReplacementStatement => RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.FSIS9540RP);

		RefSysConfig.Loader RefSysConfigLoader
		{
			get { return refSysConfigLoader ?? (refSysConfigLoader = new RefSysConfig.Loader(Factory)); }
		}
		RefSysConfig.Loader refSysConfigLoader;
		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get
			{
				var invoiceLine = Parent;
				var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
				return declaration == null ? ZGuid.Empty : declaration.PK;
			}
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				var invoiceLine = Parent;
				var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
				return declaration == null ? null : declaration.DocManagerInfo;
			}
		}

		#endregion

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return Parent; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_FSISInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_FSISDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.JZ_OH_Buyer, JobComInvoiceHeader.Schema.US_FDAContactName, JobComInvoiceHeader.Schema.US_FDAContactPhoneNo, JobComInvoiceHeader.Schema.US_FDAContactEmail };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.US_FDAContactName, JobDeclaration.Schema.US_FDAContactPhoneNo, JobDeclaration.Schema.US_FDAContactEmail, JobDeclaration.Schema.JE_OH_Importer };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion
		#region ICustomsBrokerDetails Members

		IAddressDetails ICustomsBrokerDetails.Address
		{
			get
			{
				if (US_CertifyingIndividual == PartyTypeList.Codes.Importer)
				{
					if (Parent.Declaration != null)
					{
						return ((IPGAContactDetails)OrgHeaderWrapper.New(Parent.Declaration.IOR))?.CompanyAddress;
					}
				}
				else if (US_CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
				{
					if (Parent != null)
					{
						return ((ICustomsBrokerDetails)Parent).Address;
					}
				}
				return null;
			}
		}

		ZString ICustomsBrokerDetails.ContactName
		{
			get { return US_PGAContactName; }
		}

		ZString ICustomsBrokerDetails.ContactPhone
		{
			get { return US_PGAContactPhoneNo; }
		}

		ZString ICustomsBrokerDetails.ContactEmail
		{
			get { return US_PGAContactEmail; }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USInvoiceLineFSISLine businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.FSI; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}
		#endregion
	}
}
