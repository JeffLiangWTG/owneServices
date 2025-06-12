package cargowise.eadaptorsamplewebclient;

import java.io.FileInputStream;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;

public class eAdaptorSampleWebClient {
	public static boolean ping(String serviceAddress, String senderId, String password) throws Exception {
		eHubAdapter adapter = new eHubAdapter(serviceAddress, senderId, password);
		boolean result = adapter.ping();
		adapter.close();
		return result;
	}
	
	public static void sendMessage(
		String serviceAddress,
		String messageFilePath,
		String recipientId,
		String senderId,
		String password
	) throws Exception {
		String messageNamespace = getMessageNamespace(messageFilePath);
		
		try (Stream fileStream = new Stream(new FileInputStream(messageFilePath))) {
			eHubAdapter adapter = new eHubAdapter(serviceAddress, senderId, password);	
			adapter.getOutbox().addMessage(new eHubMessage(
				Guid.newGuid(),
				senderId,
				recipientId,
				MessageSchemaType.Xml,
				getApplicationCode(messageNamespace),
				getSchemaName(messageNamespace),
				fileStream,
				"Test email subject",
				"Test file name"
			));
			adapter.sendMessages();
			adapter.close();
		}
	}
	
	private static String getMessageNamespace(String messageFilePath) throws Exception {
		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		factory.setNamespaceAware(true);
		
		String result = "";
		try (Stream fileStream = new Stream(new FileInputStream(messageFilePath))) {
			Document document = factory.newDocumentBuilder().parse(fileStream);
			NodeList nodeList = document.getDocumentElement().getChildNodes();
			for (int i = 0; i < nodeList.getLength(); i++) {
				Node node = nodeList.item(i);
				if (node.getNodeType() == Node.ELEMENT_NODE) {
					result = node.getNamespaceURI();
					break;
				}
			}
		}
		
		return result;
	}
	
	private static String getApplicationCode(String messageNamespace) {
		switch (messageNamespace) {
			case "http://www.cargowise.com/Schemas/Universal":
			case "http://www.cargowise.com/Schemas/Universal/2011/11":
				return "UDM";

			case "http://www.cargowise.com/Schemas/Native":
				return "NDM";

			case "http://www.edi.com.au/EnterpriseService/":
				return "XMS";

			default: return "";
		}
	}
	
	private static String getSchemaName(String messageNamespace) {
		switch (messageNamespace) {
			case "http://www.cargowise.com/Schemas/Native":
			case "http://www.cargowise.com/Schemas/Universal":
			case "http://www.cargowise.com/Schemas/Universal/2011/11":
				return messageNamespace + "#UniversalInterchange";

			case "http://www.edi.com.au/EnterpriseService/":
				return messageNamespace + "#XmlInterchange";

			default:
				return messageNamespace;
		}
	}
}
