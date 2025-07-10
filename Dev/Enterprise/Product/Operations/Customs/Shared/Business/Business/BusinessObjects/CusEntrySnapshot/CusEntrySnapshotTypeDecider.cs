using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntrySnapshotTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusEntrySnapshot>(); })
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEntrySnapshot);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetDeclarationCountryCode(row, factory));
		}

		protected ZString GetDeclarationCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var entryHeaderPK = (row != null) ? new ZGuid(row[CusEntrySnapshot.Schema.CES_CH_EntryHeader]) : ZGuid.Invalid;
			var entryHeader = entryHeaderPK.IsValid ? factory.Load<CusEntryHeader>(entryHeaderPK) : null;
			return (entryHeader != null) ? entryHeader.Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
