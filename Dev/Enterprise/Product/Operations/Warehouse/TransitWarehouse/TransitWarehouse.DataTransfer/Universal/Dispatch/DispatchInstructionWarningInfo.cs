using System.Collections.Generic;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class DispatchInstructionWarningInfo
	{
		public DispatchInstructionWarningInfo()
		{
			DCNIDsWithPackageMatchingDiscrepancy = new HashSet<string>();
			DCNIDsCanNotBeCreated = new HashSet<string>();
		}
		public HashSet<string> DCNIDsWithPackageMatchingDiscrepancy { get; set; }

		public HashSet<string> DCNIDsCanNotBeCreated { get; set; }
	}
}
