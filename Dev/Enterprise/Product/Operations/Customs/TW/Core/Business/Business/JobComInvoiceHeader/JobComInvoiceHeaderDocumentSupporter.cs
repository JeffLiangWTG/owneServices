using System;
using System.Collections.Generic;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceHeaderDocumentSupporter : Customs.Business.JobComInvoiceHeaderDocumentSupporter
	{
		internal const string InvoicePackingWeightListDocument = ".InvoicePackingWeightListDocument";
		internal const string GenericCommercialInvoice = ".GenericCommercialInvoice";
		internal const string CusPackingList = "CusPackingList";
		public JobComInvoiceHeaderDocumentSupporter(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader) { }

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(InvoicePackingWeightListDocument));
			result.Add(new DataContextValue(GenericCommercialInvoice));
			result.Add(new DataContextValue(CusPackingList));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.FullDataContext == InvoicePackingWeightListDocument)
			{
				return new IBODocDataProvider[] { BODocDataProvider.Get(new InvoicePackingWeightListDocumentWrapper(InvoiceHeader, Factory)) };
			}
			else if (dataContextValue.FullDataContext == GenericCommercialInvoice)
			{
				if (InvoiceHeader.IsAttachedToPersistentDeclaration)
				{
					return [CommercialInvoiceWrapper.New(InvoiceHeader, InvoiceHeader.Factory)];
				}
				else
				{
					return [SingleCommercialInvoiceWrapper.New(InvoiceHeader, InvoiceHeader.Factory)];
				}
			}
			else
			{
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.CusPackingList)
			{
				var packingList = InvoiceHeader.LoadCusPackingList(Factory);
				return packingList == null ? Array.Empty<DocumentWrapper>() : [InvoiceDocCusPackingList.New(packingList, packingList.Factory)];
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}
	}
}
