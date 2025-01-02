using PIX_BancoDoBrasil.Services;
using System;
using System.Web.UI;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using QRCoder;
using PIX_BancoDoBrasil.Models;

namespace PIX_BancoDoBrasil
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGerarPIX_Click(object sender, EventArgs e)
        {
            string nomeFuncao = "btn_gerarPix_Click";

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

            var respValidaWebhook = bancoDoBrasil.ValidarWebhook(codAcesso, respAccessToken.accessToken);
            if (!respValidaWebhook.sucesso)
            {
                //lblMensagem.Text = accessToken.mensagem;
                acessos.Atualizar(codAcesso, "", "", respValidaWebhook.mensagem);
                return;
            }

            int expiracaoSegundos = 3600;
            string devedorCPF = "11122233396";
            string devedorNome = "Jose da Silva";
            double valor = 10.00;

            if (!string.IsNullOrWhiteSpace(txtCPF.Text))
                devedorCPF = txtCPF.Text.Replace(".", "").Replace("-", "");
            if (!string.IsNullOrWhiteSpace(txtValor.Text))
                valor = Convert.ToDouble(txtValor.Text);

            var minhaIdentificacao = Guid.NewGuid().ToString().Replace("-", "");

            var payload = new
            {
                chave = ReadConf.chavePix(),
                calendario = new { expiracao = expiracaoSegundos.ToString() }, //
                devedor = new { cpf = devedorCPF, nome = devedorNome },
                valor = new { original = valor.ToString("N2").Replace(".", "").Replace(",", ".") },
                infoAdicionais = new[] {
                    new { nome = "MinhaIdentificacao", valor = minhaIdentificacao }
                }
            };

            var tabelaPix = new TabelaPix();
            var codPix = tabelaPix.Inserir(minhaIdentificacao, devedorCPF, devedorNome, valor.ToString("N2").Replace(".", "").Replace(",", "."));

            var respCriarPix = bancoDoBrasil.CriarPix(codAcesso, respAccessToken.accessToken, JsonConvert.SerializeObject(payload));
            if (!respCriarPix.sucesso)
            {
                //lblMensagem.Text = accessToken.mensagem;
                acessos.Atualizar(codAcesso, "", "", respCriarPix.mensagem);
                return;
            }

            var dataExpiracao = respCriarPix.dataCriacao.AddSeconds(expiracaoSegundos);

            tabelaPix.Atualizar(codPix, dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"), respCriarPix.txid, respCriarPix.pixCopiaECola);


            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(respCriarPix.pixCopiaECola, QRCodeGenerator.ECCLevel.L);
            var qrCode = new QRCode(qrCodeData);

            var qrCodeImage = qrCode.GetGraphic(4);
            //Bitmap qrCodeImageMenor = new Bitmap(qrCodeImage, new Size(250, 250));
            var qrCodeImageBW = qrCodeImage.Clone(new Rectangle(0, 0, qrCodeImage.Width, qrCodeImage.Height), PixelFormat.Format1bppIndexed); //228x228

            var ms = new MemoryStream();
            qrCodeImageBW.Save(ms, ImageFormat.Png);

            byte[] byteImage = ms.ToArray();
            var qrCodeImageAsBase64 = Convert.ToBase64String(byteImage); // Get Base64  

            // Exibir a imagem na página sem gravar em disco
            imgQRCode.ImageUrl = "data:image/png;base64," + qrCodeImageAsBase64;


            txtValorGerado.Text = "Valor R$ " + valor.ToString("N2");
            txtPixCopiaECola.Text = respCriarPix.pixCopiaECola;
            divQRCode.Style["display"] = "block";

            divGerarPix.Style["display"] = "none";

            txtValidoAte.Text = "Válido Até " + dataExpiracao.ToString("dd/MM/yyyy HH:mm");

            //hiddenDataExpiracao.Value = dataExpiracao.ToString("o"); // Formato ISO 8601
            //ScriptManager.RegisterStartupScript(this, GetType(), "iniciarContagemRegressiva", "iniciarContagemRegressiva();", true);
        }


    }
}