using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryLine : EU.Business.Declaration.CusEntryLine, ICusEntryCPDecParent, Integration.Customs.TR.ICusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ReadOnlySupportingDocumentCollection ReadOnlySupportingDocuments => (ReadOnlySupportingDocumentCollection)base.ReadOnlySupportingDocuments;

		[ChildEditable(true)]
		public CusEntryCPDecCollection CPDecCollection
		{
			get
			{
				if (cpDecs == null)
				{
					cpDecs = new CusEntryCPDecCollection(this);
					cpDecs.Load();
					RegisterEditableChildObject(cpDecs);
				}
				return cpDecs;
			}
		}
		CusEntryCPDecCollection cpDecs;

		ZString ICusEntryCPDecParent.Prefix => CusEntryLineSchema.Constants.Prefix;

		protected override EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocumentCollection GetNewReadOnlySupportingDocuments() => new ReadOnlySupportingDocumentCollection(this);

		protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
	}
}
