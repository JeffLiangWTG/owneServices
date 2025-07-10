using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.MasterFiles.Business.Res;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceNumberSequenceCustomisationElement : RegistryBusinessObjectTemplate
	{
		public ComplianceNumberSequenceCustomisationElement()
			: base()
		{
		}

		public ComplianceNumberSequenceCustomisationElement(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory;

		#region Schema

		public abstract class Schema
		{
			public const string ElementName = "ElementName";
			public const string Description = "Description";
			public const string Include = "Include";
			public const string Order = "Order";
			public const string DigitCode = "DigitCode";
			public const string Length = "Length";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded element name, Hard coded element name\\")]
		public static class ElementNames
		{
			public const string TaxStatusCode = "Tax Status Code";
			public const string OriginalAmendmentStatus = "Original/Amendment Status";
			public const string TransactionType = "Invoice/Credit Note/Adjustment Note";
			public const string BlankSpace = "Blank Space";
			public const string SeriesPrefix = "Series Prefix";
			public const string SequenceNumber = "Sequence Number";
			public const string ComplianceDateDayOfIssue = "Compliance Document Date - Day of Issue";
			public const string ComplianceDateMonthOfIssue = "Compliance Document Date - Month of Issue";
			public const string ComplianceDateYearOfIssue = "Compliance Document Date - Year of Issue";
			public const string ComplianceSubType = "Compliance Sub Type";
			public const string PostDateDayOfIssue = "Post Date - Day of Issue";
			public const string PostDateMonthOfIssue = "Post Date  - Month of Issue";
			public const string PostDateYearOfIssue = "Post Date - Year of Issue";
			public const string InvoiceDateDayOfIssue = "Invoice Date - Day of Issue";
			public const string InvoiceDateMonthOfIssue = "Invoice Date - Month of Issue";
			public const string InvoiceDateYearOfIssue = "Invoice Date - Year of Issue";
			public const string CustomElement1 = "Custom Element 1";
			public const string CustomElement2 = "Custom Element 2";
		}

		public static string[] GetAllElementNames()
		{
			return new string[] { ElementNames.TaxStatusCode, ElementNames.OriginalAmendmentStatus,
										ElementNames.TransactionType, ElementNames.BlankSpace,
										ElementNames.SeriesPrefix, ElementNames.SequenceNumber,
										ElementNames.ComplianceDateDayOfIssue, ElementNames.ComplianceDateMonthOfIssue, ElementNames.ComplianceDateYearOfIssue,
										ElementNames.PostDateDayOfIssue, ElementNames.PostDateMonthOfIssue, ElementNames.PostDateYearOfIssue,
										ElementNames.InvoiceDateDayOfIssue, ElementNames.InvoiceDateMonthOfIssue, ElementNames.InvoiceDateYearOfIssue,
										ElementNames.CustomElement1, ElementNames.CustomElement2, ElementNames.ComplianceSubType };
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceNumberSequenceCustomisationElement(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateOrder();
			Validation.ValidateInclude();
			Validation.ValidateDigitCode();
		}

		public ComplianceNumberSequenceCustomisationElementValidation Validation => validation ?? (validation = new ComplianceNumberSequenceCustomisationElementValidation(this));

		ComplianceNumberSequenceCustomisationElementValidation validation;

		public ComplianceNumberSequenceCustomisationElementCollection ParentCollection
		{
			get
			{
				var result = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is ComplianceNumberSequenceCustomisationElementCollection) as ComplianceNumberSequenceCustomisationElementCollection;
				return result ?? new ComplianceNumberSequenceCustomisationElementCollection(Factory);
			}
		}

		public ComplianceNumberSequenceConfiguration ParentConfiguration
		{
			get { return ParentCollection.ParentConfiguration; }
		}

		#region Bound Properties

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

		public bool ElementName_ReadOnly => true;

		#endregion

		#region Description

		public ZString Description
		{
			get
			{
				ZString result;
				switch (ElementName)
				{
					case ElementNames.TaxStatusCode:
						result = Res.GetString("fb99b369-85c9-4017-9b37-030d86a3ee74", "Identifies the type of recipient and the Tax rate applied");
						break;
					case ElementNames.OriginalAmendmentStatus:
						result = Res.GetString("9840dfe6-3a57-40b5-afdd-9206a1e5a09b", "Identifies whether the transaction is original or amending");
						break;
					case ElementNames.TransactionType:
						result = Res.GetString("49a7ea30-b690-4a6f-9b69-e79dca48d256", "Identifies the document type.");
						break;
					case ElementNames.BlankSpace:
						result = Res.GetString("0e50002b-2abf-47a1-8c13-dfb84db345a0", "Add blank space to the compliance number");
						break;
					case ElementNames.SeriesPrefix:
						result = Res.GetString("bcf539d6-0568-41c4-90b4-872c87463b26", "Specific series authorized by the Tax Authority to be printed for the current period");
						break;
					case ElementNames.SequenceNumber:
						result = Res.GetString("c55a08ab-c117-4a34-bb1d-1077400bc8dd", "The incremental unique number of the transaction.  This could be regulated by the government or assigned by the issuer");
						break;
					case ElementNames.ComplianceDateDayOfIssue:
						result = Res.GetString("28eab020-ecce-4bae-bac2-10ae2eb33384", "Day of issue of the document");
						break;
					case ElementNames.ComplianceDateMonthOfIssue:
						result = Res.GetString("33ce2d79-f8b8-40e7-b762-884d00eb63a4", "Month of issue of the document");
						break;
					case ElementNames.ComplianceDateYearOfIssue:
						result = Res.GetString("ded73b0b-bc01-48d5-90ed-12ff2c96b1b2", "Year of issue of the document");
						break;
					case ElementNames.ComplianceSubType:
						result = Res.GetString("d18e6cd3-eb94-43d4-8a8e-0434cccd4586", "Compliance sub type");
						break;
					case ElementNames.InvoiceDateDayOfIssue:
						result = Res.GetString("db7dfb60-4f5a-4687-bc5a-9e8b93dbdbc6", "Day of the invoice date");
						break;
					case ElementNames.InvoiceDateMonthOfIssue:
						result = Res.GetString("0e973752-4879-45fe-a87e-728ac7287361", "Month of the invoice date");
						break;
					case ElementNames.InvoiceDateYearOfIssue:
						result = Res.GetString("0ae9d34b-32d8-4b94-a1a6-f334e51029b6", "Year of the invoice date");
						break;
					case ElementNames.PostDateDayOfIssue:
						result = Res.GetString("58ae3ba0-5f35-4a5a-8494-71b6874f684b", "Day of the post date");
						break;
					case ElementNames.PostDateMonthOfIssue:
						result = Res.GetString("ed35a3da-321e-4a35-bdf4-2bbcfa96ccf2", "Month of the post date");
						break;
					case ElementNames.PostDateYearOfIssue:
						result = Res.GetString("ae314fcf-2fa1-4666-ac7b-ddc354c28efd", "Year of the post date");
						break;
					default:
						result = ZString.Empty;
						break;
				}
				return result;
			}
		}

		#endregion

		#region Include

		public ZBool Include
		{
			get { return include; }
			set
			{
				SetNonPersistentPropertyValue(IncludeInfo, ref include, value);

				if (!value)
				{
					Order = 0;
					DigitCode = ZString.Empty;
				}
				else if (IsYearElement)
				{
					DigitCode = "4";
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateInclude();
				}
			}
		}
		ZBool include;

		public ZPropertyInfo IncludeInfo
		{
			get { return GetZPropertyInfo(Schema.Include); }
		}

		public bool Include_ReadOnly => ElementName == ElementNames.SequenceNumber
			|| ParentConfiguration.IsMandatory;

		#endregion

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

		public bool Order_ReadOnly => !Include || ParentConfiguration.IsMandatory;

		#endregion

		#region Digit/Code

		public ZString DigitCode
		{
			get { return digitCode; }
			set
			{
				SetNonPersistentPropertyValue(DigitCodeInfo, ref digitCode, value);

				if (IsCodePairElement)
				{
					InitializeCodePair();
				}
				else if (IsCustomElement || ElementName == ElementNames.BlankSpace)
				{
					Length = DigitCode.Length;
				}
				else if (IsYearElement)
				{
					Length = ZInt.ParseSafe(DigitCode, 0);
				}

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

		public bool DigitCode_ReadOnly => !(Include && (IsCustomElement || IsCodePairElement || IsYearElement))
			|| ParentConfiguration.IsMandatory;

		#region OriginalAmendmentStatus

		public ZString OriginalStatusCode { get; private set; }
		public ZString AmendmentStatusCode { get; private set; }

		#endregion

		#region TransactionType

		public ZString InvoiceCode { get; private set; }
		public ZString CreditNoteCode { get; private set; }
		public ZString AdjustmentNoteCode { get; private set; }

		#endregion

		void InitializeCodePair()
		{
			var splittedCodes = DigitCode.Split('/');
			if (ElementName == ElementNames.OriginalAmendmentStatus)
			{
				if (splittedCodes.Length == 2 && !string.IsNullOrEmpty(splittedCodes[0]) && !string.IsNullOrEmpty(splittedCodes[1]))
				{
					OriginalStatusCode = splittedCodes[0];
					AmendmentStatusCode = splittedCodes[1];
					Length = splittedCodes.Max(x => x.Length);
				}
				else
				{
					OriginalStatusCode = ZString.Empty;
					AmendmentStatusCode = ZString.Empty;
					Length = 0;
				}
			}
			else if (ElementName == ElementNames.TransactionType)
			{
				if (splittedCodes.Length == 3 &&
					!string.IsNullOrEmpty(splittedCodes[0]) &&
					!string.IsNullOrEmpty(splittedCodes[1]) &&
					!string.IsNullOrEmpty(splittedCodes[2]))
				{
					InvoiceCode = splittedCodes[0];
					CreditNoteCode = splittedCodes[1];
					AdjustmentNoteCode = splittedCodes[2];
					Length = splittedCodes.Max(x => x.Length);
				}
				else
				{
					InvoiceCode = ZString.Empty;
					CreditNoteCode = ZString.Empty;
					AdjustmentNoteCode = ZString.Empty;
					Length = 0;
				}
			}
		}

		#endregion

		#region Length

		[BusinessObjectMaxLengthTestExclude]
		public ZInt Length
		{
			get
			{
				ZInt result;
				switch (ElementName)
				{
					case ElementNames.ComplianceDateDayOfIssue:
					case ElementNames.ComplianceDateMonthOfIssue:
					case ElementNames.InvoiceDateDayOfIssue:
					case ElementNames.InvoiceDateMonthOfIssue:
					case ElementNames.PostDateDayOfIssue:
					case ElementNames.PostDateMonthOfIssue:
						result = 2;
						break;
					default:
						result = length;
						break;
				}
				return result;
			}
			set
			{
				SetNonPersistentPropertyValue(LengthInfo, ref length, value);
			}
		}
		ZInt length;

		public ZPropertyInfo LengthInfo
		{
			get { return GetZPropertyInfo(Schema.Length); }
		}

		public bool Length_ReadOnly => !Include || ElementName != ElementNames.BlankSpace || ParentConfiguration.IsMandatory;

		#endregion

		public ZBool IsCustomElement => ElementName == ElementNames.CustomElement1 || ElementName == ElementNames.CustomElement2;

		public ZBool IsCodePairElement => ElementName == ElementNames.OriginalAmendmentStatus || ElementName == ElementNames.TransactionType;

		public ZBool IsYearElement => ElementName == ElementNames.ComplianceDateYearOfIssue ||
									ElementName == ElementNames.InvoiceDateYearOfIssue ||
									ElementName == ElementNames.PostDateYearOfIssue;

		public ZBool IsTransactionRelatedOnly =>
			ElementName == ElementNames.InvoiceDateDayOfIssue || ElementName == ElementNames.InvoiceDateMonthOfIssue || ElementName == ElementNames.InvoiceDateYearOfIssue
			|| ElementName == ElementNames.PostDateDayOfIssue || ElementName == ElementNames.PostDateMonthOfIssue || ElementName == ElementNames.PostDateYearOfIssue;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ElementName, ElementName);
			writer.WriteElementString(Schema.Include, Include.ToString());
			writer.WriteElementString(Schema.Order, Order.ToString());
			writer.WriteElementString(Schema.DigitCode, DigitCode);
			writer.WriteElementString(Schema.Length, Length.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ElementName = reader.ReadElementString(Schema.ElementName);
			Include = reader.ReadElementStringAsZBool(Schema.Include);
			Order = ZByte.ParseSafe(reader.ReadElementString(Schema.Order), 0);
			DigitCode = reader.ReadElementString(Schema.DigitCode);
			Length = reader.ReadElementStringAsZInt(Schema.Length);
		}

		#endregion
	}
}
