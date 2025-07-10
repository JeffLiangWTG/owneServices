@echo off

set OPENSSL=D:\DevTools\openssl\openssl.exe

mkdir temp out

echo -----------------------------------------------------------------------------
echo ---- Setting up Root CA
echo -----------------------------------------------------------------------------

mkdir temp\root-ca
pushd temp\root-ca
mkdir certs crl newcerts private
copy nul index.txt
echo unique_subject = no > index.txt.attr
echo 1000 > serial
popd

%OPENSSL% genrsa -out temp/root-ca/private/root-ca.key.pem 4096
%OPENSSL% req -config root-ca.config -key temp/root-ca/private/root-ca.key.pem -new -x509 -days 1825 -extensions v3_ca -out temp/root-ca/certs/root-ca.crt

%OPENSSL% x509 -in temp\root-ca\certs\root-ca.crt -outform DER -out out/root-ca.cer

echo -----------------------------------------------------------------------------
echo ---- Setting up Intermediate CA
echo -----------------------------------------------------------------------------

mkdir temp\intermediate-ca
pushd temp\intermediate-ca
mkdir certs crl csr newcerts private
copy nul index.txt
echo 1000 > serial
echo 1000 > crlnumber
popd

%OPENSSL% genrsa -out temp/intermediate-ca/private/intermediate-k1.key.pem 4096
%OPENSSL% genrsa -out temp/intermediate-ca/private/intermediate-k2.key.pem 4096

%OPENSSL% req -config intermediate-ca.config -new -key temp/intermediate-ca/private/intermediate-k1.key.pem -out temp/intermediate-ca/csr/intermediate-k1-365.csr.pem
%OPENSSL% req -config intermediate-ca.config -new -key temp/intermediate-ca/private/intermediate-k1.key.pem -out temp/intermediate-ca/csr/intermediate-k1-730.csr.pem
%OPENSSL% req -config intermediate-ca.config -new -key temp/intermediate-ca/private/intermediate-k2.key.pem -out temp/intermediate-ca/csr/intermediate-k2-900.csr.pem

%OPENSSL% ca -config root-ca.config -batch -extensions v3_intermediate_ca -days 365 -notext -in temp/intermediate-ca/csr/intermediate-k1-365.csr.pem -out temp/intermediate-ca/certs/intermediate-k1-365.crt
%OPENSSL% ca -config root-ca.config -batch -extensions v3_intermediate_ca -days 730 -notext -in temp/intermediate-ca/csr/intermediate-k1-730.csr.pem -out temp/intermediate-ca/certs/intermediate-k1-730.crt
%OPENSSL% ca -config root-ca.config -batch -extensions v3_intermediate_ca -days 900 -notext -in temp/intermediate-ca/csr/intermediate-k2-900.csr.pem -out temp/intermediate-ca/certs/intermediate-k2-900.crt

%OPENSSL% x509 -in temp\intermediate-ca\certs\intermediate-k1-365.crt -outform DER -out out/intermediate-k1-365.cer
%OPENSSL% x509 -in temp\intermediate-ca\certs\intermediate-k1-730.crt -outform DER -out out/intermediate-k1-730.cer
%OPENSSL% x509 -in temp\intermediate-ca\certs\intermediate-k2-900.crt -outform DER -out out/intermediate-k2-900.cer

echo -----------------------------------------------------------------------------
echo ---- Issuing Organization Certificate (Sample Organization)
echo -----------------------------------------------------------------------------
%OPENSSL% genrsa -out out/sample-org.key 2048
%OPENSSL% req -config intermediate-ca-sample-org.config -key out/sample-org.key -new -out temp/intermediate-ca/csr/sample-org.csr.pem
%OPENSSL% ca -config intermediate-ca-sample-org.config -batch -extensions server_cert -days 375 -notext -in temp/intermediate-ca/csr/sample-org.csr.pem -out temp/intermediate-ca/certs/sample-org.crt

%OPENSSL% x509 -in temp\intermediate-ca\certs\sample-org.crt -outform DER -out out/sample-org.cer
copy temp\intermediate-ca\certs\intermediate-k1-730.crt + temp\root-ca\certs\root-ca.crt temp\CAs.crt.pem
%OPENSSL% pkcs12 -export ^
			-out out/sample-org.pfx ^
			-in temp/intermediate-ca/certs/sample-org.crt ^
			-inkey out/sample-org.key ^
			-certfile temp/CAs.crt.pem ^
			-password pass:123

echo -----------------------------------------------------------------------------
echo ---- Issuing Organization Certificate (Sample Organization 2)
echo -----------------------------------------------------------------------------
%OPENSSL% genrsa -out out/sample-org-2.key 2048
%OPENSSL% req -config intermediate-ca-sample-org-2.config -key out/sample-org-2.key -new -out temp/intermediate-ca/csr/sample-org-2.csr.pem
%OPENSSL% ca -config intermediate-ca-sample-org-2.config -batch -extensions server_cert -days 375 -notext -in temp/intermediate-ca/csr/sample-org-2.csr.pem -out temp/intermediate-ca/certs/sample-org-2.crt

%OPENSSL% x509 -in temp\intermediate-ca\certs\sample-org-2.crt -outform DER -out out/sample-org-2.cer
copy temp\intermediate-ca\certs\intermediate-k1-730.crt + temp\root-ca\certs\root-ca.crt temp\CAs.crt.pem
%OPENSSL% pkcs12 -export ^
			-out out/sample-org-2.pfx ^
			-in temp/intermediate-ca/certs/sample-org-2.crt ^
			-inkey out/sample-org-2.key ^
			-certfile temp/CAs.crt.pem ^
			-password pass:123
