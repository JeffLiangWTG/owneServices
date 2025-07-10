using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class StatementProcessTask : ProcessTask, Integration.Customs.Shared.IBaseCusStatementProcessTask
	{
		public static class StatementWorkflow
		{
			public const string Code = "STM";
			public static IMultilingualString Description => ResString.GetMultilingualString("8b1fa8d3-b8a7-4fea-bcdf-d99d7d3bd72b", "Statement");
		}

		public StatementProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(BaseCusStatementHeader);

		public new BaseCusStatementHeader Parent => (BaseCusStatementHeader)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.Customs.CustomsStatement;
	}
}
