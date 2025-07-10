using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AgentSelectionBusinessObject))]
	sealed class AgentSelectionBusinessObjectTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestLookupsForwarders()
		{
			AgentSelectionBusinessObject selectionBizo = new AgentSelectionBusinessObject(Factory);
			selectionBizo.Agents.Load();

			Assert(selectionBizo.Agents.Count > 0);
		}

		public void TestSelectedAgentPKValidation()
		{
			AgentSelectionBusinessObject selectionBizo = new AgentSelectionBusinessObject(Factory);
			selectionBizo.Agents.Load();

			selectionBizo.RunPreSaveValidation();
			AssertHasErrors(selectionBizo.SelectedAgentPKInfo);

			selectionBizo.SelectedAgentPK = selectionBizo.Agents[0].PK;
			selectionBizo.RunPreSaveValidation();
			AssertNoErrors(selectionBizo.SelectedAgentPKInfo);

			AssertEquals("Object loaded", selectionBizo.Agents[0], selectionBizo.SelectedAgent);

			selectionBizo.SelectedAgentPK = ZGuid.NewZGuid();
			selectionBizo.RunPreSaveValidation();
			AssertHasErrors(selectionBizo.SelectedAgentPKInfo);

			AssertNull("Object cleared", selectionBizo.SelectedAgent);

			selectionBizo.SelectedAgentPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.False)).PK;
			selectionBizo.RunPreSaveValidation();
			AssertHasErrors(selectionBizo.SelectedAgentPKInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AgentSelectionBusinessObject(Factory);
		}

		#endregion
	}
}
