using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FilterBusinessObjectWithProblems : FilterBusinessObject
	{
		public FilterBusinessObjectWithProblems(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZQuery Filter
		{
			get { throw new ArgumentException("Error"); }
		}
	}
}
