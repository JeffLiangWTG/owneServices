using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business
{
	public static class ConsignmentHelper
	{
		#region GetInstructionDocAddressTypeToChangeTo

		public static DocAddressType GetInstructionDocAddressTypeToChangeTo(string instructionType, DocAddressType fallbackDocAddressType)
		{
			var docAddressType = fallbackDocAddressType;

			if (instructionType != InstructionTypes.Codes.Multi)
			{
				if (instructionType == InstructionTypes.Codes.PickUp)
				{
					docAddressType = DocAddressType.LocalCartageExporter;
				}
				else if (instructionType == InstructionTypes.Codes.Delivery)
				{
					docAddressType = DocAddressType.LocalCartageImporter;
				}
			}

			return docAddressType;
		}

		#endregion
	}
}
