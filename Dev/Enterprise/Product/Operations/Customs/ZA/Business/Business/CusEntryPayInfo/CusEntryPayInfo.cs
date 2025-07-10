using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(CusEntryPayInfo.Schema.LRNumber)]
	[DescriptionProperty(CusEntryPayInfo.Schema.MRNumber)]
	public class CusEntryPayInfo : Customs.Business.CusEntryPayInfo, Integration.Customs.ZA.ICusEntryPayInfo
	{
		public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : Customs.Business.AutoCusEntryPayInfo.Schema
		{
			public const string JobNumber = "JobNumber";
			public const string CustomsOffice = "CustomsOffice";
			public const string FANumber = "FANumber";
			public const string LRNumber = "LRNumber";
			public const string MRNumber = "MRNumber";
			public const string TotalVat = "TotalVat";
		}

		public new static readonly CusEntryPayInfoTypeDecider TypeDecider = new CusEntryPayInfoTypeDecider();

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("4CAD4603-1987-4C5F-9EBF-56D5BC13DA8B", "Proof of Payment {0}", LRNumber);

		public JobDeclaration Declaration => EntryHeader?.Declaration;

		public new CusEntryHeader EntryHeader { get { return (CusEntryHeader)base.EntryHeader; } }

		#region Validation

		public new CusEntryPayInfoValidation Validation
		{
			get { return (CusEntryPayInfoValidation)base.Validation; }
		}

		protected override Customs.Business.CusEntryPayInfoValidation GetNewValidation()
		{
			return new CusEntryPayInfoValidation(this);
		}

		#endregion

		#region Lookups

		public new CusEntryPayInfoLookups Lookups => (CusEntryPayInfoLookups)base.Lookups;

		protected override Customs.Business.CusEntryPayInfoLookups GetNewLookups()
		{
			return new CusEntryPayInfoLookups(this);
		}

		#endregion

		#region Properties for VAT 404 – Proof of Payment

		[DecimalPlaces(2)]
		public override ZDecimal C9_PaymentAmount
		{
			get { return base.C9_PaymentAmount; }
			set { base.C9_PaymentAmount = value; }
		}

		public ZString JobNumber => Declaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZPropertyInfo JobNumberInfo => GetZPropertyInfo(Schema.JobNumber);

		public ZString CustomsOffice => EntryHeader?.CustomsOffice ?? ZString.Empty;

		public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(Schema.CustomsOffice);

		public ZString FANumber => EntryHeader?.GetFinancialAccountNumber() ?? ZString.Empty;

		public ZPropertyInfo FANumberInfo => GetZPropertyInfo(Schema.FANumber);

		public OrgHeader Importer => Declaration?.Importer ?? OrgHeader.DefaultOrg;

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryPayInfo|LRNumber", Caption = "LRN", FullDescription = "Local Reference Number")]
		public ZString LRNumber => EntryHeader?.CH_BGMReference ?? ZString.Empty;

		public ZPropertyInfo LRNumberInfo => GetZPropertyInfo(Schema.LRNumber);

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryPayInfo|MRNumber", Caption = "MRN", FullDescription = "Movement Reference Number")]
		public ZString MRNumber => EntryHeader?.MovementReferenceNumber ?? ZString.Empty;

		public ZPropertyInfo MRNumberInfo => GetZPropertyInfo(Schema.MRNumber);

		[DecimalPlaces(2)]
		public ZDecimal TotalVat
		{
			get
			{
				if (C9_PaymentReference.IsEmpty)
				{
					return ZDecimal.Zero;
				}

				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_CusEntryPayInfo_TotalVat", C9_PaymentReference);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = ZDecimal.Zero;
					ZQuery query = new ZQuery(CusEntryPayInfoSchema.C9_TransactionType, UniversalReferenceConstants.TaxOrFeeTypeCode.VAT);
					query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentReference, C9_PaymentReference);
					foreach (var item in Factory.Load<CusEntryPayInfo>(query))
					{
						result += item.C9_PaymentAmount;
					}
					return result;
				});
			}
		}

		public ZPropertyInfo TotalVatInfo => GetZPropertyInfo(Schema.TotalVat);

		public bool C9_PaymentDate_ReadOnly
		{
			get { return true; }
		}

		public bool C9_PaymentAmount_ReadOnly
		{
			get { return true; }
		}

		public ZString AgentsReference => Declaration?.JE_AgentsReference ?? ZString.Empty;

		#endregion

		public ZString RelatedARInvoiceNumbers
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var accInvoiceHeader in EntryHeader?.RelatedARInvoices ?? System.Array.Empty<AccTransactionHeader>())
				{
					result.AppendIfNotEmpty(accInvoiceHeader.AH_TransactionNum);
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public static bool DoesDbContainConflictingRecords(BusinessObjectFactory factory, ZString receiptNumber, ZDate receiptDate, List<CusEntryPayInfo> excludedPayInfos)
		{
			var query = new ZQuery(CusEntryPayInfoSchema.C9_PaymentReference, receiptNumber);
			query.AddToFilter(CusEntryPayInfoSchema.C9_TransactionType, UniversalReferenceConstants.TaxOrFeeTypeCode.VAT);
			if (receiptDate.IsValid)
			{
				query.AddToFilter(CusEntryPayInfoSchema.C9_ReceiptDate, SQLComparisonOperator.NotEqual, receiptDate);
			}
			else
			{
				query.AddToFilter(CusEntryPayInfoSchema.C9_ReceiptDate, SQLComparisonOperator.NotEqual, null);
			}
			query.AddToFilter(CusEntryPayInfoSchema.PK, SQLComparisonOperator.NotEqual, (from CusEntryPayInfo c in excludedPayInfos select c.PK));
			return factory.LoadTop1<CusEntryPayInfo>(query) != null;
		}
	}
}
