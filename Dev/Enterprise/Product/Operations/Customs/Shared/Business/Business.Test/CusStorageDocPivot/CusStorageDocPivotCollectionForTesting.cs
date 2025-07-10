namespace Enterprise.Customs.Business.Testing
{
	sealed class CusStorageDocPivotCollectionForTesting : CusStorageDocPivotCollection<CusStorageDocPivotForTest, CusEntryInstructionAsTypeSupporter>
	{
		public CusStorageDocPivotCollectionForTesting(CusEntryInstructionAsTypeSupporter master) : base(master) { }
	}
}
