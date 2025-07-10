using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class USReleaseDeclarationLoader
	{
		public static JobDeclaration GetReleaseDeclaration(BusinessObjectFactory factory, ZString releaseNumber, ZGuid companyPK)
		{
			JobDeclaration releaseDeclaration = null;
			if (!releaseNumber.IsEmpty)
			{
				var entryFilerCode = releaseNumber.Left(3);
				var entryNumber = releaseNumber.SubstringSafe(3);
				if (!entryFilerCode.IsEmpty && !entryNumber.IsEmpty)
				{
					releaseDeclaration = factory.GetCachedValue("ReleaseDeclarationFor" + releaseNumber + companyPK, delegate
					{
						JobDeclaration returnVal = null;
						var originalEntry = new CusEntryHeader.Loader(factory).FindByEntryNumberAndFilerCode(companyPK, entryNumber, entryFilerCode, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease);
						if (originalEntry != null && originalEntry.Declaration != null)
						{
							returnVal = factory.Load<JobDeclaration>(originalEntry.Declaration.PK);
						}
						return returnVal;
					});
				}
			}
			return releaseDeclaration;
		}
	}
}
