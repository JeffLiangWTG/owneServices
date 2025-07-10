using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryHeader == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
			}
		}

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(CusEntryHeader.Declaration, Factory); }
		}

		CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		public DocCusEntryLineCollection EntryLines
		{
			get
			{
				if (fEntryLines == null)
				{
					fEntryLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
					fEntryLines.Sort("LineNumber", System.ComponentModel.ListSortDirection.Ascending);
				}
				return fEntryLines;
			}
		}
		DocCusEntryLineCollection fEntryLines;
	}
}
