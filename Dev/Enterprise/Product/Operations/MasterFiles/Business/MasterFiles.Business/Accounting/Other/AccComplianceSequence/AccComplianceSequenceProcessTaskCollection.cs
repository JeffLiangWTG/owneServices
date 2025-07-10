namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceProcessTaskCollection : ProcessTaskCollection
	{
		public AccComplianceSequenceProcessTaskCollection(AccComplianceSequence sequence) : base(sequence) { }

		public new AccComplianceSequenceProcessTask this[int index]
		{
			get { return (AccComplianceSequenceProcessTask)Elements[index]; }
		}

		public new AccComplianceSequenceProcessTask AddNew()
		{
			return (AccComplianceSequenceProcessTask)base.AddNew();
		}

		public new AccComplianceSequence Parent
		{
			get { return (AccComplianceSequence)base.Parent; }
		}
	}
}
