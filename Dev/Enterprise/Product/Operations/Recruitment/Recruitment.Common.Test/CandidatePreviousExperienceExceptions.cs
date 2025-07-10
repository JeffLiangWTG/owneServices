using System;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruitment.Common;

namespace Enterprise.Recruitment.Testing
{
	sealed class CandidatePreviousExperienceExceptions<T> : Candidate where T : Exception, new()
	{
		public CandidatePreviousExperienceExceptions(BusinessObjectFactory factory, ZGuid applicationPk) : base(factory, applicationPk)
		{ }

		protected override ZString BuildPrevExpDetails(IXPathNavigable doc) => throw new T();

		public ZString GetPreviousExperienceDetails_Exposed() => GetPreviousExperienceDetails();
	}
}
