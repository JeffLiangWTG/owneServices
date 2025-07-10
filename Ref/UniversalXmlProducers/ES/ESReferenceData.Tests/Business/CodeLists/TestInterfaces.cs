namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

interface ITestEmptyCodeErrorMessage
{
	string ExpectedEmptyCodeErrorMessage { get; }
}

interface ITestEmptyDescriptionErrorMessage
{
	string ExpectedEmptyDescriptionErrorMessage { get; }
}

interface ITestEmptyStartDateErrorMessage
{
	string ExpectedEmptyStartDateErrorMessage { get; }
}

interface ITestInvalidStartDateErrorMessage
{
	string ExpectedInvalidStartDateErrorMessage { get; }
}

interface ITestEmptyEndDateErrorMessage
{
	string ExpectedEmptyEndDateErrorMessage { get; }
}

interface ITestInvalidEndDateErrorMessage
{
	string ExpectedInvalidEndDateErrorMessage { get; }
}

interface ITestEmptyCharacterIndicationErrorMessage
{
	string ExpectedEmptyCharacterIndicationErrorMessage { get; }
}

interface ITestEmptySpecialIndicationErrorMessage
{
	string ExpectedEmptySpecialIndicationErrorMessage { get; }
}
