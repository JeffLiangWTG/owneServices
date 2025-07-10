namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	interface ITestEmptyCodeErrorMessage
	{
		string ExpectedEmptyCodeErrorMessage { get; }
	}

	interface ITestEmptyDescriptionErrorMessage
	{
		string ExpectedEmptyDescriptionErrorMessage { get; }
	}

	interface ITestInvalidDateErrorMessage
	{
		string ExpectedInvalidDateErrorMessage { get; }
	}

	interface ITestManyEmptyCodeErrorMessages
	{
		string[] ExpectedEmptyCodeErrorMessages { get; }
	}

	interface ITestManyEmptyDescriptionErrorMessages
	{
		string[] ExpectedEmptyDescriptionErrorMessages { get; }
	}

	interface ITestManyInvalidDateErrorMessages
	{
		string[] ExpectedInvalidDateErrorMessages { get; }
	}
}
