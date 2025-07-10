using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolStmNoteCollectionView : StmNoteCollectionView
	{
		public ForwardingConsolStmNoteCollectionView(ForwardingConsol consol)
			: base(consol, typeof(ForwardingConsolStmNote))
		{
		}
	}
}
