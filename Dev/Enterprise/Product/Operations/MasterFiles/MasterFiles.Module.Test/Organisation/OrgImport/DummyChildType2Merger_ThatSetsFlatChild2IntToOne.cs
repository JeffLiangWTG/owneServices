namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyChildType2Merger_ThatSetsFlatChild2IntToOne : DummyChildType2Merger
	{
		protected override bool CreateChildRecord(DummyHeader parent, DummyFlattened record)
		{
			record.FlatChild2Int = 1;
			return base.CreateChildRecord(parent, record);
		}
	}
}
