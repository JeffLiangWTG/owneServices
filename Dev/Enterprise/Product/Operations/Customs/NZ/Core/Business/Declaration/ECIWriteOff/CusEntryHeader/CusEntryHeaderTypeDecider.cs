using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class CusEntryHeaderTypeDecider : TypeDecider
	{
		public CusEntryHeaderTypeDecider()
		{
		}

		public override Type GetTypeForNew()
		{
			return typeof(CusEntryHeader);
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			string entryType = row[CusEntryHeaderSchema.CH_MessageType.Name].ToString();
			switch (entryType)
			{
				case CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest:
					result = typeof(Manifesting.CusEntryHeader);
					break;
				default:
					result = typeof(CusEntryHeader);
					break;
			}

			return result;
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeForNew();
		}
	}
}
