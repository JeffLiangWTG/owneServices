using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Mexico
{
	public interface IMexicoEInvoicingExtension
	{
		CodeDescriptionPairList GetUsosCFDI();
	}

	public class MexicoEInvoicingExtension : IMexicoEInvoicingExtension
	{
		#region Debtor Number descriptions

		CodeDescriptionPairList IMexicoEInvoicingExtension.GetUsosCFDI()
		{
			var mexico_usosCFDI = new CodeDescriptionPairList();

			#region SuppressResourceStringsCheckRegion

			mexico_usosCFDI.AddPair("G01", "Adquisición de mercancías");
			mexico_usosCFDI.AddPair("G02", "Devoluciones, descuentos o bonificaciones");
			mexico_usosCFDI.AddPair("G03", "Gastos en general");
			mexico_usosCFDI.AddPair("I01", "Construcciones");
			mexico_usosCFDI.AddPair("I02", "Mobilario y equipo de oficina por inversiones");
			mexico_usosCFDI.AddPair("I03", "Equipo de transporte");
			mexico_usosCFDI.AddPair("I04", "Equipo de computo y accesorios");
			mexico_usosCFDI.AddPair("I05", "Dados, troqueles, moldes, matrices y herramental");
			mexico_usosCFDI.AddPair("I06", "Comunicaciones telefónicas");
			mexico_usosCFDI.AddPair("I07", "Comunicaciones satelitales");
			mexico_usosCFDI.AddPair("I08", "Otra maquinaria y equipo");
			mexico_usosCFDI.AddPair("D01", "Honorarios médicos, dentales y gastos hospitalarios");
			mexico_usosCFDI.AddPair("D02", "Gastos médicos por incapacidad o discapacidad");
			mexico_usosCFDI.AddPair("D03", "Gastos funerales");
			mexico_usosCFDI.AddPair("D04", "Donativos");
			mexico_usosCFDI.AddPair("D05", "Intereses reales efectivamente pagados por créditos hipotecarios (casa habitación).");
			mexico_usosCFDI.AddPair("D06", "Aportaciones voluntarias al SAR");
			mexico_usosCFDI.AddPair("D07", "Primas por seguros de gastos médicos");
			mexico_usosCFDI.AddPair("D08", "Gastos de transportación escolar obligatoria");
			mexico_usosCFDI.AddPair("D09", "Depósitos en cuentas para el ahorro, primas que tengan como base planes de pensiones");
			mexico_usosCFDI.AddPair("D10", "Pagos por servicios educativos (colegiaturas)");
			mexico_usosCFDI.AddPair("S01", "Sin efectos fiscales");
			mexico_usosCFDI.AddPair("CP01", "Pagos");
			mexico_usosCFDI.AddPair("CN01", "Nómina");

			#endregion

			return mexico_usosCFDI;
		}

		#endregion
	}
}
