using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class OrgWithSourceSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var orgWithSource = (OrgWithSource)objectToSerialize;

			return orgWithSource != null
				? ZString.Format("{0}\r\n{1}", Res.GetString("c15e6c65-c983-47e8-b63e-cdcdf059f3a4", "Code: {0}", orgWithSource.Org.OH_Code), Res.GetString("52ec7878-4be3-4339-9921-9e4fb6e0697a", "Source: {0}", orgWithSource.SourceText))
				: (ZString)Res.GetString("a5142608-436c-486e-ad6c-d24fd7572f9f", "(null)");
		}
	}
}
