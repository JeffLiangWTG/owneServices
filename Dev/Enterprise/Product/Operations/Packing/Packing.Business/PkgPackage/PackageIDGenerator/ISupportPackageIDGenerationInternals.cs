namespace Enterprise.Packing.Business
{
	interface ISupportPackageIDGenerationInternals : ISupportPackageIDGeneration
	{
		void CallAfterIDGenerated();
	}
}
