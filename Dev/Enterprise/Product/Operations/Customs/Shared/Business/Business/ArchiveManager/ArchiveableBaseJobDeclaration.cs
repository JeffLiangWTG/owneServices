namespace Enterprise.Customs.Business.ArchiveManager
{
	public class ArchiveableBaseJobDeclaration : ArchiveableJobDeclaration<Integration.Customs.IBaseJobDeclaration, BaseJobDeclaration>
	{
		public ArchiveableBaseJobDeclaration(Integration.Customs.IBaseJobDeclaration declaration)
			: base(declaration)
		{
		}
	}
}
