using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class IACEPA : CertificateOfOriginDocDataObject<IACEPALineItem>
	{
		public IACEPA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region isExhibition

		public ZBool IsExhibition
		{
			get => isExhibition;
			set
			{
				if (SetNonPersistentPropertyValue(IsExhibitionInfo, ref isExhibition, value))
				{
					Validate(IsExhibitionInfo);
				}
			}
		}

		ZBool isExhibition;

		public ZPropertyInfo IsExhibitionInfo => GetZPropertyInfo(nameof(IsExhibition));

		#endregion

		#region ExhibitionDetail

		public ZString ExhibitionDetail
		{
			get => exhibitionDetail;
			set
			{
				if (SetNonPersistentPropertyValue(ExhibitionDetailInfo, ref exhibitionDetail, value))
				{
					Validate(ExhibitionDetailInfo);
				}
			}
		}

		ZString exhibitionDetail;

		public ZPropertyInfo ExhibitionDetailInfo => GetZPropertyInfo(nameof(ExhibitionDetail));

		#endregion
	}
}
