using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceCustomisationElementCollection))]
	sealed class InvoiceRemittanceCustomisationElementCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceRemittanceCustomisationElementCollection>
	{
		public void TestElementCount()
		{
			AssertEquals(21, ElementCollection.Count);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, ElementCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, ElementCollection.AllowRemove);
		}

		public void TestAllowSort()
		{
			bool actualAllowSort = (bool)typeof(InvoiceRemittanceCustomisationElementCollection).InvokeMember("AllowSort", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, ElementCollection, null);
			AssertEquals(true, actualAllowSort);
		}

		#region Implementation

		InvoiceRemittanceCustomisationElementCollection ElementCollection;

		protected override void SetUp()
		{
			base.SetUp();
			if (ElementCollection == null)
			{
				ElementCollection = new InvoiceRemittanceCustomisationElementCollection(Factory);
				ElementCollection.ParentConfiguration = new InvoiceRemittanceConfiguration(Collection.CurrentFallbackLevel, Factory);
				ElementCollection.PopulateElements();
			}
		}

		protected override InvoiceRemittanceCustomisationElementCollection GetCollectionToTest()
		{
			return new InvoiceRemittanceCustomisationElementCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceRemittanceCustomisationElement(Collection.CurrentFallbackLevel, Factory);
		}

		new InvoiceRemittanceCustomisationElementCollection Collection => base.Collection;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion

	}
}
