The DATSecureStorage.xml file is the public key of DAT SecureStorage encryption.
Use this public key file to encrypt your secrets, and use the "DATSecureStorage" as the container name on the machine the private key is installed to decrypt your secrets.
Consult Bret Ehlert <Bret.Ehlert@wisetechglobal.com>; Yaakov Smith <Yaakov.Smith@wisetechglobal.com> should you need to access the private key and install on other machines.

$/eServices/eHub/DevScripts/Encryption.Common/Encryption.Common.proj and $/eServices/Tools/Security/Encryption.Common have been facilitated to use any pair of public/private keys.