
namespace Enterprise.Freight.Agency.Business
{
	using Enterprise.ZArchitecture.Core;

	public class PortMessageTargetPort : CodeDescriptionPair
	{
		public PortMessageTargetPort(string code, PortDirections directions)
			: base(code, "")
		{
			this.directions = directions;
		}

		public PortDirections Directions
		{
			get { return directions; }
		}

		readonly PortDirections directions;
	}
}
