namespace Enterprise.Customs.NZ.Business
{
	public partial class PackageTypeList
	{
		public bool IsBulkType(string code)
		{
			return code == "VG" || code == "VQ" || code == "VL" || code == "VY" || code == "VR" || code == "VO";
		}
	}
}
