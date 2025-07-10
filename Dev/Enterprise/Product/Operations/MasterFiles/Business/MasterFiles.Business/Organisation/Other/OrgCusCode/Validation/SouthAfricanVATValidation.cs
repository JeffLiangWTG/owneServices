using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for SouthAfricanVATValidation.
	/// </summary>
	public class SouthAfricanVATValidation
	{
		public void Validate(ZPropertyInfo vATInfo)
		{
			ZString vAT = ((ZString)vATInfo.Value);
			if (vAT.Length == 10)
			{
				if (!vAT.IsNumbersOnlyOrEmpty)
				{
					vATInfo.AddWarning(Res.GetString("CB23474E-9322-483E-BE33-FA031738FE65", "The VAT must be 10 digits only"));
				}
				else if (vAT[0] != '4')
				{
					vATInfo.AddWarning(Res.GetString("89ea5890-52bd-424f-9a00-b0365b8e0d7d", "The VAT must start with a '4'"));
				}
				else if (!IsValidCheckDigit(vAT))
				{
					vATInfo.AddWarning(Res.GetString("7a6be71d-97fd-41a7-8eb2-88ab4579b3e1", "The VAT is invalid"));
				}
			}
			else if (vAT != "NA")
			{
				vATInfo.AddWarning(Res.GetString("74816c0c-794f-4ac2-82d1-20f467d59c01", "The VAT must be 10 digits long"));
			}
		}

		#region Implementation

		protected internal bool IsValidCheckDigit(ZString vAT)
		{
			int vatTot = 0;
			for (int i = 1; i < 10; i += 2)
			{
				vatTot += int.Parse(vAT[i].ToString());
			}
			for (int i = 0; i < 9; i += 2)
			{
				int tempVat = int.Parse(vAT[i].ToString()) * 2;
				if (tempVat > 9)
				{
					tempVat -= 9;
				}

				vatTot += tempVat;
			}
			return vatTot % 10 == 0;
		}

		#endregion
	}
}
