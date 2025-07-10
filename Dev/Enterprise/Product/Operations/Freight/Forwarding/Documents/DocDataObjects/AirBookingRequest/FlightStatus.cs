using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using ICodeDescription = Enterprise.DocumentVisualizer.DocDataObjects.ICodeDescription;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class FlightStatus : ICodeDescription
	{
		public FlightStatus(ICodeDescriptionPairList codes)
		{
			this.codes = Argument.NotNull(codes, nameof(codes));
		}

		readonly ICodeDescriptionPairList codes;

		#region Code

		public ZString Code
		{
			get => code;
			set
			{
				code = value;
				Description = codes.GetDescriptionFromCode(code);
			}
		}

		ZString code;

		#endregion

		#region Description

		public ZString Description
		{
			get; set;
		}

		#endregion

		#region Codes

		public object Codes => code;

		#endregion
	}
}
