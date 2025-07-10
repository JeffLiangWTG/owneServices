using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceNumberSequenceConfiguration : RegistryBusinessObjectTemplate
	{
		public ComplianceNumberSequenceConfiguration()
			: base()
		{ }

		public ComplianceNumberSequenceConfiguration(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ComplianceNumberSequenceConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateCode();
			Validation.ValidateDescription();
		}

		public ComplianceNumberSequenceConfigurationValidation Validation => validation ?? (validation = new ComplianceNumberSequenceConfigurationValidation(this));

		ComplianceNumberSequenceConfigurationValidation validation;

		public ComplianceNumberSequenceConfigurationCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is ComplianceNumberSequenceConfigurationCollection) as ComplianceNumberSequenceConfigurationCollection;
				return result ?? new ComplianceNumberSequenceConfigurationCollection(CurrentFallbackLevel, CurrentFactory);
			}
		}

		[ChildEditable]
		public ComplianceNumberSequenceCustomisationElementCollection Elements
		{
			get
			{
				if (customisationElements == null)
				{
					customisationElements = new ComplianceNumberSequenceCustomisationElementCollection(CurrentFallbackLevel, CurrentFactory);
					customisationElements.PopulateElements();
					customisationElements.ParentConfiguration = this;
					RegisterEditableChildObject(customisationElements);
				}
				return customisationElements;
			}
		}
		ComplianceNumberSequenceCustomisationElementCollection customisationElements;

		void SetNewElements(ComplianceNumberSequenceCustomisationElementCollection elements)
		{
			if (elements != null)
			{
				foreach (var newName in ComplianceNumberSequenceCustomisationElement.GetAllElementNames().Except(elements.Cast<ComplianceNumberSequenceCustomisationElement>().Select(c => c.ElementName.ToString())))
				{
					elements.AddNew().ElementName = newName;
				}
			}
			if (customisationElements != null)
			{
				customisationElements.ParentConfiguration = null;
				UnRegisterEditableChildObject(customisationElements);
			}
			customisationElements = elements;
			if (customisationElements != null)
			{
				customisationElements.ParentConfiguration = this;
				RegisterEditableChildObject(customisationElements);
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceNumberSequenceConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((ComplianceNumberSequenceConfiguration)clone).SetNewElements((ComplianceNumberSequenceCustomisationElementCollection)Elements.Clone(CurrentFallbackLevel, CurrentFactory));
		}

		public override bool CanDelete => !IsInUse && !IsMandatory;

		public override MultilingualString ReasonForNotAbleToDelete
			=> IsMandatory ? ResString.GetMultilingualString("a5c094e1-d410-419f-bd63-3c9e3bf16748", "This code is mandatory thus cannot be deleted.")
				: ResString.GetMultilingualString("7c8eb967-65b9-444e-95bb-4be6c26f9602", "This code is already used thus cannot be deleted.");

		bool IsInUse
		{
			get
			{
				if (Code.IsEmpty)
				{
					return false;
				}
				else if (!isInUse.HasValue)
				{
					var query = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, CurrentFallbackLevel.CompanyPK(false));
					query.AddToFilter(AccComplianceSequenceSchema.XD_NumberFormat, Code);
					isInUse = CurrentFactory.Exists(typeof(AccComplianceSequence), query);
				}
				return isInUse.Value;
			}
		}
		bool? isInUse;

		public bool IsMandatory
		{
			get
			{
				if (Code.IsEmpty)
				{
					return false;
				}
				else if (!isMandatory.HasValue)
				{
					var countryCode = AccountingMasterFilesUtils.GetCountryCodeFromRegistryFallBackLevel(CurrentFactory, CurrentFallbackLevel);
					var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(countryCode);
					if (complianceInfo != null)
					{
						var mandatoryConfigs = complianceInfo.GetMandatoryComplianceNumberSequenceConfiguration();
						isMandatory = mandatoryConfigs.Any(x => x.Code == Code);
					}
				}

				return isMandatory.HasValue && isMandatory.Value;
			}
		}
		bool? isMandatory;

		#region Bound Properties

		#region Code

		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCode();
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public bool Code_ReadOnly
		{
			get { return IsInUse || IsMandatory; }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return description; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDescription();
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public bool Description_ReadOnly => IsMandatory;

		#endregion

		#endregion

		IEnumerable<ComplianceNumberSequenceCustomisationElement> IncludedElementCollection => Elements.Cast<ComplianceNumberSequenceCustomisationElement>().Where(x => x.Include);

		public string ValidateOrdersForIncludedElements()
		{
			if (!IncludedElementCollection.Any())
			{
				return Res.GetString("730FF51B-358C-4BD2-BAD7-8332BDF99096", "Configuration Code {0} : There should be at least one element is included.", Code);
			}

			return string.Empty;
		}

		#region Format Number

		public ZString GetFormatedComplianceDocumentNumber(IComplianceNumberSequence complianceNumberSequence, ZString nextNumberAsString, ZString prefix)
		{
			ZString complianceNumber = default;
			if (IncludedElementCollection.Any())
			{
				foreach (var element in IncludedElementCollection.OrderBy(x => x.Order))
				{
					var digitCode = element.IsCustomElement ? element.DigitCode : GetDigitCode(element, complianceNumberSequence, nextNumberAsString, prefix);
					complianceNumber += digitCode;
				}
			}

			return complianceNumber;
		}

		ZString GetDigitCode(ComplianceNumberSequenceCustomisationElement element, IComplianceNumberSequence complianceNumberSequence, ZString nextNumberAsString, ZString prefix)
		{
			var digitCode = ZString.Empty;
			switch (element.ElementName)
			{
				case ElementNames.TaxStatusCode:
					var taxStatusCode = CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(GlbCompany.CurrentCompany.Country.Code)?.GetComplianceSubTypes()?.First(x => x.Code == complianceNumberSequence.ComplianceSubType)?.TaxStatusCode ?? ZString.Empty;
					digitCode = taxStatusCode;
					break;
				case ElementNames.OriginalAmendmentStatus:
					digitCode = complianceNumberSequence.IsCorrected ? element.AmendmentStatusCode : element.OriginalStatusCode;
					break;
				case ElementNames.TransactionType:
					var transactionType = complianceNumberSequence.ComplianceTransactionType;
					if (transactionType == TransactionTypes.Invoice)
					{
						digitCode = element.InvoiceCode;
					}
					else if (transactionType == TransactionTypes.CreditNote)
					{
						digitCode = element.CreditNoteCode;
					}
					else if (transactionType == TransactionTypes.AdjustmentNote)
					{
						digitCode = element.AdjustmentNoteCode;
					}
					break;
				case ElementNames.BlankSpace:
					for (int i = 0; i < element.Length; i++)
					{
						digitCode += new ZString(" ");
					}
					break;
				case ElementNames.SeriesPrefix:
					digitCode = prefix;
					break;
				case ElementNames.SequenceNumber:
					digitCode = nextNumberAsString;
					break;
				case ElementNames.ComplianceDateDayOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.ComplianceDocumentDate, DateElement.Day);
					break;
				case ElementNames.ComplianceDateMonthOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.ComplianceDocumentDate, DateElement.Month);
					break;
				case ElementNames.ComplianceDateYearOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.ComplianceDocumentDate, DateElement.Year, element.Length);
					break;
				case ElementNames.ComplianceSubType:
					digitCode = complianceNumberSequence.ComplianceSubType;
					break;
				case ElementNames.PostDateDayOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.PostDate, DateElement.Day);
					break;
				case ElementNames.PostDateMonthOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.PostDate, DateElement.Month);
					break;
				case ElementNames.PostDateYearOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.PostDate, DateElement.Year, element.Length);
					break;
				case ElementNames.InvoiceDateDayOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.InvoiceDate, DateElement.Day);
					break;
				case ElementNames.InvoiceDateMonthOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.InvoiceDate, DateElement.Month);
					break;
				case ElementNames.InvoiceDateYearOfIssue:
					digitCode = GetDigitCodeFromDate(complianceNumberSequence.InvoiceDate, DateElement.Year, element.Length);
					break;
			}

			return digitCode;
		}

		enum DateElement
		{
			Year,
			Month,
			Day
		}

		string GetDigitCodeFromDate(ZDateTime dateTime, DateElement dateElement, int length = 0)
		{
			var result = string.Empty;
			if (!dateTime.IsEmpty)
			{
				switch (dateElement)
				{
					case DateElement.Year:
						var year = dateTime.Year.ToString(CultureInfo.InvariantCulture);
						result = year.Substring(year.Length - length);
						break;
					case DateElement.Month:
						result = dateTime.Month.ToString("00", CultureInfo.InvariantCulture);
						break;
					case DateElement.Day:
						result = dateTime.Day.ToString("00", CultureInfo.InvariantCulture);
						break;
					default:
						break;
				}
			}

			return result;
		}

		#endregion

		#region Total Length

		public ZInt GetTotalLengthOfIncludedElements(ZByte maximumNumberDigits, ZInt prefixLength, ZString sequenceClass)
		{
			var includedElementsLength = IncludedElementCollection.Sum(x => x.Length) + maximumNumberDigits;

			if (IncludedElementCollection.Any(x => x.ElementName == ElementNames.SeriesPrefix))
			{
				includedElementsLength += prefixLength;
			}
			if (IncludedElementCollection.Any(x => x.ElementName == ElementNames.TaxStatusCode))
			{
				includedElementsLength += GetTaxStatusCodesMaxLength(sequenceClass);
			}
			if (IncludedElementCollection.Any(x => x.ElementName == ElementNames.ComplianceSubType))
			{
				includedElementsLength += sequenceClass.Length;
			}

			return includedElementsLength;
		}

		ZInt GetTaxStatusCodesMaxLength(string parentSubtype)
		{
			var maxLengthOfTaxStatus = 0;

			var complianceSubTypeDependencySetup = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.Value;
			var childSubTypes = complianceSubTypeDependencySetup?.Cast<ComplianceSubTypeDependencyConfiguration>()?.Where(x => x.ParentSubType == parentSubtype)?.Select(y => y.ChildSubType);

			if (childSubTypes != null && childSubTypes.Any())
			{
				var countryComplianceFactory = ObjectFactory.Get<ICountryComplianceFactoryIntegration>();
				maxLengthOfTaxStatus = countryComplianceFactory.GetIComplianceSubTypeCodeProvider(GlbCompany.CurrentCompany.Country.Code)?.GetComplianceSubTypes()?.Where(x => childSubTypes.Contains(x.Code))?.Max(y => y.TaxStatusCode.Length) ?? ZInt.Zero;
			}

			return maxLengthOfTaxStatus;
		}

		#endregion

		#region Elements Changed Log

		public string GetElementsChangedLog()
		{
			var result = new ZStringBuilder();

			var includedElements = Elements.Cast<ComplianceNumberSequenceCustomisationElement>().Where(x => x.Include).OrderBy(y => y.Order);
			foreach (var element in includedElements)
			{
				ZString content;
				if (element.ElementName == ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace)
				{
					content = element.Length.ToString();
				}
				else
				{
					content = element.DigitCode;
				}
				content = content.IsEmpty ? content : ZString.Format(" '{0}'", content);

				result.Append(Res.GetString("bd641159-d6a0-4f41-a833-b2343adbfdb8", "{0}-{1}{2}, ", element.Order, element.ElementName, content));
			}
			return result.ToString().TrimEnd(',', ' ');
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description);

			CollectionSerialiser.Serialize(writer, Elements);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Description = reader.ReadElementString(Schema.Description);

			SetNewElements(((ComplianceNumberSequenceCustomisationElementCollection)CollectionSerialiser.Deserialize(reader)));
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(ComplianceNumberSequenceCustomisationElementCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
