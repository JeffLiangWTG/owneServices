using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CIQRequirement
	{
		readonly char code;

		public CIQRequirement(char code)
		{
			this.code = code;
		}

		public string RequirementName
		{
			get
			{
				switch (code)
				{
					case 'M':
					case 'P':
					case 'R':
					case 'L':
					case 'V':
						return "ImportCIQRequirement";
					case 'N':
					case 'Q':
					case 'S':
					case 'W':
						return "ExportCIQRequirement";
					default:
						throw new NotSupportedException($"Not supported CIQRequirement {code}");
				}
			}
		}

		public char Code => code;

		public static IEnumerable<CIQRequirement> Extract(string codes)
		{
			return codes.Where(c => char.IsLetterOrDigit(c) && c != 'A').Select(c => new CIQRequirement(c));
		}
	}
}
