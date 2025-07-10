using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGADisclaimOptionCollection : NonPersistentBusinessObjectCollection<CusUSLVItemPGADisclaimOption>
	{
		public CusUSLVItemPGADisclaimOptionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Populate(IEnumerable<CusUSLVConsignment> consignments)
		{
			if (!consignments.IsNullOrEmpty())
			{
				var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.GetNull<CusUSLVItem>());
				var programs = provider.GetGovernmentAgencyProgramCodeList();
				foreach (CodeDescriptionPair program in programs)
				{
					if (program.Code != GovernmentAgencyProgramCodeList.Codes.DOT
						&& program.Code != GovernmentAgencyProgramCodeList.Codes.DDTC
						&& program.Code != GovernmentAgencyProgramCodeList.Codes.ATF
						&& program.Code != GovernmentAgencyProgramCodeList.Codes.SIMP)
					{
						if (consignments.Any(consignment => consignment.HasAnyItemApplicableToPGA(program.Code)))
						{
							var disclaimOption = new CusUSLVItemPGADisclaimOption(Factory, program.Code);
							disclaimOption.AgencyCodeWithDescription = provider.PopulateAgencyCodeWithDescription(program);
							Add(disclaimOption);
						}
					}
				}
			}
		}

		public Dictionary<ZString, ZString> ToDictionary()
		{
			Dictionary<ZString, ZString> result = new Dictionary<ZString, ZString>();
			foreach (CusUSLVItemPGADisclaimOption option in this)
			{
				if (option.DisclaimReason != ZString.Empty)
				{
					if (!result.ContainsKey(option.AgencyCode))
					{
						result.Add(option.AgencyCode, option.DisclaimReason);
					}
				}
			}
			return result;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException("This is a fixed collection");
		}
	}
}
