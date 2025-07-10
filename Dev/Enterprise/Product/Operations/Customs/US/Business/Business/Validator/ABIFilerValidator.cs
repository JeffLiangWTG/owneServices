using System.Text.RegularExpressions;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class ABIFilerValidator
	{
		public static string Validate(ZString filer)
		{
			return IsValidFiler(filer) ? "" : ABIFilerRightFormat;
		}

		public static string ABIFilerRightFormat
		{
			get { return ResString.GetMultilingualString("CusInBond|6CE0A415-0E53-4670-840A-4AF4A04E98AA", "ABI Routing Code should be in the format, NNNNXXXNN where NNN is the Census Schedule D Code representing CBP port, XXX is the filer code and NN is the office code."); }
		}

		public static bool IsValidFiler(ZString filer)
		{
			return Regex.IsMatch(filer, @"^[0-9]{4}[A-Z0-9]{3}([0-9]{2})?$", RegexOptions.IgnoreCase);
		}
	}
}
