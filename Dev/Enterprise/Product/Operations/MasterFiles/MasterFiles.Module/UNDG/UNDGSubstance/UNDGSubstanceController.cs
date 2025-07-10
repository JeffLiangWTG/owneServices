using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGSubstanceController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Module Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.UNDGSubstance; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.UNDGSubstance; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UNDGSubstance); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var substance = (UNDGSubstance)businessEntity;
			if (substance == null)
			{
				return new UNDGSubstanceForm(substance);
			}

			switch (substance.StandardSubstance)
			{
				case UNDGSubstanceADR standardSubstance:
					return new UNDGSubstanceADRForm(standardSubstance);

				case UNDGSubstanceRID standardSubstance:
					return new UNDGSubstanceRIDForm(standardSubstance);

				case UNDGSubstanceADN standardSubstance:
					return new UNDGSubstanceADNForm(standardSubstance);

				case UNDGSubstanceCFR standardSubstance:
					return new UNDGSubstanceCFRForm(standardSubstance);

				case UNDGSubstanceJTT standardSubstance:
					return new UNDGSubstanceJTTForm(standardSubstance);
			}

			return GetZZUNDGSubstanceForm(substance);
		}

		IZForm GetZZUNDGSubstanceForm(UNDGSubstance substance)
		{
			if (substance.DG_Standard == UNDGSubstanceStandardTypes.IATA)
			{
				return new UNDGSubstanceIATAForm(substance);
			}

			return new UNDGSubstanceForm(substance);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.UNDGSubstanceDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.UNDGSubstanceEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.UNDGSubstanceNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.UNDGSubstance; }
		}

		#endregion
	}
}
