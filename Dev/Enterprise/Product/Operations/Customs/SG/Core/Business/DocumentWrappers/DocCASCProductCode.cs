using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocCASCProductCode : DocumentWrapper
	{
		protected DocCASCProductCode(ICASCProductCode productCode, BusinessObjectFactory factoryToWrap)
			: base(productCode, factoryToWrap)
		{
			this.productCode = productCode;
		}

		readonly ICASCProductCode productCode;

		public static DocCASCProductCode New(ICASCProductCode productCode, BusinessObjectFactory factoryToWrap)
		{
			return productCode != null ? new DocCASCProductCode(productCode, factoryToWrap) : null;
		}

		public ZString SequenceNumber => productCode.SequenceNumber;

		public ZString ProductCode => productCode.ProductCode;

		public ZString ProductQuantity => productCode.ProductQuantity;
	}

	public class DocCASCProductCodeCollection : DocumentWrapperCollection
	{
		public DocCASCProductCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCASCProductCode this[int index]
		{
			get { return (DocCASCProductCode)base[index]; }
		}
	}
}
