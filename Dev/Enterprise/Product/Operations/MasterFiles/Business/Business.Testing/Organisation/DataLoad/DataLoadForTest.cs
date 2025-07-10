using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DataLoadForTest : DataLoad
	{
		readonly List<string> result = new List<string>();

		public List<string> GetResult()
		{
			return result;
		}

		internal void UpdateAndDisplayIfRequiredExposed(Guid transPK, string table) => UpdateAndDisplayIfRequired(transPK, table);

		internal void OutputFinalTotalsExposed(string dataType) => OutputFinalTotals(dataType);

		public override string CSVTemplateHeading
		{
			get { return @"TEST1,TEST2,TEST3"; }
		}

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 3
				&& line.FieldValues[0].ToUpper() == "TEST1"
				&& line.FieldValues[1].ToUpper() == "TEST2"
				&& line.FieldValues[2].ToUpper() == "TEST3";
		}

		protected override void SetupBeforeImport()
		{
			base.SetupBeforeImport();
			VesselGuid = Factory.LoadFromNaturalKey(typeof(RefVessel), RefVesselSchema.RV_Code, "ADMIRALENGRACHT").PK;
			result.Clear();
		}

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			result.Add(line.ToString());
			UpdateAndDisplayIfRequired(Guid.Empty, "");
		}

		public ZGuid VesselGuid;
	}
}
