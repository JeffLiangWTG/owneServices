using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GLAccountSelectionAndEntryGuidRegistryDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != Guid.Empty)
			{
				var factory = new ReadOnlyBusinessObjectFactory();
				var chart = factory.Load<AccAlternateChart>(proposedValue) ?? throw new RegistryValidationException(Res.GetString("120502C5-41D1-4843-B09A-C10A04A29BF1", "Please select a valid Alternate Chart."));
				var query = new ZQuery();
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, proposedValue);
				var alternateGLAccounts = factory.Load<AccAlternateGLAccount>(query);

				if (!alternateGLAccounts.Any())
				{
					throw new RegistryValidationException(Res.GetString("ED02D0D3-F196-4FE6-BE9F-AE7AACA72F32", "The Alternate Chart of Account you selected does not have any Alternate GL Accounts. Please create Alternate GL Accounts for it first."));
				}
			}
		}
	}
}
