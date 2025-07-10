namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderValidation : AutoNZCusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader entry)
			: base(entry)
		{
		}

		protected override void CheckCH_BGMReference()
		{
			//should not validate at all
			//I know this looks nuts, but NZ never creates nor deletes an entry header 
			//it doesn't need or require.
		}
	}
}
