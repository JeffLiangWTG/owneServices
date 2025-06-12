
using System;
using System.Collections;
using System.Xml;

// Copied from C:\dev\Enterprise\Product\Operations\Customs\ASYCUDA\ASYCUDA.Business\CAR\XmlToCar
// I cannot branch from $/Dev to $/eServcies, so copying instead.
// Please view original file and this file in KDiff to see changes. 
namespace CargoWise.eHub.Products.AsycudaCustoms.Common
{
	public class ReadcarXmlToVector
	{
		Vector carFullVector = new Vector();
		Vector carHeader = new Vector();
		Vector carContainers = new Vector();
		Vector Containers = new Vector();
		Vector carFLBL = new Vector();
		Vector flbil = new Vector();
		Vector carSLBL = new Vector();
		Vector slbil = new Vector();
		Vector V1 = new Vector();
		Vector V2 = new Vector();
		Vector V3 = new Vector();
		Vector ContainersClone = new Vector();
		Vector flbilClone = new Vector();
		Vector slbilClone = new Vector();
		Builder builder = new Builder();

		public Vector XmlToVector(String xmlString)
		{
			Document localDocument = this.builder.build(xmlString);
			
			this.carHeader.add(0, "1");
			this.carHeader.add(1, "381");
			this.carHeader.add(2, "1");
			
			var localElement = localDocument.getRootElement();
			listChildren(localElement, 1, this.V1, this.V2, this.V3);

			int i = Integer.parseInt(this.V1.get(6).ToString());

			int j = 0;
			int k = 2;
			if (i > 0)
			{
				for (int m = 0; m <= i; m++)
				{
					if (Integer.parseInt(this.V2.get(k).ToString()) == 0)
					{
						j += 1;
					}
					if (k >= this.V2.size() - 58)
					{
						break;
					}
					k += 58;
				}
			}
			this.carHeader.add(3, StringExtender.valueOf(j));
			this.carHeader.add(4, "0");

			String str1 = this.V1.get(2).ToString();
			String str2 = this.V1.get(0).ToString();
			String str3 = this.V1.get(1).ToString();
			str3 = str3.substringJavaStyle(0, 4) + str3.substringJavaStyle(5, 7) + str3.substringJavaStyle(8, 10);
			this.carHeader.add(5, str1);
			this.carHeader.add(6, str2);
			this.carHeader.add(7, str3);
			String str4 = this.V1.get(12).ToString();
			if (str4.Length == 10)
			{
				str4 = str4.substringJavaStyle(0, 4) + str4.substringJavaStyle(5, 7) + str4.substringJavaStyle(8, 10);
			}
			this.carHeader.add(8, str4);
			String str5 = this.V1.get(13).ToString();
			if (str5.Length >= 5)
			{
				str5 = str5.substringJavaStyle(0, 5);
			}
			else
			{
				str5 = "";
			}
			this.carHeader.add(9, str5);
			this.carHeader.add(10, "");
			this.carHeader.add(11, "");
			this.carHeader.add(12, "");
			this.carHeader.add(13, "");
			this.carHeader.add(14, this.V1.get(14).ToString());
			this.carHeader.add(15, this.V1.get(15).ToString());
			this.carHeader.add(16, this.V1.get(16).ToString());
			this.carHeader.add(17, this.V1.get(17).ToString());
			this.carHeader.add(18, this.V1.get(18).ToString());
			this.carHeader.add(19, this.V1.get(19).ToString());
			this.carHeader.add(20, this.V1.get(20).ToString());
			this.carHeader.add(21, this.V1.get(21).ToString());

			this.carHeader.add(22, this.V1.get(24).ToString());
			this.carHeader.add(23, this.V1.get(22).ToString());
			this.carHeader.add(24, this.V1.get(26).ToString());
			this.carHeader.add(25, this.V1.get(23).ToString());
			this.carHeader.add(26, this.V1.get(28).ToString());
			String str6 = this.V1.get(29).ToString();
			if (str6.Length >= 10)
			{
				str6 = str6.substringJavaStyle(0, 4) + str6.substringJavaStyle(5, 7) + str6.substringJavaStyle(8, 10);
			}
			this.carHeader.add(27, str6);

			this.carHeader.add(28, this.V1.get(3));
			this.carHeader.add(29, this.V1.get(4));
			this.carHeader.add(30, this.V1.get(5));
			this.carHeader.add(31, this.V1.get(11).ToString());
			this.carHeader.add(32, this.V1.get(10).ToString());

			this.carHeader.add(33, StringExtender.valueOf(j));
			this.carHeader.add(34, this.V1.get(7).ToString());
			this.carHeader.add(35, this.V1.get(9).ToString());
			this.carHeader.add(36, this.V1.get(8).ToString());
			this.carHeader.add(37, "");
			if (i > 0)
			{
				int n = 2;
				int i1 = Integer.parseInt(this.V1.get(8).ToString());
				int i2 = 0; int i3 = 0;
				this.carContainers.addElement(StringExtender.valueOf(i1));
				int i6;
				int i5;
				for (int i4Cw = 1; i4Cw <= i; i4Cw++)
				{
					i5 = Integer.parseInt(this.V2.get(n + 5).ToString());
					i6 = Integer.parseInt(this.V2.get(n + 8).ToString());
					if ((i5 > 0) && (i6 == 0))
					{
						for (int i7 = 0; i7 < i5; i7++)
						{
							this.Containers.add(this.V2.get(n - 1));
							this.Containers.add(this.V2.get(n));
							this.Containers.add(StringExtender.valueOf(i7 + 1));
							for (i3 = i2; i3 < i2 + 7; i3++)
							{
								this.Containers.add(this.V3.get(i3));
							}
							if (i2 > this.V3.size() - 7)
							{
								break;
							}
							i2 += 7;



							this.ContainersClone = this.Containers.clone();
							this.carContainers.addElement(this.ContainersClone);
							this.Containers.clear();
						}
					}
					if (n >= this.V2.size() - 58)
					{
						break;
					}
					n += 58;
				}
				int i4 = 2;
				String str7;
				String[] arrayOfString1;
				String str8;
				String[] arrayOfString2;
				String str9;
				String[] arrayOfString3;
				String str10;
				String[] arrayOfString4;
				String str11;
				String[] arrayOfString5;
				String str12;
				String[] arrayOfString6;
				String str13;
				String[] arrayOfString7;
				for (int i5Cw = 1; i5Cw <= i; i5Cw++)
				{
					i6 = Integer.parseInt(this.V2.get(i4).ToString());
					if (i6 == 0)
					{
						this.flbil.add(0, str1);
						this.flbil.add(1, str2);
						this.flbil.add(2, str3);
						this.flbil.add(3, this.V2.get(i4 - 2));
						this.flbil.add(4, this.V2.get(i4 - 1));
						this.flbil.add(5, this.V2.get(i4));
						this.flbil.add(6, this.V2.get(i4 + 1));
						this.flbil.add(7, this.V2.get(i4 + 2));
						this.flbil.add(8, this.V2.get(i4 + 9));
						this.flbil.add(9, this.V2.get(i4 + 3));
						this.flbil.add(10, this.V2.get(i4 + 4));

						this.flbil.add(11, this.V2.get(i4 + 11));
						str7 = this.V2.get(i4 + 12).ToString();
						arrayOfString1 = desBreaker10(str7);
						this.flbil.add(12, arrayOfString1[0]);
						this.flbil.add(13, arrayOfString1[1]);
						this.flbil.add(14, arrayOfString1[2]);
						this.flbil.add(15, arrayOfString1[3]);

						this.flbil.add(16, this.V2.get(i4 + 13));
						this.flbil.add(17, this.V2.get(i4 + 14));
						str8 = this.V2.get(i4 + 15).ToString();
						arrayOfString2 = desBreaker10(str8);
						this.flbil.add(18, arrayOfString2[0]);
						this.flbil.add(19, arrayOfString2[1]);
						this.flbil.add(20, arrayOfString2[2]);
						this.flbil.add(21, arrayOfString2[3]);

						this.flbil.add(22, this.V2.get(i4 + 16));
						this.flbil.add(23, this.V2.get(i4 + 17));
						str9 = this.V2.get(i4 + 18).ToString();
						arrayOfString3 = desBreaker10(str9);
						this.flbil.add(24, arrayOfString3[0]);
						this.flbil.add(25, arrayOfString3[1]);
						this.flbil.add(25, arrayOfString3[2]);
						this.flbil.add(27, arrayOfString3[3]);


						this.flbil.add(28, this.V2.get(i4 + 19));
						this.flbil.add(29, this.V2.get(i4 + 21));
						this.flbil.add(30, this.V2.get(i4 + 5));

						this.flbil.add(31, this.V2.get(i4 + 23));
						this.flbil.add(32, this.V2.get(i4 + 25));
						this.flbil.add(33, this.V2.get(i4 + 6));
						this.flbil.add(34, this.V2.get(i4 + 7));

						str10 = this.V2.get(i4 + 26).ToString();
						arrayOfString4 = desBreaker10(str10);
						this.flbil.add(35, arrayOfString4[0]);
						this.flbil.add(36, arrayOfString4[1]);
						this.flbil.add(37, arrayOfString4[2]);
						this.flbil.add(38, arrayOfString4[3]);
						this.flbil.add(39, arrayOfString4[4]);
						this.flbil.add(40, arrayOfString4[5]);
						this.flbil.add(41, arrayOfString4[6]);
						this.flbil.add(42, arrayOfString4[7]);
						this.flbil.add(43, arrayOfString4[8]);
						this.flbil.add(44, arrayOfString4[9]);

						str11 = this.V2.get(i4 + 27).ToString();
						arrayOfString5 = desBreaker10(str11);
						this.flbil.add(45, arrayOfString5[0]);
						this.flbil.add(46, arrayOfString5[1]);
						this.flbil.add(47, arrayOfString5[2]);
						this.flbil.add(48, arrayOfString5[3]);
						this.flbil.add(49, arrayOfString5[4]);

						this.flbil.add(50, this.V2.get(i4 + 30));

						this.flbil.add(51, this.V2.get(i4 + 28));
						this.flbil.add(52, this.V2.get(i4 + 29));


						this.flbil.add(53, this.V2.get(i4 + 32));
						this.flbil.add(54, this.V2.get(i4 + 33));

						this.flbil.add(55, this.V2.get(i4 + 34));
						this.flbil.add(56, this.V2.get(i4 + 35));

						this.flbil.add(57, this.V2.get(i4 + 36));
						this.flbil.add(58, this.V2.get(i4 + 37));

						this.flbil.add(59, this.V2.get(i4 + 8));

						this.flbil.add(60, this.V2.get(i4 + 38));
						str12 = this.V2.get(i4 + 39).ToString();
						arrayOfString6 = desBreaker2(str12);
						this.flbil.add(61, arrayOfString6[0]);
						this.flbil.add(62, arrayOfString6[1]);
						this.flbil.add(63, this.V2.get(i4 + 40));

						str13 = this.V2.get(i4 + 42).ToString();
						arrayOfString7 = desBreaker10(str13);
						this.flbil.add(64, arrayOfString7[0]);
						this.flbil.add(65, arrayOfString7[1]);

						this.flbil.add(66, this.V2.get(i4 - 1));

						this.flbil.add(67, this.V2.get(i4 + 43));
						this.flbil.add(68, this.V2.get(i4 + 44));

						this.flbil.add(69, this.V2.get(i4 + 45));
						this.flbil.add(70, this.V2.get(i4 + 47));


						this.flbilClone = this.flbil.clone();
						this.carFLBL.addElement(this.flbilClone);
						this.flbil.clear();
					}
					if (i4 >= this.V2.size() - 58)
					{
						break;
					}
					i4 += 58;
				}
				i4 = 2;
				for (i5 = 1; i5 <= i; i5++)
				{
					i6 = Integer.parseInt(this.V2.get(i4).ToString());
					if (i6 != 0)
					{
						this.slbil.add(0, str1);
						this.slbil.add(1, str2);
						this.slbil.add(2, str3);
						this.slbil.add(3, "");

						this.slbil.add(4, this.V2.get(i4 - 1));
						this.slbil.add(5, this.V2.get(i4));
						this.slbil.add(6, "");
						this.slbil.add(7, "");
						this.slbil.add(8, "");
						this.slbil.add(9, "");
						this.slbil.add(10, this.V2.get(i4 + 11));
						this.slbil.add(11, this.V2.get(i4 + 9));
						this.slbil.add(12, "");
						str7 = this.V2.get(i4 + 12).ToString();
						arrayOfString1 = desBreaker10(str7);
						this.slbil.add(13, arrayOfString1[0]);
						this.slbil.add(14, this.V2.get(i4 - 2));
						this.slbil.add(15, arrayOfString1[1]);
						this.slbil.add(16, this.V2.get(i4 + 3));
						this.slbil.add(17, "");
						this.slbil.add(18, arrayOfString1[2]);
						this.slbil.add(19, this.V2.get(i4 + 2));
						this.slbil.add(20, arrayOfString1[3]);
						this.slbil.add(21, this.V2.get(i4 + 4));
						this.slbil.add(22, this.V2.get(i4 + 13));
						this.slbil.add(23, this.V2.get(i4 + 19));
						this.slbil.add(24, "");
						this.slbil.add(25, this.V2.get(i4 + 14));
						this.slbil.add(26, this.V2.get(i4 + 21));
						this.slbil.add(27, "");
						str8 = this.V2.get(i4 + 15).ToString();
						arrayOfString2 = desBreaker10(str8);
						this.slbil.add(28, arrayOfString2[0]);
						this.slbil.add(29, arrayOfString2[1]);
						this.slbil.add(30, "");
						this.slbil.add(31, "");
						this.slbil.add(32, arrayOfString2[2]);
						this.slbil.add(33, "");
						this.slbil.add(34, arrayOfString2[3]);
						this.slbil.add(35, "");
						this.slbil.add(36, "");

						this.slbil.add(37, this.V2.get(i4 + 16));
						this.slbil.add(38, "");
						this.slbil.add(39, this.V2.get(i4 + 17));
						this.slbil.add(40, "");
						str9 = this.V2.get(i4 + 18).ToString();
						arrayOfString3 = desBreaker10(str9);
						this.slbil.add(41, arrayOfString3[0]);
						this.slbil.add(42, "");
						this.slbil.add(43, arrayOfString3[1]);
						this.slbil.add(44, "");
						this.slbil.add(45, arrayOfString3[2]);
						this.slbil.add(46, "");
						this.slbil.add(47, arrayOfString3[3]);
						this.slbil.add(48, "");
						this.slbil.add(49, this.V2.get(i4 + 5));

						this.slbil.add(50, "HDG");
						this.slbil.add(51, this.V2.get(i4 + 8));
						this.slbil.add(52, this.V2.get(i4 + 23));
						this.slbil.add(53, "");
						str10 = this.V2.get(i4 + 26).ToString();
						arrayOfString4 = desBreaker10(str10);
						this.slbil.add(54, arrayOfString4[0]);
						this.slbil.add(55, arrayOfString4[1]);
						this.slbil.add(56, arrayOfString4[2]);
						this.slbil.add(57, arrayOfString4[3]);
						this.slbil.add(58, arrayOfString4[4]);
						this.slbil.add(59, arrayOfString4[5]);
						this.slbil.add(60, arrayOfString4[6]);
						this.slbil.add(61, arrayOfString4[7]);
						this.slbil.add(62, arrayOfString4[8]);
						this.slbil.add(63, arrayOfString4[9]);
						this.slbil.add(64, this.V2.get(i4 + 25));
						this.slbil.add(65, this.V2.get(i4 + 6));
						this.slbil.add(66, this.V2.get(i4 + 43));
						this.slbil.add(67, this.V2.get(i4 + 44));
						this.slbil.add(68, this.V2.get(i4 + 7));
						str11 = this.V2.get(i4 + 27).ToString();
						arrayOfString5 = desBreaker10(str11);
						this.slbil.add(69, arrayOfString5[0]);
						this.slbil.add(70, arrayOfString5[1]);
						this.slbil.add(71, arrayOfString5[2]);
						this.slbil.add(72, arrayOfString5[3]);
						this.slbil.add(73, arrayOfString5[4]);
						this.slbil.add(74, this.V2.get(i4 + 45));
						this.slbil.add(75, this.V2.get(i4 + 47));
						this.slbil.add(76, this.V2.get(i4 + 30));
						this.slbil.add(77, "");

						this.slbil.add(78, this.V2.get(i4 + 28));
						this.slbil.add(79, this.V2.get(i4 + 29));

						this.slbil.add(80, this.V2.get(i4 + 32));
						this.slbil.add(81, this.V2.get(i4 + 33));

						this.slbil.add(82, this.V2.get(i4 + 34));
						this.slbil.add(83, this.V2.get(i4 + 35));

						this.slbil.add(84, this.V2.get(i4 + 36));
						this.slbil.add(85, this.V2.get(i4 + 37));


						this.slbil.add(86, this.V2.get(i4 + 38));
						str12 = this.V2.get(i4 + 39).ToString();
						arrayOfString6 = desBreaker2(str12);
						this.slbil.add(87, arrayOfString6[0]);
						this.slbil.add(88, arrayOfString6[1]);
						this.slbil.add(89, this.V2.get(i4 + 40));
						this.slbil.add(90, this.V2.get(i4 + 41));

						str13 = this.V2.get(i4 + 42).ToString();
						arrayOfString7 = desBreaker10(str13);
						this.slbil.add(91, arrayOfString7[0]);
						this.slbil.add(92, arrayOfString7[1]);

						this.slbil.add(93, "");
						this.slbil.add(94, "");
						this.slbil.add(95, "");
						this.slbil.add(96, "");
						this.slbil.add(97, "");
						this.slbil.add(98, "");
						this.slbil.add(99, "");
						this.slbil.add(100, "");

						this.slbilClone = this.slbil.clone();
						this.carSLBL.addElement(this.slbilClone);
						this.slbil.clear();
					}
					if (i4 >= this.V2.size() - 58)
					{
						break;
					}
					i4 += 58;
				}
			}
			this.carFullVector.addElement(this.carHeader);
			this.carFullVector.addElement(this.carContainers);
			this.carFullVector.addElement(this.carFLBL);
			this.carFullVector.addElement(this.carSLBL);

			return this.carFullVector;
		}

