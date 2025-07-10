using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public static class OtherReconIssueListCreator
	{
		public static ReconIssueCodeList CreateOtherReconIssueList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OtherReconIssueList", delegate
				{
					ReconIssueCodeList result = new ReconIssueCodeList();
					result.RemoveCode(ReconIssueCodeList.Codes.FTA);
					return result;
				});
		}
	}
}
