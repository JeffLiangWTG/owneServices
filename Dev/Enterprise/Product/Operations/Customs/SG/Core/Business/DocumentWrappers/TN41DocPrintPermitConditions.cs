using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class TN41DocPrintPermitConditions : DocumentWrapper
	{
		protected TN41DocPrintPermitConditions(TN41PrintPermitConditions conditions, BusinessObjectFactory factoryToWrap)
			: base(conditions, factoryToWrap)
		{
		}

		public static TN41DocPrintPermitConditions New(TN41PrintPermitConditions conditions, BusinessObjectFactory factoryToWrap)
		{
			return conditions != null ? new TN41DocPrintPermitConditions(conditions, factoryToWrap) : null;
		}

		protected ITN41PermitConditions inConditions
		{
			get { return ((TN41PrintPermitConditions)WrappedObject).inConditions; }
		}

		#region ITN41PermitConditions

		public ZString Condition
		{
			get { return inConditions != null ? inConditions.Condition : new ZString(""); }
		}

		#endregion
	}
}
