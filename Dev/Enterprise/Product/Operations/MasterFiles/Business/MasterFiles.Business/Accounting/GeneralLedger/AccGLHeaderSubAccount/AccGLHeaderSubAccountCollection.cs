using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderSubAccountCollection : DependentBusinessObjectCollection<AccGLHeaderSubAccount, AccGLHeader>
	{
		public AccGLHeaderSubAccountCollection(AccGLHeader accGLHeader)
			: base(accGLHeader)
		{
		}

		protected override string FkColumnName
		{
			get
			{
				return AccGLHeaderSubAccountSchema.Constants.ASA_AG;
			}
		}

		protected override bool AllowSort => false;

		public override void Load()
		{
			base.Load();
			Resort();
		}

		public void Resort()
		{
			Sort("ASA_Sequence");
		}
	}
}
