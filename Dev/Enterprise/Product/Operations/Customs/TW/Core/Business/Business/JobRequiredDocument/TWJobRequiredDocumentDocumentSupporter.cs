using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobRequiredDocumentDocumentSupporter : JobRequiredDocumentDocumentSupporter
	{
		public TWJobRequiredDocumentDocumentSupporter(IHaveRequiredDocuments parent) : base(parent)
		{
		}

		public TWJobRequiredDocumentDocumentSupporter(JobRequiredDocument jobRequiredDocument) : base(jobRequiredDocument)
		{
		}

		public const string TWLetterOfAuthorization = ".TWLetterOfAuthorization";

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var wrapperList = new List<IBODocDataProvider>();
			switch (dataContextValue.FullDataContext)
			{
				case TWLetterOfAuthorization:
					if (RequiredDocument != null && OrgHeader != null)
					{
						wrapperList.Add(new TWJobRequiredDocumentWrapper(OrgHeader, RequiredDocument));
					}
					else if (OrgHeader != null)
					{
						AddRequiredDocumentWrappers(wrapperList);
					}
					break;
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapperList.Count != 0 ? wrapperList.ToArray() : null;
		}

		void AddRequiredDocumentWrappers(List<IBODocDataProvider> wrapperList)
		{
			var list = new List<ZString>();
			var requiredDocuments = OrgHeader.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.IsTaiwanAttorneyDocumentForBroker && x.EQ_DocCategory == Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship);
			if (requiredDocuments?.Any() ?? false)
			{
				foreach (var requiredDocument in requiredDocuments)
				{
					var boxNumber = requiredDocument.BoxNumberDocAttrib?.D0_AttribDisplayValue ?? ZString.Empty;
					var customsDistrict = requiredDocument.CustomsDistrict;
					if (!boxNumber.IsEmpty || !customsDistrict.IsEmpty)
					{
						var key = ZString.Format("{0}_{1}", boxNumber, customsDistrict);
						if (!list.Contains(key))
						{
							list.Add(key);
							wrapperList.Add(new TWJobRequiredDocumentWrapper(OrgHeader, requiredDocument));
						}
					}
				}
			}
			if (!list.Any())
			{
				wrapperList.Add(new TWJobRequiredDocumentWrapper(OrgHeader, null));
			}
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(TWLetterOfAuthorization));
			return result;
		}
	}
}
