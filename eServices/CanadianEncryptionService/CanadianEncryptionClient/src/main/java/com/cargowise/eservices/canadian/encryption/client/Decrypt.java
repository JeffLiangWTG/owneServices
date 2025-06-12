
package com.cargowise.eservices.canadian.encryption.client;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElementRef;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for decrypt complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="decrypt">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="encodedData" type="{http://www.w3.org/2001/XMLSchema}base64Binary" minOccurs="0"/>
 *         &lt;element name="isProduction" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "decrypt", propOrder = {
    "encodedData",
    "isProduction"
})
public class Decrypt {

    @XmlElementRef(name = "encodedData", type = JAXBElement.class, required = false)
    protected JAXBElement<byte[]> encodedData;
    protected boolean isProduction;

    /**
     * Gets the value of the encodedData property.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link byte[]}{@code >}
     *     
     */
    public JAXBElement<byte[]> getEncodedData() {
        return encodedData;
    }

    /**
     * Sets the value of the encodedData property.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link byte[]}{@code >}
     *     
     */
    public void setEncodedData(JAXBElement<byte[]> value) {
        this.encodedData = value;
    }

    /**
     * Gets the value of the isProduction property.
     * 
     */
    public boolean isIsProduction() {
        return isProduction;
    }

    /**
     * Sets the value of the isProduction property.
     * 
     */
    public void setIsProduction(boolean value) {
        this.isProduction = value;
    }

}
