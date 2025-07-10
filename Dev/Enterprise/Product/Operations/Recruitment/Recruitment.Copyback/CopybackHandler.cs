using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Recruitment.Copyback
{
	public class CopybackHandler
	{
		public CopybackHandler()
		{
			Processor = ObjectFactory.Get<IColumnCopybackProcessor>();
		}

		readonly IColumnCopybackProcessor Processor;
		const char LogReferenceDelimiter = '_';

		Action<BusinessObjectFactory, GlbStaff, IQueuedLog> GetCopybackFunction(string code)
		{
			switch (code)
			{
				case "GSW":
					return Processor.StaffWorkingBasis;
				case "GEH":
					return Processor.EmploymentHistory;
				case "GWP":
					return Processor.WorkPattern;
				case "GSM":
					return Processor.StaffManager;
				default:
					throw new ArgumentException($"{code} doesn't have a copyback function implemented.", nameof(code));
			}
		}

		public void Copyback(BusinessObjectFactory factory, IQueuedLog log)
		{
			var staff = log.Factory.Load<GlbStaff>(log.ParentID);
			var tableCode = log.Reference.Split(LogReferenceDelimiter)[0];
			GetCopybackFunction(tableCode).Invoke(factory, staff, log);
		}
	}
}
