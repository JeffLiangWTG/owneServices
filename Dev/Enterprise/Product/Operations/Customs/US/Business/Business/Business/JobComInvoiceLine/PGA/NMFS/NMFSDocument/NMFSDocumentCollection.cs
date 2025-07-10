using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class NMFSDocumentCollection : CusCodeDataCollection<NMFSDocument>
	{
		public NMFSDocumentCollection(NMFSLine master)
			: base(master, CusCodeDataTypeList.Codes.NMFSDocument)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var nmfsLine = Master as NMFSLine;
				return nmfsLine != null && !nmfsLine.Is370ProgramType && !nmfsLine.IsSIMPOrCOAProgramType;
			}
		}
	}
}