		String getxmlValue(String paramString, Document paramDocument)
		{
			String str = "";
			var localNodes = paramDocument.query(paramString);
			str = localNodes[0].getValue().ToString(); // str = localNodes.get(0).getValue().ToString();
			return str;
		}

		public static void listChildren(XmlNode paramElement, int paramInt, Vector paramVector1, Vector paramVector2, Vector paramVector3)
		{
			object[] localObject = null;   // CW
			String str = "";
			if (paramElement.getQualifiedName() == "Voyage_number")
			{
				paramVector1.add(0, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Date_of_departure")
			{
				paramVector1.add(1, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Customs_office_segment")
			{
				paramVector1.add(2, paramElement.getFirstChildElement("Code").getValue());
			}
			if (paramElement.getQualifiedName() == "Master_information")
			{
				str = paramElement.getValue().Trim();
				localObject = desBreaker10(str);
				paramVector1.add(3, localObject[0]);
				paramVector1.add(4, localObject[1]);
				paramVector1.add(5, localObject[2]);
			}
			if (paramElement.getQualifiedName() == "Totals_segment")
			{
				paramVector1.add(6, paramElement.getFirstChildElement("Total_number_of_bols").getValue().Trim());
				paramVector1.add(7, paramElement.getFirstChildElement("Total_numer_of_packages").getValue().Trim());
				paramVector1.add(8, paramElement.getFirstChildElement("Total_number_of_containers").getValue().Trim());
				paramVector1.add(9, paramElement.getFirstChildElement("Total_gross_mass").getValue().Trim());
				paramVector1.add(10, paramElement.getFirstChildElement("Total_tonnage_gross").getValue().Trim());
				paramVector1.add(11, paramElement.getFirstChildElement("Total_tonnage_net").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Arrival_segment")
			{
				paramVector1.add(12, paramElement.getFirstChildElement("Date_of_arrival").getValue());
				paramVector1.add(13, paramElement.getFirstChildElement("Time_of_arrival").getValue());
			}
			if (paramElement.getQualifiedName() == "Place_of_departure_code")
			{
				paramVector1.add(14, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Place_of_destination_code")
			{
				paramVector1.add(15, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Carrier_code")
			{
				paramVector1.add(16, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Carrier_name")
			{
				paramVector1.add(17, paramElement.getValue());
			}
			if (paramElement.getQualifiedName() == "Carrier_address")
			{
				var s = paramElement.getValue();  // localObject = paramElement.getValue();  
				String[] arrayOfString = desBreaker10(s);  // String[] arrayOfString = desBreaker10((String)localObject);
				paramVector1.add(18, arrayOfString[0]);
				paramVector1.add(19, arrayOfString[1]);
				paramVector1.add(20, arrayOfString[2]);
				paramVector1.add(21, arrayOfString[3]);
			}
			if (paramElement.getQualifiedName() == "Transporter_segment")
			{
				paramVector1.add(22, paramElement.getFirstChildElement("Name_of_transporter").getValue().Trim());
				paramVector1.add(23, paramElement.getFirstChildElement("Place_of_transporter").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Mode_of_transport_segment")
			{
				paramVector1.add(24, paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector1.add(25, paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Nationality_of_transport_segment")
			{
				paramVector1.add(26, paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector1.add(27, paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Transporter_registration_segment")
			{
				paramVector1.add(28, paramElement.getFirstChildElement("Registration_number").getValue().Trim());
				var dateValue = paramElement.getFirstChildElement("Registration_date").getValue().Trim(); // localObject = paramElement.getFirstChildElement("Registration_date").getValue().Trim();
				paramVector1.add(29, dateValue); // paramVector1.add(29, localObject);
			}
			if (paramElement.getQualifiedName() == "Bol_specific_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Bol_reference").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Line_number").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Sub_line_number").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Status").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Previous_document_reference").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Bol_Nature").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Unique_carrier_reference").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Total_number_of_containers").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Total_gross_mass_manifested").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Volume_in_cubic_meters").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Number_of_sub_bols").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Bol_type_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Exporter_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Address").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Consignee_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Address").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Notify_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Address").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Place_of_loading_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Place_of_unloading_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Packages_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Package_type_code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Package_type_name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Number_of_packages").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Shipping_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Shipping_marks").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Goods_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Goods_description").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Freight_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Value").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Currency").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Indicator_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Customs_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Value").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Currency").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Transport_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Value").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Currency").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Insurance_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Value").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Currency").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Seals_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Number_of_seals").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Marks_of_seals").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Sealing_party_code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Sealing_party_name").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Information_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Information_part_a").getValue());
			}
			if (paramElement.getQualifiedName() == "Operations_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Packages_remaining").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Gross_mass_remaining").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Location_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Information").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Transit_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Customs_office_code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Customs_office_name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Document_reference").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Transhipment_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Transhipment_location_code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Transhipment_location_name").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Document_reference").getValue().Trim());
			}
			if (paramElement.getQualifiedName() == "Onward_carrier_segment")
			{
				paramVector2.add(paramElement.getFirstChildElement("Code").getValue().Trim());
				paramVector2.add(paramElement.getFirstChildElement("Name").getValue().Trim());
			}
			if ((paramElement.getQualifiedName() == "Containers_segment") &&
			  (paramElement.getFirstChildElement("Container_Code").getValue().Length > 0))
			{
				paramVector3.add(paramElement.getFirstChildElement("Container_Code").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Type").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Efi").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Seal1").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Seal2").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Seal3").getValue().Trim());
				paramVector3.add(paramElement.getFirstChildElement("Container_Seal_Party").getValue().Trim());
			}
			localObject = paramElement.getChildElements().ToArray();
			for (int i = 0; i < localObject.Length; i++)  // for (int i = 0; i < ((Elements)localObject).size(); i++) 
			{
				listChildren((XmlNode)localObject[i], paramInt + 1, paramVector1, paramVector2, paramVector3); // listChildren(((Elements)localObject).get(i), paramInt + 1, paramVector1, paramVector2, paramVector3);       
			}
		}

