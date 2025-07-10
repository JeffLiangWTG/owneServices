using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public sealed class ReeferSetting : AutoReeferSetting
	{
		IStmALogParent parent;

		public ReeferSetting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public override ZGuid RFS_ParentId
		{
			get => base.RFS_ParentId;
			set
			{
				base.RFS_ParentId = value;
				parent = null;
			}
		}

		public override ZString RFS_ParentTableCode
		{
			get => base.RFS_ParentTableCode;
			set
			{
				base.RFS_ParentTableCode = value;
				parent = null;
			}
		}

		public IStmALogParent Parent
		{
			get
			{
				return parent ??= GetParent(RFS_ParentId, RFS_ParentTableCode, Factory);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			CreateReeferDetailsUpdatedForParent(Parent);
		}

		protected override void OnSavingForDelete()
		{
			base.OnSavingForDelete();
			CreateReeferDetailsUpdatedForParent(Parent);
		}

		static IStmALogParent GetParent(ZGuid parentId, ZString parentTableCode, BusinessObjectFactory factory)
		{
			if (!parentId.IsValid)
			{
				return null;
			}

			var parent = factory.GetBizOsForPK(parentId.ToGuid()).FirstOrDefault(parent => !parent.IsDeleted);

			if (parent != null)
			{
				return parent as IStmALogParent;
			}

			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(parentTableCode);
			return type == null ? null : factory.Load(type, parentId) as IStmALogParent;
		}

		static void CreateReeferDetailsUpdatedForParent(IStmALogParent parent)
		{
			if (parent == null)
			{
				return;
			}

			if (parent.Logs.LogsNotInDB.Any(log => log.SL_SE_NKEvent == AutoEvents.ReeferDetailsUpdated.Code))
			{
				return;
			}

			parent.Logs.AddNew(AutoEvents.ReeferDetailsUpdated);
		}
	}
}
