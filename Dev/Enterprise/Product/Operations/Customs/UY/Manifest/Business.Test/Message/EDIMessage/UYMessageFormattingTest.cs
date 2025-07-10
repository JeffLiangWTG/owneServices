using System.IO;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYMessageFormattingTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageFormatXML()
		{
			var expectedresult = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DAERespuesta>
  <Respuestas>
    <Respuesta>
      <Tipo>A</Tipo>
      <Codigo>1450</Codigo>
      <Descripcion>Operacion Exitosa</Descripcion>
      <Ayuda>El alta del conocimiento 8AZ8493 fue exitosa.</Ayuda>
      <Referencias>
        <Referencia>
          <Codigo>Secuencia</Codigo>
          <Valor>46784836</Valor>
        </Referencia>
        <Referencia>
          <Codigo>TransporteTipo</Codigo>
          <Valor>4  </Valor>
        </Referencia>
        <Referencia>
          <Codigo>ManifiestoTipo</Codigo>
          <Valor>0</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ManifiestoNumero</Codigo>
          <Valor>LH8264     </Valor>
        </Referencia>
        <Referencia>
          <Codigo>Recinto</Codigo>
          <Valor>2081</Valor>
        </Referencia>
        <Referencia>
          <Codigo>FechaArribo</Codigo>
          <Valor>20200415</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ConocimientoNumeroSecuencial</Codigo>
          <Valor>1</Valor>
        </Referencia>
        <Referencia>
          <Codigo>RespuestaTipo</Codigo>
          <Valor>C</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ConocimientoNumeroDNA</Codigo>
          <Valor>7</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ConocimientoOriginalNumero</Codigo>
          <Valor>8AZ8493</Valor>
        </Referencia>
        <Referencia>
          <Codigo>MensajeNumeroDNA</Codigo>
          <Valor>5034365</Valor>
        </Referencia>
      </Referencias>
    </Respuesta>
    <Respuesta>
      <Tipo>A</Tipo>
      <Codigo>1450</Codigo>
      <Descripcion>Operacion Exitosa</Descripcion>
      <Ayuda>Alta linea exitosa.</Ayuda>
      <Referencias>
        <Referencia>
          <Codigo>Secuencia</Codigo>
          <Valor>46784837</Valor>
        </Referencia>
        <Referencia>
          <Codigo>TransporteTipo</Codigo>
          <Valor>4  </Valor>
        </Referencia>
        <Referencia>
          <Codigo>ManifiestoTipo</Codigo>
          <Valor>0</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ManifiestoNumero</Codigo>
          <Valor>LH8264     </Valor>
        </Referencia>
        <Referencia>
          <Codigo>Recinto</Codigo>
          <Valor>2081</Valor>
        </Referencia>
        <Referencia>
          <Codigo>FechaArribo</Codigo>
          <Valor>20200415</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ConocimientoNumeroSecuencial</Codigo>
          <Valor>1</Valor>
        </Referencia>
        <Referencia>
          <Codigo>LineaNumero</Codigo>
          <Valor>1</Valor>
        </Referencia>
        <Referencia>
          <Codigo>RespuestaTipo</Codigo>
          <Valor>L</Valor>
        </Referencia>
        <Referencia>
          <Codigo>ConocimientoOriginalNumero</Codigo>
          <Valor>8AZ8493</Valor>
        </Referencia>
        <Referencia>
          <Codigo>MensajeNumeroDNA</Codigo>
          <Valor>5034365</Valor>
        </Referencia>
      </Referencias>
    </Respuesta>
  </Respuestas>
</DAERespuesta>";

			var messageText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign));
			AssertMultilineASCIIEquals("Text from  UY Messages should be formatted like an xml message", expectedresult.Trim(), UYMessageFormatting.FormatWithXMLRepresentation(messageText));
		}
	}
}
