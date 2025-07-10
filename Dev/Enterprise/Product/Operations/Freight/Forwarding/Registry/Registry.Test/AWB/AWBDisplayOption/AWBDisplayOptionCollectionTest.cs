using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.AWB.Testing
{
	[TestedType(typeof(AWBDisplayOptionCollection))]
	public class AWBDisplayOptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AWBDisplayOptionCollection>
	{
		public void TestGetDefault()
		{
			AWBDisplayOptionCollection defaultValue = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			AssertEquals(121, defaultValue.Count);

			defaultValue = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			AssertEquals(121, defaultValue.Count);

			foreach (AWBDisplayOption option in defaultValue)
			{
				if (option.IATADescription.ToString().ToUpper().Contains("AGENT"))
				{
					AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, option.Entitlement);
				}
			}
		}

		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, this.Collection.AllowNew);
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

		protected override AWBDisplayOptionCollection GetCollectionToTest()
		{
			return new AWBDisplayOptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AWBDisplayOption();
		}

		#endregion
	}
}
