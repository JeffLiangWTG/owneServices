
package com.cargowise.eservices.canadian.encryption.client;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlElementDecl;
import javax.xml.bind.annotation.XmlRegistry;
import javax.xml.namespace.QName;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the com.cargowise.eservices.canadian.encryption.client package. 
 * <p>An ObjectFactory allows you to programatically 
 * construct new instances of the Java representation 
 * for XML content. The Java representation of XML 
 * content can consist of schema derived interfaces 
 * and classes representing the binding of schema 
 * type definitions, element declarations and model 
 * groups.  Factory methods for each of these are 
 * provided in this class.
 * 
 */
@XmlRegistry
public class ObjectFactory {

    private final static QName _EncryptContentResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptContentResponse");
    private final static QName _EncryptMessageResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMessageResponse");
    private final static QName _Decrypt_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "decrypt");
    private final static QName _EncryptMdnContent_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMdnContent");
    private final static QName _DecryptContent_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "decryptContent");
    private final static QName _EncryptMdnResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMdnResponse");
    private final static QName _EncryptMessage_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMessage");
    private final static QName _Exception_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "Exception");
    private final static QName _EncryptContent_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptContent");
    private final static QName _DecryptResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "decryptResponse");
    private final static QName _EncryptMdn_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMdn");
    private final static QName _EncryptMdnContentResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "encryptMdnContentResponse");
    private final static QName _DecryptContentResponse_QNAME = new QName("http://cargowise.com/eservices/CanadianEncryptionService", "decryptContentResponse");
    private final static QName _EncryptMdnResponseReturn_QNAME = new QName("", "return");
    private final static QName _DecryptContentEncodedData_QNAME = new QName("", "encodedData");

    /**
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: com.cargowise.eservices.canadian.encryption.client
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link EncryptContent }
     * 
     */
    public EncryptContent createEncryptContent() { // NO_UCD (unused code)
        return new EncryptContent();
    }

    /**
     * Create an instance of {@link DecryptResponse }
     * 
     */
    public DecryptResponse createDecryptResponse() { // NO_UCD (unused code)
        return new DecryptResponse();
    }

    /**
     * Create an instance of {@link EncryptMdn }
     * 
     */
    public EncryptMdn createEncryptMdn() { // NO_UCD (unused code)
        return new EncryptMdn();
    }

    /**
     * Create an instance of {@link EncryptMdnContentResponse }
     * 
     */
    public EncryptMdnContentResponse createEncryptMdnContentResponse() { // NO_UCD (unused code)
        return new EncryptMdnContentResponse();
    }

    /**
     * Create an instance of {@link DecryptContentResponse }
     * 
     */
    public DecryptContentResponse createDecryptContentResponse() { // NO_UCD (unused code)
        return new DecryptContentResponse();
    }

    /**
     * Create an instance of {@link EncryptMessage }
     * 
     */
    public EncryptMessage createEncryptMessage() { // NO_UCD (unused code)
        return new EncryptMessage();
    }

    /**
     * Create an instance of {@link Exception }
     * 
     */
    public Exception createException() { // NO_UCD (unused code)
        return new Exception();
    }

    /**
     * Create an instance of {@link EncryptMdnResponse }
     * 
     */
    public EncryptMdnResponse createEncryptMdnResponse() { // NO_UCD (unused code)
        return new EncryptMdnResponse();
    }

    /**
     * Create an instance of {@link DecryptContent }
     * 
     */
    public DecryptContent createDecryptContent() { // NO_UCD (unused code)
        return new DecryptContent();
    }

    /**
     * Create an instance of {@link EncryptContentResponse }
     * 
     */
    public EncryptContentResponse createEncryptContentResponse() { // NO_UCD (unused code)
        return new EncryptContentResponse();
    }

    /**
     * Create an instance of {@link Decrypt }
     * 
     */
    public Decrypt createDecrypt() { // NO_UCD (unused code)
        return new Decrypt();
    }

    /**
     * Create an instance of {@link EncryptMdnContent }
     * 
     */
    public EncryptMdnContent createEncryptMdnContent() { // NO_UCD (unused code)
        return new EncryptMdnContent();
    }

    /**
     * Create an instance of {@link EncryptMessageResponse }
     * 
     */
    public EncryptMessageResponse createEncryptMessageResponse() { // NO_UCD (unused code)
        return new EncryptMessageResponse();
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptContentResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptContentResponse")
    public JAXBElement<EncryptContentResponse> createEncryptContentResponse(EncryptContentResponse value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptContentResponse>(_EncryptContentResponse_QNAME, EncryptContentResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMessageResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMessageResponse")
    public JAXBElement<EncryptMessageResponse> createEncryptMessageResponse(EncryptMessageResponse value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMessageResponse>(_EncryptMessageResponse_QNAME, EncryptMessageResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link Decrypt }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "decrypt")
    public JAXBElement<Decrypt> createDecrypt(Decrypt value) { // NO_UCD (unused code)
        return new JAXBElement<Decrypt>(_Decrypt_QNAME, Decrypt.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMdnContent }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMdnContent")
    public JAXBElement<EncryptMdnContent> createEncryptMdnContent(EncryptMdnContent value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMdnContent>(_EncryptMdnContent_QNAME, EncryptMdnContent.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link DecryptContent }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "decryptContent")
    public JAXBElement<DecryptContent> createDecryptContent(DecryptContent value) { // NO_UCD (unused code)
        return new JAXBElement<DecryptContent>(_DecryptContent_QNAME, DecryptContent.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMdnResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMdnResponse")
    public JAXBElement<EncryptMdnResponse> createEncryptMdnResponse(EncryptMdnResponse value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMdnResponse>(_EncryptMdnResponse_QNAME, EncryptMdnResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMessage }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMessage")
    public JAXBElement<EncryptMessage> createEncryptMessage(EncryptMessage value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMessage>(_EncryptMessage_QNAME, EncryptMessage.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link Exception }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "Exception")
    public JAXBElement<Exception> createException(Exception value) { // NO_UCD (unused code)
        return new JAXBElement<Exception>(_Exception_QNAME, Exception.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptContent }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptContent")
    public JAXBElement<EncryptContent> createEncryptContent(EncryptContent value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptContent>(_EncryptContent_QNAME, EncryptContent.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link DecryptResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "decryptResponse")
    public JAXBElement<DecryptResponse> createDecryptResponse(DecryptResponse value) { // NO_UCD (unused code)
        return new JAXBElement<DecryptResponse>(_DecryptResponse_QNAME, DecryptResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMdn }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMdn")
    public JAXBElement<EncryptMdn> createEncryptMdn(EncryptMdn value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMdn>(_EncryptMdn_QNAME, EncryptMdn.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link EncryptMdnContentResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "encryptMdnContentResponse")
    public JAXBElement<EncryptMdnContentResponse> createEncryptMdnContentResponse(EncryptMdnContentResponse value) { // NO_UCD (unused code)
        return new JAXBElement<EncryptMdnContentResponse>(_EncryptMdnContentResponse_QNAME, EncryptMdnContentResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link DecryptContentResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://cargowise.com/eservices/CanadianEncryptionService", name = "decryptContentResponse")
    public JAXBElement<DecryptContentResponse> createDecryptContentResponse(DecryptContentResponse value) { // NO_UCD (unused code)
        return new JAXBElement<DecryptContentResponse>(_DecryptContentResponse_QNAME, DecryptContentResponse.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "return", scope = EncryptMdnResponse.class)
    public JAXBElement<byte[]> createEncryptMdnResponseReturn(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_EncryptMdnResponseReturn_QNAME, byte[].class, EncryptMdnResponse.class, ((byte[]) value));
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "encodedData", scope = DecryptContent.class)
    public JAXBElement<byte[]> createDecryptContentEncodedData(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_DecryptContentEncodedData_QNAME, byte[].class, DecryptContent.class, ((byte[]) value));
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "return", scope = EncryptMessageResponse.class)
    public JAXBElement<byte[]> createEncryptMessageResponseReturn(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_EncryptMdnResponseReturn_QNAME, byte[].class, EncryptMessageResponse.class, ((byte[]) value));
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "encodedData", scope = Decrypt.class)
    public JAXBElement<byte[]> createDecryptEncodedData(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_DecryptContentEncodedData_QNAME, byte[].class, Decrypt.class, ((byte[]) value));
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "return", scope = EncryptContentResponse.class)
    public JAXBElement<byte[]> createEncryptContentResponseReturn(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_EncryptMdnResponseReturn_QNAME, byte[].class, EncryptContentResponse.class, ((byte[]) value));
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link byte[]}{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "", name = "return", scope = EncryptMdnContentResponse.class)
    public JAXBElement<byte[]> createEncryptMdnContentResponseReturn(byte[] value) { // NO_UCD (unused code)
        return new JAXBElement<byte[]>(_EncryptMdnResponseReturn_QNAME, byte[].class, EncryptMdnContentResponse.class, ((byte[]) value));
    }

}
