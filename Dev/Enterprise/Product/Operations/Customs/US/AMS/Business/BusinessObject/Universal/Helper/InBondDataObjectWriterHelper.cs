namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class InBondDataObjectWriterHelper : DataTransfer.Universal.InBondDataObjectWriterHelper
	{
		public InBondDataObjectWriterHelper(CusInBondHeader header)
			: base(header)
		{
		}

		public DirectionTypeList DirectionTypes
		{
			get { return factory.GetCachedValue<DirectionTypeList>(); }
		}
	}
}
