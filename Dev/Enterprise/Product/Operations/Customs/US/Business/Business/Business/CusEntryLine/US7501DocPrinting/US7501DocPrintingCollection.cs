using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class US7501DocPrintingCollection : DependentCusAddInfoCollection<US7501DocPrinting, CusEntryHeader>
	{
		public US7501DocPrintingCollection(CusEntryHeader header)
			: base(header, CusAddInfoTypeAttribute.Codes.US7501DocPrinting)
		{
		}
	}
}
