using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CharterSailingCollection))]
	sealed class CharterSailingsCollectionTest : CharterStateSpecificCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CharterSailingCollection(Factory);
		}

		protected override ZBool IsCharter
		{
			get { return true; }
		}

		protected override ZString ErrorText
		{
			get { return "Only a chartered sailing may be selected"; }
		}

		protected override ZString ModuleFilterCode
		{
			get { return FreightConstants.CharterFilter.CharterOnlyCode; }
		}

		#endregion
	}
}
