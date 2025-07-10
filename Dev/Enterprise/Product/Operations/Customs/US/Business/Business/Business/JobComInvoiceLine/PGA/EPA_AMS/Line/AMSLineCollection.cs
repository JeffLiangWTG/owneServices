using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class AMSLineCollection : DependentCusAddInfoCollection<AMSLine, AMS>
	{
		public AMSLineCollection(AMS master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAMSLine)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = (AMSLine)child;
			if (newLine != null)
			{
				var master = Master;
				var invoiceLine = master.InvoiceLine;
				if (invoiceLine != null)
				{
					if (master.US_Program == AMSProgramList.Codes.MO1 || master.US_Program == AMSProgramList.Codes.MO5 || master.US_Program == AMSProgramList.Codes.PN1)
					{
						newLine.US_OA_Applicant = invoiceLine.JI_OA_ConsigneeAddress;
						newLine.US_Packages = invoiceLine.JI_InvoiceQuantity;
						newLine.US_PackagesUQ = invoiceLine.JI_InvoiceUQ;
						DefaultTheNetWeight(newLine, invoiceLine);
					}
					if (master.US_Program == AMSProgramList.Codes.MO4)
					{
						DefaultTheNetWeight(newLine, invoiceLine);
					}
					if (master.US_Program == AMSProgramList.Codes.EG1)
					{
						DefalutDocSubmitted(newLine, invoiceLine);
					}

					if (master.US_Program == AMSProgramList.Codes.MO2)
					{
						newLine.US_CertType = LPCOTypeList.Codes.AM6;
						DefaultTheNetWeight(newLine, invoiceLine);
					}
					var declration = invoiceLine.Declaration;
					if (declration != null)
					{
						newLine.US_InspecDateTime = invoiceLine.Declaration.US_InspecDate;
					}
				}
			}
		}

		void DefaultTheNetWeight(AMSLine newLine, JobComInvoiceLine invoiceLine)
		{
			if (Core.Constants.Weight.ContainsCode(invoiceLine.JI_CustomsUnitQty))
			{
				newLine.US_NetWeight = invoiceLine.JI_CustomsQuantity;
				newLine.US_NetWeightUQ = invoiceLine.JI_CustomsUnitQty.Left(3);
			}
			else
			{
				newLine.US_NetWeight = invoiceLine.JI_NetWeight;
				newLine.US_NetWeightUQ = invoiceLine.JI_NetWeightUQ;
			}
		}

		void DefalutDocSubmitted(AMSLine newLine, JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			if (declaration != null && declaration.CheckDISDocumentHasBeenAcceptedByFormType("AMSMO1"))
			{
				newLine.US_IsDocSubmitted = true;
			}
		}
	}
}
