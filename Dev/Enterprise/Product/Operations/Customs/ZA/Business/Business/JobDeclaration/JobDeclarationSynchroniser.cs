using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		public new JobDeclaration Destination
		{
			get { return (JobDeclaration)base.Destination; }
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();

			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_MasterBillIssuedDateInfo, GetMasterBillIssuedDate, GetMasterBillIssuedDateRelatedInfos, dontSetFieldsReadOnly: true, IsMasterBillIssuedDateSyncEnabled));
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKMasterBillIssuedAtInfo, GetMasterBillIssuedAt, GetMasterBillIssuedAtRelatedInfos, dontSetFieldsReadOnly: true, IsMasterBillIssuedAtSyncEnabled));
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.HouseBillIssuedDateInfo, GetHouseBillIssuedDate, GetHouseBillIssuedDateRelatedInfos));
		}

		#region MasterBillIssuedDate

		IEnumerable<ZPropertyInfo> GetMasterBillIssuedDateRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_MasterBillIssueDateInfo);
			}
			return infos;
		}

		IZType GetMasterBillIssuedDate()
		{
			var result = ZDateTime.Empty;
			if (hookedConsol != null)
			{
				result = hookedConsol.JK_MasterBillIssueDate;
			}
			return result;
		}

		bool IsMasterBillIssuedDateSyncEnabled() => Destination.JE_MasterBillIssuedDateInfo.Value.IsEmpty;

		#endregion

		#region MasterBillIssuedAt

		IEnumerable<ZPropertyInfo> GetMasterBillIssuedAtRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKMasterBillIssuePlaceInfo);
			}
			return infos;
		}

		IZType GetMasterBillIssuedAt()
		{
			var result = ZString.Empty;
			if (hookedConsol != null)
			{
				result = hookedConsol.JK_RL_NKMasterBillIssuePlace;
			}
			return result;
		}

		bool IsMasterBillIssuedAtSyncEnabled() => Destination.JE_RL_NKMasterBillIssuedAtInfo.Value.IsEmpty;

		#endregion

		#region HouseBillIssuedDate

		IEnumerable<ZPropertyInfo> GetHouseBillIssuedDateRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(Source.JS_HouseBillInfo);
			infos.Add(Source.JS_HouseBillIssueDateInfo);
			return infos;
		}

		IZType GetHouseBillIssuedDate()
		{
			return Source.JS_HouseBillIssueDate;
		}

		#endregion
	}
}
