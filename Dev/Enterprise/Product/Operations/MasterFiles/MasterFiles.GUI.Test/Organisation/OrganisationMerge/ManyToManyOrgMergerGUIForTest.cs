using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ManyToManyOrgMergerGUIForTest : ManyToManyOrgMergerGUI
	{
		public ManyToManyOrgMergerGUIForTest(ZQuery exportQuery)
			: base(exportQuery)
		{ }

		public bool ReturnErrorForMergeSelectedOrgsCore { get; set; }
		public bool Cancel { get; set; }
		protected override List<string> MergeSelectedOrgsCore(FilteredBusinessObjectReader reader, ManyToManyOrgMerger merger)
		{
			List<string> result = merger.Merge();
			if (Cancel)
			{
				BulkMergeProgressForm.CancelButton.PerformClick();
			}
			if (ReturnErrorForMergeSelectedOrgsCore)
			{
				result.Add("hello");
			}
			return result;
		}

		public ZQuery ExportQuery_Exposed
		{
			get
			{
				return ExportQuery;
			}
		}
	}
}
