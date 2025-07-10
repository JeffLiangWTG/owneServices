using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityGoodsData
	{
		int ItemNumber { get; }
		int PackageCount { get; }
		string PackageCode { get; }
		string GoodsDescription { get; }
		decimal Kilograms { get; }
		decimal CubicMetres { get; }
		string MarksAndNumbers { get; }
		string HarmonisedCode { get; }
		string ContainerNumber { get; }
		IEnumerable<string> ContainerNumbers { get; }
		string ContainerYardAddress { get; }
	}
}
