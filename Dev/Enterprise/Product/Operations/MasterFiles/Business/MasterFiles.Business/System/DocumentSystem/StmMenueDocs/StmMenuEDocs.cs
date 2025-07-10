using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuEDocs : AutoStmMenuEDocs
	{
		public StmMenuEDocs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CodeDescriptionPairList PrintCopyTypeList
		{
			get
			{
				if (printCopyTypeList == null)
				{
					printCopyTypeList = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
					printCopyTypeList.AddPair(nameof(PrintCopyType.ALL), ResString.GetMultilingualString("ababed01-82ac-4d16-bc40-a623990087f9", "ALL"));
				}

				return printCopyTypeList;
			}
		}
		CodeDescriptionPairList printCopyTypeList;
	}
}
