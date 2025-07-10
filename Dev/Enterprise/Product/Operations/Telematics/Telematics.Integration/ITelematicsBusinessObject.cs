using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.Integration
{
	public interface ITelematicsBusinessObject
	{
		string Code { get; }
		string DescriptionForInterface { get; }
		string DescriptionInEnglish { get; }
		string TypeIdentifier { get; }
	}
}
