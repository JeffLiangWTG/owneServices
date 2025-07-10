using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationDocument : AutoHRJobApplicationDocument, IHRApplicationDocument
	{
		public HRJobApplicationDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HPD_Type = "RES";
		}

		[RelatedBusinessObject("Application")]
		public override ZGuid HPD_HP
		{
			get => base.HPD_HP;
			set => base.HPD_HP = value;
		}

		public HRJobApplication Application => Factory.Load<HRJobApplication>(HPD_HP);
	}
}
