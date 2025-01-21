using Newtonsoft.Json;
using PIX_BancoDoBrasil.Models;
using PIX_BancoDoBrasil.Services;
using System;
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
        }
    }
}
