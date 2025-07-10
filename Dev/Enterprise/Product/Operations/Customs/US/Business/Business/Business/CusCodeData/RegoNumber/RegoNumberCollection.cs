namespace Enterprise.Customs.US.Business
{
	public class RegoNumberCollection : Customs.Business.CusCodeDataCollection<RegoNumber>
	{
		public RegoNumberCollection(AIILine parent)
			: base(parent, CusCodeDataTypeList.Codes.RegoNumber)
		{
		}
	}
}
