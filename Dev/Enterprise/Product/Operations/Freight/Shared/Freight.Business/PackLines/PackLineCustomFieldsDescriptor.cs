using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineCustomFieldsDescriptor : CustomFieldsDescriptor<PackLine>
	{
		protected override IEnumerable<CustomFieldInfo> GetCustomFieldsInfos()
		{
			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption,
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Hint,
				JobPackLinesSchema.JL_CustomAttrib1);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption,
				Env.Registry.Freight.PackLine.PackLineCustomAttribute2Hint,
				JobPackLinesSchema.JL_CustomAttrib2);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption,
				Env.Registry.Freight.PackLine.PackLineCustomAttribute3Hint,
				JobPackLinesSchema.JL_CustomAttrib3);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption,
				Env.Registry.Freight.PackLine.PackLineCustomAttribute4Hint,
				JobPackLinesSchema.JL_CustomAttrib4);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption,
				Env.Registry.Freight.PackLine.PackLineCustomDecimal1Hint,
				JobPackLinesSchema.JL_CustomDecimal1);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption,
				Env.Registry.Freight.PackLine.PackLineCustomDecimal2Hint,
				JobPackLinesSchema.JL_CustomDecimal2);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomDate1Caption,
				Env.Registry.Freight.PackLine.PackLineCustomDate1Hint,
				JobPackLinesSchema.JL_CustomDate1);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomDate2Caption,
				Env.Registry.Freight.PackLine.PackLineCustomDate2Hint,
				JobPackLinesSchema.JL_CustomDate2);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption,
				Env.Registry.Freight.PackLine.PackLineCustomFlag1Hint,
				JobPackLinesSchema.JL_CustomFlag1);

			yield return CustomFieldInfo.New(Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption,
				Env.Registry.Freight.PackLine.PackLineCustomFlag2Hint,
				JobPackLinesSchema.JL_CustomFlag2);
		}
	}
}
