
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business
{
	public class LineOtherInfoCollection : OtherInfoCollection
	{
		public LineOtherInfoCollection(BusinessObjectFactory factory, ZPropertyInfo addInfoPropertyInfo) : base(factory, addInfoPropertyInfo) { }

		public new LineOtherInfo this[int index]
		{
			get { return (LineOtherInfo)base[index]; }
		}

		public new LineOtherInfo AddNew()
		{
			return (LineOtherInfo)base.AddNew();
		}

		protected override CodeDataPair CreateNewCodeInfo()
		{
			return new LineOtherInfo(Factory, this);
		}
	}
}
