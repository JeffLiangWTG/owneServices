using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceCustomisationElementCollection))]
	sealed class ComplianceNumberSequenceCustomisationElementCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceNumberSequenceCustomisationElementCollection>
	{
		public void TestElementCount()
		{
			AssertEquals(18, ElementCollection.Count);
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
			bool actualAllowSort = (bool)typeof(ComplianceNumberSequenceCustomisationElementCollection).InvokeMember("AllowSort", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, ElementCollection, null);
			AssertEquals(true, actualAllowSort);
		}

		#region Implementation

		ComplianceNumberSequenceCustomisationElementCollection ElementCollection;

		protected override void SetUp()
		{
			base.SetUp();
			if (ElementCollection == null)
			{
				ElementCollection = new ComplianceNumberSequenceCustomisationElementCollection(Factory);
				ElementCollection.ParentConfiguration = new ComplianceNumberSequenceConfiguration(Collection.CurrentFallbackLevel, Factory);
				ElementCollection.PopulateElements();
			}
		}

		protected override ComplianceNumberSequenceCustomisationElementCollection GetCollectionToTest()
		{
			return new ComplianceNumberSequenceCustomisationElementCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceNumberSequenceCustomisationElement(Collection.CurrentFallbackLevel, Factory);
		}

		new ComplianceNumberSequenceCustomisationElementCollection Collection => base.Collection;

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
