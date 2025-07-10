using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public class NewAWBFormBuilder
	{
		public NewAWBFormBuilder(IMAWBAllocationParent parent = null)
				: base()
		{
			Parent = parent;
		}

		readonly IMAWBAllocationParent Parent;
		public static EventHandler NewEventHandler(IMAWBAllocationParent parent)
		{
			return delegate
			{
				NewAWBFormBuilder newAWBFormBuilder = new NewAWBFormBuilder(parent);
				newAWBFormBuilder.ShowDialog(parent);
			};
		}

		#region Show

		public void ShowDialog(IMAWBAllocationParent parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (!Env.Security.JobMAWBModify.IsAllowed)
			{
				Env.Security.JobMAWBModify.ShowError();
			}
			else if (!parent.IsAir || !parent.IsValidForNeutralMaster)
			{
				ShowError(Res.GetString("012eb6f5-440b-4a6a-b254-3dce63ae47ca", "This option only applies to direct, agent, coload AWB or gateway agent air consols with local Load Port."));
			}
			else if (parent.MasterBillAirlinePrefix.IsEmpty || RefAirline.LoadFromAirlinePrefix(parent.Factory, parent.MasterBillAirlinePrefix) == null)
			{
				ShowError(Res.GetString("65693a34-8857-4e1f-96af-bf435d558e15", "You must first enter a valid airline prefix."));
			}
			else if (new FreightJobMawbLink(parent.Factory).LoadFromParentPK(parent.Prefix, parent.PK) != null)
			{
				ShowError(Res.GetString("1d18b2f7-c6fa-4814-aebb-232330a3582a", "This consol already has a MAWB allocated."));
			}
			else
			{
				var factory = new BusinessObjectFactory();

				var mawb = factory.New<JobMawb>();
				using (mawb.GetValidationSuspender())
				{
					mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
					mawb.JM_GB = GlbBranch.CurrentBranch.PK;
					mawb.JM_Airline3DigitPrefix = parent.MasterBillAirlinePrefix;
					mawb.JM_MAWB = parent.MasterBillMAWB;

					mawb.ServiceLevelFilter = (serviceLevel) =>
					{
						return serviceLevel.PL_Code == parent.AWBServiceLevel
							|| serviceLevel.PL_Code == OrgCarrierServiceLevel.AllCode;
					};

					var isParentServiceLevelInCarrierList = mawb.NeutralAirWaybillServiceLevels
						.Cast<OrgCarrierServiceLevel>()
						.Any(sl => sl.PL_Code == parent.AWBServiceLevel);

					mawb.JM_ServiceLevel = isParentServiceLevelInCarrierList
						? parent.AWBServiceLevel.ToString()
						: OrgCarrierServiceLevel.AllCode;
				}

				using (NewAWBForm form = CreateAWBForm(mawb))
				{
					form.DialogResult = DialogResult.Cancel;

					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						UpdateParent(parent, (JobMawb)parent.Factory.ImportFromAnotherFactory(mawb));
					}
				}
			}
		}

		void UpdateParent(IMAWBAllocationParent parent, JobMawb mawb)
		{
			parent.IsNeutralMaster = mawb.JM_Calc_IsNeutral;

			if (parent.MAWBAllocation != null && parent.IsNeutralMaster)
			{
				parent.MAWBAllocation.SetAllocatedMAWBToParent(mawb, forceAllocateSpecificMAWB: true);
			}
			else if (!parent.IsNeutralMaster)
			{
				parent.MasterBillMAWB = mawb.JM_MAWB;
			}
		}

		protected virtual NewAWBForm CreateAWBForm(JobMawb mawb)
		{
			return new NewAWBForm(mawb, Parent);
		}

		#endregion

		#region Errors

		static void ShowError(string message)
		{
			Globals.Message.Show(
				message,
				Res.GetString("69434bf8-a3be-4b60-b48a-b1b1d9baf22e", "Error adding new MAWB."),
				System.Windows.Forms.MessageBoxButtons.OK,
				System.Windows.Forms.MessageBoxIcon.Error
				);
		}

		#endregion
	}
}
