using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform;

#if DEBUG
/// <summary>
/// This is a sample to tranform by SRDbVersion.
/// </summary>
public class SV564Transform : ITransformStrategy
{
	SV564Transform() { }

	static SV564Transform transform;

	public static SV564Transform Instance() => transform ??= new SV564Transform();

	public Type[] ToBeTransformedTypes => [typeof(RefCusCodeList)];

	public bool RequireTransform(Type dataSetType, int version)
		=> version < 564 && ToBeTransformedTypes.Contains(dataSetType);

	public T Transform<T>(T data)
	{
		if (data is RefCusCodeList codeList)
		{
			codeList.ZZD_Description += "_ForTest";
		}
		return data;
	}
}
#endif
