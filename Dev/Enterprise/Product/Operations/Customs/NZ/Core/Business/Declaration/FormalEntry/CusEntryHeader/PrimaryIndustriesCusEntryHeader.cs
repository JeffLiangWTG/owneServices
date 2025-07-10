using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class PrimaryIndustriesCusEntryHeader : CusEntryHeader, Integration.Customs.NZ.IPrimaryIndustriesCusEntryHeader
	{
		public PrimaryIndustriesCusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_MessageType = EntryHeaderTypes.NZ.PrimaryIndustries;
		}

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypeList.Codes.PrimaryIndustriesEntry; }
		}
	}
}
