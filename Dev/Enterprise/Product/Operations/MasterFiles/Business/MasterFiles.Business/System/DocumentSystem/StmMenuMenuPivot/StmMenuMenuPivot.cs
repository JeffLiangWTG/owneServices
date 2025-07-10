using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("MenuName"), DescriptionProperty("DocumentIndex")]
	public class StmMenuMenuPivot : AutoStmMenuMenuPivot
	{
		public StmMenuMenuPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString MenuName
		{
			get
			{
				var menu = Outward;
				return menu != null ? menu.SU_MenuName : ZString.Empty;
			}
		}

		public ZString DocumentIndex
		{
			get
			{
				return ResString.GetMultilingualString("50430077-b1c4-411e-85ee-7540315b9744", "Document index: {0}", SF_Index);
			}
		}

		#region Properties

		#region SF_Filter
		[BusinessObjectTestExclude]
		public override ZString SF_Filter
		{
			get { return base.SF_Filter; }
			set { base.SF_Filter = value; }
		}
		#endregion

		#endregion
	}
}
