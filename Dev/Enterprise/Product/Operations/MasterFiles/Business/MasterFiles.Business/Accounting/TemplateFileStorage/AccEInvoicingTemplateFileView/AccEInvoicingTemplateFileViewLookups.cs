//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccEInvoicingTemplateFileViewLookups
//
//    This class should be used for overriding collections in AutoAccEInvoicingTemplateFileViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.JobConfigurationLookupsExtensions;

namespace Enterprise.MasterFiles.Business
{
	public class AccEInvoicingTemplateFileViewLookups : AutoAccEInvoicingTemplateFileViewLookups
	{
		public AccEInvoicingTemplateFileViewLookups(AutoAccEInvoicingTemplateFileView parent) : base(parent)
		{
		}

		public new IJobConfiguration Parent => (IJobConfiguration)base.Parent;

		public CodeDescriptionPairList JobTypesList => GetJobTypeList();

		public CodeDescriptionPairList DirectionsList => Parent.GetDirectionList();

		public CodeDescriptionPairList TransportModesList => Parent.GetTransportModeList();

		public CodeDescriptionPairList TemplateCodesList
		{
			get
			{
				var filter = new ZQuery(AccTemplateFileStorageSchema.TFS_GC, ((AutoAccEInvoicingTemplateFileView)base.Parent).ETF_GC.ToGuid());
				filter.AddToFilter(AccTemplateFileStorageSchema.TFS_IsActive, true);
				filter.AddToFilter(AccTemplateFileStorageSchema.TFS_Ledger, LedgerTypes.AccountsReceivable);
				var templateFileList = Factory.Load<AccTemplateFileStorage>(filter).ToList();

				var list = new CodeDescriptionPairList();
				foreach (var item in templateFileList)
				{
					list.Add(new CodeDescriptionPair(item.TFS_Code.ToString(), item.TFS_Description.ToString()));
				}

				return list;
			}
		}
	}
}
