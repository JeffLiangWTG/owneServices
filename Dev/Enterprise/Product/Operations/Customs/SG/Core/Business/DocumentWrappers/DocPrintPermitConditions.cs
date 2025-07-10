using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocPrintPermitConditions : DocumentWrapper
	{
		protected DocPrintPermitConditions(PrintPermitConditions conditions, BusinessObjectFactory factoryToWrap)
			: base(conditions, factoryToWrap)
		{
		}

		public static DocPrintPermitConditions New(PrintPermitConditions conditions, BusinessObjectFactory factoryToWrap)
		{
			return conditions != null ? new DocPrintPermitConditions(conditions, factoryToWrap) : null;
		}

		protected ICConditions inConditions
		{
			get { return ((PrintPermitConditions)WrappedObject).inConditions; }
		}

		#region ICConditions

		public ZString Code
		{
			get { return inConditions != null ? inConditions.Code : new ZString(""); }
		}

		public ZString Message
		{
			get { return inConditions != null ? inConditions.Message : new ZString(""); }
		}

		#endregion
	}

	public class DocPrintPermitConditionsCollection : DocumentWrapperCollection
	{
		public DocPrintPermitConditionsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPrintPermitConditions this[int index]
		{
			get { return (DocPrintPermitConditions)base[index]; }
		}
	}

	public class TN41DocPrintPermitConditionsCollection : DocumentWrapperCollection
	{
		public TN41DocPrintPermitConditionsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new TN41DocPrintPermitConditions this[int index]
		{
			get { return (TN41DocPrintPermitConditions)base[index]; }
		}
	}
}
