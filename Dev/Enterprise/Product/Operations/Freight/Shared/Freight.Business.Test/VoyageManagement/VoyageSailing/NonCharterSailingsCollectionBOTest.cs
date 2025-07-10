using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(NonCharterSailingCollection))]
	sealed class NonCharterSailingsCollectionBOTest : CharterStateSpecificCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonCharterSailingCollection(Factory);
		}

		protected override ZBool IsCharter
		{
			get { return false; }
		}

		protected override ZString ErrorText
		{
			get { return "Only a non chartered sailing may be selected"; }
		}

		protected override ZString ModuleFilterCode
		{
			get { return FreightConstants.CharterFilter.NonCharterOnlyCode; }
		}

		#endregion
	}
}
