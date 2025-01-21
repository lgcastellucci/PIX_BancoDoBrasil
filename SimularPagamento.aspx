<%@ Page Title="PIX_BancoDoBrasil" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SimularPagamento.aspx.cs" Inherits="PIX_BancoDoBrasil.SimularPagamento" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="d-flex justify-content-center align-items-center" style="height: 100vh;">
        <div class="d-flex justify-content-center">
            <div id="divSimularPagamento" runat="server" class="rounded p-4" style="border: 1px solid #ccc; display: block;">
                <div id="qr-reader" style="width: 300px; height: 300px;"></div>

                <div id="divBtnPermitirCamera" runat="server" class="row justify-content-center mt-3">
                    <section class="col-md-12">
                        <asp:Button ID="btnPermitirCamera" runat="server" Text="Permitir Uso da Câmera" CssClass="btn btn-primary w-100" OnClientClick="solicitarPermissaoCamera(); return false;" />
                    </section>
                </div>

                <div id="divTxtQRCode" runat="server" class="row justify-content-center mt-3">
                    <section class="col-md-12 text-center">
                        <asp:TextBox ID="txtQRCode" runat="server" CssClass="form-control mt-3" ReadOnly="true" />
                    </section>
                </div>

                <div id="divBtnEnviarPagamento" runat="server" class="row justify-content-center mt-3">
                    <section class="col-md-12 text-center">
                        <asp:Button ID="btnEnviarPagamento" runat="server" Text="Enviar Pagamento" CssClass="btn btn-success w-100" OnClick="btnEnviarPagamento_Click" />
                    </section>
                </div>

            </div>
        </div>
    </main>
    <script src="Scripts/html5-qrcode.min.js"></script>
    <script>
        function solicitarPermissaoCamera() {
            navigator.mediaDevices.getUserMedia({ video: true })
                .then(function (stream) {
                    stream.getTracks().forEach(track => track.stop());
                    document.getElementById('<%= divBtnPermitirCamera.ClientID %>').style.display = 'none';

                    document.getElementById('<%= divTxtQRCode.ClientID %>').style.display = 'none';
                    document.getElementById('<%= divBtnEnviarPagamento.ClientID %>').style.display = 'none';

                    console.log("Permissão concedida. Agora você pode ler o QR Code.");
                })
                .catch(function (err) {
                    console.log("Permissão negada: " + err);
                });
        }

        function abrirCamera() {
            const html5QrCode = new Html5Qrcode("qr-reader");
            html5QrCode.start(
                { facingMode: "environment" },
                {
                    fps: 10,
                    qrbox: 300
                },
                qrCodeMessage => {
                    document.getElementById('<%= txtQRCode.ClientID %>').value = qrCodeMessage;
                    html5QrCode.stop();

                    document.getElementById('<%= divTxtQRCode.ClientID %>').style.display = 'block';
                    document.getElementById('<%= divBtnEnviarPagamento.ClientID %>').style.display = 'block';

                    //html5QrCode.clear();
                },
                errorMessage => {
                    console.log(`QR Code no match: ${errorMessage}`);
                }
            ).catch(err => {
                console.log(`Unable to start scanning, error: ${err}`);
            });
        }

        window.onload = function () {
            document.getElementById('<%= divTxtQRCode.ClientID %>').style.display = 'none';
            document.getElementById('<%= divBtnEnviarPagamento.ClientID %>').style.display = 'none';

            navigator.permissions.query({ name: 'camera' }).then(function (permissionStatus) {
                if (permissionStatus.state === 'granted') {
                    document.getElementById('<%= btnPermitirCamera.ClientID %>').style.display = 'none';
                    abrirCamera();
                } else {
                    solicitarPermissaoCamera();
                }
            });
        }

    </script>
    >

</asp:Content>
