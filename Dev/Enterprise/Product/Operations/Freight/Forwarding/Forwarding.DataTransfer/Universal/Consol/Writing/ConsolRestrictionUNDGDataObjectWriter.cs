using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolRestrictionUNDGDataObjectWriter : DataObjectWriter<ConsolDGRestrictions, UNDG>
	{
		public ConsolRestrictionUNDGDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{ }

		protected override UNDG PopulateDataObject(ConsolDGRestrictions sourceBO)
		{
			return new UNDG(writeManager.WriterStrategy)
			{
				IMOClass = sourceBO.JKD_Class,
				UNDGCode = sourceBO.JKD_Calc_Substance
			};
		}
	}
}
