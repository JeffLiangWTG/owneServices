using System;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public sealed override RefCountryCollection DefaultOrigins
		{
			get { throw new NotSupportedException("Use USCountryList instead"); }
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get
			{
				CodeDescriptionPairList result = base.JZ_IncoTerm_List;
				if (Invoice.JobDeclaration != null)
				{
					if (Invoice.JobDeclaration.IsImport)
					{
						result = Factory.GetCachedValue<TermsOfDeliveryList>();
					}
				}

				return result;
			}
		}

		public override CodeDescriptionPairList MessageTypes
		{
			get { return JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory); }
		}

		public CodeDescriptionPairList AIIStatus
		{
			get { return new MessageStatusListEI(); }
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return Parent.IsExport ? base.MessageStatusList : Parent.AddInfoLookups.MessageStatusList; }
		}

		public override OrgHeaderCollection BuyerAgents
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public override OrgHeaderCollection SellingAgents
		{
			get { return new ConsignorCollection(Factory); }
		}
	}
}
