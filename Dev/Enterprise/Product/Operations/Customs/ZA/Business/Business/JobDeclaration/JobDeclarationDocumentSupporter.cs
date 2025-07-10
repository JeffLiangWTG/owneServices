using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)BusinessObject; }
		}

		#region Overrides

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(BusinessObject.GetType());
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.RefundWorkSheet));
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			List<DocumentWrapper> list = [];
			switch (dataContext)
			{
				case Core.Constants.DataContext.CusEntryHeader:
					for (int i = 0; i < JobDeclaration.CustomsEntryHeaders.Count; i++)
					{
						if ((commandBeingRun?.SU_MenuName ?? ZString.Empty) != CustomsWorksheet || (JobDeclaration.CustomsEntryHeaders[i]?.InvoiceLines?.Count() ?? 0) > 0)
						{
							list.Add(DocCusEntryHeader.New(JobDeclaration.CustomsEntryHeaders[i], JobDeclaration.CustomsEntryHeaders[i].Factory));
						}
					}
					break;
				case Core.Constants.DataContext.DeclarationWithCusEntryHeaders:
					if (JobDeclaration.CustomsEntryHeaders.Count > 0)
					{
						list.Add(DocDeclaration.New(JobDeclaration, JobDeclaration.Factory));
					}
					break;
				case Core.Constants.DataContext.ComInvoiceHeader:
				case Core.Constants.DataContext.CommercialInvoice:
					for (int i = 0; i < JobDeclaration.Invoices.Count; i++)
					{
						list.Add(DocJobComInvoiceHeader.New(JobDeclaration.Invoices[i], JobDeclaration.Invoices[i].Factory));
					}
					break;
				case Core.Constants.DataContext.Declaration:
				case Core.Constants.DataContext.Notes:
				case Core.Constants.DataContext.ChargeSheet:
					list.Add(DocDeclaration.New(JobDeclaration, JobDeclaration.Factory));
					break;
				default:
					return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return [.. list];
		}
		const string CustomsWorksheet = "Customs Worksheet";

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var fullDataContext = dataContextValue.FullDataContext;
			if (fullDataContext == CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair ||
				fullDataContext == CusEntryHeaderDocumentSupporter.SADDocumentPack ||
				fullDataContext == CusEntryHeaderDocumentSupporter.VOCDocumentPack ||
				fullDataContext == CusEntryHeaderDocumentSupporter.DA63Document ||
				fullDataContext == CusEntryHeaderDocumentSupporter.RefundWorkSheet)
			{
				var result = new List<IBODocDataProvider>();
				foreach (CusEntryHeader entryHeader in JobDeclaration.ActiveEntryHeaders)
				{
					result.AddRange(entryHeader.DocumentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun));
				}
				return result.ToArray();
			}
			else
			{
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new ZStringBuilder();
			var fullDataContext = dataContextValue.FullDataContext;
			if (fullDataContext == CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair ||
				fullDataContext == CusEntryHeaderDocumentSupporter.SADDocumentPack ||
				fullDataContext == CusEntryHeaderDocumentSupporter.VOCDocumentPack ||
				fullDataContext == CusEntryHeaderDocumentSupporter.DA63Document ||
				fullDataContext == CusEntryHeaderDocumentSupporter.RefundWorkSheet)
			{
				var entryHeaders = JobDeclaration.ActiveEntryHeaders;
				if (entryHeaders.Count == 0)
				{
					result.AppendLine(Res.GetString("2A3486F2-08C5-430B-886E-A94942C668D7", "No Entry Header has been found for {0} document.", commandBeingRun?.SU_MenuName ?? ZString.Empty));
				}
				else
				{
					foreach (CusEntryHeader header in entryHeaders)
					{
						result.AppendLine(header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun));
					}
				}
			}
			else
			{
				result.AppendLine(base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun));
			}
			return result.ToString().Trim();
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			ZString result = ZString.Empty;

			if (filterType == MenuTemplateFilterType.CONTINUE)
			{
				CusEntryHeader currentHeader = (CusEntryHeader)docWrapperForCurrentPivot.ParentBusinessObject;

				if (currentHeader.MergedLines.Count > 1)
				{
					return "Yes";
				}

				return "No";
			}
			else if (filterType == MenuTemplateFilterType.MULTISUPPLIER)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica &&
					JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)
				{
					CusEntryHeader currentHeader = (CusEntryHeader)docWrapperForCurrentPivot.ParentBusinessObject;

					if (currentHeader.Suppliers.Count > 1)
					{
						return "Yes";
					}
				}

				return "No";
			}
			else if (filterType == MenuTemplateFilterType.BOE)
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode + JobDeclaration.JE_MessageType;
			}

			return base.GetMenuTemplateFilterValue(filterType, docWrapperForCurrentPivot);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			ZString result = ZGuid.Empty.ToString();
			if (filterName == DocumentFilters.DECTP)
			{
				result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + JobDeclaration.JE_MessageType;
			}
			else if (filterName == DocumentFilters.MSGBKR)
			{
				result = JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Miscellaneous ? "IMP" : base.GetFilterValue(filterName);
			}
			else
			{
				result = base.GetFilterValue(filterName);
			}

			return result;
		}

		protected override ZString MessageForInvaildEntryPrintDataState(IStmMenuItem menuItem)
		{
			return menuItem.SU_MenuName + " can not be printed until after the Declaration has been Framed.";
		}

		public override MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider)
		{
			return DocumentWatermarkHelper.GetWatermarkText(documentCommand.SU_MenuName, docDataProvider);
		}
		#endregion
	}
}
