using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListDocManagerInfo : DocManagerInfo
	{
		public HVLVOriginLoadListDocManagerInfo(HVLVOriginLoadList loadList)
			: base(loadList, Constants.DocManagerCodes.HVLVOriginLoadList)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());
			var loadList = (HVLVOriginLoadList)BusinessEntity;
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(loadList));
			return result.ToArray();
		}
	}
}
