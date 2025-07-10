using System.Collections.Generic;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	internal class DummyChildType2Merger : FlattenedToUniqueChildrenMerger<DummyHeader, DummyFlattened>
	{
		public DummyChildType2Merger()
			: base(Child2ColumnsOnFlattened)
		{
		}

		static IEnumerable<string> Child2ColumnsOnFlattened
		{
			get
			{
				yield return DummyFlattened.Schema.FlatChild2Int;
				yield return DummyFlattened.Schema.FlatChild2Date;
			}
		}

		protected override bool CreateChildRecord(DummyHeader parent, DummyFlattened record)
		{
			if (record.FlatChild1Int >= 0)
			{
				var child = parent.ChildrenType2.AddNew();
				child.ChildPositiveInt = record.FlatChild2Int;
				child.ChildZDate = record.FlatChild2Date;
				return true;
			}

			return false;
		}

		protected override string ChildHumanReadableNameForPluralCore
		{
			get { return "DummyChildrenType2"; }
		}
	}
}
