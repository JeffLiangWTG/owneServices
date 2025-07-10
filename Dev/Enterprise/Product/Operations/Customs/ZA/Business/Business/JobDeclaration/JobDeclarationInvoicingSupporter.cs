using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;

namespace Enterprise.Customs.ZA.Business
{
	class JobDeclarationInvoicingSupporter : Customs.Business.BaseJobDeclaration.BaseJobDeclarationInvoicingSupporter
	{
		public JobDeclarationInvoicingSupporter(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		protected override ZGuid OverridenDepartment
		{
			get
			{
				return JobInvoicingTransportMode == Constants.TransportModes.FixedTransportInstallations ?
					(IsImport ? ObjectFactory.Get<IAccounting>().CustomsImportOther : ObjectFactory.Get<IAccounting>().CustomsOther) :
					base.OverridenDepartment;
			}
		}
	}
}
