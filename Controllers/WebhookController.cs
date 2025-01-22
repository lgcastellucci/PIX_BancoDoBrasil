using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIX_BancoDoBrasil.Models;
using PIX_BancoDoBrasil.Services;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace PIX_BancoDoBrasil.Controllers
{
    public class WebhookController : ApiController
    {
        public class RespostaWebhook
        {
            public string status { get; set; }
            public string mensagem { get; set; }
            public RespostaWebhook()
            {
                status = "ERRO";
                mensagem = "";
            }
        }


        [AcceptVerbs("POST")]
        [Route("webhook/BancodDoBrasil/pix")]
        public HttpResponseMessage ProcessaPixItau(JObject value)
        {
            var respostaWebhook = new RespostaWebhook();

            var retHttp = new HttpResponseMessage();
            retHttp.StatusCode = HttpStatusCode.NotAcceptable;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
            retHttp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


            var httpRequest = HttpContext.Current.Request;

            var acessos = new Acessos();
            string codAcesso = acessos.Inserir("POST", httpRequest.Url.ToString(), JsonConvert.SerializeObject(value), httpRequest.UserHostAddress);

            // Exemplo de retorno do BancodDoBrasil
            //{"pix":[
            //          {
            //            "endToEndId":"E00000000202501021748Z52KV5PYYSD",
            //            "txid":"ZxQm8lyQbHHQNfOz7l4zkSQTJMi2yTaD3Zb",
            //            "valor":"10.00",
            //            "componentesValor":{"original":{"valor":"10.00"}},
            //            "chave":"9e881f18-cc66-4fc7-8f2c-a795dbb2bfc1",
            //            "horario":"2025-01-02T14:48:02-03:00",
            //            "infoPagador":"Solicitacao Pix",
            //            "pagador":{"cpf":"93492239293","nome":"VICTOR LOPES DORNELES"}
            //          }
            //       ]}

            if (value.SelectToken("pix") == null)
            {
                respostaWebhook.status = "ERRO";
                respostaWebhook.mensagem = "Campo pix nao encontrado";

                acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(respostaWebhook), "", respostaWebhook.mensagem);

                retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
                return retHttp;
            }

            if (value.SelectToken("pix").Count() == 0)
            {
                respostaWebhook.status = "ERRO";
                respostaWebhook.mensagem = "Campo pix não é um array";

                acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(respostaWebhook), "", respostaWebhook.mensagem);

                retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
                return retHttp;
            }

            if (value.SelectToken("pix")[0].SelectToken("txid") == null)
            {
                respostaWebhook.status = "ERRO";
                respostaWebhook.mensagem = "Campo txid não é um array";

                acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(respostaWebhook), "", respostaWebhook.mensagem);

                retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
                return retHttp;
            }

            string txid = "";
            txid = value.SelectToken("pix")[0].SelectToken("txid").ToString();
            bool lEncontrouTxid = false;

            string codPix = "";
            string devedorCpf = "";
            string devedorNome = "";
            string valor = "";


            string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sbInstrucao = new StringBuilder();
                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append(" SELECT COD_PIX, MEU_ID, DEVEDOR_CPF, DEVEDOR_NOME, VALOR, TXID ");
                sbInstrucao.Append(" FROM PIX ");
                sbInstrucao.Append(" WHERE TXID = '" + txid + "'");
                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            //Depois comparar campos MEU_ID, DEVEDOR_CPF, VALOR
                            if (txid == dataReader["TXID"].ToString())
                            {
                                lEncontrouTxid = true;
                                codPix = dataReader["COD_PIX"].ToString();
                                devedorCpf = dataReader["DEVEDOR_CPF"].ToString();
                                devedorNome = dataReader["DEVEDOR_NOME"].ToString();
                                valor = dataReader["VALOR"].ToString().Replace(".", "").Replace(",", ".");
                            }
                        }
                    }
                }
            }

            if (!lEncontrouTxid)
            {
                respostaWebhook.status = "ERRO";
                respostaWebhook.mensagem = "Não encontrou txid: " + txid;

                acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(respostaWebhook), "", respostaWebhook.mensagem);

                retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
                return retHttp;
            }

            var bancoDoBrasil = new BancoDoBrasil();
            var respostaConsultaPix = bancoDoBrasil.ConsultarPixPeloTxID(codAcesso, "", txid);
            //var respostaConsultaPix = bancoDoBrasil.ConsultarPixPeloExtrato(codAcesso, "", devedorCpf, devedorNome, valor);

            if (!respostaConsultaPix.sucesso)
            {
                respostaWebhook.status = "ERRO";
                respostaWebhook.mensagem = respostaConsultaPix.mensagem;
            }

            var tabelaPix = new TabelaPix();
            tabelaPix.AtualizarStatus(codPix, respostaConsultaPix.status);
            if (!string.IsNullOrWhiteSpace(respostaConsultaPix.e2e))
                tabelaPix.AtualizarEndToEnd(codPix, respostaConsultaPix.e2e);

            respostaWebhook.status = "OK";
            respostaWebhook.mensagem = "Recebido com sucesso";

            acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(respostaWebhook), "", respostaWebhook.mensagem);

            retHttp.StatusCode = HttpStatusCode.OK;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(respostaWebhook));
            return retHttp;

        }

    }
}