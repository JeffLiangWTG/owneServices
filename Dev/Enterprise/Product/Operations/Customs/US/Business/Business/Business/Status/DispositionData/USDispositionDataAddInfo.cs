using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDisposition)]
	public class USDispositionDataAddInfo : AutoUSDispositionDataAddInfo
	{
		public USDispositionDataAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new DispositionData Parent
		{
			get { return (DispositionData)base.Parent; }
			protected set { base.Parent = value; }
		}

		public override ZBool US_IsInactive
		{
			get { return base.US_IsInactive; }
			set
			{
				var oldValue = US_IsInactive;
				base.US_IsInactive = value;
				if (!IsCopying && oldValue != US_IsInactive && US_IsInactive)
				{
					foreach (SchemaColumn column in Parent.GetColumnsForFastSearch())
					{
						IZType addInfoValue = (IZType)this[column];
						Parent.SetSystemDefinedValue(column.Name, addInfoValue.Default);
					}
				}
			}
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get { return (!US_IsInactive && Parent != null) ? Parent.GetColumnsForFastSearch() : base.ColumnsForFastSearch; }
		}
	}
}
