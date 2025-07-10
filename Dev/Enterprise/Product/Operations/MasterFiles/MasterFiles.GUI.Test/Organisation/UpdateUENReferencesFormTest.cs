using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UpdateUENReferencesForm))]
	sealed class UpdateUENReferencesFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new UpdateUENReferencesForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}
	}
}
