using System.Collections.Generic;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	internal class DummyChildType1Merger : FlattenedToUniqueChildrenMerger<DummyHeader, DummyFlattened>
	{
		public DummyChildType1Merger()
			: base(Child1ColumnsOnFlattened)
		{
		}

		static IEnumerable<string> Child1ColumnsOnFlattened
		{
			get
			{
				yield return DummyFlattened.Schema.FlatChild1Int;
				yield return DummyFlattened.Schema.FlatChild1Date;
			}
		}

		protected override bool CreateChildRecord(DummyHeader parent, DummyFlattened record)
		{
			if (record.FlatChild1Int >= 0)
			{
				var child = parent.ChildrenType1.AddNew();
				child.ChildPositiveInt = record.FlatChild1Int;
				child.ChildZDate = record.FlatChild1Date;
				return true;
			}

			return false;
		}

		protected override string ChildHumanReadableNameForPluralCore
		{
			get { return "DummyChildrenType1"; }
		}
	}
}
