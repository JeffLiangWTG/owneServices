using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
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
	public class PGA : AutoPGA, ILaceyActCommon, ICusAddInfoTypeSupporter, ICustomsBrokerDetails
		, IPGADataCorrection
		, ICanDelete
		, ICusDispositionParent
	{
		public PGA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoPGA.Schema
		{
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "Lacey Act line"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			PGA result = (PGA)base.CloneInternal(args);

			foreach (ConstituentElement element in PG04ConstituentElements)
			{
				result.PG04ConstituentElements.Add((ConstituentElement)element.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ConstituentElement), false)));
			}

			foreach (License element in Licenses)
			{
				result.Licenses.Add((License)element.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(License), false)));
			}

			foreach (LaceyCountry country in LaceyCountries)
			{
				result.LaceyCountries.Add((LaceyCountry)country.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(LaceyCountry), false)));
			}

			return result;
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			ContainersForPGALine.RemoveAndDeleteAll();
			PG04ConstituentElements.RemoveAndDeleteAll();
			Licenses.RemoveAndDeleteAll();
			LaceyCountries.DeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}
		public override ZString US_CertifyingIndividual
		{
			get { return base.US_CertifyingIndividual; }
			set
			{
				var hasChange = US_CertifyingIndividual != value;
				base.US_CertifyingIndividual = value;

				if (hasChange && !IsCopying)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							var importerWrapper = InvoiceLine.IORWrapper as IPGAContactDetails;
							if (importerWrapper != null)
							{
								US_PGAContactName = importerWrapper.Name;
								US_PGAContactPhoneNo = importerWrapper.PhoneNumber.Left(USPGAAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = importerWrapper.EmailAddress;
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							var brokerWrapper = invoiceLine as ICustomsBrokerDetails;
							if (brokerWrapper != null)
							{
								US_PGAContactName = brokerWrapper.ContactName;
								US_PGAContactPhoneNo = brokerWrapper.ContactPhone.Left(USPGAAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = brokerWrapper.ContactEmail;
							}
						}
					}
				}
			}
		}

		public override ZInt US_PGALineItemNumber
		{
			get { return base.US_PGALineItemNumber; }
			set
			{
				var oldValue = US_PGALineItemNumber;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						base.US_PGALineItemNumber = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}

		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(US_PGALineValue_ReadOnly))]
		public override ZDecimal US_PGALineValue
		{
			get { return base.US_PGALineValue; }
			set
			{
				try
				{
					suspendTrackingStatusChange = true;
					base.US_PGALineValue = value.Round(0);
				}
				finally
				{
					suspendTrackingStatusChange = false;
				}
			}
		}
		bool suspendTrackingStatusChange;

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.PGA|US_TrackingStatusDesc", Caption = "Status")]
		[ReadOnly(true)]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		bool US_PGALineValue_ReadOnly
		{
			get { return true; }
		}

		public ZString PGAValue
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = this.InvoiceLine;

				if (invoiceLine == null || invoiceLine.IsOGAValueUpToDate)
				{
					result = US_PGALineValue.ToString(0);
				}
				else
				{
					result = "...";
				}

				return result;
			}
		}

		public ZPropertyInfo PGAValueInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PGAValue), (x) => US_PGALineValueInfo); }
		}

		[DecimalPlaces(2)]
		public override ZDecimal US_InvCurrPGAValue
		{
			get { return base.US_InvCurrPGAValue; }
			set
			{
				base.US_InvCurrPGAValue = value;

				var invoiceLine = this.InvoiceLine;

				if (invoiceLine != null && invoiceLine.Declaration != null)
				{
					invoiceLine.Declaration.MarkApportionmentDirty();
				}
			}
		}

		public override ZBool US_UnknownBreakdownTotal
		{
			get { return base.US_UnknownBreakdownTotal; }
			set
			{
				var hasChanges = US_UnknownBreakdownTotal != value;
				base.US_UnknownBreakdownTotal = value;
				if (hasChanges)
				{
					if (value)
					{
						PG04ConstituentElements.OfType<ConstituentElement>().ForEach(x =>
						{
							x.US_PGANameOfTheConstituentElement = ZString.Empty;
							x.US_PGAQuantityOfConstituentElement = ZDecimal.Zero;
							x.US_PGAUnitOfMeasure = ZString.Empty;
							x.US_PGAPercentOfConstituentElement = ZDecimal.Zero;
							x.US_UnknownBreakdownCountryCode = ZString.Empty;
						});

						LaceyCountries.RemoveAndDeleteAll();
					}
					else
					{
						US_NameOfConstituentElement = ZString.Empty;
						US_QuantityOfConstituentElement = ZDecimal.Zero;
						US_UnitOfMeasure = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region Related Objects

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		//DO NOT CACHE - Memory Held up by local cache
		public RelatedContainersCollection ContainersForInvoiceLine
		{
			get { return new RelatedContainersCollection(this); }
		}

		[ChildEditable(true)]
		public PGARelatedContainersGenPivotCollection ContainersForPGALine
		{
			get
			{
				if (containersForPGALine == null)
				{
					containersForPGALine = new PGARelatedContainersGenPivotCollection(this);
					containersForPGALine.Load();
					RegisterEditableChildObject(containersForPGALine);
				}

				return containersForPGALine;
			}
		}
		PGARelatedContainersGenPivotCollection containersForPGALine;

		#endregion

		#region ILaceyActCommon Data

		ICustomsBrokerDetails ILaceyActCommon.ContactDetails
		{
			get { return this; }
		}

		ZString ILaceyActCommon.CertifyingIndividual
		{
			get { return US_CertifyingIndividual; }
		}

		ZString ILaceyActCommon.CommercialDescription
		{
			get { return US_PGACommercialDescription; }
		}

		ZInt ILaceyActCommon.PGALineItemNumber
		{
			get { return US_PGALineItemNumber; }
			set { US_PGALineItemNumber = value; }
		}

		IEnumerable<IConstituentElement> ILaceyActCommon.ConstituentElements
		{
			get { return new TypedEnumerable<IConstituentElement>(PG04ConstituentElements); }
		}

		ZBool ILaceyActCommon.ShouldSendPG15PG16Records
		{
			get { return PG04ConstituentElements.Count > 1; }
		}

		ZDecimal ILaceyActCommon.PGALineValue
		{
			get { return US_PGALineValue; }
		}

		IEnumerable<IContainerNumber> ILaceyActCommon.ContainerNumbers
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null && invoiceLine.Declaration.IsACECargoCertificationMode)
				{
					return new TypedEnumerable<IContainerNumber>(ContainersForInvoiceLine);
				}

				return new TypedEnumerable<IContainerNumber>(ContainersForPGALine);
			}
		}

		ZString IOGALine.CommercialDesc
		{
			get { return US_PGACommercialDescription; }
			set { US_PGACommercialDescription = value; }
		}

		IPGAContactDetails ILaceyActCommon.ImporterContactDetails
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.IORWrapper : null;
			}
		}

		ZBool ILaceyActCommon.UnknownBreakdown
		{
			get { return US_UnknownBreakdown; }
		}

		ZBool ILaceyActCommon.UnknownBreakdownTotal
		{
			get { return US_UnknownBreakdownTotal; }
		}

		ZString ILaceyActCommon.NameOfConstituent
		{
			get { return US_NameOfConstituentElement; }
		}

		ZDecimal ILaceyActCommon.QuantityOfConstituent
		{
			get { return US_QuantityOfConstituentElement; }
		}

		ZString ILaceyActCommon.UnitOfMeasure
		{
			get { return US_UnitOfMeasure; }
		}

		IEnumerable<ILaceyCountry> ILaceyActCommon.CountryCodes
		{
			get { return new TypedEnumerable<ILaceyCountry>(LaceyCountries); }
		}

		ZDate ILaceyActCommon.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_LACEYACTSignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_LACEYACTSignDate = value;
				}
			}
		}

		ZString ILaceyActCommon.DeclarationCertificate
		{
			get
			{
				return ((ILaceyActCommon)this).CertifySignatureDate.IsValid ? "Y" : "";
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
			get { return suspendTrackingStatusChange || AddInfo.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_LaceyIndicator };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_LaceyDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return Array.Empty<string>();
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
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

		#region ICustomsBrokerDetails Members

		IAddressDetails ICustomsBrokerDetails.Address
		{
			get
			{
				var invoiceLine = InvoiceLine;

				if (US_CertifyingIndividual == PartyTypeList.Codes.Importer)
				{
					if (invoiceLine.Declaration != null)
					{
						return ((IPGAContactDetails)OrgHeaderWrapper.New(invoiceLine.Declaration.IOR))?.CompanyAddress;
					}
				}
				else if (US_CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
				{
					if (invoiceLine != null)
					{
						return ((ICustomsBrokerDetails)invoiceLine).Address;
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

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(PGA pga)
				: base(pga)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		[ChildEditable(true)]
		public PG04ConstituentElementCollection PG04ConstituentElements
		{
			get
			{
				if (constituentElements == null)
				{
					constituentElements = new PG04ConstituentElementCollection(this);
					constituentElements.Load();
					constituentElements.CountChanged += constituentElements_CountChanged;
					RegisterEditableChildObject(constituentElements);
				}
				return constituentElements;
			}
		}
		PG04ConstituentElementCollection constituentElements;

		[ChildEditable(true)]
		public LaceyCountryCollection LaceyCountries
		{
			get
			{
				if (laceyCountries == null)
				{
					laceyCountries = new LaceyCountryCollection(this);
					laceyCountries.Load();
					RegisterEditableChildObject(laceyCountries);
				}
				return laceyCountries;
			}
		}
		LaceyCountryCollection laceyCountries;

		void constituentElements_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsDeleted && !IsDeleting && IsACommittedElement)
			{
				AddInfoValidation.ValidateAll();
			}
		}

		bool IsACommittedElement
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null && !invoiceLine.LaceyActLines.IsNonCommittedCollectionElement(this);
			}
		}

		internal void SetPGARelatedContainers()
		{
			if (!IsDeleted)
			{
				ContainersForPGALine.AddMissingPivotIfOnlyOneContainer();
			}
		}

		public ZInt ContainersCount
		{
			get
			{
				ZInt result = 0;
				foreach (IContainerNumber number in ((ILaceyActCommon)this).ContainerNumbers)
				{
					result++;
				}
				return result;
			}
		}

		[ChildEditable(true)]
		public LicenseCollection Licenses
		{
			get
			{
				if (licenses == null)
				{
					licenses = new LicenseCollection(this);
					licenses.Load();
					RegisterEditableChildObject(licenses);
				}
				return licenses;
			}
		}
		LicenseCollection licenses;

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USPGA, typeof(ConstituentElement));
			result.Add(CusAddInfoTypeAttribute.Codes.USLaceyActLicense, typeof(License));
			result.Add(CusAddInfoTypeAttribute.Codes.USLaceyCountries, typeof(LaceyCountry));
			return result;
		}

		#endregion

		#region Transform data between ACE and ACS

		public void TransformData(string newMessagingMode)
		{
			if (newMessagingMode == JobApplicationCodeList.Codes.ACE)
			{
				var newConstituentElements = new List<ConstituentElement>();
				foreach (var constituentElement in PG04ConstituentElements.Cast<ConstituentElement>().ToArray())
				{
					var scientificDataCollection = constituentElement.ScientificDataCollection.Cast<ScientificData>().Where(x => !x.US_PGACountryCode.IsEmpty || !x.US_PGAScientificGenusName.IsEmpty || !x.US_PGAScientificSpeciesName.IsEmpty).ToArray();
					if (scientificDataCollection.Length > 0)
					{
						var constituentElementCountryCode = constituentElement.US_UnknownBreakdownCountryCode;

						constituentElement.US_UnknownBreakdownCountryCode = ZString.Empty;

						foreach (var scientificData in scientificDataCollection)
						{
							var additionalCountries = new List<ZString>();
							if (!constituentElementCountryCode.IsEmpty)
							{
								additionalCountries.Add(constituentElementCountryCode);
							}
							if (!scientificData.US_PGACountryCode.IsEmpty && !additionalCountries.Contains(scientificData.US_PGACountryCode))
							{
								additionalCountries.Add(scientificData.US_PGACountryCode);
							}

							if (constituentElement.US_UnknownBreakdownCountryCode.IsEmpty)
							{
								var countryCode = additionalCountries.FirstOrDefault();
								CopyFromScientificDataToConstituentElement(constituentElement, scientificData.US_PGAScientificGenusName, scientificData.US_PGAScientificSpeciesName, countryCode);
								if (!countryCode.IsEmpty)
								{
									additionalCountries.Remove(countryCode);
								}
							}

							foreach (var additionalCountry in additionalCountries)
							{
								newConstituentElements.Add(NewConstituentElement(constituentElement, scientificData.US_PGAScientificGenusName, scientificData.US_PGAScientificSpeciesName, additionalCountry));
							}
						}
					}
				}
				PG04ConstituentElements.AddRange(newConstituentElements);
				PG04ConstituentElements.Cast<ConstituentElement>().ForEach(x => x.ScientificDataCollection.RemoveAndDeleteAll());
			}
			else
			{
				foreach (var constituentElement in PG04ConstituentElements.Cast<ConstituentElement>().Where(x => !x.US_UnknownBreakdownCountryCode.IsEmpty || !x.US_GenusName.IsEmpty || !x.US_SpeciesName.IsEmpty))
				{
					var scientificData = constituentElement.ScientificDataCollection.AddNew();
					scientificData.US_PGACountryCode = constituentElement.US_UnknownBreakdownCountryCode;
					scientificData.US_PGAScientificGenusName = constituentElement.US_GenusName;
					scientificData.US_PGAScientificSpeciesName = constituentElement.US_SpeciesName;

					constituentElement.US_UnknownBreakdownCountryCode = ZString.Empty;
					constituentElement.US_PGAPercentOfConstituentElement = constituentElement.US_PGAPercentOfConstituentElement.Round(3);
				}
			}
		}

		static ConstituentElement NewConstituentElement(ConstituentElement constituentElement, ZString genusName, ZString species, ZString countryCode)
		{
			var result = constituentElement.CopyIndividualColumns();
			CopyFromScientificDataToConstituentElement(result, genusName, species, countryCode);
			result.US_PGAQuantityOfConstituentElement = ZDecimal.Zero;
			return result;
		}

		static void CopyFromScientificDataToConstituentElement(ConstituentElement constituentElement, ZString genusName, ZString species, ZString countryCode)
		{
			constituentElement.US_GenusName = genusName;
			constituentElement.US_SpeciesName = species;
			constituentElement.US_UnknownBreakdownCountryCode = countryCode;
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.APH; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_PGALineItemNumber; }
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
