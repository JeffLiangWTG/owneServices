using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLLocalNumberFormatCollection))]
	sealed class GLLocalNumberFormatCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GLLocalNumberFormatCollection>
	{
		public void TestGetGLLocalNumberFormat()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			AssertEquals(gNF, list.GetGLLocalNumberFormat(Constants.Languages.ChineseSimplified, Constants.CountryCodes.China));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override GLLocalNumberFormatCollection GetCollectionToTest()
		{
			return new GLLocalNumberFormatCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLLocalNumberFormat();
		}

		#endregion
	}
}
