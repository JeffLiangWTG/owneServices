using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class GeneratePalletIDsHelper
	{
		public static void GenerateIDs(IPalletIDGenerator palletIDGenerator, WhsDocket docket, IEnumerable<WhsDocketLine> docketLines, Func<WhsDocketLine, bool> funcToValidate, string invalidLineMessage)
		{
			var validLines = GetValidLinesForPalletIDGeneration(docketLines, funcToValidate, invalidLineMessage).ToArray();

			if (validLines.Length > 0)
			{
				var ids = palletIDGenerator.GenerateIDs(docket, validLines.Length, shouldPrompt: true).ToArray();

				var idsIndex = 0;
				foreach (var line in validLines)
				{
					if (idsIndex >= ids.Length)
					{
						line.AddRowWarning(ErrorMessage_NoMoreIDs);
					}
					else
					{
						line.WE_PalletID = ids[idsIndex];
						idsIndex++;
					}
				}
			}
		}

		static IEnumerable<WhsDocketLine> GetValidLinesForPalletIDGeneration(IEnumerable<WhsDocketLine> lines, Func<WhsDocketLine, bool> funcToValidate, string invalidLineMessage)
		{
			foreach (var line in lines)
			{
				if (line.WE_PalletID.IsEmpty)
				{
					if (funcToValidate(line))
					{
						yield return line;
					}
					else
					{
						line.AddRowWarning(invalidLineMessage);
					}
				}
			}
		}

		public static ZString ErrorMessage_NoMoreIDs
		{
			get => Res.GetString(
				"4f0a08e1-3457-4622-a754-ca5be1d6965b",
				"The system has not generated a Pallet ID for this line because all Pallet ID's have been allocated. You must split this job to access more Pallet ID's.");
		}
	}
}
