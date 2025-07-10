namespace Enterprise.Packing.Business
{
	interface IPackingHasChanges
	{
		bool HasChangesThatAreInvalidIfFinalised { get; }
	}
}
