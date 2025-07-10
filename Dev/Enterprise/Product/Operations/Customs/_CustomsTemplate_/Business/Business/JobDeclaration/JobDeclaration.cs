using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public partial class JobDeclaration : Customs.Business.BaseJobDeclaration, Integration.Customs._CustomsTemplate_.IJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ZString LocalCurrencyCodeCore
		{
			get
			{
				var fullName = GetType().FullName;
				if (!fullName.Contains("_CustomsTemplate_") && !fullName.Contains("Castle.Proxies.JobDeclarationProxy"))
				{
					throw new NotImplementedException();
				}
				return base.LocalCurrencyCodeCore;
			}
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("_CustomsTemplate_"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}
	}
}
