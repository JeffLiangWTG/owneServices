using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Recruiter.AutomatedRejection
{
	public class DocRecruitmentAutoRejectionEmailParser : DocumentParser<EmailGenerationLookup, DocRecruitmentAutoRejectionEmail>
	{
		public DocRecruitmentAutoRejectionEmailParser()
			: this(new BusinessObjectFactory())
		{
		}

		public DocRecruitmentAutoRejectionEmailParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper => typeof(DocRecruitmentAutoRejectionEmail);
	}
}
