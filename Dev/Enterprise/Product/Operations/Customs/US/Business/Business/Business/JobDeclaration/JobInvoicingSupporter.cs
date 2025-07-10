using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationInvoicingSupporter : Customs.Business.BaseJobDeclaration.BaseJobDeclarationInvoicingSupporter
	{
		public JobDeclarationInvoicingSupporter(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			string result = null;
			if (Parent.JE_MessageType == JobMessageTypeList.Codes.Import)
			{
				if (CustomsDataRegistry.Instance.EnableAccountingIntegration.Value.EnableAccountingIntegration &&
				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetAllChargeCodesIncludingDefault().Contains(chargeCodePK))
				{
					if (Parent.US_PaymentType != PaymentTypeList.Codes.IndividualBasis)
					{
						if (Parent.RelatedStatement == null || Parent.RelatedStatement.B2_Status != StatementHeaderStatusList.Codes.Final)
						{
							result = ShouldNotChangeCostOrPostWhileOnStatement;
						}
					}
				}
			}

			return result;
		}

		public const string ShouldNotChangeCostOrPostWhileOnStatement = @"it is put on a statement. 
The Customs Disbursement AP charges will be posted automatically on receipt of the final statement from Customs.
To post other charges first, remove cost amounts from the Customs Disbursement AP charge line(s)";

		protected override ZGuid OverridenDepartment
		{
			get
			{
				ZGuid departmentPK = ObjectFactory.Get<IAccounting>().CustomsOther;

				if (Parent.IsExWarehouse)
				{
					departmentPK = ObjectFactory.Get<IAccounting>().CustomsExWarehouse;
				}
				else if (IsImport)
				{
					switch (JobInvoicingTransportMode)
					{
						case TransportTypeList.Codes.Air:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportAirUld;
							break;
						case TransportTypeList.Codes.Sea:
							departmentPK = Parent.JE_ContainerCount > 0 ? ObjectFactory.Get<IAccounting>().CustomsImportSeaFcl : ObjectFactory.Get<IAccounting>().CustomsImportSeaLcl;
							break;
						case TransportTypeList.Codes.Rail:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportRail;
							break;
						case TransportTypeList.Codes.Road:
						case TransportTypeList.Codes.Auto:
						case TransportTypeList.Codes.Truck:
						case TransportTypeList.Codes.Pedestrian:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportRoad;
							break;
						case TransportTypeList.Codes.Mail:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportPost;
							break;
						default:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsImportOther;
							break;
					}
				}
				else if (IsExport)
				{
					switch (JobInvoicingTransportMode)
					{
						case TransportTypeList.Codes.Air:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportAirUld;
							break;
						case TransportTypeList.Codes.Sea:
							departmentPK = Parent.JE_ContainerCount > 0 ? ObjectFactory.Get<IAccounting>().CustomsExportSeaFcl : ObjectFactory.Get<IAccounting>().CustomsExportSeaLcl;
							break;
						case TransportTypeList.Codes.Rail:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportRail;
							break;
						case TransportTypeList.Codes.Road:
						case TransportTypeList.Codes.Auto:
						case TransportTypeList.Codes.Truck:
						case TransportTypeList.Codes.Pedestrian:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportRoad;
							break;
						case TransportTypeList.Codes.Mail:
							departmentPK = ObjectFactory.Get<IAccounting>().CustomsExportPost;
							break;
					}
				}

				return departmentPK;
			}
		}
	}
}
