namespace Enterprise.Packing.Business
{
	public interface IPackingParentWithOutturn
	{
		IOutturnProvider GetOutturnProvider(PkgPackage package);
		IContainerView GetContainerView(PkgPackageContainer container);
		int TotalNumberOfPiecesOutturned { get; }
		(string ContainerNumber, PkgPackageContainer Container) GetParentContainer(PkgPackage package);
	}
}
