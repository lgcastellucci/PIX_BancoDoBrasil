<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PIX_BancoDoBrasil._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery.mask/1.14.16/jquery.mask.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#<%= txtCPF.ClientID %>').mask('000.000.000-00');
            $('#<%= txtValor.ClientID %>').mask('000.000.000.000.000,00', { reverse: true });
        });
    </script>

    <main class="text-center">
        <div class="row justify-content-center mb-3">
            <section class="col-md-6">
                <h5>Digite seu CPF</h5>
                <asp:TextBox ID="txtCPF" runat="server" CssClass="form-control mx-auto" placeholder="CPF" Style="max-width: 300px;"></asp:TextBox>
            </section>
        </div>
        <div class="row justify-content-center mb-3">
            <section class="col-md-6">
                <h5>Digite o valor em R$</h5>
                <asp:TextBox ID="txtValor" runat="server" CssClass="form-control mx-auto" placeholder="Valor em R$" Style="max-width: 300px;"></asp:TextBox>
            </section>
        </div>
        <div class="row justify-content-center">
            <section class="col-md-6">
                <asp:Button ID="btnGerarPIX" runat="server" CssClass="btn btn-primary" Text="Gerar PIX" OnClick="btnGerarPIX_Click" />
            </section>
        </div>


        <div class="row justify-content-center">
            <section class="col-md-6">
                <asp:Image ID="imgQRCode" runat="server" Visible="false" />
            </section>
        </div>
    </main>

</asp:Content>
