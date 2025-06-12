using System.IO;
using System.Text;
using System.Xml;


// Copied from C:\dev\Enterprise\Product\Operations\Customs\ASYCUDA\ASYCUDA.Business\CAR\XmlToCar
// I cannot branch from $/Dev to $/eServcies, so copying instead.
// Please view original file and this file in KDiff to see changes. 
namespace CargoWise.eHub.Products.AsycudaCustoms.Common
{
	public class CWConvertXmlToCar
    {
        public static MemoryStream ConvertXmlToCar(XmlDocument xmlDoc)
        {
            string sourceXml = "";
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    xmlDoc.WriteTo(tx);
                    sourceXml = sw.ToString();                    
                }
            }
            string convertedToCar = ConvertXml2Car(sourceXml);
            var bytes = Encoding.UTF8.GetBytes(convertedToCar);
            return new MemoryStream(bytes, 0,bytes.Length, false, true);
        }

        public static string ConvertXml2Car(string sourceXml)
        {
            var xmlToVectorConverter = new ReadcarXmlToVector();
            var vectorToCarConverter = new VectorToCar();
            var vector = xmlToVectorConverter.XmlToVector(sourceXml);
            string result = "";
            using (var ms = new MemoryStream())
            {
                vectorToCarConverter.createCar(vector, ms);                
                ms.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                result = sr.ReadToEnd();
            }
            return result;
        }
	}
}