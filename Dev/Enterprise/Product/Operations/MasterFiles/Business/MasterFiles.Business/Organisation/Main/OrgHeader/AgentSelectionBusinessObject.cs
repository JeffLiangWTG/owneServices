using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AgentSelectionBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AgentSelectionBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Selected Agent
		[List("Agents")]
		public ZGuid SelectedAgentPK
		{
			get { return selectedAgentPK; }
			set
			{
				SetNonPersistentPropertyValue(SelectedAgentPKInfo, ref selectedAgentPK, value);
				if (!IsValidationSuspended)
				{
					ValidateSelectedAgentPK();
				}
			}
		}

		ZGuid selectedAgentPK;

		public ZPropertyInfo SelectedAgentPKInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedAgentPK)); }
		}

		public OrgHeader SelectedAgent
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), SelectedAgentPK); }
		}

		#endregion

		#region Validation

		public void ValidateSelectedAgentPK()
		{
			SelectedAgentPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SelectedAgentPKInfo);
			ListValidation.ErrorIfInvalidPK(SelectedAgentPKInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSelectedAgentPK();
		}

		#endregion

		#region Lookups

		public ForwarderCollection Agents
		{
			get
			{
				if (fAgents == null)
				{
					fAgents = new ForwarderCollection(Factory);
				}

				return fAgents;
			}
		}

		ForwarderCollection fAgents;

		#endregion
	}
}
