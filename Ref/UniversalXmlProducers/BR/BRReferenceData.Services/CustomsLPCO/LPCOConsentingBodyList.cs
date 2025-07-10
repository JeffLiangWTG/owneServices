using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class LPCOConsentingBodyList
	{
		public const string ABGF = "ABGF";
		public const string ANATEL = "ANATEL";
		public const string ANCINE = "ANCINE";
		public const string ANEEL = "ANEEL";
		public const string ANP = "ANP";
		public const string ANVISA = "ANVISA";
		public const string BB = "BB";
		public const string BEFIEX = "BEFIEX";
		public const string BNDES = "BNDES";
		public const string CNEN = "CNEN";
		public const string CNPQ = "CNPQ";
		public const string CONFAZ = "CONFAZ";
		public const string COTAC = "COTAC";
		public const string DEAEX = "DEAEX";
		public const string DECEX = "DECEX";
		public const string DEPLA = "DEPLA";
		public const string DFPC = "DFPC";
		public const string DNPM = "DNPM";
		public const string DPF = "DPF";
		public const string ECT = "ECT";
		public const string GESTOR = "GESTOR";
		public const string IBAMA = "IBAMA";
		public const string INMETRO = "INMETRO";
		public const string IPHAN = "IPHAN";
		public const string MAPA = "MAPA";
		public const string MCT = "MCT";
		public const string MIN_DEFESA = "MIN.DEFESA";
		public const string MRE = "MRE";
		public const string RECEITA = "RECEITA";
		public const string SDAVO = "SDAVO";
		public const string SE_CAMEX = "SE-CAMEX";
		public const string SECEX = "SECEX";
		public const string SEPIN = "SEPIN";
		public const string SPC_MA = "SPC-MA";
		public const string SUFRAMA = "SUFRAMA";

		public ICollection<string> ConsentingBodyList => consentingBodyList ?? (consentingBodyList = ToArray);
		ICollection<string> consentingBodyList;

		ICollection<string> ToArray => GetType().GetFields().Select(field => field.GetValue(this) as string).ToList();
	}
}
