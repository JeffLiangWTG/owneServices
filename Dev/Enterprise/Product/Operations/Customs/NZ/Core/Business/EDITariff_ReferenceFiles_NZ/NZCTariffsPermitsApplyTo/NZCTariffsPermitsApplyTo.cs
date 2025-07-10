using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCTariffsPermitsApplyTo : AutoNZCTariffsPermitsApplyTo
	{
		public NZCTariffsPermitsApplyTo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Load
		public static NZCTariffsPermitsApplyTo Load(BusinessObjectFactory factory, ZString tariffCode, bool isImport, bool isExport)
		{
			if (tariffCode.Length == 14 && (isImport || isExport))
			{
				// hint is just in case (also is in validation fetch hint)
				foreach (int i in new int[] { 14, 10, 7, 4, 2 })
				{
					factory.AddFetchHint(typeof(NZCTariffsPermitsApplyTo), NZCTariffsPermitsApplyToSchema.U6_TariffPortion, tariffCode.Left(i));
				}

				foreach (int i in new int[] { 14, 10, 7, 4, 2 })
				{
					ZQuery query = new ZQuery(NZCTariffsPermitsApplyToSchema.U6_TariffPortion, tariffCode.Left(i));
					foreach (NZCTariffsPermitsApplyTo match in factory.Load<NZCTariffsPermitsApplyTo>(query))
					{
						if ((isImport && match.U6_AppliesToImport) || (isExport && match.U6_AppliesToExport))
						{
							return match;
						}
					}
				}
			}
			return null;
		}
		#endregion
	}
}
