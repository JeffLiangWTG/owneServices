
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEACCaseQuery : AutoACEACCaseQuery, IACEACCaseQueryInput
	{
		public ACEACCaseQuery(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[List(nameof(Lookups) + "." + nameof(ACEACCaseQueryLookups.CompanyCaseStatusList))]
		public override ZString US_CompanyCaseStatus
		{
			get { return base.US_CompanyCaseStatus; }
			set { base.US_CompanyCaseStatus = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ACEACCaseQueryLookups.CountryList))]
		public override ZString US_CountryCode
		{
			get { return base.US_CountryCode; }
			set { base.US_CountryCode = value; }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(ACEACCaseQueryLookups.TariffList))]
		public override ZString US_HTSNumber
		{
			get { return new TariffFormatter().DisplayFormat(base.US_HTSNumber); }
			set { base.US_HTSNumber = new TariffFormatter().Format(value); }
		}

		public ACEACCaseQueryLookups Lookups
		{
			get { return lookups ?? (lookups = new ACEACCaseQueryLookups(this)); }
		}
		ACEACCaseQueryLookups lookups;

		ZString IACEACCaseQueryInput.CaseStatus
		{
			get { return US_CompanyCaseStatus; }
		}

		ZString IACEACCaseQueryInput.CountryCode
		{
			get { return US_CountryCode; }
		}

		ZString IACEACCaseQueryInput.HTSNumber
		{
			get { return US_HTSNumber; }
		}

		ZString IACEACCaseQueryInput.TSUSANumber
		{
			get { return US_TSUSA; }
		}

		ZString IACEACCaseQueryInput.ManufacturerMID
		{
			get { return US_ManufacturerMID; }
		}

		ZString IACEACCaseQueryInput.ForeignExporterMID
		{
			get { return US_ForeignShipperMID; }
		}

		ZDate IACEACCaseQueryInput.DateSinceLastUpdate
		{
			get { return US_DateSinceLastUpdate.Date; }
		}

		[ChildEditable(true)]
		public ACECaseNumberForQueryCollection CaseNumbers
		{
			get
			{
				if (caseNumbers == null)
				{
					caseNumbers = new ACECaseNumberForQueryCollection();
					RegisterEditableChildObject(caseNumbers);
				}
				return caseNumbers;
			}
		}
		ACECaseNumberForQueryCollection caseNumbers;

		IEnumerable<ZString> IACEACCaseQueryInput.CaseNumbers
		{
			get
			{
				foreach (ACECaseNumberForQuery caseNumber in CaseNumbers)
				{
					yield return caseNumber.CaseNumber;
				}
			}
		}
	}
}
