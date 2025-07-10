using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderTypeDecider : TypeDecider
	{
		public CusEntryHeaderTypeDecider()
		{
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			string entryType = row[CusEntryHeaderSchema.CH_MessageType.Name].ToString();
			switch (entryType)
			{
				case CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOff:
					result = typeof(ECIWriteOff.CusEntryHeader);
					break;
				case CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest:
					result = typeof(ECIWriteOff.Manifesting.CusEntryHeader);
					break;
				case CusEntryHeader.EntryHeaderTypes.NZ.Completion:
					result = typeof(FormalEntry.CompletionCusEntryHeader);
					break;
				case CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries:
					result = typeof(FormalEntry.PrimaryIndustriesCusEntryHeader);
					break;
				case CusEntryHeader.EntryHeaderTypes.NZ.Original:
					result = typeof(FormalEntry.OriginalCusEntryHeader);
					break;
				default:
					result = typeof(FormalEntry.CusEntryHeader);
					break;
			}

			return result;
		}

		public override Type GetTypeForNew()
		{
			return typeof(FormalEntry.CusEntryHeader);
		}
	}
}
