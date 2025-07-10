using System;
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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CheckAlgorithm = Enterprise.NumberFountain.CheckDigitAlgorithm;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRemittanceCustomisationElement : RegistryBusinessObjectTemplate
	{
		public InvoiceRemittanceCustomisationElement()
			: base()
		{ }

		public InvoiceRemittanceCustomisationElement(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{ }

		#region Schema

		public abstract class Schema
		{
			public const string Order = "Order";
			public const string ElementName = "ElementName";
			public const string Include = "Include";
			public const string DigitCode = "DigitCode";
			public const string CheckDigit = "CheckDigit";
			public const string CheckDigitAlgorithm = "CheckDigitAlgorithm";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Only For developers")]
		public static class ElementNames
		{
			public const string CheckDigit1 = "Check Digit #1";
			public const string CheckDigit2 = "Check Digit #2";
			public const string CustomCode1 = "Custom Code #1";
			public const string CustomCode2 = "Custom Code #2";
			public const string CustomCode3 = "Custom Code #3";
			public const string CustomCode4 = "Custom Code #4";
			public const string CustomCode5 = "Custom Code #5";
			public const string CustomCode6 = "Custom Code #6";
			public const string CustomCode7 = "Custom Code #7";
			public const string CustomCode8 = "Custom Code #8";
			public const string CustomCode9 = "Custom Code #9";
			public const string CustomCode10 = "Custom Code #10";
			public const string InvoiceNumber = "Invoice Number";
			public const string InvoiceTotalInLocalCurrency = "Invoice Total in Local Currency";
			public const string InvoiceTotalInInvoiceCurrency = "Invoice Total in Invoice Currency";
			public const string BillerCode = "Biller Code";
			public const string BillerAccountNumber = "Biller Account Number";
			public const string DebtorOrganizationCode = "Debtor Organization Code";
			public const string DebtorClientNumber = "Debtor Client Number";
			public const string InvoiceTransactionReference = "Invoice Transaction Reference";
			public const string InvoiceNumberInNumeric = "Invoice Number (in Numeric)";
		}

		public static string[] GetAllElementNames()
		{
			return new string[] { ElementNames.CheckDigit1, ElementNames.CheckDigit2,
										ElementNames.CustomCode1, ElementNames.CustomCode2,ElementNames.CustomCode3,
										ElementNames.CustomCode4,ElementNames.CustomCode5,
										ElementNames.CustomCode6,ElementNames.CustomCode7,ElementNames.CustomCode8,
										ElementNames.CustomCode9,ElementNames.CustomCode10,
										ElementNames.InvoiceNumber,ElementNames.InvoiceTotalInLocalCurrency,ElementNames.InvoiceTotalInInvoiceCurrency,
										ElementNames.BillerCode,ElementNames.BillerAccountNumber,
										ElementNames.DebtorOrganizationCode,ElementNames.DebtorClientNumber,ElementNames.InvoiceTransactionReference,
										ElementNames.InvoiceNumberInNumeric };
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRemittanceCustomisationElement(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateOrder();
			Validation.ValidateInclude();
			Validation.ValidateDigitCode();
			Validation.ValidateCheckDigit();
			Validation.ValidateCheckDigitAlgorithm();
		}

		public InvoiceRemittanceCustomisationElementValidation Validation => validation ?? (validation = new InvoiceRemittanceCustomisationElementValidation(this));

		InvoiceRemittanceCustomisationElementValidation validation;

		public InvoiceRemittanceCustomisationElementCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is InvoiceRemittanceCustomisationElementCollection) as InvoiceRemittanceCustomisationElementCollection;
				return result ?? new InvoiceRemittanceCustomisationElementCollection(Factory);
			}
		}

		public InvoiceRemittanceConfiguration ParentConfiguration
		{
			get { return ParentCollection.ParentConfiguration; }
		}

		#region Bound Properties

		#region Order

		public ZByte Order
		{
			get { return order; }
			set
			{
				SetNonPersistentPropertyValue(OrderInfo, ref order, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOrder();
				}
			}
		}

		ZByte order;

		public ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		public bool Order_ReadOnly
		{
			get { return !Include; }
		}

		#endregion

		#region ElementName

		public ZString ElementName
		{
			get { return elementName; }
			set
			{
				SetNonPersistentPropertyValue(ElementNameInfo, ref elementName, value);
			}
		}
		ZString elementName;

		public ZPropertyInfo ElementNameInfo
		{
			get { return GetZPropertyInfo(Schema.ElementName); }
		}

		public bool ElementName_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Include

		public ZBool Include
		{
			get { return include; }
			set
			{
				SetNonPersistentPropertyValue(IncludeInfo, ref include, value);

				if (!IsValidationSuspended)
				{
					if (value)
					{
						SetDigitCode();
					}
					else
					{
						Order = 0;
						DigitCode = ZString.Empty;
						CheckDigit = ZString.Empty;
						CheckDigitAlgorithm = ZString.Empty;
					}

					Validation.ValidateInclude();

					if (IsCheckDigitElement)
					{
						ParentConfiguration?.Elements.ForEach(x => ((InvoiceRemittanceCustomisationElement)x).Validation.ValidateCheckDigit());
					}
				}
			}
		}
		ZBool include;

		public ZPropertyInfo IncludeInfo
		{
			get { return GetZPropertyInfo(Schema.Include); }
		}

		void SetDigitCode()
		{
			switch (ElementName)
			{
				case ElementNames.BillerCode:
					DigitCode = ParentConfiguration?.BillerCode.Length.ToString(CultureInfo.InvariantCulture);
					break;
				case ElementNames.BillerAccountNumber:
					DigitCode = ParentConfiguration?.BillerAccountNumber.Length.ToString(CultureInfo.InvariantCulture);
					break;
				case ElementNames.InvoiceNumber:
					DigitCode = InvoiceNumberLength;
					break;
				case ElementNames.DebtorClientNumber:
				case ElementNames.InvoiceTransactionReference:
					DigitCode = FormattedNumberFountainFactory.DefaultFormatDigits.ToString(CultureInfo.InvariantCulture);
					break;
				case ElementNames.DebtorOrganizationCode:
					DigitCode = OrgHeaderSchema.OH_Code.MaxLength.ToString(CultureInfo.InvariantCulture);
					break;
				case ElementNames.InvoiceNumberInNumeric:
					DigitCode = InvoiceNumberInNumericLength;
					break;
				default:
					break;
			}
		}

		#endregion

		#region Digit/Code

		public ZString DigitCode
		{
			get { return digitCode; }
			set
			{
				SetNonPersistentPropertyValue(DigitCodeInfo, ref digitCode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDigitCode();
				}
			}
		}
		ZString digitCode;

		public ZPropertyInfo DigitCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DigitCode); }
		}

		public bool DigitCode_ReadOnly
		{
			get { return !(Include && (IsCustomElement || IsInvoiceAmountElement)); }
		}

		public bool IsCheckDigitElement
		{
			get { return ElementName == ElementNames.CheckDigit1 || ElementName == ElementNames.CheckDigit2; }
		}

		public bool IsInvoiceAmountElement
		{
			get { return ElementName == ElementNames.InvoiceTotalInInvoiceCurrency || ElementName == ElementNames.InvoiceTotalInLocalCurrency; }
		}

		public bool IsCustomElement
		{
			get { return ElementName.StartsWith((NoResString)"Custom Code", StringComparison.OrdinalIgnoreCase); }
		}

		ZString InvoiceNumberLength
		{
			get
			{
				if (invoiceNumberLength == ZString.Empty)
				{
					invoiceNumberLength = ObjectFactory.Get<IAccounting>().InvoiceNumberLength(ParentConfiguration.CurrentFallbackLevel.CompanyPK(false)).ToString(CultureInfo.InvariantCulture);
				}

				return invoiceNumberLength;
			}
		}
		ZString invoiceNumberLength;

		ZString InvoiceNumberInNumericLength
		{
			get
			{
				if (invoiceNumberInNumericLength == ZString.Empty)
				{
					invoiceNumberInNumericLength = ObjectFactory.Get<IAccounting>().InvoiceNumberInNumericLength(ParentConfiguration.CurrentFallbackLevel.CompanyPK(false)).ToString(CultureInfo.InvariantCulture);
				}

				return invoiceNumberInNumericLength;
			}
		}
		ZString invoiceNumberInNumericLength;

		#endregion

		#region CheckDigit

		[List("CheckDigitList")]
		public ZString CheckDigit
		{
			get { return checkDigit; }
			set
			{
				SetNonPersistentPropertyValue(CheckDigitInfo, ref checkDigit, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCheckDigit();
					ParentConfiguration?.Validation.ValidateMaxPossibleLength();
				}
			}
		}
		ZString checkDigit;

		public ZPropertyInfo CheckDigitInfo
		{
			get { return GetZPropertyInfo(Schema.CheckDigit); }
		}

		public bool CheckDigit_ReadOnly
		{
			get { return !Include; }
		}

		public CodeDescriptionPairList CheckDigitList
		{
			get
			{
				if (checkDigitList == null)
				{
					checkDigitList = new CodeDescriptionPairList();
					checkDigitList.AddPair(ElementNames.CheckDigit1);
					checkDigitList.AddPair(ElementNames.CheckDigit2);
				}

				return checkDigitList;
			}
		}
		CodeDescriptionPairList checkDigitList;

		#endregion

		#region CheckDigitAlgorithm

		[List("CheckDigitAlgorithmList")]
		public ZString CheckDigitAlgorithm
		{
			get { return checkDigitAlgorithm; }
			set
			{
				SetNonPersistentPropertyValue(CheckDigitAlgorithmInfo, ref checkDigitAlgorithm, value);

				if (!IsValidationSuspended)
				{
					if (IsCheckDigitElement)
					{
						if (CheckDigitAlgorithm.IsEmpty)
						{
							DigitCode = ZString.Empty;
						}
						else if (CheckDigitAlgorithm == CheckAlgorithm.MOD97)
						{
							DigitCode = "2";
						}
						else
						{
							DigitCode = "1";
						}
					}

					Validation.ValidateCheckDigitAlgorithm();
				}
			}
		}
		ZString checkDigitAlgorithm;

		public ZPropertyInfo CheckDigitAlgorithmInfo
		{
			get { return GetZPropertyInfo(Schema.CheckDigitAlgorithm); }
		}

		public bool CheckDigitAlgorithm_ReadOnly
		{
			get { return !(Include && IsCheckDigitElement); }
		}

		public CodeDescriptionPairList CheckDigitAlgorithmList
		{
			get
			{
				if (checkDigitAlgorithmList == null)
				{
					checkDigitAlgorithmList = new CodeDescriptionPairList();
					checkDigitAlgorithmList.AddPair(CheckAlgorithm.RecursiveMOD10, ResString.GetMultilingualString("85824457-BB06-4BC5-ACF1-7E0FCDC9AB2E", "RECURSIVE MODULE 10"));
					checkDigitAlgorithmList.AddPair(CheckAlgorithm.MOD10, ResString.GetMultilingualString("490B45C9-7616-40CF-9B74-AC1E2D55019F", "MOD10"));
					checkDigitAlgorithmList.AddPair(CheckAlgorithm.MOD97, ResString.GetMultilingualString("79FC3FBC-7AB4-4E94-994A-DF14611ED24E", "MOD97"));
					checkDigitAlgorithmList.AddPair(CheckAlgorithm.Algorithm731, ResString.GetMultilingualString("28F993FC-9202-4C07-9880-CBA219890B56", "7-3-1"));
					checkDigitAlgorithmList.AddPair(CheckAlgorithm.MOD10V05, ResString.GetMultilingualString("AA2D55DB-7037-4856-B3A1-04DE53217E77", "MOD10V05"));
				}

				return checkDigitAlgorithmList;
			}
		}
		CodeDescriptionPairList checkDigitAlgorithmList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Order, Order.ToString());
			writer.WriteElementString(Schema.ElementName, ElementName);
			writer.WriteElementString(Schema.Include, Include.ToString());
			writer.WriteElementString(Schema.DigitCode, DigitCode);
			writer.WriteElementString(Schema.CheckDigit, CheckDigit);
			writer.WriteElementString(Schema.CheckDigitAlgorithm, CheckDigitAlgorithm);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Order = ZByte.ParseSafe(reader.ReadElementString(Schema.Order), 0);
			ElementName = reader.ReadElementString(Schema.ElementName);
			Include = new ZBool(reader.ReadElementString(Schema.Include));
			DigitCode = reader.ReadElementString(Schema.DigitCode);
			CheckDigit = reader.ReadElementString(Schema.CheckDigit);
			CheckDigitAlgorithm = reader.ReadElementString(Schema.CheckDigitAlgorithm);
		}

		#endregion
	}
}
