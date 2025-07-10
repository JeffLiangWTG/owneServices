namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyChildType1Merger_ThatSetsFlatChild1IntToZero : DummyChildType1Merger
	{
		protected override bool CreateChildRecord(DummyHeader parent, DummyFlattened record)
		{
			record.FlatChild1Int = 0;
			return base.CreateChildRecord(parent, record);
		}
	}
}
