using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public class SalesRelationTypeList : CodeDescriptionPairList
	{
		protected SalesRelationTypeList()
		{
			var codes = new HashSet<string>(GetCodes(), StringComparer.OrdinalIgnoreCase);

			foreach (ICodeDescription pair in new RelatableActivityTypeList())
			{
				if (codes.Contains(pair.Code))
				{
					Add(pair);
				}
			}
		}

		public static SalesRelationTypeList New()
		{
			return OverridableNewDelegate.Value?.Invoke() ?? new SalesRelationTypeList();
		}

		protected delegate SalesRelationTypeList NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public virtual IEnumerable<string> GetCodes()
		{
			yield return RelatableActivityTypeList.Codes.CampaignManagement;
			yield return RelatableActivityTypeList.Codes.InquiryManager;
			yield return RelatableActivityTypeList.Codes.OpportunityManager;
			yield return RelatableActivityTypeList.Codes.Quotations;
			yield return RelatableActivityTypeList.Codes.OneOffQuotes;
			yield return RelatableActivityTypeList.Codes.Communication;
			yield return RelatableActivityTypeList.Codes.Projects;
		}
	}
}