		String getxmlBlValue(String paramString, Document paramDocument, int paramInt)
		{
			String str = "";
			var localNodes1 = paramDocument.query("//Bol_specific_segment");
			var localNodes2 = localNodes1[paramInt - 1].query(paramString); // localNodes1.get(paramInt - 1).query(paramString);
			str = localNodes2[paramInt - 1].getValue().ToString();  //   str = localNodes2.get(paramInt - 1).getValue().ToString();
			return str;
		}

		ArrayList doTokans(String paramString)
		{
			var arrayList = new ArrayList();
			arrayList.AddRange(System.Text.RegularExpressions.Regex.Split(paramString, "\n"));
			return arrayList;

			//var localArrayList = new ArrayList();

			//int i = 0;
			//String str = paramString;
			//StringTokenizer localStringTokenizer = new StringTokenizer(str, "\n");
			//int j = localStringTokenizer.countTokens();
			//while (localStringTokenizer.hasMoreTokens())
			//{
			//	localArrayList.Insert(i, localStringTokenizer.nextToken());
			//	i++;
			//}
			//return localArrayList;
		}

		static String[] desBreaker10(String paramString)
		{
			String[] arrayOfString = new String[10];
			if (paramString.Length > 349)
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, 209).Trim();
				arrayOfString[6] = paramString.substringJavaStyle(209, 244).Trim();
				arrayOfString[7] = paramString.substringJavaStyle(244, 279).Trim();
				arrayOfString[8] = paramString.substringJavaStyle(279, 314).Trim();
				arrayOfString[9] = paramString.substringJavaStyle(314, 349).Trim();
			}
			else if ((paramString.Length <= 350) && (paramString.Length > 315))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34);
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, 209).Trim();
				arrayOfString[6] = paramString.substringJavaStyle(209, 244).Trim();
				arrayOfString[7] = paramString.substringJavaStyle(244, 279).Trim();
				arrayOfString[8] = paramString.substringJavaStyle(279, 314).Trim();
				arrayOfString[9] = paramString.substringJavaStyle(314, paramString.Length - 1).Trim();
			}
			else if ((paramString.Length <= 315) && (paramString.Length > 280))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, 209).Trim();
				arrayOfString[6] = paramString.substringJavaStyle(209, 244).Trim();
				arrayOfString[7] = paramString.substringJavaStyle(244, 279).Trim();
				arrayOfString[8] = paramString.substringJavaStyle(279, paramString.Length - 1).Trim();
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 280) && (paramString.Length > 245))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, 209).Trim();
				arrayOfString[6] = paramString.substringJavaStyle(209, 244).Trim();
				arrayOfString[7] = paramString.substringJavaStyle(244, paramString.Length - 1).Trim();
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 245) && (paramString.Length > 210))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, 209).Trim();
				arrayOfString[6] = paramString.substringJavaStyle(209, paramString.Length - 1).Trim();
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 210) && (paramString.Length > 175))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, 174).Trim();
				arrayOfString[5] = paramString.substringJavaStyle(174, paramString.Length - 1).Trim();
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 175) && (paramString.Length > 140))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, 139).Trim();
				arrayOfString[4] = paramString.substringJavaStyle(139, paramString.Length - 1).Trim();
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 140) && (paramString.Length > 105))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, 104).Trim();
				arrayOfString[3] = paramString.substringJavaStyle(104, paramString.Length - 1).Trim();
				arrayOfString[4] = "";
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 105) && (paramString.Length > 70))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, 69).Trim();
				arrayOfString[2] = paramString.substringJavaStyle(69, paramString.Length - 1).Trim();
				arrayOfString[3] = "";
				arrayOfString[4] = "";
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 70) && (paramString.Length > 35))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 34).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(34, paramString.Length - 1).Trim();
				arrayOfString[2] = "";
				arrayOfString[3] = "";
				arrayOfString[4] = "";
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if ((paramString.Length <= 35) && (paramString.Length > 0))
			{
				arrayOfString[0] = paramString.Trim();
				arrayOfString[1] = "";
				arrayOfString[2] = "";
				arrayOfString[3] = "";
				arrayOfString[4] = "";
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			else if (paramString.Length == 0)
			{
				arrayOfString[0] = "";
				arrayOfString[1] = "";
				arrayOfString[2] = "";
				arrayOfString[3] = "";
				arrayOfString[4] = "";
				arrayOfString[5] = "";
				arrayOfString[6] = "";
				arrayOfString[7] = "";
				arrayOfString[8] = "";
				arrayOfString[9] = "";
			}
			return arrayOfString;
		}

		String[] desBreaker2(String paramString)
		{
			String[] arrayOfString = new String[10];
			if (paramString.Length > 20)
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 10).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(10, 20).Trim();
			}
			else if ((paramString.Length <= 20) && (paramString.Trim().Length > 10))
			{
				arrayOfString[0] = paramString.substringJavaStyle(0, 10).Trim();
				arrayOfString[1] = paramString.substringJavaStyle(10, paramString.Length).Trim();
			}
			else if (paramString.Length <= 10)
			{
				arrayOfString[0] = paramString.Trim();
				arrayOfString[1] = "";
			}
			return arrayOfString;
		}

		public static void main(String[] paramArrayOfString) { }
	}
}