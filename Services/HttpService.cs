using PIX_BancoDoBrasil.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PIX_BancoDoBrasil.Services
{
    /// <summary>
    /// HttpService
    /// </summary>
    public class HttpService
    {
        private string _url;
        private string _codAcesso;
        private string _nomeFuncao;

        private AuthenticationHeaderValue _authenticationHeader;
        private HttpClientHandler _httpClientHandler;
        private List<KeyValuePair<string, string>> _headers;
        private List<MediaTypeWithQualityHeaderValue> _headersAccept;
        private string _content;
        private StringContent _stringContent;
        private int _timeout;
        private bool _ignoreCertificateValidation;
        public bool _resultBodyString;


        /// <summary>
        /// Classe de retorno
        /// </summary>
        public class Retorno
        {
            public bool Erro { get; set; }
            public string MensagemErro { get; set; }
            public HttpStatusCode HttpStatusCode { get; set; }
            public string Body { get; set; }
            public byte[] BodyArrayByte { get; set; }
            public List<KeyValuePair<string, string>> Headers { get; set; }
        }

        public void UrlSet(string url)
        {
            _url = url;
        }
        public void HandlerSet(HttpClientHandler httpClientHandler)
        {
            _httpClientHandler = httpClientHandler;
        }

        public void AuthenticationSet(AuthenticationHeaderValue authenticationHeader)
        {
            _authenticationHeader = authenticationHeader;
        }
        public void HeaderAcceptClear()
        {
            if (_headersAccept != null)
                _headersAccept.Clear();
        }
        public void HeaderAcceptAdd(MediaTypeWithQualityHeaderValue headerAccept)
        {
            if (_headersAccept == null)
            {
                _headersAccept = new List<MediaTypeWithQualityHeaderValue>();
            }
            _headersAccept.Add(headerAccept);
        }
        public void HeaderClear()
        {
            if (_headers != null)
                _headers.Clear();
        }
        public void HeaderAdd(string name, string value)
        {
            if (_headers == null)
            {
                _headers = new List<KeyValuePair<string, string>>();
            }
            _headers.Add(new KeyValuePair<string, string>(name, value));
        }
        public void PayLoadSet(string content, Encoding encoding, string mediaType)
        {
            _content = content;
            _stringContent = new StringContent(content, encoding, mediaType);
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

        public void ResultByteSet()
        {
            _resultBodyString = false;
        }
        public void ResultStringSet()
        {
            _resultBodyString = true;
        }

        public HttpService(string codAcesso, string nomeFuncao)
        {
            _codAcesso = codAcesso;
            _nomeFuncao = nomeFuncao;
            _timeout = 15;
            _resultBodyString = true;
            _ignoreCertificateValidation = false;
        }

        public Retorno ExecuteGet()
        {
            Retorno retorno = ExecuteGetAsync().Result;
            return retorno;
        }
        private async Task<Retorno> ExecuteGetAsync()
        {
            var retorno = new Retorno();
            var dataInicio = DateTime.Now;
            var acessosExternos = new AcessosExternos();
            var codAcessoExterno = acessosExternos.Inserir(_codAcesso, _url, _content);

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClient httpClient = null;
            if (_httpClientHandler != null)
                httpClient = new HttpClient(_httpClientHandler);
            else
                httpClient = new HttpClient();

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = "Set BaseAddress";

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            if (_authenticationHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;
            }

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                {
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);
                    }
                }
                if (_headers != null)
                {
                    foreach (KeyValuePair<string, string> header in _headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            byte[] responseBodyArrayByte = null;
            try
            {
                httpClient.Timeout = TimeSpan.FromMinutes(_timeout);
                response = httpClient.GetAsync(_url).Result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "TaskCanceledException";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "O nome remoto não pôde ser resolvido";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Impossível conectar-se ao servidor remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = ex.InnerException.ToString();

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }

                return retorno;
            }

            try
            {
                if (_resultBodyString)
                    responseBody = response.Content.ReadAsStringAsync().Result;
                else
                    responseBodyArrayByte = response.Content.ReadAsByteArrayAsync().Result;

            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = ex.InnerException.ToString();

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            responseBody = responseBody.Replace("'", "");

            retorno.Body = responseBody;
            retorno.BodyArrayByte = responseBodyArrayByte;
            retorno.HttpStatusCode = response.StatusCode;
            retorno.Headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
            {
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                {
                    retorno.Headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));
                }
            }

            acessosExternos.Atualizar(codAcessoExterno, responseBody, (int)response.StatusCode);
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
            var dataInicio = DateTime.Now;
            var acessosExternos = new AcessosExternos();
            var codAcessoExterno = acessosExternos.Inserir(_codAcesso, _url, _content);

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = "Set BaseAddress";

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            if (_authenticationHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;
            }

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                {
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);
                    }
                }
                if (_headers != null)
                {
                    foreach (KeyValuePair<string, string> header in _headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            byte[] responseBodyArrayByte = null;
            try
            {
                httpClient.Timeout = TimeSpan.FromMinutes(_timeout);
                response = httpClient.PostAsync(_url, _stringContent).Result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "TaskCanceledException";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "O nome remoto não pôde ser resolvido";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Impossível conectar-se ao servidor remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = ex.InnerException.ToString();

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }

                return retorno;
            }

            try
            {
                if (_resultBodyString)
                    responseBody = response.Content.ReadAsStringAsync().Result;
                else
                    responseBodyArrayByte = response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = ex.InnerException.ToString();

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            responseBody = responseBody.Replace("'", "");

            retorno.Body = responseBody;
            retorno.BodyArrayByte = responseBodyArrayByte;
            retorno.HttpStatusCode = response.StatusCode;
            retorno.Headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
            {
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                {
                    retorno.Headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));
                }
            }

            acessosExternos.Atualizar(codAcessoExterno, responseBody, (int)response.StatusCode);
            return retorno;
        }


        public Retorno ExecutePut()
        {
            Retorno retorno = ExecutePutAsync().Result;
            return retorno;
        }
        private async Task<Retorno> ExecutePutAsync()
        {
            var retorno = new Retorno();
            var dataInicio = DateTime.Now;
            var acessosExternos = new AcessosExternos();
            var codAcessoExterno = acessosExternos.Inserir(_codAcesso, _url, _content);

            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });

            try
            {
                httpClient.BaseAddress = new Uri(_url);
            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = "Set BaseAddress";

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            if (_authenticationHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;
            }

            if ((_headersAccept != null) || (_headers != null))
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                if (_headersAccept != null)
                {
                    foreach (MediaTypeWithQualityHeaderValue headerAccept in _headersAccept)
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(headerAccept);
                    }
                }
                if (_headers != null)
                {
                    foreach (KeyValuePair<string, string> header in _headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }

            HttpResponseMessage response = null;
            string responseBody = "";
            byte[] responseBodyArrayByte = null;
            try
            {
                httpClient.Timeout = TimeSpan.FromMinutes(_timeout);
                response = httpClient.PutAsync(_url, _stringContent).Result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("TaskCanceledException"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "TaskCanceledException";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("O nome remoto não pôde ser resolvido"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "O nome remoto não pôde ser resolvido";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Impossível conectar-se ao servidor remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Impossível conectar-se ao servidor remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else if (ex.InnerException.ToString().Contains("Foi forçado o cancelamento de uma conexão existente pelo host remoto"))
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = "Foi forçado o cancelamento de uma conexão existente pelo host remoto";

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }
                else
                {
                    retorno.Erro = true;
                    retorno.MensagemErro = ex.InnerException.ToString();

                    acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                }

                return retorno;
            }

            try
            {
                if (_resultBodyString)
                    responseBody = response.Content.ReadAsStringAsync().Result;
                else
                    responseBodyArrayByte = response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (Exception ex)
            {
                retorno.Erro = true;
                retorno.MensagemErro = ex.InnerException.ToString();

                acessosExternos.Atualizar(codAcessoExterno, retorno.MensagemErro, 404);
                return retorno;
            }

            responseBody = responseBody.Replace("'", "");

            retorno.Body = responseBody;
            retorno.BodyArrayByte = responseBodyArrayByte;
            retorno.HttpStatusCode = response.StatusCode;
            retorno.Headers = new List<KeyValuePair<string, string>>();
            foreach (var headerItem in response.Headers)
            {
                foreach (var valueItem in response.Headers.GetValues(headerItem.Key))
                {
                    retorno.Headers.Add(new KeyValuePair<string, string>(headerItem.Key, valueItem));
                }
            }

            acessosExternos.Atualizar(codAcessoExterno, responseBody, (int)response.StatusCode);
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