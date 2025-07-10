using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;

static class DeTariffsConstants
{
	public const string NationalCodeElementName = "N42000";
	public const string NationalCodeDescriptionElementName = "T40010";
	public const string VatCodeElementName = "N42020";
	public const string CreationDateElementName = "Erzeugungsdatum";
	public const string SetElementName = "Satz";
	public const string UpdateFlag = "u";
	public const string InsertFlag = "i";
	public const string DeleteFlag = "d";
	public const string DefaultVatCode = "KEI";
	public static readonly DateTime MinDate = new (2001,01,01);
}

