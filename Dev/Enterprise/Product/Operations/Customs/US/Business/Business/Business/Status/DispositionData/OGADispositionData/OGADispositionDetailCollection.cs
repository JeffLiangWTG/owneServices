using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class OGADispositionDetailCollection : DependentCusAddInfoCollection<OGADispositionDetail, BusinessObject>
	{
		public OGADispositionDetailCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USOGADispositionDetail)
		{
		}

		public OGADispositionDetail AddNewIfNotExist(ZString referenceIDQualifier, ZString referenceID, ZDateTime refReceiptDate)
		{
			OGADispositionDetail result = null;

			foreach (OGADispositionDetail dispositionDetail in this)
			{
				if (dispositionDetail.US_ReferenceIDQualifier == referenceIDQualifier &&
					dispositionDetail.US_ReferenceID == referenceID &&
					dispositionDetail.US_ReceiptDateTime == refReceiptDate)
				{
					result = dispositionDetail;
					break;
				}
			}

			if (result == null)
			{
				result = AddNew();
				result.US_ReferenceIDQualifier = referenceIDQualifier;
				result.US_ReferenceID = referenceID;
				result.US_ReceiptDateTime = refReceiptDate;
			}
			return result;
		}

		public void AddNewDispositionDetail(IPGADispositionDetailProvider dispDetailProvider)
		{
			OGADispositionDetail dispDetail = AddNewIfNotExist(dispDetailProvider.ReferenceIDQualifier, dispDetailProvider.ReferenceID, dispDetailProvider.ReceiptDateTime);
			dispDetail.SubReasonCodes = dispDetailProvider.SubReasonCodes;
		}
	}
}
