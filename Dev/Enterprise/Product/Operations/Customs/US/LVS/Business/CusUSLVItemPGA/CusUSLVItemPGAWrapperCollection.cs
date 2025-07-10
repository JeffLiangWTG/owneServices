using System;
using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGAWrapperCollection : NonPersistentBusinessObjectCollection<CusUSLVItemPGAWrapper>
	{
		public CusUSLVItemPGAWrapperCollection(CusUSLVItemPGAAgencyRequirementsProvider provider)
			: base(provider.ParentItem.Factory)
		{
			pgaProvider = provider;
			ParentItem = provider.ParentItem;
		}

		CusUSLVItem ParentItem { get; }
		readonly CusUSLVItemPGAAgencyRequirementsProvider pgaProvider;

		internal void Populate()
		{
			if (pgaProvider.IsPGAReqirementRelevant)
			{
				var programs = pgaProvider.GetGovernmentAgencyProgramCodeList();

				foreach (CodeDescriptionPair program in programs)
				{
					if (pgaProvider.DoesMatchCertificationMode(program.Code))
					{
						CreatePGAWrapper(program);
					}
				}
			}
		}

		void CreatePGAWrapper(CodeDescriptionPair program)
		{
			var agencyProgram = pgaProvider.PopulateAgencyProgram(program.Code);
			var pga = ParentItem.CusUSLVItemPGAs.Find(p => p.ULP_Agency == agencyProgram.Agency && p.ULP_AgencyProgram == agencyProgram.Program).FirstOrDefault();
			var requirement = new CusUSLVItemPGAWrapper(Factory, pgaProvider, program.Code, pga);
			(requirement.Agency, requirement.AgencyProgram) = agencyProgram;
			requirement.AgencyCodeWithDescription = pgaProvider.PopulateAgencyCodeWithDescription(program);
			Add(requirement);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException("This is a fixed collection");
		}
	}
}
