using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Registry.Business.InvoiceRemittanceCustomisationElement;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRemittanceConfiguration : RegistryBusinessObjectTemplate
	{
		public InvoiceRemittanceConfiguration()
			: base()
		{ }

		public InvoiceRemittanceConfiguration(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public InvoiceRemittanceConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string BillerCode = "BillerCode";
			public const string BillerAccountNumber = "BillerAccountNumber";
			public const string MaxPossibleLength = "MaxPossibleLength";
			public const string Message = "Message";
			public const string DebtorLocation = "DebtorLocation";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateCode();
			Validation.ValidateDescription();
			Validation.ValidateMaxPossibleLength();
			Validation.ValidateDebtorLocation();
		}

		public InvoiceRemittanceConfigurationValidation Validation => validation ?? (validation = new InvoiceRemittanceConfigurationValidation(this));

		InvoiceRemittanceConfigurationValidation validation;

		public InvoiceRemittanceConfigurationCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is InvoiceRemittanceConfigurationCollection) as InvoiceRemittanceConfigurationCollection;
				return result ?? new InvoiceRemittanceConfigurationCollection(CurrentFallbackLevel, CurrentFactory);
			}
		}

		[ChildEditable]
		public InvoiceRemittanceCustomisationElementCollection Elements
		{
			get
			{
				if (customisationElements == null)
				{
					customisationElements = new InvoiceRemittanceCustomisationElementCollection(CurrentFallbackLevel, CurrentFactory);
					customisationElements.PopulateElements();
					customisationElements.ParentConfiguration = this;
					RegisterEditableChildObject(customisationElements);
				}
				return customisationElements;
			}
		}
		InvoiceRemittanceCustomisationElementCollection customisationElements;

		void SetNewElements(InvoiceRemittanceCustomisationElementCollection elements)
		{
			if (elements != null)
			{
				foreach (var newName in GetAllElementNames().Except(elements.Cast<InvoiceRemittanceCustomisationElement>().Select(c => c.ElementName.ToString())))
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
			return new InvoiceRemittanceConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((InvoiceRemittanceConfiguration)clone).SetNewElements((InvoiceRemittanceCustomisationElementCollection)Elements.Clone(CurrentFallbackLevel, CurrentFactory));
		}

		public override bool CanDelete => !IsInUse;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("B345D356-E415-4E7C-93C4-2E2D14A6D03D", "This code is already used thus cannot be deleted.");

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
					var query = new ZQuery(AccTransactionHeaderSchema.AH_GC, CurrentFallbackLevel.CompanyPK(false));
					query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePaymentReferenceCode, Code);
					isInUse = CurrentFactory.Exists(typeof(AccTransactionHeader), query);
				}
				return isInUse.Value;
			}
		}
		bool? isInUse;

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
			get { return IsInUse; }
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

		#endregion

		#region Biller Code

		[MaxLength(20)]
		public ZString BillerCode
		{
			get { return billerCode; }
			set
			{
				SetNonPersistentPropertyValue(BillerCodeInfo, ref billerCode, value);
			}
		}
		ZString billerCode;

		public ZPropertyInfo BillerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BillerCode); }
		}

		#endregion

		#region Billder Account Number

		[MaxLength(50)]
		public ZString BillerAccountNumber
		{
			get { return billerAccountNumber; }
			set
			{
				SetNonPersistentPropertyValue(BillerAccountNumberInfo, ref billerAccountNumber, value);
			}
		}
		ZString billerAccountNumber;

		public ZPropertyInfo BillerAccountNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BillerAccountNumber); }
		}

		#endregion

		#region Max Possible Length

		public ZInt MaxPossibleLength
		{
			get { return maxPossibleLength; }
			set
			{
				SetNonPersistentPropertyValue(MaxPossibleLengthInfo, ref maxPossibleLength, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMaxPossibleLength();
				}
			}
		}

		ZInt maxPossibleLength;

		public ZPropertyInfo MaxPossibleLengthInfo
		{
			get { return GetZPropertyInfo(Schema.MaxPossibleLength); }
		}

		#endregion

		#region Message

		[MaxLength(160)]
		public ZString Message
		{
			get { return message; }
			set
			{
				SetNonPersistentPropertyValue(MessageInfo, ref message, value);
			}
		}
		ZString message;

		public ZPropertyInfo MessageInfo
		{
			get { return GetZPropertyInfo(Schema.Message); }
		}

		#endregion

		#region Debtor Location

		[List("DebtorLocationList")]
		public ZString DebtorLocation
		{
			get { return debtorLocation; }
			set
			{
				SetNonPersistentPropertyValue(DebtorLocationInfo, ref debtorLocation, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDebtorLocation();
				}
			}
		}

		ZString debtorLocation;

		public ZPropertyInfo DebtorLocationInfo
		{
			get { return GetZPropertyInfo(Schema.DebtorLocation); }
		}

		#endregion

		public CodeDescriptionPairList DebtorLocationList
		{
			get
			{
				return AccountingTaxLocations.GetLocations(CurrentFactory);
			}
		}

		#endregion

		IEnumerable<InvoiceRemittanceCustomisationElement> IncludedElementCollection => Elements.Cast<InvoiceRemittanceCustomisationElement>().Where(x => x.Include);

		public ZInt ReferenceNumberTotalLength
		{
			get
			{
				ZInt length = ZInt.Zero;
				foreach (var element in IncludedElementCollection)
				{
					if (element.IsCustomElement)
					{
						length += element.DigitCode.Length;
					}
					else
					{
						length += ZInt.ParseSafe(element.DigitCode, ZInt.Zero);
					}
				}

				return length;
			}
		}

		public ZString GetInvoiceTotalInLocalCurrencyDigitCode(string invoiceTotal)
		{
			var result = ZString.Empty;
			if (Elements[ElementNames.InvoiceTotalInLocalCurrency].Include)
			{
				if (int.TryParse(Elements[ElementNames.InvoiceTotalInLocalCurrency].DigitCode, out int digitCode))
				{
					if (invoiceTotal.Length > digitCode)
					{
						invoiceTotal = invoiceTotal.Remove(0, invoiceTotal.Length - digitCode);
					}
					result = invoiceTotal.PadLeft(digitCode, '0');
				}
			}
			return result;
		}

		public ZString GetInvoiceTotalInInvoiceCurrencyDigitCode(string invoiceTotal)
		{
			var result = ZString.Empty;
			if (Elements[ElementNames.InvoiceTotalInInvoiceCurrency].Include)
			{
				if (int.TryParse(Elements[ElementNames.InvoiceTotalInInvoiceCurrency].DigitCode, out int digitCode))
				{
					if (invoiceTotal.Length > digitCode)
					{
						invoiceTotal = invoiceTotal.Remove(0, invoiceTotal.Length - digitCode);
					}
					result = invoiceTotal.PadLeft(digitCode, '0');
				}
			}
			return result;
		}

		public ZString GetInvoiceRemittanceReference(IInvoiceRemittance configuration)
		{
			ZString referenceNumber = default;
			if (IncludedElementCollection.Any())
			{
				var stack = new Stack<(string digitCode, string checkDigitAlgorithm)>();
				foreach (var element in IncludedElementCollection.OrderBy(x => x.Order))
				{
					if (!element.IsCheckDigitElement)
					{
						var digitCode = element.IsCustomElement ? element.DigitCode : GetDigitCode(element.ElementName, configuration);

						if (element.CheckDigit.IsEmpty)
						{
							if (stack.Count > 0)
							{
								var calculatedNumber = GetCalculatedNumber(stack, CheckDigitAlgorithm.MOD97);
								referenceNumber += calculatedNumber;
							}
							referenceNumber += digitCode;
						}
						else
						{
							stack.Push((digitCode, Elements[element.CheckDigit].CheckDigitAlgorithm));
						}
					}
					else if (IsMod97CheckDigit(element))
					{
						stack.Push(("00", CheckDigitAlgorithm.MOD97));
					}
					else
					{
						var calculatedNumber = GetCalculatedNumber(stack, element.CheckDigitAlgorithm);

						if (!element.CheckDigit.IsEmpty)
						{
							stack.Push((calculatedNumber, Elements[element.CheckDigit].CheckDigitAlgorithm));
						}
						else
						{
							referenceNumber += calculatedNumber;
						}
					}
				}

				var calculatedMod97Number = GetCalculatedNumber(stack, CheckDigitAlgorithm.MOD97);
				referenceNumber += calculatedMod97Number;
			}
			else
			{
				referenceNumber = ZString.Empty;
			}

			return referenceNumber;
		}

		ZString GetDigitCode(string elementName, IInvoiceRemittance configuration)
		{
			ZString digitCode = ZString.Empty;
			switch (elementName)
			{
				case ElementNames.InvoiceNumber:
					digitCode = configuration.InvoiceNumber;
					break;
				case ElementNames.InvoiceNumberInNumeric:
					digitCode = configuration.InvoiceNumberInNumeric;
					break;
				case ElementNames.InvoiceTotalInLocalCurrency:
					digitCode = configuration.InvoiceTotalInLocalCurrency;
					break;
				case ElementNames.InvoiceTotalInInvoiceCurrency:
					digitCode = configuration.InvoiceTotalInInvoiceCurrency;
					break;
				case ElementNames.BillerCode:
					digitCode = configuration.BillerCode;
					break;
				case ElementNames.BillerAccountNumber:
					digitCode = configuration.BillerAccountNumber;
					break;
				case ElementNames.DebtorOrganizationCode:
					digitCode = configuration.DebtorOrganizationCode;
					break;
				case ElementNames.DebtorClientNumber:
					digitCode = configuration.DebtorClientNumber;
					break;
				case ElementNames.InvoiceTransactionReference:
					digitCode = configuration.InvoiceTransactionReference;
					break;
			}

			return digitCode;
		}

		string GetCalculatedNumber(Stack<(string digitCode, string checkDigitAlgorithm)> stack, string checkDigitAlgorithm)
		{
			string digitToCalculate = string.Empty;
			if (stack.Count > 0)
			{
				while (stack.Count > 0)
				{
					var top = stack.Peek();
					if (top.checkDigitAlgorithm == checkDigitAlgorithm)
					{
						digitToCalculate = top.digitCode + digitToCalculate;
						stack.Pop();
					}
					else
					{
						break;
					}
				}

				digitToCalculate = GetAlgorithmCheckDigit(checkDigitAlgorithm, digitToCalculate);
			}

			return digitToCalculate;
		}

		bool IsMod97CheckDigit(InvoiceRemittanceCustomisationElement element)
		{
			return element.IsCheckDigitElement && element.CheckDigitAlgorithm == CheckDigitAlgorithm.MOD97;
		}

		string GetAlgorithmCheckDigit(string checkDigitAlgorithm, string number)
		{
			var result = string.Empty;
			var checkDigit = CheckDigitHelper.CalculateCheckDigit(checkDigitAlgorithm, number);
			if (checkDigitAlgorithm == CheckDigitAlgorithm.MOD97 && number.Length >= 4)
			{
				var temp = number.Substring(0, 4);
				result = temp.Substring(0, 2) + checkDigit.PadLeft(2,'0') + number.Remove(0, 4);
			}
			else
			{
				result = number + checkDigit;
			}

			return result;
		}

		public string ValidateOrdersForIncludedElements()
		{
			if (!IncludedElementCollection.Any())
			{
				return Res.GetString("730FF51B-358C-4BD2-BAD7-8332BDF99096", "Configuration Code {0} : There should be at least one element is included.", Code);
			}

			foreach (var element in IncludedElementCollection.Where(x => x.IsCheckDigitElement))
			{
				var checkDigitElements = IncludedElementCollection.Where(x => x.CheckDigit == element.ElementName).ToList();
				if (checkDigitElements.Count == 0)
				{
					return Res.GetString("4DA86BCF-8F69-4EEC-BD14-738A3984DD70", "Configuration Code {0} : Element {1} is included but not used in Check Digit.", Code, element.ElementName);
				}
				var maxOrder = checkDigitElements.Select(x => x.Order).Max();

				if (!IsMod97CheckDigit(element) && maxOrder > element.Order)
				{
					return Res.GetString("5910F88B-64BA-470D-A381-4EAB186E7D11", "Configuration Code {0} : Element {1} should be after the elements which use it.", Code, element.ElementName);
				}

				if (!IsMod97CheckDigit(element))
				{
					maxOrder = element.Order;
				}

				var minOrder = checkDigitElements.Select(x => x.Order).Min();
				if (IncludedElementCollection.Any(x => x.Order > minOrder && x.Order < maxOrder && x.CheckDigit.IsEmpty && !x.IsCheckDigitElement))
				{
					return Res.GetString("87CB5217-562B-4983-A477-086F147644DB", "Configuration Code {0} : Element {1} order list should not contain elements without check digit.", Code, element.ElementName);
				}
			}

			return string.Empty;
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.BillerCode, BillerCode);
			writer.WriteElementString(Schema.BillerAccountNumber, BillerAccountNumber);
			writer.WriteElementString(Schema.MaxPossibleLength, MaxPossibleLength.ToString());
			writer.WriteElementString(Schema.Message, Message);
			writer.WriteElementString(Schema.DebtorLocation, DebtorLocation);

			CollectionSerialiser.Serialize(writer, Elements);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Description = reader.ReadElementString(Schema.Description);
			BillerCode = reader.ReadElementString(Schema.BillerCode);
			BillerAccountNumber = reader.ReadElementString(Schema.BillerAccountNumber);
			MaxPossibleLength = ZInt.ParseSafe(reader.ReadElementString(Schema.MaxPossibleLength), ZInt.Zero);
			Message = reader.ReadElementString(Schema.Message);
			DebtorLocation = reader.ReadElementString(Schema.DebtorLocation);

			SetNewElements(((InvoiceRemittanceCustomisationElementCollection)CollectionSerialiser.Deserialize(reader)));
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(InvoiceRemittanceCustomisationElementCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
