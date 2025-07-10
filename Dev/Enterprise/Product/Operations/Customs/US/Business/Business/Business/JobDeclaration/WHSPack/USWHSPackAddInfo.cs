using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USWHSPack)]
	public class USWHSPackAddInfo : AutoUSWHSPackAddInfo
	{
		public USWHSPackAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Parent; }
		}

		public new WHSPack Parent
		{
			get { return (WHSPack)base.Parent; }
			protected set { base.Parent = value; }
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[]
				{
					USWHSPackAddInfoSchema.US_PackageReference
				};
			}
		}
	}
}
