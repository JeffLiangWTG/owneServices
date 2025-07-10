namespace Enterprise.Customs.Business
{
	public class CusGuaranteeReferenceNumberCollection<T> : CusGuaranteeReferenceNumberCollection where T : CusGuaranteeReferenceNumber
	{
		public CusGuaranteeReferenceNumberCollection(BaseCusGuaranteeHeader master) : base(master)
		{
		}

		public new T AddNew() => (T)base.AddNew();

		public new T this[int index] => (T)base[index];
	}

	public abstract class CusGuaranteeReferenceNumberCollection : CusCodeDataCollection<CusGuaranteeReferenceNumber>
	{
		public CusGuaranteeReferenceNumberCollection(BaseCusGuaranteeHeader master)
			: base(master, GuaranteeCusCodeDataTypeList.Codes.GRN)
		{
		}
	}
}
