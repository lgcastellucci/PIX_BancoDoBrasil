using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PIX_BancoDoBrasil.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;

namespace PIX_BancoDoBrasil.Services
{
    public class BancoDoBrasil
    {
        public class Resposta
        {
            public bool sucesso { get; set; }
            public string mensagem { get; set; }
            public Resposta()
            {
                sucesso = false;
                mensagem = "";
            }
        }
        public class RespostaToken : Resposta
        {
            public string accessToken { get; set; }
            public RespostaToken()
            {
                accessToken = "";
            }
        }
        public class RespostaPix : Resposta
        {
            public DateTime dataCriacao { get; set; }
            public string txid { get; set; }
            public string pixCopiaECola { get; set; }
            public RespostaPix()
            {
                dataCriacao = DateTime.Now;
                txid = "";
                pixCopiaECola = "";
            }
        }
        public class RespostaStatusPix : Resposta
        {
            public string status { get; set; }
            public RespostaStatusPix()
            {
                status = "";
            }
        }
        public RespostaToken PegarToken(string codAcesso)
        {
            string nomeFuncao = "PegarToken";
            var resposta = new RespostaToken();

            // Permissoes
            // cob.write - Permissão para alteração de cobranças imediatas
            // cob.read - Permissão para consulta de cobranças imediatas
            // cobv.write - Permissão para alteração de cobranças com vencimento
            // cobv.read - Permissão para consulta de cobranças com vencimento
            // lotecobv.write - Permissão para alteração de lotes de cobranças com vencimento
            // lotecobv.read - Permissão para consulta de lotes de cobranças com vencimento
            // pix.write - Permissão para alteração de Pix
            // pix.read - Permissão para consulta de Pix
            // webhook.read - Permissão para consulta do webhook
            // webhook.write - Permissão para alteração do webhook
            // payloadlocation.write - Permissão para alteração de payloads
            // payloadlocation.read - Permissão para consulta de payloads             

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "cob.read cob.write pix.read pix.write webhook.read webhook.write"));
            string urlEncodedString = new FormUrlEncodedContent(keyValues).ReadAsStringAsync().Result;

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes(ReadConf.clientId() + ":" + ReadConf.clientSecret()))));
            httpService.UrlSet("https://oauth.sandbox.bb.com.br/oauth/token");
            httpService.PayLoadSet(urlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecutePost();

            if (retHttp.HttpStatusCode != HttpStatusCode.Created)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            }

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.Body);
            }
            catch
            {
                resposta.mensagem = "JObject.Parse: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("access_token") == null)
            {
                resposta.mensagem = "access_token: " + retHttp.Body;
                return resposta;
            }

            if (string.IsNullOrWhiteSpace(dataJson.SelectToken("access_token").ToString()))
            {
                resposta.mensagem = "access_token: " + retHttp.Body;
                return resposta;
            }

            resposta.sucesso = true;
            resposta.accessToken = dataJson.SelectToken("access_token").ToString();
            return resposta;
        }

        public Resposta ValidarWebhook(string codAcesso, string accessToken)
        {
            string nomeFuncao = "ValidarWebhook";
            var resposta = new Resposta();

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet("https://api.hm.bb.com.br/pix/v2/webhook/" + ReadConf.chavePix() + "?gw-dev-app-key=" + ReadConf.developer_application_key());
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecuteGet();

            if (retHttp.HttpStatusCode != HttpStatusCode.OK)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            }

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.Body);
            }
            catch
            {
                resposta.mensagem = "JObject.Parse: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("webhookUrl") == null)
            {
                var retCadastraWebhook = CadastrarWebhook(codAcesso, accessToken);
                return retCadastraWebhook;
            }

            if (dataJson.SelectToken("webhookUrl").ToString() != ReadConf.webhookUrl())
            {
                var retDeletarWebhook = DeletarWebhook(codAcesso, accessToken);

                var retCadastraWebhook = CadastrarWebhook(codAcesso, accessToken);
                return retCadastraWebhook;
            }

            resposta.sucesso = true;
            return resposta;
        }

        public Resposta CadastrarWebhook(string codAcesso, string accessToken)
        {
            string nomeFuncao = "CadastrarWebhook";
            var resposta = new Resposta();

            var payLoad = new
            {
                webhookUrl = ReadConf.webhookUrl()
            };

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet("https://api.hm.bb.com.br/pix/v2/webhook/" + ReadConf.chavePix() + "?gw-dev-app-key=" + ReadConf.developer_application_key());
            httpService.PayLoadSet(JsonConvert.SerializeObject(payLoad), Encoding.UTF8, "application/json");
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecutePut();

            if (retHttp.HttpStatusCode != HttpStatusCode.OK)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            };

            resposta.sucesso = true;
            return resposta;
        }

        public Resposta DeletarWebhook(string codAcesso, string accessToken)
        {
            string nomeFuncao = "DeletarWebhook";
            var resposta = new Resposta();

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet("https://api.hm.bb.com.br/pix/v2/webhook/" + ReadConf.chavePix() + "?gw-dev-app-key=" + ReadConf.developer_application_key());
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecuteDelete();

            if (retHttp.HttpStatusCode != HttpStatusCode.OK)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            };

            resposta.sucesso = true;
            return resposta;
        }

        public RespostaPix CriarPix(string codAcesso, string accessToken, string strPayLoad)
        {
            string nomeFuncao = "CadastrarWebhook";
            var resposta = new RespostaPix();

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet("https://api.hm.bb.com.br/pix/v2/cob?gw-dev-app-key=" + ReadConf.developer_application_key());
            httpService.PayLoadSet(strPayLoad, Encoding.UTF8, "application/json");
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecutePost();

            if (retHttp.HttpStatusCode != HttpStatusCode.Created)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            }

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.Body);
            }
            catch
            {
                resposta.mensagem = "JObject.Parse: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("txid") == null)
            {
                resposta.mensagem = "txid: " + retHttp.Body;
                return resposta;
            }
            if (dataJson.SelectToken("pixCopiaECola") == null)
            {
                resposta.mensagem = "pixCopiaECola: " + retHttp.Body;
                return resposta;
            }

            try
            {
                resposta.dataCriacao = Convert.ToDateTime(dataJson.SelectToken("calendario").SelectToken("criacao").ToString());
            }
            catch
            {

            }

            resposta.sucesso = true;
            resposta.txid = dataJson.SelectToken("txid").ToString();
            resposta.pixCopiaECola = dataJson.SelectToken("pixCopiaECola").ToString();
            return resposta;
        }

        public RespostaStatusPix ConsultarPixPeloExtrato(string codAcesso, string accessToken, string devedorCPF, string devedorNome, string valor)
        {
            string nomeFuncao = "ConsultaPixPeloExtrato";
            var resposta = new RespostaStatusPix();

            if (string.IsNullOrWhiteSpace(accessToken))
                accessToken = PegarToken(codAcesso).accessToken;

            string url = "https://api.hm.bb.com.br/pix/v2/cob?";
            url += "inicio=" + DateTime.Now.AddHours(-24).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            url += "&fim=" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            url += "&gw-dev-app-key=" + ReadConf.developer_application_key();

            var payload = new
            {
                calendario = new { },
                devedor = new
                {
                    cpf = devedorCPF,
                    nome = devedorNome
                },
                valor = new
                {
                    original = valor
                },
                chave = ReadConf.chavePix(),
            };


            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet(url);
            httpService.PayLoadSet(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecutePost();

            if (retHttp.HttpStatusCode != HttpStatusCode.Created)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            }

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.Body);
            }
            catch
            {
                resposta.mensagem = "JObject.Parse: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("txid") == null)
            {
                resposta.mensagem = "txid: " + retHttp.Body;
                return resposta;
            }
            if (dataJson.SelectToken("status") == null)
            {
                resposta.mensagem = "status: " + retHttp.Body;
                return resposta;
            }

            try
            {
                resposta.status = dataJson.SelectToken("status").ToString();
            }
            catch
            {

            }

            resposta.sucesso = true;
            return resposta;
        }

        public RespostaStatusPix ConsultarPixPeloTxID(string codAcesso, string accessToken, string txid)
        {
            string nomeFuncao = "ConsultaPixPeloExtrato";
            var resposta = new RespostaStatusPix();

            if (string.IsNullOrWhiteSpace(accessToken))
                accessToken = PegarToken(codAcesso).accessToken;

            string url = "https://api.hm.bb.com.br/pix/v2/cob/" +txid + "?";
            url += "&gw-dev-app-key=" + ReadConf.developer_application_key();

            var httpService = new HttpService(codAcesso, nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet(url);
            httpService.IgnoreCertificateValidationSet();
            var retHttp = httpService.ExecuteGet();

            if (retHttp.HttpStatusCode != HttpStatusCode.OK)
            {
                resposta.mensagem = "HttpStatusCode: " + retHttp.HttpStatusCode;
                return resposta;
            }

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.Body);
            }
            catch
            {
                resposta.mensagem = "JObject.Parse: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("txid") == null)
            {
                resposta.mensagem = "txid: " + retHttp.Body;
                return resposta;
            }
            if (dataJson.SelectToken("status") == null)
            {
                resposta.mensagem = "status: " + retHttp.Body;
                return resposta;
            }

            if (dataJson.SelectToken("txid").ToString() != txid)
            {
                resposta.mensagem = "txid: " + retHttp.Body;
                return resposta;
            }


            try
            {
                resposta.status = dataJson.SelectToken("status").ToString();
            }
            catch
            {

            }

            resposta.sucesso = true;
            return resposta;
        }

    }
}