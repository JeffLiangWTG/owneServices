New-Item -ItemType Directory -Force -Path C:\Input
Invoke-WebRequest "https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_consultation.jsp?Lang=en" -Outfile Input\Index.html

Invoke-WebRequest "https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_list.jsp?Lang=en&offset=0&LangNm=en&sortOrder=1" -Outfile Input\page1.html
Invoke-WebRequest "https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_list.jsp?Lang=en&offset=25&LangNm=en&sortOrder=1" -Outfile Input\page2.html
Invoke-WebRequest "https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_list.jsp?Lang=en&offset=99999&LangNm=en&sortOrder=1" -Outfile Input\emptypage.html
