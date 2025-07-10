using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USITNumber)]
	public class USITNumberAddInfo : AutoUSITNumberAddInfo
	{
		public USITNumberAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get { return new SchemaColumn[] { USITNumberAddInfoSchema.US_ITNumber }; }
		}
	}
}
