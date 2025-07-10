using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoDeliveryOrderHazmat : Customs.Business.MultiLineAddInfos.CusAddInfoWithAutoDelete<USDeliveryOrderHazmatAddInfo>
	{
		public virtual UNDGSubstance Hazmat
		{
			get
			{
				var unno = US_UNNumber.SubstringSafe(0, 4);
				var variant = US_UNNumber.SubstringSafe(4, 2);
				var standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

				return UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).FirstOrDefault();
			}
		}
	}
}
