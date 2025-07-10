namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	using System;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.GUI.PlugIn;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Environment;
	using Enterprise.Licensing;

	public abstract class BaseMAFeBACCaDeclarationPlugIn : CustomsPlugIn
	{
		protected BaseMAFeBACCaDeclarationPlugIn(JobDeclaration declaration)
			: base(declaration)
		{
			Argument.NotNull(declaration, "JobDeclaration declaration");
			this.declaration = declaration;
			declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
			declaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
			ChangeTheVisibility();
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ImportQuarantineMessaging; }
		}

		public override string Name
		{
			get { return "eBACCa/IPI"; }
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = declaration.IsImport && !declaration.IsTSWDeclaration;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected readonly JobDeclaration declaration;
	}
}
