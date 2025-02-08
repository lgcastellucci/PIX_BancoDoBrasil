using Newtonsoft.Json;
using PIX_BancoDoBrasil.Models;
using PIX_BancoDoBrasil.Services;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace PIX_BancoDoBrasil.Controllers
{
    public class ConsultaController : ApiController
    {
        public class RespostConsultaPixBancodDoBrasil
        {
            public string status { get; set; }
            public string mensagem { get; set; }
            public RespostConsultaPixBancodDoBrasil()
            {
                status = "ERRO";
                mensagem = "";
            }
        }


        [AcceptVerbs("GET")]
        [Route("consulta/BancodDoBrasil/pix/{MinhaIdentificacao}")]
        public HttpResponseMessage ConsultarPixBancodDoBrasil(string minhaIdentificacao)
        {
            var resposta = new RespostConsultaPixBancodDoBrasil();

            var httpRequest = HttpContext.Current.Request;
            var acessos = new Acessos();
            string codAcesso = acessos.Inserir("POST", httpRequest.Url.ToString(), "", httpRequest.UserHostAddress);

            string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                var txid = "";
                sqlConnection.Open();

                var sbInstrucao = new StringBuilder();
                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append(" SELECT COD_PIX, MEU_ID, DEVEDOR_CPF, DEVEDOR_NOME, VALOR, TXID, STATUS ");
                sbInstrucao.Append(" FROM PIX ");
                sbInstrucao.Append(" WHERE MEU_ID = '" + minhaIdentificacao + "'");
                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            txid = dataReader["TXID"].ToString();
                            resposta.status = dataReader["STATUS"].ToString();
                        }
                    }
                    dataReader.Close();
                }

                if (string.IsNullOrWhiteSpace(resposta.status) && !string.IsNullOrWhiteSpace(txid))
                {
                    var bancoDoBrasil = new BancoDoBrasil();
                    var respostaConsultaPix = bancoDoBrasil.ConsultarPixPeloTxID(codAcesso, "", txid);

                    sbInstrucao.Remove(0, sbInstrucao.Length);
                    sbInstrucao.Append(" SELECT COD_PIX, MEU_ID, DEVEDOR_CPF, DEVEDOR_NOME, VALOR, TXID, STATUS ");
                    sbInstrucao.Append(" FROM PIX ");
                    sbInstrucao.Append(" WHERE MEU_ID = '" + minhaIdentificacao + "'");
                    using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                    {
                        var dataReader = cmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                resposta.status = dataReader["STATUS"].ToString();
                            }
                        }
                        dataReader.Close();
                    }
                }
            }

            acessos.Atualizar(codAcesso, JsonConvert.SerializeObject(resposta), "", resposta.mensagem);

            var retHttp = new HttpResponseMessage();
            retHttp.StatusCode = HttpStatusCode.NotAcceptable;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(resposta));
            retHttp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            retHttp.StatusCode = HttpStatusCode.OK;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(resposta));
            return retHttp;

        }

    }
}