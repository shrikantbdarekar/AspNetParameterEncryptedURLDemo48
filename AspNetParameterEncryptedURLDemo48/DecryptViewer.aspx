<%@ Page Title="Decrypted Parameters" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DecryptViewer.aspx.cs" Inherits="AspNetParameterEncryptedURLDemo48.DecryptViewer" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <h2>Decrypted URL Parameters</h2>
        <p class="lead">The encrypted token was decrypted by <code>SecurePage</code> before this page loaded. The query-string values below were recovered from the token.</p>

        <asp:Panel ID="pnlNoParams" runat="server" Visible="false">
            <div class="alert alert-warning">No parameters were found in the request.</div>
        </asp:Panel>

        <asp:Literal ID="litTable" runat="server" />

        <br />
        <p><a href="Default.aspx" class="btn btn-default">&laquo; Back to Home</a></p>
    </main>
</asp:Content>
