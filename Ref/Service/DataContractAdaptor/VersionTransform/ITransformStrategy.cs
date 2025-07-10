using System;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public interface ITransformStrategy
	{
		T Transform<T>(T data);
		Type[] ToBeTransformedTypes { get; }
		bool RequireTransform(Type dataSetType, int version);
	}
}
