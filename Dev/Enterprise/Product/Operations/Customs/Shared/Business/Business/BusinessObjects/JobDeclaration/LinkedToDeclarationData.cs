namespace Enterprise.Customs.Business
{
	public class LinkedToDeclarationData
	{
		public LinkedToDeclarationData(BaseJobDeclaration declaration)
		{
			IsImport = declaration.IsImport;
			Declaration = declaration;
		}

		public BaseJobDeclaration Declaration { get; private set; }

		public bool IsImport { get; private set; }
	}
}
