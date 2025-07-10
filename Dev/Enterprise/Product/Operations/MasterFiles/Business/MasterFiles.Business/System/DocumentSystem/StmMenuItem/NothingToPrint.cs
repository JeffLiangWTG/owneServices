using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.MasterFiles.Business
{
	public class NothingToPrint : TitleCopyCountPair
	{
		public NothingToPrint() : base(null, 0)
		{
		}

		public NothingToPrint(string reason) : this()
		{
			this.Reason = reason;
		}

		public readonly string Reason;
	}
}
