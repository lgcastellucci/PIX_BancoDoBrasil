using Newtonsoft.Json;
using PIX_BancoDoBrasil.Models;
using PIX_BancoDoBrasil.Services;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace PIX_BancoDoBrasil
{
    public partial class SimularPagamento : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLerQRCode_Click(object sender, EventArgs e)
        {
            // A lógica de leitura do QR code é tratada no JavaScript
        }

        protected void btnEnviarPagamento_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(txtQRCode.Text))
            //    return;

            // Acessar o valor do TextBox diretamente do formulário
            string qrCodeMessage = Request.Form[txtQRCode.UniqueID];
            if (string.IsNullOrWhiteSpace(qrCodeMessage))
                return;

            string nomeFuncao = "btnEnviarPagamento_Click";

            var acessos = new Acessos();
            var codAcesso = acessos.Inserir("", nomeFuncao, "", "");

            var bancoDoBrasil = new BancoDoBrasil();

            var respAccessToken = bancoDoBrasil.PegarToken(codAcesso);
            if (!respAccessToken.sucesso)
            {
                //lblMensagem.Text = accessToken.mensagem;
                acessos.Atualizar(codAcesso, "", "", respAccessToken.mensagem);
                return;
            }

            var payload = new { pix = qrCodeMessage };
            var respSimularPagamento = bancoDoBrasil.SimularPagamento(codAcesso, respAccessToken.accessToken, JsonConvert.SerializeObject(payload));
            if (!respSimularPagamento.sucesso)
            {
                //lblMensagem.Text = accessToken.mensagem;
                acessos.Atualizar(codAcesso, "", "", respSimularPagamento.mensagem);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sbInstrucao = new StringBuilder();
                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append(" SELECT COD_PIX, TXID ");
                sbInstrucao.Append(" FROM PIX ");
                sbInstrucao.Append(" WHERE PIX_COPIA_E_COLA = '" + qrCodeMessage + "'");
                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            var respostaConsultaPix = bancoDoBrasil.ConsultarPixPeloTxID(codAcesso, respAccessToken.accessToken, dataReader["TXID"].ToString());
                            if (respostaConsultaPix.sucesso)
                            {
                                var tabelaPix = new TabelaPix();
                                tabelaPix.AtualizarStatus(dataReader["COD_PIX"].ToString(), respostaConsultaPix.status);
                                if (!string.IsNullOrWhiteSpace(respostaConsultaPix.e2e))
                                    tabelaPix.AtualizarEndToEnd(dataReader["COD_PIX"].ToString(), respostaConsultaPix.e2e);
                            }

                        }
                    }
                }
            }


        }
    }
}
