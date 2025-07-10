using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	public static class ReviewFilterHelper
	{
		public static StmModuleFilter CreateValidFilter(BusinessObjectFactory factory, params GlbStaff[] matching)
		{
			var serialisedFilter = GlowFilterFor(matching.Select(s => s.GS_Code.ToString()));
			var filter = factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterData = Encoding.UTF8.GetBytes(serialisedFilter);
			factory.Save();
			FixFilterCompression(filter);

			return filter;
		}

		static void FixFilterCompression(StmModuleFilter filter)
			// CW1 Automatically compresses this column, but GLOW expects it uncompressed
			=> Db.Connection.ExecuteNonQuery("UPDATE StmModuleFilter SET S9_FilterData=dbo.CLRUncompressAsBytes(S9_FilterData) WHERE S9_PK=@pk", r => r.AddParameterBasedOnDbColumn("@pk", filter.PK.ToGuid(), StmModuleFilterSchema.PK));

		static string GlowFilterFor(IEnumerable<string> staffCodes)
			=>
$@"<ArrayOfFilterGroup xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""
    xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""
    xmlns=""http://cargowise.com/glow/2014/07/16/filters.xsd"">
	{string.Join(System.Environment.NewLine, staffCodes.Select(FilterGroupMatching))}
</ArrayOfFilterGroup>";

		static string FilterGroupMatching(string staffCode)
			=>
$@"<FilterGroup>
    <Filters>
        <Filter>
            <FilterType>StringFilter</FilterType>
            <Operation>Is</Operation>
            <PropertyPath>GS_Code</PropertyPath>
            <Values>
                <a:string>{staffCode}</a:string>
            </Values>
        </Filter>
    </Filters>
    <IsImplicit>false</IsImplicit>
</FilterGroup>";
	}
}
