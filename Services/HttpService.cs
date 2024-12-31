using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PIX_BancoDoBrasil.Services
{
    /// <summary>
    /// HttpService
    /// </summary>
    public class HttpService : IDisposable
    {
        private string _diretorioLogArquivoUnitario;
        private string _msgErroInterno = "Falha de conexão.";

        private string _nomeFuncao;
        private string _url;

        private AuthenticationHeaderValue _authenticationHeader;
        private List<KeyValuePair<string, string>> _headers;
        private List<MediaTypeWithQualityHeaderValue> _headersAccept;
        private List<X509Certificate2> _cert;
        private string _payload;
        private StringContent _stringPayload;
        private int _timeout;
        private bool _ignoreCertificateValidation;

        /// <summary>
        /// Classe de retorno
        /// </summary>
        public class Retorno
        {
            public bool erro { get; set; }
            public string mensagemErro { get; set; }
            public string mensagemErroDetalhada { get; set; }
            public HttpStatusCode httpStatusCode { get; set; }
            public string responseBody { get; set; }
            public List<KeyValuePair<string, string>> headers { get; set; }
            public string codAcessoExterno;
        }
        public void NomeFuncaoSet(string nomeFuncao)
        {
            _nomeFuncao = nomeFuncao;
        }
        public void UrlSet(string url)
        {
            _url = url;
        }
        public void AuthenticationSet(AuthenticationHeaderValue authenticationHeader)
        {
            _authenticationHeader = authenticationHeader;
        }
        public void HeaderAcceptAdd(MediaTypeWithQualityHeaderValue headerAccept)
        {
            if (_headersAccept == null)
                _headersAccept = new List<MediaTypeWithQualityHeaderValue>();

            _headersAccept.Add(headerAccept);
        }
        public void HeaderAdd(string name, string value)
        {
            if (_headers == null)
                _headers = new List<KeyValuePair<string, string>>();

            _headers.Add(new KeyValuePair<string, string>(name, value));
        }
        public void CertificateAdd(X509Certificate2 cert)
        {
            if (_cert == null)
                _cert = new List<X509Certificate2>();

            _cert.Add(cert);
        }
        public void SecurityProtocolClear()
        {
            ServicePointManager.SecurityProtocol = 0;
        }
        public void SecurityProtocolAdd(SecurityProtocolType TipoProtocolo)
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | TipoProtocolo;
        }

        /// <summary>Adiciona o Payoload</summary>
        /// <remarks>
        /// Usado configurar o payload a ser enviado
        /// </remarks>
        /// <param name="payload">Dado a ser enviado</param>
        /// <param name="encoding">Codificação do dado</param>
        /// <param name="mediaType">Tipo de mídia</param>
        public void PayLoadSet(string payload, Encoding encoding, string mediaType)
        {
            _payload = payload;
            _stringPayload = new StringContent(payload, encoding, mediaType);
        }

        /// <summary>Configura o timeout</summary>
        /// <remarks>
        /// Usado para alterar o valor padrão de 3 minutos para o timeout da requisição
        /// </remarks>
        /// <param name="timeout">Numero inteiro em minutos</param>
        public void TimeoutSet(int timeout)
        {
            _timeout = timeout * 60; //Parametro em minutos sendo transformado em segundos
        }

        /// <summary>Configura o timeout</summary>
        /// <remarks>
        /// Usado para alterar o valor padrão de 3 minutos para o timeout da requisição
        /// </remarks>
        /// <param name="timeout">Numero inteiro em segundos</param>
        public void TimeoutSet_EmSegundos(int timeout)
        {
            _timeout = timeout;
        }

        /// <summary>Configura validação do certificado</summary>
        /// <remarks>
        /// Usado para ignorar a validação do certificado SSL
        /// </remarks>
        public void IgnoreCertificateValidationSet()
        {
            _ignoreCertificateValidation = true;
        }

        public HttpService(string nomeFuncao)
        {
            _nomeFuncao = nomeFuncao;
            _timeout = 3 * 60; //Configurando em segundos
            _ignoreCertificateValidation = false;

            if (ConfigurationManager.AppSettings["DiretorioLog"] != null)
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["DiretorioLog"].ToString()))
                    _diretorioLogArquivoUnitario = ConfigurationManager.AppSettings["DiretorioLog"].ToString();

        }
        public string GetConfig()
        {
            var config = new
            {
                url = _url,
                authenticationHeader = _authenticationHeader,
                header = _headers,
                request = _payload
            };

            return JsonConvert.SerializeObject(config);
        }
        public Retorno ExecuteGet()
        {
            Retorno retorno = ExecuteGetAsync().Result;
            return retorno;
        }
        private async Task<Retorno> ExecuteGetAsync()
        {
            var retorno = new Retorno();
            string dataInicio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");
            string dataFim = "";

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClient httpClient;
            if (_cert == null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                HttpClientHandler handler = new HttpClientHandler();
                foreach (X509Certificate2 cert in _cert)
                    handler.ClientCertificates.Add(cert);

                httpClient = new HttpClient(handler);
            }

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "Uri" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            if (_authenticationHeader != null)
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);

                if (_headers != null)
                    foreach (KeyValuePair<string, string> header in _headers)
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            string jsonHeaders = "";
            if ((_authenticationHeader != null) || (_headersAccept != null) || (_headers != null))
            {
                var headersDictionary = new Dictionary<string, string>();
                foreach (var header in httpClient.DefaultRequestHeaders)
                    headersDictionary[header.Key] = string.Join(",", header.Value);
                jsonHeaders = JsonConvert.SerializeObject(headersDictionary);
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            try
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_timeout);
                response = httpClient.GetAsync(_url).Result;
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "GetAsync" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    if (response != null)
                        messageException = messageException + "StatusCode" + response.StatusCode + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            try
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                retorno.erro = true;
                retorno.mensagemErro = _msgErroInterno;
                retorno.mensagemErroDetalhada = ex.Message;

                string messageException = "";
                messageException = messageException + "responseBody" + "\r\n";
                messageException = messageException + " --------------------------------------- " + "\r\n";
                messageException = messageException + "Message --- " + ex.Message + "\r\n";
                messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                messageException = messageException + "Source --- " + ex.Source + "\r\n";
                messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                if (response == null)
                    messageException = messageException + "response  --- null";

                //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
            }

            dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

            retorno.responseBody = responseBody;
            retorno.httpStatusCode = response.StatusCode;
            retorno.headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                    retorno.headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));

            return retorno;
        }


        public Retorno ExecutePost()
        {
            Retorno retorno = ExecutePostAsync().Result;
            return retorno;
        }
        private async Task<Retorno> ExecutePostAsync()
        {
            var retorno = new Retorno();
            string dataInicio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");
            string dataFim = "";

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClient httpClient;
            if (_cert == null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                HttpClientHandler handler = new HttpClientHandler();
                foreach (X509Certificate2 cert in _cert)
                    handler.ClientCertificates.Add(cert);

                httpClient = new HttpClient(handler);
            }

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "Uri" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            if (_authenticationHeader != null)
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);

                if (_headers != null)
                    foreach (KeyValuePair<string, string> header in _headers)
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            string jsonHeaders = "";
            if ((_authenticationHeader != null) || (_headersAccept != null) || (_headers != null))
            {
                var headersDictionary = new Dictionary<string, string>();
                foreach (var header in httpClient.DefaultRequestHeaders)
                    headersDictionary[header.Key] = string.Join(",", header.Value);
                jsonHeaders = JsonConvert.SerializeObject(headersDictionary);
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            try
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_timeout);
                response = httpClient.PostAsync(_url, _stringPayload).Result;
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "PostAsync" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    if (response != null)
                        messageException = messageException + "StatusCode" + response.StatusCode + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            try
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                retorno.erro = true;
                retorno.mensagemErro = _msgErroInterno;
                retorno.mensagemErroDetalhada = ex.Message;

                string messageException = "";
                messageException = messageException + "responseBody" + "\r\n";
                messageException = messageException + " --------------------------------------- " + "\r\n";
                messageException = messageException + "Message --- " + ex.Message + "\r\n";
                messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                messageException = messageException + "Source --- " + ex.Source + "\r\n";
                messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                if (response == null)
                    messageException = messageException + "response  --- null";

                //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
            }

            dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

            retorno.responseBody = responseBody;
            retorno.httpStatusCode = response.StatusCode;
            retorno.headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                    retorno.headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));

            return retorno;
        }


        public Retorno ExecutePut()
        {
            Retorno retorno = ExecutePutAsync().Result;
            return retorno;
        }
        private async Task<Retorno> ExecutePutAsync()
        {
            Retorno retorno = new Retorno();
            string dataInicio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");
            string dataFim = "";

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClient httpClient;
            if (_cert == null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                HttpClientHandler handler = new HttpClientHandler();
                foreach (X509Certificate2 cert in _cert)
                    handler.ClientCertificates.Add(cert);

                httpClient = new HttpClient(handler);
            }

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "Uri" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            if (_authenticationHeader != null)
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);

                if (_headers != null)
                    foreach (KeyValuePair<string, string> header in _headers)
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            string jsonHeaders = "";
            if ((_authenticationHeader != null) || (_headersAccept != null) || (_headers != null))
            {
                var headersDictionary = new Dictionary<string, string>();
                foreach (var header in httpClient.DefaultRequestHeaders)
                    headersDictionary[header.Key] = string.Join(",", header.Value);
                jsonHeaders = JsonConvert.SerializeObject(headersDictionary);
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            try
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_timeout);
                response = httpClient.PutAsync(_url, _stringPayload).Result;
            }
            catch (Exception ex)
            {
                dataFim = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff");

                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "TaskCanceledException";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O nome remoto não pôde ser resolvido";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Impossível conectar-se ao servidor remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.Message.ToString().Contains("Porta especificada inválida."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Porta especificada inválida";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A conexão subjacente estava fechada: A conexão foi fechada de modo inesperado."))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "A conexão foi fechada de modo inesperado";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "Não foi possível criar um canal seguro para SSL/TLS";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else if (ex.InnerException.ToString().Contains("O certificado remoto é inválido, de acordo com o procedimento de validação"))
                {
                    retorno.erro = true;
                    retorno.mensagemErro = "O certificado remoto é inválido, de acordo com o procedimento de validação";
                    retorno.mensagemErroDetalhada = ex.Message;
                }
                else
                {
                    retorno.erro = true;
                    retorno.mensagemErro = _msgErroInterno;
                    retorno.mensagemErroDetalhada = ex.Message;

                    string messageException = "";
                    messageException = messageException + "PutAsync" + "\r\n";
                    messageException = messageException + "caminhoServico --- " + _url + "\r\n";
                    messageException = messageException + "DefaultRequestHeaders --- " + httpClient.DefaultRequestHeaders.ToString() + "\r\n";
                    messageException = messageException + " --------------------------------------- " + "\r\n";
                    if (response != null)
                        messageException = messageException + "StatusCode" + response.StatusCode + "\r\n";
                    messageException = messageException + "Message --- " + ex.Message + "\r\n";
                    messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                    messageException = messageException + "Source --- " + ex.Source + "\r\n";
                    messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                    messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                    messageException = messageException + "InnerException --- " + ex.InnerException + "\r\n";

                    //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
                }

                return retorno;
            }

            try
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                retorno.erro = true;
                retorno.mensagemErro = _msgErroInterno;
                retorno.mensagemErroDetalhada = ex.Message;

                string messageException = "";
                messageException = messageException + "responseBody" + "\r\n";
                messageException = messageException + " --------------------------------------- " + "\r\n";
                messageException = messageException + "Message --- " + ex.Message + "\r\n";
                messageException = messageException + "HelpLink --- " + ex.HelpLink + "\r\n";
                messageException = messageException + "Source --- " + ex.Source + "\r\n";
                messageException = messageException + "StackTrace --- " + ex.StackTrace + "\r\n";
                messageException = messageException + "TargetSite --- " + ex.TargetSite + "\r\n";
                if (response == null)
                    messageException = messageException + "response  --- null";

                //string NomeArquivoLog = Logging.DebugEmArquivoTxt(_nomeFuncao, "catch", messageException, _diretorioLogArquivoUnitario);
            }

            retorno.responseBody = responseBody;
            retorno.httpStatusCode = response.StatusCode;
            retorno.headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                    retorno.headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));

            return retorno;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
        }

    }

}