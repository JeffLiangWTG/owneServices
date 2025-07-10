using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class ConsolidatedDeclarationProcessTask : ProcessTask
	{
		public ConsolidatedDeclarationProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.ConsolidatedDeclaration; }
		}

		public new ConsolidatedDeclaration Parent
		{
			get { return (ConsolidatedDeclaration)base.Parent; }
		}

		protected override Type ParentType
		{
			get { return typeof(ConsolidatedDeclaration); }
		}
	}
}
