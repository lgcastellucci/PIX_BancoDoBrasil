<%@ Page Title="Sobre" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="PIX_BancoDoBrasil.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h3>Geração de um PIX no Banco do Brasil.</h3>
        <p>
            <asp:Label ID="lblOSDescription" runat="server"></asp:Label> (<asp:Label ID="lblWindowsVersion" runat="server"></asp:Label>)
        </p>
    </main>
</asp:Content>
