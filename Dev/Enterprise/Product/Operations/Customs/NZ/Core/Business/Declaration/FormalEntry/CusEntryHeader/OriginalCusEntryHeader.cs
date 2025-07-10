using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class OriginalCusEntryHeader : CusEntryHeader, Integration.Customs.NZ.IOriginalCusEntryHeader
	{
		public OriginalCusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_MessageType = EntryHeaderTypes.NZ.Original;
			CH_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
		}

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypeList.Codes.OriginalEntry; }
		}

		protected override CusEntryNumber CreateCusEntryNumber()
		{
			CusEntryNumber result = base.CreateCusEntryNumber();
			result.CE_EntryIsSystemGenerated = false;
			return result;
		}
	}
}
