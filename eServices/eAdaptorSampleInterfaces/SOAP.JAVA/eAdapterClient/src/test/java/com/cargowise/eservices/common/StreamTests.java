package com.cargowise.eservices.common;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;

import javax.xml.soap.MessageFactory;
import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class StreamTests extends TestCase {

	@Test
	public void testDecodeAndDecompressGZipStream() throws IOException {
		try (Stream sourceStream = new Stream(StreamTests.class.getResourceAsStream("PSAFlatInputSmall.txt"))) {
			String sourceString = sourceStream.readToEnd();
			Stream gZipStream = sourceStream.compressAndEncode();
			Stream decompressGZipStream = gZipStream.decodeAndDecompress();
			String decompressString = decompressGZipStream.readToEnd();
			assertEquals(sourceString, decompressString);
			sourceStream.close();
		}
	}

	@Test
	public void testDecodeAndDecompressZipStream() throws IOException {
		try (InputStream resourceStream =  StreamTests.class.getResourceAsStream("dat.zip");
				Stream stream = new Stream(resourceStream);
				Stream sourceStream = stream.encodeStream()) {
			String expected = new StringBuilder().append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>\n")
					.append("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.0\" >\n")
					.append("  <Header>\n")
					.append("    <SenderID>cevtst</SenderID>\n")
					.append("    <RecipientID>CEVHOUTST</RecipientID>\n")
					.append("  </Header>\n")
					.append("  <Body>\n")
					.append("    <Native xmlns=\"http://www.cargowise.com/Schemas/Native/2011/11\" version=\"2.0\" >\n")
					.append("      <Body>\n")
					.append("        <Product version=\"2.0\" >\n")
					.append("          <OrgSupplierPart Action=\"MERGE\" >\n")
					.append("            <PartNum>EATX0814089-3</PartNum>\n")
					.append("            <NetWeight>0.001</NetWeight>\n")
					.append("            <Desc>INSERTER BLOCK X0814089 15MM X 8DEG</Desc>\n")
					.append("            <CusClassPartPivotCollection>\n")
					.append("              <CusClassPartPivot Action=\"MERGE\" >\n")
					.append("                <TariffNum>8542.33.0000</TariffNum>\n")
					.append("                <ChildType>HTI</ChildType>\n")
					.append("                <DateStart>2014-01-01T00:00:00</DateStart>\n")
					.append("                <DateEnd>2014-12-31T00:00:00</DateEnd>\n")
					.append("                <Country>\n")
					.append("                  <Code>US</Code>\n")
					.append("                </Country>\n")
					.append("                <ComponentCusClassPartPivotCollection>\n")
					.append("                  <ComponentCusClassPartPivot Action=\"MERGE\" >\n")
					.append("                    <TariffNum>8542.33.0000</TariffNum>\n")
					.append("                    <ChildType>COM</ChildType>\n")
					.append("                    <ChildListOrder>1</ChildListOrder>\n")
					.append("                    <Country>\n")
					.append("                      <Code>US</Code>\n")
					.append("                    </Country>\n")
					.append("                  </ComponentCusClassPartPivot>\n")
					.append("                </ComponentCusClassPartPivotCollection>\n")
					.append("                <AdditionalInformationChildCollection>\n")
					.append("                  <AdditionalInformationChild Action=\"MERGE\" >\n")
					.append("                    <Type>USA</Type>\n")
					.append("                    <AddInfoData>FDAProductCode=016FDX0</AddInfoData>\n")
					.append("                  </AdditionalInformationChild>\n")
					.append("                </AdditionalInformationChildCollection>\n")
					.append("                <CusUSClassificationCollection>\n")
					.append("                  <CusUSClassification Action=\"MERGE\" >\n")
					.append("                    <SPI>MX</SPI>\n")
					.append("                    <TSCAIndicator>+</TSCAIndicator>\n")
					.append("                    <USCCountryOfOrigin TableName=\"RefDbEntUS_USCCountry\" >\n")
					.append("                      <Code>US</Code>\n")
					.append("                    </USCCountryOfOrigin>\n")
					.append("                  </CusUSClassification>\n")
					.append("                </CusUSClassificationCollection>\n")
					.append("              </CusClassPartPivot>\n")
					.append("            </CusClassPartPivotCollection>\n")
					.append("            <OrgPartRelationCollection>\n")
					.append("              <OrgPartRelation Action=\"MERGE\" >\n")
					.append("                <Relationship>OWN</Relationship>\n")
					.append("                <OrgHeader>\n")
					.append("                  <Code>738006</Code>\n")
					.append("                </OrgHeader>\n")
					.append("              </OrgPartRelation>\n")
					.append("            </OrgPartRelationCollection>\n")
					.append("          </OrgSupplierPart>\n")
					.append("        </Product>\n")
					.append("      </Body>\n")
					.append("    </Native>\n")
					.append("  </Body>\n")
					.append("</UniversalInterchange>\n\n").toString();

			Stream decompressedZipStream = sourceStream.decodeAndDecompress();
			String actual = decompressedZipStream.readToEnd();
			assertEquals(expected.replace("\r", ""), actual.replace("\r", ""));
		}
	}

	@Test
	public void testWriteStreamToXML() throws SOAPException, IOException {
		MessageFactory messageFactory = MessageFactory.newInstance();
		SOAPMessage message = messageFactory.createMessage();
		SOAPElement element = message.getSOAPPart().getEnvelope().getBody().addChildElement("test");
		try (Stream sourceStream = new Stream(StreamTests.class.getResourceAsStream("BLLIST.xml"))) {
			sourceStream.compressAndEncode().copyTo(element);
			try (Stream resultStream = new Stream(new ByteArrayInputStream(element.getValue().getBytes(StandardCharsets.UTF_8)))) {
				sourceStream.reset();
				assertEquals(sourceStream.compressAndEncode().toString(), element.getValue());
				sourceStream.reset();
				assertEquals(sourceStream.compressAndEncode().readToEnd(), resultStream.readToEnd());
			}
		}
	}

	@Test
	public void testCompressAndEncode_DecodeAndDecompress_ResetStreamMultipleTimes_LargeFile() throws IOException {
		try (Stream sourceStream = new Stream(StreamTests.class.getResourceAsStream("BLLIST.xml"))) {
			String expected = sourceStream.readToEnd();
			String actual = sourceStream.compressAndEncode().decodeAndDecompress().readToEnd();
			assertEquals(820601, expected.length());
			assertEquals(expected, actual);
			sourceStream.reset();
			expected = sourceStream.readToEnd();
			assertEquals(820601, expected.length());
			assertEquals(expected, sourceStream.compressAndEncode().decodeAndDecompress().readToEnd());
			sourceStream.close();
		}
	}
}
