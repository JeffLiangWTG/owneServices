using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class InBondDataObjectWriterHelper : DataTransfer.Universal.InBondDataObjectWriterHelper
	{
		public InBondDataObjectWriterHelper(CusInBondHeader header)
			: base(header)
		{
		}

		protected new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		#region List

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public Freight.Common.Business.BindToLists BindToLists
		{
			get { return Freight.Common.Business.BindToLists.GetCachedLists(factory); }
		}

		#endregion

		public ICustomLabelsProvider GetCusInBondCargoDescCustomLabelsProvider()
		{
			var header = Header;
			var pk = header == null ? ZGuid.Empty : header.PK;
			var key = "CusInBondCargoDescCustomLabelsProvider" + pk.ToStringKey();
			return factory.GetCachedValue(key, () =>
			{
				ICustomLabelsProvider result = null;
				if (header != null)
				{
					result = new CusInBondCargoDescCustomLabelsProvider(header);
				}
				return result;
			});
		}
	}
}
