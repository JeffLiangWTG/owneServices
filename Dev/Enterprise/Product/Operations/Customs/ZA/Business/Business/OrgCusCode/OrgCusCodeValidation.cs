using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
			this.parent = (OrgCusCode)parent;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "It works with Constants as well")]
		protected override void CheckOK_CustomsRegNo()
		{
			MandatoryValidation.CheckEntered(parent.OK_CustomsRegNoInfo);
			if (!parent.OK_CustomsRegNoInfo.HasErrors())
			{
				if (parent.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode
					|| parent.OK_CodeType == OrgCusCode.CodeTypes.SupplierCode)
				{
					if (parent.Organisation.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
					{
						if (!Regex.IsMatch(parent.OK_CustomsRegNo, "^[0-9]{8}$"))
						{
							parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("ABEDECC5-0A46-4882-A253-932BBB911410", ZAAgentCodeFormatError));
						}
						else
						{
							var lastDigit = ValidCheckDigitForZA(parent.OK_CustomsRegNo);
							var cusRegNo = parent.OK_CustomsRegNo[7].ToString();
							if (lastDigit != cusRegNo)
							{
								parent.OK_CustomsRegNoInfo.AddMessageError(IncorrectCheckDigit(lastDigit));
							}
						}
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.VATCode)
				{
					new SouthAfricanVATValidation().Validate(parent.OK_CustomsRegNoInfo);
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.AgentCode)
				{
					if (!Regex.IsMatch(parent.OK_CustomsRegNo, "^[0-9]{8}$"))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("ABEDECC5-0A46-4882-A253-932BBB911410", ZAAgentCodeFormatError));
					}
					else
					{
						var query = GetDuplicateQuery(parent.OK_RN_NKCodeCountry, OrgCusCode.CodeTypes.AgentCode, parent.OK_CustomsRegNo);
						var duplicatedCode = parent.Factory.Load<OrgCusCode>(query);
						if (duplicatedCode.Length > 1)
						{
							var otherOrg = duplicatedCode.FirstOrDefault(x => x.Organisation.OH_Code != parent.Organisation.OH_Code);
							parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("C9C28FA5-8CE7-4727-AB97-285099D41501", "Agent Code {0} can be found on the following Organization: {1}",
								parent.OK_CustomsRegNo, otherOrg?.Organisation.OH_Code));
						}
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter)
				{
					if (!Regex.IsMatch(parent.OK_CustomsRegNo, "^[A-z0-9]{1,8}$"))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("BD1C33DA-660D-4136-94BB-AD0CCA1E2BD1", ZACustomsApprovedExporterCodeFormatError));
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.SouthAfricaCodeTypes.IDNumber)
				{
					new SouthAfricanIDNumberValidation().Validate(parent.OK_CustomsRegNoInfo);
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.VGMRegistrationNumber)
				{
					if (!Regex.IsMatch(parent.OK_CustomsRegNo.ToLower(), @"^y(es)?$"))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("528a8744-972e-4fcc-8007-16975efbe803", "Approved Method 2 weighing parties for this country/region do not have an approval number. Enter \"Y\" or \"Yes\" to show this company’s address has an approved status."));
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.WarehouseControlledPremisesID)
				{
					var customsRegNo = parent.OK_CustomsRegNo;
					if (customsRegNo.Length == 11)
					{
						if (!CustomsOffices.ContainsCode(customsRegNo.Substring(0, 3)))
						{
							parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("EFDB2551-DBD3-4700-AF33-C3C6153AC365", "Characters 1-3 of Customs Controlled Premises Code – Warehouse must be a valid customs office code."));
						}
						if (CodesOfSecondPart.All(x => !string.Equals(x, customsRegNo.Substring(3, 3), StringComparison.OrdinalIgnoreCase)))
						{
							parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("A238A691-0266-4ECF-BC78-AB1D1F9A2000", "Characters 4-6 of Customs Controlled Premises Code – Warehouse can only be one of the following 'OS', 'SOS', 'VM', 'SVM'，'AM', 'VS', 'VMP', 'VMS', 'SWE'.Note that for ‘VM’, ‘OS’ & ‘VS’ the 3rd character must be a space."));
						}
						if (!customsRegNo.Substring(6).IsNumbersOnlyOrEmpty)
						{
							parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("C3F94301-3518-4D2B-A89E-AB4EBA8F3814", "Characters 7-11 of Customs Controlled Premises Code – Warehouse must be numeric."));
						}
					}
					else
					{
						parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("ED0A457B-354D-4810-8EEB-8DE37B4602E7", "The length of Customs Controlled Premises Code – Warehouse must be 11"));
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.TaxFileCode
					&& parent.Organisation?.CustomsCodes.Cast<OrgCusCode>().SingleOrDefault(
						x => x.CountryIs(Core.Constants.CountryCodes.SouthAfrica)
						&& x.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode
						&& x.OK_CustomsRegNo == ValidationConstants.Declaration.UnregisteredTraderCustomsCode) == null)
				{
					parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("F3A68D9F-245F-4FD7-A5B9-85DB71C2621E", "There should be a Customs Importer code of 70707070."));
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.TerminalControlledPremisesID && !Regex.IsMatch(parent.OK_CustomsRegNo, "^[A-z0-9]{1,17}$"))
				{
					parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("F8F74F1D-2409-4BC9-9E4A-2BEFB7D0B788", "A valid Customs Controlled Premises Code – Terminal is composed of between 1 and 17 alpha numeric characters."));
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.DepotControlledPremisesID && !Regex.IsMatch(parent.OK_CustomsRegNo, "^[A-z0-9]{1,17}$"))
				{
					parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("87765275-2D19-4315-8B9E-97A5E06EAD96", "A valid Customs Controlled Premises Code – Depot is composed of between 1 and 17 alpha numeric characters."));
				}
				else if (parent.OK_CodeType == OrgCusCode.CodeTypes.CommercialAndGovernmentEntity)
				{
					MandatoryValidation.CheckEntered(parent.OK_CustomsRegNoInfo);
					if (!CAGCodeIssuer.GetIsCAGCodeValid(parent) && !parent.OK_CodeTypeInfo.HasErrors() && !parent.OK_RN_NKCodeCountryInfo.HasErrors())
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("A18EBFAC-CC64-4211-845F-AB666D8348AB", "Commercial And Government Entity Code is not valid."));
					}
				}
				else if (parent.OK_CodeType == OrgCusCode.SouthAfricaCodeTypes.BGV)
				{
					ZDecimal amount;
					if (ZDecimal.TryParse(parent.OK_CustomsRegNo, out amount))
					{
						if (amount < 0m)
						{
							parent.OK_CustomsRegNoInfo.AddError(Res.GetString("B5644141-846D-441A-8FFF-08DC84BCA237", "Bond Guarantee Value cannot be less than zero."));
						}
					}
					else
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("85BDEF09-41AA-4BED-B34A-829C70B827B5", "Bond Guarantee Amount must be numeric."));
					}
				}
			}
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();
			if (parent.CountryIs(Core.Constants.CountryCodes.SouthAfrica)
				&& parent.OK_CodeType == OrgCusCode.SouthAfricaCodeTypes.BGV
				&& (!parent.Organisation?.CustomsCodes?.Cast<OrgCusCode>()?.Any(
						x => x.CountryIs(Core.Constants.CountryCodes.SouthAfrica)
						&& (x.OK_CodeType == OrgCusCode.CodeTypes.BondHolderCode || x.OK_CodeType == OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode)) ?? true))
			{
				parent.OK_CodeTypeInfo.AddError(Res.GetString("2DB1381C-D9D4-4BBB-B454-15CA96FA28B3", "Bond Guarantee Value only applicable if a Bond Holder (BHR) or Remover (REM) Code has been captured."));
			}
		}

		public const string ZAAgentCodeFormatError = @"Agent Code should be 8 digits.";
		public const string ZACustomsApprovedExporterCodeFormatError = @"A valid Customs Approved Exporter Code is composed of between 1 and 8 alpha numeric characters.";
		public static string IncorrectCheckDigit(ZString lastDigit)
		{
			return Res.GetString("0E22CC96-ED66-44CA-B1BE-846421A9424A", "Incorrect Check Digit. Last digit should be '{0}'", lastDigit);
		}

		internal ZString ValidCheckDigitForZA(ZString regNo)
		{
			var totalCalculation =
					int.Parse(regNo[0].ToString(), CultureInfo.CurrentCulture) * 9 +
					int.Parse(regNo[1].ToString(), CultureInfo.CurrentCulture) * 8 +
					int.Parse(regNo[2].ToString(), CultureInfo.CurrentCulture) * 7 +
					int.Parse(regNo[3].ToString(), CultureInfo.CurrentCulture) * 6 +
					int.Parse(regNo[4].ToString(), CultureInfo.CurrentCulture) * 4 +
					int.Parse(regNo[5].ToString(), CultureInfo.CurrentCulture) * 3 +
					int.Parse(regNo[6].ToString(), CultureInfo.CurrentCulture) * 2;

			var z = 11 - (totalCalculation % 11);
			if (z == 11)
			{
				z = 0;
			}
			else if (z == 10)
			{
				z = (10 - totalCalculation % 10) % 10;
			}

			return z.ToString(CultureInfo.CurrentCulture);
		}

		readonly OrgCusCode parent;

		CodeDescriptionPairList CustomsOffices => ZARefCusCodeListTypes.GetCustomsOfficeList(parent.Factory);

		static IEnumerable<string> CodesOfSecondPart
		{
			get
			{
				yield return "OS ";
				yield return "SOS";
				yield return "VM ";
				yield return "SVM";
				yield return "AM ";
				yield return "VS ";
				yield return "VMP";
				yield return "VMS";
				yield return "SWE";
			}
		}
	}
}
