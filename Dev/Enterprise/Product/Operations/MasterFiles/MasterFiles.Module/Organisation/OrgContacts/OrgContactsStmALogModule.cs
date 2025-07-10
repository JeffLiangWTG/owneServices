using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsStmALogModule : ZFilterGridModule, IFilterGridModuleInternalsForTesting
	{
		public override ModuleIdentifier ID => ModuleIDs.OrgContactsStmALog;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgContactsStmALog);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new OrgContactStmALogFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgContactStmALogFilterControl((StmALogCollection)GridCollection, (OrgContactStmALogFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmALogCollection(Factory);
		}

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		public override bool AllowView => false;

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

#if DEBUG

		IBusinessObjectCollection IFilterModuleInternalsForTesting.GridCollection
		{
			get { return new StmALogCollectionForTest(new BusinessObjectFactory()); }
		}

		public class StmALogCollectionForTest : StmALogCollection
		{
			public StmALogCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new StmALogForTest this[int i] => (StmALogForTest)Elements[i];

			public new StmALogForTest AddNew()
			{
				return Factory.New<StmALogForTest>();
			}
		}

		public class StmALogForTest : StmALog
		{
			public StmALogForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[BusinessObjectTestExclude]
			public override ZString SL_SE_NKEvent
			{
				get => base.SL_SE_NKEvent;
				set
				{
					using (LockForUpdatingKeyFieldsForTesting())
					{
						base.SL_SE_NKEvent = value;
					}
				}
			}

			public override ZDateTime SL_EventTime
			{
				get => base.SL_EventTime;
				set
				{
					using (LockForUpdatingKeyFieldsForTesting())
					{
						base.SL_EventTime = value;
					}
				}
			}
		}

#endif
	}
}
