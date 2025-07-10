using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business
{
	public static class JobDeclarationDocumentSupporterHelper
	{
		public static DocumentWrapper[] GetWrappersForInvoice(IStmMenuItem commandBeingRun, bool useDocBuilderInvoice, BaseJobDeclaration declaration)
		{
			OrgHeader orgForInvoice = null;
			var orgs = new List<ZGuid>();

			if (commandBeingRun != null)
			{
				if (commandBeingRun.SU_ContactType == ContactType.Consignee.Code)
				{
					orgForInvoice = declaration.Consignee;
				}
				else if (commandBeingRun.SU_ContactType == ContactType.Consignor.Code)
				{
					orgForInvoice = declaration.Consignor;
				}
				else if (declaration.Job != null)
				{
					if (commandBeingRun.SU_ContactType.IsEmpty || commandBeingRun.SU_ContactType == ContactType.All.Code || commandBeingRun.SU_ContactType == ContactType.NoContactType.Code || commandBeingRun.SU_ContactType == ContactType.Receivables.Code)
					{
						orgForInvoice = declaration.Job.LocalCharges;
					}
					else if (commandBeingRun.SU_ContactType == ContactType.ExportBroker.Code || commandBeingRun.SU_ContactType == ContactType.ImportBroker.Code)
					{
						orgForInvoice = declaration.Job.AgentCollect;
					}
				}

				if (orgForInvoice != null)
				{
					orgs.Add(orgForInvoice.PK);

					var billToOrg = orgForInvoice.GetCustomsBillTo(false, declaration.TransportMode, declaration.ContainerMode);
					if (billToOrg != null)
					{
						orgs.Add(billToOrg.PK);
					}
				}
			}

			return new InvoiceLoader(declaration.Factory).GetWrappersForARInvoice(orgs, declaration.JE_DeclarationReference, useDocBuilderInvoice);
		}
	}
}
