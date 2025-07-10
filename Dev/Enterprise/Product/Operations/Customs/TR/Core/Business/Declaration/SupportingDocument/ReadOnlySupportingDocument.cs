using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ReadOnlySupportingDocument : EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument
	{
		public ReadOnlySupportingDocument(SupportingDocument supportingDocument)
			: base(supportingDocument)
		{
		}

		public ZString CSI_StatusDescription => StatusList.GetDescriptionFromCode(CSI_Status);

		CodeDescriptionPairList StatusList
		{
			get
			{
				if (statusList == null)
				{
					statusList = Factory.GetCachedValue<SupportingDocumentAvailabilityList>();
				}

				return statusList;
			}
		}
		CodeDescriptionPairList statusList;
	}
}
