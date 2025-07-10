using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFEquipValidation : AutoCusISFEquipValidation
	{
		public CusISFEquipValidation(AutoCusISFEquip parent)
			: base(parent)
		{
		}

		protected new CusISFEquip Parent
		{
			get { return (CusISFEquip)base.Parent; }
		}

		protected CusISFHeader header
		{
			get { return Parent.Header; }
		}

		protected override void CheckBE_EquipCode()
		{
			base.CheckBE_EquipCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BE_EquipCodeInfo);
		}

		protected override void CheckBE_ContainerNum()
		{
			base.CheckBE_ContainerNum();
			bool warningNotError = !SendEquipment;

			CheckContainerNoHasValidCharactersOnly(warningNotError);
			if (!Parent.BE_ContainerNumInfo.HasMessageErrors())
			{
				CheckContainerNoHasValidCheckDigit();
			}

			CheckNoDuplicateContainerNumber(warningNotError);
		}

		bool SendEquipment
		{
			get
			{
				bool sendEquipment = true;
				if (header != null)
				{
					switch (header.BF_SendEquipment)
					{
						case YesNoDefaultList.Codes.No:
							sendEquipment = false;
							break;
						case YesNoDefaultList.Codes.Default:
							sendEquipment = ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.GetFallBackValueAtAllLevels(header.RegistryCompanyPK, header.RegistryBranchPK, Guid.Empty);
							break;
					}
				}

				return sendEquipment;
			}
		}

		void CheckNoDuplicateContainerNumber(bool warningNotError)
		{
			if (header != null && !Parent.BE_ContainerNum.IsEmpty)
			{
				foreach (CusISFEquip otherEquipment in header.Equipments)
				{
					if (otherEquipment != Parent && otherEquipment.BE_ContainerNum == Parent.BE_ContainerNum)
					{
						if (warningNotError)
						{
							Parent.BE_ContainerNumInfo.AddWarning(ContainerNumberAlreadyExists);
						}
						else
						{
							Parent.BE_ContainerNumInfo.AddError(ContainerNumberAlreadyExists);
						}
					}
				}
			}
		}
		internal const string ContainerNumberAlreadyExists = "There is already another record with the same 'Container Number'.";

		public const string MustOnlyContainAlphaNumerics = "Invalid Characters In Container Number - Container number must only contain alphanumeric characters.";
		void CheckContainerNoHasValidCharactersOnly(bool warningNotError)
		{
			if (Parent.BE_ContainerNum.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") != Parent.BE_ContainerNum)
			{
				if (warningNotError)
				{
					Parent.BE_ContainerNumInfo.AddWarning(MustOnlyContainAlphaNumerics);
				}
				else
				{
					Parent.BE_ContainerNumInfo.AddMessageError(MustOnlyContainAlphaNumerics);
				}
			}
		}

		void CheckContainerNoHasValidCheckDigit()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.BE_ContainerNumInfo);
			ContainerNumberValidation.WarnIfInvalid(Parent.BE_ContainerNumInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Retained for TODO")]
		protected override void CheckBE_ContainerISO()
		{
			base.CheckBE_ContainerISO();
			// TODO: validate ContainerISO???
		}
	}
}
