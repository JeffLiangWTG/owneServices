using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay
{
	public class ComplianceSubTypeDisplay : NonPersistentBusinessObject
	{
		public ComplianceSubTypeDisplay(ZString countryCode, ComplianceSubType complianceSubType) : base(new ReadOnlyBusinessObjectFactory())
		{
			CountryCode = countryCode;
			Code = complianceSubType.Code;
			Description = complianceSubType.Description().ToString();
			LocalDescription = complianceSubType.LocalDescription();
			InternalImplemenationNote = complianceSubType.InternalImplemenationNote();
			LedgerOfUse = complianceSubType.Ledger.ToString();
			TransactionTypeOfUse = complianceSubType.TransactionType.ToString();
			TaxStatusCode = complianceSubType.TaxStatusCode;
		}

		[ResourceStringData("D487E65F-0CB4-455C-BBEC-D5D05295E9E9", Caption = "Country/Region Code")]
		public ZString CountryCode { get; }

		[ResourceStringData("9A001313-2968-4C87-8352-02696C615DCE", Caption = "Code")]
		public ZString Code { get; }

		[ResourceStringData("7C7C8F1C-12E9-4639-A423-993C77A2C63B", Caption = "Description")]
		public ZString Description { get; }

		[ResourceStringData("3665D5DE-DC06-49AE-B54F-BDC3D3248B31", Caption = "Local Description")]
		public ZString LocalDescription { get; }

		[ResourceStringData("AB51AB06-E5B8-4597-A10A-B3B7BEDE7D88", Caption = "Internal Implementation Note")]
		public ZString InternalImplemenationNote { get; }

		[ResourceStringData("949731D3-3F36-445E-A719-48E0C966A891", Caption = "Ledger of Use")]
		public ZString LedgerOfUse { get; }

		[ResourceStringData("a9fb5b33-005e-4e1c-a5f8-74d15b80ecde", Caption = "Transaction Type of Use")]
		public ZString TransactionTypeOfUse { get; }

		[ResourceStringData("F56A07D8-69D4-4C59-A752-EBF617DC8903", Caption = "Tax Status Code")]
		public ZString TaxStatusCode { get; }
	}

	public class ComplianceSubTypeDisplayCollection : NonPersistentBusinessObjectCollection<ComplianceSubTypeDisplay>
	{
		internal void LoadComplianceSubTypes(ZString[] countryCodes)
		{
			RemoveAll();
			foreach (var countryCode in countryCodes)
			{
				var complianceSubTypes = CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(countryCode)?.GetComplianceSubTypes();
				if (complianceSubTypes != null)
				{
					foreach (ComplianceSubType complianceSubType in complianceSubTypes)
					{
						Add(new ComplianceSubTypeDisplay(countryCode, complianceSubType));
					}
				}
			}
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
