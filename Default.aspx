<%@ Page Title="PIX_BancoDoBrasil" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PIX_BancoDoBrasil._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery.mask/1.14.16/jquery.mask.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#<%= txtCPF.ClientID %>').mask('000.000.000-00');
            $('#<%= txtValor.ClientID %>').mask('000.000.000.000.000,00', { reverse: true });
        });

        function copiarQRCode() {
            var txtPixCopiaECola = document.getElementById('<%= txtPixCopiaECola.ClientID %>');
            if (txtPixCopiaECola.value) {
                navigator.clipboard.writeText(txtPixCopiaECola.value).then(function () {
                    alert('Texto copiado com sucesso!');
                }, function (err) {
                    alert('Erro ao copiar texto: ' + err);
                });
            } else {
                alert('Nenhum texto disponível para copiar.');
            }
        }

        function iniciarContagemRegressiva() {
            var dataExpiracao = new Date($('#<%= hiddenDataExpiracao.ClientID %>').val());
            var countdownElement = $('#countdown');

            function updateCountdown() {
                var now = new Date();
                var timeRemaining = dataExpiracao - now;

                if (timeRemaining > 0) {
                    var minutes = Math.floor((timeRemaining % (1000 * 60 * 60)) / (1000 * 60));
                    var seconds = Math.floor((timeRemaining % (1000 * 60)) / 1000);

                    countdownElement.text(minutes + "m " + seconds + "s ");
                } else {
                    countdownElement.text("Expirado");
                }
            }

            setInterval(updateCountdown, 1000);
        }

    </script>

    <main class="d-flex justify-content-center align-items-center" style="height: 100vh;">
        <div class="d-flex justify-content-center">
            <div id="divGerarPix" runat="server" class="rounded p-4" style="border: 1px solid #ccc; max-width: 400px; display: block;">
                <div class="row justify-content-center mb-3">
                    <section class="col-md-12">
                        <h5>Digite seu CPF</h5>
                        <asp:TextBox ID="txtCPF" runat="server" CssClass="form-control mx-auto" placeholder="CPF" Style="max-width: 300px;"></asp:TextBox>
                    </section>
                </div>
                <div class="row justify-content-center mb-3">
                    <section class="col-md-12">
                        <h5>Digite o valor em R$</h5>
                        <asp:TextBox ID="txtValor" runat="server" CssClass="form-control mx-auto" placeholder="Valor em R$" Style="max-width: 300px;"></asp:TextBox>
                    </section>
                </div>
                <div class="row justify-content-center">
                    <section class="col-md-12">
                        <asp:Button ID="btnGerarPIX" runat="server" CssClass="btn btn-primary w-100" Text="Gerar PIX" OnClick="btnGerarPIX_Click" />
                    </section>
                </div>
            </div>

            <div id="divQRCode" runat="server" class="rounded p-4" style="border: 1px solid #ccc; max-width: 400px; display: none;">
                <div class="row justify-content-center mb-3">
                    <section class="col-md-12 text-center">
                        <h5>
                            <asp:Label ID="txtValorGerado" Text="" runat="server" />
                        </h5>
                    </section>
                    <section class="col-md-12 text-center">
                        <asp:Label ID="txtValidoAte" Text="" runat="server" />
                    </section>
                </div>
                <div class="row justify-content-center mt-3">
                    <section class="col-md-12 text-center">
                        <asp:Image ID="imgQRCode" runat="server" />
                    </section>
                </div>
                <div class="row justify-content-center mb-3">
                    <section class="col-md-12 text-center">
                        <h5>Copia e Cola</h5>
                        <asp:TextBox ID="txtPixCopiaECola" runat="server" CssClass="form-control mx-auto" Style="max-width: 300px;"></asp:TextBox>
                    </section>
                </div>
                <div class="row justify-content-center">
                    <section class="col-md-12 text-center">
                        <asp:Button ID="btnCopiarQRCode" runat="server" CssClass="btn btn-primary w-100" Text="Copiar QR Code" OnClientClick="copiarQRCode(); return false;" />
                    </section>
                </div>
                <asp:HiddenField ID="hiddenDataExpiracao" runat="server" />
            </div>
        </div>
    </main>

</asp:Content>
