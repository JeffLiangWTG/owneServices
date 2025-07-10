using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	interface ITestEmptyCodeErrorMessage
	{
		ListCodeSet EmptyCodeTestData { get; }
		string ExpectedEmptyCodeErrorMessage { get; }
	}

	interface ITestEmptyDescriptionErrorMessage
	{
		ListCodeSet EmptyDescriptionTestData { get; }
		string ExpectedEmptyDescriptionErrorMessage { get; }
	}

	interface ITestInvalidStartDateErrorMessage
	{
		ListCodeSet InvalidStartDateTestData { get; }
		string InvalidStartDataErrorMessage { get; }
	}

	interface ITestEmptySecondaryCodeErrorMessage
	{
		ListCodeSet InvalidSecondaryCodeTestData { get; }
		string InvalidSecondaryCodeErrorMessage { get; }
	}
}
