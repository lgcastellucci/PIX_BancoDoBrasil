<%@ Page Title="PIX_BancoDoBrasil" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MeusPix.aspx.cs" Inherits="PIX_BancoDoBrasil.MeusPix" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="d-flex justify-content-center align-items-center" style="height: 100vh;">
        <div class="d-flex justify-content-center">
            <div id="divPixEmitidos" runat="server" class="rounded p-4" style="border: 1px solid #ccc; display: block;">
                <div class="row justify-content-center mb-3">
                    <section class="col-md-12">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h5 class="d-inline-block">PIX Emitidos</h5>
                            <asp:Button ID="btnAtualizar" runat="server" Text="Atualizar Pagina" OnClick="btnAtualizar_Click" CssClass="btn btn-primary d-inline-block" />
                        </div>
                        <asp:GridView ID="gvPixEmitidos" runat="server" CssClass="table table-striped mx-auto" AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundField DataField="MEU_ID" HeaderText="Meu ID" />
                                <asp:BoundField DataField="DATA" HeaderText="Data" />
                                <asp:BoundField DataField="DEVEDOR_CPF" HeaderText="Devedor CPF/CNPJ" />
                                <asp:BoundField DataField="DEVEDOR_NOME" HeaderText="Devedor Nome" />
                                <asp:BoundField DataField="VALOR" HeaderText="Valor" />
                                <asp:BoundField DataField="DATA_STATUS" HeaderText="Data do Status" />
                                <asp:BoundField DataField="STATUS" HeaderText="Status" />
                            </Columns>
                        </asp:GridView>
                        <div class="text-right mt-3">
                            <asp:Label ID="lblUltimaAtualizacao" runat="server" CssClass="text-muted"></asp:Label>
                        </div>
                    </section>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
