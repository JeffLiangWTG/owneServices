using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryProcessTaskCollection : ProcessTaskCollection
	{
		public SalesEnquiryProcessTaskCollection(SalesEnquiry enquiry) : base(enquiry)
		{
		}

		public new SalesEnquiryProcessTask this[int index]
		{
			get { return (SalesEnquiryProcessTask)Elements[index]; }
		}

		public new SalesEnquiryProcessTask AddNew()
		{
			return (SalesEnquiryProcessTask)base.AddNew();
		}

		SalesEnquiry ParentEnquiry
		{
			get { return (SalesEnquiry)base.Parent; }
		}

		#region Contacts and Addresses

		public override bool SupportsContactAndAddress
		{
			get { return false; }
		}

		public override ZString OriginCountry
		{
			get { return Country; }
		}

		public override ZString DestinationCountry
		{
			get { return Country; }
		}

		ZString Country
		{
			get
			{
				ZString result = ZString.Empty;
				if (ParentEnquiry.O1_PortOrCountry.Length == 2)
				{
					result = ParentEnquiry.O1_PortOrCountry;
				}
				else
				{
					var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, ParentEnquiry.O1_PortOrCountry);
					if (port != null)
					{
						result = port.RL_RN_NKCountryCode;
					}
				}
				return result;
			}
		}

		#endregion

		#region Default Values

		protected internal override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask task, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, task, defaultAssignedStaff);

			task.OrganisationPK = ParentEnquiry.OrgPk;
			var org = ParentEnquiry.Header;

			if (org != null && org.MainAddress != null)
			{
				task.P9_OA = ParentEnquiry.Header.MainAddress.PK;
			}

			if (!task.P9_ParentTemplateID.IsEmpty && Count == 0 // first template task
				&& task.P9_GS_NKAssignedStaffMember.IsEmpty
				&& !ParentEnquiry.O1_GS_NKRepAssigned.IsEmpty)
			{
				task.P9_GS_NKAssignedStaffMember = ParentEnquiry.O1_GS_NKRepAssigned;
			}
		}

		#endregion
	}
}
