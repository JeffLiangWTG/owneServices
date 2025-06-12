using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Models.View.Certificate;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
    public class ClientCertificatesController : ControllerBase
    {
        public ILog logger = LogManager.GetLogger("ClientCertificatesLogger");

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Certificates()
        {
            int page = Convert.ToInt32(Request["page"]);
            int rows = Convert.ToInt32(Request["rows"]);
            string sidx = Request["sidx"];
            string sord = Request["sord"];
            string searchQuery = Request["filters"];
            MultipleFilter filterQuery = new MultipleFilter();

            if (!string.IsNullOrEmpty(searchQuery))
                filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);

            IQueryable<eHubCertificate> cetificates = Context.eHubCertificates;

            if (filterQuery.rules != null && filterQuery.rules.Count > 0)
                cetificates = ApplyMultipleValuesFilter(filterQuery, cetificates);

            int count = cetificates.Count();

            cetificates = ApplyValuesSort(sidx, sord, cetificates);

            cetificates = cetificates.Skip((page - 1) * rows).Take(rows);

            var list = cetificates.Select(r => new
            {
                CE_PK = r.CE_PK,
                CE_Category = r.CE_Category,
                CE_ID = r.CE_ID,
                CE_CC_ID = r.eHubClient.CC_ID,
                CE_EH_ID = r.eHubClientSystem.EH_ID,
                CE_AddedUTC = r.CE_AddedUTC,
                CE_ContainerType = r.CE_ContainerType,
                CE_TextContainer = r.CE_TextContainer,
                CE_Password = r.CE_Password,
                CE_ValidFromUTC = r.CE_ValidFromUTC,
                CE_ValidToUTC = r.CE_ValidToUTC,
                CE_ActiveFromUTC = r.CE_ActiveFromUTC,
                CE_Thumbprint = r.CE_Thumbprint,
                CE_Issuer = r.CE_Issuer,
                CE_SerialNumber = r.CE_SerialNumber,
                CE_SubjectKeyIdentifier = r.CE_SubjectKeyIdentifier
            });

            return Json(new
            {
                page = page,
                total = Math.Ceiling((double)count / (double)rows),
                records = count,
                eHubClientCertificates = list.ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        protected void AddCertificateLog(string oper, eHubCertificate certificate)
        {
            logger.Info(() => $"[{oper}] eHubCertificate: CE_PK={certificate.CE_PK}, CE_Category={certificate.CE_Category}, CE_ID={certificate.CE_ID}, CE_CC_Owner={certificate.CE_CC_Owner}, CE_EH_Owner={certificate.CE_EH_Owner}" +
                              $", CE_ValidFromUTC={certificate.CE_ValidFromUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}, CE_ValidToUTC={certificate.CE_ValidToUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}" +
                              $", CE_ActiveFromUTC={certificate.CE_ActiveFromUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}, CE_AddedUTC={certificate.CE_AddedUTC.ToString("yyyy-MM-dd hh:mm:ss.fff")}, CE_ContainerType={certificate.CE_ContainerType}" +
                              $", CE_Password={certificate.CE_Password}, CE_Thumbprint={certificate.CE_Thumbprint}, CE_Issuer={certificate.CE_Issuer}, CE_SerialNumber={certificate.CE_SerialNumber}, CE_SubjectKeyIdentifier={certificate.CE_SubjectKeyIdentifier}");
        }

        [HttpPost]
        public JsonResult CertificateEditJQGrid()
        {
            try
            {
                var oper = Request["oper"];
                var cepk = Request["CE_PK"] ?? Request["id"];

                if (oper == "del")
                {
                    var cxpkguid = new Guid(cepk);
                    var certificate = Context.eHubCertificates.First(r => r.CE_PK == cxpkguid);
                    Context.eHubCertificates.DeleteObject(certificate);
                    Context.SaveChanges();
                    AddCertificateLog(oper, certificate);
                    return Json(new { success = true, id = certificate.CE_PK }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "invalid oper action: " + oper }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public String CertificateEdit()
        {
            JsonResult json;
            var oper = Request["oper"];
            try
            {
                var cepk = Request["CE_PK"] ?? Request["certificatesTable_id"] ?? Request["id"];
                var category = Request["CE_Category"];
                var id = Request["CE_ID"];
                var clientID = Request["CE_CC_ID"];
                var clientSystemID = Request["CE_EH_ID"];
                var containerType = Request["CE_ContainerType"];
                var password = Request["CE_Password"] == "" ? null : Request["CE_Password"];
                var activeFromUtc = String.IsNullOrWhiteSpace(Request["CE_ActiveFromUtc"]) ? (DateTime?)null : DateTime.Parse(Request["CE_ActiveFromUtc"]);

                eHubCertificate certificate;
                if (String.IsNullOrWhiteSpace(cepk) || cepk == "_empty")
                    certificate = new eHubCertificate();
                else
                {
                    var cxpkguid = new Guid(cepk);
                    certificate = Context.eHubCertificates.First(r => r.CE_PK == cxpkguid);
                }

                byte[] binaryContainer = null;
                string textContainer = null;
                var addedUTC = GetDateTimeUtcNow();
                bool hasFile = false;
                if (Request.Files.Count > 0)
                {
                    using (var stream = Request.Files["certificate_inputFile"].InputStream)
                    {
                        FileType type = ContainerType.GetContainerType(containerType).Type;
                        if (type == FileType.Text)
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                textContainer = streamReader.ReadToEnd();
                            }
                            hasFile = textContainer.Length > 0;
                        }
                        else
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                binaryContainer = ms.ToArray();
                            }
                            hasFile = binaryContainer.Length > 0;
                        }
                    }
                }
                if (certificate.CE_ContainerType != containerType && !hasFile)
                {
                    throw new ArgumentException("New File must be submitted with the ContainerType change.");
                }

                switch (oper)
                {
                    case "add":
                        certificate.CE_PK = Guid.NewGuid();
                        certificate.CE_Category = category;
                        certificate.CE_ID = id;
                        try
                        {
                            certificate.eHubClient = string.IsNullOrEmpty(clientID) ? null : Context.eHubClients.First(c => c.CC_ID == clientID);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new InvalidOperationException("Client ID invalid. Please reload the grid.", ex);
                        }
                        try
                        {
                            certificate.eHubClientSystem = string.IsNullOrEmpty(clientSystemID) ? null : Context.eHubClientSystems.First(eh => eh.EH_ID == clientSystemID);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new InvalidOperationException("Client System ID invalid. Please reload the grid.", ex);
                        }

                        certificate.CE_AddedUTC = addedUTC;
                        certificate.CE_ActiveFromUTC = activeFromUtc;
                        certificate.CE_ContainerType = containerType;
                        certificate.CE_TextContainer = textContainer;
                        certificate.CE_Password = password;
                        certificate.CE_TextContainer = textContainer;
                        certificate.CE_BinaryContainer = binaryContainer;
                        Context.eHubCertificates.AddObject(certificate);
                        break;
                    case "edit":
                        certificate.CE_Category = category;
                        certificate.CE_ID = id;
                        try
                        {
                            certificate.eHubClient = string.IsNullOrEmpty(clientID) ? null : Context.eHubClients.First(c => c.CC_ID == clientID);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new InvalidOperationException("Client ID invalid. Please reload the grid.", ex);
                        }
                        try
                        {
                            certificate.eHubClientSystem = string.IsNullOrEmpty(clientSystemID) ? null : Context.eHubClientSystems.First(eh => eh.EH_ID == clientSystemID);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new InvalidOperationException("Client System ID invalid. Please reload the grid.", ex);
                        }
                        certificate.CE_ActiveFromUTC = activeFromUtc;
                        certificate.CE_ContainerType = containerType;
                        certificate.CE_TextContainer = textContainer;
                        certificate.CE_Password = password;
                        if (hasFile)
                        {
                            certificate.CE_AddedUTC = addedUTC;
                            certificate.CE_TextContainer = textContainer;
                            certificate.CE_BinaryContainer = binaryContainer;
                        }
                        break;

                    default:
                        break;
                }
                if (hasFile)
                {
                    var certs = new X509Certificate2Collection();

                    if (containerType == ContainerType.PKCS12_Binary.ID)
                        certs.Import(binaryContainer, password, X509KeyStorageFlags.MachineKeySet);
                    else if (containerType == ContainerType.X509_Binary.ID)
                        certs.Import(binaryContainer);

                    var cert = certs.Cast<X509Certificate2>().LastOrDefault();
                    if (cert != null)
                    {
                        certificate.CE_Thumbprint = cert.Thumbprint;
                        certificate.CE_ValidFromUTC = cert.NotBefore.ToUniversalTime();
                        certificate.CE_ValidToUTC = cert.NotAfter.ToUniversalTime();
                        certificate.CE_Issuer = cert.Issuer;
                        certificate.CE_SerialNumber = cert.SerialNumber;
                        var subjectKeyId = cert.Extensions.Cast<X509Extension>().OfType<X509SubjectKeyIdentifierExtension>().FirstOrDefault();
                        certificate.CE_SubjectKeyIdentifier = subjectKeyId == null ? null : subjectKeyId.SubjectKeyIdentifier;
                    }
                    else
                    {
                        certificate.CE_Thumbprint = null;
                        certificate.CE_ValidFromUTC = null;
                        certificate.CE_ValidToUTC = null;
                        certificate.CE_Issuer = null;
                        certificate.CE_SerialNumber = null;
                        certificate.CE_SubjectKeyIdentifier = null;
                    }
                }
                Context.SaveChanges();
                AddCertificateLog(oper, certificate);
                json = Json(new { success = true, id = certificate.CE_PK });
            }
            catch (Exception ex)
            {
                json = Json(new { success = false, message = ex.Message });
            }
            var result = JsonConvert.SerializeObject(json.Data);
            return result;
        }

        public JsonResult Clients(bool includeNonProd)
        {
            int page = Convert.ToInt32(Request["page"]);
            int rows = Convert.ToInt32(Request["rows"]);
            string sidx = Request["sidx"];
            string sord = Request["sord"];
            bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = Context.eHubClients.Select(c => new
				{
					CC_ID = c.CC_ID,
					CC_FriendlyName = c.CC_FriendlyName
				});
			}
			else
			{
				clients = from c in Context.eHubClients
						  join p in Context.ediProdClients on c.CC_PK equals p.CC_PK into joinedClients
						  from p in joinedClients.DefaultIfEmpty()
						  where p == null || p.LD_LicenceType == "PRD"
						  select new { c.CC_ID, c.CC_FriendlyName };
			}

			if (filtered)
            {
                string ccid = Request["CC_ID"];
                string ccname = Request["CC_FriendlyName"];
                if (!String.IsNullOrWhiteSpace(ccid))
                    clients = clients.Where(c => c.CC_ID.StartsWith(ccid, StringComparison.OrdinalIgnoreCase));
                if (!String.IsNullOrWhiteSpace(ccname))
                    clients = clients.Where(c => c.CC_FriendlyName.Contains(ccname));
            }

            int count = clients.Count();

            switch (sidx + " " + sord)
            {
                case "CC_ID asc":
                    clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
                    break;
                case "CC_ID desc":
                    clients = clients.OrderByDescending(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
                    break;
                case "CC_FriendlyName asc":
                    clients = clients.OrderBy(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
                    break;
                case "CC_FriendlyName desc":
                    clients = clients.OrderByDescending(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
                    break;
                default:
                    clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
                    break;
            }

            return Json(new
            {
                page = page,
                total = Math.Ceiling((double)count / (double)rows),
                records = count,
                eHubClients = clients.Skip((page - 1) * rows).Take(rows).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ClientSystems(bool includeNonProd)
        {
            int page = Convert.ToInt32(Request["page"]);
            int rows = Convert.ToInt32(Request["rows"]);
            string sidx = Request["sidx"];
            string sord = Request["sord"];
            bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clientSystems;
			if (includeNonProd)
			{
				clientSystems = Context.eHubClientSystems.Select(cs => new
				{
					cs.EH_ID
				});
			}
			else
			{
				clientSystems = from sys in Context.eHubClientSystems
						  join prodClient in Context.ediProdClients
						  on sys.EH_ID equals prodClient.EnterpriseServerCode
						  where prodClient == null || prodClient.LD_LicenceType == "PRD"
						  select new { sys.EH_ID };
			}


			if (filtered)
            {
                string ehid = Request["EH_ID"];
                if (!String.IsNullOrWhiteSpace(ehid))
                    clientSystems = clientSystems.Where(eh => eh.EH_ID.StartsWith(ehid, StringComparison.OrdinalIgnoreCase));
            }

            int count = clientSystems.Count();

            switch (sidx + " " + sord)
            {
                case "EH_ID asc":
                    clientSystems = clientSystems.OrderBy(eh => eh.EH_ID);
                    break;
                case "EH_ID desc":
                    clientSystems = clientSystems.OrderByDescending(eh => eh.EH_ID);
                    break;
                default:
                    clientSystems = clientSystems.OrderBy(eh => eh.EH_ID);
                    break;
            }

            return Json(new
            {
                page = page,
                total = Math.Ceiling((double)count / (double)rows),
                records = count,
                eHubClientSystems = clientSystems.Skip((page - 1) * rows).Take(rows).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public FileContentResult DownloadCertificate()
        {
            var ce_pk = Request["CE_PK"];

            var cxpkguid = new Guid(ce_pk);
            var certificate = Context.eHubCertificates.First(r => r.CE_PK == cxpkguid);

            var containerType = ContainerType.GetContainerType(certificate.CE_ContainerType);
            var fileName = certificate.CE_ID + containerType.DefaultExtension;
            var fileByte = containerType.Type == FileType.Text ? Encoding.Default.GetBytes(certificate.CE_TextContainer) : certificate.CE_BinaryContainer;
            return File(fileByte, containerType.FileContentType, fileName);
        }

        [HttpGet]
        public String ContainerTypes()
        {
            return string.Join(";", ContainerType.List.Select(x => x.ID + ":" + x.DisplayName));
        }

        private IQueryable<eHubCertificate> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubCertificate> cetificates)
        {
            switch (filterQuery.groupOp)
            {
                case "AND":

                    foreach (var rule in filterQuery.rules)
                        cetificates = ApplyValuesFilter(rule.field, rule.data, rule.op, cetificates);
                    break;

                case "OR":

                    cetificates = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, cetificates);
                    for (int rule = 1; rule < filterQuery.rules.Count; rule++)
                    {
                        IQueryable<eHubCertificate> filterData = Context.eHubCertificates;
                        filterData = ApplyValuesFilter(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
                        cetificates = cetificates.AsQueryable().Union(filterData);
                    }
                    break;

                default:
                    break;
            }

            return cetificates;
        }


        static IQueryable<eHubCertificate> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubCertificate> cetificates)
        {
            switch (searchField)
            {
                case "CE_Category":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_Category == searchString);
                            break;
                        case "bw":
                            cetificates = cetificates.Where(r => r.CE_Category.StartsWith(searchString));
                            break;
                        case "ew":
                            cetificates = cetificates.Where(r => r.CE_Category.EndsWith(searchString));
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_Category.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_ID":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_ID == searchString);
                            break;
                        case "bw":
                            cetificates = cetificates.Where(r => r.CE_ID.StartsWith(searchString));
                            break;
                        case "ew":
                            cetificates = cetificates.Where(r => r.CE_ID.EndsWith(searchString));
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_ID.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_CC_ID":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.eHubClient.CC_ID == searchString);
                            break;
                        case "bw":
                            cetificates = cetificates.Where(r => r.eHubClient.CC_ID.StartsWith(searchString));
                            break;
                        case "ew":
                            cetificates = cetificates.Where(r => r.eHubClient.CC_ID.EndsWith(searchString));
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.eHubClient.CC_ID.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_EH_ID":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.eHubClientSystem.EH_ID == searchString);
                            break;
                        case "bw":
                            cetificates = cetificates.Where(r => r.eHubClientSystem.EH_ID.StartsWith(searchString));
                            break;
                        case "ew":
                            cetificates = cetificates.Where(r => r.eHubClientSystem.EH_ID.EndsWith(searchString));
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.eHubClientSystem.EH_ID.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_ValidFromUTC":
                    switch (searchOper)
                    {
                        case "gt":
                            cetificates = cetificates.Where(r => r.CE_ValidFromUTC > DateTime.Parse(searchString));
                            break;
                        case "ge":
                            cetificates = cetificates.Where(r => r.CE_ValidFromUTC >= DateTime.Parse(searchString));
                            break;
                        case "lt":
                            cetificates = cetificates.Where(r => r.CE_ValidFromUTC < DateTime.Parse(searchString));
                            break;
                        case "le":
                            cetificates = cetificates.Where(r => r.CE_ValidFromUTC <= DateTime.Parse(searchString));
                            break;
                    }
                    break;
                case "CE_ValidToUTC":
                    switch (searchOper)
                    {
                        case "gt":
                            cetificates = cetificates.Where(r => r.CE_ValidToUTC > DateTime.Parse(searchString));
                            break;
                        case "ge":
                            cetificates = cetificates.Where(r => r.CE_ValidToUTC >= DateTime.Parse(searchString));
                            break;
                        case "lt":
                            cetificates = cetificates.Where(r => r.CE_ValidToUTC < DateTime.Parse(searchString));
                            break;
                        case "le":
                            cetificates = cetificates.Where(r => r.CE_ValidToUTC <= DateTime.Parse(searchString));
                            break;
                    }
                    break;
                case "CE_ActiveFromUTC":
                    switch (searchOper)
                    {
                        case "gt":
                            cetificates = cetificates.Where(r => r.CE_ActiveFromUTC > DateTime.Parse(searchString));
                            break;
                        case "ge":
                            cetificates = cetificates.Where(r => r.CE_ActiveFromUTC >= DateTime.Parse(searchString));
                            break;
                        case "lt":
                            cetificates = cetificates.Where(r => r.CE_ActiveFromUTC < DateTime.Parse(searchString));
                            break;
                        case "le":
                            cetificates = cetificates.Where(r => r.CE_ActiveFromUTC <= DateTime.Parse(searchString));
                            break;
                    }
                    break;
                case "CE_AddedUTC":
                    switch (searchOper)
                    {
                        case "gt":
                            cetificates = cetificates.Where(r => r.CE_AddedUTC > DateTime.Parse(searchString));
                            break;
                        case "ge":
                            cetificates = cetificates.Where(r => r.CE_AddedUTC >= DateTime.Parse(searchString));
                            break;
                        case "lt":
                            cetificates = cetificates.Where(r => r.CE_AddedUTC < DateTime.Parse(searchString));
                            break;
                        case "le":
                            cetificates = cetificates.Where(r => r.CE_AddedUTC <= DateTime.Parse(searchString));
                            break;
                    }
                    break;
                case "CE_Thumbprint":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_Thumbprint == searchString);
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_Thumbprint.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_Issuer":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_Issuer == searchString);
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_Issuer.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_SerialNumber":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_SerialNumber == searchString);
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_SerialNumber.Contains(searchString));
                            break;
                    }
                    break;
                case "CE_SubjectKeyIdentifier":
                    switch (searchOper)
                    {
                        case "eq":
                            cetificates = cetificates.Where(r => r.CE_SubjectKeyIdentifier == searchString);
                            break;
                        case "cn":
                            cetificates = cetificates.Where(r => r.CE_SubjectKeyIdentifier.Contains(searchString));
                            break;
                    }
                    break;
            }
            return cetificates;
        }

        static IQueryable<eHubCertificate> ApplyValuesSort(string sidx, string sord, IQueryable<eHubCertificate> cetificates)
        {
            switch (sidx + " " + sord)
            {
                case "CE_CC_ID asc":
                    cetificates = cetificates.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_CC_ID desc":
                    cetificates = cetificates.OrderByDescending(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_EH_ID asc":
                    cetificates = cetificates.OrderBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_EH_ID desc":
                    cetificates = cetificates.OrderByDescending(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ValidFromUTC asc":
                    cetificates = cetificates.OrderBy(r => r.CE_ValidFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ValidFromUTC desc":
                    cetificates = cetificates.OrderByDescending(r => r.CE_ValidFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ValidToUTC asc":
                    cetificates = cetificates.OrderBy(r => r.CE_ValidToUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ValidToUTC desc":
                    cetificates = cetificates.OrderByDescending(r => r.CE_ValidToUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ActiveFromUTC asc":
                    cetificates = cetificates.OrderBy(r => r.CE_ActiveFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_ActiveFromUTC desc":
                    cetificates = cetificates.OrderByDescending(r => r.CE_ActiveFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
                case "CE_AddedUTC asc":
                    cetificates = cetificates.OrderBy(r => r.CE_ValidFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC);
                    break;
                case "CE_AddedUTC desc":
                    cetificates = cetificates.OrderByDescending(r => r.CE_ValidFromUTC).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC);
                    break;
                default:
                    cetificates = cetificates.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CE_Category).ThenBy(r => r.CE_ID).ThenBy(r => r.CE_ActiveFromUTC ?? r.CE_ValidFromUTC ?? r.CE_AddedUTC);
                    break;
            }
            return cetificates;
        }

        internal static Func<DateTime> GetDateTimeUtcNow = () => DateTime.UtcNow;
    }
}
