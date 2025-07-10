using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	public class CusAddInfoWithAutoDelete<T> : CusAddInfo<T> where T : BaseAddInfo
	{
		public CusAddInfoWithAutoDelete(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusAddInfoWithAutoDelete<T> Clone()
		{
			return (CusAddInfoWithAutoDelete<T>)base.Clone();
		}

		public override void OnSaving()
		{
			if (!IsDeleted && IsDataEmpty)
			{
				CusAddInfoTypeAttribute typeAttribute = TypeAttribute;
				if (typeAttribute != null && B7_Type == TypeAttribute.TypeCode)
				{
					Delete();
				}
			}
			base.OnSaving();
		}

		protected virtual bool IsDataEmpty
		{
			get { return B7_AddInfoData.IsEmpty; }
		}
	}
}
