using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header.UniversalCopyIgnoreElement;

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalCopyIgnoreElement(UNDGSubstancePivots)]
	[UniversalCopyWithExtendedEntities]
	public class UNDGDataItem : MasterFiles.Business.UNDGDataItem
	{
		public UNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
